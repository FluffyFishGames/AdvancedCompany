using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Lib
{
    public class Cosmetics
    {
        internal static void LoadNew()
        {
            // an attempt to fix wrongfully loaded MoreCompany.dll...
            var types = Plugin.MoreCompanyAssembly.GetTypes();
            // for debug purposes
            bool foundField1 = false;
            bool foundField2 = false;
            foreach (var t in types)
            {
                if (t.FullName == "MoreCompany.Cosmetics.CosmeticRegistry" || t.FullName == "MoreCompany.Cosmetics.CosmeticGeneric")
                {
                    var fields = t.GetFields();
                    foreach (var f in fields)
                    {
                        if (f.Name == "ToLoad")
                        {
                            var val = f.GetValue(null);
                            if (val is ObservableCollection<AssetBundle> aval)
                            {
                                foundField1 = true;
                                foreach (var v in aval)
                                    LoadCosmeticsFromBundle(v);
                                aval.Clear();
                            }
                            else if (val is ObservableCollection<GameObject> gval)
                            {
                                foundField2 = true;
                                foreach (var v in gval)
                                    LoadCosmeticsFromPrefab(v);
                                gval.Clear();
                            }
                        }
                    }
                }
            }

            Plugin.Log.LogDebug("Cosmetics field1: " + foundField1);
            Plugin.Log.LogDebug("Cosmetics field2: " + foundField2);

            //foreach (var assetBundle in MoreCompany.Cosmetics.CosmeticRegistry.ToLoad)
            //    LoadCosmeticsFromBundle(assetBundle);
            //MoreCompany.Cosmetics.CosmeticRegistry.ToLoad.Clear();
            //foreach (var assetBundle in MoreCompany.Cosmetics.CosmeticGeneric.ToLoad)
            //    LoadCosmeticsFromPrefab(assetBundle);
            //MoreCompany.Cosmetics.CosmeticGeneric.ToLoad.Clear();
        }

        public static GameObject[] GetSpawnedCosmetics(GameNetcodeStuff.PlayerControllerB controller)
        {
            var player = Game.Player.GetPlayer((int) controller.playerClientId);
            return player.AppliedCosmetics.Values.ToArray();
        }

        public static void LoadCosmeticsFromPrefab(GameObject prefab)
        {
            prefab.hideFlags = HideFlags.DontUnloadUnusedAsset;

            var advComp = prefab.GetComponent<AdvancedCompany.Cosmetics.CosmeticInstance>();
            if (advComp == null)
            {
                var comp = prefab.GetComponent<MoreCompany.Cosmetics.CosmeticInstance>();
                if (comp != null)
                {
                    advComp = prefab.AddComponent<AdvancedCompany.Cosmetics.CosmeticInstance>();
                    advComp.cosmeticId = comp.cosmeticId;
                    advComp.cosmeticType = Enum.Parse<AdvancedCompany.Cosmetics.CosmeticType>(Enum.GetName(typeof(MoreCompany.Cosmetics.CosmeticType), comp.cosmeticType));
                    advComp.icon = comp.icon;
                    Component.Destroy(comp);
                }
                else
                {
                    Plugin.Log.LogWarning("Error while loading cosmetic " + prefab.name + ". No AdvancedCompany.Cosmetics.CosmeticInstance nor MoreCompany.Cosmetics.CosmeticInstance were present.");
                }
            }
            if (advComp != null)
            {
                CosmeticDatabase.AddCosmetic(advComp);
            }
        }

        public static void LoadCosmeticsFromBundle(AssetBundle bundle)
        {
            if (bundle != null)
            {
                var names = bundle.GetAllAssetNames();
                for (var i = 0; i < names.Length; i++)
                {
                    var name = names[i];
                    if (name.EndsWith(".prefab"))
                    {
                        GameObject go = bundle.LoadAsset<GameObject>(name);
                        LoadCosmeticsFromPrefab(go);
                    }
                }
            }
        }
    }
}
