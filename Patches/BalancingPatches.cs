using AdvancedCompany.Config;
using BepInEx.Bootstrap;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class BalancingPatches
    {

        [HarmonyPatch(typeof(global::RoundManager), "SpawnScrapInLevel")]
        [HarmonyPrefix]
        [HarmonyPriority(int.MaxValue)]
        public static void ResetMultiplier(global::RoundManager __instance)
        {
            __instance.scrapValueMultiplier = 0.4f;
            __instance.scrapAmountMultiplier = 1f;
        }

        [HarmonyPatch(typeof(LungProp), "Start")]
        [HarmonyPostfix]
        [HarmonyAfter(new string[] { "LethalRadiation" })]
        public static void ChangeLungPrice(LungProp __instance)
        {
            __instance.scrapValue = (int)(__instance.scrapValue * Game.Manager.Moons.GetScrapValueModifierOnly(global::RoundManager.Instance));
        }

        [HarmonyPatch(typeof(LungProp), "EquipItem")]
        [HarmonyPrefix]
        [HarmonyAfter(new string[] { "me.loaforc.facilitymeltdown" })]
        public static void ChangeLungPrice2(LungProp __instance)
        {
            if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("me.loaforc.facilitymeltdown") && __instance.isLungDocked)
                __instance.scrapValue = (int)(__instance.scrapValue * Game.Manager.Moons.GetScrapValueModifierOnly(global::RoundManager.Instance));
        }
    }
}
