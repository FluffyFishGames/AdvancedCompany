using AdvancedCompany.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{/*
    public class ConfigLootTableItem : MonoBehaviour
    {
        public Toggle OverrideInput;
        public bool IsCustom = false;
        public TMPro.TMP_InputField NameInput;
        public TMPro.TMP_InputField RarityInput;
        public TextMeshProUGUI NameDefault;
        public TextMeshProUGUI RarityDefault;
        public Button ResetButton;
        public Button DeleteButton;
        private LobbyConfiguration.MoonConfig.LootTableItem Item;

        public delegate void Deleted(ConfigLootTableItem item);
        [HideInInspector]
        public Deleted OnDeleted;
        
        void Start()
        {
            OverrideInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                ChangeOverride(val);
                Item.Override = val;
            }));
            NameInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                Item.Name = val;
            }));
            RarityInput.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Item.Rarity = @int;
            }));

            ResetButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Item.Reset(nameof(Item.Rarity));
                RarityInput.text = Item.Rarity + "";
            }));
        }

        void ChangeOverride(bool @override)
        {
            if (@override)
            {
                RarityDefault.gameObject.SetActive(false);
                RarityInput.gameObject.SetActive(true);
            }
            else
            {
                RarityDefault.gameObject.SetActive(true);
                RarityInput.gameObject.SetActive(false);
            }
        }

        public void UpdateValue()
        {
            NameDefault.gameObject.SetActive(!Item.IsCustom);
            NameInput.gameObject.SetActive(Item.IsCustom);
            NameDefault.text = Item.Name;
            NameInput.text = Item.Name;
            RarityInput.text = Item.Rarity + "";
            RarityDefault.text = Item.Default(nameof(Item.Rarity)).ToString();
            ChangeOverride(Item.Override);
        }

        public void SetValue(LobbyConfiguration.MoonConfig.LootTableItem item)
        {
            Item = item;
            UpdateValue();
        }
    }*/
}
