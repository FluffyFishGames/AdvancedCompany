using AdvancedCompany.Lib;
using BepInEx.Bootstrap;
using BepInEx.Logging;
using HarmonyLib;
using Steamworks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using static PlayerSettings;

namespace AdvancedCompany.Patches
{
    internal class MaskedEnemyPatches
    {
        public class MaskedPlayerAdditions
        {
            public MaskedPlayerEnemy MaskedPlayer;
            public int MimickedPlayer = 9999;
            public Dictionary<string, GameObject> AppliedCosmetics = new();
            private Dictionary<Game.Player.Bone, Transform> Bones = new();

            public MaskedPlayerAdditions(MaskedPlayerEnemy enemy)
            {
                MaskedPlayer = enemy;
                Plugin.Log.LogMessage("Searching for bones of masked enemy.");
                FindBones(enemy.transform.Find("ScavengerModel").Find("metarig"));
            }

            public Transform GetBone(Game.Player.Bone bone)
            {
                if (Bones.ContainsKey(bone))
                    return Bones[bone];
                return null;
            }

            protected void FindBones(Transform transform)
            {
                for (var i = 0; i < transform.childCount; i++)
                {
                    var child = transform.GetChild(i);
                    if (child.name == "ScavengerModelArmsOnly")
                        continue;
                    if (Game.Player.BoneNames.ContainsKey(child.name))
                        Bones.Add(Game.Player.BoneNames[child.name], child);

                    FindBones(child);
                }
            }

        }
        private static Dictionary<int, MaskedPlayerAdditions> MaskedPlayers = new Dictionary<int, MaskedPlayerAdditions>();

        [HarmonyPatch(typeof(MaskedPlayerEnemy), "Start")]
        [HarmonyPostfix]
        private static void Start(MaskedPlayerEnemy __instance)
        {
            ApplyCosmetics(__instance);
        }

        internal static void ApplyCosmetics(MaskedPlayerEnemy enemy)
        {
            if (enemy == null)
            {
                Plugin.Log.LogWarning("Masked enemy was null when applying cosmetics.");
                return;
            }

            var instanceID = enemy.GetInstanceID();
            if (!MaskedPlayers.ContainsKey(instanceID))
            {
                // tidy up dictionary
                var r = new List<int>();
                foreach (var kv in MaskedPlayers)
                {
                    if (kv.Value.MaskedPlayer == null)
                    {
                        Plugin.Log.LogMessage("Removing deleted masked player from cache.");
                        r.Add(kv.Key);
                    }
                }
                foreach (var n in r)
                    MaskedPlayers.Remove(n);

                if (!MaskedPlayers.ContainsKey(instanceID))
                    MaskedPlayers.Add(instanceID, new MaskedPlayerAdditions(enemy));
            }
            var additions = MaskedPlayers[instanceID];
            if (enemy.mimickingPlayer != null)
            {
                if ((int)enemy.mimickingPlayer.playerClientId != additions.MimickedPlayer)
                {
                    Plugin.Log.LogMessage("Masked player is mimicking player " + enemy.mimickingPlayer.playerClientId + ".");
                    enemy.SetSuit(enemy.mimickingPlayer.currentSuitID);

                    var mimickedPlayer = Game.Player.GetPlayer(enemy.mimickingPlayer);
                    additions.MimickedPlayer = mimickedPlayer.PlayerNum;

                    Plugin.Log.LogMessage("Cosmetics of player: " + string.Join(", ", mimickedPlayer.Cosmetics));
                    var remove = new List<string>();
                    var add = new List<string>();
                    foreach (var kv in additions.AppliedCosmetics)
                    {
                        if (!mimickedPlayer.Cosmetics.Contains(kv.Key))
                            remove.Add(kv.Key);
                    }
                    foreach (var k in mimickedPlayer.Cosmetics)
                    {
                        if (!additions.AppliedCosmetics.ContainsKey(k))
                            add.Add(k);
                    }
                    for (var i = 0; i < remove.Count; i++)
                    {
                        GameObject.Destroy(additions.AppliedCosmetics[remove[i]]);
                        additions.AppliedCosmetics.Remove(remove[i]);
                    }
                    for (var i = 0; i < add.Count; i++)
                    {
                        AddCosmetic(additions, add[i]);
                    }
                }
            }
            else
            {
                Plugin.Log.LogMessage("MaskedPlayer is not mimicking anyone.");
                if (additions.MimickedPlayer != 9999)
                {
                    Plugin.Log.LogMessage("Removing all cosmetics");
                    additions.MimickedPlayer = 9999;
                    foreach (var kv in additions.AppliedCosmetics)
                    {
                        GameObject.Destroy(kv.Value);
                    }
                    additions.AppliedCosmetics = new();
                }
            }
        }

        internal static void AddCosmetic(MaskedPlayerAdditions maskedPlayer, string cosmeticID)
        {
            if (CosmeticDatabase.AllCosmetics.ContainsKey(cosmeticID))
            {
                var cosmetic = GameObject.Instantiate(CosmeticDatabase.AllCosmetics[cosmeticID].gameObject);
                var instance = cosmetic.GetComponent<AdvancedCompany.Cosmetics.CosmeticInstance>();

                Transform bone = null;
                switch (instance.cosmeticType)
                {
                    case AdvancedCompany.Cosmetics.CosmeticType.HAT:
                        bone = maskedPlayer.GetBone(Game.Player.Bone.SPINE_3);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.CHEST:
                        bone = maskedPlayer.GetBone(Game.Player.Bone.SPINE_2);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.HIP:
                        bone = maskedPlayer.GetBone(Game.Player.Bone.SPINE_0);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.R_LOWER_ARM:
                        bone = maskedPlayer.GetBone(Game.Player.Bone.R_LOWER_ARM);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.L_SHIN:
                        bone = maskedPlayer.GetBone(Game.Player.Bone.L_SHIN);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.R_SHIN:
                        bone = maskedPlayer.GetBone(Game.Player.Bone.R_SHIN);
                        break;
                }

                if (bone == null)
                {
                    Plugin.Log.LogWarning("Bone was null!");
                }
                else
                {
                    cosmetic.transform.position = bone.position;
                    cosmetic.transform.rotation = bone.rotation;
                    cosmetic.transform.localScale *= 0.38f;
                    cosmetic.transform.parent = bone;
                    maskedPlayer.AppliedCosmetics.Add(cosmeticID, cosmetic);
                }
            }
        }
    }
}
