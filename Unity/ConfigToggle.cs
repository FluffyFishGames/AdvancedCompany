using AdvancedCompany.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigToggle : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public Toggle Toggle;
        public UnityEngine.UI.Button ResetButton;
        private Configuration.ConfigField Field;

        void Start()
        {
            Toggle.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<bool>((val) => {
                Field.Value = val;
            }));

            ResetButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Field.Reset();
                Toggle.SetIsOnWithoutNotify((bool)Field.Value);
            }));
        }

        public void UpdateValue()
        {
            Toggle.SetIsOnWithoutNotify((bool)Field.Value);
        }

        public void SetValue(Configuration.ConfigField field)
        {
            Field = field;
            UpdateValue();
        }

        public void Initialize(string label, bool showReset = true)
        {
            Label.text = label + (label.EndsWith(":") ? "" : ":");
            ResetButton.gameObject.SetActive(showReset);
        }
    }
}
