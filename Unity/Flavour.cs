using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Unity
{
    public class Flavour : MonoBehaviour
    {
        internal static Sprite OriginalLogo;
        internal static Image HeaderImage;
        internal static Image LoadImage;
        internal static TextMeshProUGUI SimplifiedCopyright;

        void LateUpdate()
        {
            /*var logo = ClientConfiguration.Instance.Graphics.ShowOriginalLogo ? OriginalLogo : (Lib.Flavour.OverrideLogo != null ? Lib.Flavour.OverrideLogo : OriginalLogo);
            if (HeaderImage != null && HeaderImage.sprite != logo)
                HeaderImage.sprite = logo;
            if (LoadImage != null && LoadImage.sprite != logo)
                LoadImage.sprite = logo;*/
        }

        void Awake()
        {
            if (HeaderImage == null)
            {
                HeaderImage = GameObject.Find("HeaderImage")?.GetComponent<Image>() ?? null;

                if (HeaderImage != null)
                {
                    var tmp = HeaderImage.transform.parent.GetComponentInChildren<TextMeshProUGUI>();
                    if (tmp != null)
                    {
                        var newTextObject = new GameObject("PoweredBy");
                        newTextObject.transform.parent = HeaderImage.transform.parent.parent;
                        newTextObject.transform.rotation = Quaternion.identity;
                        newTextObject.transform.localScale = Vector3.one;
                        var newTextRect = newTextObject.AddComponent<RectTransform>();
                        newTextRect.anchoredPosition3D = new Vector3(0f, 0f, 0f);
                        newTextRect.pivot = new Vector2(0f, 0f);
                        newTextRect.anchorMin = new Vector2(0f, 1f);
                        newTextRect.anchorMax = new Vector2(1f, 1f);
                        newTextRect.offsetMin = new Vector2(0f, -10f);
                        newTextRect.offsetMax = new Vector2(-25f, 0f);
                        var newText = newTextObject.AddComponent<TextMeshProUGUI>();
                        newText.font = tmp.font;
                        newText.fontSize = 10;
                        newText.text = "Powered by AdvancedCompany";
                        newText.alignment = TextAlignmentOptions.Right;
                        SimplifiedCopyright = newText;
                    }
                    OriginalLogo = HeaderImage.sprite;
                }
            }
            if (LoadImage == null)
            {
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
                                LoadImage = c2.GetComponent<Image>();
                            }
                        }
                        break;
                    }
                }
            }
        }
    }
}