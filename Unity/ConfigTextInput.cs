using AdvancedCompany.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigTextInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public TMPro.TMP_InputField Input;
        public LayoutElement InputLayout;
        public UnityEngine.UI.Button ResetButton;
        private Configuration.ConfigField Field;

        // Start is called before the first frame update
        void Start()
        {
            Input.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                Field.Value = val;
            }));

            ResetButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Field.Reset();
                Input.SetTextWithoutNotify(Field.Value.ToString());
            }));
        }

        public void UpdateValue()
        {
            Input.SetTextWithoutNotify(Field.Value.ToString());
        }

        public void SetValue(Configuration.ConfigField field)
        {
            Field = field;
            UpdateValue();
        }

        public void Initialize(string label, bool showReset = true)
        {
            Label.text = label + (label.EndsWith(":") ? "" : ":");
            InputLayout.preferredWidth = 100f;
            ResetButton.gameObject.SetActive(showReset);
        }
    }
}
