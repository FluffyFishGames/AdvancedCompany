using AdvancedCompany;
using AdvancedCompany.Config;
using AdvancedCompany.Cosmetics;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSettings : BaseSettings<PlayerConfiguration>
{

    public interface ICosmeticProvider
    {
        List<Cosmetic> GetCosmetics();
    }

    public class TestCosmeticProvider : ICosmeticProvider
    {
        private List<Cosmetic> Cosmetics;
        public TestCosmeticProvider()
        {
            Cosmetics = new List<Cosmetic>();
            var cosmetics = CosmeticDatabase.AllCosmetics.Values.ToList();
            for (var i = 0; i < cosmetics.Count; i++)
            {
                Cosmetics.Add(new Cosmetic() { MoreCompanyInstance = cosmetics[i], Icon = cosmetics[i].icon });
            }
            Cosmetics.OrderBy(v => v.MoreCompanyInstance.cosmeticId);
        }

        public List<Cosmetic> GetCosmetics()
        {
            return Cosmetics;
        }
    }

    public class Cosmetic
    {
        public Texture2D Icon;
        public CosmeticInstance MoreCompanyInstance;
        internal Image SelectedBackground;
        internal Button Button;

        public void Unselect()
        {
            SelectedBackground.enabled = false;
        }

        public void Select()
        {
            SelectedBackground.enabled = true;
        }
    }

    [Header("File")]
    public ConfigTabContent FileTabContent;
    private ConfigToggle SaveInProfile;

    [Header("Hotbar")]
    public ConfigTabContent HotbarTabContent;
    private ConfigSlider HotbarAlpha;
    private ConfigSlider HotbarScale;
    private ConfigSlider HotbarSpacing;
    private ConfigSlider HotbarBorderWidth;
    private ConfigSlider HotbarY;
    private ConfigToggle InvertScroll;

    [Header("Graphics")]
    public ConfigTabContent GraphicsTabContent;
    private ConfigSlider MusicVolume;
    private ConfigSlider VisionEnhancerBrightness;

    [Header("Compability")]
    public ConfigTabContent CompabilityTabContent;
    private ConfigToggle DisableMusic;
    private ConfigToggle HideEquipment;
    private ConfigToggle HideCosmetics;
    private ConfigToggle AnimationsCompability;

    [Header("Cosmetics")]
    public GameObject IconTemplate;

    public Transform CosmeticTabsContainer;
    public Transform CosmeticContainer;
    
    public RawImage PlayerCamera; 
    public DressUpDrag PlayerDragHandler;
    private float DressUpRotation;
    private float DressUpHeight;
    public Transform DressUpTarget;
    public Camera DressUpCamera;
    public Transform DressUpPlayer;
    
    private Transform DressUpSpine004;
    private Transform DressUpSpine003;
    private Transform DressUpArmRLower;
    private Transform DressUpSpine;
    private Transform DressUpShinL;
    private Transform DressUpShinR;

    private List<Button> CosmeticTabs = new();
    private List<GameObject> CosmeticContainers = new();

    private ICosmeticProvider CosmeticsProvider;


    public override void ApplyConfiguration()
    {
        RemoveAllCosmetics();
        HotbarAlpha.UpdateValue();
        HotbarScale.UpdateValue();
        HotbarBorderWidth.UpdateValue();
        HotbarSpacing.UpdateValue();
        HotbarY.UpdateValue();
        InvertScroll.UpdateValue();
        SaveInProfile.UpdateValue();
        MusicVolume.UpdateValue();
        VisionEnhancerBrightness.UpdateValue();
        DisableMusic.UpdateValue();
        HideCosmetics.UpdateValue();
        HideEquipment.UpdateValue();
        AnimationsCompability.UpdateValue();

        if (CosmeticsProvider != null)
        {
            var cosmetics = CosmeticsProvider.GetCosmetics();
            foreach (var cosmetic in cosmetics)
                cosmetic.Unselect();

            foreach (var c in Configuration.Cosmetics.ActivatedCosmetics)
            {
                for (var i = 0; i < cosmetics.Count; i++)
                {
                    if (cosmetics[i].MoreCompanyInstance != null && cosmetics[i].MoreCompanyInstance.cosmeticId == c)
                    {
                        cosmetics[i].Select();
                        AddCosmetic(cosmetics[i]);
                        break;
                    }
                }
            }
        }
    }

    public void SetCosmeticsProvider(ICosmeticProvider provider)
    {
        CosmeticsProvider = provider;
        var cosmetics = CosmeticsProvider.GetCosmetics();
        foreach (var container in CosmeticContainers)
        {
            var c = container.transform.childCount;
            for (var i = c - 1; i >= 0; i--)
            {
                var child = container.transform.GetChild(i);
                GameObject.Destroy(child.gameObject);
            }
        }
        
        for (var i = 0; i < cosmetics.Count; i++)
        {
            var cosmetic = cosmetics[i];
            if (cosmetic.MoreCompanyInstance != null)
            {
                var container = CosmeticContainers[0];
                if (cosmetic.MoreCompanyInstance.cosmeticType == CosmeticType.CHEST)
                    container = CosmeticContainers[1];
                else if (cosmetic.MoreCompanyInstance.cosmeticType == CosmeticType.HIP)
                    container = CosmeticContainers[2];
                //else if (cosmetic.MoreCompanyInstance.cosmeticType == CosmeticType.WRIST)
                //    container = CosmeticContainers[3];
                else if (cosmetic.MoreCompanyInstance.cosmeticType == CosmeticType.R_LOWER_ARM)
                    container = CosmeticContainers[3];
                else if (cosmetic.MoreCompanyInstance.cosmeticType == CosmeticType.L_SHIN)
                    container = CosmeticContainers[4];
                else if (cosmetic.MoreCompanyInstance.cosmeticType == CosmeticType.R_SHIN)
                    container = CosmeticContainers[5];

                var icon = GameObject.Instantiate(IconTemplate, container.transform);
                icon.GetComponent<Image>().enabled = true;
                cosmetics[i].Button = icon.GetComponent<Button>();
                cosmetics[i].Button.enabled = true;
                cosmetics[i].Button.onClick.AddListener(new UnityEngine.Events.UnityAction(() => {
                    ChangeCosmetic(cosmetic);
                }));
                cosmetics[i].SelectedBackground = icon.transform.GetChild(0).GetComponent<Image>();
                cosmetics[i].SelectedBackground.enabled = false;
                var iconImage = icon.transform.GetChild(2).GetComponent<RawImage>();
                iconImage.texture = cosmetic.MoreCompanyInstance.icon;
                iconImage.enabled = true;
                icon.SetActive(true);
            }
        }

        if (Configuration != null)
        {
            foreach (var cosmetic in cosmetics)
                cosmetic.Unselect();

            foreach (var cc in Configuration.Cosmetics.ActivatedCosmetics)
            {
                for (var i = 0; i < cosmetics.Count; i++)
                {
                    if (cosmetics[i].MoreCompanyInstance != null && cosmetics[i].MoreCompanyInstance.cosmeticId == cc)
                    {
                        cosmetics[i].Select();
                        AddCosmetic(cosmetics[i]);
                        break;
                    }
                }
            }
        }
    }

    void AddCosmetic(Cosmetic cosmetic)
    {
        if (!SpawnedCosmetics.ContainsKey(cosmetic.MoreCompanyInstance.cosmeticType))
            SpawnedCosmetics.Add(cosmetic.MoreCompanyInstance.cosmeticType, new());
        var spawned = SpawnedCosmetics[cosmetic.MoreCompanyInstance.cosmeticType];
        if (!spawned.ContainsKey(cosmetic.MoreCompanyInstance.cosmeticId))
        {
            var bone = SelectBone(cosmetic.MoreCompanyInstance.cosmeticType);
            var go = GameObject.Instantiate(cosmetic.MoreCompanyInstance.gameObject);
            go.transform.position = bone.transform.position;
            go.transform.rotation = bone.transform.rotation;
            var allTransforms = go.GetComponentsInChildren<Transform>();
            for (var i = 0; i < allTransforms.Length; i++)
            {
                allTransforms[i].gameObject.layer = 31;
            } 
            go.transform.parent = bone;
            go.transform.localScale *= DressUpPlayer.parent.lossyScale.x * 0.38f;
            go.layer = 31;
            spawned.Add(cosmetic.MoreCompanyInstance.cosmeticId, go);
        }
    }

    void RemoveAllCosmetics()
    {
        foreach (var kv in SpawnedCosmetics)
        {
            foreach (var kv2 in kv.Value)
            {
                GameObject.Destroy(kv2.Value);
            }
        }
        SpawnedCosmetics = new();
    }

    void RemoveCosmetic(Cosmetic cosmetic)
    {
        if (SpawnedCosmetics.ContainsKey(cosmetic.MoreCompanyInstance.cosmeticType))
        {
            var spawned = SpawnedCosmetics[cosmetic.MoreCompanyInstance.cosmeticType];
            if (spawned.ContainsKey(cosmetic.MoreCompanyInstance.cosmeticId))
            {
                GameObject.Destroy(spawned[cosmetic.MoreCompanyInstance.cosmeticId]);
                spawned.Remove(cosmetic.MoreCompanyInstance.cosmeticId);
            }
        }
    }

    void ChangeCosmetic(Cosmetic cosmetic)
    {
        if (Configuration.Cosmetics.ActivatedCosmetics == null)
            Configuration.Cosmetics.ActivatedCosmetics = new List<string>();

        if (Configuration.Cosmetics.ActivatedCosmetics.Contains(cosmetic.MoreCompanyInstance.cosmeticId))
        {
            cosmetic.Unselect();
            Configuration.Cosmetics.ActivatedCosmetics.Remove(cosmetic.MoreCompanyInstance.cosmeticId);
            RemoveCosmetic(cosmetic);
        }
        else
        {
            Configuration.Cosmetics.ActivatedCosmetics.Add(cosmetic.MoreCompanyInstance.cosmeticId);
            if (!SpawnedCosmetics.ContainsKey(cosmetic.MoreCompanyInstance.cosmeticType))
                SpawnedCosmetics.Add(cosmetic.MoreCompanyInstance.cosmeticType, new Dictionary<string, GameObject>());
            cosmetic.Select();
            AddCosmetic(cosmetic);
        }
    }

    private Dictionary<CosmeticType, Dictionary<string, GameObject>> SpawnedCosmetics = new Dictionary<CosmeticType, Dictionary<string, GameObject>>();

    Transform SelectBone(CosmeticType type)
    {
        switch (type)
        {
            case CosmeticType.HAT:
                return DressUpSpine004;
            case CosmeticType.CHEST:
                return DressUpSpine003;
            case CosmeticType.HIP:
                return DressUpSpine;
            case CosmeticType.R_LOWER_ARM:
                return DressUpArmRLower;
            case CosmeticType.L_SHIN:
                return DressUpShinL;
            case CosmeticType.R_SHIN:
                return DressUpShinR;
        }
        return null;
    }

    public override void Awake()
    {
        base.Awake();
        for (var i = 0; i < CosmeticContainer.childCount; i++)
        {
            CosmeticContainers.Add(CosmeticContainer.GetChild(i).gameObject);
        }
        for (var i = 0; i < CosmeticTabsContainer.childCount; i++)
        {
            var button = CosmeticTabsContainer.GetChild(i).gameObject.GetComponent<Button>();
            var container = CosmeticContainers[i];
            button.onClick.AddListener(new UnityEngine.Events.UnityAction(() =>
            {
                ChangeCosmeticTab(button, container);
            }));
            CosmeticTabs.Add(button);
        }

        var metarig = DressUpPlayer.transform.GetChild(0).GetChild(1);
        DressUpSpine = metarig.Find("spine");
        var thighL = DressUpSpine.Find("thigh.L");
        DressUpShinL = thighL.Find("shin.L");
        var thighR = DressUpSpine.Find("thigh.R");
        DressUpShinR = thighR.Find("shin.R");
        var spine001 = DressUpSpine.Find("spine.001");
        var spine002 = spine001.Find("spine.002");
        DressUpSpine003 = spine002.Find("spine.003");
        DressUpSpine004 = DressUpSpine003.Find("spine.004");
        var shoulderR = DressUpSpine003.Find("shoulder.R");
        var armUpperR = shoulderR.Find("arm.R_upper");
        DressUpArmRLower = armUpperR.Find("arm.R_lower");

        PresetTemplate.SetActive(false);
        IconTemplate.SetActive(false);

        var configContainer = HotbarTabContent.AddContainer("Hotbar", "Here you can customize the appearance of the hotbar.");
        HotbarAlpha = configContainer.AddSlider(Configuration.Hotbar.Field(nameof(Configuration.Hotbar.HotbarAlpha)), "Hotbar alpha");
        HotbarScale = configContainer.AddSlider(Configuration.Hotbar.Field(nameof(Configuration.Hotbar.HotbarScale)), "Hotbar scale");
        HotbarBorderWidth = configContainer.AddSlider(Configuration.Hotbar.Field(nameof(Configuration.Hotbar.HotbarBorderWidth)), "Hotbar border width");
        HotbarSpacing = configContainer.AddSlider(Configuration.Hotbar.Field(nameof(Configuration.Hotbar.HotbarSpacing)), "Hotbar spacing");
        HotbarY = configContainer.AddSlider(Configuration.Hotbar.Field(nameof(Configuration.Hotbar.HotbarY)), "Hotbar Y pos");
        InvertScroll = configContainer.AddToggle(Configuration.Hotbar.Field(nameof(Configuration.Hotbar.InvertScroll)), "Invert scroll");

        configContainer = FileTabContent.AddContainer("Save in profile", "When activated your progression file will be saved in your profile folder.");
        SaveInProfile = configContainer.AddToggle(Configuration.File.Field(nameof(Configuration.File.SaveInProfile)), "Enable");

        var audioContainer = GraphicsTabContent.AddContainer("Audio", "Change audio volume.");
        MusicVolume = audioContainer.AddSlider(Configuration.Graphics.Field(nameof(Configuration.Graphics.MusicVolume)), "Music volume");

        var visionEnhancerContainer = GraphicsTabContent.AddContainer("Vision enhancer", "Change how the vision enhancer looks.");
        VisionEnhancerBrightness = visionEnhancerContainer.AddSlider(Configuration.Graphics.Field(nameof(Configuration.Graphics.VisionEnhancerBrightness)), "Brightness");
        //ChangeTab(Tabs[0], Containers[0]);

        var compabilityContainer = CompabilityTabContent.AddContainer("Compatibility", "Here you can change certain client side functionality for compatibility reasons. Those settings will mostly only take effect when applied BEFORE joining a game.");
        DisableMusic = compabilityContainer.AddToggle(Configuration.Compability.Field(nameof(Configuration.Compability.DisableMusic)), "Disable music", true);
        HideCosmetics = compabilityContainer.AddToggle(Configuration.Compability.Field(nameof(Configuration.Compability.HideCosmetics)), "Hide cosmetics", true);
        HideEquipment = compabilityContainer.AddToggle(Configuration.Compability.Field(nameof(Configuration.Compability.HideEquipment)), "Hide equipment", true);
        AnimationsCompability = compabilityContainer.AddToggle(Configuration.Compability.Field(nameof(Configuration.Compability.AnimationsCompability)), "Animation compability mode", true);
        
        DressUpPlayer.transform.parent.localScale = new Vector3(1f / DressUpPlayer.transform.parent.lossyScale.x, 1f / DressUpPlayer.transform.parent.lossyScale.y, 1f / DressUpPlayer.transform.parent.lossyScale.z);
    }

    internal void ChangeCosmeticTab(Button toTab, GameObject container)
    {
        for (var i = 0; i < CosmeticContainers.Count; i++)
        {
            var tab = CosmeticTabs[i];
            bool active = tab == toTab;
            CosmeticContainers[i].SetActive(active);
            if (tab.targetGraphic is Image img)
                img.sprite = active ? TabActive : TabInactive;
            var layout = tab.GetComponent<HorizontalLayoutGroup>();
            var image = tab.transform.GetChild(1).GetComponent<Image>();
            image.color = active ? new Color(254f / 255f, 101f / 255f, 22f / 255f) : new Color(176f / 255f, 69f / 255f, 14f / 255f);
            layout.padding = active ? new RectOffset(5, 5, 5, 0) : new RectOffset(5, 5, 5, 0);
        }
    }

    public void Update()
    {
        if (PlayerDragHandler.Delta != Vector2.zero)
        {
            DressUpRotation += PlayerDragHandler.Delta.x * 0.2f;
            DressUpHeight += PlayerDragHandler.Delta.y * 0.01f;
        }
        DressUpRotation = Mathf.Clamp((DressUpRotation + 360f) % 360f, 0f, 360f);
        DressUpHeight = Mathf.Clamp(DressUpHeight, -0.5f, 1.5f);

        DressUpCamera.transform.localPosition = new Vector3(0f, 1f + DressUpHeight, 4f);
        DressUpCamera.transform.LookAt(DressUpTarget);

        DressUpPlayer.transform.localRotation = Quaternion.Euler(new Vector3(0f, DressUpRotation, 0f));
    }
}
