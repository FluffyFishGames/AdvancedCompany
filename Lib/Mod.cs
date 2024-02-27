using BepInEx;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace AdvancedCompany.Lib
{
    public static class Mod
    {
        internal static string Hash;
        internal static Dictionary<string, string> RequiredMods = new Dictionary<string, string>();

        public static void RegisterRequiredMod(string modName, string modVersion)
        {
            Hash = null;
            RequiredMods[modName] = modVersion;
        }

        public static void RegisterRequiredMod(BaseUnityPlugin plugin)
        {
            if (plugin != null)
            {
                Hash = null;
                var pluginInfo = plugin.GetType().GetCustomAttribute<BepInPlugin>();
                if (pluginInfo != null)
                    RequiredMods.Add(pluginInfo.GUID, pluginInfo.Version.ToString());
            }
        }

        public static string GetHash()
        {
            if (Hash == null)
            {
                var mods = RequiredMods.Keys.ToList();
                mods.Sort();
                var modString = "";
                for (var i = 0; i < mods.Count; i++)
                {
                    modString += mods[i] + RequiredMods[mods[i]];
                }
                using (var md5 = MD5.Create())
                {
                    var bytes = md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(modString));
                    StringBuilder sb = new System.Text.StringBuilder();
                    for (int i = 0; i < bytes.Length; i++)
                    {
                        sb.Append(bytes[i].ToString("X2"));
                    }
                    Hash = sb.ToString().ToLowerInvariant();
                }
            }
            return Hash;
        }
    }
}
