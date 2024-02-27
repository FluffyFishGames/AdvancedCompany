using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Rendering;
using UnityEngine;
using HarmonyLib;

namespace AdvancedCompany.PostProcessing
{
    [HarmonyPatch]
    internal class Volumes
    {
        static Volume Volume;
        static internal void Initialize()
        {
            var go = new GameObject("RenderVolume");
            Volume = go.AddComponent<Volume>();
            Volume.isGlobal = true;
            Volume.priority = 1000f;
        }

        static internal void ChangeEnvironment(string moonID, bool onMoon, bool inFacility, bool underwater, bool alive)
        {
            var flags = Lib.HDRP.PostProcessingFlags.ORBIT;
            if (onMoon && inFacility)
                flags = Lib.HDRP.PostProcessingFlags.INSIDE;
            if (onMoon && !inFacility && !underwater)
                flags = Lib.HDRP.PostProcessingFlags.OUTSIDE;
            if (onMoon && underwater)
                flags = Lib.HDRP.PostProcessingFlags.UNDERWATER;
            Volume.sharedProfile = AdvancedCompany.Lib.HDRP.GetProfile(moonID, flags);
        }

        [HarmonyPatch(typeof(global::StartOfRound), "Awake")]
        [HarmonyPrefix]
        private static void Initialize(global::StartOfRound __instance)
        {
            PostProcessing.Volumes.Initialize();
            ChangeEnvironment(null, false, false, false, true);
        }

        [HarmonyPatch(typeof(global::EntranceTeleport), "TeleportPlayer")]
        [HarmonyPostfix]
        private static void EntranceTeleport(global::EntranceTeleport __instance)
        {
            ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, true, GameNetworkManager.Instance.localPlayerController.isInsideFactory, false, GameNetworkManager.Instance.localPlayerController.isPlayerDead);
        }

        [HarmonyPatch(typeof(global::StartOfRound), "ReviveDeadPlayers")]
        [HarmonyPostfix]
        private static void ReviveDeadPlayers(global::StartOfRound __instance)
        {
            ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, true, GameNetworkManager.Instance.localPlayerController.isInsideFactory, false, GameNetworkManager.Instance.localPlayerController.isPlayerDead);
        }

        [HarmonyPatch(typeof(global::ShipTeleporter), "TeleportPlayerOutWithInverseTeleporter")]
        [HarmonyPostfix]
        private static void TeleportPlayerOutWithInverseTeleporter(global::ShipTeleporter __instance, int playerObj)
        {
            ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, true, global::StartOfRound.Instance.allPlayerScripts[playerObj].isInsideFactory, false, global::StartOfRound.Instance.allPlayerScripts[playerObj].isPlayerDead);
        }

        [HarmonyPatch(typeof(global::ShipTeleporter), "beamUpPlayer")]
        [HarmonyPrefix]
        private static void beamUpPlayer(global::ShipTeleporter __instance)
        {
            if (StartOfRound.Instance.mapScreen.targetedPlayer != null && StartOfRound.Instance.mapScreen.targetedPlayer == global::StartOfRound.Instance.localPlayerController)
                ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, true, false, false, StartOfRound.Instance.mapScreen.targetedPlayer.isPlayerDead);
        }

        [HarmonyPatch(typeof(global::StartOfRound), "openingDoorsSequence")]
        [HarmonyPrefix]
        private static void openingDoorsSequence(global::StartOfRound __instance)
        {
            ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, true, false, false, false);
        }

        [HarmonyPatch(typeof(global::StartOfRound), "ShipLeave")]
        [HarmonyPostfix]
        private static void ShipLeave(global::StartOfRound __instance)
        {
            ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, false, false, false, GameNetworkManager.Instance.localPlayerController.isPlayerDead);
        }

        [HarmonyPatch(typeof(global::StartOfRound), "ShipLeaveAutomatically")]
        [HarmonyPostfix]
        private static void ShipLeaveAutomatically(global::StartOfRound __instance)
        {
            ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, false, false, false, GameNetworkManager.Instance.localPlayerController.isPlayerDead);
        }

        [HarmonyPatch(typeof(global::GameNetcodeStuff.PlayerControllerB), "SetFaceUnderwaterServerRpc")]
        [HarmonyPostfix]
        private static void SetFaceUnderwaterServerRpc(global::GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (__instance == global::StartOfRound.Instance.localPlayerController)
            {
                ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, true, GameNetworkManager.Instance.localPlayerController.isInsideFactory, true, GameNetworkManager.Instance.localPlayerController.isPlayerDead);
            }
        }

        [HarmonyPatch(typeof(global::GameNetcodeStuff.PlayerControllerB), "SetFaceOutOfWaterServerRpc")]
        [HarmonyPostfix]
        private static void SetFaceOutOfWaterServerRpc(global::GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (__instance == global::StartOfRound.Instance.localPlayerController)
            {
                ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, true, GameNetworkManager.Instance.localPlayerController.isInsideFactory, false, GameNetworkManager.Instance.localPlayerController.isPlayerDead);
            }
        }

    }
}
