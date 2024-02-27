using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal
{
    public class HalfBoxedScreen : IScreen
    {
        public string Title;
        public int LeftWidth = 20;
        public int Height = 20;
        public List<ITextElement> RightContent;
        public List<ITextElement> LeftContent;
        public virtual string GetText(int availableWidth)
        {
            var text = $"  ╔{new String('═', Title.Length + 2)}╗\n" +
                $"╭─╢ <color=#ffffff>{Title}</color> ╟{new String('─', availableWidth - 7 - Title.Length)}╮\n" +
                $"│ ╚{new String('═', Title.Length + 2)}╝{new String(' ', availableWidth - Title.Length - 7)}│\n" +
                $"├{new String('─', LeftWidth)}┬{new String('─', availableWidth - LeftWidth - 3)}┤\n";
            var leftText = "";
            foreach (var c in LeftContent)
                leftText += c.GetText(LeftWidth);
            var rightText = "";
            var rightWidth = availableWidth - LeftWidth - 5;
            foreach (var c in RightContent)
                rightText += c.GetText(rightWidth);
            var leftLines = Utils.WrapText(leftText, LeftWidth).Split(new char[] {'\n' }, StringSplitOptions.None);
            var rightLines = Utils.WrapText(rightText, rightWidth).Split(new char[] { '\n' }, StringSplitOptions.None);
            
            for (var i = 0; i < Height - 5; i++)
            {
                var left = leftLines.Length > i ? leftLines[i] : "";
                if (left.Length < LeftWidth)
                    left += new string(' ', LeftWidth - left.Length);
                var right = rightLines.Length > i ? rightLines[i] : "";
                right = right.TrimEnd();
                if (right.Length < rightWidth)
                    right += new string(' ', rightWidth - right.Length);
                text += $"│{left}│ {right} │\n";
            }
            text += $"╰{new String('─', availableWidth - 2)}╯\n";
            return text;
        }
    }
}
