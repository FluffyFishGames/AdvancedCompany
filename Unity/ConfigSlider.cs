using AdvancedCompany.Config;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfigSlider : MonoBehaviour
    {
        public TextMeshProUGUI Label;
        public UnityEngine.UI.Slider Slider;
        public TMP_InputField ValueField;
        public LayoutElement ValueLayout;
        public Button ResetButton;
        private Configuration.ConfigField Field;

        // Start is called before the first frame update
        void Start()
        {
            this.Slider.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<float>((val) => {
                ValueField.SetTextWithoutNotify(((int)val) + "");
                if (Field.Value is int)
                    Field.Value = Mathf.RoundToInt(val / Field.Field.SliderAttribute.Conversion);
                else if (Field.Value is float)
                    Field.Value = val / Field.Field.SliderAttribute.Conversion;
            }));

            this.ValueField.onValueChanged.AddListener(new UnityEngine.Events.UnityAction<string>((val) => {
                if (float.TryParse(val, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var n))
                {
                    this.Slider.SetValueWithoutNotify(n);
                    if (Field.Value is int)
                        Field.Value = Mathf.RoundToInt(n / Field.Field.SliderAttribute.Conversion);
                    else if (Field.Value is float)
                        Field.Value = n / Field.Field.SliderAttribute.Conversion;
                }
            }));

            this.ResetButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                Field.Reset();
                float val = 0f;
                if (Field.Value is float f)
                    val = f * Field.Field.SliderAttribute.Conversion;
                else if (Field.Value is int i)
                    val = i * Field.Field.SliderAttribute.Conversion;
                this.Slider.SetValueWithoutNotify(val);
                this.ValueField.SetTextWithoutNotify(val + "");
            }));
        }

        public void UpdateValue()
        {
            float val = 0f;
            if (Field.Value is float f)
                val = f * Field.Field.SliderAttribute.Conversion;
            else if (Field.Value is int i)
                val = i * Field.Field.SliderAttribute.Conversion;

            this.Slider.minValue = Field.Field.SliderAttribute.MinValue * Field.Field.SliderAttribute.Conversion;
            this.Slider.maxValue = Field.Field.SliderAttribute.MaxValue * Field.Field.SliderAttribute.Conversion;
            this.ValueField.gameObject.SetActive(Field.Field.SliderAttribute.ShowValue);
            this.ValueLayout.preferredWidth = Field.Field.SliderAttribute.InputWidth;
            this.ValueLayout.minWidth = Field.Field.SliderAttribute.InputWidth;

            this.Slider.SetValueWithoutNotify(val);
            this.ValueField.SetTextWithoutNotify(val + "");
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
