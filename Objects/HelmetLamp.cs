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
    internal class HelmetLamp : Helmet, IEquipmentFlashlight
    {
        private static GameObject HelmetLampPrefab;
        private Material Material;
        private Light Light1;
        private Light Light2;
        private Light Light3;
        private float StrengthTarget = 1f;
        private float Strength = 1f;
        private float StrengthTime = 0.5f;
        private float StrengthPhase = 1f;
        static HelmetLamp()
        {
            Network.Manager.AddListener<ActivateHelmetLamp>((msg) =>
            {
                if (msg.PlayerNum != (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
                {
                    var player = Game.Player.GetPlayer(msg.PlayerNum);
                    if (player.Helmet is HelmetLamp helmetLamp)
                    {
                        helmetLamp.SwitchFlashlight(player, msg.IsUsed);
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

        public bool IsUsed()
        {
            return isBeingUsed;
        }
        private float Phase = 0f;
        public override void Update()
        {
            base.Update();
            if (isBeingUsed)
            {
                //if (Light1 != null && !Light1.enabled) Light1.enabled = true;
                if (Light2 != null && !Light2.enabled) Light2.enabled = true;
                //if (Light3 != null && !Light3.enabled) Light3.enabled = true;
                Phase = Mathf.Clamp01(Phase + Time.deltaTime * 5f);

                if (StrengthPhase >= 1f)
                {
                    StrengthTime = UnityEngine.Random.Range(0.1f, 0.3f);
                    StrengthTarget = UnityEngine.Random.Range(0.95f, 1.12f);
                    Strength = StrengthTarget;
                    StrengthPhase = 0f;
                }
                else
                {
                    var str = Strength + (StrengthTarget - Strength) * StrengthPhase;
                    StrengthPhase += Time.deltaTime / StrengthTime;
                    //if (Light1 != null) Light1.intensity = 100f * Phase * str;
                    if (Light2 != null) Light2.intensity = 100f * Phase * str;
                    //if (Light3 != null) Light3.intensity = 100f * Phase * str;
                }
            }
            else
            {
                //if (Light1 != null && Light1.enabled) Light1.enabled = false;
                if (Light2 != null && Light2.enabled) Light2.enabled = false;
                //if (Light3 != null && Light3.enabled) Light3.enabled = false;
                Phase = Mathf.Clamp01(Phase - Time.deltaTime * 5f);
            }

            if (Material != null)
                Material.SetFloat("_EmissiveExposureWeight", 2f - (Phase * 2f));
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            this.isBeingUsed = false;
        }

        public override void Unequipped(Game.Player player)
        {
            this.isBeingUsed = false;
        }

        public override GameObject[] CreateWearable(Player player)
        {
            var helmetLamp = GameObject.Instantiate(HelmetLampPrefab, player.HeadMount.transform);
            helmetLamp.transform.localPosition = new Vector3(-0.0015f, 0.066f, 0.008f);
            helmetLamp.transform.localRotation = Quaternion.Euler(-0.053f, 180f, 0f);
            var lights = helmetLamp.GetComponentsInChildren<Light>();
            Light2 = lights[0];
            //Light1 = lights[0];
            //Light2 = lights[1];
            //Light3 = lights[2];

            var renderer = helmetLamp.GetComponent<Renderer>();
            Material = renderer.material;
            if (player.IsLocal)
            {
                renderer.gameObject.layer = 23;

                //Light1.transform.parent = player.Controller.gameplayCamera.transform;
                Light2.transform.parent = player.Controller.gameplayCamera.transform;
                //Light3.transform.parent = player.Controller.gameplayCamera.transform;
                //Light1.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                Light2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                //Light3.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                //Light1.transform.localPosition = new Vector3(-0.2f, 0.25f, 0.15f);
                Light2.transform.localPosition = new Vector3(0f, 0.5f, 0.45f);
                //Light3.transform.localPosition = new Vector3(0.2f, 0.25f, 0.15f);
                return new GameObject[] { helmetLamp, Light2.gameObject };

                //return new GameObject[] { helmetLamp, Light1.gameObject, Light2.gameObject, Light3.gameObject };
            }


            return new GameObject[] { helmetLamp };
        }

        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            isBeingUsed = false;

            // deactivate
            Network.Manager.Send(new ActivateHelmetLamp() { PlayerNum = (int) playerHeldBy.playerClientId, IsUsed = false });
        }

        public static void LoadAssets(AssetBundle assets)
        {
            HelmetLampPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/HelmetLamp.prefab");
        }

        public void SwitchFlashlight(Player player, bool on)
        {
            if (on && insertedBattery.charge <= 0f)
                return;

            if (player.IsLocal)
            {
                //PostProcessing.intensity.Override(on ? 1f: 0f);

                //LightAndFog.SetActive(on);
                Network.Manager.Send(new ActivateHelmetLamp() { PlayerNum = player.PlayerNum, IsUsed = on });
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
