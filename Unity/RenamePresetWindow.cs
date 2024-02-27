using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class RenamePresetWindow : MonoBehaviour
    {
        public GameObject Shadow;
        public TMP_InputField NameInput;
        public Button ConfirmButton;
        public Button CancelButton;
        public delegate void Submitted(string value);
        public delegate void Cancelled();
        [HideInInspector]
        public Submitted OnSubmitted;
        [HideInInspector]
        public Cancelled OnCancelled;
        // Start is called before the first frame update
        void Start()
        {
            CancelButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Close();
                if (OnCancelled != null)
                    OnCancelled();
            }));
            ConfirmButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                Close();
                if (OnSubmitted != null)
                    OnSubmitted(NameInput.text.Trim());
            }));
        }

        public void Open(string name)
        {
            Shadow.SetActive(true);
            gameObject.SetActive(true);
            NameInput.text = name;
        }

        public void Close()
        {
            Shadow.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
