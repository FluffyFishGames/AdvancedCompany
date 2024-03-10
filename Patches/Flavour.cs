using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Patches
{
    [HarmonyPatch]
    [LoadAssets]
    internal class Flavour
    {
        private static Sprite Logo;
        private static Sprite PoweredByLogo;
        private static Image HeaderImage;
        private static Image LoadImage;
        private static Image HeaderPoweredByImage;
        private static Image LoadPoweredByImage;
        private static float PoweredByPhase = 0f;
        private static string RandomChars = "abcdefghjijkllmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        private static bool FoundHeader = false;
        private static bool FoundLoad = false;

        private static string GenerateRandomString(int length = 8)
        {
            var ret = "";
            var r = new System.Random();
            for (var i = 0; i < length; i++)
            {
                ret += RandomChars[r.Next(RandomChars.Length - 1)];
            }
            return ret;
        }

        [HarmonyPatch(typeof(MenuManager), "Update")]
        [HarmonyPostfix]
        public static void Update()
        {
            if (HeaderImage == null)
            {
                HeaderImage = GameObject.Find("HeaderImage")?.GetComponent<Image>() ?? null;
                if (HeaderImage != null && HeaderPoweredByImage == null)
                {
                    HeaderImage.name = GenerateRandomString(11);
                    var newChild = new GameObject(GenerateRandomString(7));
                    newChild.transform.parent = HeaderImage.transform;
                    newChild.SetActive(Lib.Flavour.OverrideLogo != null);
                    var rectTransform = newChild.AddComponent<RectTransform>();
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.anchorMin = new Vector2(1f, 1f);
                    var s = 0.4f;
                    rectTransform.sizeDelta = new Vector2(545f * s, 125f * s);
                    rectTransform.anchoredPosition = new Vector2(38f, -32f);
                    HeaderPoweredByImage = newChild.AddComponent<Image>();
                    HeaderPoweredByImage.sprite = PoweredByLogo;
                    FoundHeader = true;
                }
            }
            if (LoadImage == null)
            {
//                foreach (var k in BepInEx.Bootstrap.Chainloader.PluginInfos)
//                    Plugin.Log.LogMessage(k.Key);
                var container = GameObject.Find("MenuContainer");
                for (var i = 0; i < container.transform.childCount; i++)
                {
                    var c = container.transform.GetChild(i);
                    if (c.name == "LoadingScreen")
                    {
                        for (var j = 0; j < c.transform.childCount; j++)
                        {
                            var c2 = c.transform.GetChild(j);
                            if (c2.name == "Image")
                            {
                                c2.name = GenerateRandomString(12);
                                LoadImage = c2.GetComponent<Image>();
                            }
                        }
                        break;
                    }
                }
                if (LoadImage != null && LoadPoweredByImage == null)
                {
                    var newChild = new GameObject(GenerateRandomString(9));
                    newChild.transform.parent = LoadImage.transform;
                    newChild.SetActive(Lib.Flavour.OverrideLogo != null);
                    var rectTransform = newChild.AddComponent<RectTransform>();
                    rectTransform.pivot = new Vector2(0.5f, 0.5f);
                    rectTransform.anchorMax = new Vector2(1f, 1f);
                    rectTransform.anchorMin = new Vector2(1f, 1f);
                    var s = 0.4f;
                    rectTransform.sizeDelta = new Vector2(545f * s, 125f * s);
                    rectTransform.anchoredPosition = new Vector2(38f, -32f);
                    LoadPoweredByImage = newChild.AddComponent<Image>();
                    LoadPoweredByImage.sprite = PoweredByLogo;
                    FoundLoad = true;
                }
            }
            if ((LoadImage == null && FoundLoad) || (HeaderImage == null && FoundHeader))
                Plugin.Cripple = true;

            bool overridden = Lib.Flavour.OverrideLogo != null;
            if (overridden && LoadPoweredByImage != null && !LoadPoweredByImage.gameObject.activeSelf)
                LoadPoweredByImage.gameObject.SetActive(true);
            if (overridden && HeaderPoweredByImage != null && !HeaderPoweredByImage.gameObject.activeSelf)
                HeaderPoweredByImage.gameObject.SetActive(true);
            
            var logo = Lib.Flavour.OverrideLogo != null ? Lib.Flavour.OverrideLogo : Logo;
            if (HeaderImage != null && HeaderImage.sprite != logo)
            {
                HeaderImage.sprite = logo;
            }
            if (LoadImage != null && LoadImage.sprite != Logo)
            {
                LoadImage.sprite = logo;
            }

            PoweredByPhase += Time.deltaTime;
            var scale = ((Mathf.Sin(PoweredByPhase * Mathf.PI * .5f) + 1f) / 45f) + 0.5f;
            var rotation = (((Mathf.Cos(PoweredByPhase * Mathf.PI / 2) + 1f * .5f) - 0.25f) * 0.01f);
            if (HeaderPoweredByImage != null)
            {
                HeaderPoweredByImage.transform.rotation = Quaternion.Euler(0f, 0f, rotation * 360f);
                HeaderPoweredByImage.transform.localScale = new Vector3(scale, scale, scale);
                HeaderPoweredByImage.enabled = true;
                HeaderPoweredByImage.color = Color.white;
                var r = HeaderPoweredByImage.GetComponent<RectTransform>();
                r.pivot = new Vector2(0.5f, 0.5f);
                r.anchorMax = new Vector2(1f, 1f);
                r.anchorMin = new Vector2(1f, 1f);
                var s = 0.4f;
                r.sizeDelta = new Vector2(545f * s, 125f * s);
                r.anchoredPosition = new Vector2(38f, -32f);
                if (HeaderPoweredByImage.transform.parent != HeaderImage.transform)
                    HeaderPoweredByImage.transform.parent = HeaderImage.transform;
            }
            else if (FoundHeader) Plugin.Cripple = true;
            if (LoadPoweredByImage != null)
            {
                LoadPoweredByImage.transform.rotation = Quaternion.Euler(0f, 0f, rotation * 360f);
                LoadPoweredByImage.transform.localScale = new Vector3(scale, scale, scale);
                LoadPoweredByImage.enabled = true;
                LoadPoweredByImage.color = Color.white;
                var r = LoadPoweredByImage.GetComponent<RectTransform>();
                r.pivot = new Vector2(0.5f, 0.5f);
                r.anchorMax = new Vector2(1f, 1f);
                r.anchorMin = new Vector2(1f, 1f);
                var s = 0.4f;
                r.sizeDelta = new Vector2(545f * s, 125f * s);
                r.anchoredPosition = new Vector2(38f, -32f);
                if (LoadPoweredByImage.transform.parent != LoadImage.transform)
                    LoadPoweredByImage.transform.parent = LoadImage.transform;
            }
            else if (FoundLoad) Plugin.Cripple = true;
        }

        public static void LoadAssets(AssetBundle assets)
        {
            Logo = assets.LoadAsset<Sprite>("Assets/Icons/Logo.png");
            PoweredByLogo = assets.LoadAsset<Sprite>("Assets/Icons/SmallLogo.png");
        }
    }
}
