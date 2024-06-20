using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Objects;
using AdvancedCompany.Config;
using System.Collections;
using Discord;
using AdvancedCompany.Game;
using static TMPro.SpriteAssetUtilities.TexturePacker_JsonArray;
using static UnityEngine.Rendering.VolumeComponent;
using UnityEngine.Rendering.HighDefinition;
using AdvancedCompany.Lib;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    [LoadAssets]
    [Boot.Requires(typeof(ClientConfiguration))]
    [Boot.Bootable]
    public class InventoryPatches
    {
        private static InputAction InventorySlot1Action;
        private static InputAction InventorySlot2Action;
        private static InputAction InventorySlot3Action;
        private static InputAction InventorySlot4Action;
        private static InputAction InventorySlot5Action;
        private static InputAction InventorySlot6Action;
        private static InputAction InventorySlot7Action;
        private static InputAction InventorySlot8Action;
        private static InputAction InventorySlot9Action;
        private static InputAction InventorySlot10Action;
        private static InputAction EquipmentAction;
        private static InputAction FlashlightAction;
        private static InputAction NightVisionAction;
        private static InputAction CommunicationAction;

        public const int MAX_INVENTORY = 13;
        public static Image HelmetImage;
        public static Image BodyImage;
        public static Image ShoesImage;

        public enum InventoryType
        {
            INVENTORY,
            HELMET,
            BODY,
            SHOES
        };

        public static void Boot()
        {
            ClientConfiguration.Keybinds.InventorySlot1.performed += (obj) => { ChangeToInventorySlot(0); };
//            InventorySlot1Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot2.performed += (obj) => { ChangeToInventorySlot(1); };
//            InventorySlot2Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot3.performed += (obj) => { ChangeToInventorySlot(2); };
//            InventorySlot3Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot4.performed += (obj) => { ChangeToInventorySlot(3); };
//            InventorySlot4Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot5.performed += (obj) => { ChangeToInventorySlot(4); };
//            InventorySlot5Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot6.performed += (obj) => { ChangeToInventorySlot(5); };
//            InventorySlot6Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot7.performed += (obj) => { ChangeToInventorySlot(6); };
//            InventorySlot7Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot8.performed += (obj) => { ChangeToInventorySlot(7); };
//            InventorySlot8Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot9.performed += (obj) => { ChangeToInventorySlot(8); };
//            InventorySlot9Action.Enable();
            ClientConfiguration.Keybinds.InventorySlot10.performed += (obj) => { ChangeToInventorySlot(9); };
//            InventorySlot10Action.Enable();

            //EquipmentAction = new InputAction("InventorySlot10", InputActionType.Button, ClientConfiguration.Keybinds.Equipment);
            ClientConfiguration.Keybinds.Equipment.started += (obj) => { ToggleEquipment(true); };
            ClientConfiguration.Keybinds.Equipment.canceled += (obj) => { ToggleEquipment(false); };
            //EquipmentAction.Enable();

            //ClientConfiguration.Keybinds.HeadLamp.performed += (obj) => { ToggleNightVision(); };                
            ClientConfiguration.Keybinds.Flashlight.performed += (obj) => { ToggleFlashlight(); };

            ClientConfiguration.Keybinds.Communications.started += (obj) => { ToggleComms(true); };
            ClientConfiguration.Keybinds.Communications.canceled += (obj) => { ToggleComms(false); };
            
            Network.Manager.AddListener<SwitchItem>((msg) =>
            {
                if (ServerConfiguration.Instance.General.DeactivateHotbar)
                    return;

                try
                {
                    foreach (var script in global::StartOfRound.Instance.allPlayerScripts)
                    {
                        if ((int) script.playerClientId == msg.PlayerNum)
                        {
                            script.SwitchToItemSlot(msg.Slot, null);
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError(e);
                }
            });
            Network.Manager.AddListener<ChangeItemSlot>((msg) =>
            {
                if (ServerConfiguration.Instance.General.DeactivateHotbar)
                    return;

                try
                {
                    foreach (var script in global::StartOfRound.Instance.allPlayerScripts)
                    {
                        if ((script.isPlayerControlled || script.isHostPlayerObject) && (int) script.playerClientId == msg.PlayerNum)
                        {
                            if (script.ItemSlots[msg.FromSlot] != null)
                            {
                                var fromSlot = script.ItemSlots[msg.FromSlot];
                                var toSlot = script.ItemSlots[msg.ToSlot];

                                if (script == global::GameNetworkManager.Instance.localPlayerController)
                                {
                                    var toSprite = global::HUDManager.Instance.itemSlotIcons[msg.ToSlot].sprite;
                                    var toEnabled = global::HUDManager.Instance.itemSlotIcons[msg.ToSlot].enabled;
                                    global::HUDManager.Instance.itemSlotIcons[msg.ToSlot].sprite = global::HUDManager.Instance.itemSlotIcons[msg.FromSlot].sprite;
                                    global::HUDManager.Instance.itemSlotIcons[msg.ToSlot].enabled = global::HUDManager.Instance.itemSlotIcons[msg.FromSlot].enabled;
                                    global::HUDManager.Instance.itemSlotIcons[msg.FromSlot].sprite = toSprite;
                                    global::HUDManager.Instance.itemSlotIcons[msg.FromSlot].enabled = toEnabled;
                                }

                                script.ItemSlots[msg.FromSlot] = toSlot;
                                script.ItemSlots[msg.ToSlot] = fromSlot;

                                SwitchToItemSlotMethod.Invoke(script, new object[] { script.currentItemSlot, null });
                            }
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError(e);
                }
            });

            Network.Manager.AddListener<UseFlashlight>((msg) =>
            {
                if (ServerConfiguration.Instance.General.DeactivateHotbar)
                    return;

                var player = Game.Player.GetPlayer(msg.PlayerNum);
                if (player != null && !player.Controller.inSpecialInteractAnimation)
                {
                    var flashlightObj = player.Controller.ItemSlots[msg.Slot];
                    if (flashlightObj != null && flashlightObj is FlashlightItem flashlight)
                    {
                        flashlight.flashlightAudio.PlayOneShot(flashlight.flashlightClips[UnityEngine.Random.Range(0, flashlight.flashlightClips.Length)]);
                        global::RoundManager.Instance.PlayAudibleNoise(flashlight.transform.position, 7f, 0.4f, 0, flashlight.isInElevator && global::StartOfRound.Instance.hangarDoorsClosed);

                        if (flashlight.flashlightInterferenceLevel < 2)
                        {
                            // was off, turning on. Deactivate all other flashlights
                            if (!flashlight.isBeingUsed)
                            {
                                for (var i = 0; i < player.Controller.ItemSlots.Length; i++)
                                {
                                    if (i != msg.Slot && player.Controller.ItemSlots[i] != null && player.Controller.ItemSlots[i] is FlashlightItem otherFlashlight)
                                    {
                                        if (otherFlashlight.IsOwner)
                                        {
                                            if (otherFlashlight.playerHeldBy != null)
                                                otherFlashlight.playerHeldBy.ChangeHelmetLight(otherFlashlight.flashlightTypeID, false);
                                        }
                                        if (otherFlashlight.isBeingUsed)
                                        {
                                            otherFlashlight.isBeingUsed = false;
                                            /*otherFlashlight.flashlightBulb.enabled = false;
                                            otherFlashlight.flashlightBulbGlow.enabled = false;
                                            if (otherFlashlight.usingPlayerHelmetLight && otherFlashlight.playerHeldBy != null)
                                                otherFlashlight.playerHeldBy.helmetLight.enabled = false;
                                            if (otherFlashlight.changeMaterial)
                                            {
                                                Material[] sharedMaterials = otherFlashlight.flashlightMesh.sharedMaterials;
                                                sharedMaterials[1] = otherFlashlight.bulbDark;
                                                otherFlashlight.flashlightMesh.sharedMaterials = sharedMaterials;
                                            }*/
                                        }
                                        UpdateFlashlightState(otherFlashlight, !flashlight.isPocketed);
                                    }
                                }
                            }

                            if (!flashlight.isBeingUsed && flashlight.insertedBattery != null && flashlight.insertedBattery.charge > 0f)
                                flashlight.isBeingUsed = true;
                            else
                                flashlight.isBeingUsed = false;

                            UpdateFlashlightState(flashlight, player.Controller.currentItemSlot == msg.Slot);
                        }
                    }
                }
            });
        }

        internal static bool InEquipmentSlots = false;
        internal static int PreviousInventorySlot = 0;
        internal static float EquipmentPhase = 0f;

        internal static void ToggleEquipment(bool active)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (global::StartOfRound.Instance == null || global::StartOfRound.Instance.localPlayerController == null)
                return;
            var player = global::StartOfRound.Instance.localPlayerController;
            if (active && (HeadSlotAvailable || BodySlotAvailable || FeetSlotAvailable))
            {
                PreviousInventorySlot = player.currentItemSlot;
                if (HeadSlotAvailable)
                    SwitchItem(player, 10);
                else if (BodySlotAvailable)
                    SwitchItem(player, 11);
                else if (FeetSlotAvailable)
                    SwitchItem(player, 12);
                InEquipmentSlots = true;
            }
            else if (InEquipmentSlots)
            {
                SwitchItem(player, PreviousInventorySlot);
                InEquipmentSlots = false;
            }
        }

        internal static void UpdateFlashlightState(FlashlightItem flashlight, bool isHeld)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            var player = Game.Player.GetPlayer(flashlight.playerHeldBy);
            if (player == null)
                return;
            if (isHeld)
            {
                // held
                flashlight.previousPlayerHeldBy = flashlight.playerHeldBy;
                flashlight.playerHeldBy.ChangeHelmetLight(flashlight.flashlightTypeID);
                flashlight.playerHeldBy.helmetLight.enabled = false;
                flashlight.usingPlayerHelmetLight = false;
                if (!flashlight.IsOwner)
                {
                    flashlight.playerHeldBy.ChangeHelmetLight(flashlight.flashlightTypeID, flashlight.isBeingUsed);
                    flashlight.flashlightBulb.enabled = false;
                    flashlight.flashlightBulbGlow.enabled = false;
                }
                else
                {
                    flashlight.flashlightBulb.enabled = flashlight.isBeingUsed;
                    flashlight.flashlightBulbGlow.enabled = flashlight.isBeingUsed;
                }

                if (flashlight.changeMaterial)
                {
                    Material[] sharedMaterials = flashlight.flashlightMesh.sharedMaterials;
                    if (flashlight.isBeingUsed)
                        sharedMaterials[1] = flashlight.bulbLight;
                    else
                        sharedMaterials[1] = flashlight.bulbDark;
                    flashlight.flashlightMesh.sharedMaterials = sharedMaterials;
                }
                //flashlight.isPocketed = false;
                //flashlight.EnableItemMeshes(true);
            }
            else
            {
                // pocketed
                flashlight.flashlightBulb.enabled = false;
                flashlight.flashlightBulbGlow.enabled = false;

                flashlight.playerHeldBy.ChangeHelmetLight(flashlight.flashlightTypeID);
                if (!flashlight.IsOwner)
                {
                    flashlight.playerHeldBy.ChangeHelmetLight(flashlight.flashlightTypeID, flashlight.isBeingUsed);
                }
                else
                {
                    if (flashlight.isBeingUsed)
                    {
                        flashlight.playerHeldBy.helmetLight.enabled = true;
                        flashlight.playerHeldBy.pocketedFlashlight = flashlight;
                        flashlight.usingPlayerHelmetLight = true;
                        //flashlight.PocketFlashlightServerRpc(true);
                    }
                    else
                    {
                        flashlight.usingPlayerHelmetLight = false;
                        flashlight.playerHeldBy.helmetLight.enabled = false;
                        if (flashlight.changeMaterial)
                        {
                            Material[] sharedMaterials = flashlight.flashlightMesh.sharedMaterials;
                            sharedMaterials[1] = flashlight.bulbDark;
                            flashlight.flashlightMesh.sharedMaterials = sharedMaterials;
                        }
                        //flashlight.PocketFlashlightServerRpc(false);
                    }
                }
                //flashlight.isPocketed = true;
                //flashlight.EnableItemMeshes(false);
            }

            flashlight.SwitchFlashlight(flashlight.isBeingUsed);
        }

        public static void ChangeToInventorySlot(int slot)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (global::GameNetworkManager.Instance != null && global::GameNetworkManager.Instance.localPlayerController != null)
            {
                var controller = global::GameNetworkManager.Instance.localPlayerController;
                var player = Game.Player.GetPlayer(controller);
                if (player.LockInventory)
                    return;
                if (!controller.inTerminalMenu && !controller.isPlayerDead)
                {
                    var max = Perks.InventorySlotsOf(controller);
                    if (slot < max)
                        SwitchItem(controller, slot);
                }
            }
        }

        public static void ToggleComms(bool on)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (global::GameNetworkManager.Instance != null && global::GameNetworkManager.Instance.localPlayerController != null)
            {
                var controller = global::GameNetworkManager.Instance.localPlayerController;

                if (!controller.inTerminalMenu && !controller.isPlayerDead)
                {
                    IEquipmentCommunication equipmentComms = null;
                    if (controller.ItemSlots[10] is IEquipmentCommunication commsEquipments1)
                        equipmentComms = commsEquipments1;
                    if (controller.ItemSlots[11] is IEquipmentCommunication commsEquipments2)
                        equipmentComms = commsEquipments2;
                    if (controller.ItemSlots[12] is IEquipmentCommunication commsEquipments3)
                        equipmentComms = commsEquipments3;

                    if (equipmentComms != null)
                    {
                        equipmentComms.SwitchTalking(Game.Player.GetPlayer(controller), on);
                    }
                }
            }

        }
        public static void ToggleNightVision()
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (global::GameNetworkManager.Instance != null && global::GameNetworkManager.Instance.localPlayerController != null)
            {
                var controller = global::GameNetworkManager.Instance.localPlayerController;

                if (!controller.inTerminalMenu && !controller.isPlayerDead)
                {
                    IEquipmentFlashlight equipmentFlashlight = null;
                    if (controller.ItemSlots[10] is IEquipmentFlashlight flashlight1)
                        equipmentFlashlight = flashlight1;
                    if (controller.ItemSlots[11] is IEquipmentFlashlight flashlight2)
                        equipmentFlashlight = flashlight2;
                    if (controller.ItemSlots[12] is IEquipmentFlashlight flashlight3)
                        equipmentFlashlight = flashlight3;

                    if (equipmentFlashlight != null)
                    {
                        equipmentFlashlight.SwitchFlashlight(Game.Player.GetPlayer(controller), !equipmentFlashlight.IsUsed());
                    }
                }
            }
        }

        public static void ToggleFlashlight(bool ignoreHead = false)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (global::GameNetworkManager.Instance != null && global::GameNetworkManager.Instance.localPlayerController != null)
            {
                var controller = global::GameNetworkManager.Instance.localPlayerController;

                if (!controller.inTerminalMenu && !controller.isPlayerDead)
                {
                    global::FlashlightItem firstUnused = null;
                    float firstCharge = 0f;
                    var isPro = false;
                    IEquipmentFlashlight equipmentFlashlight = null;
                    if (!ignoreHead)
                    {
                        if (controller.ItemSlots[10] is IEquipmentFlashlight flashlight1 && flashlight1.CanBeUsed())
                            equipmentFlashlight = flashlight1;
                        if (controller.ItemSlots[11] is IEquipmentFlashlight flashlight2 && flashlight2.CanBeUsed())
                            equipmentFlashlight = flashlight2;
                        if (controller.ItemSlots[12] is IEquipmentFlashlight flashlight3 && flashlight3.CanBeUsed())
                            equipmentFlashlight = flashlight3;
                    }

                    if (equipmentFlashlight != null)
                    {
                        equipmentFlashlight.SwitchFlashlight(Game.Player.GetPlayer(controller), !equipmentFlashlight.IsUsed());

                        for (var i = 0; i < 7; i++)
                        {
                            if (controller.ItemSlots[i] is global::FlashlightItem flashlight)
                            {
                                if (flashlight.isBeingUsed)
                                    UseFlashlight(controller, flashlight);
                            }
                        }
                    }
                    else
                    {
                        for (var i = 0; i < controller.ItemSlots.Length; i++)
                        {
                            if (controller.ItemSlots[i] is global::FlashlightItem flashlight)
                            {
                                var pro = flashlight.itemProperties.itemName.StartsWith("pro", StringComparison.OrdinalIgnoreCase);
                                var normal = flashlight.itemProperties.itemName.StartsWith("flash", StringComparison.OrdinalIgnoreCase);
                                if (pro || normal)
                                {
                                    if (flashlight.isBeingUsed)
                                    {
                                        UseFlashlight(controller, flashlight);
                                        firstUnused = null;
                                        break;
                                    }
                                    else
                                    {
                                        if (flashlight.insertedBattery != null && flashlight.insertedBattery.charge > 0f && (flashlight.insertedBattery.charge > firstCharge || (pro && !isPro)))
                                        {
                                            firstUnused = flashlight;
                                            isPro = pro;
                                            firstCharge = flashlight.insertedBattery.charge;
                                        }
                                    }
                                }
                            }
                        }
                        if (firstUnused != null)
                            UseFlashlight(controller, firstUnused);
                    }
                }
            }
        }

        private static FieldInfo IsSendingItemRPCField = typeof(global::GrabbableObject).GetField("isSendingItemRPC", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        private static MethodInfo ActivateItemServerRpcMethod = typeof(global::GrabbableObject).GetMethod("ActivateItemServerRpc", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        private static void UseFlashlightOnClient(FlashlightItem flashlight, bool buttonDown)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (!flashlight.IsOwner)
            {
                Debug.Log("Can't use item; not owner");
            }
            else if (!flashlight.RequireCooldown() && flashlight.UseItemBatteries(false, true))
            {
                if (flashlight.itemProperties.syncUseFunction)
                {
                    IsSendingItemRPCField.SetValue(flashlight, ((int)IsSendingItemRPCField.GetValue(flashlight)) + 1);
                    ActivateItemServerRpcMethod.Invoke(flashlight, new object[] { flashlight.isBeingUsed, buttonDown });
                }
                flashlight.ItemActivate(flashlight.isBeingUsed, buttonDown);
            }
        }

        [HarmonyPatch(typeof(global::GrabbableObject), "UseItemOnClient")]
        [HarmonyPrefix]
        public static bool UseItemOnClient(global::GrabbableObject __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;

            if (__instance is FlashlightItem flashlight)
            {
                int inventorySlot = -1;
                if (flashlight.insertedBattery != null && flashlight.insertedBattery.charge > 0f)
                {
                    for (var i = 0; i < __instance.playerHeldBy.ItemSlots.Length; i++)
                    {
                        if (__instance.playerHeldBy.ItemSlots[i] == __instance)
                            inventorySlot = i;
                    }
                    if (inventorySlot > -1)
                    {
                        Network.Manager.Send(new UseFlashlight() { PlayerNum = (int) __instance.playerHeldBy.playerClientId, Slot = inventorySlot });
                        return false;
                    }
                }
            }
            return true;
        }

        [HarmonyPatch(typeof(FlashlightItem), "SwitchFlashlight")]
        [HarmonyPrefix]
        public static bool SwitchFlashlight(bool on)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;
            return false;
        }
        [HarmonyPatch(typeof(FlashlightItem), "UseUpBatteries")]
        [HarmonyPrefix]
        public static void UseUpBatteries(FlashlightItem __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;
            if (__instance.playerHeldBy != null && __instance.playerHeldBy == global::StartOfRound.Instance.localPlayerController)
            {
                Network.Manager.Send(new UseFlashlight() { PlayerNum = (int) __instance.playerHeldBy.playerClientId, Slot = GrabbableObjectAdditions.GetInventoryPosition(__instance) });
                //UpdateFlashlightState(__instance, __instance.isHeld);
            }
        }

        [HarmonyPatch(typeof(FlashlightItem), "DiscardItem")]
        [HarmonyPrefix]
        public static bool DiscardItem(global::FlashlightItem __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;
            if (__instance.previousPlayerHeldBy != null)
            {
                if (__instance.isBeingUsed)
                    __instance.previousPlayerHeldBy.helmetLight.enabled = false;
                __instance.flashlightBulb.enabled = __instance.isBeingUsed;
                __instance.flashlightBulbGlow.enabled = __instance.isBeingUsed;
            }
            var ftn = AccessTools.Method(typeof(global::GrabbableObject), "DiscardItem").MethodHandle.GetFunctionPointer();
            ((Action)Activator.CreateInstance(typeof(Action), __instance, ftn))();
            return false;
        }

        private static FieldInfo FlashlightPreviousPlayerHeldBy = typeof(FlashlightItem).GetField("previousPlayerHeldBy", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);
        [HarmonyPatch(typeof(FlashlightItem), "EquipItem")]
        [HarmonyPrefix]
        public static bool FlashlightEquipItem(FlashlightItem __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;
            FlashlightPreviousPlayerHeldBy.SetValue(__instance, __instance.playerHeldBy);

            var controller = __instance.playerHeldBy;
            bool otherFlashlight = false;
            for (var i = 0; i < controller.ItemSlots.Length; i++)
            {
                if (controller.ItemSlots[i] is FlashlightItem fl && fl != __instance && fl.isBeingUsed)
                {
                    otherFlashlight = true;
                    __instance.isBeingUsed = false;
                    __instance.flashlightBulb.enabled = false;
                    __instance.flashlightBulbGlow.enabled = false;
                    if (__instance.changeMaterial)
                    {
                        Material[] sharedMaterials = __instance.flashlightMesh.sharedMaterials;
                        sharedMaterials[1] = __instance.bulbDark;
                        __instance.flashlightMesh.sharedMaterials = sharedMaterials;
                    }
                    break;
                }
            }

            if (!otherFlashlight)
            {
                __instance.usingPlayerHelmetLight = false;
                __instance.playerHeldBy.helmetLight.enabled = false;
                if (__instance.isBeingUsed)
                {
                    UpdateFlashlightState(__instance, true);
                    //__instance.SwitchFlashlight(true);
                }
            }

            var ftn = AccessTools.Method(typeof(global::GrabbableObject), "EquipItem").MethodHandle.GetFunctionPointer();
            ((Action)Activator.CreateInstance(typeof(Action), __instance, ftn))();
            return false;
        }

        [HarmonyPatch(typeof(FlashlightItem), "PocketItem")]
        [HarmonyPrefix]
        public static bool FlashlightPocketItem(FlashlightItem __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;
            if (__instance.IsOwner)
            {
                GameNetcodeStuff.PlayerControllerB previousPlayerHeldBy = (GameNetcodeStuff.PlayerControllerB)FlashlightPreviousPlayerHeldBy.GetValue(__instance);
                __instance.flashlightBulb.enabled = false;
                __instance.flashlightBulbGlow.enabled = false;
                if (__instance.isBeingUsed)
                {
                    previousPlayerHeldBy.helmetLight.enabled = true;
                    previousPlayerHeldBy.pocketedFlashlight = __instance;
                    __instance.usingPlayerHelmetLight = true;
                    __instance.PocketFlashlightServerRpc(true);
                }
            }

            var ftn = AccessTools.Method(typeof(global::GrabbableObject), "PocketItem").MethodHandle.GetFunctionPointer();
            ((Action)Activator.CreateInstance(typeof(Action), __instance, ftn))();
            return false;
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "OnEnable")]
        [HarmonyPostfix]
        public static void OnEnable(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;
            try
            {
                if (__instance.ItemSlots.Length < MAX_INVENTORY)
                {
                    var arr = __instance.ItemSlots;
                    var newArr = new global::GrabbableObject[MAX_INVENTORY];
                    Array.Copy(arr, newArr, arr.Length);
                    __instance.ItemSlots = newArr;
                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while changing inventory size");
                Plugin.Log.LogError(e);
            }
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "Update")]
        [HarmonyPostfix]
        public static void UpdatePre(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;
            try
            {
                if (global::GameNetworkManager.Instance.localPlayerController == __instance)
                {

                }
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
        }

        private static void UseFlashlight(GameNetcodeStuff.PlayerControllerB __instance, global::FlashlightItem flashlight)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;
            flashlight.UseItemOnClient(true);
            /*
            var pocketed = flashlight.isPocketed;
            var on = !flashlight.isBeingUsed;

            flashlight.isBeingUsed = on;
            __instance.ChangeHelmetLight(flashlight.flashlightTypeID);
            flashlight.flashlightAudio.PlayOneShot(flashlight.flashlightClips[UnityEngine.Random.Range(0, flashlight.flashlightClips.Length)]);
            global::RoundManager.Instance.PlayAudibleNoise(flashlight.transform.position, 7f, 0.4f, 0, flashlight.isInElevator && global::StartOfRound.Instance.hangarDoorsClosed);

            if (pocketed)
            {
                flashlight.flashlightBulb.enabled = false;
                flashlight.flashlightBulbGlow.enabled = false;
                flashlight.usingPlayerHelmetLight = on;
                __instance.pocketedFlashlight = flashlight;
                __instance.helmetLight.enabled = on;
                flashlight.PocketFlashlightServerRpc(on);
            }
            else
            {
                flashlight.flashlightBulb.enabled = on;
                flashlight.flashlightBulbGlow.enabled = on;
                flashlight.usingPlayerHelmetLight = false;
                __instance.helmetLight.enabled = false;
                if (flashlight.changeMaterial)
                {
                    Material[] sharedMaterials = flashlight.flashlightMesh.sharedMaterials;
                    if (on)
                        sharedMaterials[1] = flashlight.bulbLight;
                    else
                        sharedMaterials[1] = flashlight.bulbDark;
                    flashlight.flashlightMesh.sharedMaterials = sharedMaterials;
                }
            }*/
        }

        private static int OwnItemSlotResult;
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "NextItemSlot")]
        [HarmonyPrefix]
        public static bool NextItemSlot(GameNetcodeStuff.PlayerControllerB __instance, ref int __result, ref bool forward)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;
            var inventorySize = Perks.InventorySlotsOf(__instance);
            var wantedSlot = __instance.currentItemSlot;

            if (InEquipmentSlots)
            {
                if (forward)
                    wantedSlot--;
                else
                    wantedSlot++;
                if (wantedSlot < 10)
                    wantedSlot = 12;
                if (wantedSlot > 12)
                    wantedSlot = 10;

                var isSlotAvailable = (wantedSlot == 10 && HeadSlotAvailable) || (wantedSlot == 11 && BodySlotAvailable) || (wantedSlot == 12 && FeetSlotAvailable);
                if (!isSlotAvailable)
                {
                    if (!forward)
                    {
                        if (wantedSlot == 10 && !HeadSlotAvailable)
                            wantedSlot++;
                        if (wantedSlot == 11 && !BodySlotAvailable)
                            wantedSlot++;
                        if (wantedSlot == 12 && !FeetSlotAvailable)
                            wantedSlot = HeadSlotAvailable ? 10 : 11;
                    }
                    else
                    {
                        if (wantedSlot == 12 && !FeetSlotAvailable)
                            wantedSlot--;
                        if (wantedSlot == 11 && !BodySlotAvailable)
                            wantedSlot--;
                        if (wantedSlot == 10 && !HeadSlotAvailable)
                            wantedSlot = FeetSlotAvailable ? 12 : 11;
                    }
                }
            }
            else
            {
                if (forward)
                    wantedSlot++;
                else
                    wantedSlot--;
                if (wantedSlot >= inventorySize)
                    wantedSlot = 0;
                else if (wantedSlot < 0)
                    wantedSlot = inventorySize - 1;
            }

            __result = wantedSlot;
            return false;
        }
        public static FieldInfo TimeSinceSwitchingSlotsField = AccessTools.Field(typeof(GameNetcodeStuff.PlayerControllerB), "timeSinceSwitchingSlots");
        public static FieldInfo ThrowingObjectField = AccessTools.Field(typeof(GameNetcodeStuff.PlayerControllerB), "throwingObject");
        public static MethodInfo NextItemMethod = AccessTools.Method(typeof(GameNetcodeStuff.PlayerControllerB), "NextItemSlot");
        public static MethodInfo SwitchToItemSlotMethod = AccessTools.Method(typeof(GameNetcodeStuff.PlayerControllerB), "SwitchToItemSlot");

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "ScrollMouse_performed")]
        [HarmonyBefore(new string[] { "ReservedItemSlotCore" })]
        [HarmonyPrefix]
        public static bool ScrollMouse_performed(GameNetcodeStuff.PlayerControllerB __instance, InputAction.CallbackContext context)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;
            try
            {
                if (BepInEx.Bootstrap.Chainloader.PluginInfos.ContainsKey("FlipMods.ReservedItemSlotCore") && __instance.currentItemSlot >= MAX_INVENTORY)
                    return true;
                if (!context.performed)
                    return false;

                var player = Game.Player.GetPlayer(__instance);
                if (player.LockInventory)
                    return false;

                if (__instance.inTerminalMenu)
                {
                    if (player.MobileTerminal != null && player.MobileTerminal.IsOpen)
                    {
                        float num = context.ReadValue<float>();
                        var delta = (num * 20f) / player.MobileTerminal.ScrollRect.content.sizeDelta.y;
                        player.MobileTerminal.ScrollRect.verticalNormalizedPosition += delta;
                    }
                    else
                    {
                        float num = context.ReadValue<float>();
                        __instance.terminalScrollVertical.value += num / 8f;
                    }
                }
                else if (__instance.timeSinceSwitchingSlots >= 0.1f)
                {
                    var slot = __instance.currentItemSlot;
                    if (context.ReadValue<float>() > 0f)
                    {
                        slot = (int)NextItemMethod.Invoke(__instance, new object[] { ClientConfiguration.Instance.Hotbar.InvertScroll ? false : true });
                    }
                    else if (context.ReadValue<float>() < 0f)
                    {
                        slot = (int)NextItemMethod.Invoke(__instance, new object[] { ClientConfiguration.Instance.Hotbar.InvertScroll ? true : false });
                    }
                    SwitchItem(__instance, slot);
                }
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
            return true;
        }


        public static bool SwitchItem(GameNetcodeStuff.PlayerControllerB __instance, int slot)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;
            try
            {
                var player = Game.Player.GetPlayer(__instance);
                if (player.LockInventory)
                    return false;

                if (((__instance.IsOwner && __instance.isPlayerControlled &&
                    (!__instance.IsServer || __instance.isHostPlayerObject)) || __instance.isTestingPlayer) &&
                    !((float)TimeSinceSwitchingSlotsField.GetValue(__instance) < 0.1f) && !__instance.isGrabbingObjectAnimation && !__instance.quickMenuManager.isMenuOpen &&
                    !__instance.inSpecialInteractAnimation && !((bool)ThrowingObjectField.GetValue(__instance)) && !__instance.isTypingChat && !__instance.twoHanded &&
                    !__instance.activatingItem && !__instance.jetpackControls && !__instance.disablingJetpackControls)
                {
                    ShipBuildModeManager.Instance.CancelBuildMode();
                    __instance.playerBodyAnimator.SetBool("GrabValidated", value: false);

                    //SwitchItemSlotsServerRpc(forward: true);

                    TimeSinceSwitchingSlotsField.SetValue(__instance, 0f);

                    Network.Manager.Send(new SwitchItem() { PlayerNum = (int) __instance.playerClientId, Slot = slot });
                    if (__instance.currentlyHeldObjectServer != null)
                    {
                        __instance.currentlyHeldObjectServer.gameObject.GetComponent<AudioSource>().PlayOneShot(__instance.currentlyHeldObjectServer.itemProperties.grabSFX, 0.6f);
                    }

                }
                return false;
            }
            catch (Exception e)
            {
                Plugin.Log.LogError(e);
            }
            return true;
        }

        private static GameObject EnergyPrefab;
        private static AnimationClip EquipmentFrameAnimation;
        private static AnimationClip EquipmentFrameEnlargeAnimation;
        private static Sprite HelmetIcon;
        private static Sprite BodyIcon;
        private static Sprite ShoesIcon;

        public static void LoadAssets(AssetBundle assets)
        {
            EnergyPrefab = assets.LoadAsset<GameObject>("Assets/Prefabs/UI/Energy.prefab");
            EquipmentFrameAnimation = assets.LoadAsset<AnimationClip>("Assets/Icons/EquipmentSlot.anim");
            EquipmentFrameEnlargeAnimation = assets.LoadAsset<AnimationClip>("Assets/Icons/EquipmentSlotEnlarge.anim");

            HelmetIcon = assets.LoadAsset<Sprite>("Assets/Icons/Helmet.png");
            BodyIcon = assets.LoadAsset<Sprite>("Assets/Icons/Body.png");
            ShoesIcon = assets.LoadAsset<Sprite>("Assets/Icons/Shoes.png");
        }

        private static GameObject[] EnergyBars = new GameObject[MAX_INVENTORY];
        private static RectTransform[] EnergyInnerBars = new RectTransform[MAX_INVENTORY];
        
        [HarmonyPatch(typeof(global::HUDManager), "OnEnable")]
        [HarmonyPostfix]
        public static void InitializeHotbar(global::HUDManager __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;
            if (__instance.itemSlotIcons.Length < MAX_INVENTORY)
            {
                var newIcons = new Image[MAX_INVENTORY];
                var newFrames = new Image[MAX_INVENTORY];
                Array.Copy(__instance.itemSlotIcons, newIcons, __instance.itemSlotIcons.Length);
                Array.Copy(__instance.itemSlotIconFrames, newFrames, __instance.itemSlotIconFrames.Length);
                __instance.itemSlotIcons = newIcons;
                __instance.itemSlotIconFrames = newFrames;

                var equipmentSlots = new GameObject("EquipmentSlots");
                equipmentSlots.transform.parent = newFrames[0].transform.parent;
                equipmentSlots.transform.localScale = Vector3.one;

                var equipmentRect = equipmentSlots.AddComponent<RectTransform>();
                equipmentRect.anchorMax = new Vector3(1f, 1f);
                equipmentRect.anchorMin = new Vector3(1f, 0f);
                equipmentRect.pivot = new Vector3(1f, 0f);
                equipmentRect.offsetMin = new Vector2(0f, 0f);
                equipmentRect.offsetMax = new Vector2(0f, 0f);
                equipmentRect.anchoredPosition3D = new Vector3(-10f, 0f, 0f);// -18.5f, 0f);
                EquipmentSlots = equipmentSlots.transform;

                EquipmentGridLayout = equipmentSlots.AddComponent<UnityEngine.UI.GridLayoutGroup>();
                EquipmentGridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                EquipmentGridLayout.constraintCount = 1;
                EquipmentGridLayout.childAlignment = TextAnchor.LowerRight;
                EquipmentGridLayout.startAxis = GridLayoutGroup.Axis.Vertical;
                EquipmentGridLayout.startCorner = GridLayoutGroup.Corner.LowerLeft;
                EquipmentGridLayout.cellSize = new Vector2(80f, 80f);
                EquipmentGridLayout.spacing = new Vector2(0f, 10f);
                equipmentSlots.AddComponent<LayoutElement>().ignoreLayout = true;

                newFrames[0].transform.parent.gameObject.SetActive(false);
                float yPosition = newFrames[0].GetComponent<RectTransform>().anchoredPosition.y;

                for (int i = 0; i < MAX_INVENTORY; i++)
                {
                    if (newFrames[i] == null)
                    {
                        newFrames[i] = GameObject.Instantiate(newFrames[0].gameObject, i >= MAX_INVENTORY - 3 ? EquipmentSlots : newFrames[0].transform.parent).GetComponent<Image>();
                        newIcons[i] = newFrames[i].transform.GetChild(0).GetComponent<Image>();
                        //newFrames[i].transform.SetSiblingIndex(newFrames[i - 1].transform.GetSiblingIndex() + 1);
                    }
                    newFrames[i].name = "Slot" + i;
                    newIcons[i].name = "Icon";

                    newIcons[i].rectTransform.anchorMin = new Vector2(0f, 0f);
                    newIcons[i].rectTransform.anchorMax = new Vector2(1f, 1f);

                    newFrames[i].rectTransform.rotation = Quaternion.identity;
                    newFrames[i].rectTransform.anchorMax = new Vector2(i >= MAX_INVENTORY - 3 ? 1f : 0f, 0f);
                    newFrames[i].rectTransform.anchorMin = new Vector2(i >= MAX_INVENTORY - 3 ? 1f : 0f, 0f);
                    newFrames[i].transform.GetChild(0).rotation = Quaternion.identity;
                    newFrames[i].rectTransform.anchoredPosition = new Vector2(0f, 0f);
                }
                InitializeHotbarStepTwo();
            }
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "ConnectClientToPlayerObject")]
        [HarmonyPostfix]
        public static void OnLocalPlayerConnect(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            __instance.StartCoroutine(ArrangeHotbarLate());
        }

        private static IEnumerator ArrangeHotbarLate()
        {
            ArrangeHotbar(Perks.InventorySlots());
            yield return new WaitForEndOfFrame();
            ArrangeHotbar(Perks.InventorySlots());
        }

        private static GameObject ScaleCompensator;
        private static Transform Inventory;
        private static Transform EquipmentSlots;
        private static UnityEngine.UI.GridLayoutGroup InventoryGridLayout;
        private static UnityEngine.UI.GridLayoutGroup EquipmentGridLayout;

        private static void InitializeHotbarStepTwo()
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            var __instance = global::HUDManager.Instance;
            var newIcons = __instance.itemSlotIcons;
            var newFrames = __instance.itemSlotIconFrames;
            float yPosition = 0f;
            
            Inventory = newFrames[0].transform.parent;
            Inventory.gameObject.SetActive(true);
            InventoryGridLayout = Inventory.gameObject.AddComponent<UnityEngine.UI.GridLayoutGroup>();
            InventoryGridLayout.constraint = GridLayoutGroup.Constraint.FixedRowCount;
            InventoryGridLayout.constraintCount = 1;
            InventoryGridLayout.childAlignment = TextAnchor.LowerCenter;
            InventoryGridLayout.cellSize = new Vector2(40f, 40f);
            InventoryGridLayout.spacing = new Vector2(5f, 0f);
            Inventory.localScale = Vector3.one;
            //ScaleCompensator = new GameObject("Scaler");
            //ScaleCompensator.transform.parent = Inventory;
            /*var rect = ScaleCompensator.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.localScale = new Vector3(1f, 1f, 1f);
            */
            
            for (int i = 0; i < MAX_INVENTORY; i++)
            {
                if (i < MAX_INVENTORY && i >= MAX_INVENTORY - 3)
                {
                    newIcons[i].transform.localPosition = new Vector2(0f, -3.2f);
                    var animator = newFrames[i].GetComponent<Animator>();
                    if (!(animator.runtimeAnimatorController is AnimatorOverrideController))
                    {
                        var @override = new AnimatorOverrideController(animator.runtimeAnimatorController);
                        var overrides = new AnimationClipOverrides(50);
                        animator.runtimeAnimatorController = @override;
                        @override.GetOverrides(overrides);
                        overrides["PanelLines"] = EquipmentFrameAnimation;
                        overrides["PanelEnlarge"] = EquipmentFrameEnlargeAnimation;
                        @override.ApplyOverrides(overrides);
                    }

                    newFrames[i].color = new Color(0.4f, 0.4f, 0.4f);
                    if (i == 10)
                    {
                        GameObject helmet = new GameObject("helmet");
                        helmet.transform.parent = newFrames[i].transform;
                        helmet.transform.localScale = new Vector3(1f, 1f, 1f);
                        helmet.transform.localPosition = new Vector3(0f, 0f, 0f);

                        HelmetImage = helmet.AddComponent<Image>();
                        HelmetImage.rectTransform.sizeDelta = new Vector2(16f, 16f);
                        HelmetImage.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                        HelmetImage.rectTransform.anchorMin = new Vector2(0.5f, 1f);
                        HelmetImage.rectTransform.pivot = new Vector2(0.5f, 1f);
                        HelmetImage.rectTransform.anchoredPosition = new Vector2(0f, -4f);
                        HelmetImage.sprite = HelmetIcon;
                    }
                    if (i == 11)
                    {
                        GameObject body = new GameObject("body");
                        body.transform.parent = newFrames[i].transform;
                        body.transform.localScale = new Vector3(1f, 1f, 1f);
                        body.transform.localPosition = new Vector3(0f, 0f, 0f);

                        BodyImage = body.AddComponent<Image>();
                        BodyImage.rectTransform.sizeDelta = new Vector2(16f, 16f);
                        BodyImage.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                        BodyImage.rectTransform.anchorMin = new Vector2(0.5f, 1f);
                        BodyImage.rectTransform.pivot = new Vector2(0.5f, 1f);
                        BodyImage.rectTransform.anchoredPosition = new Vector2(0f, -4f);
                        BodyImage.sprite = BodyIcon;
                    }
                    if (i == 12)
                    {
                        GameObject shoes = new GameObject("shoes");
                        shoes.transform.parent = newFrames[i].transform;
                        shoes.transform.localScale = new Vector3(1f, 1f, 1f);
                        shoes.transform.localPosition = new Vector3(0f, 0f, 0f);

                        ShoesImage = shoes.AddComponent<Image>();
                        ShoesImage.rectTransform.sizeDelta = new Vector2(16f, 16f);
                        ShoesImage.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                        ShoesImage.rectTransform.anchorMin = new Vector2(0.5f, 1f);
                        ShoesImage.rectTransform.pivot = new Vector2(0.5f, 1f);
                        ShoesImage.rectTransform.anchoredPosition = new Vector2(0f, -4f);
                        ShoesImage.sprite = ShoesIcon;
                    }
                }
                else
                    newFrames[i].rectTransform.anchoredPosition = new Vector2(newFrames[i].rectTransform.anchoredPosition.x, 0f);
                //newFrames[i].transform.parent = ScaleCompensator.transform;
            }
            for (int i = 0; i < MAX_INVENTORY; i++)
            {
                if (EnergyBars[i] == null)
                {
                    EnergyBars[i] = GameObject.Instantiate(EnergyPrefab, newFrames[i].transform);
                    EnergyInnerBars[i] = EnergyBars[i].transform.GetChild(0).gameObject.GetComponent<RectTransform>();
                    EnergyBars[i].SetActive(false);
                }

                
                var energyRect = EnergyBars[i].GetComponent<RectTransform>();
                if (i < 10)
                {
                    energyRect.pivot = new Vector2(0.5f, 1f);
                    energyRect.anchorMin = new Vector2(0.5f, 0f);
                    energyRect.anchorMax = new Vector2(0.5f, 0f);
                    energyRect.anchoredPosition = new Vector2(0f, -5f);
                }
                else
                {
                    energyRect.pivot = new Vector2(0.5f, 0f);
                    energyRect.anchorMin = new Vector2(0f, 0.5f);
                    energyRect.anchorMax = new Vector2(0f, 0.5f);
                    energyRect.anchoredPosition = new Vector2(-5f, 0f);
                    //EnergyBars[i].transform.localPosition = new Vector2(-23f, 0f);
                    //EnergyBars[i].transform.localRotation = Quaternion.Euler(0f, 0f, 90f);
                    //EnergyBars[i].transform.localScale = new Vector3(0f, -1f, 0f);
                }
            }
        }

        [HarmonyPatch(typeof(global::StartOfRound), "PlayFirstDayShipAnimation")]
        [HarmonyPostfix]
        public static void PlayFirstDayShipAnimation(global::HUDManager __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            ArrangeHotbar(Perks.InventorySlots());
        }

        [HarmonyPatch(typeof(global::HUDManager), "Update")]
        [HarmonyPostfix]
        public static void UpdateHotbar(global::HUDManager __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (global::GameNetworkManager.Instance != null &&
                global::GameNetworkManager.Instance.localPlayerController != null)
            {
                var player = global::GameNetworkManager.Instance.localPlayerController;
                EquipmentSlots.localScale = new Vector3(0.5f + 0.5f * EquipmentPhase / 2f, 0.5f + 0.5f * EquipmentPhase / 2f, 0.5f + 0.5f * EquipmentPhase / 2f);
                var prevPhase = EquipmentPhase;
                if (InEquipmentSlots && EquipmentPhase < 1f)
                    EquipmentPhase = Mathf.Clamp01(EquipmentPhase + Time.deltaTime * 5f);
                else if (!InEquipmentSlots && EquipmentPhase > 0f)
                    EquipmentPhase = Mathf.Clamp01(EquipmentPhase - Time.deltaTime * 5f);
                if (EquipmentPhase != prevPhase)
                    ArrangeHotbar(Perks.InventorySlots());
                for (var i = 0; i < MAX_INVENTORY; i++)
                {
                    if (EnergyBars[i] != null)
                    {
                        if (player.ItemSlots[i] != null && player.ItemSlots[i].itemProperties.requiresBattery)
                        {
                            EnergyBars[i].SetActive(true);
                            EnergyInnerBars[i].sizeDelta = new Vector2(89f * (player.ItemSlots[i].insertedBattery?.charge ?? 0f), EnergyInnerBars[i].sizeDelta.y);
                        }
                        else
                        {
                            EnergyBars[i].SetActive(false);
                        }
                    }
                }
            }
        }

        public static bool HeadSlotAvailable
        {
            get
            {
                if (ServerConfiguration.Instance.General.DeactivateHotbar)
                    return false;

                return
                    (ServerConfiguration.Instance.Items.GetByItemName("Vision enhancer")?.Active ?? false) || 
                    (ServerConfiguration.Instance.Items.GetByItemName("Helmet lamp")?.Active ?? false) || 
                    (ServerConfiguration.Instance.Items.GetByItemName("Headset")?.Active ?? false);
            }
        }
        public static bool BodySlotAvailable
        {
            get
            {
                if (ServerConfiguration.Instance.General.DeactivateHotbar)
                    return false;

                return (ServerConfiguration.Instance.Items.GetByItemName("Bulletproof vest")?.Active ?? false);
            }
        }
        public static bool FeetSlotAvailable
        {
            get
            {
                if (ServerConfiguration.Instance.General.DeactivateHotbar)
                    return false;

                return
                    (ServerConfiguration.Instance.Items.GetByItemName("Rocket boots")?.Active ?? false) || 
                    (ServerConfiguration.Instance.Items.GetByItemName("Flippers")?.Active ?? false) ||
                    (ServerConfiguration.Instance.Items.GetByScrapName("Light shoes")?.Active ?? false);
            }
        }

        public static void ArrangeHotbar(int hotbarSize)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (Inventory == null) return;
            EquipmentGridLayout.cellSize = new Vector2(80f * ClientConfiguration.Instance.Hotbar.HotbarScale, 80f * (463f / 396f) * ClientConfiguration.Instance.Hotbar.HotbarScale);
            EquipmentGridLayout.spacing = new Vector2(0f, 10f * ClientConfiguration.Instance.Hotbar.HotbarScale);
            InventoryGridLayout.spacing = new Vector2(ClientConfiguration.Instance.Hotbar.HotbarSpacing, 0f);

            var inventoryRect = global::HUDManager.Instance.Inventory.canvasGroup.GetComponent<RectTransform>();
            var parentRect = inventoryRect.parent.GetComponent<RectTransform>();
            var corners = new Vector3[4];
            parentRect.GetWorldCorners(corners);
            var canvas = parentRect.GetComponentInParent<Canvas>();
            var leftRightScreen = canvas.worldCamera.WorldToScreenPoint(corners[1]);
            var topRightScreen = canvas.worldCamera.WorldToScreenPoint(corners[2]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, leftRightScreen, canvas.worldCamera, out var parentLeft);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRect, topRightScreen, canvas.worldCamera, out var parentRight);

            float scale = ClientConfiguration.Instance.Hotbar.HotbarScale * 1.75f;
            float leftClearance = 200f;
            float rightClearance = 40f * ClientConfiguration.Instance.Hotbar.HotbarScale;
            float availableWidth = parentRect.rect.width;

            var effectiveHotbarSize = hotbarSize;

            var wantedSize = (effectiveHotbarSize * 40f + (effectiveHotbarSize - 1) * ClientConfiguration.Instance.Hotbar.HotbarSpacing) * scale;
            if (availableWidth / 2f - wantedSize / 2f < leftClearance)
            {
                availableWidth -= leftClearance;
                inventoryRect.offsetMin = new Vector2(200f, ClientConfiguration.Instance.Hotbar.HotbarY / 100f * 40f);
                InventoryGridLayout.childAlignment = TextAnchor.LowerLeft;
            }
            else
            {
                inventoryRect.offsetMin = new Vector2(-35f, ClientConfiguration.Instance.Hotbar.HotbarY / 100f * 40f);
                InventoryGridLayout.childAlignment = TextAnchor.LowerCenter;
            }
            if ((availableWidth - rightClearance) < wantedSize)
            {
                scale *= (availableWidth - rightClearance) / wantedSize;
            }

            InventoryGridLayout.cellSize = new Vector2(40f * scale, 40f * scale);

            RectTransformUtility.ScreenPointToLocalPointInRectangle(inventoryRect, topRightScreen, canvas.worldCamera, out var local);

            inventoryRect.anchorMin = new Vector2(0.0f, 0f);
            inventoryRect.anchorMax = new Vector2(1f, 0f);
            inventoryRect.pivot = new Vector2(0f, 0f);
            
            inventoryRect.offsetMax = new Vector2(local.x - (parentRect.rect.width - inventoryRect.offsetMin.x), 100f + ClientConfiguration.Instance.Hotbar.HotbarY);

            var n = 0;
            var e = 0;
            for (var i = 0; i < MAX_INVENTORY; i++)
            {
                var frame = global::HUDManager.Instance.itemSlotIconFrames[i];
                frame.GetComponent<Image>().pixelsPerUnitMultiplier = 10f - (ClientConfiguration.Instance.Hotbar.HotbarBorderWidth / 12f);
                var icon = global::HUDManager.Instance.itemSlotIcons[i];
                var type = i == 10 ? InventoryType.HELMET : i == 11 ? InventoryType.BODY : i == 12 ? InventoryType.SHOES : InventoryType.INVENTORY;
                
                var energyRect = EnergyBars[i].GetComponent<RectTransform>();

                if (type == InventoryType.INVENTORY)
                {
                    energyRect.anchoredPosition = new Vector2(0f, -3f * scale);
                    energyRect.localScale = new Vector3(InventoryGridLayout.cellSize.x / 100f, InventoryGridLayout.cellSize.x / 100f, 1f);

                    icon.rectTransform.localScale = Vector3.one;
                    icon.rectTransform.offsetMin = new Vector2(5f * scale, 5f * scale);
                    icon.rectTransform.offsetMax = new Vector2(-5f * scale, -5f * scale);

                    var isActive = i < hotbarSize;
                    frame.gameObject.SetActive(isActive); 
                    //ArrangeFrame(i, isActive, new Vector2(startX + n * slotDistance, type != InventoryType.INVENTORY ? 2.7f : 0f), type);
                    //if (isActive) n++;
                }
                else
                {
                    energyRect.anchoredPosition = new Vector2(-20f * ClientConfiguration.Instance.Hotbar.HotbarScale, -7f * ClientConfiguration.Instance.Hotbar.HotbarScale);
                    energyRect.rotation = Quaternion.Euler(0f, 0f, 90f);
                    energyRect.localScale = new Vector3(EquipmentGridLayout.cellSize.x / 100f, -(EquipmentGridLayout.cellSize.x / 100f), 1f);

                    icon.rectTransform.localScale = Vector3.one;
                    icon.rectTransform.offsetMin = new Vector2(5f * ClientConfiguration.Instance.Hotbar.HotbarScale, 5f * ClientConfiguration.Instance.Hotbar.HotbarScale);
                    icon.rectTransform.offsetMax = new Vector2(-5f * ClientConfiguration.Instance.Hotbar.HotbarScale, -15f * ClientConfiguration.Instance.Hotbar.HotbarScale);

                    var isActive = (type == InventoryType.HELMET && HeadSlotAvailable) || (type == InventoryType.BODY && BodySlotAvailable) || (type == InventoryType.SHOES && FeetSlotAvailable);
                    
                    var equipmentIcon = frame.transform.GetChild(1);
                    var iconRect = equipmentIcon.GetComponent<RectTransform>();
                    iconRect.localScale = new Vector3(ClientConfiguration.Instance.Hotbar.HotbarScale, ClientConfiguration.Instance.Hotbar.HotbarScale, ClientConfiguration.Instance.Hotbar.HotbarScale);

                    frame.gameObject.SetActive(isActive); 
                    /*ArrangeFrame(i, isActive, new Vector2(0f, e * 50f), type);
                    if (isActive) e++;*/
                }
            }

        }

        private static float PreviousAnchorMax = 0f;
        private static float PreviousWidth = 0f;
        public static void ArrangeFrame(int index, bool isActive, Vector2 position, InventoryType type)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            var frame = global::HUDManager.Instance.itemSlotIconFrames[index];
            //frame.rectTransform.sizeDelta = new Vector2(scale * 36f, scale * (36f + (type != InventoryType.INVENTORY ? 6f : 0f)));
            frame.rectTransform.anchoredPosition = position + (type == InventoryType.INVENTORY ? Vector2.zero : new Vector2(-frame.rectTransform.sizeDelta.x / 2f, frame.rectTransform.sizeDelta.y / 2f));
            //frame.transform.GetChild(0).localScale = new Vector3(scale, scale, scale);
            //if (type != InventoryType.INVENTORY)
            //    frame.transform.GetChild(1).localScale = new Vector3(scale, scale, scale);
            frame.gameObject.SetActive(isActive);

        }

        private static bool UpdateNextFrame = false;
        [HarmonyPatch(typeof(global::HUDManager), "Update")]
        [HarmonyPrefix]
        private static void ObserveWindowSize()
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (global::GameNetworkManager.Instance.localPlayerController != null)
            {
                if (Inventory != null)
                {
                    if (UpdateNextFrame)
                    {
                        ArrangeHotbar(Perks.InventorySlots());
                        UpdateNextFrame = false;
                    }
                    var anchorMax = Inventory.parent.GetComponent<RectTransform>().offsetMax.x;
                    var width = Inventory.parent.GetComponent<RectTransform>().rect.width;

                    if (anchorMax != PreviousAnchorMax && width != PreviousWidth)
                    {
                        UpdateNextFrame = true;
                        PreviousAnchorMax = anchorMax;
                        PreviousWidth = width;
                    }
                }
            }
        }

        private static FieldInfo CurrentlyGrabbingObjectField = typeof(GameNetcodeStuff.PlayerControllerB).GetField("currentlyGrabbingObject", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
        private static global::GrabbableObject HoveringObject;
        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "SetHoverTipAndCurrentInteractTrigger")]
        [HarmonyPrefix]
        private static void SetHoverTipAndCurrentInteractTrigger(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return;

            if (!__instance.isGrabbingObjectAnimation)
            {
                var interactRay = new Ray(__instance.gameplayCamera.transform.position, __instance.gameplayCamera.transform.forward);
                if (Physics.Raycast(interactRay, out var hit, __instance.grabDistance, 832) && hit.collider.gameObject.layer != 8)
                {
                    var go = hit.collider.gameObject;
                    HoveringObject = go.GetComponent<global::GrabbableObject>();
                    return;
                }
            }
            HoveringObject = null;
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "GrabObjectClientRpc")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchGrabObjectClientRpc(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase methodBase)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->GrabObjectClientRpc...");

            var inst = new List<CodeInstruction>(instructions);
            bool first = false;
            for (var i = 0; i < inst.Count; i++)
            {
                if (!first && inst[i].opcode == OpCodes.Call && inst[i].operand.ToString().Contains("SwitchToItemSlot"))
                {
                    var insts = new List<CodeInstruction>()
                    {
                        new CodeInstruction(OpCodes.Ldarg_0),
                        inst[i - 3],
                        inst[i - 2],
                        inst[i - 1],
                        new CodeInstruction(OpCodes.Stfld, typeof(global::GameNetcodeStuff.PlayerControllerB).GetField("currentlyGrabbingObject", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public))
                    };
                    insts[0].MoveLabelsFrom(inst[i - 6]);
                    for (var j = insts.Count - 1; j >= 0; j--)
                        inst.Insert(i - 6, insts[j]);
                    first = true;
                }
                if (first) break;
            }
            Plugin.Log.LogDebug("Patched PlayerControllerB->GrabObjectClientRpc...");
            return inst.AsEnumerable();
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "FirstEmptyItemSlot")]
        [HarmonyPrefix]
        public static bool FirstEmptyItemSlot(GameNetcodeStuff.PlayerControllerB __instance, ref int __result)
        {
            if (ServerConfiguration.Instance.General.DeactivateHotbar)
                return true;
            __result = -1;
            var grabbingObject = __instance.currentlyGrabbingObject;
            var inventorySize = Perks.InventorySlotsOf(__instance);
            global::GrabbableObject checkingObject = null;
            if (grabbingObject != null)
                checkingObject = (global::GrabbableObject)grabbingObject;
            if (checkingObject == null)
                checkingObject = HoveringObject;
            var currentSlotType = __instance.currentItemSlot == 10 ? InventoryType.HELMET : __instance.currentItemSlot == 11 ? InventoryType.BODY : __instance.currentItemSlot == 12 ? InventoryType.SHOES : InventoryType.INVENTORY;
            if (checkingObject != null)
            {
                var checkingObjectType = checkingObject is Objects.Helmet ? InventoryType.HELMET : checkingObject is Objects.Body ? InventoryType.BODY : checkingObject is Objects.Boots ? InventoryType.SHOES : InventoryType.INVENTORY;

                if (__instance.ItemSlots[__instance.currentItemSlot] == null && (currentSlotType == InventoryType.INVENTORY || checkingObjectType == currentSlotType))
                {
                    __result = __instance.currentItemSlot;
                }
                else
                {
                    for (int i = 0; i < inventorySize; i++)
                    {
                        if (__instance.ItemSlots[i] == null)
                        {
                            __result = i;
                            break;
                        }
                    }
                    if (__result == -1 && checkingObjectType != InventoryType.INVENTORY)
                    {
                        var clothing = (Objects.Clothing)checkingObject;
                        var equipmentSlot = clothing.GetEquipmentSlot();
                        if (__instance.ItemSlots[equipmentSlot] == null)
                            __result = equipmentSlot;
                    }
                }
                return false;
            }
            return true;
        }


        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "SwitchToItemSlot")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> PatchSwitchToItemSlot(IEnumerable<CodeInstruction> instructions, ILGenerator generator, MethodBase methodBase)
        {
            Plugin.Log.LogDebug("Patching PlayerControllerB->SwitchToItemSlot...");

            var method = typeof(GrabbableObjectAdditions).GetMethod("GetIcon", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);
            var method2 = typeof(ClientConfiguration).GetMethod("GetHotbarAlpha", BindingFlags.Static | BindingFlags.FlattenHierarchy | BindingFlags.Public);

            var inst = new List<CodeInstruction>(instructions);
            bool first = false;
            for (var i = 0; i < inst.Count; i++)
            {
                if (!first && inst[i].opcode == OpCodes.Ldfld && inst[i].operand.ToString() == "UnityEngine.Sprite itemIcon")
                {
                    inst.RemoveAt(i);
                    inst.Insert(i, new CodeInstruction(OpCodes.Call, method));
                    inst.RemoveAt(i - 1);
                    first = true;
                }
                if (inst[i].opcode == OpCodes.Ldc_R4 && (float)inst[i].operand == 0.13f)
                {
                    inst[i].opcode = OpCodes.Call;
                    inst[i].operand = method2;
                }
            }
            Plugin.Log.LogDebug("Patched PlayerControllerB->SwitchToItemSlot...");
            return inst.AsEnumerable();
        }

    }
}
