using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class DebugPatches
    {/*
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "UpdatePlayerAnimationClientRpc")]
        [HarmonyPrefix]
        internal static void DebugA(GameNetcodeStuff.PlayerControllerB __instance, int animationState, float animationSpeed)
        {
            Plugin.Log.LogMessage("Received animation state: " + animationState);

            if (animationState == 0 || __instance.playerBodyAnimator.GetCurrentAnimatorStateInfo(0).fullPathHash == animationState)
            {
                Plugin.Log.LogMessage("Root found");
            }
            for (int i = 0; i < __instance.playerBodyAnimator.layerCount; i++)
            {
                if (__instance.playerBodyAnimator.HasState(i, animationState))
                {
                    Plugin.Log.LogMessage("Layer " + i + " has state!");
                }
            }
        }*/
    }
}
