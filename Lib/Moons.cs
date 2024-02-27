using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Lib
{
    public class Moons
    {
        public static int GetMoonPrice(int route)
        {
            return Manager.Moons.GetMoonPrice(route);
        }
    }
}
