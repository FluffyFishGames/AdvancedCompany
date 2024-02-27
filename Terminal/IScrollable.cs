using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal
{
    public interface IScrollable : ITextElement
    {
        public int GetScroll(int maxHeight);
    }
}
