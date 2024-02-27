using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace AdvancedCompany
{
    public static class CodeInstructionExtensions
    {
        public static bool IsLdfld(this CodeInstruction instruction, string fieldName = null)
        {
            if (instruction.opcode == OpCodes.Ldfld)
            {
                if (fieldName != null && instruction.operand is FieldInfo f)
                {
                    var compare = f.DeclaringType.FullName + "." + f.Name;
                    return compare == fieldName;
                }
                return true;
            }
            return false;
        }
    }
}
