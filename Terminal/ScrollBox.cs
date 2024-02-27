using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedCompany.Terminal
{
    public class ScrollBox : ITextElement
    {
        public IScrollable Content;
        public int MaxHeight = 0;
        public int ScrollPos = 0;

        public ScrollBox(int maxHeight)
        {
            MaxHeight = maxHeight;
        }

        public string GetText(int availableWidth)
        {
            var lines = Content.GetText(availableWidth);
            var lineCount = lines.Count<char>(c => c.Equals('\n')) - 1;
            if (lineCount > MaxHeight)
            {
                lines = Content.GetText(availableWidth - 2);
                int scroll = Content.GetScroll(MaxHeight);
                var l = lines.Split('\n');
                if (scroll >= l.Length - MaxHeight)
                    scroll = l.Length - MaxHeight - 1;
                if (scroll < 0)
                    scroll = 0;
                
                var text = "";
                var scrollBarHeight = (int)System.Math.Floor((float)MaxHeight * ((float)MaxHeight / (float)(l.Length)));
                var scrollBarPosition = (int)System.Math.Round(((float)scroll / (float)(l.Length - MaxHeight)) * (float)(MaxHeight - scrollBarHeight));
                for (int i = scroll; i < scroll + MaxHeight && i < l.Length; i++)
                {
                    var isScroll = (i - scroll) >= scrollBarPosition && (i - scroll) <= scrollBarPosition + scrollBarHeight;
                    var line = l[i];
                    text += line + " " + (isScroll ? "█" : "░") + "\n";
                }
                return text;
            }
            else
                return lines;

        }
    }
}