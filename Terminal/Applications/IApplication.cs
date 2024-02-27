using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Terminal.Applications
{
    public interface IApplication
    {
        void Main(Game.MobileTerminal terminal, string[] args);
        void Exit();
        void Submit(string text);
        void Update();
    }
}
