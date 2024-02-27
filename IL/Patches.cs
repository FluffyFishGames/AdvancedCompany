using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace AdvancedCompany.IL
{
    internal static class Patches
    {
        internal static MethodInfo GetMultiplierPlayer;
        internal static MethodInfo GetMultiplier;
        static Patches()
        {
            var methods = typeof(Perks).GetMethods(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy);
            foreach (var method in methods)
            {
                if (method.Name == "GetMultiplier")
                {
                    var parameters = method.GetParameters();
                    if (parameters.Length == 1)
                    {
                        GetMultiplier = method;
                    }
                    else if (parameters.Length == 2 && parameters[0].ParameterType.Name == "PlayerControllerB")
                    {
                        GetMultiplierPlayer = method;
                    }
                }
            }
        }

        public static void AddMultiplierInstruction(string perkName, List<CodeInstruction> instructions, int pos, bool addMul = true)
        {
            var inst = new List<CodeInstruction>();
            inst.Add(new CodeInstruction(OpCodes.Ldstr, perkName));
            inst.Add(new CodeInstruction(OpCodes.Call, GetMultiplier));
            if (addMul)
                inst.Add(new CodeInstruction(OpCodes.Mul));
            instructions.InsertRange(pos, inst);
        }
        public static void AddWeightMultiplierInstruction(string perkName, List<CodeInstruction> instructions, int pos)
        {
            var inst = new List<CodeInstruction>();
            inst.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));
            inst.Add(new CodeInstruction(OpCodes.Sub));
            inst.Add(new CodeInstruction(OpCodes.Ldstr, perkName));
            inst.Add(new CodeInstruction(OpCodes.Call, GetMultiplier));
            inst.Add(new CodeInstruction(OpCodes.Mul));
            inst.Add(new CodeInstruction(OpCodes.Ldc_R4, 1.0f));
            inst.Add(new CodeInstruction(OpCodes.Add));
            instructions.InsertRange(pos, inst);
        }

        public static void AddMultiplierPlayerInstruction(string perkName, List<CodeInstruction> instructions, int pos, bool addMul = true)
        {
            var inst = new List<CodeInstruction>();
            inst.Add(new CodeInstruction(OpCodes.Ldstr, perkName));
            inst.Add(new CodeInstruction(OpCodes.Call, GetMultiplierPlayer));
            if (addMul)
                inst.Add(new CodeInstruction(OpCodes.Mul));
            instructions.InsertRange(pos, inst);
        }
    }
}
