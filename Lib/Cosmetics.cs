using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Lib
{
    public class Cosmetics
    {
        internal static void LoadNew()
        {
            foreach (var assetBundle in MoreCompany.Cosmetics.CosmeticRegistry.ToLoad)
                LoadCosmeticsFromBundle(assetBundle);
            MoreCompany.Cosmetics.CosmeticRegistry.ToLoad.Clear();
            foreach (var assetBundle in MoreCompany.Cosmetics.CosmeticGeneric.ToLoad)
                LoadCosmeticsFromPrefab(assetBundle);
            MoreCompany.Cosmetics.CosmeticGeneric.ToLoad.Clear();
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
