using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Config
{
    public class AllScrap : Configuration
    {
        public Dictionary<string, LobbyConfiguration.ScrapConfig> Items = new Dictionary<string, LobbyConfiguration.ScrapConfig>();
    }
}
