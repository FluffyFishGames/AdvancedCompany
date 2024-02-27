using AdvancedCompany.Config;
using Mono.Collections.Generic;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using static AdvancedCompany.Perks;

namespace AdvancedCompany.Game
{
    internal class Perk
    {
        public string ID;
        public Type PerkType;
        public string Name;
        public string Description;
        public ServerConfiguration.PerkConfiguration Configuration;
        private Func<float, float, int, float> Multiplier;

        public bool IsActive
        {
            get
            {
                /*if (ID == "TravelDiscount" && !ServerConfiguration.Instance.Moons.ActivateMoonPrices)
                    return false;*/
                return Configuration.Active;
            }
        }
        public int[] Prices
        {
            get
            {
                return Configuration.Prices;
            }
        }
        public float BaseValue
        {
            get
            {
                return Configuration.Base;
            }
        }
        public float Change
        {
            get
            {
                return Configuration.Change;
            }
        }

        public enum Type
        {
            PLAYER = 0,
            SHIP = 1
        }
        public int Levels
        {
            get
            {
                return Prices.Length;
            }
        }

        private static Dictionary<string, Perk> AllPerks = new Dictionary<string, Perk>();
        private static Dictionary<Type, List<Perk>> ByType = new Dictionary<Type, List<Perk>>();

        public string DisplayDescription
        {
            get
            {
                return Description.Replace("[change]", (System.Math.Round(Change * 1000f) / 10f).ToString(CultureInfo.InvariantCulture) + "");
            }
        }

        public static IReadOnlyDictionary<string, Perk> Perks
        {
            get
            {
                return AllPerks;
            }
        }

        public static IReadOnlyCollection<Perk> PerksByType(Type type)
        {
            if (ByType.ContainsKey(type))
                return ByType[type];
            else return new List<Perk>();
        }

        public Perk(Type type, string id, string name, string description, ServerConfiguration.PerkConfiguration configuration, Func<float, float, int, float> multiplier)
        {
            if (AllPerks.ContainsKey(id))
                throw new ArgumentException($"Perk with ID '{id}' already exists!");
            ID = id;
            PerkType = type;
            Name = name;
            Description = description;
            Configuration = configuration;
            Multiplier = multiplier;

            AllPerks.Add(id, this);
            if (!ByType.ContainsKey(type))
                ByType.Add(type, new());
            ByType[type].Add(this);
        }

        public float GetMultiplier(IUpgradeable upgradeable)
        {
            return Multiplier(BaseValue, Change, GetLevel(upgradeable));
        }

        public int GetTotalPrice(int level)
        {
            var total = 0;
            for (var i = 0; i < level && i < Levels; i++)
                total += Price(i);
            return total;
        }

        public int Price(int level)
        {
            if (level < Prices.Length)
                return Prices[level];
            return 0;
        }

        public int GetNextPrice(IUpgradeable upgradeable)
        {
            var level = GetLevel(upgradeable);
            if (level < Levels)
                return Price(level);
            return 0;
        }

        public int GetLevel(IUpgradeable upgradeable)
        {
            if (upgradeable is Ship && PerkType != Type.SHIP ||
                upgradeable is Player && PerkType != Type.PLAYER)
                return 0;
            var level = upgradeable.GetLevel(this);
            var ret = level;
            if (ret > Levels)
                ret = Levels;
            return ret;
        }

        public void IncreaseLevel(IUpgradeable upgradeable)
        {
            var newLevel = GetLevel(upgradeable) + 1;
            if (newLevel <= Levels)
                upgradeable.SetLevel(this, upgradeable.GetLevel(this) + 1);
        }
    }
}
