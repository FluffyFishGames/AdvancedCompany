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
using System.Collections;
using AdvancedCompany.Config;

namespace AdvancedCompany.Objects
{
    [LoadAssets]
    [HarmonyPatch]
    internal class TacticalHelmet : WalkieTalkie, IEquipmentCommunication, IHelmet, IEquipmentFlashlight
    {
        private static GameObject HelmetPrefab;
        private Material Material;
        private Light Light1;
        private Light Light2;
        private float StrengthTarget = 1f;
        private float Strength = 1f;
        private float StrengthTime = 0.5f;
        private float StrengthPhase = 1f;
        public bool CanUnequip()
        {
            return true;
        }
        public virtual Game.Player.BodyLayers GetLayers() { return Game.Player.BodyLayers.NONE; }
        static TacticalHelmet()
        {

            Network.Manager.AddListener<ActivateTacticalHelmet>((msg) =>
            {
                if (msg.PlayerNum != (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
                {
                    var player = Game.Player.GetPlayer(msg.PlayerNum);
                    if (player.Helmet is TacticalHelmet helmet)
                    {
                        helmet.SwitchFlashlight(player, msg.IsUsed);
                    }
                }
            });
        }

        public bool IsUsed()
        {
            return isFlashlightBeingUsed;
        }

        public override void Start()
        {
            //if (insertedBattery == null)
            //    insertedBattery = new Battery(false, 1f);

            thisAudio = GetComponent<AudioSource>();
            wallAudio = gameObject.AddComponent<AudioSource>();
            target = transform.Find("Target").GetComponent<AudioSource>();
            foreach (var item in StartOfRound.Instance.allItemsList.itemsList)
            {
                if (item.itemName == "Walkie-talkie")
                {
                    var original = item.spawnPrefab.GetComponent<WalkieTalkie>();
                    this.talkingOnWalkieTalkieNotHeldSFX = original.talkingOnWalkieTalkieNotHeldSFX;
                    this.switchWalkieTalkiePowerOn = original.switchWalkieTalkiePowerOn;
                    this.switchWalkieTalkiePowerOff = original.switchWalkieTalkiePowerOff;
                    this.stopTransmissionSFX = original.stopTransmissionSFX;
                    this.startTransmissionSFX = original.startTransmissionSFX;
                    this.recordingRange = original.recordingRange;
                    this.playerDieOnWalkieTalkieSFX = original.playerDieOnWalkieTalkieSFX;
                    this.wallAudios = original.wallAudios;
                    this.wallAudio.dopplerLevel = original.wallAudio.dopplerLevel;
                    this.wallAudio.bypassEffects = original.wallAudio.bypassEffects;
                    this.wallAudio.bypassListenerEffects = original.wallAudio.bypassListenerEffects;
                    this.wallAudio.bypassReverbZones = original.wallAudio.bypassReverbZones;
                    this.wallAudio.ignoreListenerPause = original.wallAudio.ignoreListenerPause;
                    this.wallAudio.ignoreListenerVolume = original.wallAudio.ignoreListenerVolume;
                    this.wallAudio.loop = original.wallAudio.loop;
                    this.wallAudio.maxDistance = original.wallAudio.maxDistance;
                    this.wallAudio.minDistance = original.wallAudio.minDistance;
                    this.wallAudio.outputAudioMixerGroup = original.wallAudio.outputAudioMixerGroup;
                    this.wallAudio.volume = original.wallAudio.volume;
                    this.wallAudio.spatialBlend = original.wallAudio.spatialBlend;
                    this.wallAudio.spatialize = original.wallAudio.spatialize;
                    break;
                }
            }

            base.Start();
        }

        public override void EquipItem()
        {
            if (base.IsOwner)
            {
                HUDManager.Instance.ClearControlTips();
                SetControlTipsForItem();
            }
            EnableItemMeshes(enable: true);
            isPocketed = false;
            if (!hasBeenHeld)
            {
                hasBeenHeld = true;
                if (!isInShipRoom && !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap)
                {
                    RoundManager.Instance.valueOfFoundScrapItems += scrapValue;
                }
            }
        }

        public override void PocketItem()
        {
            if (base.IsOwner && playerHeldBy != null)
            {
                playerHeldBy.IsInspectingItem = false;
            }
            isPocketed = true;
            EnableItemMeshes(enable: false);
            base.gameObject.GetComponent<AudioSource>().PlayOneShot(itemProperties.pocketSFX, 1f);
        }

        public static void Initialize()
        {
        }

        private float Phase = 0f;
        public bool isFlashlightBeingUsed = false;
        public override void Update()
        {
            base.Update();
            if (isFlashlightBeingUsed)
            {
                insertedBattery.charge += (Time.deltaTime / ServerConfiguration.Instance.Items.TacticalHelmet.BatteryTime) - (Time.deltaTime / ServerConfiguration.Instance.Items.TacticalHelmet.BatteryTimeWithLight);

                if (Light1 != null && !Light1.enabled) Light1.enabled = true;
                if (Light2 != null && !Light2.enabled) Light2.enabled = true;
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
                    if (Light1 != null) Light1.intensity = 100f * Phase * str;
                    if (Light2 != null) Light2.intensity = 100f * Phase * str;
                    //if (Light3 != null) Light3.intensity = 100f * Phase * str;
                }
            }
            else
            {
                if (Light1 != null && Light1.enabled) Light1.enabled = false;
                if (Light2 != null && Light2.enabled) Light2.enabled = false;
                Phase = Mathf.Clamp01(Phase - Time.deltaTime * 5f);
            }
            if (Material != null)
                Material.SetFloat("_EmissiveExposureWeight", 2f - (Phase * 2f));
        }

        public override void DiscardItem()
        {
            playerHeldBy.holdingWalkieTalkie = false;
            isBeingUsed = false;
            isFlashlightBeingUsed = false;
            base.DiscardItem();
        }

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            if (IsEquipped())
                Unequip();
            else
                Equip();
        }

