using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using AdvancedCompany.Game;
using static UnityEngine.InputSystem.LowLevel.InputStateHistory;
using static UnityEngine.ParticleSystem;
using static AdvancedCompany.Patches.PlayerControllerB;
using UnityEngine;
using AdvancedCompany.Config;
using static UnityEngine.GraphicsBuffer;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class ItemDataHandling
    {
        public static Dictionary<int, SelectableLevel> Moons = new Dictionary<int, SelectableLevel>();
        private static Dictionary<GameNetcodeStuff.PlayerControllerB, AnimatorOverrideController> Animators = new();
        private static Dictionary<GameNetcodeStuff.PlayerControllerB, AnimationClipOverrides> Overrides = new();

        
        [HarmonyPatch(typeof(global::StartOfRound), "Awake")]
        [HarmonyPrefix]
        public static void AddItemsToLootTable(global::StartOfRound __instance)
        {
            foreach (var item in Game.Manager.ItemProperties)
            {
                if (!__instance.allItemsList.itemsList.Contains(item.Value))
                {
                    __instance.allItemsList.itemsList.Add(item.Value);
                    var walkie = item.Value.spawnPrefab.GetComponent<WalkieTalkie>();
                    if (walkie != null)
                    {
                        walkie.target = walkie.transform.Find("Target").GetComponent<AudioSource>();
                        walkie.thisAudio = walkie.GetComponent<AudioSource>();
                        foreach (var item2 in __instance.allItemsList.itemsList)
                        {
                            if (item2.itemName == "Walkie-talkie")
                            {
                                var original = item2.spawnPrefab.GetComponent<WalkieTalkie>();
                                walkie.talkingOnWalkieTalkieNotHeldSFX = original.talkingOnWalkieTalkieNotHeldSFX;
                                walkie.switchWalkieTalkiePowerOn = original.switchWalkieTalkiePowerOn;
                                walkie.switchWalkieTalkiePowerOff = original.switchWalkieTalkiePowerOff;
                                walkie.stopTransmissionSFX = original.stopTransmissionSFX;
                                walkie.startTransmissionSFX = original.startTransmissionSFX;
                                walkie.recordingRange = original.recordingRange;
                                walkie.playerDieOnWalkieTalkieSFX = original.playerDieOnWalkieTalkieSFX;
                                break;
                            }
                        }
                    }
                }
            }


/*            for (var i = 0; i < __instance.allItemsList.itemsList.Count; i++)
            {
                var item = __instance.allItemsList.itemsList[i];
                
            }
*/
            foreach (var l in __instance.levels)
            {
                if (l.name == "ExperimentationLevel")
                    Moons[Game.Moons.EXPERIMENTATION] = l;
                if (l.name == "AssuranceLevel")
                    Moons[Game.Moons.ASSURANCE] = l;
                if (l.name == "VowLevel")
                    Moons[Game.Moons.VOW] = l;
                if (l.name == "MarchLevel")
                    Moons[Game.Moons.MARCH] = l;
                if (l.name == "RendLevel")
                    Moons[Game.Moons.REND] = l;
                if (l.name == "DineLevel")
                    Moons[Game.Moons.DINE] = l;
                if (l.name == "OffenseLevel")
                    Moons[Game.Moons.OFFENSE] = l;
                if (l.name == "TitanLevel")
                    Moons[Game.Moons.TITAN] = l;
            }

            foreach (var item in Game.Manager.AddedItems)
            {
                if (item.Value.IsScrap)
                {
                    if (item.Value.LimitPlanets)
                    {
                        for (var i = 0; i < Game.Moons.Count; i++)
                        {
                            if (Moons.ContainsKey(i) && item.Value.PlanetRarities[i] > 0)
                            {
                                Moons[i].spawnableScrap.Add(new SpawnableItemWithRarity() { rarity = item.Value.PlanetRarities[i], spawnableItem = Game.Manager.ItemProperties[item.Value.ID] });
                            }
                        }
                    }
                    else
                    {
                        foreach (var m in Moons)
                            m.Value.spawnableScrap.Add(new SpawnableItemWithRarity() { rarity = item.Value.Rarity, spawnableItem = Game.Manager.ItemProperties[item.Value.ID] });
                    }
                }
            }
        }

        [HarmonyPatch(typeof(global::GrabbableObject), "LateUpdate")]
        [HarmonyPrefix]
        public static bool GrabbableObjectLateUpdate(global::GrabbableObject __instance)
        {
            var comp = __instance.GetComponent<ItemData>();
            if (comp != null && __instance.parentObject != null)
            {
                if (__instance.parentObject.name == "LocalItemHolder")
                {
                    __instance.transform.rotation = __instance.parentObject.rotation;
                    __instance.transform.Rotate(comp.EgoHeldRotation);
                    __instance.transform.position = __instance.parentObject.position;
                    __instance.transform.position += __instance.parentObject.rotation * comp.EgoHeldPosition;
                    return false;
                }
                else if (__instance.parentObject.name == "ServerItemHolder")
                {
                    __instance.transform.rotation = __instance.parentObject.rotation;
                    __instance.transform.Rotate(comp.HeldRotation);
                    __instance.transform.position = __instance.parentObject.position;
                    __instance.transform.position += __instance.parentObject.rotation * comp.HeldPosition;
                    return false;
                }
            }
            return true;
        }
        static IEnumerable<CodeInstruction> PatchUpdate(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->Update...");

            var inst = new List<CodeInstruction>(instructions);
            bool first = false;
            bool second = true;
            for (var i = 0; i < inst.Count; i++)
            {
                if (!first && inst[i].opcode == OpCodes.Ldc_R4 && (float)inst[i].operand == 2.25f)
                {
                    inst.RemoveAt(i + 2);
                    IL.Patches.AddMultiplierInstruction("SprintSpeed", inst, i + 2);

                    IL.Patches.AddMultiplierInstruction("SprintSpeed", inst, i + 1, false);
                    first = true;
                }
                if (!second && inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Single climbSpeed")
                {
                    IL.Patches.AddMultiplierInstruction("ClimbSpeed", inst, i + 1);
                    second = true;
                }
                if (first && second)
                    break;
            }
            /*var log = "";
            for (var i = 0; i < inst.Count; i++)
                log += inst[i].opcode + ": " + inst[i].operand + "\r\n";
            Plugin.Log.LogMessage(log);*/
            Plugin.Log.LogDebug("Patched PlayerControllerB->Update...");
            return inst.AsEnumerable();
        }
    }
}
;