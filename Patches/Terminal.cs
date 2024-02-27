using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using AdvancedCompany.Network;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Terminal;
using AdvancedCompany.Config;
using UnityEngine.Assertions.Must;
using UnityEngine.UI;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class Terminal
    {

        [HarmonyPatch(typeof(global::Terminal), "TextPostProcess")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchTextPostProcess(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching Terminal->TextPostProcess...");
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if ((inst[i].opcode == OpCodes.Ldstr && inst[i].operand.ToString() == "[No items in stock!]"))
                {
                    object branchTarget = null;
                    // find branch
                    for (var j = i + 1; j < inst.Count; j++)
                    {
                        if (inst[j].opcode == OpCodes.Beq_S || inst[j].opcode == OpCodes.Beq)
                        {
                            branchTarget = inst[j].operand;
                            break;
                        }
                    }
                    if (branchTarget != null)
                    {
                        var inserts = new List<CodeInstruction>() {
                            inst[i + 12],
                            inst[i + 13],
                            inst[i + 14],
                            inst[i + 15],
                            new CodeInstruction(OpCodes.Call, typeof(Game.Manager.Items).GetMethod("IsBuyable", BindingFlags.Static | BindingFlags.Public | BindingFlags.FlattenHierarchy)),
                            new CodeInstruction(OpCodes.Brfalse, branchTarget),
                            new CodeInstruction(inst[i + 9].opcode, inst[i + 9].operand)
                        };

                        inst[i + 9].opcode = inserts[0].opcode;
                        inst[i + 9].operand = inserts[0].operand;

                        for (var k = inserts.Count - 1; k >= 1; k--)
                            inst.Insert(i + 10, inserts[k]);
                        Plugin.Log.LogDebug("Added activation check");
                    }
                    else
                    {
                        Plugin.Log.LogWarning("Branch not found. Cant apply! Was the game updated?");
                    }
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched Terminal->TextPostProcess!");
            return inst.AsEnumerable();
        }


        [HarmonyPatch(typeof(global::Terminal), "LoadNewNodeIfAffordable")]
        [HarmonyPrefix]
        private static bool LoadNewNodeIfAffordable(global::Terminal __instance, TerminalNode node)
        {
            if (node.buyItemIndex >= 0)
            {
                if (__instance.buyableItemsList.Length > node.buyItemIndex)
                {
                    var item = __instance.buyableItemsList[node.buyItemIndex];
                    var config = ServerConfiguration.Instance.Items.GetByItemName(item.itemName);
                    if (config != null && !config.Active)
                    {
                        __instance.LoadNewNode(__instance.terminalNodes.specialNodes[16]);
                        return false;
                    }
                }
            }
            if (node.shipUnlockableID >= 0)
            {
                if (global::StartOfRound.Instance.unlockablesList.unlockables.Count > node.shipUnlockableID)
                {
                    var unlockable = global::StartOfRound.Instance.unlockablesList.unlockables[node.shipUnlockableID];
                    var config = ServerConfiguration.Instance.Items.GetByUnlockableName(unlockable.unlockableName);
                    if (config != null && !config.Active)
                    {
                        __instance.LoadNewNode(__instance.terminalNodes.specialNodes[16]);
                        return false;
                    }
                }
            }
            return true;
        }
    }
}
