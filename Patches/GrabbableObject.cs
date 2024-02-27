using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Linq;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class GrabbableObject
    {
        static IEnumerable<CodeInstruction> PatchUpdate(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching GrabbableObject->Update...");

            var property = typeof(Perks).GetProperty("ExtraBatteryMultiplier", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Single batteryUsage")
                {
                    IL.Patches.AddMultiplierInstruction("ExtraBattery", inst, i + 1);
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched GrabbableObject->Update...");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::GrabbableObject), "Update")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase method)
        {
            if (method.Name == "Update")
            {
                return PatchUpdate(instructions);
            }
            return instructions;
        }

    }
}
