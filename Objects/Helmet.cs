using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Objects
{
    internal abstract class Helmet : Clothing, IHelmet
    {
        public override int GetEquipmentSlot()
        {
            return 10;
        }
    }
}
