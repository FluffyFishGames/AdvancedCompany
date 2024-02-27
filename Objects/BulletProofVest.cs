using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Objects.Interfaces;
using System.Reflection.Emit;
using AdvancedCompany.Config;
using AdvancedCompany.PostProcessing;

namespace AdvancedCompany.Objects
{
    [LoadAssets]
    [HarmonyPatch]
    internal class BulletProofVest : Body, IIconProvider
    {
        public int Damage;
        private static GameObject VestPrefab;
        private static Sprite Damage0Icon;
        private static Sprite Damage1Icon;
        private static Sprite Damage2Icon;
        private static Texture2D Damage0Normal;
        private static Texture2D Damage1Normal;
        private static Texture2D Damage2Normal;
        private static float ProtectionChance0 = 0.99f;
        private static float ProtectionChance1 = 0.95f;
        private static float ProtectionChance2 = 0.90f;

        private Material Material;

        [HarmonyPatch(typeof(Turret), "Update")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PatchTurretUpdate(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase methodBase)
        {
            Plugin.Log.LogDebug("Patching Turret->Update...");

            var method = typeof(BulletProofVest).GetMethod("DamageVest", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand.ToString() == "Void KillPlayer(UnityEngine.Vector3, Boolean, CauseOfDeath, Int32)")
                {
                    inst.RemoveAt(i - 15);
                    inst.RemoveAt(i - 15);
                    inst.RemoveAt(i - 15);
                    inst.Insert(i - 15, inst[i - 7]);
                    inst.Insert(i - 15, inst[i - 7]);
                    inst.Insert(i - 15, inst[i - 7]);
                    inst.Insert(i - 15, inst[i - 7]);
                    inst.Insert(i - 15, inst[i - 7]);

                    inst.RemoveAt(i - 6);
                    inst.RemoveAt(i - 6);
                    inst.RemoveAt(i - 6);
                    inst.RemoveAt(i - 6);
                    inst.RemoveAt(i - 6);
                    inst.RemoveAt(i - 6);
                    inst.RemoveAt(i - 6);
                    inst.RemoveAt(i - 6);
                    inst.RemoveAt(i - 6);

                    inst.Insert(i - 6, inst[i - 10]);
                    inst.Insert(i - 6, inst[i - 11]);
                    inst.Insert(i - 6, inst[i - 12]);
                    inst.Insert(i - 6, inst[i - 13]);
                    inst.Insert(i - 6, inst[i - 14]);
                    inst.Insert(i - 6, inst[i - 15]);
                    inst.Insert(i - 6, inst[i - 16]);
                    inst.Insert(i - 6, inst[i - 17]);
                    inst.Insert(i - 6, inst[i - 18]);
                    inst.Insert(i - 6, inst[i - 19]);
                    inst.Insert(i - 6, inst[i - 20]);
                    inst.Insert(i - 6, inst[i - 21]);
                }
            }
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand.ToString() == "Void DamagePlayer(Int32, Boolean, Boolean, CauseOfDeath, Int32, Boolean, UnityEngine.Vector3)")
                {
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Call, method));
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_I4_0));
                    inst.Insert(i + 1, inst[i - 12]);
                    inst.Insert(i + 1, inst[i - 13]);
                }
            }
            /*var log = "";
            for (var i = 0; i < inst.Count; i++)
                log += inst[i].opcode + ": " + inst[i].operand + "\r\n";
            Plugin.Log.LogMessage(log);*/
            Plugin.Log.LogDebug("Patched Turret->Update...");
            return inst.AsEnumerable();
        }


        [HarmonyPatch(typeof(ShotgunItem), "ShootGun")]
        [HarmonyTranspiler]
        public static IEnumerable<CodeInstruction> PatchShootGun(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase methodBase)
        {
            Plugin.Log.LogDebug("Patching ShotgunItem->ShootGun...");

            var method = typeof(BulletProofVest).GetMethod("DamageVest", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Callvirt && inst[i].operand.ToString() == "Void DamagePlayer(Int32, Boolean, Boolean, CauseOfDeath, Int32, Boolean, UnityEngine.Vector3)")
                {
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Call, method));
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Ldc_I4_1));
                    inst.Insert(i + 1, inst[i - 12]);
                }
            }
            /*var log = "";
            for (var i = 0; i < inst.Count; i++)
                log += inst[i].opcode + ": " + inst[i].operand + "\r\n";
            Plugin.Log.LogMessage(log);*/
            Plugin.Log.LogDebug("Patched ShotgunItem->ShootGun...");
            return inst.AsEnumerable();
        }

        public enum DamageOrigin : int
        {
            TURRET = 0,
            SHOTGUN = 1
        };
        public static void DamageVest(GameNetcodeStuff.PlayerControllerB instance, DamageOrigin origin)
        {
            var player = Game.Player.GetPlayer(instance);
            if (player.Body != null && player.Body is BulletProofVest vest)
            {
                if (origin == DamageOrigin.TURRET)
                {
                    Plugin.Log.LogMessage("Turret is damaging vest.");
                    vest.TakeDamage(ServerConfiguration.Instance.Items.BulletProofVest.TurretDamage);
                }
                else if (origin == DamageOrigin.SHOTGUN)
                {
                    Plugin.Log.LogMessage("Shotgun is damaging vest.");
                    vest.TakeDamage(ServerConfiguration.Instance.Items.BulletProofVest.ShotgunDamage);
                }
            }
        }

        
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "DamagePlayer")]
        [HarmonyPrefix]
        public static void DamagePlayer(GameNetcodeStuff.PlayerControllerB __instance, ref int damageNumber, bool hasDamageSFX, bool callRPC, CauseOfDeath causeOfDeath, int deathAnimation, bool fallDamage, Vector3 force)
        {
            if (causeOfDeath == CauseOfDeath.Gunshots)
            {
                Game.Player player = Game.Player.GetPlayer(__instance);

                if (player.Body is BulletProofVest vest)
                {
                    var health = ((float)ServerConfiguration.Instance.Items.BulletProofVest.MaxDamage - vest.Damage) / ((float)ServerConfiguration.Instance.Items.BulletProofVest.MaxDamage);
                    var damageReduction = 
                        ServerConfiguration.Instance.Items.BulletProofVest.DamageReductionAtNoHealth +
                        health * (ServerConfiguration.Instance.Items.BulletProofVest.DamageReductionAtFullHealth - ServerConfiguration.Instance.Items.BulletProofVest.DamageReductionAtNoHealth);
                    if (damageReduction < 0f) damageReduction = 0f;
                    if (damageReduction > 1f) damageReduction = 1f;
                    damageNumber = (int)(((float)damageNumber) * (1f - damageReduction));
                }
            }
        }

        public void Awake()
        {
            var renderer = GetComponent<MeshRenderer>();
            Material = new Material(renderer.sharedMaterial);
            renderer.sharedMaterial = Material;
            SetNormal();
        }

        public override GameObject[] CreateWearable(Player player)
        {
            var vestBones = new Transform[] {
                player.GetBone(Player.Bone.ROOT),
                player.GetBone(Player.Bone.SPINE_0),
                player.GetBone(Player.Bone.SPINE_1),
                player.GetBone(Player.Bone.SPINE_2),
                player.GetBone(Player.Bone.L_SHOULDER),
                player.GetBone(Player.Bone.L_LOWER_ARM),
                player.GetBone(Player.Bone.R_SHOULDER),
                player.GetBone(Player.Bone.R_LOWER_ARM),
                player.GetBone(Player.Bone.SPINE_3),
                player.GetBone(Player.Bone.L_THIGH),
                player.GetBone(Player.Bone.R_THIGH)
            };

            var metarig = player.GetBone(Player.Bone.METARIG);
            var vest = GameObject.Instantiate(VestPrefab, metarig);
            vest.GetComponent<SkinnedMeshRenderer>().rootBone = player.GetBone(Player.Bone.ROOT);
            vest.GetComponent<SkinnedMeshRenderer>().bones = vestBones;
            vest.GetComponent<SkinnedMeshRenderer>().ResetBounds();
            vest.GetComponent<SkinnedMeshRenderer>().sharedMaterial = Material;

            if (player.IsLocal)
            {
                vest.layer = 23;
                var t = vest.GetComponentsInChildren<Transform>();
                foreach (var tt in t)
                    tt.gameObject.layer = 23;
            }
            return new GameObject[] { vest };
        }

        public override int GetItemDataToSave()
        {
            return Damage;
        }

        public override void LoadItemSaveData(int saveData)
        {
            Damage = saveData;
            SetNormal();
        }

        public void SetNormal()
        {
            if (Damage < ServerConfiguration.Instance.Items.BulletProofVest.MaxDamage / 3f)
                Material.SetTexture("_NormalMap", Damage0Normal);
            else if (Damage < ServerConfiguration.Instance.Items.BulletProofVest.MaxDamage * 2f / 3f)
                Material.SetTexture("_NormalMap", Damage1Normal);
            else
                Material.SetTexture("_NormalMap", Damage2Normal);
        }

        public static void LoadAssets(AssetBundle assets)
        {
            VestPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/BulletProofVest.prefab");
            Damage0Icon = assets.LoadAsset<Sprite>("Assets/Icons/Items/BulletProofVest.png");
            Damage1Icon = assets.LoadAsset<Sprite>("Assets/Icons/Items/BulletProofVest1.png");
            Damage2Icon = assets.LoadAsset<Sprite>("Assets/Icons/Items/BulletProofVest2.png");
            Damage0Normal = assets.LoadAsset<Texture2D>("Assets/Models/BulletProofVest/BulletProofVestNormal.png");
            Damage1Normal = assets.LoadAsset<Texture2D>("Assets/Models/BulletProofVest/BulletProofVestDamage1Normal.png");
            Damage2Normal = assets.LoadAsset<Texture2D>("Assets/Models/BulletProofVest/BulletProofVestDamage2Normal.png");
        }

        public Sprite GetIcon()
        {
            if (Damage < ServerConfiguration.Instance.Items.BulletProofVest.MaxDamage / 3f)
                return Damage0Icon;
            else if (Damage < ServerConfiguration.Instance.Items.BulletProofVest.MaxDamage * 2f / 3f)
                return Damage1Icon;
            else
                return Damage2Icon;
        }

        public void TakeDamage(int amount)
        {
            Damage += amount;
            SetNormal();
            if (Damage >= ServerConfiguration.Instance.Items.BulletProofVest.MaxDamage)
            {
                Damage = ServerConfiguration.Instance.Items.BulletProofVest.MaxDamage;
                if (ServerConfiguration.Instance.Items.BulletProofVest.DestroyAtNoHealth)
                    Destroy();
            }
            GrabbableObjectAdditions.ChangeIcon(this, GetIcon());
        }

        public void Destroy()
        {
            GetComponent<MeshRenderer>().enabled = false;
            GetComponent<BoxCollider>().enabled = false;
            grabbable = false;
            grabbableToEnemies = false;
            deactivated = true;

            this.Unequip();

            var inventoryPos = GrabbableObjectAdditions.GetInventoryPosition(this);
            if (inventoryPos > -1)
            {
                this.playerHeldBy.carryWeight -= Mathf.Clamp(itemProperties.weight - 1f, 0f, 10f);
                this.playerHeldBy.ItemSlots[inventoryPos] = null;
                if (this.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
                    HUDManager.Instance.itemSlotIcons[inventoryPos].enabled = false;
            }

            this.DiscardItemOnClient();
        }
    }
}
