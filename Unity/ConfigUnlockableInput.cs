using AdvancedCompany.Config;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigUnlockableInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public Toggle ActiveInput;
        public Toggle OverridePriceToggle;
        public TMPro.TMP_InputField PriceInput;
        public Button ResetPriceButton;
        private LobbyConfiguration.UnlockableConfig Unlockable;

        void Start()
        {
            ActiveInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) =>
            {
                Unlockable.Active = val;
            }));
            PriceInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Unlockable.Price = @int;
            }));

            OverridePriceToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Unlockable.OverridePrice = val;
                UpdateValue();
            }));

            ResetPriceButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Unlockable.Reset(nameof(Unlockable.Price));
                Unlockable.Reset(nameof(Unlockable.OverridePrice));
                UpdateValue();
            }));
        }

        public void UpdateValue()
        {
            ActiveInput.SetIsOnWithoutNotify(Unlockable.Active);

            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);

            OverridePriceToggle.SetIsOnWithoutNotify(Unlockable.OverridePrice);
            PriceInput.SetTextWithoutNotify(Unlockable.OverridePrice ? Unlockable.Price.ToString(System.Globalization.CultureInfo.InvariantCulture) : ((int)Unlockable.Default(nameof(Unlockable.Price))).ToString(System.Globalization.CultureInfo.InvariantCulture));
            PriceInput.interactable = Unlockable.OverridePrice;
            PriceInput.textComponent.color = PriceInput.interactable ? activeTextColor : inactiveTextColor;
        }

        public void SetValue(LobbyConfiguration.UnlockableConfig config)
        {
            Unlockable = config;
            UpdateValue();
        }

        public void Initialize(string label)
        {
            Label.text = label + (label.EndsWith(":") ? "" : ":");
        }
    }
}
