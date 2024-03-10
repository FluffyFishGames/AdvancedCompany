using UnityEngine;
using BepInEx;
using Mono.Cecil;
using HarmonyLib;
using System;
using BepInEx.Logging;
using System.Reflection;
using AdvancedCompany.Boot;
using AdvancedCompany.Config;
using AdvancedCompany.Objects;
using AdvancedCompany.UI;
using System.IO;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine.Profiling;
using Steamworks;
using AdvancedCompany.Lib;

[assembly: AssemblyMetadata("AdvancedCompanyIgnore", "True")]
namespace AdvancedCompany
{
    [BepInPlugin(GUID, "AdvancedCompany", Version)]
    [BepInDependency("com.rune580.LethalCompanyInputUtils", BepInDependency.DependencyFlags.HardDependency)]
    public class Plugin : BaseUnityPlugin
    {
        public const string Version = "1.1.10";
        private const string GUID = "com.potatoepet.AdvancedCompany";
        internal static ManualLogSource Log;
        internal static Plugin Instance;
        internal static bool UseAnimationOverride = false;
        internal static List<Assembly> AssembliesToScan = new List<Assembly>();
        internal static bool Cripple = false;

        private void Awake()
        {
            try
            {
                Log = base.Logger;

                Lib.Mod.RegisterRequiredMod(this);
                try
                {
                    Plugin.Log.LogInfo("Manually loading MoreCompany stub...");
                    AppDomain.CurrentDomain.Load(System.IO.File.ReadAllBytes(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(typeof(Plugin).Assembly.Location), "MoreCompany.dll")));
                    Plugin.Log.LogInfo("MoreCompany Stub loaded!");
                }
                catch (Exception e)
                {
                    Plugin.Log.LogError("Error while loading MoreCompany.dll stub:");
                    Plugin.Log.LogError(e);
                }

                foreach (var pl in BepInEx.Bootstrap.Chainloader.PluginInfos)
                {
                    if (pl.Value.Instance != null)
                    {
                        CheckAssembly(pl.Value.Instance.GetType().Assembly);
                    }
                }
                
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                var assemblies = AppDomain.CurrentDomain.GetAssemblies();
                foreach (var assembly in assemblies)
                {
                    CheckAssemblyPatches(assembly);
                }
                Instance = this;
                BootManager.Boot();

                Log.LogDebug("Patching with harmony...");
                Harmony.CreateAndPatchAll(typeof(AdvancedCompany.Hooks.PlayerControllerB.KillPlayer), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Network.Manager), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(BulletProofVest), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(LightningRod), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(VisionEnhancer), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Flippers), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(LightShoes), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(BunnyEars), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(PietSmietController), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Game.Manager), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.AnimationPatches), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.FieldPatches), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.BalancingPatches), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.DebugPatches), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.EnemyAICollisionDetect), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.Flavour), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.GameNetworkManager), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.GrabbableObject), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.HUDManager), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.InventoryPatches), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.ItemDataHandling), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.ItemDropship), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.MaskedEnemyPatches), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.SavePatches), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Game.MobileTerminal), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Rocket), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(LobbySetup), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(ClientSetup), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(DeathScreen), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Game.Player), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.LobbySizePatches), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(PostProcessing.Volumes), "AdvancedCompany");
                Patches.LobbySizePatches.PatchAsync();
                Patches.FieldPatches.ApplyPatches();
                Harmony.CreateAndPatchAll(typeof(Patches.PlayerControllerB), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.RoundManager), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.StartOfRound), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.Terminal), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.TimeOfDay), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Patches.Compability), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(UI.Endscreen), "AdvancedCompany");
                Harmony.CreateAndPatchAll(typeof(Utils), "AdvancedCompany");

                Plugin.Log.LogInfo("Loading cosmetics...");
                foreach (var f in Directory.EnumerateFiles(BepInEx.Paths.PluginPath, "*.cosmetics", SearchOption.AllDirectories))
                {
                    var assetBundle = AssetBundle.LoadFromFile(f);
                    AdvancedCompany.Lib.Cosmetics.LoadCosmeticsFromBundle(assetBundle); 
                }
                foreach (var f in Directory.EnumerateFiles(BepInEx.Paths.PluginPath, "MoreCompany.dll", SearchOption.AllDirectories))
                    ExtractMoreCompanyAssets(f);

                foreach (var f in Directory.EnumerateFiles(BepInEx.Paths.PluginPath, ModpackConfig.Instance.LogoFilename.Value, SearchOption.AllDirectories))
                {
                    Log.LogInfo("Found a logo image at " + f);
                    try
                    {
                        Texture2D t = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                        t.LoadImage(File.ReadAllBytes(f));
                        Sprite s = Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f));
                        Flavour.OverrideLogo = s;
                        Plugin.Log.LogInfo("Successfully set logo!");
                    }
                    catch (Exception e)
                    {
                        Plugin.Log.LogError("Error while loading logo:");
                        Plugin.Log.LogError(e);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.LogError("Error while booting!");
                Log.LogError(ex);
            }
        }

        private static void ExtractMoreCompanyAssets(string fileName)
        {
            Plugin.Log.LogMessage("Found MoreCompany.dll. Extracting assets.");
            AssemblyDefinition def = AssemblyDefinition.ReadAssembly(fileName);
            foreach (var resource in def.MainModule.Resources)
            {
                if (resource is EmbeddedResource res && res.Name == "MoreCompany.Resources.morecompany.cosmetics")
                {
                    Plugin.Log.LogMessage("Found cosmetics. Loading them.");
                    AssetBundle assets = AssetBundle.LoadFromMemory(res.GetResourceData());
                    AdvancedCompany.Lib.Cosmetics.LoadCosmeticsFromBundle(assets);
                }
            }
        }

        private static List<string> IgnoreAssemblies = new List<string>()
        {
            "System",
            "Facepunch",
            "Dissonance",
            "ClientNetworkTransform",
            "Newtonsoft",
            "netstandard",
            "UnityEngine",
            "eval",
            "Unity.",
            "HarmonyDTF",
            "mscorlib",
            "Mono",
            "Assembly-CSharp",
            "UniverseLib",
            "AmazingAssets",
            "MMHOOK",
        };
        private void CheckAssembly(Assembly assembly)
        {
            foreach (var ignore in IgnoreAssemblies)
            {
                if (assembly.FullName.StartsWith(ignore))
                    return;
            }
            if (!AssembliesToScan.Contains(assembly))
            {
                var attr = assembly.GetCustomAttributes<AssemblyMetadataAttribute>();
                bool ignore = false;
                //if (assembly.FullName.Contains("LethalLevelLoader")) ignore = true;
                if (!ignore)
                {
                    foreach (var a in attr)
                    {
                        if (a.Key == "AdvancedCompanyIgnore" && a.Value.ToLowerInvariant() == "true")
                        {
                            ignore = true;
                            break;
                        }
                    }
                }
                if (!ignore)
                {
                    AssembliesToScan.Add(assembly);
                    Plugin.Log.LogInfo("Added " + assembly.FullName + " to assemblies to scan.");
                }
                else
                {
                    Plugin.Log.LogWarning("Assembly " + assembly.FullName + " defined AdvancedCompanyIgnore metadata. This mod might lead to config overrides not working. Be cautious about override errors.");
                }
            }
            CheckAssemblyPatches(assembly);
        }

        private void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            CheckAssembly(args.LoadedAssembly);
        }

        private static void CheckAssemblyPatches(Assembly assembly)
        {
            if (assembly.FullName.StartsWith("FuckYouMod"))
            {
                PatchMoreEmotes(assembly);
            }
        }

        private static bool Deactivate()
        {
            return false;
        }

        private static void PatchMoreEmotes(Assembly assembly)
        {
            UseAnimationOverride = true;
        }

        public static bool ShouldSaveOutfits()
        {
            return ServerConfiguration.Instance?.General?.SaveSuitsAfterDeath ?? false;
        }
    }
}