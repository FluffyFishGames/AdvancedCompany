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
using AdvancedCompany.Config;

namespace AdvancedCompany.Objects
{
    [HarmonyPatch]
    [LoadAssets]
    internal class Flippers : Boots, IMovement
    {
        private static GameObject LeftSwimmingFin;
        private static GameObject RightSwimmingFin;

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
                player.GetBone(Player.Bone.L_SHIN)
            };
            var rightBones = new Transform[] {
                player.GetBone(Player.Bone.R_TOE),
                player.GetBone(Player.Bone.R_HEEL),
                player.GetBone(Player.Bone.R_FOOT),
                player.GetBone(Player.Bone.R_SHIN),
            };

            var metarig = player.GetBone(Player.Bone.METARIG);
            var left = GameObject.Instantiate(LeftSwimmingFin, metarig);
            left.GetComponent<SkinnedMeshRenderer>().rootBone = player.GetBone(Player.Bone.L_SHIN);
            left.GetComponent<SkinnedMeshRenderer>().bones = leftBones;

            var right = GameObject.Instantiate(RightSwimmingFin, metarig);
            right.GetComponent<SkinnedMeshRenderer>().rootBone = player.GetBone(Player.Bone.R_SHIN);
            right.GetComponent<SkinnedMeshRenderer>().bones = rightBones;

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
            LeftSwimmingFin = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/FlippersLeft.prefab");
            RightSwimmingFin = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/FlippersRight.prefab");
        }

        private bool LastFrameActive = false;
        private float Gravity = 0f;
        private float MaxTarget = 6f;
        private bool PreviousSpecialAnimation;
        public bool Movement(Player player)
        {
            var colliders = Physics.OverlapSphere(player.Controller.gameplayCamera.transform.position, 0.5f);
            bool underwater = false;
            for (var i = 0; i < colliders.Length; i++)
            {
                var quickSand = colliders[i].GetComponent<QuicksandTrigger>();
                if (quickSand != null && quickSand.isWater)
                {
                    underwater = colliders[i].bounds.Contains(player.Controller.gameplayCamera.transform.position + new Vector3(0f, LastFrameActive ? -0.5f : 0f, 0f));
                    if (underwater)
                    {
                        if (player.IsUnderwater && player.Controller.underwaterCollider != colliders[i])
                            player.Controller.underwaterCollider = colliders[i];
                        break;
                    }
                }
            }
            if (underwater)
            {
                if (!LastFrameActive)
                {
                    Gravity = player.FallValue;
                }
                if (player.Controller.thisController.isGrounded)
                    Gravity = 0f;

                //player.IsJumping = true;
                //player.IsFallingFromJump = true;

                var target = (player.Controller.gameplayCamera.transform.forward * player.Controller.moveInputVector.y) + 
                             player.Controller.gameplayCamera.transform.right * player.Controller.moveInputVector.x;

                if (player.Controller.playerActions.Movement.Jump.IsPressed())
                    target.y = 1.5f;
                if (player.Controller.playerActions.Movement.Crouch.IsPressed())
                    target.y = -1.5f;

                if (target.magnitude > MaxTarget)
                    target = target.normalized * MaxTarget;

                //player.PlayerSlidingTimer = 0f;
                player.WalkForce = Vector3.MoveTowards(player.WalkForce, target, 30f * Time.deltaTime);

                if (Gravity < -2f)
                {
                    Gravity += Time.deltaTime * 40f;
                    if (Gravity > -2f) Gravity = -2f;
                }
                if (Gravity > -2f)
                {
                    Gravity -= Time.deltaTime * 40f;
                    if (Gravity < -2f) Gravity = -2f;
                }
                player.SprintMultiplier = 2.25f;
                player.Controller.isSprinting = true;
                player.FallValue = player.WalkForce.y * 1.5f;// Mathf.MoveTowards(player.FallValue, 0f, Time.deltaTime * 5f);
                player.FallValueUncapped = player.FallValue;
                player.IsMovementHindered = 0;
                player.MovementHinderedPrev = 0;

                var maxSpeed = Mathf.Clamp(10f * ServerConfiguration.Instance.Items.Flippers.Speed - player.Controller.carryWeight * 5f * AdvancedCompany.Perks.GetMultiplier(AdvancedCompany.Perks.WeightSpeedPerk), 2f, 10f * ServerConfiguration.Instance.Items.Flippers.Speed);

                var move = (player.WalkForce * maxSpeed + new Vector3(0f, Gravity, 0f));
                player.Controller.thisController.Move(move * Time.deltaTime);
                LastFrameActive = true;
                return true;
            }
            else if (LastFrameActive)
            {
//                player.WalkForce = new Vector3(player.WalkForce.x, 0f, player.WalkForce.z);
                player.IsMovementHindered = 0;
                player.MovementHinderedPrev = 0;
                
//                player.IsJumping = true;
//                player.IsFallingFromJump = false;
//                player.JumpCoroutine = player.Controller.StartCoroutine(player.PlayerJump());
                LastFrameActive = false;
                return false;
            }
            return false;
        }

        static Flippers()
        {
        }
        /*
        [HarmonyPatch(typeof(QuicksandTrigger), "OnTriggerStay")]
        [HarmonyPrefix]
        public static bool FixWaterBug(QuicksandTrigger __instance, Collider other)
        {
            if (__instance.isWater)
            {
                if (other.gameObject.CompareTag("Player"))
                {
                    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
                    if (component != null && 
                        component == GameNetworkManager.Instance.localPlayerController && (component.underwaterCollider != null))
                    {
                        return false;
                    }
                }
            }
            return true;
        }*/
    }
}
