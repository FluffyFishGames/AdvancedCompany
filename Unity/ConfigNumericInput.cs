using AdvancedCompany.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigNumericInput : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public TextMeshProUGUI Unit;
        public TMPro.TMP_InputField Input;
        public LayoutElement InputLayout;
        public Button ResetButton;
        private Configuration.ConfigField Field;

        void Start()
        {
            Input.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) =>
            {
                if (int.TryParse(val, out int @int))
                    Field.Value = @int;
            }));

            ResetButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Field.Reset();
                int val = 0;
                if (Field.Value is int i)
                    val = i;
                else if (Field.Value is float f)
                    val = Mathf.RoundToInt(f);
                Input.SetTextWithoutNotify(val.ToString());
            }));
        }

        public void UpdateValue()
        {
            int val = 0;
            if (Field.Value is int i)
                val = i;
            else if (Field.Value is float f)
                val = Mathf.RoundToInt(f);
            Input.SetTextWithoutNotify(val.ToString());
        }

        public void SetValue(Configuration.ConfigField field)
        {
            Field = field;
            UpdateValue();
        }

        public void Initialize(string label, string unit, float inputWidth, bool showReset)
        {
            Label.text = label + (label.EndsWith(":") ? "" : ":");
            Unit.text = unit;
            InputLayout.preferredWidth = inputWidth;
            ResetButton.gameObject.SetActive(showReset);
        }
    }
}
