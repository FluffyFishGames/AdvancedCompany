using AdvancedCompany.Cosmetics;
using System.Collections.Generic;

namespace AdvancedCompany
{
    public class CosmeticDatabase
    {
        public static Dictionary<CosmeticType, Dictionary<string, CosmeticInstance>> Cosmetics = new Dictionary<CosmeticType, Dictionary<string, CosmeticInstance>>()
        {
            { CosmeticType.HAT, new Dictionary<string, CosmeticInstance>() },
            { CosmeticType.CHEST, new Dictionary<string, CosmeticInstance>() },
            { CosmeticType.HIP, new Dictionary<string, CosmeticInstance>() },
            { CosmeticType.R_LOWER_ARM, new Dictionary<string, CosmeticInstance>() },
            { CosmeticType.WRIST, new Dictionary<string, CosmeticInstance>() },
            { CosmeticType.R_SHIN, new Dictionary<string, CosmeticInstance>() },
            { CosmeticType.L_SHIN, new Dictionary<string, CosmeticInstance>() }
        };
        public static Dictionary<string, CosmeticInstance> AllCosmetics = new Dictionary<string, CosmeticInstance>();
        public static void AddCosmetic(CosmeticInstance instance)
        {
            if (!AllCosmetics.ContainsKey(instance.cosmeticId))
            {
                AllCosmetics.Add(instance.cosmeticId, instance);
                Cosmetics[instance.cosmeticType].Add(instance.cosmeticId, instance);
            }
        }
    }
}
