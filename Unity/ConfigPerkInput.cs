using AdvancedCompany.Config;
using System.Collections.Generic;
using System.Diagnostics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigPerkInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public Toggle ActiveInput;
        public TMPro.TMP_InputField BaseInput;
        public TMPro.TMP_InputField ChangeInput;
        public TMPro.TMP_InputField CostInput;
        public Button ResetBaseButton;
        public Button ResetChangeButton;
        public Button ResetPricesButton;
        private LobbyConfiguration.PerkConfiguration Perk;
        private static Validator ValidatorInstance;

        void Start()
        {
            ActiveInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => 
            {
                Perk.Active = val;
            }));
            BaseInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float @float))
                    Perk.Base = @float / 100f;
            }));
            ChangeInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float @float))
                    Perk.Change = @float / 100f;
            }));
            CostInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                var p = val.Split(new char[] { ',' }, System.StringSplitOptions.RemoveEmptyEntries);
                var l = new List<int>();
                for (var i = 0; i < p.Length; i++)
                {
                    if (int.TryParse(p[i], out var v))
                        l.Add(v);
                }
                Perk.Prices = l.ToArray();
            }));

            if (ValidatorInstance == null)
                ValidatorInstance = ScriptableObject.CreateInstance<Validator>();
            CostInput.inputValidator = ValidatorInstance;

            ResetBaseButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Perk.Reset(nameof(Perk.Base));
                BaseInput.SetTextWithoutNotify((Mathf.RoundToInt(Perk.Base * 1000f) / 10f).ToString(System.Globalization.CultureInfo.InvariantCulture));
            }));
            ResetChangeButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Perk.Reset(nameof(Perk.Change));
                ChangeInput.SetTextWithoutNotify((Mathf.RoundToInt(Perk.Change * 1000f) / 10f).ToString(System.Globalization.CultureInfo.InvariantCulture));
            }));
            ResetPricesButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Perk.Reset(nameof(Perk.Prices));
                CostInput.SetTextWithoutNotify(string.Join(", ", Perk.Prices));
            }));
        }

        public void UpdateValue()
        {
            ActiveInput.SetIsOnWithoutNotify(Perk.Active);
            BaseInput.SetTextWithoutNotify((Mathf.RoundToInt(Perk.Base * 1000f) / 10f).ToString(System.Globalization.CultureInfo.InvariantCulture));
            ChangeInput.SetTextWithoutNotify((Mathf.RoundToInt(Perk.Change * 1000f) / 10f).ToString(System.Globalization.CultureInfo.InvariantCulture));
            CostInput.SetTextWithoutNotify(string.Join(", ", Perk.Prices));
        }

        public void SetValue(LobbyConfiguration.PerkConfiguration perk)
        {
            Perk = perk;
            UpdateValue();
        }

        public void Initialize(string label)
        {
            Label.text = label + ":";
        }

        class Validator : TMP_InputValidator
        {
            public override char Validate(ref string text, ref int pos, char ch)
            {
                if ((ch >= '0' && ch <= '9') || ch == ' ' || ch == ',')
                {
                    text = text.Substring(0, pos) + ch + text.Substring(pos);
                    pos += 1;
                    return ch;
                }
                return (char)0;
            }
        }
    }
}
