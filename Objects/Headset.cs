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

namespace AdvancedCompany.Objects
{
    [LoadAssets]
    [HarmonyPatch]
    internal class Headset : WalkieTalkie, IEquipmentCommunication, IHelmet
    {
        private static GameObject HeadsetPrefab;

        public virtual Game.Player.BodyLayers GetLayers() { return Game.Player.BodyLayers.NONE; }
        static Headset()
        {

            /*Network.Manager.AddListener<ActivateHelmetLamp>((msg) =>
            {
                if (msg.ClientID != GameNetworkManager.Instance.localPlayerController.playerClientId)
                {
                    var player = Game.Player.GetPlayer(msg.ClientID);
                    if (player.Helmet is HelmetLamp helmetLamp)
                    {
                        helmetLamp.SwitchFlashlight(player, msg.IsUsed);
                    }
                }
            });*/
        }

        public bool CanUnequip()
        {
            return true;
        }

        public override void Start()
        {
            //if (insertedBattery == null)
            //    insertedBattery = new Battery(false, 1f);

            thisAudio = GetComponent<AudioSource>();
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
        public override void Update()
        {
            base.Update();
            if (isBeingUsed)
            {
            }
            else
            {
            }
        }

        public override void DiscardItem()
        {
            playerHeldBy.holdingWalkieTalkie = false;
            isBeingUsed = false;
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
            var headset = GameObject.Instantiate(HeadsetPrefab, player.HeadMount.transform);
            headset.transform.localPosition = new Vector3(0f, -0.15f, 0.1433f);
            headset.transform.localRotation = Quaternion.Euler(0.053f, 0f, 0f);

            if (player.IsLocal)
                headset.layer = 23;

            return new GameObject[] { headset };
        }

        public override void UseUpBatteries()
        {
            base.UseUpBatteries();
            isBeingUsed = false;
            thisAudio.PlayOneShot(switchWalkieTalkiePowerOff);
            if (this.playerHeldBy.holdingWalkieTalkie)
                this.playerHeldBy.holdingWalkieTalkie = false;
        }

        public static void LoadAssets(AssetBundle assets)
        {
            HeadsetPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/Headset.prefab");
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
