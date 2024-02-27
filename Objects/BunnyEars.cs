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
    internal class BunnyEars : Helmet, ICursed, IOnUpdate, IMovement
    {
        private Light ItemLight;
        private Light ClothingLight;
        private float Phase = 0f;
        private Material ItemMaterial;
        private Material ClothingMaterial;
        private AudioSource ItemAudio;
        private AudioSource ClothingAudio;
        public bool CurseLifted = false;
        private bool CurseLiftedLastFrame = false;
        private float AttractEnemiesTimer = 0f;
        private GameNetcodeStuff.PlayerControllerB lastPlayerHeldBy;
        internal static GameObject BunnyEarsPrefab;
        private bool LightsOn = true;

        void Awake()
        {
            Phase = UnityEngine.Random.value;
            ItemAudio = transform.Find("Audio").GetComponent<AudioSource>();
            ItemMaterial = GetComponent<MeshRenderer>().material;
            ItemLight = GetComponentInChildren<Light>();
            if (!ClientConfiguration.Instance.Compability.DisableMusic)
                ItemAudio.Play();
        }

        public override void EquipItem()
        {
            base.EquipItem();
        }

        private IEnumerator CurseEquip()
        {
            while (!this.CurseLifted && this.playerHeldBy != null && !this.playerHeldBy.isInHangarShipRoom)
            {
                if (this.playerHeldBy.grabbedObjectValidated && !this.IsEquipped())
                {
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                    this.Equip();
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (!this.CurseLifted && !this.playerHeldBy.isInHangarShipRoom && !this.IsEquipped())
            {
                StartCoroutine(CurseEquip());
            }
        }

        public override void Equipped(Player player)
        {
            base.Equipped(player);
            if (!CurseLifted)
            {
                if (!ClientConfiguration.Instance.Compability.DisableMusic)
                    ItemAudio.Stop();
            }
        }

        public override void DiscardItem()
        {
            if (!CurseLifted)
            {
                if (!ClientConfiguration.Instance.Compability.DisableMusic)
                    ItemAudio.Play();
            }
            base.DiscardItem();
        }

        public override bool CanUnequip()
        {
            return CurseLifted;
        }

        IEnumerator StopAudio()
        {
            if (!ClientConfiguration.Instance.Compability.DisableMusic)
            {
                ItemAudio.Stop();
                ItemLight.enabled = false;
                var startVolume = ClothingAudio.volume;
                while (ClothingAudio != null && ClothingAudio.pitch > 0)
                {
                    ClothingAudio.pitch -= Time.deltaTime / 3f;
                    ClothingAudio.volume = ClothingAudio.pitch * startVolume;
                    yield return new WaitForEndOfFrame();
                }
                ClothingAudio.Stop();
            }
            else yield return new WaitForEndOfFrame();
        }

        public override void Update()
        {
            base.Update();
            var lightsOn = (playerHeldBy != null && this.isHeld && playerHeldBy.currentItemSlot == GrabbableObjectAdditions.GetInventoryPosition(this)) || playerHeldBy == null;
            if (playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
                CurseLifted = true;

            if (CurseLifted)
            {
                ItemMaterial.SetFloat("_EmissiveExposureWeight", 2f);
                if (ClothingMaterial != null)
                    ClothingMaterial.SetFloat("_EmissiveExposureWeight", 2f);
                ItemLight.enabled = false;
                if (ClothingLight != null)
                    ClothingLight.enabled = false;

                if (CurseLifted != CurseLiftedLastFrame)
                    StartCoroutine(StopAudio());
                else if (ItemAudio.isPlaying)
                    ItemAudio.Stop();
            }
            else
            {
                if (ItemAudio != null)
                    ItemAudio.volume = ClientConfiguration.Instance.Graphics.MusicVolume;
                if (ClothingAudio != null)
                    ClothingAudio.volume = ClientConfiguration.Instance.Graphics.MusicVolume;

                if (lightsOn != LightsOn)
                {
                    ItemLight.enabled = lightsOn;
                    LightsOn = lightsOn;
                }
                Phase += Time.deltaTime / 5f;
                while (Phase > 1f)
                    Phase -= 1f;

                var c = Color.HSVToRGB(Phase % 1f, 1f, 1f);
                ItemMaterial.SetColor("_EmissiveColor", c);
                ItemLight.color = c;
                if (ClothingMaterial != null)
                    ClothingMaterial.SetColor("_EmissiveColor", c);
                if (ClothingLight != null)
                    ClothingLight.color = c;
            }
            CurseLiftedLastFrame = CurseLifted;
            lastPlayerHeldBy = playerHeldBy;
        }

        public override GameObject[] CreateWearable(Player player)
        {
            var spine3 = player.GetBone(Player.Bone.SPINE_3);
            var ears = GameObject.Instantiate(BunnyEarsPrefab, spine3);
            ears.transform.localPosition = new Vector3(-0.002f, 0.389f, 0.079f);
            ears.transform.localRotation = Quaternion.Euler(-91.477f, 0f, -179.998f);
            ears.transform.localScale = new Vector3(0.40f, 0.40f, 0.40f);

            ClothingMaterial = ears.GetComponent<MeshRenderer>().material;
            ClothingLight = ears.GetComponentInChildren<Light>();
            ClothingAudio = ears.GetComponent<AudioSource>();
            
            if (player.IsLocal)
            {
                ears.layer = 23;
            }

            ItemAudio.Stop();
            if (!CurseLifted)
            {
                ClothingLight.enabled = true;
                if (!ClientConfiguration.Instance.Compability.DisableMusic)
                    ClothingAudio.Play();
            }
            return new GameObject[] { ears };
        }

        public static void LoadAssets(AssetBundle assets)
        {
            BunnyEarsPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/BunnyEars.prefab");
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "DiscardHeldObject")]
        [HarmonyPrefix]
        public static bool DiscardHeldObject(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer is BunnyEars ears)
            {
                if (!ears.CurseLifted)
                    return false;
            }
            return true;
        }

        public override int GetItemDataToSave()
        {
            return CurseLifted ? 1 : 0;
        }

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            CurseLifted = saveData == 1;
            CurseLiftedLastFrame = CurseLifted;
        }

        public void OnUpdate(Player player)
        {
            if (!this.CurseLifted)
            {
                player.Controller.sprintMeter += Time.deltaTime * 0.2f;
            }
        }

        public void UpdateEffects(Player player)
        {
            if (!CurseLifted)
            {
                player.SetMultiplierOverride("WeightSpeed", 0f);
                player.SetMultiplierOverride("Weight", 0f);
                player.SetMultiplierOverride("SprintSpeed", 1f);
                player.SetMultiplierOverride("FallDamage", 10f);
                player.SetMultiplierOverride("JumpHeight", 0.7f);
                player.SetMultiplierOverride("SprintStamina", 100f);
                player.SetMultiplierOverride("JumpStamina", 100f);
            }
        }

        public bool Movement(Player player)
        {
            if (!CurseLifted)
            {
                player.IsPlayerSliding = false;
                player.IsMovementHindered = 0;
                if (!player.IsJumping && !player.IsFallingFromJump && !player.IsFallingNoJump)
                    return true;
            }
            return false;
        }
    }
}
