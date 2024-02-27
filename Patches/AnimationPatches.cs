using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Patches
{
    [LoadAssets]
    [HarmonyPatch]
    internal class AnimationPatches
    {
        internal static RuntimeAnimatorController PlayerAnimator;
        internal static RuntimeAnimatorController OtherPlayerAnimator;

        public static void LoadAssets(AssetBundle assets)
        {
            PlayerAnimator = new AnimatorOverrideController(assets.LoadAsset<RuntimeAnimatorController>("Assets/Animators/metarig.controller"));
            OtherPlayerAnimator = new AnimatorOverrideController(assets.LoadAsset<RuntimeAnimatorController>("Assets/Animators/metarigOtherPlayers.controller"));
        }

        [HarmonyPatch(typeof(global::StartOfRound), "Awake")]
        [HarmonyPrefix]
        private static void Initialize(global::StartOfRound __instance)
        {
            __instance.otherClientsAnimatorController = new AnimatorOverrideController(OtherPlayerAnimator);
            __instance.localClientAnimatorController = new AnimatorOverrideController(PlayerAnimator);
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "Awake")]
        [HarmonyPostfix]
        public static void Awake(GameNetcodeStuff.PlayerControllerB __instance)
        {
            __instance.playerBodyAnimator.runtimeAnimatorController = OtherPlayerAnimator;
        }
    }
}
