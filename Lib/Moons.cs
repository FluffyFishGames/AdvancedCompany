using AdvancedCompany.Game;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Lib
{
    public class Moons
    {
        public static int GetConfigMoonPrice(int levelID, int defaultPrice = 0)
        {
            return Manager.Moons.GetMoonPrice(levelID, defaultPrice);
        }

        public static int GetMoonPrice(int levelID, int defaultPrice = 0)
        {
            return Mathf.RoundToInt(Manager.Moons.GetMoonPrice(levelID, defaultPrice) * Perks.GetMultiplier("TravelDiscount"));
        }
    }
}
