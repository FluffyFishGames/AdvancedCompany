using AdvancedCompany.Config;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using static AdvancedCompany.Config.LobbyConfiguration;

namespace AdvancedCompany.Unity.Moons
{
    internal class ScrapItem : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public UnityEngine.UI.Toggle OverrideRarityToggle;
        public TMPro.TMP_InputField RarityField;
        public UnityEngine.UI.Button ResetButton;
        private LobbyConfiguration.MoonConfig.LootTableItem Config;

        void Awake()
        {
            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
            OverrideRarityToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Config != null)
                {
                    Config.Override = val;
                    RarityField.interactable = val;
                    RarityField.SetTextWithoutNotify(Config.Override ? Config.Rarity.ToString() : Config.Default(nameof(Config.Rarity)).ToString());
                    RarityField.textComponent.color = RarityField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            RarityField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Config.Rarity = @int;
                }
            }));
            ResetButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Config.Override = false;
                Config.Reset(nameof(Config.Rarity));
                OverrideRarityToggle.SetIsOnWithoutNotify(false);
                RarityField.interactable = Config.Override;
                RarityField.SetTextWithoutNotify(Config.Override ? Config.Rarity.ToString() : Config.Default(nameof(Config.Rarity)).ToString());
                RarityField.textComponent.color = RarityField.interactable ? activeTextColor : inactiveTextColor;
            }));
        }

        public void SetValue(LobbyConfiguration.MoonConfig.LootTableItem itemConfig)
        {
            Config = itemConfig;
            UpdateValues();
        }

        public void UpdateValues()
        {
            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
            OverrideRarityToggle.SetIsOnWithoutNotify(Config.Override);
            RarityField.interactable = Config.Override;
            RarityField.SetTextWithoutNotify(Config.Override ? Config.Rarity.ToString() : Config.Default(nameof(Config.Rarity)).ToString());
            RarityField.textComponent.color = RarityField.interactable ? activeTextColor : inactiveTextColor;
        }

        public void Initialize(string label)
        {
            Label.text = label;
        }
    }
}
