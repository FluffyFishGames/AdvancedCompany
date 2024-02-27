using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Objects
{
    internal abstract class Boots : Clothing
    {
        public override int GetEquipmentSlot()
        {
            return 12;
        }
    }
}
