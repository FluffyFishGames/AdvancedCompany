using AdvancedCompany.Network.Messages;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Objects
{
    internal interface IClothing
    {
        bool CanUnequip();
        Game.Player.BodyLayers GetLayers();
        GameObject[] CreateWearable(Game.Player player);
        void Equipped(Game.Player player);
        void Equip();
        void Unequipped(Game.Player player);
        void Unequip();
        int GetEquipmentSlot();
        bool IsEquipped();
    }
}
