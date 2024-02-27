using AdvancedCompany.Game;
using AdvancedCompany.Objects;
using BepInEx.Configuration;
using LethalCompanyInputUtils.Api;
using System;
using System.Collections.Generic;
using System.Text;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem;
using static MonoMod.Cil.RuntimeILReferenceBag.FastDelegateInvokers;

namespace AdvancedCompany.Config
{
    [Boot.Bootable]
    internal class ClientConfiguration : PlayerConfiguration
    {
        public static float GetHotbarAlpha()
        {
            return Instance.Hotbar.HotbarAlpha;
        }

        public class KeybindsConfig : LcInputActions
        {
            [InputAction("<Keyboard>/1", Name = "Inventory slot 1")]
            public InputAction InventorySlot1 { get; set; }
            [InputAction("<Keyboard>/2", Name = "Inventory slot 2")]
            public InputAction InventorySlot2 { get; set; }
            [InputAction("<Keyboard>/3", Name = "Inventory slot 3")]
            public InputAction InventorySlot3 { get; set; }
            [InputAction("<Keyboard>/4", Name = "Inventory slot 4")]
            public InputAction InventorySlot4 { get; set; }
            [InputAction("<Keyboard>/5", Name = "Inventory slot 5")]
            public InputAction InventorySlot5 { get; set; }
            [InputAction("<Keyboard>/6", Name = "Inventory slot 6")]
            public InputAction InventorySlot6 { get; set; }
            [InputAction("<Keyboard>/7", Name = "Inventory slot 7")]
            public InputAction InventorySlot7 { get; set; }
            [InputAction("<Keyboard>/8", Name = "Inventory slot 8")]
            public InputAction InventorySlot8 { get; set; }
            [InputAction("<Keyboard>/9", Name = "Inventory slot 9")]
            public InputAction InventorySlot9 { get; set; }
            [InputAction("<Keyboard>/0", Name = "Inventory slot 10")]
            public InputAction InventorySlot10 { get; set; }
            [InputAction("<Keyboard>/f", Name = "Flashlight")]
            public InputAction Flashlight { get; set; }
            [InputAction("<Keyboard>/v", Name = "Equipment comms")]
            public InputAction Communications { get; set; }
            [InputAction("<Keyboard>/alt", Name = "Switch to equipment")]
            public InputAction Equipment { get; set; }
            [InputAction("<Keyboard>/x", Name = "Portable terminal")]
            public InputAction PortableTerminal { get; set; }

        }

        public static KeybindsConfig Keybinds = new();

        public static ClientConfiguration Instance;
        public static void Boot()
        {
            Instance = new ClientConfiguration();
        }
    }
}
