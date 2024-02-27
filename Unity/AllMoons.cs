using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Config
{
    internal class AllMoons : Configuration
    {
        public Dictionary<string, LobbyConfiguration.MoonConfig> Moons = new Dictionary<string, LobbyConfiguration.MoonConfig>();
    }
}
