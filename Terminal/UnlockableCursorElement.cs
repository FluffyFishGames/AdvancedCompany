using System;
using System.Collections.Generic;
using System.Text;
using AdvancedCompany.Game;
using UnityEngine;

namespace AdvancedCompany.Terminal
{
    internal class UnlockableCursorElement : CursorElement
    {
        internal UnlockableItem Item;
        internal int Amount;
        internal int MaxAmount = 1;
        
        public UnlockableCursorElement(UnlockableItem item, int maxAmount)
        {
            MaxAmount = maxAmount;
            Item = item;
            Amount = 0;
        }

        public override string GetText(int availableWidth)
        {
            var nameLength = 22;
            var priceLength = 6;

            var name = "<color=#ffffff>" + (Item.unlockableName.Length > nameLength ? Item.unlockableName.Substring(0, nameLength) : Item.unlockableName + new string(' ', nameLength - Item.unlockableName.Length)) + "</color>";
            var price = Game.Manager.Items.GetUnlockablePrice(Item);
            var priceText = "$" + Mathf.FloorToInt(price) + "";
            priceText = new string(' ', priceLength - priceText.Length) + priceText;
            var amountText = Amount + "";
            if (amountText.Length == 1) amountText = " " + amountText;
            var text = "← " + amountText + " → " + name + " " + priceText;

            text += "     ";

            var totalLength = 7;
            var total = " $" + (Amount * price);
            text += new string(' ', totalLength - total.Length) + total;

            return text;
        }
    }
}
