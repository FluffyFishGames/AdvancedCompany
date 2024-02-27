using System;
using System.Collections.Generic;
using System.Text;
using AdvancedCompany.Game;

namespace AdvancedCompany.Terminal
{
    internal class PerkCursorElement : CursorElement
    {
        private Perk Perk;
        public PerkCursorElement(Perk perk)
        {
            Perk = perk;
        }

        public override string GetText(int availableWidth)
        {
            var nameLength = 25;
            var expBarLength = 11;
            var name = Perk.Name.Length > nameLength ? Perk.Name.Substring(0, nameLength) : Perk.Name + new string(' ', nameLength - Perk.Name.Length);
            var level = 0;
            var nextLevelCost = 0;
            if (Perk.PerkType == Perk.Type.PLAYER)
            {
                level = Network.Manager.Lobby.Player().GetLevel(Perk);
                nextLevelCost = Perk.GetNextPrice(Network.Manager.Lobby.Player());
            }
            if (Perk.PerkType == Perk.Type.SHIP)
            {
                level = Network.Manager.Lobby.CurrentShip.GetLevel(Perk);
                nextLevelCost = Perk.GetNextPrice(Network.Manager.Lobby.CurrentShip);
            }

            var lenEmpty = Perk.Levels - level;
            if (lenEmpty < 0) lenEmpty = 0;
            var lenSpace = expBarLength - Perk.Levels;
            if (lenSpace < 0) lenSpace = 0;
            var expBar = new string('◼', level) + new string('□', lenEmpty) + new string(' ', lenSpace);
            var nextLevel = "";
            if (nextLevelCost > 0)
                nextLevel = " " + nextLevelCost + "XP";
            return name + expBar + nextLevel;
        }
    }
}
