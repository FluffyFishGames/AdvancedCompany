using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class FieldPatches
    {
        [HarmonyPatch(typeof(PreInitSceneScript), "Awake")]
        [HarmonyPrefix]
        public static void Awake()
        {
            AdvancedCompany.Lib.Cosmetics.LoadNew();
            if (Plugin.AssembliesToScan.Count > 0)
            {
                for (var i = 0; i < Plugin.AssembliesToScan.Count; i++)
                {
                    Plugin.Log.LogDebug("Patching " + Plugin.AssembliesToScan[i].FullName);
                    ScanAssemblyForFields(Plugin.AssembliesToScan[i]);
                    Plugin.Log.LogDebug("Patched " + Plugin.AssembliesToScan[i].FullName);
                }
                Plugin.AssembliesToScan.Clear();
            }
        }

        public static IEnumerable<Type> GetLoadableTypes(Assembly assembly)
        {
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                Plugin.Log.LogWarning("Mod " + assembly.FullName + " tried to reference a missing assembly " + e.Message + ". Trying to parse other types...");
                var ret = new List<Type>();
                foreach (var t in e.Types)
                {
                    try
                    {
                        if (t != null && t.Assembly == assembly)
                            ret.Add(t);
                    }
                    catch (BadImageFormatException)
                    {
                    }
                }
                return ret;
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Failed to parse mod " + assembly.FullName + ":");
                Plugin.Log.LogWarning(e);
                return new List<Type>();
            }
        }

        internal static void ScanAssemblyForFields(Assembly assembly)
        {
            try
            {
                var types = GetLoadableTypes(assembly);
                foreach (var type in types)
                {
                    ScanTypeForFields(type);
                }
            }
            catch (ReflectionTypeLoadException e)
            {
                Plugin.Log.LogWarning("Mod " + assembly.FullName + " tried to reference a missing reference " + e.Message);
            }
            catch (Exception e)
            {
                Plugin.Log.LogWarning("Failed to parse mod " + assembly.FullName + ":");
                Plugin.Log.LogWarning(e);
            }
        }


        internal static void ScanTypeForFields(Type type)
        {
            try
            {
                if (type.IsInterface || type.IsGenericTypeDefinition) return;
                var nestedTypes = type.GetNestedTypes();
                foreach (var nestedType in nestedTypes)
                {
                    ScanTypeForFields(nestedType);
                }
                var harmony = new Harmony("AdvancedCompany");
                var harmonyMethod = new HarmonyMethod(typeof(Game.Manager).GetMethod("PatchFields", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static));
                var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
                for (var i = 0; i < methods.Length; i++)
                {
                    var method = methods[i];
                    if (method.Name == "MoveNext") continue;
                    if (method.IsAbstract || !method.HasMethodBody() || method.DeclaringType != type || method.IsGenericMethod || method.IsGenericMethodDefinition) continue;
                    //Plugin.Log.LogMessage(type.FullName + "." + method.Name);
                    var instructions = HarmonyLib.PatchProcessor.ReadMethodBody(method);
                    var needsPatch = false;
                    foreach (var inst in instructions)
                    {
                        if (inst.Key == OpCodes.Ldfld && inst.Value is FieldInfo f)
                        {
                            foreach (var kv in Game.Manager.Patches)
                            {
                                if (f.DeclaringType.FullName + "." + f.Name == kv.Key)
                                {
                                    needsPatch = true;
                                    break;
                                }
                            }
                        }
                    }
                    if (needsPatch)
                    {
                        try
                        {
                            Plugin.Log.LogDebug("Patching " + method.DeclaringType.FullName + "->" + method.Name);
                            harmony.Patch(method, null, null, harmonyMethod, null, null);
                            Plugin.Log.LogDebug("Patched " + method.DeclaringType.FullName + "->" + method.Name);
                        }
                        catch (Exception e)
                        {
                            Plugin.Log.LogError("Error while transpiling " + method.DeclaringType.FullName + "->" + method.Name + ":");
                            Plugin.Log.LogError(e);
                        }
                    }
                }
            }
            catch (FileNotFoundException e)
            {
                Plugin.Log.LogWarning("Likely some soft dependency of another mod is missing (THIS IS MOST LIKELY A NON ERROR! THIS IS MOST LIKELY A NON ERROR! THIS IS MOST LIKELY A NON ERROR! THIS IS MOST LIKELY A NON ERROR!):");
                Plugin.Log.LogWarning(e);
            }
            catch (TypeLoadException e)
            {
                Plugin.Log.LogWarning("Likely some soft dependency of another mod is missing (THIS IS MOST LIKELY A NON ERROR! THIS IS MOST LIKELY A NON ERROR! THIS IS MOST LIKELY A NON ERROR! THIS IS MOST LIKELY A NON ERROR!):");
                Plugin.Log.LogWarning(e);
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while scanning types:");
                Plugin.Log.LogError(e);
            }
        }

        internal static Dictionary<Type, List<(string, MethodType)>> Patches = new Dictionary<Type, List<(string, MethodType)>>()
        {
            { typeof(global::Terminal), new List<(string, MethodType)>(){
                ("LoadNewNodeIfAffordable", MethodType.Normal),
                ("OnSubmit", MethodType.Normal),
                ("SetItemSales", MethodType.Normal),
                ("TextPostProcess", MethodType.Normal)
            }},
            { typeof(GameNetcodeStuff.PlayerControllerB), new List<(string, MethodType)>(){
                ("BeginGrabObject", MethodType.Normal),
                ("GrabObject", MethodType.Enumerator),
                ("GrabObjectClientRpc", MethodType.Normal),
                ("DespawnHeldObjectOnClient", MethodType.Normal),
                ("SetObjectAsNoLongerHeld", MethodType.Normal),
                ("PlaceGrabbableObject", MethodType.Normal),
                ("DestroyItemInSlot", MethodType.Normal)
            }},
            { typeof(global::RoundManager), new List<(string, MethodType)>() {
                ("SpawnScrapInLevel", MethodType.Normal),
                ("GenerateNewFloor", MethodType.Normal),
                ("PlotOutEnemiesForNextHour", MethodType.Normal),
                ("SpawnDaytimeEnemiesOutside", MethodType.Normal),
                ("SpawnRandomDaytimeEnemy", MethodType.Normal),
                ("SpawnRandomOutsideEnemy", MethodType.Normal),
                ("SpawnEnemyGameObject", MethodType.Normal),
                ("AssignRandomEnemyToVent", MethodType.Normal),
                ("EnemyCannotBeSpawned", MethodType.Normal),
                ("DespawnEnemyGameObject", MethodType.Normal),
                ("ResetEnemyTypesSpawnedCounts", MethodType.Normal),
                ("RefreshEnemiesList", MethodType.Normal),
                ("BeginEnemySpawning", MethodType.Normal),
            }},
            { typeof(global::GiftBoxItem), new List<(string, MethodType)>() {
                ("Start", MethodType.Normal)
            }},
            { typeof(global::EnemyAI), new List<(string, MethodType)>() {
                ("SubtractFromPowerLevel", MethodType.Normal)
            }},
            { typeof(global::EnemyVent), new List<(string, MethodType)>() {
                ("SyncVentSpawnTimeClientRpc", MethodType.Normal)
            }},
            { typeof(global::GameNetworkManager), new List<(string, MethodType)>() {
                ("ConvertUnsellableItemsToCredits", MethodType.Normal)
            }},
        };

        internal static void ApplyPatches()
        {
            var harmony = new Harmony("AdvancedCompany");
            foreach (var patch in Patches)
            {
                Plugin.Log.LogDebug("Patching " + patch.Key);
                var methods = patch.Key.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.FlattenHierarchy);
                foreach (var method in patch.Value)
                {
                    foreach (var m in methods)
                    {
                        if (m.Name == method.Item1)
                        {
                            var patchMethod = new HarmonyMethod(typeof(FieldPatches).GetMethod("PatchFields", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.FlattenHierarchy));
                            patchMethod.methodType = method.Item2;
                            Plugin.Log.LogDebug("Patching " + patch.Key.FullName + "->" + m.Name);
                            harmony.Patch(m, null, null, patchMethod, null, null);
                            Plugin.Log.LogDebug("Patched " + patch.Key.FullName + "->" + m.Name);
                        }
                    }
                }
            }
        }

        static IEnumerable<CodeInstruction> PatchFields(IEnumerable<CodeInstruction> instructions)
        {
            var inst = Game.Manager.PatchFields(instructions);
            return inst.AsEnumerable();
        }
    }
}
