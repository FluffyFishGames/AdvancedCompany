using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Unity.UI
{
    [ExecuteInEditMode]
    public class DoomDigits : MonoBehaviour
    {
        public RectTransform LayoutGroup;
        private int LastNumber = -1;
        public int Number = 0;
        public bool AddPercentage = false;
        public bool Reverse = false;
        public Color Color;

        public Image[] Images;
        [Header("0-9%")]
        public Sprite[] Sprites;

        void Update()
        {
            if (Number == LastNumber)
                return;
            if (Sprites.Length < 11)
                return;
            var tex = Number.ToString();
            if (AddPercentage) tex += "%";
            for (var i = 0; i < Images.Length; i++)
            {
                if (tex.Length > i)
                {
                    var c = tex[Reverse ? tex.Length - 1 - i : i];
                    Images[i].enabled = true;
                    Images[i].color = Color;
                    if (c == '0') Images[i].sprite = Sprites[0];
                    else if (c == '1') Images[i].sprite = Sprites[1];
                    else if (c == '2') Images[i].sprite = Sprites[2];
                    else if (c == '3') Images[i].sprite = Sprites[3];
                    else if (c == '4') Images[i].sprite = Sprites[4];
                    else if (c == '5') Images[i].sprite = Sprites[5];
                    else if (c == '6') Images[i].sprite = Sprites[6];
                    else if (c == '7') Images[i].sprite = Sprites[7];
                    else if (c == '8') Images[i].sprite = Sprites[8];
                    else if (c == '9') Images[i].sprite = Sprites[9];
                    else if (c == '%') Images[i].sprite = Sprites[10];
                    Images[i].rectTransform.sizeDelta = new Vector2((Images[i].sprite.rect.width / Images[i].sprite.rect.height) * LayoutGroup.rect.height, LayoutGroup.rect.height);
                }
                else
                    Images[i].enabled = false;
            }
            LastNumber = Number;
        }
    }
}
