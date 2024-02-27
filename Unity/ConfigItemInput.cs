using AdvancedCompany.Config;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigItemInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public Toggle ActiveInput;
        public Toggle OverridePriceToggle;
        public TMPro.TMP_InputField PriceInput;
        public Button ResetPriceButton;
        public Toggle OverrideWeightToggle;
        public TMPro.TMP_InputField WeightInput;
        public Button ResetWeightButton;
        public Toggle OverrideDiscountToggle;
        public TMPro.TMP_InputField DiscountInput;
        public Button ResetDiscountButton;
        private LobbyConfiguration.ItemConfig Item;

        void Start()
        {
            ActiveInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => 
            {
                Item.Active = val;
            }));
            PriceInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Item.Price = @int;
            }));
            WeightInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Item.Weight = 1f + @int / 105f;
            }));
            DiscountInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Item.MaxDiscount = @int;
            }));

            OverridePriceToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Item.OverridePrice = val;
                UpdateValue();
            }));
            OverrideWeightToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Item.OverrideWeight = val;
                UpdateValue();
            }));
            OverrideDiscountToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Item.OverrideMaxDiscount = val;
                UpdateValue();
            }));
            ResetPriceButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Item.Reset(nameof(Item.Price));
                Item.Reset(nameof(Item.OverridePrice));
                UpdateValue();
            }));
            ResetWeightButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Item.Reset(nameof(Item.Weight));
                Item.Reset(nameof(Item.OverrideWeight));
                UpdateValue();
            }));
            ResetDiscountButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Item.Reset(nameof(Item.MaxDiscount));
                Item.Reset(nameof(Item.MaxDiscount));
                UpdateValue();
            }));
        }

        public void UpdateValue()
        {
            ActiveInput.SetIsOnWithoutNotify(Item.Active);

            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

            OverridePriceToggle.SetIsOnWithoutNotify(Item.OverridePrice);
            PriceInput.SetTextWithoutNotify(Item.OverridePrice ? Item.Price.ToString(System.Globalization.CultureInfo.InvariantCulture) : ((int)Item.Default(nameof(Item.Price))).ToString(System.Globalization.CultureInfo.InvariantCulture));
            PriceInput.interactable = Item.OverridePrice;
            PriceInput.textComponent.color = PriceInput.interactable ? activeTextColor : inactiveTextColor;

            OverrideWeightToggle.SetIsOnWithoutNotify(Item.OverrideWeight);
            WeightInput.SetTextWithoutNotify(Item.OverrideWeight ? Mathf.RoundToInt((Item.Weight - 1f) * 105f).ToString(System.Globalization.CultureInfo.InvariantCulture) : Mathf.RoundToInt((((float)Item.Default(nameof(Item.Weight))) - 1f) * 105f).ToString(System.Globalization.CultureInfo.InvariantCulture));
            WeightInput.interactable = Item.OverrideWeight;
            WeightInput.textComponent.color = WeightInput.interactable ? activeTextColor : inactiveTextColor;

            OverrideDiscountToggle.SetIsOnWithoutNotify(Item.OverrideMaxDiscount);
            DiscountInput.SetTextWithoutNotify(Item.OverrideMaxDiscount ? Item.MaxDiscount.ToString(System.Globalization.CultureInfo.InvariantCulture) : ((int)Item.Default(nameof(Item.MaxDiscount))).ToString(System.Globalization.CultureInfo.InvariantCulture));
            DiscountInput.interactable = Item.OverrideMaxDiscount;
            DiscountInput.textComponent.color = DiscountInput.interactable ? activeTextColor : inactiveTextColor;
        }

        public void SetValue(LobbyConfiguration.ItemConfig config)
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
