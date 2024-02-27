using AdvancedCompany.Config;
using AdvancedCompany.Network;
using AdvancedCompany.Network.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Game
{
    internal partial class Player
    {
        internal List<Light> CosmeticLights = new List<Light>();
        internal void ShowCosmeticLights()
        {
            foreach (var light in CosmeticLights)
                light.enabled = true;
        }

        internal void HideCosmeticLights()
        {
            foreach (var light in CosmeticLights)
                light.enabled = false;
        }

        public void EquipConfigurationCosmetics()
        {
            SetCosmetics(ClientConfiguration.Instance.Cosmetics.ActivatedCosmetics.ToArray(), false);
        }

        private bool CosmeticsHidden = false;
        public void ShowCosmetics()
        {
            CosmeticsHidden = false;
            foreach (var cosmetic in AppliedCosmetics)
                cosmetic.Value.SetActive(true);
        }

        public void HideCosmetics()
        {
            CosmeticsHidden = true;
            foreach (var cosmetic in AppliedCosmetics)
                cosmetic.Value.SetActive(false);
        }

        public string[] Cosmetics;
        public Dictionary<string, GameObject> AppliedCosmetics = new Dictionary<string, GameObject>();
        public void SetCosmetics(string[] cosmetics, bool sendMessage = true)
        {
            if (ServerConfiguration.Instance.General.EnableCosmetics)
            {
                if (cosmetics == null)
                    cosmetics = new string[0];
                Cosmetics = cosmetics;

                if (Controller != null)
                {
                    if (Lobby.LocalPlayerNum == (int)Controller.playerClientId && sendMessage)
                    {
                        Network.Manager.Send(new CosmeticsSync() { PlayerNum = PlayerNum, Cosmetics = cosmetics });
                    }

                    if (!ClientConfiguration.Instance.Compability.HideCosmetics)
                    {
                        Plugin.Log.LogMessage("Applying cosmetics for " + PlayerNum);
                        Plugin.Log.LogMessage(string.Join(" ", Cosmetics));
                        var remove = new List<string>();
                        var add = new List<string>();
                        foreach (var kv in AppliedCosmetics)
                        {
                            if (!cosmetics.Contains(kv.Key))
                                remove.Add(kv.Key);
                        }
                        foreach (var k in Cosmetics)
                        {
                            if (!AppliedCosmetics.ContainsKey(k))
                                add.Add(k);
                        }
                        for (var i = 0; i < remove.Count; i++)
                        {
                            GameObject.Destroy(AppliedCosmetics[remove[i]]);
                            AppliedCosmetics.Remove(remove[i]);
                        }
                        for (var i = 0; i < add.Count; i++)
                        {
                            AddCosmetic(add[i]);
                        }

                        CosmeticLights.Clear();
                        foreach (var kv in AppliedCosmetics)
                        {
                            var lights = kv.Value.GetComponentsInChildren<Light>();
                            CosmeticLights.AddRange(lights);
                        }
                    }
                }
            }
            else Cosmetics = new string[0];
        }

        private void AddCosmetic(string id)
        {
            if (CosmeticDatabase.AllCosmetics.ContainsKey(id))
            {
                var cosmetic = GameObject.Instantiate(CosmeticDatabase.AllCosmetics[id].gameObject);
                var instance = cosmetic.GetComponent<AdvancedCompany.Cosmetics.CosmeticInstance>();

                Transform bone = null;
                switch (instance.cosmeticType)
                {
                    case AdvancedCompany.Cosmetics.CosmeticType.HAT:
                        bone = GetBone(Bone.SPINE_3);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.CHEST:
                        bone = GetBone(Bone.SPINE_2);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.HIP:
                        bone = GetBone(Bone.ROOT);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.R_LOWER_ARM:
                        bone = GetBone(Bone.R_LOWER_ARM);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.L_SHIN:
                        bone = GetBone(Bone.L_SHIN);
                        break;
                    case AdvancedCompany.Cosmetics.CosmeticType.R_SHIN:
                        bone = GetBone(Bone.R_SHIN);
                        break;
                }

                cosmetic.transform.position = bone.position;
                cosmetic.transform.rotation = bone.rotation;
                cosmetic.transform.localScale *= 0.38f;
                cosmetic.transform.parent = bone;
                if ((int)Controller.playerClientId == Lobby.LocalPlayerNum)
                {
                    var renderer = cosmetic.gameObject.GetComponent<MeshRenderer>();
                    if (renderer != null)
                        renderer.gameObject.layer = 23;
                    var transforms = cosmetic.gameObject.GetComponentsInChildren<Transform>();
                    for (var i = 0; i < transforms.Length; i++)
                        transforms[i].gameObject.layer = 23;
                }
                AppliedCosmetics.Add(id, cosmetic);
                if (CosmeticsHidden)
                    cosmetic.SetActive(false);
            }
            else
            {
                Plugin.Log.LogWarning("Player has unsupported cosmetic: " + id);
            }
        }

    }
}
