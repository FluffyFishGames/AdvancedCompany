using BepInEx.Configuration;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;

namespace AdvancedCompany.Config
{
    internal class ModpackConfig
    {
        internal static ModpackConfig Instance;
        internal ConfigFile File;
        internal ConfigEntry<string> LogoFilename;
        internal ConfigEntry<bool> ShowDefaultPresets;
        internal ConfigEntry<string> StandardPreset;
        internal ConfigEntry<bool> SkipPresets;

        static ModpackConfig()
        {
            Instance = new ModpackConfig();
        }

        internal ModpackConfig()
        {
            File = new ConfigFile(System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "ModPack.cfg"), true);
            LogoFilename = File.Bind<string>(new ConfigDefinition("Logo", "Image filename"), "Logo.png", new ConfigDescription("The image filename AC should look for when searching for a custom menu logo image."));
            ShowDefaultPresets = File.Bind<bool>(new ConfigDefinition("Presets", "Show default"), true, new ConfigDescription("If the standard presets \"Default\", \"Alternative\" and \"Vanilla\" should be shown."));
            StandardPreset = File.Bind<string>(new ConfigDefinition("Presets", "Standard"), "Default", new ConfigDescription("The name of the preset which should be selected for a new save file."));
            SkipPresets = File.Bind<bool>(new ConfigDefinition("Presets", "Skip presets"), false, new ConfigDescription("Automatically selects the standard preset and skips the setup screen of AC when activated."));
        }
    }
}
