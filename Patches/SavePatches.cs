using HarmonyLib;
using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace AdvancedCompany.Patches
{
    internal class SavePatches
    {

        [HarmonyPatch(typeof(global::GameNetworkManager), "SaveItemsInShip")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSaveItems(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching GameNetworkManager->SaveItemsInShip...");

            var inst = new List<CodeInstruction>(instructions);
            var saveItemsInShip = typeof(Game.Manager.Save).GetMethod("SaveItemsInShip", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);

            if (inst[3].opcode == OpCodes.Stloc_0 && inst[4].opcode == OpCodes.Ldloc_0)
            {
                inst.Insert(4, new CodeInstruction(OpCodes.Call, saveItemsInShip));
                inst.Insert(4, new CodeInstruction(OpCodes.Ldloc_0));
                Plugin.Log.LogDebug("Added custom save method.");
            }
            else
            {
                Plugin.Log.LogWarning("Couldn't find needed OpCodes for patching!");
            }
            Plugin.Log.LogDebug("Patched GameNetworkManager->SaveItemsInShip!");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(global::StartOfRound), "LoadShipGrabbableItems")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchLoadShipGrabbableItems(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching StartOfRound->LoadShipGrabbableItems...");

            var inst = new List<CodeInstruction>(instructions);
            var loadItemsInShip = typeof(Game.Manager.Save).GetMethod("LoadItemsInShip", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var first = true;
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldstr && inst[i].operand.ToString() == "shipGrabbableItemIDs")
                {
                    if (first)
                    {
                        first = false;
                        continue;
                    }
                    if (inst[i + 4].opcode == OpCodes.Stloc_1)
                    {
                        Plugin.Log.LogDebug("Found injection point for LoadItems");
                        inst.Insert(i + 5, new CodeInstruction(OpCodes.Stloc_1));
                        inst.Insert(i + 5, new CodeInstruction(OpCodes.Call, loadItemsInShip));
                        inst.Insert(i + 5, new CodeInstruction(OpCodes.Ldloc_1));
                    }
                    else
                    {
                        Plugin.Log.LogWarning("Method was changed. Game update?");
                    }
                    break;
                }
            }
            Plugin.Log.LogDebug("Patched StartOfRound->LoadShipGrabbableItems!");
            return inst.AsEnumerable();
        }
    }
}
