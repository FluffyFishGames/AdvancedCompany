using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Config
{
    public class AllEnemies : Configuration
    {
        public Dictionary<string, LobbyConfiguration.EnemyConfig> Enemies = new Dictionary<string, LobbyConfiguration.EnemyConfig>();
    }
}
