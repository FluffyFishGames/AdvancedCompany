using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Objects
{
    public interface ICursed
    {
        internal void UpdateEffects(Game.Player player);
    }
}
