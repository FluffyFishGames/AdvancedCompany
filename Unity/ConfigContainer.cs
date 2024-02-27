using AdvancedCompany.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigContainer : MonoBehaviour
    {
        public TextMeshProUGUI TitleField;
        public TextMeshProUGUI DescriptionField;
        public Transform Container;
        public GameObject SliderPrefab;
        public GameObject NumericInputPrefab;
        public GameObject TextInputPrefab;
        public GameObject TogglePrefab;
        public GameObject ItemPrefab;
        public GameObject UnlockablePrefab;
        public GameObject ScrapPrefab;
        public GameObject EnemyPrefab;
        public GameObject PerkPrefab;
        public GameObject WeatherPrefab;
        public GameObject MoonPrefab;
        public Button AddButton;
        public delegate void Add();
        public Add OnAdd;

        public void Start()
        {
            if (AddButton != null)
            {
                AddButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
                {
                    if (OnAdd != null)
                        OnAdd();
                }));
            }
        }
        public ConfigContainer Initialize(string title, string description)
        {
            if (TitleField != null)
                TitleField.text = title;
            if (DescriptionField != null)
                DescriptionField.text = description;
            return this;
        }

        public GameObject AddEmpty()
        {
            GameObject n = new GameObject();
            n.AddComponent<RectTransform>();
            n.transform.parent = Container;
            return n;
        }

        public ConfigMoonInput AddMoon(LobbyConfiguration.MoonConfig moonConfig, string moonName)
        {
            var moon = GameObject.Instantiate(MoonPrefab, Container);
            var comp = moon.GetComponent<ConfigMoonInput>();
            comp.Initialize(moonName);
            comp.SetValue(moonConfig);
            return comp;
        }

        public ConfigWeatherInput AddWeather(LobbyConfiguration.WeatherConfig weatherConfig, string weatherName)
        {
            var weather = GameObject.Instantiate(WeatherPrefab, Container);
            var comp = weather.GetComponent<ConfigWeatherInput>();
            comp.Initialize(weatherName);
            comp.SetValue(weatherConfig);
            return comp;
        }
        public ConfigPerkInput AddPerk(LobbyConfiguration.PerkConfiguration perkConfig, string itemName)
        {
            var perk = GameObject.Instantiate(PerkPrefab, Container);
            var comp = perk.GetComponent<ConfigPerkInput>();
            comp.Initialize(itemName);
            comp.SetValue(perkConfig);
            return comp;
        }

        public ConfigItemInput AddItem(LobbyConfiguration.ItemConfig itemConfig, string itemName)
        {
            var item = GameObject.Instantiate(ItemPrefab, Container);
            var comp = item.GetComponent<ConfigItemInput>();
            comp.Initialize(itemName);
            comp.SetValue(itemConfig);
            return comp;
        }

        public ConfigUnlockableInput AddUnlockable(LobbyConfiguration.UnlockableConfig unlockableConfig, string unlockableName)
        {
            var item = GameObject.Instantiate(UnlockablePrefab, Container);
            var comp = item.GetComponent<ConfigUnlockableInput>();
            comp.Initialize(unlockableName);
            comp.SetValue(unlockableConfig);
            return comp;
        }

        public ConfigScrapInput AddScrap(LobbyConfiguration.ScrapConfig scrapConfig, string scrapName)
        {
            var item = GameObject.Instantiate(ScrapPrefab, Container);
            var comp = item.GetComponent<ConfigScrapInput>();
            comp.Initialize(scrapName);
            comp.SetValue(scrapConfig);
            return comp;
        }

        public ConfigEnemyInput AddEnemy(LobbyConfiguration.EnemyConfig enemyConfig, string enemyName)
        {
            var item = GameObject.Instantiate(EnemyPrefab, Container);
            var comp = item.GetComponent<ConfigEnemyInput>();
            comp.Initialize(enemyName);
            comp.SetValue(enemyConfig);
            return comp;
        }

        public ConfigSlider AddSlider(Configuration.ConfigField field, string label, bool showReset = true)
        {
            var slider = GameObject.Instantiate(SliderPrefab, Container);
            var comp = slider.GetComponent<ConfigSlider>();
            comp.Initialize(label, showReset);
            comp.SetValue(field);
            return comp;
        }

        public ConfigNumericInput AddNumericInput(Configuration.ConfigField field, string label, string unit, float valueWidth = 35f, bool showReset = true)
        {
            var numericInput = GameObject.Instantiate(NumericInputPrefab, Container);
            var comp = numericInput.GetComponent<ConfigNumericInput>();
            comp.Initialize(label, unit, valueWidth, showReset);
            comp.SetValue(field);
            return comp;
        }

        public ConfigTextInput AddTextInput(Configuration.ConfigField field, string label, bool showReset = true)
        {
            var input = GameObject.Instantiate(TextInputPrefab, Container);
            var comp = input.GetComponent<ConfigTextInput>();
            comp.Initialize(label, showReset);
            comp.SetValue(field);
            return comp;
        }

        public ConfigToggle AddToggle(Configuration.ConfigField field, string label, bool showReset = true)
        {
            var toggle = GameObject.Instantiate(TogglePrefab, Container);
            var comp = toggle.GetComponent<ConfigToggle>();
            comp.Initialize(label, showReset);
            comp.SetValue(field);
            return comp;
        }
    }
}
