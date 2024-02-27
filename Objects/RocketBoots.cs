using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Objects.Interfaces;
using AdvancedCompany.PostProcessing;

namespace AdvancedCompany.Objects
{
    [LoadAssets]
    internal class RocketBoots : Boots, IPerformJump, IOnUpdate
    {
        private static FieldInfo isJumping = AccessTools.Field(typeof(PlayerControllerB), "isJumping");
        private static FieldInfo isFallingFromJump = AccessTools.Field(typeof(PlayerControllerB), "isFallingFromJump");
        private static GameObject LeftRocketBoot;
        private static GameObject RightRocketBoot;
        private static AudioClip RocketBootsAudio;


        public override Player.BodyLayers GetLayers()
        {
            return Player.BodyLayers.HIDE_FEET;
        }

        public override GameObject[] CreateWearable(Player player)
        {
            var leftBones = new Transform[] {
                player.GetBone(Player.Bone.L_TOE),
                player.GetBone(Player.Bone.L_HEEL),
                player.GetBone(Player.Bone.L_FOOT),
                player.GetBone(Player.Bone.L_SHIN),
                player.GetBone(Player.Bone.L_THIGH),
            };
            var rightBones = new Transform[] {
                player.GetBone(Player.Bone.R_TOE),
                player.GetBone(Player.Bone.R_HEEL),
                player.GetBone(Player.Bone.R_FOOT),
                player.GetBone(Player.Bone.R_SHIN),
                player.GetBone(Player.Bone.R_THIGH),
            };

            var metarig = player.GetBone(Player.Bone.METARIG);
            var left = GameObject.Instantiate(LeftRocketBoot, metarig);
            left.GetComponent<SkinnedMeshRenderer>().rootBone = player.GetBone(Player.Bone.L_THIGH);
            left.GetComponent<SkinnedMeshRenderer>().bones = leftBones;
            left.GetComponent<SkinnedMeshRenderer>().ResetBounds();

            var right = GameObject.Instantiate(RightRocketBoot, metarig);
            right.GetComponent<SkinnedMeshRenderer>().rootBone = player.GetBone(Player.Bone.R_THIGH);
            right.GetComponent<SkinnedMeshRenderer>().bones = rightBones;
            right.GetComponent<SkinnedMeshRenderer>().ResetBounds();

            if (player.IsLocal)
            {
                left.layer = 23;
                var t = left.GetComponentsInChildren<Transform>();
                foreach (var tt in t)
                    tt.gameObject.layer = 23;

                right.layer = 23;
                t = left.GetComponentsInChildren<Transform>();
                foreach (var tt in t)
                    tt.gameObject.layer = 23;
            }
            return new GameObject[] { left, right };
        }

        public static void LoadAssets(AssetBundle assets)
        {
            LeftRocketBoot = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/RocketBootsLeft.prefab");
            RightRocketBoot = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/RocketBootsRight.prefab");
            RocketBootsAudio = assets.LoadAsset<AudioClip>("Assets/Sounds/RocketBoots.mp3");
        }

        static RocketBoots()
        {
            Network.Manager.AddListener<RocketJump>((msg) =>
            {
                for (var i = 0; i < StartOfRound.Instance.allPlayerScripts.Length; i++)
                {
                    var player = StartOfRound.Instance.allPlayerScripts[i];
                    if ((int)player.playerClientId == msg.PlayerNum)
                    {
                        if (msg.PlayerNum != (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
                            player.GetComponent<PlayerRocketBoots>().PlayParticles();
                        player.movementAudio.PlayOneShot(RocketBootsAudio);
                    }
                }
            });
        }

        public bool DoubleJumpAvailable = false;
        public bool DoubleJumpExecuted = false;
        public void OnUpdate(Player player)
        {
            if (player.IsOwner)
            {
                if (!player.IsGrounded && player.FallValue < 6f && player.IsFallingNoJump && !player.IsFallingFromJump && !player.IsJumping)
                {
                    if (!DoubleJumpAvailable && !DoubleJumpExecuted)
                    {
                        DoubleJumpAvailable = true;
                    }
                }
                else if (player.IsGrounded && !player.IsJumping && !player.IsFallingFromJump)
                {
                    DoubleJumpExecuted = false;
                    if (DoubleJumpAvailable)
                    {
                        DoubleJumpAvailable = false;
                    }
                }
            }
        }

        public bool PerformJump(Player player)
        {
            bool jumping = player.IsJumping || player.IsFallingFromJump;
            if (!DoubleJumpExecuted && !player.QuickMenuManager.isMenuOpen && (player.IsOwner && player.IsPlayerControlled && (!player.IsServer || player.IsHostPlayerObject) || player.IsTestingPlayer))
            {
                if (!DoubleJumpAvailable && !jumping)
                {
                    DoubleJumpAvailable = true;
                }
                else if (!player.InSpecialInteractAnimation && !player.IsTypingChat &&
                (
                 player.IsMovementHindered <= 0 || player.IsUnderwater
                ) &&
                !player.IsCrouching && DoubleJumpAvailable && player.FallValue < 6f)
                {
                    player.FallValue = Mathf.Max(player.FallValue, 0f) + player.JumpForce * 2f;
                    player.FallValueUncapped = player.FallValue;
                    //player.IsPlayerSliding = false;
                    player.IsFallingFromJump = false;
                    player.IsFallingNoJump = true;
                    DoubleJumpAvailable = false;
                    DoubleJumpExecuted = true;
                    Network.Manager.Send(new RocketJump() { PlayerNum = player.PlayerNum });
                }
            }
            return true;
        }
    }
}
