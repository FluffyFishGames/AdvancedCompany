using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Objects
{
    internal interface IEquipmentFlashlight
    {
        bool IsUsed();
        void SwitchFlashlight(Player player, bool on);
        bool CanBeUsed();
    }
}
