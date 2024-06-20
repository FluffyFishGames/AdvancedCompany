using AdvancedCompany.Config;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigEnemyInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public Toggle ActiveInput;
        public Toggle OverridePowerLevelToggle;
        public TMPro.TMP_InputField PowerLevelInput;
        public Button ResetPowerLevelButton;
        private LobbyConfiguration.EnemyConfig Item;

        void Start()
        {
            ActiveInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) =>
            {
                Item.Active = val;
            }));
            PowerLevelInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float @int))
                    Item.PowerLevel = @int;
            }));

            OverridePowerLevelToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Item.OverridePowerLevel = val;
                UpdateValue();
            }));
            ResetPowerLevelButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Item.Reset(nameof(Item.PowerLevel));
                Item.Reset(nameof(Item.OverridePowerLevel));
                UpdateValue();
            }));
        }

        public void UpdateValue()
        {
            ActiveInput.SetIsOnWithoutNotify(Item.Active);

            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
            OverridePowerLevelToggle.SetIsOnWithoutNotify(Item.OverridePowerLevel);
            PowerLevelInput.SetTextWithoutNotify(Item.OverridePowerLevel ? Item.PowerLevel.ToString(System.Globalization.CultureInfo.InvariantCulture) : ((float)Item.Default(nameof(Item.PowerLevel))).ToString(System.Globalization.CultureInfo.InvariantCulture));
            PowerLevelInput.interactable = Item.OverridePowerLevel;
            PowerLevelInput.textComponent.color = PowerLevelInput.interactable ? activeTextColor : inactiveTextColor;
        }

        public void SetValue(LobbyConfiguration.EnemyConfig config)
        {
            Item = config;
            UpdateValue();
        }

        public void Initialize(string label)
        {
            Label.text = label + (label.EndsWith(":") ? "" : ":");
        }
    }
}
