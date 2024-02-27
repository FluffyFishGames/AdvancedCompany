using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Game
{
    internal interface IUpgradeable
    {
        int GetLevel(Perk perk);
        void SetLevel(Perk perk, int level);
    }
}
