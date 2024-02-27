using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Objects
{
    internal interface IEquipmentCommunication
    {
        void SwitchTalking(Player player, bool talking);
    }
}
