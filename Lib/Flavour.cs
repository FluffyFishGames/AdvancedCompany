using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Lib
{
    public class Flavour
    {
        internal static Sprite OverrideLogo;
        public static void SetLogo(Texture2D logo)
        {
            OverrideLogo = Sprite.Create(logo, new Rect(0f, 0f, logo.width, logo.height), new Vector2(0.5f, 0.5f));
        }
    }
}
