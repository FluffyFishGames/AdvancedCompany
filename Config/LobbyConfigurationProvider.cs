using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AdvancedCompany.Config
{
    [Boot.Bootable]
    [Boot.Requires(typeof(ServerConfiguration))]
    internal class LobbyConfigurationProvider : LobbySettings.IConfigurationProvider
    {
        internal static LobbyConfigurationProvider Instance;
        private List<LobbySettings.Preset> Presets = new List<LobbySettings.Preset>();
        private LobbySettings.Preset FilePreset;

        public static void Boot()
        {
            Instance = new LobbyConfigurationProvider();
        }

        public string PredefinedPresetsDirectory
        {
            get
            {
                return System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(LobbyConfigurationProvider).Assembly.Location), "presets");
            }
        }
        public string Directory
        {
            get
            {
                return System.IO.Path.Combine(BepInEx.Paths.ConfigPath, "advancedcompany", "presets", "server");
            }
        }
        public LobbyConfigurationProvider()
        {
            /*var configuration = new LobbyConfiguration();
            configuration.LoadFromPreset(FileConfiguration.Instance);
            FilePreset = new LobbySettings.Preset(this, "File config", configuration, false, false);
            Presets.Add(FilePreset); */
            string[] files = null;
            if (ModpackConfig.Instance.ShowDefaultPresets.Value)
            {
                if (!System.IO.Directory.Exists(PredefinedPresetsDirectory))
                    System.IO.Directory.CreateDirectory(PredefinedPresetsDirectory);
                files = System.IO.Directory.GetFiles(PredefinedPresetsDirectory);
                foreach (var file in files)
                {
                    try
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(file);

                        LobbyConfiguration config = new LobbyConfiguration();
                        config.Build();

                        var jsonText = System.IO.File.ReadAllText(file);
                        var json = JObject.Parse(jsonText);
                        config.FromJSON(json, false);

                        Presets.Add(new LobbySettings.Preset(this, name, config, false, false, false));
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogError("Error while loading lobby preset \"" + file + "\":");
                        Plugin.Log.LogError(e);
                    }
                }
                Presets = Presets.OrderBy(x => x.Name == "Default" ? "aa" : x.Name).ToList();
            }

            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
            files = System.IO.Directory.GetFiles(Directory);
            foreach (var file in files)
            {
                if (System.IO.Path.GetExtension(file) == ".json")
                {
                    try
                    {
                        var name = System.IO.Path.GetFileNameWithoutExtension(file);

                        LobbyConfiguration config = new LobbyConfiguration();
                        config.Build();

                        var jsonText = System.IO.File.ReadAllText(file);
                        var json = JObject.Parse(jsonText);
                        config.FromJSON(json, false);

                        Presets.Add(new LobbySettings.Preset(this, name, config, true, true));
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogError("Error while loading lobby preset \"" + file + "\":");
                        Plugin.Log.LogError(e);
                    }
                }
            }
            if (Presets.Count == 0)
            {
                Presets.Add(new BaseSettings<LobbyConfiguration>.Preset(this, "Default", new LobbyConfiguration(), false, false));
            }
        }

        public LobbySettings.Preset CreatePreset(string name, LobbyConfiguration configuration)
        {
            var preset = new LobbySettings.Preset(this, name, configuration, true, true);
            Presets.Add(preset);
            return preset;
        }

        public List<LobbySettings.Preset> GetPresets()
        {
            return Presets;
        }

        public void PresetCreated(LobbySettings.Preset preset)
        {
            PresetSaved(preset);
        }

        public void PresetRemoved(LobbySettings.Preset preset)
        {
            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
            var file = System.IO.Path.Combine(Directory, preset.Name + ".json");
            if (System.IO.File.Exists(file))
                System.IO.File.Delete(file);
            Presets.Remove(preset);
        }

        public void PresetRenamed(LobbySettings.Preset preset, string oldName, string newName)
        {
            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
            var oldFile = System.IO.Path.Combine(Directory, oldName + ".json");
            var newFile = System.IO.Path.Combine(Directory, newName + ".json");
            System.IO.File.Move(oldFile, newFile);
        }

        public void PresetSaved(LobbySettings.Preset preset)
        {
            /*if (preset == FilePreset)
                FileConfiguration.Save(preset.Configuration);
            else
            {*/
            var text = preset.Configuration.ToJSON().ToString(Newtonsoft.Json.Formatting.Indented);
            if (!System.IO.Directory.Exists(Directory))
                System.IO.Directory.CreateDirectory(Directory);
            var file = System.IO.Path.Combine(Directory, preset.Name + ".json");
            System.IO.File.WriteAllText(file, text);
            // save to file
            //}
        }

        public BaseSettings<LobbyConfiguration>.Preset GetPreset(string name)
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
