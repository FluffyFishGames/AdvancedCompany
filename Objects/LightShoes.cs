using GameNetcodeStuff;
using HarmonyLib;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;
using AdvancedCompany.Game;
using AdvancedCompany.Network.Messages;
using AdvancedCompany.Objects.Interfaces;
using AdvancedCompany.PostProcessing;
using AdvancedCompany.Config;

namespace AdvancedCompany.Objects
{
    [HarmonyPatch]
    [LoadAssets]
    internal class LightShoes : Boots, ICursed, IOnUpdate
    {
        private static GameObject LeftLightShoe;
        private static GameObject RightLightShoe;
        private static GameObject LeftRGB;
        private static GameObject RightRGB;
        private Light[] Lights;
        private float[] Hues;
        private float Phase = 0f;
        private Material Material;
        private AudioSource Audio;
        private bool LightsOn = true;
        private GameObject LeftRGBInstance;
        private GameObject RightRGBInstance;
        public bool CurseLifted = false;
        private float AttractEnemiesTimer = 0f;
        private GameNetcodeStuff.PlayerControllerB lastPlayerHeldBy;

        void Awake()
        {
            Phase = UnityEngine.Random.value;
            Audio = transform.Find("Audio").GetComponent<AudioSource>();
            Material = GetComponent<MeshRenderer>().materials[1];
            Lights = GetComponentsInChildren<Light>();
            Hues = new float[Lights.Length];
            for (var i = 0; i < Lights.Length; i++)
            {
                Color.RGBToHSV(Lights[i].color, out var h, out var s, out var v);
                Hues[i] = h;
            }
            if (!ClientConfiguration.Instance.Compability.DisableMusic)
                Audio.Play();
        }

        public override void EquipItem()
        {
            base.EquipItem();
        }

        private IEnumerator CurseEquip()
        {
            while (!this.CurseLifted && this.playerHeldBy != null && !this.playerHeldBy.isInHangarShipRoom)
            {
                if (this.playerHeldBy.grabbedObjectValidated && !this.IsEquipped())
                {
                    yield return new WaitForEndOfFrame();
                    yield return new WaitForEndOfFrame();
                    this.Equip();
                    break;
                }
                yield return new WaitForEndOfFrame();
            }
        }

        public override void GrabItem()
        {
            base.GrabItem();
            if (!this.CurseLifted && !this.playerHeldBy.isInHangarShipRoom && !this.IsEquipped())
            {
                StartCoroutine(CurseEquip());
            }
        }

        public override void Equipped(Player player)
        {
            base.Equipped(player);
            if (!CurseLifted)
            {
                if (!ClientConfiguration.Instance.Compability.DisableMusic)
                    Audio.Stop();
            }
        }

        public override void DiscardItem()
        {
            if (!CurseLifted)
            {
                if (!ClientConfiguration.Instance.Compability.DisableMusic)
                    Audio.Play();
            }
            base.DiscardItem();
        }

        public override bool CanUnequip()
        {
            return CurseLifted;
        }

        public override void Update()
        {
            base.Update();
            var lightsOn = (playerHeldBy != null && this.isHeld && playerHeldBy.currentItemSlot == GrabbableObjectAdditions.GetInventoryPosition(this)) || playerHeldBy == null;
            if (playerHeldBy != null && playerHeldBy.isInHangarShipRoom)
                CurseLifted = true;
            if (NetworkManager.Singleton.IsServer && (CurseLifted || playerHeldBy == null && lastPlayerHeldBy != playerHeldBy))
            {
                foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
                {
                    if (enemy.IsOwner && enemy.targetPlayer == lastPlayerHeldBy)
                    {
                        enemy.targetPlayer = null;
                        enemy.movingTowardsTargetPlayer = false;
                    }
                }
            }

            if (CurseLifted)
            {
                Material.SetFloat("_EmissiveExposureWeight", 2f);
                for (var i = 0; i < Lights.Length; i++)
                    Lights[i].enabled = false;
                if (Audio.isPlaying)
                    Audio.Stop();
                if (LeftRGBInstance != null)
                    LeftRGBInstance.GetComponent<LightShoeRGB>().CurseLifted();
                if (RightRGBInstance != null)
                    RightRGBInstance.GetComponent<LightShoeRGB>().CurseLifted();
            }
            else
            {
                if (Audio != null)
                    Audio.volume = ClientConfiguration.Instance.Graphics.MusicVolume;
                if (LeftRGBInstance != null)
                {
                    var shoes = LeftRGBInstance.GetComponent<LightShoeRGB>();
                    if (shoes != null && shoes.Audio != null)
                        shoes.Audio.volume = ClientConfiguration.Instance.Graphics.MusicVolume;
                }
                if (playerHeldBy != null)
                {
                    foreach (var enemy in RoundManager.Instance.SpawnedEnemies)
                    {
                        if (enemy.isOutside == !playerHeldBy.isInsideFactory && enemy.IsOwner)
                        {
                            enemy.StopSearch(enemy.currentSearch);
                            enemy.movingTowardsTargetPlayer = true;
                            enemy.targetPlayer = playerHeldBy;
                        }
                    }
                }

                if (lightsOn != LightsOn)
                {
                    for (var i = 0; i < Lights.Length; i++)
                    {
                        Lights[i].enabled = lightsOn;
                    }
                    LightsOn = lightsOn;
                }
                if (LightsOn)
                {
                    Phase += Time.deltaTime / 5f;
                    while (Phase > 1f)
                        Phase -= 1f;

                    Material.SetTextureOffset("_EmissiveColorMap", new Vector2(0f, Phase));
                    for (var i = 0; i < Lights.Length; i++)
                        Lights[i].color = Color.HSVToRGB((Hues[i] + Phase) % 1f, 1f, 1f);
                }
            }
            lastPlayerHeldBy = playerHeldBy;
        }

