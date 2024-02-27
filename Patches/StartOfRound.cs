using Dissonance;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Network.Messages;
using UnityEngine.Rendering;
using AdvancedCompany.PostProcessing;
using UnityEngine.Rendering.HighDefinition;
using AdvancedCompany.Objects;
using AdvancedCompany.Config;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class StartOfRound
    {
        private static float InitialShipAnimationSpeed = 1f;
        private static bool Initialized = false;
        
        [HarmonyPatch(typeof(global::StartOfRound), "Awake")]
        [HarmonyPrefix]
        private static void Initialize(global::StartOfRound __instance)
        {
            PietSmietController.DoomRunning = false;
            __instance.spectateCamera.cullingMask |= (1 << 23);

            Objects.VisionEnhancer.Initialize();
            __instance.voiceChatModule.OnPlayerStartedSpeaking += VoiceChatModule_OnPlayerStartedSpeaking;
            __instance.maxShipItemCapacity = 999;
            if (!Initialized)
            {
                InitialShipAnimationSpeed = __instance.shipAnimator.speed;
                Initialized = true;
            
                var levels = (SelectableLevel[])AccessTools.Field(typeof(global::StartOfRound), "levels").GetValue(__instance);
                for (var i = 0; i < levels.Length; i++)
                {
                    for (var j = 0; j < levels[i].spawnableScrap.Count; j++)
                    {
                        var scrap = levels[i].spawnableScrap[j];
                        ExtractScrap(scrap.spawnableItem);
                    }
                }

            }
        }

        private static void VoiceChatModule_OnPlayerStartedSpeaking(VoicePlayerState obj)
        {
            //Plugin.Log.LogMessage("Player speaking: " + obj. + " " + obj.Volume + " " + obj.Amplitude);
        }

        private static void ExtractAudio(AudioClip clip)
        {
            if (clip == null) return;
            var name = clip.name;
            if (!GameAssets.Audio.ContainsKey(name))
            {
                GameAssets.Audio.Add(name, clip);
            }
        }

        private static void ExtractScrap(Item scrap)
        {
            if (scrap == null) return;
            var name = scrap.itemName;
            if (!GameAssets.Scrap.ContainsKey(name))
            {
                GameAssets.Scrap.Add(name, scrap);
                ExtractAudio(scrap.dropSFX);
                ExtractAudio(scrap.pocketSFX);
                ExtractAudio(scrap.grabSFX);
            }
        }


        [HarmonyPatch(typeof(global::StartOfRound), "ShipLeave")]
        [HarmonyPrefix]
        private static void Leaving(global::StartOfRound __instance)
        {
            if (Initialized)
            {
                __instance.shipAnimator.speed = InitialShipAnimationSpeed / Mathf.Max(0.15f, Perks.GetMultiplier("LandingSpeed"));
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "openingDoorsSequence")]
        [HarmonyPrefix]
        private static void Entering(global::StartOfRound __instance)
        {
            if (Initialized)
            {
                __instance.shipAnimator.speed = InitialShipAnimationSpeed / Mathf.Max(0.05f, Perks.GetMultiplier("LandingSpeed"));
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "EndOfGame")]
        [HarmonyPrefix]
        private static void ResetAnimatorSpeed(global::StartOfRound __instance)
        {
            if (Initialized)
            {
                __instance.shipAnimator.speed = InitialShipAnimationSpeed;
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "ResetShip")]
        [HarmonyPrefix]
        private static void ResetShip(global::StartOfRound __instance)
        {
            Network.Manager.Lobby.CurrentShip.TotalQuota = 0;
            Network.Manager.Lobby.CurrentShip.ExtendedDeadline = false;
            if (!ServerConfiguration.Instance.General.SaveProgress && ServerConfiguration.Instance.General.ResetXP)
            {
                Network.Manager.Lobby.Player().XP = ServerConfiguration.Instance.General.StartingXP;
                Network.Manager.Lobby.Player().Levels = new Dictionary<string, int>();
                Network.Manager.Lobby.CurrentShip.XP = ServerConfiguration.Instance.General.StartingShipXP;
                Network.Manager.Lobby.CurrentShip.Levels = new Dictionary<string, int>();

                if (NetworkManager.Singleton.IsServer)
                {
                    Network.Manager.Lobby.CurrentShip.ResetSave(global::GameNetworkManager.Instance.currentSaveFileName);
                }
            }

            if (NetworkManager.Singleton.IsServer)
            {
                Network.Manager.Send(new ChangeShip() { TotalQuota = 0, ExtendedDeadline = false });
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "FirePlayersAfterDeadlineClientRpc")]
        [HarmonyPostfix]
        public static void FirePlayersAfterDeadlineClientRpc(global::StartOfRound __instance)
        {
            global::HUDManager.Instance.EndOfRunStatsText.text += "\nXP: " + Network.Manager.Lobby.CurrentShip.TotalQuota;
        }

        /*
        [HarmonyPatch(typeof(global::StartOfRound), "ResetShip")]
        [HarmonyPrefix]
        static void ResetShipPre()
        {
            global::TimeOfDay.Instance.quotaVariables.deadlineDaysAmount = ServerConfiguration.Instance.General.DeadlineLength;
        }

        [HarmonyPatch(typeof(global::StartOfRound), "SetNewProfitQuota")]
        [HarmonyPrefix]
        static void SetNewProfitQuotaPre()
        {
            global::TimeOfDay.Instance.quotaVariables.deadlineDaysAmount = ServerConfiguration.Instance.General.DeadlineLength;
        }*/

        public static bool ShouldSaveObject(global::UnlockableItem item)
        {
            return item.unlockableType == 0 && ServerConfiguration.Instance.General.SaveSuitsAfterDeath;
        }

        [HarmonyPatch(typeof(global::StartOfRound), "ResetShip")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchResetShip(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->ResetShip...");

            var method = typeof(StartOfRound).GetMethod("ShouldSaveObject", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count - 1; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Boolean spawnPrefab" && inst[i + 1].opcode == OpCodes.Brfalse)
                {
                    var brTarget = inst[i + 1].operand;
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Brtrue, brTarget));
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Call, method));
                    inst.Insert(i + 2, new CodeInstruction(inst[i - 1].opcode, inst[i - 1].operand));
                    inst.Insert(i + 2, new CodeInstruction(inst[i - 2].opcode, inst[i - 2].operand));
                    inst.Insert(i + 2, new CodeInstruction(inst[i - 3].opcode, inst[i - 3].operand));
                    inst.Insert(i + 2, new CodeInstruction(inst[i - 4].opcode, inst[i - 4].operand));
                    inst.Insert(i + 2, new CodeInstruction(inst[i - 5].opcode, inst[i - 5].operand));
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched StartOfRound->ResetShip...");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::StartOfRound), "openingDoorsSequence", MethodType.Enumerator)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchOpeningDoorsSequence(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->openingDoorsSequence...");

            var property = typeof(Perks).GetProperty("LandingSpeedMultiplier", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4 && ((float)inst[i].operand == 5f || (float)inst[i].operand == 10f)) // (float)inst[i].operand == 4f ||
                {
                    inst.Insert(i + 1, new CodeInstruction(OpCodes.Mul));
                    IL.Patches.AddMultiplierInstruction("LandingSpeed", inst, i + 1, false);
                    
                }
            }
            Plugin.Log.LogDebug("Patched StartOfRound->openingDoorsSequence...");
            return inst.AsEnumerable();
        }
    }
}
