using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine.Animations.Rigging;
using UnityEngine;
using System.Linq;
using System.Reflection;
using UnityEngine.UIElements;
using System.Reflection.Emit;
using Unity.Mathematics;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class RoundManager
    {
        [HarmonyPatch(typeof(global::RoundManager), "Update")]
        [HarmonyPostfix]
        public static void Update(global::RoundManager __instance)
        {
            Game.Player.Update();
        }


        private static System.Random Random;
        public static void SetRandom()
        {
            Random = new System.Random(global::StartOfRound.Instance.randomMapSeed);
        }
        public static bool ShouldSaveObject()
        {
            return Random.NextDouble() < Perks.GetMultiplier("SaveLoot");
        }

        [HarmonyPatch(typeof(global::RoundManager), "DespawnPropsAtEndOfRound")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchResetShip(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching RoundManager->DespawnPropsAtEndOfRound...");

            var method1 = typeof(RoundManager).GetMethod("SetRandom", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            var method2 = typeof(RoundManager).GetMethod("ShouldSaveObject", BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count - 1; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString().Contains("isScrap"))
                {
                    var brTarget = inst[i + 1].operand;
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Brtrue, brTarget));
                    inst.Insert(i + 2, new CodeInstruction(OpCodes.Call, method2));
                    break;
                }
            }
            inst.Insert(0, new CodeInstruction(OpCodes.Call, method1));
            Plugin.Log.LogDebug("Patched RoundManager->DespawnPropsAtEndOfRound...");
            return inst.AsEnumerable();
        }
        /*
        [HarmonyPrefix]
        public static bool ProtectLoot(global::RoundManager __instance, bool despawnAllItems)
        {
            if (!despawnAllItems && __instance.IsServer)
            {
                if (global::StartOfRound.Instance.allPlayersDead)
                {
                    global::GrabbableObject[] objects = GameObject.FindObjectsOfType<global::GrabbableObject>();
                    var random = new System.Random(global::StartOfRound.Instance.randomMapSeed);
                    for (var i = 0; i < objects.Length; i++)
                    {
                        global::GrabbableObject item = objects[i];
                        bool save = false;
                        Plugin.Log.LogMessage("SaveLoot: " + Perks.GetMultiplier("SaveLoot"));
                        if (item.isInShipRoom && (!item.itemProperties.isScrap || (!item.deactivated && !item.isHeld && random.NextDouble() < Perks.GetMultiplier("SaveLoot"))))
                        {
                            save = true;
                        }
                        if (!save)
                        {
                            item.gameObject.GetComponent<NetworkObject>().Despawn(true);
                            if (__instance.spawnedSyncedObjects.Contains(item.gameObject))
                                __instance.spawnedSyncedObjects.Remove(item.gameObject);
                        }
                        else
                        {
                            item.scrapPersistedThroughRounds = true;
                        }
                    }
                    GameObject[] temporary = GameObject.FindGameObjectsWithTag("TemporaryEffect");
                    for (int i = 0; i < temporary.Length; i++)
                        GameObject.Destroy(temporary[i]);
                    return false;
                }
            }
            return true;
        }*/
    }
}
