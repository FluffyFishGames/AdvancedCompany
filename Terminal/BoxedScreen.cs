using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal
{
    public class BoxedScreen : IScreen
    {
        public string Title;
        public List<ITextElement> Content;

        public virtual string GetText(int availableWidth)
        {
            var text = $"  ╔{new String('═', Title.Length + 2)}╗\n" +
                $"╭─╢ <color=#ffffff>{Title}</color> ╟{new String('─', availableWidth - 7 - Title.Length)}╮\n" +
                $"│ ╚{new String('═', Title.Length + 2)}╝{new String(' ', availableWidth - Title.Length - 7)}│\n";
            foreach (var c in Content)
                text += Utils.WrapText(c.GetText(availableWidth - 4), availableWidth, "│ ", " │");
            text += $"╰{new String('─', availableWidth - 2)}╯\n";
            return text;
        }
    }
}
