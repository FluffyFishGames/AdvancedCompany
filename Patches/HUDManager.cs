using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UI;
using UnityEngine;
using GameNetcodeStuff;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class HUDManager
    {
        [HarmonyPatch(typeof(global::HUDManager), "MeetsScanNodeRequirements")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchMeetsScanNodeRequirements(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching HUDManager->MeetsScanNodeRequirements...");

            var property = typeof(Perks).GetProperty("ScanDistanceMultiplier", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Int32 maxRange")
                {
                    IL.Patches.AddMultiplierInstruction("ScanDistance", inst, i + 2);
                    break;
                } 
            }

            Plugin.Log.LogDebug("Patched HUDManager->MeetsScanNodeRequirements...");
            return inst.AsEnumerable();
        }
    }
}
