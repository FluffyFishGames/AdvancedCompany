using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using UnityEngine;
using AdvancedCompany.Network.Messages;

namespace AdvancedCompany.Objects
{
    internal abstract class Clothing : GrabbableObject, IClothing
    {
        public static MethodInfo FirstEmptyItemSlotMethod = typeof(GameNetcodeStuff.PlayerControllerB).GetMethod("FirstEmptyItemSlot", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.FlattenHierarchy);

        public virtual Game.Player.BodyLayers GetLayers() { return Game.Player.BodyLayers.NONE; }
        public virtual GameObject[] CreateWearable(Game.Player player) { return null; }
        public virtual bool CanUnequip()
        {
            return true;
        }

        public virtual void Equipped(Game.Player player)
        {

        }
        public virtual void Equip()
        {
            if (playerHeldBy.ItemSlots[GetEquipmentSlot()] is IClothing c && !c.CanUnequip())
                return;
            Network.Manager.Send(new ChangeItemSlot() { PlayerNum = (int) playerHeldBy.playerClientId, FromSlot = playerHeldBy.currentItemSlot, ToSlot = GetEquipmentSlot() });
        }

        public virtual void Unequipped(Game.Player player)
        {

        }
        public virtual void Unequip()
        {
            if (!this.CanUnequip())
                return;
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

        public virtual int GetEquipmentSlot()
        {
            return -1;
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

        public override void ItemActivate(bool used, bool buttonDown = true)
        {
            base.ItemActivate(used, buttonDown);
            if (IsEquipped())
                Unequip();
            else
                Equip();
        }
    }
}
