using System;
using System.Collections.Generic;
using System.Text;
using AdvancedCompany.Game;
using Steamworks.Ugc;
using UnityEngine;

namespace AdvancedCompany.Terminal
{
    internal class ItemCursorElement : CursorElement
    {
        internal Item Item;
        internal int Amount;

        public ItemCursorElement(Item item)
        {
            Item = item;
            Amount = 0;
        }

        public override string GetText(int availableWidth)
        {
            var price = Game.Manager.Items.GetItemPrice(Item, true);
            var percentage = Game.Manager.Items.GetItemSalesPercentage(Item);
            
            var nameLength = 22;
            var priceLength = 6;
            
            var name = "<color=#ffffff>" + (Item.itemName.Length > nameLength ? Item.itemName.Substring(0, nameLength) : Item.itemName + new string(' ', nameLength - Item.itemName.Length)) + "</color>";
            var priceText = "$" + price + "";
            priceText = new string(' ', priceLength - priceText.Length) + priceText;

            var amountText = Amount + "";
            if (amountText.Length == 1) amountText = " " + amountText;
            var text = "← " + amountText + " → " + name + " " + priceText;
            
            var percentLength = 5;
            var percent = "";
            if (percentage < 100)
                percent = " -" + (100 - percentage) + "%";
            else
                percent = " ";

            text += new string(' ', percentLength - percent.Length) + percent;

            var totalLength = 7;
            var total = " $" + (Amount * price);
            text += new string(' ', totalLength - total.Length) + total;
            
            return text;
        }
    }
}
