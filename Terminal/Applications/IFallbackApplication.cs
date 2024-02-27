using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal.Applications
{
    internal interface IFallbackApplication
    {
        float Certainty(string input);
        void Fallback(MobileTerminal terminal, string input);
    }
}
