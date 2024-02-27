using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class ConfirmOverrideWindow : MonoBehaviour
    {
        public GameObject Shadow;
        public TextMeshProUGUI Text;
        public Button ConfirmButton;
        public Button CancelButton;
        public delegate void Submitted(string name);
        public delegate void Cancelled();
        [HideInInspector]
        public Submitted OnSubmitted;
        [HideInInspector]
        public Cancelled OnCancelled;
        private string Name;

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
                    OnSubmitted(Name);
            }));
        }

        public void Open(string name)
        {
            Name = name;
            Text.text = "Do you really want to override \"" + name + "\"? This step is irreversible.";
            Shadow.SetActive(true);
            gameObject.SetActive(true);
        }

        public void Close()
        {
            Shadow.SetActive(false);
            gameObject.SetActive(false);
        }
    }
}
