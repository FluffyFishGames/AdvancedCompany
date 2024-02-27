using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal
{
    public class BoxedScreenStore : IScreen
    {
        public string Title;
        public int TotalCost = 0;
        public List<ITextElement> Content;

        public virtual string GetText(int availableWidth)
        {
            var credits = Game.Manager.Terminal.groupCredits;
            string creditsHeader = "Credits: " + credits;
            var text = $"  ╔{new String('═', Title.Length + 2)}╗{new String(' ', availableWidth - 6 - Title.Length - creditsHeader.Length)}{creditsHeader}\n" +
                $"╭─╢ <color=#ffffff>{Title}</color> ╟{new String('─', availableWidth - 7 - Title.Length)}╮\n" +
                $"│ ╚{new String('═', Title.Length + 2)}╝{new String(' ', availableWidth - Title.Length - 7)}│\n";
            foreach (var c in Content)
                text += Utils.WrapText(c.GetText(availableWidth - 4), availableWidth, "│ ", " │");
            
            var rightPos = availableWidth - 11;
            var totalCostLength = 6;
            var totalCost = "$" + TotalCost;
            if (totalCost.Length < totalCostLength) totalCost = new string(' ', totalCostLength - totalCost.Length) + totalCost;
            text += $"│{new String(' ', rightPos - 3)}╔{new String('═', totalCostLength + 2)}╗{new String(' ', availableWidth - rightPos - totalCost.Length - 3)}│\n";
            text += $"╰{new String('─', rightPos - 3)}╢ {(credits < TotalCost ? "<color=#ff0000>" : "")}{totalCost}{(credits < TotalCost ? "</color>" : "")} ╟{new String('─', availableWidth - rightPos - totalCost.Length - 3)}╯\n";
            text += $"{new String(' ', rightPos - 2)}╚{new String('═', totalCostLength + 2)}╝\n";
            return text;
        }
    }
}
