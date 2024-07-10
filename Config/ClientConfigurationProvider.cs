using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace AdvancedCompany.Config
{

    [Boot.Requires(typeof(ClientConfiguration))]
    [Boot.Bootable]
    internal class ClientConfigurationProvider : PlayerSettings.IConfigurationProvider
    {
        internal static ClientConfigurationProvider Instance;
        private List<PlayerSettings.Preset> Presets = new List<PlayerSettings.Preset>();
        private PlayerSettings.Preset FilePreset;

        public static void Boot()
        {
            Instance = new ClientConfigurationProvider();
        }

        public string Directory
        {
            get
            {
                return Path.Combine(Path.GetFullPath(Environment.ExpandEnvironmentVariables("%appdata%/../LocalLow/ZeekerssRBLX/Lethal Company")), "presets", "client");
            }
        }
        public ClientConfigurationProvider()
        {
            var configuration = new PlayerConfiguration();

            //configuration.LoadFromPreset(FileConfiguration.Instance);
            //FilePreset = new PlayerSettings.Preset(this, "File config", configuration, false, false);
            //Presets.Add(FilePreset);
            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
            var files = System.IO.Directory.GetFiles(Directory);
            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file) == ".preset")
                {
                    try
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(file);

                        PlayerConfiguration config = new PlayerConfiguration();
                        config.Build();
                        var jsonText = System.IO.File.ReadAllText(file);
                        var json = JObject.Parse(jsonText);
                        config.FromJSON(json, false);

                        Presets.Add(new PlayerSettings.Preset(this, name, config, true, true));
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogError("Error while loading client preset \""+file+"\":");
                        Plugin.Log.LogError(e);
                    }
                }
            }
            if (Presets.Count == 0)
            {
                Presets.Add(new BaseSettings<PlayerConfiguration>.Preset(this, "Default", new PlayerConfiguration(), false, false));
            }
        }

        public PlayerSettings.Preset CreatePreset(string name, PlayerConfiguration configuration)
        {
            var preset = new PlayerSettings.Preset(this, name, configuration, true, true);
            Presets.Add(preset);
            return preset;
        }

        public List<PlayerSettings.Preset> GetPresets()
        {
            return Presets;
        }

        public void PresetCreated(PlayerSettings.Preset preset)
        {
            PresetSaved(preset);
        }

        public void PresetRemoved(PlayerSettings.Preset preset)
        {
            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
            var file = System.IO.Path.Combine(Directory, preset.Name + ".preset");
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);
            Presets.Remove(preset);
        }

        public void PresetRenamed(PlayerSettings.Preset preset, string oldName, string newName)
        {
            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
            var oldFile = System.IO.Path.Combine(Directory, oldName + ".preset");
            var newFile = System.IO.Path.Combine(Directory, newName + ".preset");
            System.IO.File.Move(oldFile, newFile);
        }

        public void PresetSaved(PlayerSettings.Preset preset)
        {
            /*if (preset == FilePreset)
                FileConfiguration.Save(preset.Configuration);
            else
            {*/
            var text = preset.Configuration.ToJSON().ToString(Newtonsoft.Json.Formatting.Indented);
            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
            var file = System.IO.Path.Combine(Directory, preset.Name + ".preset");
            System.IO.File.WriteAllText(file, text);
            // save to file
            //}
        }

        public BaseSettings<PlayerConfiguration>.Preset GetPreset(string name)
        {
            for (var i = 0; i < Presets.Count; i++)
            {
                if (Presets[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    return Presets[i];
            }
            return null;
        }
    }
}
