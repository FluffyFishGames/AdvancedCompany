using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace AdvancedCompany.Unity.UI
{
    [CreateAssetMenu(menuName = "DoomFont", order = 2, fileName = "DoomFont")]
    public class DoomFont : ScriptableObject
    {
        public List<char> Chars;
        public List<Sprite> Sprites;
        public int CharHeight;

        public Sprite GetSprite(char c)
        {
            var ind = Chars.IndexOf(c);
            if (ind > -1)
                return Sprites[ind];
            return null;
        }
    }
}
