using AdvancedCompany.Config;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigScrapInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public Toggle ActiveInput;
        public Toggle OverrideMinValueToggle;
        public TMPro.TMP_InputField MinValueInput;
        public Button ResetMinValueButton;
        public Toggle OverrideWeightToggle;
        public TMPro.TMP_InputField WeightInput;
        public Button ResetWeightButton;
        public Toggle OverrideMaxValueToggle;
        public TMPro.TMP_InputField MaxValueInput;
        public Button ResetMaxValueButton;
        private LobbyConfiguration.ScrapConfig Item;

        void Start()
        {
            ActiveInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) =>
            {
                Item.Active = val;
            }));
            WeightInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Item.Weight = 1f + @int / 105f;
            }));
            MinValueInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Item.MinValue = @int;
            }));
            MaxValueInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Item.MaxValue = @int;
            }));

            OverrideWeightToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Item.OverrideWeight = val;
                UpdateValue();
            }));
            OverrideMinValueToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Item.OverrideMinValue = val;
                UpdateValue();
            }));
            OverrideMaxValueToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Item.OverrideMaxValue = val;
                UpdateValue();
            }));
            ResetWeightButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Item.Reset(nameof(Item.Weight));
                Item.Reset(nameof(Item.OverrideWeight));
                UpdateValue();
            }));
            ResetMinValueButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Item.Reset(nameof(Item.MinValue));
                Item.Reset(nameof(Item.OverrideMinValue));
                UpdateValue();
            }));
            ResetMaxValueButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Item.Reset(nameof(Item.MaxValue));
                Item.Reset(nameof(Item.MaxValue));
                UpdateValue();
            }));
        }

        public void UpdateValue()
        {
            ActiveInput.SetIsOnWithoutNotify(Item.Active);

            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

            OverrideWeightToggle.SetIsOnWithoutNotify(Item.OverrideWeight);
            WeightInput.SetTextWithoutNotify(Item.OverrideWeight ? Mathf.RoundToInt((Item.Weight - 1f) * 105f).ToString(System.Globalization.CultureInfo.InvariantCulture) : Mathf.RoundToInt((((float)Item.Default(nameof(Item.Weight))) - 1f) * 105f).ToString(System.Globalization.CultureInfo.InvariantCulture));
            WeightInput.interactable = Item.OverrideWeight;
            WeightInput.textComponent.color = WeightInput.interactable ? activeTextColor : inactiveTextColor;

            OverrideMinValueToggle.SetIsOnWithoutNotify(Item.OverrideMinValue);
            MinValueInput.SetTextWithoutNotify(Item.OverrideMinValue ? Item.MinValue.ToString(System.Globalization.CultureInfo.InvariantCulture) : ((int)Item.Default(nameof(Item.MinValue))).ToString(System.Globalization.CultureInfo.InvariantCulture));
            MinValueInput.interactable = Item.OverrideMinValue;
            MinValueInput.textComponent.color = MinValueInput.interactable ? activeTextColor : inactiveTextColor;

            OverrideMaxValueToggle.SetIsOnWithoutNotify(Item.OverrideMaxValue);
            MaxValueInput.SetTextWithoutNotify(Item.OverrideMaxValue ? Item.MaxValue.ToString(System.Globalization.CultureInfo.InvariantCulture) : ((int)Item.Default(nameof(Item.MaxValue))).ToString(System.Globalization.CultureInfo.InvariantCulture));
            MaxValueInput.interactable = Item.OverrideMaxValue;
            MaxValueInput.textComponent.color = MaxValueInput.interactable ? activeTextColor : inactiveTextColor;
        }

        public void SetValue(LobbyConfiguration.ScrapConfig config)
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
