using System.Collections.Generic;
using System.Linq;

namespace AdvancedCompany.Config
{
    public class PlayerConfiguration : Configuration
    {
        public class GraphicsConfig : Configuration
        {
            public bool ShowOriginalLogo = false;

            [Slider(0f, 1f, Conversion = 100f, ShowValue = true)]
            public float VisionEnhancerBrightness = 0.7f;
            [Slider(0f, 1f, Conversion = 100f, ShowValue = true)]
            public float MusicVolume = 1f;
        }

        public class FileConfig : Configuration
        {
            public bool SaveInProfile = false;
        }

        public class CompabilityConfig : Configuration
        {
            public bool DisableMusic;
            public bool HideEquipment;
            public bool HideCosmetics;
            public bool AnimationsCompability;
        }

        public class HotbarConfig : Configuration
        {
            [Slider(0.1f,2f, Conversion = 100f, ShowValue = true)]
            public float HotbarScale = 1f;
            [Slider(0f, 1f, Conversion = 100f, ShowValue = true)]
            public float HotbarAlpha = 0.13f;
            [Slider(5f, 20f, Conversion = 1f, ShowValue = false)]
            public float HotbarSpacing = 10f;
            [Slider(10f, 50f, Conversion = 1f, ShowValue = false)]
            public float HotbarBorderWidth = 4f;
            [Slider(0f, 100f, Conversion = 1f, ShowValue = false)]
            public float HotbarY = 50f;
            public bool InvertScroll = true;
        }

        public class CosmeticsConfig : Configuration
        {
            public List<string> ActivatedCosmetics = new();
        }

        public FileConfig File = new();
        public HotbarConfig Hotbar = new();
        public CosmeticsConfig Cosmetics = new();
        public CompabilityConfig Compability = new();
        public GraphicsConfig Graphics = new();
    }
}