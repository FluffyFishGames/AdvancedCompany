using AdvancedCompany.Config;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;

namespace AdvancedCompany.Unity.Moons
{
    internal class MoonItem : MonoBehaviour
    {
        public UnityEngine.UI.Image ActiveBackground;
        public TextMeshProUGUI Label;
        public UnityEngine.UI.Button Button;
        public LobbyConfiguration.MoonConfig Config;

        public void Initialize(string label)
        {
            Label.text = label;
        }

        public void SetValue(LobbyConfiguration.MoonConfig config)
        {
            Config = config;
        }

        public void Select()
        {
            ActiveBackground.gameObject.SetActive(true);
        }

        public void Unselect()
        {
            ActiveBackground.gameObject.SetActive(false);
        }
    }
}
