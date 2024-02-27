using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Messages;
using Unity.Netcode;
using HarmonyLib;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using JetBrains.Annotations;

namespace AdvancedCompany.Objects
{
    [LoadAssets]
    [HarmonyPatch]
    internal class VisionEnhancer : Helmet, IEquipmentFlashlight
    {
        private static GameObject NightVisionPrefab;
        private Transform Goggles;
        private GameObject PlayerLight;
        private Volume Volume;
        private static PostProcessing.VisionEnhancer Effect;

        static VisionEnhancer()
        {
            Network.Manager.AddListener<ActivateVisionEnhancer>((msg) =>
            {
                if (msg.PlayerNum != (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
                {
                    var player = Game.Player.GetPlayer(msg.PlayerNum);
                    if (player.Helmet is VisionEnhancer nightVision)
                    {
                        nightVision.SwitchFlashlight(player, msg.IsUsed);
                    }
                }
            });
        }

        public override void Start()
        {
            //if (insertedBattery == null)
            //    insertedBattery = new Battery(false, 1f);
            
            base.Start();
        }

        public static void Initialize()
        {
        }

        private float Phase = 0f;
        public override void Update()
        {
            base.Update();
            if (isBeingUsed)
                Phase = Mathf.Clamp01(Phase + Time.deltaTime * 5f);
            else
                Phase = Mathf.Clamp01(Phase - Time.deltaTime * 5f);

            if (Goggles != null)
                Goggles.transform.localRotation = Quaternion.Euler(110f + 160f * Phase, 0f, 0f);
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            this.isBeingUsed = false;
        }

        public override void Unequipped(Game.Player player)
        {
            this.isBeingUsed = false;
            if (player.IsLocal)
            {
                PostProcessing.VisionEnhancerEffect.Deactivate();
            }
        }

        public override GameObject[] CreateWearable(Player player)
        {
            var nightVision = GameObject.Instantiate(NightVisionPrefab, player.HeadMount.transform);
            nightVision.transform.localPosition = Vector3.zero;
            nightVision.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);

            Goggles = nightVision.transform.Find("Goggles");
            Goggles.transform.localRotation = Quaternion.Euler(isBeingUsed ? -90f : 110f, 0f, 0f);

            if (player.IsLocal)
            {
                nightVision.layer = 23;
                var t = nightVision.GetComponentsInChildren<Transform>();
                foreach (var tt in t)
                    tt.gameObject.layer = 23;
            }
            return new GameObject[] { nightVision };
        }

        public bool IsUsed()
        {
            return isBeingUsed;
        }
        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            isBeingUsed = false;
            PostProcessing.VisionEnhancerEffect.Deactivate();
            Network.Manager.Send(new ActivateVisionEnhancer() { PlayerNum = (int) playerHeldBy.playerClientId, IsUsed = false });
        }

        public static void LoadAssets(AssetBundle assets)
        {
            NightVisionPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/VisionEnhancer.prefab");
        }

        public void SwitchFlashlight(Player player, bool on)
        {
            if (on && insertedBattery.charge <= 0f)
                return;

            if (player.IsLocal)
            {
                //PostProcessing.intensity.Override(on ? 1f: 0f);

                //LightAndFog.SetActive(on);
                if (on)
                    PostProcessing.VisionEnhancerEffect.Activate();
                else
                    PostProcessing.VisionEnhancerEffect.Deactivate();
                
                Network.Manager.Send(new ActivateVisionEnhancer() { PlayerNum = player.PlayerNum, IsUsed = on });
            }

            this.isBeingUsed = on;
        }

        public bool CanBeUsed()
        {
            if (insertedBattery != null && insertedBattery.charge > 0f)
                return true;
            return false;
        }


        /*
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "SetNightVisionEnabled")]
        [HarmonyPrefix]
        public static bool SetNightVisionEnabled(GameNetcodeStuff.PlayerControllerB __instance, bool isNotLocalClient)
        {
            if (Game.Player.GetPlayer(__instance).Helmet is NightVision nightVision && nightVision.isBeingUsed)
            {
                __instance.nightVision.enabled = true;
                return false;
            }
            return true;
        }*/
    }
}
