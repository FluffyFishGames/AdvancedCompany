using AdvancedCompany.Unity;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class Flavour
    {
        [HarmonyPatch(typeof(MenuManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake(MenuManager __instance)
        {
            __instance.gameObject.AddComponent<Unity.Flavour>();
        }
    }
}
