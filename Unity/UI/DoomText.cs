using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Unity.UI
{
    [ExecuteInEditMode]
    [RequireComponent(typeof(HorizontalLayoutGroup))]
    public class DoomText : MonoBehaviour
    {
        protected HorizontalLayoutGroup LayoutGroup;
        protected RectTransform RectTransform;
        private string LastText = "";

        public DoomFont Font;
        public string Text = "";
        public float Spacing = 0f;
        public bool Shadow = false;
        public Color ShadowColor;

        public Color Color;
        protected List<Image> Images = new List<Image>();
        protected List<Image> Shadows = new List<Image>();
        protected List<RectTransform> Containers = new List<RectTransform>();

        void Update()
        {
            if (Text == LastText || Font == null)
                return;
            if (RectTransform == null)
                RectTransform = GetComponent<RectTransform>();
            if (LayoutGroup == null)
                LayoutGroup = GetComponent<HorizontalLayoutGroup>();
            
            for (var i = 0; i < Text.Length; i++)
            {
                if (i >= Containers.Count || i >= Images.Count || i >= Shadows.Count || Images[i] == null || Shadows[i] == null || Containers[i] == null)
                {
                    Transform child = null;
                    if (transform.childCount > i)
                        child = transform.GetChild(i);
                    else
                    {
                        var container = new GameObject("Letter" + i);
                        var rect = container.AddComponent<RectTransform>();
                        rect.SetParent(RectTransform);
                        rect.localScale = Vector3.one;
                        rect.anchoredPosition3D = Vector3.zero;
                        child = rect.transform;
                    }
                    child.name = "Letter" + i;

                    if (i >= Containers.Count)
                        Containers.Add(child.GetComponent<RectTransform>());
                    else
                        Containers[i] = child.GetComponent<RectTransform>();

                    Transform firstChild = null;
                    if (child.childCount > 0)
                        firstChild = child.GetChild(0);
                    else
                    {
                        var container = new GameObject("Shadow");
                        var rect = container.AddComponent<RectTransform>();
                        rect.SetParent(child);
                        rect.localScale = Vector3.one;
                        rect.anchoredPosition3D = Vector3.zero;
                        firstChild = rect.transform;
                    }
                    Transform secondChild = null;
                    if (child.childCount > 1)
                        secondChild = child.GetChild(1);
                    else
                    {
                        var container = new GameObject("Face");
                        var rect = container.AddComponent<RectTransform>();
                        rect.SetParent(child);
                        rect.localScale = Vector3.one;
                        rect.anchoredPosition3D = Vector3.zero;
                        secondChild = rect.transform;
                    }

                    var shadowImage = firstChild.gameObject.GetComponent<Image>();
                    if (shadowImage == null)
                        shadowImage = firstChild.gameObject.AddComponent<Image>();

                    var faceImage = secondChild.gameObject.GetComponent<Image>();
                    if (faceImage == null)
                        faceImage = secondChild.gameObject.AddComponent<Image>();

                    if (i >= Images.Count)
                        Images.Add(faceImage);
                    else
                        Images[i] = faceImage;

                    if (i >= Shadows.Count)
                        Shadows.Add(shadowImage);
                    else
                        Shadows[i] = shadowImage;
                }
            }
            for (var i = Containers.Count - 1; i >= Text.Length; i--)
            {
                if (Containers[i] != null)
                    GameObject.DestroyImmediate(Containers[i].gameObject);
            }
            if (Containers.Count > Text.Length)
                Containers.RemoveRange(Text.Length, Containers.Count - Text.Length);
            if (Images.Count > Text.Length)
                Images.RemoveRange(Text.Length, Images.Count - Text.Length);
            if (Shadows.Count > Text.Length)
                Shadows.RemoveRange(Text.Length, Shadows.Count - Text.Length);
            LayoutGroup.spacing = Spacing;
            for (var i = 0; i < Containers.Count; i++)
            {
                if (i < Text.Length)
                {
                    if (!Containers[i].gameObject.activeSelf)
                        Containers[i].gameObject.SetActive(true);
                    Images[i].sprite = Font.GetSprite(Text[i]);

                    var size = new Vector2((Images[i].sprite.rect.width / (float)Font.CharHeight) * RectTransform.rect.height, RectTransform.rect.height * (Images[i].sprite.rect.height / (float)Font.CharHeight));
                    Containers[i].sizeDelta = size;
                    Images[i].rectTransform.sizeDelta = size;
                    Images[i].color = Color;
                    if (Shadow)
                    {
                        if (!Shadows[i].gameObject.activeSelf)
                            Shadows[i].gameObject.SetActive(true);
                        Shadows[i].sprite = Images[i].sprite;
                        Shadows[i].rectTransform.sizeDelta = size;
                        Shadows[i].rectTransform.anchorMax = new Vector3(0f, 1f);
                        Shadows[i].rectTransform.anchorMin = new Vector3(0f, 1f);
                        Shadows[i].rectTransform.pivot = new Vector3(0f, 1f);
                        Shadows[i].color = ShadowColor;
                        Shadows[i].rectTransform.anchoredPosition = new Vector3(Spacing, -Spacing, 0);
                    }
                    else
                    {
                        if (Shadows[i].gameObject.activeSelf)
                            Shadows[i].gameObject.SetActive(false);
                    }
                }
                else if (Containers[i] != null && Containers[i].gameObject.activeSelf)
                {
                    Containers[i].gameObject.SetActive(false);
                }
            }
        }
    }
}
