using AdvancedCompany.Objects.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Objects
{
    internal class GrabbableObjectAdditions
    {
        public static Sprite GetIcon(GrabbableObject obj)
        {
            if (obj is IIconProvider provider)
            {
                return provider.GetIcon();
            }
            return obj.itemProperties.itemIcon;
        }

        public static int GetInventoryPosition(GrabbableObject obj)
        {
            if (obj.playerHeldBy != null)
                for (var i = 0; i < obj.playerHeldBy.ItemSlots.Length; i++)
                    if (obj.playerHeldBy.ItemSlots[i] == obj)
                        return i;
            return -1;
        }

        public static void ChangeIcon(GrabbableObject obj, Sprite icon)
        {
            if (obj.playerHeldBy != null && obj.playerHeldBy == GameNetworkManager.Instance.localPlayerController)
            {
                var inventoryPos = GetInventoryPosition(obj);
                if (inventoryPos > -1)
                    HUDManager.Instance.itemSlotIcons[inventoryPos].sprite = icon;
            }
        }
    }
}
