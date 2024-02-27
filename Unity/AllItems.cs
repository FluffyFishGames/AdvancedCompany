using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Config
{
    public class AllItems : Configuration
    {
        public Dictionary<string, LobbyConfiguration.ItemConfig> Items = new Dictionary<string, LobbyConfiguration.ItemConfig>();
    }
}
