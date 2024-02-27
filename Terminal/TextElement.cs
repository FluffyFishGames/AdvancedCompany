using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Terminal
{
    public class TextElement : ITextElement
    {
        public string Text;
        public string GetText(int availableWidth)
        {
            //return Text;
            return Utils.WrapText(Text, availableWidth);
        }

        public static explicit operator TextElement(string text) => new TextElement() { Text = text };
    }
}