        public GameObject[] CreateWearable(Player player)
        {
            var helmet = GameObject.Instantiate(HelmetPrefab, player.GetBone(Player.Bone.SPINE_3));
            helmet.transform.localPosition = new Vector3(0f, 0.013f, 0.062f);
            helmet.transform.localRotation = Quaternion.Euler(-96.48f, 0f, 0f);
            helmet.transform.localScale = new Vector3(0.00794f, 0.00794f, 0.00794f);
            var lights = helmet.GetComponentsInChildren<Light>();
            Light1 = lights[0];
            Light2 = lights[1];
            var renderer = helmet.GetComponent<Renderer>();
            Material = renderer.material;
            if (player.IsLocal)
            {
                helmet.layer = 23;

                Light1.transform.parent = player.Controller.gameplayCamera.transform;
                Light2.transform.parent = player.Controller.gameplayCamera.transform;
                Light1.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                Light2.transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
                Light1.transform.localPosition = new Vector3(-0.4f, 0.3f, 0.15f);
                Light2.transform.localPosition = new Vector3(0.4f, 0.3f, 0.15f);
                return new GameObject[] { helmet, Light1.gameObject, Light2.gameObject };
            }
            else
            {
                return new GameObject[] { helmet };
            }
        }

        public override void UseUpBatteries()
        {
            isBeingUsed = false;
            isFlashlightBeingUsed = false;
            if (Light1 != null)
                Light1.enabled = false;
            if (Light2 != null)
                Light2.enabled = false;
            thisAudio.PlayOneShot(switchWalkieTalkiePowerOff);
            if (this.playerHeldBy.holdingWalkieTalkie)
                this.playerHeldBy.holdingWalkieTalkie = false;
        }

        public static void LoadAssets(AssetBundle assets)
        {
            HelmetPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/TacticalHelmet.prefab");
        }

        public bool CanBeUsed()
        {
            if (insertedBattery != null && insertedBattery.charge > 0f)
                return true;
            return false;
        }

        private bool Speaking = false;

        public void SwitchTalking(Player player, bool talking)
        {
            Speaking = talking;
            if (talking)
            {
                StartCoroutine(Speak());
            }
        }

        IEnumerator Speak()
        {
            speakingIntoWalkieTalkie = true;
            previousPlayerHeldBy.activatingItem = true;
            SetLocalClientSpeaking(true);
            yield return new WaitForSeconds(0.2f);
            yield return new WaitUntil(() => !Speaking || !isBeingUsed);
            SetLocalClientSpeaking(false);
            speakingIntoWalkieTalkie = false;
            previousPlayerHeldBy.activatingItem = false;
        }

        public virtual void Equipped(Game.Player player)
        {
            playerHeldBy.holdingWalkieTalkie = true;
            playerHeldBy = player.Controller;
            previousPlayerHeldBy = player.Controller;
            thisAudio.PlayOneShot(switchWalkieTalkiePowerOn);
            this.isBeingUsed = true;
        }

        public void Equip()
        {
            Network.Manager.Send(new ChangeItemSlot() { PlayerNum = (int) playerHeldBy.playerClientId, FromSlot = playerHeldBy.currentItemSlot, ToSlot = GetEquipmentSlot() });
        }

        public void Unequipped(Game.Player player)
        {
            try
            {
                thisAudio.PlayOneShot(switchWalkieTalkiePowerOff);
                playerHeldBy.holdingWalkieTalkie = false;
                if (this.speakingIntoWalkieTalkie)
                    SetLocalClientSpeaking(false);
                this.isBeingUsed = false;
                this.isFlashlightBeingUsed = false;
            }
            catch (Exception e) { }
        }
        public void Unequip()
        {
            if (playerHeldBy != null)
            {
                var inventorySize = Perks.InventorySlotsOf(playerHeldBy);
                var slot = -1;
                for (int i = 0; i < inventorySize; i++)
                {
                    if (playerHeldBy.ItemSlots[i] == null)
                    {
                        slot = i;
                        break;
                    }
                }
                if (slot == -1)
                    this.DiscardItem();
                else
                    Network.Manager.Send(new ChangeItemSlot() { PlayerNum = (int) playerHeldBy.playerClientId, FromSlot = playerHeldBy.currentItemSlot, ToSlot = slot });
            }
        }

        public int GetEquipmentSlot()
        {
            return 10;
        }

        public bool IsEquipped()
        {
            var slot = GetEquipmentSlot();
            if (slot == -1)
                return false;
            if (this.playerHeldBy != null)
            {
                return this.playerHeldBy.ItemSlots[slot] == this;
            }
            return false;
        }

        public void SwitchFlashlight(Player player, bool on)
        {
            if (on && insertedBattery.charge <= 0f)
                return;

            if (player.IsLocal)
            {
                Network.Manager.Send(new ActivateTacticalHelmet() { PlayerNum = player.PlayerNum, IsUsed = on });
            }

            this.isFlashlightBeingUsed = on;
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
