using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using UnityEngine;
using AdvancedCompany.Game;
using System.Threading.Tasks;
using AdvancedCompany.Objects;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    internal class PlayerControllerB
    {

        private static Dictionary<ulong, AnimatorOverrideController> Animators = new();
        private static Dictionary<ulong, AnimationClipOverrides> Overrides = new();

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (__instance.gameObject.GetComponent<PlayerRocketBoots>() == null)
                __instance.gameObject.AddComponent<PlayerRocketBoots>();
        }


        private static float OriginalJumpForce = 13f;
        private static bool Initialized = false;

        public static Dictionary<ulong, Texture2D> PlayerAvatars = new();
        public static Dictionary<ulong, Task<Steamworks.Data.Image?>> LoadingAvatars = new();

        public static Texture2D GetAvatar(GameNetcodeStuff.PlayerControllerB player)
        {
            if (PlayerAvatars.ContainsKey(player.playerSteamId))
                return PlayerAvatars[player.playerSteamId];
            return null;
        }
        
        private static void LoadAvatar(GameNetcodeStuff.PlayerControllerB player)
        {
            var steamID = player.playerSteamId;
            if (steamID > 0 && SteamClient.IsValid &&
                !PlayerAvatars.ContainsKey(steamID))
            {
                if (!LoadingAvatars.ContainsKey(steamID))
                {
                    LoadingAvatars.Add(steamID, SteamFriends.GetLargeAvatarAsync(steamID));
                }
                else if (LoadingAvatars[steamID].IsCompletedSuccessfully)
                {
                    PlayerAvatars.Add(steamID, GetTextureFromImage(LoadingAvatars[steamID].Result));
                }
                else if (LoadingAvatars[steamID].IsFaulted || LoadingAvatars[steamID].IsCanceled)
                {
                    PlayerAvatars.Add(steamID, null);
                }
            }
        }

        public static Texture2D GetTextureFromImage(Steamworks.Data.Image? image)
        {
            if (image.HasValue)
            {
                Texture2D texture2D = new Texture2D((int)image.Value.Width, (int)image.Value.Height);
                var pixels = new Color[image.Value.Width * image.Value.Height];
                for (int i = 0; i < image.Value.Width; i++)
                {
                    for (int j = 0; j < image.Value.Height; j++)
                    {
                        Steamworks.Data.Color pixel = image.Value.GetPixel(i, j);
                        pixels[i + (image.Value.Height - (j + 1)) * image.Value.Width] = new UnityEngine.Color((float)(int)pixel.r / 255f, (float)(int)pixel.g / 255f, (float)(int)pixel.b / 255f, (float)(int)pixel.a / 255f);
                    }
                }
                texture2D.SetPixels(pixels);
                texture2D.Apply();
                return texture2D;
            }
            return null;
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void UpdatePost(GameNetcodeStuff.PlayerControllerB __instance)
        {
            LoadAvatar(__instance);

            if (global::GameNetworkManager.Instance.localPlayerController == __instance)
            {
                PostProcessing.VisionEnhancerEffect.Update();
                PietSmietController.UpdateEffect();
                //__instance.jumpForce = OriginalJumpForce * Perks.GetMultiplier(__instance, "JumpHeight");
            }
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "DamagePlayer")]
        [HarmonyPrefix]
        public static void DamagePlayer(GameNetcodeStuff.PlayerControllerB __instance, ref int damageNumber, bool hasDamageSFX, bool callRPC, CauseOfDeath causeOfDeath, int deathAnimation, bool fallDamage, Vector3 force)
        {
            try 
            {
                if (global::GameNetworkManager.Instance.localPlayerController == __instance)
                {
                    if (!fallDamage && causeOfDeath != CauseOfDeath.Gravity)
                        damageNumber = (int)((float)damageNumber * Perks.GetMultiplier(__instance, "Damage"));
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "PlayerJump", MethodType.Enumerator)]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchPlayerJump(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->PlayerJump...");

            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].IsLdfld("GameNetcodeStuff.PlayerControllerB.jumpForce"))
                {
                    IL.Patches.AddMultiplierInstruction("JumpHeight", inst, i + 1);
                    Plugin.Log.LogDebug("Added jump height to jump!");
                }
            }
            Plugin.Log.LogDebug("Patched PlayerControllerB->PlayerJump...");
            return inst.AsEnumerable();
        }

        static IEnumerable<CodeInstruction> PatchJump_performed(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->Jump_performed...");

            var method = typeof(Game.Player).GetMethod("PerformJump", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            var first = false;
            var second = false;
            for (var i = 0; i < inst.Count; i++)
            {
                if (!first && inst[i].opcode == OpCodes.Ldc_R4 && (float)inst[i].operand == 0.08f)
                {
                    IL.Patches.AddMultiplierInstruction("JumpStamina", inst, i + 1);
                    Plugin.Log.LogDebug("Added jump stamina to jump!");
                    first = true;
                }
                if (first && second)
                    break;
            }
            inst.Insert(0, new CodeInstruction(OpCodes.Call, method));
            inst.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
            Plugin.Log.LogDebug("Patched PlayerControllerB->Jump_performed...");
            return inst.AsEnumerable();
        }

        static IEnumerable<CodeInstruction> PatchPlayerHitGroundEffects(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->PlayerHitGroundEffects...");

            bool first = false;
            bool second = false;
            //bool third = false;
            var inst = new List<CodeInstruction>(instructions);
            for (var i = 0; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldc_R4 && inst[i].operand is float f)
                {
                    if (f == -9f || f == -16f || f == -2f || f == -48.5f || f == -45f || f == -40f)
                    {
                        Plugin.Log.LogDebug("Added FallDamage Multiplier to " + f);
                        IL.Patches.AddMultiplierInstruction("FallDamage", inst, i + 1, true);
                    }
                }
            }
            Plugin.Log.LogDebug("Patched PlayerControllerB->PlayerHitGroundEffects...");
            return inst.AsEnumerable();
        }

        static IEnumerable<CodeInstruction> PatchLateUpdate(ILGenerator generator, IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->LateUpdate...");

            bool first = false;
            bool second = false;
            var inst = new List<CodeInstruction>(instructions);
            var localSprint = generator.DeclareLocal(typeof(float));

            inst.Insert(0, new CodeInstruction(OpCodes.Stloc, localSprint));
            inst.Insert(0, new CodeInstruction(OpCodes.Mul));
            inst.Insert(0, new CodeInstruction(OpCodes.Call, IL.Patches.GetMultiplier));
            inst.Insert(0, new CodeInstruction(OpCodes.Ldstr, "SprintStamina"));
            inst.Insert(0, new CodeInstruction(OpCodes.Ldfld, AccessTools.Field(typeof(GameNetcodeStuff.PlayerControllerB), "sprintTime")));
            inst.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));
            for (var i = 6; i < inst.Count; i++)
            {
                if (inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Single sprintTime")
                {
                    inst.RemoveAt(i);
                    inst.RemoveAt(i - 1);
                    inst.Insert(i - 1, new CodeInstruction(OpCodes.Ldloc, localSprint));
                    if (inst[i + 1].opcode == OpCodes.Add)
                    {
                        Plugin.Log.LogDebug("Adding sprint regen multiplier.");
                        IL.Patches.AddMultiplierInstruction("StaminaRegen", inst, i + 3);
                    }
                }

                if (!second && inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Single carryWeight")
                {
                    IL.Patches.AddWeightMultiplierInstruction("Weight", inst, i + 1);
                    Plugin.Log.LogDebug("Added Weight multiplier.");
                    second = true;
                }

                if (first && second)
                    break;
            }
            Plugin.Log.LogDebug("Patched PlayerControllerB->LateUpdate...");
            return inst.AsEnumerable();
        }

        static IEnumerable<CodeInstruction> PatchUpdate(IEnumerable<CodeInstruction> instructions)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->Update...");

            var inst = new List<CodeInstruction>(instructions);
            bool first = false;
            bool second = false;
            bool third = false;
            bool fourth = false;
            bool fifth = false;
            bool sixth = false;
            //var getMaxSpeedMethod = typeof(Game.Player).GetMethod("GetSpeed", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            var movementMethod = typeof(Game.Player).GetMethod("Movement", BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy);
            for (var i = 0; i < inst.Count; i++)
            {
                if (!first && inst[i].opcode == OpCodes.Ldc_R4 && (float)inst[i].operand == 2.25f)
                {
                    inst.RemoveAt(i + 2);
                    IL.Patches.AddMultiplierInstruction("SprintSpeed", inst, i + 2, false);

                    IL.Patches.AddMultiplierInstruction("SprintSpeed", inst, i + 1);
                    first = true;
                    Plugin.Log.LogDebug("Added SprintSpeed multiplier.");
                }
                if (!second && inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Single climbSpeed")
                {
                    IL.Patches.AddMultiplierInstruction("ClimbingSpeed", inst, i + 1);
                    second = true;
                    Plugin.Log.LogDebug("Added ClimbingSpeed multiplier.");
                }
                if (!third && inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "System.Boolean suckingPlayersOutOfShip")
                {
                    object target = null;
                    for (var j = i; j < inst.Count; j++)
                    {
                        if (inst[j].opcode == OpCodes.Ldfld && inst[j].operand.ToString() == "System.Boolean isClimbingLadder" && inst[j + 1].opcode == OpCodes.Brfalse)
                        {
                            target = inst[j + 1].operand;
                            break;
                        }
                    }
                    inst.Insert(i - 7, new CodeInstruction(OpCodes.Brtrue, target)); 
                    inst.Insert(i - 7, new CodeInstruction(OpCodes.Call, movementMethod)); 
                    inst.Insert(i - 7, new CodeInstruction(OpCodes.Ldarg_0));
                    
                    third = true;
                }

                if (!fourth && inst[i].opcode == OpCodes.Ldc_R4 && ((float)inst[i].operand) == -35f)
                {
                    Plugin.Log.LogDebug("Added FallDamage multiplier to " + inst[i].operand);
                    IL.Patches.AddMultiplierInstruction("FallDamage", inst, i + 1, true);
                    fourth = true;
                }
                if (!fifth && inst[i].IsLdfld("GameNetcodeStuff.PlayerControllerB.carryWeight"))
                {
                    IL.Patches.AddWeightMultiplierInstruction("WeightSpeed", inst, i + 1);
                    fifth = true;
                    Plugin.Log.LogDebug("Added WeightSpeed multiplier.");
                }
                if (!sixth && inst[i].IsLdfld("GameNetcodeStuff.PlayerControllerB.jumpForce"))
                {
                    IL.Patches.AddMultiplierInstruction("JumpHeight", inst, i + 1);
                    sixth = true;
                    Plugin.Log.LogDebug("Added jump height to jump!");
                }
                if (first && second && third && fourth && fifth && sixth)
                    break;
            }

            var method = typeof(Game.Player).GetMethod("OnUpdate", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
            inst.Insert(0, new CodeInstruction(OpCodes.Call, method));
            inst.Insert(0, new CodeInstruction(OpCodes.Ldarg_0));

            Plugin.Log.LogDebug("Patched PlayerControllerB->Update...");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "Jump_performed")]
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "LateUpdate")]
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "Update")]
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "PlayerHitGroundEffects")]
        static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase method)
        {
            if (method.Name == "Jump_performed")
            {
                return PatchJump_performed(instructions);
            }
            else if (method.Name == "LateUpdate")
            {
                return PatchLateUpdate(generator, instructions);
            }
            else if (method.Name == "Update")
            {
                return PatchUpdate(instructions);
            }
            else if (method.Name == "PlayerHitGroundEffects")
            {
                return PatchPlayerHitGroundEffects(generator, instructions);
            }
            return instructions;
        }

        [HarmonyPatch(typeof(global::StartOfRound), "ReviveDeadPlayers")]
        [HarmonyPostfix]
        public static void Reequip(global::StartOfRound __instance)
        {
            for (var i = 0; i < __instance.allPlayerScripts.Length; i++)
            {
                if (__instance.allPlayerScripts[i].isPlayerControlled)
                {
                    var player = Game.Player.GetPlayer(__instance.allPlayerScripts[i]);
                    player.Reequip();
                }
            }
        }

    }
}
