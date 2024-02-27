using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Objects
{
    internal abstract class Body : Clothing
    {
        public override int GetEquipmentSlot()
        {
            return 11;
        }
    }
}
