using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal
{

    public class CursorElement : ITextElement
    {
        public string Name;
        public string Description;
        public Action Action;

        public virtual string GetText(int availableWidth)
        {
            var text = Name;
            if (Description != null && Description != "")
                text += $"\n{Utils.WrapText(Description, availableWidth, "  ", "")}";
            return text;
        }
    }
}