        public override Player.BodyLayers GetLayers()
        {
            return Player.BodyLayers.HIDE_FEET;
        }

        public override GameObject[] CreateWearable(Player player)
        {
            var leftBones = new Transform[] {
                player.GetBone(Player.Bone.L_TOE),
                player.GetBone(Player.Bone.L_HEEL),
                player.GetBone(Player.Bone.L_FOOT)
            };
            var rightBones = new Transform[] {
                player.GetBone(Player.Bone.R_TOE),
                player.GetBone(Player.Bone.R_HEEL),
                player.GetBone(Player.Bone.R_FOOT)
            };

            var metarig = player.GetBone(Player.Bone.METARIG);
            var left = GameObject.Instantiate(LeftLightShoe, metarig);

            left.GetComponent<SkinnedMeshRenderer>().rootBone = player.GetBone(Player.Bone.L_SHIN);
            left.GetComponent<SkinnedMeshRenderer>().bones = leftBones;

            var right = GameObject.Instantiate(RightLightShoe, metarig);
            right.GetComponent<SkinnedMeshRenderer>().rootBone = player.GetBone(Player.Bone.R_SHIN);
            right.GetComponent<SkinnedMeshRenderer>().bones = rightBones;

            if (player.IsLocal)
            {
                left.layer = 23;
                var t = left.GetComponentsInChildren<Transform>();
                foreach (var tt in t)
                    tt.gameObject.layer = 23;

                right.layer = 23;
                t = left.GetComponentsInChildren<Transform>();
                foreach (var tt in t)
                    tt.gameObject.layer = 23;
            }
            
            if (!CurseLifted)
            {
                LeftRGBInstance = GameObject.Instantiate(LeftRGB, player.GetBone(Player.Bone.L_FOOT));
                LeftRGBInstance.transform.localScale = Vector3.zero;
                LeftRGBInstance.transform.localPosition = Vector3.zero;
                var leftScript = LeftRGBInstance.AddComponent<LightShoeRGB>();
                leftScript.Material = left.GetComponent<SkinnedMeshRenderer>().materials[1];

                RightRGBInstance = GameObject.Instantiate(RightRGB, player.GetBone(Player.Bone.R_FOOT));
                RightRGBInstance.transform.localScale = Vector3.zero;
                RightRGBInstance.transform.localPosition = Vector3.zero;
                var rightScript = RightRGBInstance.AddComponent<LightShoeRGB>();
                rightScript.Material = right.GetComponent<SkinnedMeshRenderer>().materials[1];

                leftScript.Play();
                rightScript.Play();

                if (player.IsLocal)
                {
                    LeftRGBInstance.layer = 23;
                    RightRGBInstance.layer = 23;
                }
                return new GameObject[] { left, right, LeftRGBInstance, RightRGBInstance };
            }
            else 
                return new GameObject[] { left, right };
        }

        public static void LoadAssets(AssetBundle assets)
        {
            LeftLightShoe = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/LightShoeLeft.prefab");
            RightLightShoe = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/LightShoeRight.prefab");
            LeftRGB = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/RGBLeft.prefab");
            RightRGB = assets.LoadAsset<GameObject>("Assets/Prefabs/Skins/RGBRight.prefab");
        }

        [HarmonyPatch(typeof(GameNetcodeStuff.PlayerControllerB), "DiscardHeldObject")]
        [HarmonyPrefix]
        public static bool DiscardHeldObject(GameNetcodeStuff.PlayerControllerB __instance)
        {
            if (__instance.currentlyHeldObjectServer is LightShoes shoes)
            {
                if (!shoes.CurseLifted)
                    return false;
            }
            return true;
        }


        public override int GetItemDataToSave()
        {
            return CurseLifted ? 1 : 0;
        }

        public override void LoadItemSaveData(int saveData)
        {
            base.LoadItemSaveData(saveData);
            CurseLifted = saveData == 1;
        }

        public void OnUpdate(Player player)
        {
            if (!this.CurseLifted)
            {
                player.Controller.sprintMeter += Time.deltaTime * 2f;
            }
        }

        public void UpdateEffects(Player player)
        {
            if (!CurseLifted)
            {
                player.SetMultiplierOverride("WeightSpeed", 0f);
                player.SetMultiplierOverride("Weight", 0f);
                player.SetMultiplierOverride("SprintSpeed", 2f);
                player.SetMultiplierOverride("FallDamage", 10f);
                player.SetMultiplierOverride("JumpHeight", 1.5f);
                player.SetMultiplierOverride("SprintStamina", 100f);
            }
        }
    }
}
