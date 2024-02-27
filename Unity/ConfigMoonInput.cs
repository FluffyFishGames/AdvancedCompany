using AdvancedCompany.Config;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigMoonInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public TMPro.TMP_InputField PriceInput;
        public TMPro.TMP_InputField ScrapAmountInput;
        public TMPro.TMP_InputField ScrapValueInput;
        public Button ResetPriceButton;
        public Button ResetScrapAmountButton;
        public Button ResetScrapValueButton;

        private LobbyConfiguration.MoonConfig Moon;

        void Start()
        {
            PriceInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Moon.Price = @int;
            }));

            ScrapAmountInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Moon.ScrapAmountModifier = @int / 100f;
            }));

            ScrapValueInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Moon.ScrapValueModifier = @int / 100f;
            }));

            ResetPriceButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Moon.Reset(nameof(Moon.Price));
                PriceInput.SetTextWithoutNotify(Moon.Price + "");
            }));

            ResetScrapAmountButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Moon.Reset(nameof(Moon.ScrapAmountModifier));
                ScrapAmountInput.SetTextWithoutNotify(Mathf.RoundToInt(Moon.ScrapAmountModifier * 100f) + "");
            }));

            ResetScrapValueButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Moon.Reset(nameof(Moon.ScrapValueModifier));
                ScrapValueInput.SetTextWithoutNotify(Mathf.RoundToInt(Moon.ScrapValueModifier * 100f) + "");
            }));
        }

        public void UpdateValue()
        {
            PriceInput.SetTextWithoutNotify(Moon.Price + "");
            ScrapAmountInput.SetTextWithoutNotify(Mathf.RoundToInt(Moon.ScrapAmountModifier * 100f) + "");
            ScrapValueInput.SetTextWithoutNotify(Mathf.RoundToInt(Moon.ScrapValueModifier * 100f) + "");
        }

        public void SetValue(LobbyConfiguration.MoonConfig config)
        {
            Moon = config;
            UpdateValue();
        }

        public void Initialize(string label)
        {
            Label.text = label;
        }
    }
}
