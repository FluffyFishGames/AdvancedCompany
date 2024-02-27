using AdvancedCompany.Config;
using System;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany.Unity.Moons
{
    internal class MoonsContainer : MonoBehaviour
    {
        public TextMeshProUGUI TitleLabel;
        public TextMeshProUGUI DescriptionLabel;

        public GameObject MoonItem;
        public Transform MoonsTransform;
        public Dictionary<string, MoonItem> Moons = new Dictionary<string, MoonItem>();

        public GameObject EnemyItem;
        public GameObject ScrapItem;

        public UnityEngine.UI.Toggle PriceOverrideToggle;
        public TMPro.TMP_InputField PriceField;
        public Button ResetPriceButton;

        public UnityEngine.UI.Toggle DungeonSizeOverrideToggle;
        public TMPro.TMP_InputField DungeonSizeField;
        public Button ResetDungeonSizeButton;

        public UnityEngine.UI.Toggle MinScrapAmountOverrideToggle;
        public TMPro.TMP_InputField MinScrapAmountField;
        public Button ResetMinScrapAmountButton;

        public UnityEngine.UI.Toggle MaxScrapAmountOverrideToggle;
        public TMPro.TMP_InputField MaxScrapAmountField;
        public Button ResetMaxScrapAmountButton;

        public UnityEngine.UI.Toggle ScrapAmountModifierOverrideToggle;
        public TMPro.TMP_InputField ScrapAmountModifierField;
        public Button ResetScrapAmountModifierButton;

        public UnityEngine.UI.Toggle ScrapValueModifierOverrideToggle;
        public TMPro.TMP_InputField ScrapValueModifierField;
        public Button ResetScrapValueModifierButton;

        public Transform LootTableContainer;
        public Dictionary<string, ScrapItem> LootTableItems = new Dictionary<string, ScrapItem>();

        public UnityEngine.UI.Toggle DaytimeEnemiesMaxPowerOverrideToggle;
        public TMPro.TMP_InputField DaytimeEnemiesMaxPowerField;
        public Button ResetDaytimeEnemiesMaxPowerButton;

        public UnityEngine.UI.Toggle DaytimeEnemiesProbabilityOverrideToggle;
        public TMPro.TMP_InputField DaytimeEnemiesProbabilityField;
        public Button ResetDaytimeEnemiesProbabilityButton;

        public Transform DaytimeEnemiesContainer;
        public Dictionary<string, EnemyItem> DaytimeEnemies = new Dictionary<string, EnemyItem>();

        public UnityEngine.UI.Toggle OutsideEnemiesMaxPowerOverrideToggle;
        public TMPro.TMP_InputField OutsideEnemiesMaxPowerField;
        public Button ResetOutsideEnemiesMaxPowerButton;

        public Transform OutsideEnemiesContainer;
        public Dictionary<string, EnemyItem> OutsideEnemies = new Dictionary<string, EnemyItem>();

        public UnityEngine.UI.Toggle InsideEnemiesMaxPowerOverrideToggle;
        public TMPro.TMP_InputField InsideEnemiesMaxPowerField;
        public Button ResetInsideEnemiesMaxPowerButton;

        public UnityEngine.UI.Toggle InsideEnemiesProbabilityOverrideToggle;
        public TMPro.TMP_InputField InsideEnemiesProbabilityField;
        public Button ResetInsideEnemiesProbabilityButton;

        public Transform InsideEnemiesContainer;
        public Dictionary<string, EnemyItem> InsideEnemies = new Dictionary<string, EnemyItem>();

        private MoonItem Selected;
        private LobbyConfiguration.MoonsConfig Config;

        public void Awake()
        {
            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
            PriceOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverridePrice = val;
                    PriceField.interactable = val;
                    PriceField.SetTextWithoutNotify(Selected.Config.OverridePrice ? Selected.Config.Price.ToString() : Selected.Config.Default(nameof(Selected.Config.Price)).ToString());
                    PriceField.textComponent.color = PriceField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            PriceField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Selected.Config.Price = @int;
                }
            }));
            ResetPriceButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverridePrice = false;
                Selected.Config.Reset(nameof(Selected.Config.Price));
                PriceOverrideToggle.SetIsOnWithoutNotify(false);
                PriceField.interactable = false;
                PriceField.SetTextWithoutNotify(Selected.Config.OverridePrice ? Selected.Config.Price.ToString() : Selected.Config.Default(nameof(Selected.Config.Price)).ToString());
                PriceField.textComponent.color = PriceField.interactable ? activeTextColor : inactiveTextColor;
            }));

            DungeonSizeOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideDungeonSize = val;
                    DungeonSizeField.interactable = val;
                    DungeonSizeField.SetTextWithoutNotify(Selected.Config.OverrideDungeonSize ? Selected.Config.DungeonSize.ToString(System.Globalization.CultureInfo.InvariantCulture) : ((float)Selected.Config.Default(nameof(Selected.Config.DungeonSize))).ToString(System.Globalization.CultureInfo.InvariantCulture));
                    DungeonSizeField.textComponent.color = DungeonSizeField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            DungeonSizeField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float @float))
                        Selected.Config.DungeonSize = @float;
                }
            }));
            ResetDungeonSizeButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideDungeonSize = false;
                Selected.Config.Reset(nameof(Selected.Config.DungeonSize));
                DungeonSizeOverrideToggle.SetIsOnWithoutNotify(false);
                DungeonSizeField.interactable = false;
                DungeonSizeField.SetTextWithoutNotify(Selected.Config.OverrideDungeonSize ? Selected.Config.DungeonSize.ToString(System.Globalization.CultureInfo.InvariantCulture) : ((float)Selected.Config.Default(nameof(Selected.Config.DungeonSize))).ToString(System.Globalization.CultureInfo.InvariantCulture));
                DungeonSizeField.textComponent.color = DungeonSizeField.interactable ? activeTextColor : inactiveTextColor;
            }));

            MinScrapAmountOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideMinScrapAmount = val;
                    MinScrapAmountField.interactable = val;
                    MinScrapAmountField.SetTextWithoutNotify(Selected.Config.OverrideMinScrapAmount ? Selected.Config.MinScrapAmount.ToString() : Selected.Config.Default(nameof(Selected.Config.MinScrapAmount)).ToString());
                    MinScrapAmountField.textComponent.color = MinScrapAmountField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            MinScrapAmountField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Selected.Config.MinScrapAmount = @int;
                }
            }));
            ResetMinScrapAmountButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideMinScrapAmount = false;
                Selected.Config.Reset(nameof(Selected.Config.MinScrapAmount));
                MinScrapAmountOverrideToggle.SetIsOnWithoutNotify(false);
                MinScrapAmountField.interactable = false;
                MinScrapAmountField.SetTextWithoutNotify(Selected.Config.OverrideMinScrapAmount ? Selected.Config.MinScrapAmount.ToString() : Selected.Config.Default(nameof(Selected.Config.MinScrapAmount)).ToString());
                MinScrapAmountField.textComponent.color = MinScrapAmountField.interactable ? activeTextColor : inactiveTextColor;
            }));

            MaxScrapAmountOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideMaxScrapAmount = val;
                    MaxScrapAmountField.interactable = val;
                    MaxScrapAmountField.SetTextWithoutNotify(Selected.Config.OverrideMaxScrapAmount ? Selected.Config.MaxScrapAmount.ToString() : Selected.Config.Default(nameof(Selected.Config.MaxScrapAmount)).ToString());
                    MaxScrapAmountField.textComponent.color = MaxScrapAmountField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            MaxScrapAmountField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Selected.Config.MaxScrapAmount = @int;
                }
            }));
            ResetMaxScrapAmountButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideMaxScrapAmount = false;
                Selected.Config.Reset(nameof(Selected.Config.MaxScrapAmount));
                MaxScrapAmountOverrideToggle.SetIsOnWithoutNotify(false);
                MaxScrapAmountField.interactable = false;
                MaxScrapAmountField.SetTextWithoutNotify(Selected.Config.OverrideMaxScrapAmount ? Selected.Config.MaxScrapAmount.ToString() : Selected.Config.Default(nameof(Selected.Config.MaxScrapAmount)).ToString());
                MaxScrapAmountField.textComponent.color = MaxScrapAmountField.interactable ? activeTextColor : inactiveTextColor;
            }));

            ScrapAmountModifierOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideScrapAmountModifier = val;
                    ScrapAmountModifierField.interactable = val;
                    ScrapAmountModifierField.SetTextWithoutNotify(Mathf.RoundToInt((Selected.Config.OverrideScrapAmountModifier ? Selected.Config.ScrapAmountModifier : (float)Selected.Config.Default(nameof(Selected.Config.ScrapAmountModifier))) * 100f).ToString());
                    ScrapAmountModifierField.textComponent.color = ScrapAmountModifierField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            ScrapAmountModifierField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Selected.Config.ScrapAmountModifier = @int / 100f;
                }
            }));
            ResetScrapAmountModifierButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideScrapAmountModifier = false;
                Selected.Config.Reset(nameof(Selected.Config.ScrapAmountModifier));
                ScrapAmountModifierOverrideToggle.SetIsOnWithoutNotify(false);
                ScrapAmountModifierField.interactable = false;
                ScrapAmountModifierField.SetTextWithoutNotify(Mathf.RoundToInt((Selected.Config.OverrideScrapAmountModifier ? Selected.Config.ScrapAmountModifier : (float)Selected.Config.Default(nameof(Selected.Config.ScrapAmountModifier))) * 100f).ToString());
                ScrapAmountModifierField.textComponent.color = ScrapAmountModifierField.interactable ? activeTextColor : inactiveTextColor;
            }));

            ScrapValueModifierOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideScrapValueModifier = val;
                    ScrapValueModifierField.interactable = val;
                    ScrapValueModifierField.SetTextWithoutNotify(Mathf.RoundToInt((Selected.Config.OverrideScrapValueModifier ? Selected.Config.ScrapValueModifier : (float)Selected.Config.Default(nameof(Selected.Config.ScrapValueModifier))) * 100f).ToString());
                    ScrapValueModifierField.textComponent.color = ScrapValueModifierField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            ScrapValueModifierField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Selected.Config.ScrapValueModifier = @int / 100f;
                }
            }));
            ResetScrapValueModifierButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideScrapValueModifier = false;
                Selected.Config.Reset(nameof(Selected.Config.ScrapValueModifier));
                ScrapValueModifierOverrideToggle.SetIsOnWithoutNotify(false);
                ScrapValueModifierField.interactable = false;
                ScrapValueModifierField.SetTextWithoutNotify(Mathf.RoundToInt((Selected.Config.OverrideScrapValueModifier ? Selected.Config.ScrapValueModifier : (float)Selected.Config.Default(nameof(Selected.Config.ScrapValueModifier))) * 100f).ToString());
                ScrapValueModifierField.textComponent.color = ScrapValueModifierField.interactable ? activeTextColor : inactiveTextColor;
            }));

            DaytimeEnemiesMaxPowerOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideDaytimeEnemiesMaxPower = val;
                    DaytimeEnemiesMaxPowerField.interactable = val;
                    DaytimeEnemiesMaxPowerField.SetTextWithoutNotify(Selected.Config.OverrideDaytimeEnemiesMaxPower ? Selected.Config.DaytimeEnemiesMaxPower.ToString() : Selected.Config.Default(nameof(Selected.Config.DaytimeEnemiesMaxPower)).ToString());
                    DaytimeEnemiesMaxPowerField.textComponent.color = DaytimeEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            DaytimeEnemiesMaxPowerField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Selected.Config.DaytimeEnemiesMaxPower = @int;
                }
            }));
            ResetDaytimeEnemiesMaxPowerButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideDaytimeEnemiesMaxPower = false;
                Selected.Config.Reset(nameof(Selected.Config.DaytimeEnemiesMaxPower));
                DaytimeEnemiesMaxPowerOverrideToggle.SetIsOnWithoutNotify(false);
                DaytimeEnemiesMaxPowerField.interactable = false;
                DaytimeEnemiesMaxPowerField.SetTextWithoutNotify(Selected.Config.OverrideDaytimeEnemiesMaxPower ? Selected.Config.DaytimeEnemiesMaxPower.ToString() : Selected.Config.Default(nameof(Selected.Config.DaytimeEnemiesMaxPower)).ToString());
                DaytimeEnemiesMaxPowerField.textComponent.color = DaytimeEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;
            }));

            DaytimeEnemiesProbabilityOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideDaytimeEnemiesProbability = val;
                    DaytimeEnemiesProbabilityField.interactable = val;
                    DaytimeEnemiesProbabilityField.SetTextWithoutNotify(Selected.Config.OverrideDaytimeEnemiesProbability ? Selected.Config.DaytimeEnemiesProbability.ToString() : Selected.Config.Default(nameof(Selected.Config.DaytimeEnemiesProbability)).ToString());
                    DaytimeEnemiesProbabilityField.textComponent.color = DaytimeEnemiesProbabilityField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            DaytimeEnemiesProbabilityField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float @float))
                        Selected.Config.DaytimeEnemiesProbability = @float;
                }
            }));
            ResetDaytimeEnemiesProbabilityButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideDaytimeEnemiesProbability = false;
                Selected.Config.Reset(nameof(Selected.Config.DaytimeEnemiesProbability));
                DaytimeEnemiesProbabilityOverrideToggle.SetIsOnWithoutNotify(false);
                DaytimeEnemiesProbabilityField.interactable = false;
                DaytimeEnemiesProbabilityField.SetTextWithoutNotify(Selected.Config.OverrideDaytimeEnemiesProbability ? Selected.Config.DaytimeEnemiesProbability.ToString() : Selected.Config.Default(nameof(Selected.Config.DaytimeEnemiesProbability)).ToString());
                DaytimeEnemiesProbabilityField.textComponent.color = DaytimeEnemiesProbabilityField.interactable ? activeTextColor : inactiveTextColor;
            }));

            OutsideEnemiesMaxPowerOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideOutsideEnemiesMaxPower = val;
                    OutsideEnemiesMaxPowerField.interactable = val;
                    OutsideEnemiesMaxPowerField.SetTextWithoutNotify(Selected.Config.OverrideOutsideEnemiesMaxPower ? Selected.Config.OutsideEnemiesMaxPower.ToString() : Selected.Config.Default(nameof(Selected.Config.OutsideEnemiesMaxPower)).ToString());
                    OutsideEnemiesMaxPowerField.textComponent.color = OutsideEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            OutsideEnemiesMaxPowerField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Selected.Config.OutsideEnemiesMaxPower = @int;
                }
            }));
            ResetOutsideEnemiesMaxPowerButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideOutsideEnemiesMaxPower = false;
                Selected.Config.Reset(nameof(Selected.Config.OutsideEnemiesMaxPower));
                OutsideEnemiesMaxPowerOverrideToggle.SetIsOnWithoutNotify(false);
                OutsideEnemiesMaxPowerField.interactable = false;
                OutsideEnemiesMaxPowerField.SetTextWithoutNotify(Selected.Config.OverrideOutsideEnemiesMaxPower ? Selected.Config.OutsideEnemiesMaxPower.ToString() : Selected.Config.Default(nameof(Selected.Config.OutsideEnemiesMaxPower)).ToString());
                OutsideEnemiesMaxPowerField.textComponent.color = OutsideEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;
            }));

            InsideEnemiesMaxPowerOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideInsideEnemiesMaxPower = val;
                    InsideEnemiesMaxPowerField.interactable = val;
                    InsideEnemiesMaxPowerField.SetTextWithoutNotify(Selected.Config.OverrideInsideEnemiesMaxPower ? Selected.Config.InsideEnemiesMaxPower.ToString() : Selected.Config.Default(nameof(Selected.Config.InsideEnemiesMaxPower)).ToString());
                    InsideEnemiesMaxPowerField.textComponent.color = InsideEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            InsideEnemiesMaxPowerField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (int.TryParse(val, out int @int))
                        Selected.Config.InsideEnemiesMaxPower = @int;
                }
            }));
            ResetInsideEnemiesMaxPowerButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideInsideEnemiesMaxPower = false;
                Selected.Config.Reset(nameof(Selected.Config.InsideEnemiesMaxPower));
                InsideEnemiesMaxPowerOverrideToggle.SetIsOnWithoutNotify(false);
                InsideEnemiesMaxPowerField.interactable = false;
                InsideEnemiesMaxPowerField.SetTextWithoutNotify(Selected.Config.OverrideInsideEnemiesMaxPower ? Selected.Config.InsideEnemiesMaxPower.ToString() : Selected.Config.Default(nameof(Selected.Config.InsideEnemiesMaxPower)).ToString());
                InsideEnemiesMaxPowerField.textComponent.color = InsideEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;
            }));

            InsideEnemiesProbabilityOverrideToggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    Selected.Config.OverrideInsideEnemiesProbability = val;
                    InsideEnemiesProbabilityField.interactable = val;
                    InsideEnemiesProbabilityField.SetTextWithoutNotify(Selected.Config.OverrideInsideEnemiesProbability ? Selected.Config.InsideEnemiesProbability.ToString() : Selected.Config.Default(nameof(Selected.Config.InsideEnemiesProbability)).ToString());
                    InsideEnemiesProbabilityField.textComponent.color = InsideEnemiesProbabilityField.interactable ? activeTextColor : inactiveTextColor;
                }
            }));
            InsideEnemiesProbabilityField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (Selected != null && Selected.Config != null)
                {
                    if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out float @float))
                        Selected.Config.InsideEnemiesProbability = @float;
                }
            }));
            ResetInsideEnemiesProbabilityButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Selected.Config.OverrideInsideEnemiesProbability = false;
                Selected.Config.Reset(nameof(Selected.Config.InsideEnemiesProbability));
                InsideEnemiesProbabilityOverrideToggle.SetIsOnWithoutNotify(false);
                InsideEnemiesProbabilityField.interactable = false;
                InsideEnemiesProbabilityField.SetTextWithoutNotify(Selected.Config.OverrideInsideEnemiesProbability ? Selected.Config.InsideEnemiesProbability.ToString() : Selected.Config.Default(nameof(Selected.Config.InsideEnemiesProbability)).ToString());
                InsideEnemiesProbabilityField.textComponent.color = InsideEnemiesProbabilityField.interactable ? activeTextColor : inactiveTextColor;
            }));
        }

        public void SetValue(LobbyConfiguration.MoonsConfig moons)
        {
            Config = moons;
            UpdateValues();
        }

        internal void ApplyValues(MoonItem moon)
        {
            var inactiveTextColor = new Color(198f / 255f, 77f / 255f, 14f / 255f, 1f);
            var activeTextColor = new Color(50f / 255f, 50f / 255f, 50f / 255f, 1f);
            if (moon.Config != null)
            {
                PriceOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverridePrice);
                PriceField.SetTextWithoutNotify(moon.Config.OverridePrice ? moon.Config.Price.ToString() : moon.Config.Default(nameof(moon.Config.Price)).ToString());
                PriceField.interactable = moon.Config.OverridePrice;
                PriceField.textComponent.color = PriceField.interactable ? activeTextColor : inactiveTextColor;

                DungeonSizeOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideDungeonSize);
                DungeonSizeField.SetTextWithoutNotify(moon.Config.OverrideDungeonSize ? moon.Config.DungeonSize.ToString() : moon.Config.Default(nameof(moon.Config.DungeonSize)).ToString());
                DungeonSizeField.interactable = moon.Config.OverrideDungeonSize;
                DungeonSizeField.textComponent.color = DungeonSizeField.interactable ? activeTextColor : inactiveTextColor;

                MinScrapAmountOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideMinScrapAmount);
                MinScrapAmountField.SetTextWithoutNotify(moon.Config.OverrideMinScrapAmount ? moon.Config.MinScrapAmount.ToString() : moon.Config.Default(nameof(moon.Config.MinScrapAmount)).ToString());
                MinScrapAmountField.interactable = moon.Config.OverrideMinScrapAmount;
                MinScrapAmountField.textComponent.color = MinScrapAmountField.interactable ? activeTextColor : inactiveTextColor;

                MaxScrapAmountOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideMaxScrapAmount);
                MaxScrapAmountField.SetTextWithoutNotify(moon.Config.OverrideMaxScrapAmount ? moon.Config.MaxScrapAmount.ToString() : moon.Config.Default(nameof(moon.Config.MaxScrapAmount)).ToString());
                MaxScrapAmountField.interactable = moon.Config.OverrideMaxScrapAmount;
                MaxScrapAmountField.textComponent.color = MaxScrapAmountField.interactable ? activeTextColor : inactiveTextColor;

                ScrapAmountModifierOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideScrapAmountModifier);
                ScrapAmountModifierField.SetTextWithoutNotify(Mathf.RoundToInt((Selected.Config.OverrideScrapAmountModifier ? Selected.Config.ScrapAmountModifier : (float)Selected.Config.Default(nameof(Selected.Config.ScrapAmountModifier))) * 100f).ToString());
                ScrapAmountModifierField.interactable = moon.Config.OverrideScrapAmountModifier;
                ScrapAmountModifierField.textComponent.color = ScrapAmountModifierField.interactable ? activeTextColor : inactiveTextColor;

                ScrapValueModifierOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideScrapValueModifier);
                ScrapValueModifierField.SetTextWithoutNotify(Mathf.RoundToInt((Selected.Config.OverrideScrapValueModifier ? Selected.Config.ScrapValueModifier : (float)Selected.Config.Default(nameof(Selected.Config.ScrapValueModifier))) * 100f).ToString());
                ScrapValueModifierField.interactable = moon.Config.OverrideScrapValueModifier;
                ScrapValueModifierField.textComponent.color = ScrapValueModifierField.interactable ? activeTextColor : inactiveTextColor;

                DaytimeEnemiesMaxPowerOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideDaytimeEnemiesMaxPower);
                DaytimeEnemiesMaxPowerField.SetTextWithoutNotify(moon.Config.OverrideDaytimeEnemiesMaxPower ? moon.Config.DaytimeEnemiesMaxPower.ToString() : moon.Config.Default(nameof(moon.Config.DaytimeEnemiesMaxPower)).ToString());
                DaytimeEnemiesMaxPowerField.interactable = moon.Config.OverrideDaytimeEnemiesMaxPower;
                DaytimeEnemiesMaxPowerField.textComponent.color = DaytimeEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;

                DaytimeEnemiesProbabilityOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideDaytimeEnemiesProbability);
                DaytimeEnemiesProbabilityField.SetTextWithoutNotify(moon.Config.OverrideDaytimeEnemiesProbability ? moon.Config.DaytimeEnemiesProbability.ToString() : moon.Config.Default(nameof(moon.Config.DaytimeEnemiesProbability)).ToString());
                DaytimeEnemiesProbabilityField.interactable = moon.Config.OverrideDaytimeEnemiesProbability;
                DaytimeEnemiesProbabilityField.textComponent.color = DaytimeEnemiesProbabilityField.interactable ? activeTextColor : inactiveTextColor;

                OutsideEnemiesMaxPowerOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideOutsideEnemiesMaxPower);
                OutsideEnemiesMaxPowerField.SetTextWithoutNotify(moon.Config.OverrideOutsideEnemiesMaxPower ? moon.Config.OutsideEnemiesMaxPower.ToString() : moon.Config.Default(nameof(moon.Config.OutsideEnemiesMaxPower)).ToString());
                OutsideEnemiesMaxPowerField.interactable = moon.Config.OverrideOutsideEnemiesMaxPower;
                OutsideEnemiesMaxPowerField.textComponent.color = OutsideEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;

                InsideEnemiesMaxPowerOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideInsideEnemiesMaxPower);
                InsideEnemiesMaxPowerField.SetTextWithoutNotify(moon.Config.OverrideInsideEnemiesMaxPower ? moon.Config.InsideEnemiesMaxPower.ToString() : moon.Config.Default(nameof(moon.Config.InsideEnemiesMaxPower)).ToString());
                InsideEnemiesMaxPowerField.interactable = moon.Config.OverrideInsideEnemiesMaxPower;
                InsideEnemiesMaxPowerField.textComponent.color = InsideEnemiesMaxPowerField.interactable ? activeTextColor : inactiveTextColor;

                InsideEnemiesProbabilityOverrideToggle.SetIsOnWithoutNotify(moon.Config.OverrideInsideEnemiesProbability);
                InsideEnemiesProbabilityField.SetTextWithoutNotify(moon.Config.OverrideInsideEnemiesProbability ? moon.Config.InsideEnemiesProbability.ToString() : moon.Config.Default(nameof(moon.Config.InsideEnemiesProbability)).ToString());
                InsideEnemiesProbabilityField.interactable = moon.Config.OverrideInsideEnemiesProbability;
                InsideEnemiesProbabilityField.textComponent.color = InsideEnemiesProbabilityField.interactable ? activeTextColor : inactiveTextColor;

                foreach (var kv in LootTableItems)
                {
                    if (moon.Config.LootTable.ContainsKey(kv.Key))
                    {
                        kv.Value.SetValue(moon.Config.LootTable[kv.Key]);
                    }
                }

                foreach (var kv in DaytimeEnemies)
                {
                    if (moon.Config.DaytimeEnemies.ContainsKey(kv.Key))
                    {
                        kv.Value.SetValue(moon.Config.DaytimeEnemies[kv.Key]);
                    }
                }

                foreach (var kv in OutsideEnemies)
                {
                    if (moon.Config.OutsideEnemies.ContainsKey(kv.Key))
                    {
                        kv.Value.SetValue(moon.Config.OutsideEnemies[kv.Key]);
                    }
                }

                foreach (var kv in InsideEnemies)
                {
                    if (moon.Config.InsideEnemies.ContainsKey(kv.Key))
                    {
                        kv.Value.SetValue(moon.Config.InsideEnemies[kv.Key]);
                    }
                }
            }
        }

        public void AddMoon(string moonName)
        {
            var newMoon = GameObject.Instantiate(MoonItem, MoonsTransform);
            var comp = newMoon.GetComponent<MoonItem>();
            comp.Initialize(moonName);
            comp.Unselect();
            comp.Button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                SelectMoon(comp);
            }));
            Moons.Add(moonName, comp);
            if (Moons.Count == 1)
                SelectMoon(comp);
        }

        public void SelectMoon(MoonItem item)
        {
            if (Selected != null)
                Selected.Unselect();
            item.Select();
            Selected = item;
            ApplyValues(Selected);
        }

        public void AddScrap(string scrapName)
        {
            var newScrap = GameObject.Instantiate(ScrapItem, LootTableContainer);
            var comp = newScrap.GetComponent<ScrapItem>();
            comp.Initialize(scrapName); 
            LootTableItems.Add(scrapName, comp);
        }

        public void AddEnemy(string enemyName)
        {
            var daytimeObj = GameObject.Instantiate(EnemyItem, DaytimeEnemiesContainer);
            var daytimeComp = daytimeObj.GetComponent<EnemyItem>();
            daytimeComp.Initialize(enemyName);
            DaytimeEnemies.Add(enemyName, daytimeComp);

            var outsideObj = GameObject.Instantiate(EnemyItem, OutsideEnemiesContainer);
            var outsideComp = outsideObj.GetComponent<EnemyItem>();
            outsideComp.Initialize(enemyName);
            OutsideEnemies.Add(enemyName, outsideComp);

            var insideObj = GameObject.Instantiate(EnemyItem, InsideEnemiesContainer);
            var insideComp = insideObj.GetComponent<EnemyItem>();
            insideComp.Initialize(enemyName);
            InsideEnemies.Add(enemyName, insideComp);

        }

        public void Initialize(string title, string description)
        {
            TitleLabel.text = title;
            DescriptionLabel.text = description;
        }

        public void UpdateValues()
        {
            foreach (var kv in Config.Moons)
            {
                if (Moons.ContainsKey(kv.Key))
                {
                    Moons[kv.Key].SetValue(kv.Value);
                    if (Selected == Moons[kv.Key])
                    {
                        ApplyValues(Moons[kv.Key]);
                    }
                }
            }
        }
    }
}
