using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AdvancedCompany
{
    public class RemovePresetWindow : MonoBehaviour
    {
        public GameObject Shadow;
        public TextMeshProUGUI Text;
        public Button ConfirmButton;
        public Button CancelButton;
        public delegate void Submitted();
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
                    OnSubmitted();
            }));
        }

        public void Open(string name)
        {
            Text.text = "Do you really want to remove \"" + name + "\"? This step is irreversible.";
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
