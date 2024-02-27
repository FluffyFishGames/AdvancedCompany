using BepInEx.Bootstrap;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class Compability
    {
        public static AnimatorOverrideController OthersOverrideController;
        public static AnimatorOverrideController LocalOverrideController;

        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPostfix]
        public static void ApplyPatch()
        {
            if (Chainloader.PluginInfos.ContainsKey("MoreEmotes"))
            {
                var assembly = Chainloader.PluginInfos["MoreEmotes"].Instance.GetType().Assembly;
                if (assembly != null)
                {
                    Plugin.UseAnimationOverride = true;

                    Plugin.Log.LogMessage("Found more emotes. Trying to add AnimatorOverrideController...");
                    Type type = assembly.GetType("MoreEmotes.Patch.EmotePatch");
                    FieldInfo localField = type.GetField("local", BindingFlags.Static | BindingFlags.Public);
                    RuntimeAnimatorController localController = (RuntimeAnimatorController)localField.GetValue(null);
                    if (localController != null && !(localController is AnimatorOverrideController))
                    {
                        localController = new AnimatorOverrideController(localController);
                    }
                    localField.SetValue(null, localController);
                    LocalOverrideController = (AnimatorOverrideController)localController;

                    FieldInfo othersField = type.GetField("others", BindingFlags.Static | BindingFlags.Public);
                    RuntimeAnimatorController othersController = (RuntimeAnimatorController)othersField.GetValue(null);
                    if (othersController != null && !(othersController is AnimatorOverrideController))
                    {
                        othersController = new AnimatorOverrideController(othersController);
                    }
                    othersField.SetValue(null, othersController);
                    OthersOverrideController = (AnimatorOverrideController)othersController;
                }
            }/*
            if (Chainloader.PluginInfos.ContainsKey("BetterEmotes"))
            {
                var assembly = Chainloader.PluginInfos["BetterEmotes"].Instance.GetType().Assembly;
                if (assembly != null)
                {
                    Plugin.UseAnimationOverride = true;

                    Plugin.Log.LogMessage("Found better emotes. Trying to add AnimatorOverrideController...");
                    Type type = assembly.GetType("BetterEmotes.EmotePatch");
                    FieldInfo localField = type.GetField("local", BindingFlags.Static | BindingFlags.Public);
                    RuntimeAnimatorController localController = (RuntimeAnimatorController)localField.GetValue(null);
                    if (localController != null && !(localController is AnimatorOverrideController))
                    {
                        localController = new AnimatorOverrideController(localController);
                    }
                    localField.SetValue(null, localController);
                    LocalOverrideController = (AnimatorOverrideController)localController;

                    FieldInfo othersField = type.GetField("others", BindingFlags.Static | BindingFlags.Public);
                    RuntimeAnimatorController othersController = (RuntimeAnimatorController)othersField.GetValue(null);
                    if (othersController != null && !(othersController is AnimatorOverrideController))
                    {
                        othersController = new AnimatorOverrideController(othersController);
                    }
                    othersField.SetValue(null, othersController);
                    OthersOverrideController = (AnimatorOverrideController)othersController;
                }
            }*/
        }

    }
}
