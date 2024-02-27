using AdvancedCompany.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigWeatherInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public UnityEngine.UI.Toggle OverrideValueToggle;
        public TMPro.TMP_InputField ValueInput;
        public UnityEngine.UI.Button ResetValueButton;
        public UnityEngine.UI.Toggle OverrideAmountToggle;
        public TMPro.TMP_InputField AmountInput;
        public UnityEngine.UI.Button ResetAmountButton;
        private LobbyConfiguration.WeatherConfig Weather;

        // Start is called before the first frame update
        void Start()
        {
            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
            ValueInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Weather.ScrapValueMultiplier = @int / 100f;
            }));
            OverrideValueToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Weather.OverrideScrapValueMultiplier = val;
                UpdateValue();
            }));
            AmountInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Weather.ScrapAmountMultiplier = @int / 100f;
            }));
            OverrideAmountToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Weather.OverrideScrapAmountMultiplier = val;
                UpdateValue();
            }));

            ResetValueButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Weather.Reset(nameof(Weather.ScrapValueMultiplier));
                Weather.Reset(nameof(Weather.OverrideScrapValueMultiplier));
                UpdateValue();
            }));
            ResetAmountButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Weather.Reset(nameof(Weather.ScrapAmountMultiplier));
                Weather.Reset(nameof(Weather.OverrideScrapAmountMultiplier));
                UpdateValue();
            }));
        }

        public void UpdateValue()
        {
            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

            OverrideValueToggle.SetIsOnWithoutNotify(Weather.OverrideScrapValueMultiplier);
            ValueInput.SetTextWithoutNotify(Weather.OverrideScrapValueMultiplier ? Mathf.RoundToInt(Weather.ScrapValueMultiplier * 100f).ToString(System.Globalization.CultureInfo.InvariantCulture) : Mathf.RoundToInt(((float)Weather.Default(nameof(Weather.ScrapValueMultiplier))) * 100f).ToString(System.Globalization.CultureInfo.InvariantCulture));
            ValueInput.interactable = Weather.OverrideScrapValueMultiplier;
            ValueInput.textComponent.color = ValueInput.interactable ? activeTextColor : inactiveTextColor;

            OverrideAmountToggle.SetIsOnWithoutNotify(Weather.OverrideScrapAmountMultiplier);
            AmountInput.SetTextWithoutNotify(Weather.OverrideScrapAmountMultiplier ? Mathf.RoundToInt(Weather.ScrapAmountMultiplier * 100f).ToString(System.Globalization.CultureInfo.InvariantCulture) : Mathf.RoundToInt(((float)Weather.Default(nameof(Weather.ScrapAmountMultiplier))) * 100f).ToString(System.Globalization.CultureInfo.InvariantCulture));
            AmountInput.interactable = Weather.OverrideScrapAmountMultiplier;
            AmountInput.textComponent.color = AmountInput.interactable ? activeTextColor : inactiveTextColor;

        }

        public void SetValue(LobbyConfiguration.WeatherConfig weather)
        {
            Weather = weather;
            UpdateValue();
        }

        public void Initialize(string label)
        {
            Label.text = label + (label.EndsWith(":") ? "" : ":");
        }
    }
}
