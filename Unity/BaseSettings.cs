using AdvancedCompany;
using AdvancedCompany.Config;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BaseSettings<T> : MonoBehaviour where T : Configuration, new()
{
    public class Preset
    {
        private IConfigurationProvider Provider;

        public Preset(IConfigurationProvider provider, string name, T configuration, bool canRemove = true, bool canRename = true, bool canBeSaved = true)
        {
            Provider = provider;
            _Name = name;
            Configuration = configuration;
            _CanBeRemoved = canRemove;
            _CanBeRenamed = canRename;
            _CanBeSaved = canBeSaved;
        }

        private bool _CanBeRenamed;
        public bool CanBeRenamed
        {
            get
            {
                return _CanBeRenamed;
            }
            set
            {
                _CanBeRenamed = value;
                if (RenameButton != null)
                    RenameButton.gameObject.SetActive(_CanBeRenamed);
            }
        }

        private bool _CanBeRemoved;
        public bool CanBeRemoved
        {
            get
            {
                return _CanBeRemoved;
            }
            set
            {
                _CanBeRemoved = value;
                if (RemoveButton != null)
                    RemoveButton.gameObject.SetActive(_CanBeRemoved);
            }
        }

        private bool _CanBeSaved;
        public bool CanBeSaved
        {
            get
            {
                return _CanBeSaved;
            }
            set
            {
                _CanBeSaved = value;
            }
        }
        private string _Name;
        public string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
                if (Label != null)
                    Label.text = _Name;
            }
        }

        public T Configuration;

        internal bool Selected = false;
        internal GameObject Container;
        internal GameObject SelectedBackground;
        internal TextMeshProUGUI Label;
        internal Button PresetButton;
        internal Button RemoveButton;
        internal Button RenameButton;
    }

    public interface IConfigurationProvider
    {
        List<Preset> GetPresets();
        Preset GetPreset(string name);
        void PresetRenamed(Preset preset, string oldName, string newName);
        void PresetRemoved(Preset preset);
        void PresetCreated(Preset preset);
        void PresetSaved(Preset preset);
        Preset CreatePreset(string name, T configuration);
    }

    public class TestConfigurationProvider : IConfigurationProvider
    {
        private List<Preset> Presets = new();

        public TestConfigurationProvider()
        {
            Presets.Add(new Preset(this, "File config", new T(), false, false));
            Presets.Add(new Preset(this, "Test 1", new T()));
            Presets.Add(new Preset(this, "Test 2", new T()));
        }

        public Preset GetPreset(string name)
        {
            for (var i = 0; i < Presets.Count; i++)
            {
                if (Presets[i].Name.Equals(name, StringComparison.OrdinalIgnoreCase))
                {
                    return Presets[i];
                }
            }
            return null;
        }

        public void PresetCreated(Preset preset)
        {
            Presets.Add(preset);
        }

        public List<Preset> GetPresets()
        {
            return Presets;
        }

        public void PresetRemoved(Preset preset)
        {
            Presets.Remove(preset);
        }

        public void PresetRenamed(Preset preset, string oldName, string newName)
        {

        }

        public void PresetSaved(Preset preset)
        {

        }

        public Preset CreatePreset(string name, T configuration)
        {
            return new Preset(this, name, configuration, true, true);
        }
    }

    [Header("Images")]
    public Sprite TabActive;
    public Sprite TabInactive;

    [Header("Window")]
    public Transform TabContainer;
    public Transform Container;


    [Header("Presets")]
    public GameObject PresetTemplate;
    public Transform PresetContainer;
    public Button CreatePreset;
    private IConfigurationProvider PresetsProvider;
    [HideInInspector]
    public Preset SelectedPreset;

    [Header("Windows")]
    public RenamePresetWindow RenamePreset;
    public RemovePresetWindow RemovePreset;
    public ConfirmOverrideWindow ConfirmOverride;
    public NewPresetWindow NewPreset;

    [Header("Buttons")]
    public Button SaveAsNewPresetButton;
    public Button SaveButton;
    public Button ContinueButton;
    public Button CancelButton;

    [Header("Notification")]
    public GameObject NotificationObject;
    public TextMeshProUGUI NotificationText;

    public delegate void Continue(T configuration, string presetName);
    public delegate void Cancel();
    [HideInInspector]
    public Continue OnContinue;
    [HideInInspector]
    public Cancel OnCancel;

    internal List<Button> Tabs = new();
    internal List<GameObject> Containers = new();

    //    public  MaxPlayersSlider;
    // Start is called before the first frame update


    internal T Configuration;


    public void SetConfiguration(IConfigurationProvider configuration)
    {
        PresetsProvider = configuration;

        var p = PresetContainer;
        for (var i = p.childCount - 1; i >= 0; i--)
        {
            var c = p.GetChild(i).gameObject;
            GameObject.Destroy(c);
        }

        var presets = PresetsProvider.GetPresets();
        for (var i = 0; i < presets.Count; i++)
            AddPreset(presets[i]);

        if (presets.Count > 0)
            SelectPreset(presets[0]);
    }

    public virtual void ApplyConfiguration()
    {
    }

    internal void ShowNotification(string text)
    {
        NotificationObject.SetActive(true);
        NotificationText.text = text;
        StartCoroutine(HideNotification());
    }

    internal IEnumerator HideNotification()
    {
        yield return new WaitForSeconds(1f);
        NotificationObject.SetActive(false);
    }

    public virtual void Awake()
    {
        Configuration = new T();
        Configuration.Build();

        for (var i = 0; i < Container.childCount; i++)
        {
            Containers.Add(Container.GetChild(i).gameObject);
        }
        for (var i = 0; i < TabContainer.childCount; i++)
        {
            var button = TabContainer.GetChild(i).gameObject.GetComponent<Button>();
            var container = Containers[i];
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                ChangeTab(button, container);
            }));
            Tabs.Add(button);
        }

        SaveButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            SelectedPreset.Configuration.CopyFrom(Configuration);
            PresetsProvider.PresetSaved(SelectedPreset);
            ShowNotification("Preset saved");
        }));
        
        SaveAsNewPresetButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            CreatingNew = false;
            NewPreset.Open("New preset");
        }));

        ContinueButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            if (OnContinue != null)
            {
                OnContinue(Configuration, SelectedPreset.Name);
                Close();
            }
        }));

        CancelButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            if (OnCancel != null)
            {
                OnCancel();
                Close();
            }
        }));

        ConfirmOverride.OnSubmitted = (name) =>
        {
            var preset = PresetsProvider.GetPreset(name);
            if (preset != null)
            {
                GameObject.Destroy(preset.Container);
                PresetsProvider.PresetRemoved(preset);
            }
            if (RenamingPreset != null)
            {
                var oldName = RenamingPreset.Name;
                RenamingPreset.Name = name;
                PresetsProvider.PresetRenamed(RenamingPreset, oldName, name);
                RenamingPreset = null;
                ShowNotification("Preset renamed");
            }
            else
            {
                var newPreset = PresetsProvider.CreatePreset(name, CreatingNew ? new T() : (T)Configuration._Clone());
                AddPreset(newPreset);
                PresetsProvider.PresetCreated(newPreset);
                ShowNotification("Preset created");
                SelectPreset(newPreset);
            }
            CreatingNew = false;
        };

        RenamePreset.OnSubmitted = (name) =>
        {
            var preset = PresetsProvider.GetPreset(name);
            if (preset != null)
            {
                if (!preset.CanBeSaved)
                    return;
                ConfirmOverride.Open(name);
            }
            else
            {
                var oldName = RenamingPreset.Name;
                RenamingPreset.Name = name;
                PresetsProvider.PresetRenamed(RenamingPreset, oldName, name);
                RenamingPreset = null;
                ShowNotification("Preset renamed");
            }
        };

        RemovePreset.OnSubmitted = () =>
        {
            PresetsProvider.PresetRemoved(DeletingPreset);
            GameObject.Destroy(DeletingPreset.Container);
            ShowNotification("Preset deleted");
            var presets = PresetsProvider.GetPresets();
            if (presets.Count > 0)
                SelectPreset(presets[0]);
            else
            {
                Configuration = new T();
                ApplyConfiguration();
            }
            DeletingPreset = null;
        };

        NewPreset.OnSubmitted = (name) =>
        {
            if (name == "File config")
                return;
            var preset = PresetsProvider.GetPreset(name);
            if (preset != null)
            {
                if (!preset.CanBeSaved)
                    return;
                ConfirmOverride.Open(name);
            }
            else
            {
                var newPreset = PresetsProvider.CreatePreset(name, CreatingNew ? new T() : (T)Configuration._Clone());
                AddPreset(newPreset);
                PresetsProvider.PresetCreated(newPreset);
                SelectPreset(newPreset);
                ShowNotification("Preset created");
            }
        };

        CreatePreset.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
            CreatingNew = true;
            NewPreset.Open("New preset");
        }));
        /*
        if (Application.isEditor)
        {
            SetConfiguration(new TestConfigurationProvider());
        }*/
        ChangeTab(Tabs[0], Containers[0]);
    }

    public void Close()
    {
        NotificationObject.SetActive(false);
        gameObject.SetActive(false);
    }

    private bool CreatingNew = false;
    private Preset RenamingPreset;
    private Preset DeletingPreset;
    
    internal void AddPreset(Preset preset)
    {
        var presetObject = GameObject.Instantiate(PresetTemplate, PresetContainer);

        preset.Container = presetObject;
        preset.PresetButton = presetObject.GetComponent<Button>();
        preset.SelectedBackground = presetObject.transform.GetChild(0).gameObject;
        preset.SelectedBackground.SetActive(false);
        preset.Label = presetObject.transform.GetChild(1).GetComponent<TMPro.TextMeshProUGUI>();
        preset.RemoveButton = presetObject.transform.GetChild(2).GetComponent<Button>();
        preset.RenameButton = presetObject.transform.GetChild(3).GetComponent<Button>();
        preset.Label.text = preset.Name;
        preset.RemoveButton.gameObject.SetActive(preset.CanBeRemoved);
        preset.RenameButton.gameObject.SetActive(preset.CanBeRenamed);

        preset.RenameButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            RenamingPreset = preset;
            RenamePreset.Open(preset.Name);
        }));
        preset.RemoveButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            DeletingPreset = preset;
            RemovePreset.Open(preset.Name);
        }));
        preset.PresetButton.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
        {
            SelectPreset(preset);
        }));
        presetObject.SetActive(true);
    }

    public void SelectPreset(string name)
    {
        SelectPreset(PresetsProvider.GetPreset(name));
    }

    public void SelectPreset(Preset preset)
    {
        if (preset == null)
            return;

        if (SelectedPreset != null)
        {
            SelectedPreset.SelectedBackground.SetActive(false);
        }

        preset.SelectedBackground.SetActive(true);
        SelectedPreset = preset;
        if (SelectedPreset != null)
            SaveButton.gameObject.SetActive(SelectedPreset.CanBeSaved);
        Configuration.CopyFrom(SelectedPreset.Configuration);
        ApplyConfiguration();
    }

    internal void ChangeTab(Button toTab, GameObject container)
    {
        for (var i = 0; i < Containers.Count; i++)
        {
            var tab = Tabs[i];
            bool active = tab == toTab;
            Containers[i].SetActive(active);
            if (tab.targetGraphic is Image img)
                img.sprite = active ? TabActive : TabInactive;
            var layout = tab.GetComponent<HorizontalLayoutGroup>();
            var label = tab.GetComponentInChildren<TextMeshProUGUI>();
            label.color = active ? new Color(254f / 255f, 101f / 255f, 22f / 255f) : new Color(176f / 255f, 69f / 255f, 14f / 255f);
            label.fontSize = active ? 25 : 20;
            layout.padding = active ? new RectOffset(20, 20, 0, 0) : new RectOffset(10, 10, 10, 0);
            var tabRect = tab.targetGraphic.GetComponent<RectTransform>();
            tabRect.offsetMin = new Vector2(0f, active ? -0.1f : 0f);
            tabRect.offsetMax = new Vector2(0f, active ? 0f : -9f);
        }
    }
}
