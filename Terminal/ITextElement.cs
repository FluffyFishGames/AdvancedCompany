using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal
{
    public interface ITextElement
    {
        string GetText(int availableWidth);
    }
}
