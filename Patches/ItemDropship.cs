using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class ItemDropship
    {
        [HarmonyPatch(typeof(global::ItemDropship), "Update")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase method)
        {
            Plugin.Log.LogDebug("Patching ItemDropship->Update...");

            var property = typeof(Perks).GetProperty("DropshipTime", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].operand is float f && f == 40f)
                {
                    IL.Patches.AddMultiplierInstruction("DeliverySpeed", inst, i + 1);
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched ItemDropship->Update...");
            return inst.AsEnumerable();
        }

    }
}
