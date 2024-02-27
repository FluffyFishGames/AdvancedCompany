using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Objects
{
    [LoadAssets]
    internal class Loader
    {
        public static void LoadAssets(AssetBundle assets)
        {
            Plugin.Log.LogInfo("Adding items...");
            try
            {
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/LightningRod.prefab").AddComponent<LightningRod>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/RocketBoots.prefab").AddComponent<RocketBoots>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/MissileLauncher.prefab").AddComponent<MissileLauncher>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/Flippers.prefab").AddComponent<Flippers>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/VisionEnhancer.prefab").AddComponent<VisionEnhancer>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/BulletproofVest.prefab").AddComponent<BulletProofVest>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/HelmetLamp.prefab").AddComponent<HelmetLamp>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/Headset.prefab").AddComponent<Headset>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Items/TacticalHelmet.prefab").AddComponent<TacticalHelmet>());

                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/LightShoes.prefab").AddComponent<LightShoes>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/BunnyEars.prefab").AddComponent<BunnyEars>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/Potatoes.prefab").AddComponent<PhysicsProp>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/ToyCar.prefab").AddComponent<PhysicsProp>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/ToyTank.prefab").AddComponent<PhysicsProp>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/WarplaneToy.prefab").AddComponent<PhysicsProp>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/Skillet.prefab").AddComponent<PhysicsProp>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/PietSmietController.prefab").AddComponent<PietSmietController>());
                Game.Manager.AddItem(assets.LoadAsset<GameObject>("Assets/Prefabs/Scrap/QuestionMarkBlock.prefab").AddComponent<PhysicsProp>());

                Plugin.Log.LogInfo("Items added...");
            }
            catch (Exception e)
            {
                Plugin.Log.LogError("Error while adding items!");
                Plugin.Log.LogError(e);
            }
        }
    }
}
