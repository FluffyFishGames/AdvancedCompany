using AdvancedCompany.Objects;
using AdvancedCompany.PostProcessing;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Hooks.PlayerControllerB
{
    internal class KillPlayer
    {
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "KillPlayer")]
        [HarmonyPrefix]
        public static bool PrePatch(global::GameNetcodeStuff.PlayerControllerB __instance, Vector3 bodyVelocity, bool spawnBody, CauseOfDeath causeOfDeath, int deathAnimation)
        {
            var isLocal = __instance == global::StartOfRound.Instance.localPlayerController;
            if (__instance.currentlyHeldObjectServer is PietSmietController controller && !controller.CurseLifted)
            {
                if (isLocal)
                {
                    if (causeOfDeath == CauseOfDeath.Suffocation || causeOfDeath == CauseOfDeath.Gravity)
                        return true;
                    else
                    {
                        controller.Damage(10);
                        if (controller.Health <= 0)
                        {
                            controller.StopDoom();
                            return true;
                        }
                        return false;
                    }
                }
            }

            if (__instance.IsOwner && !__instance.isPlayerDead && __instance.AllowPlayerDeath())
            {
                var gamePlayer = Game.Player.GetPlayer(__instance);
                gamePlayer.UnequipAll();
            }

            if (isLocal)
                Volumes.ChangeEnvironment(global::RoundManager.Instance.currentLevel.PlanetName, true, GameNetworkManager.Instance.localPlayerController.isInsideFactory, false, GameNetworkManager.Instance.localPlayerController.isPlayerDead);

            return true;
        }
    }
}
