using AdvancedCompany.Config;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;

namespace AdvancedCompany.Objects
{
    [HarmonyPatch]
    internal class LightningRod : GrabbableObject
    {
        private static List<LightningRod> AllLightningRods = new List<LightningRod>();
        private Animator Animator;
        public bool Extend = false;
        private bool _Extended = false;
        public AudioSource Rod0Sound;
        public AudioSource Rod1Sound;
        public AudioSource Rod2Sound;
        public AudioSource Rod3Sound;
        public AudioSource Rod4Sound;
        public AudioSource RodHeadSound;
        public Transform RodPoint;

        public static Vector3 CatchLightning(Vector3 position)
        {
            for (var i = 0; i < AllLightningRods.Count; i++)
            {
                if (AllLightningRods[i] == null)
                {
                    AllLightningRods.RemoveAt(i);
                    i--;
                    continue;
                }
                if (AllLightningRods[i].RodPoint.transform.position.y > position.y)
                    return AllLightningRods[i].RodPoint.transform.position;
            }
            return position;
        }

        public static bool IsTargetable(GrabbableObject thing)
        {
            for (var i = 0; i < AllLightningRods.Count; i++)
            {
                if (AllLightningRods[i] == null)
                {
                    AllLightningRods.RemoveAt(i);
                    i--;
                    continue;
                }
                if (AllLightningRods[i].RodPoint.transform.position.y > thing.transform.position.y)
                    return false;
            }
            return true;
        }

        public void Awake()
        {
            Animator = GetComponent<Animator>();
            var rod0 = transform.Find("Rod0");
            Rod0Sound = rod0.Find("Sound").GetComponent<AudioSource>();
            var rod1 = rod0.Find("Rod1");
            Rod1Sound = rod1.Find("Sound").GetComponent<AudioSource>();
            var rod2 = rod1.Find("Rod2");
            Rod2Sound = rod2.Find("Sound").GetComponent<AudioSource>();
            var rod3 = rod2.Find("Rod3");
            Rod3Sound = rod3.Find("Sound").GetComponent<AudioSource>();
            var rod4 = rod3.Find("Rod4");
            Rod4Sound = rod4.Find("Sound").GetComponent<AudioSource>();
            var rodHead = rod4.Find("RodHead");
            RodHeadSound = rodHead.Find("Sound").GetComponent<AudioSource>();
            RodPoint = rodHead.Find("Point");
        }

        public override void Update()
        {
            base.Update();
            if (Extend && !_Extended)
            {
                _Extended = true;
                Animator.SetBool("Expand", true);
            }
            if (!Extend && _Extended)
            {
                _Extended = false;
                Animator.SetBool("Expand", false);
            }
        }

        public override void OnDestroy()
        {
            base.OnDestroy();
            AllLightningRods.Remove(this);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            Plugin.Log.LogMessage("Lightning Rod used");
            base.ItemActivate(used, buttonDown);

            if (playerHeldBy != null && !playerHeldBy.isInHangarShipRoom && !playerHeldBy.isInsideFactory)
            {
                //playerHeldBy.activatingItem = true;
                if (IsOwner)
                {

                    //if (playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                    playerHeldBy.DiscardHeldObject();
                }

                StartCoroutine(PlayExtend());
            }
        }

        public override void GrabItem()
        {
            base.GrabItem();
            Extend = false;
        }

        IEnumerator PlayExtend()
        {
            yield return new WaitForSeconds(2f);

            if (playerHeldBy == null)
            {
                AllLightningRods.Add(this);
                grabbable = false;
                gameObject.layer = 0;
                Extend = true;
            }
        }

        public void PlaySoundRod0()
        {
            Rod0Sound.Play();
        }
        public void PlaySoundRod1()
        {
            Rod1Sound.Play();
        }
        public void PlaySoundRod2()
        {
            Rod2Sound.Play();
        }
        public void PlaySoundRod3()
        {
            Rod3Sound.Play();
        }
        public void PlaySoundRod4()
        {
            Rod4Sound.Play();
        }
        public void PlaySoundRodHead()
        {
            RodHeadSound.Play();
        }


        [HarmonyPatch(typeof(global::StormyWeather), "LightningStrikeRandom")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PatchLightningStrikeRandom(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StormyWeather->LightningStrikeRandom...");

            var method = typeof(LightningRod).GetMethod("CatchLightning", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = inst.Count - 1; i >= 0; i--)
            {
                if (inst[i].opcode == OpCodes.Ldloc_0)
                {
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Call, method));
                    break;
                }
            }

            Plugin.Log.LogDebug("Patched StormyWeather->LightningStrikeRandom...");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::StormyWeather), "Update")]
        [HarmonyTranspiler]
        private static IEnumerable<CodeInstruction> PatchUpdate(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StormyWeather->Update...");

            var method = typeof(LightningRod).GetMethod("IsTargetable", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            int count = 0;
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Boolean isInFactory")
                {
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Brfalse, inst[i + 1].operand));
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Call, method));
                    inst.Insert(i + 2, new CodeInstruction(inst[i - 1].opcode, inst[i - 1].operand));
                    inst.Insert(i + 2, new CodeInstruction(inst[i - 2].opcode, inst[i - 2].operand));
                    if (count == 1)
                    {
                        inst.Insert(i + 2, new CodeInstruction(inst[i - 3].opcode, inst[i - 3].operand));
                        inst.Insert(i + 2, new CodeInstruction(inst[i - 4].opcode, inst[i - 4].operand));
                    }
                    count++;
                }
            }
            Plugin.Log.LogDebug("Patched StormyWeather->Update...");
            return inst.AsEnumerable();
        }

        public override void DiscardItem()
        {
            if (!this.isPocketed && (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability))
            {
                var player = Game.Player.GetPlayer(playerHeldBy);
                if (player != null)
                    player.RemoveOverride("HoldOneHandedItem", true);
            }
            base.DiscardItem();
        }
        public override void EquipItem()
        {
            if (Plugin.UseAnimationOverride || ClientConfiguration.Instance.Compability.AnimationsCompability)
            {
                var player = Game.Player.GetPlayer(playerHeldBy);
                if (player != null)
                    player.AddOverride("HoldOneHandedItem", "HoldLightningRod", true);
            }
            base.EquipItem();
        }
    }
}
