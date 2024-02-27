using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Config
{
    public class AllUnlockables : Configuration
    {
        public Dictionary<string, LobbyConfiguration.UnlockableConfig> Unlockables = new Dictionary<string, LobbyConfiguration.UnlockableConfig>();
    }
}
