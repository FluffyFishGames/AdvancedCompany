using UnityEngine;

namespace AdvancedCompany.Cosmetics
{
    public enum CosmeticType
    {
        HAT,
        WRIST,
        CHEST,
        R_LOWER_ARM,
        HIP,
        L_SHIN,
        R_SHIN
    }
    public class CosmeticInstance : MonoBehaviour
    {
        public CosmeticType cosmeticType;

        public string cosmeticId;

        public Texture2D icon;
    }
}
