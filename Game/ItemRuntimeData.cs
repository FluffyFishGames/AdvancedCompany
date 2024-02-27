using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Game
{
    internal class ItemRuntimeData
    {
        public Item Item;
        public TerminalNode BuyConfirmationNode;
        public TerminalNode BuyCompletedNode;
        public TerminalKeyword Keyword;
        public CompatibleNoun Noun;

        public void Apply(TerminalKeyword buyKeyword, List<TerminalKeyword> keywords, List<CompatibleNoun> buyNouns, TerminalNode cancelPurchaseNode, TerminalKeyword confirmKeyword, TerminalKeyword denyKeyword, List<Item> buyableItemsList)
        {
            var index = buyableItemsList.IndexOf(Item);
            if (index == -1)
            {
                index = buyableItemsList.Count;
                buyableItemsList.Add(Item);
            }

            var id = Item.itemName.Replace(" ", "-");

            BuyCompletedNode.buyItemIndex = index;
            BuyConfirmationNode.buyItemIndex = index;

            BuyConfirmationNode.terminalOptions = new CompatibleNoun[2]
            {
                new CompatibleNoun()
                {
                    noun = confirmKeyword,
                    result = BuyCompletedNode
                },
                new CompatibleNoun()
                {
                    noun = denyKeyword,
                    result = cancelPurchaseNode
                }
            };
            Keyword.defaultVerb = buyKeyword;

            if (!keywords.Contains(Keyword))
                keywords.Add(Keyword);
            if (!buyNouns.Contains(Noun))
                buyNouns.Insert(0, Noun);
        }
    }
}
