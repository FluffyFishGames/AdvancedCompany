using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Config;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class TimeOfDay
    {
        public static float GetMultiplier()
        {
            return 700f / Mathf.Max(1f, (float) ServerConfiguration.Instance.General.DayLength);
        }

        [HarmonyPatch(typeof(global::TimeOfDay), "MoveGlobalTime")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> TranspileStart(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching TimeOfDay->MoveGlobalTime...");

            var method = typeof(TimeOfDay).GetMethod("GetMultiplier", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString().Contains("globalTimeSpeedMultiplier"))
                {
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Mul));
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Call, method));
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched TimeOfDay->MoveGlobalTime...");
            return inst.AsEnumerable();
        }
    }
}
