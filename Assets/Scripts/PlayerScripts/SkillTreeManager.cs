using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public enum SkillType
{
    Speed,
    Damage,
    Experience,
    Health
}

public class SkillTreeManager : MonoBehaviour
{
    public const int SaveVersion = 1;
    public const int MaxSkillLevel = 5;

    private const float SpeedPerLevel = 0.5f;
    private const int DamagePerLevel = 1;
    private const int HealthPerLevel = 1;
    private const float ExperienceBonusPerLevel = 0.2f;

    public static SkillTreeManager Instance { get; private set; }

    public int AvailableSkillPoints { get; private set; }
    public int SpeedSkillLevel { get; private set; }
    public int DamageSkillLevel { get; private set; }
    public int ExperienceSkillLevel { get; private set; }
    public int HealthSkillLevel { get; private set; }
    public bool IsOpen => panelRoot != null && panelRoot.activeSelf;

    private GameObject canvasRoot;
    private GameObject panelRoot;
    private Button openButton;
    private TMP_Text pointsText;
    private TMP_Text statusText;
    private readonly Button[] upgradeButtons = new Button[4];
    private readonly TMP_Text[] levelTexts = new TMP_Text[4];
    private readonly TMP_Text[] valueTexts = new TMP_Text[4];
    private readonly TMP_Text[] buttonTexts = new TMP_Text[4];
    private float previousTimeScale = 1f;

    private static readonly Color PanelColor = new Color(0.105f, 0.075f, 0.075f, 0.98f);
    private static readonly Color CardColor = new Color(0.17f, 0.12f, 0.11f, 0.98f);
    private static readonly Color GoldColor = new Color(0.95f, 0.69f, 0.22f, 1f);
    private static readonly Color TextColor = new Color(0.98f, 0.93f, 0.82f, 1f);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        BuildUserInterface();
        UpdateSceneState(SceneManager.GetActiveScene());
    }

    private void Update()
    {
        if (IsGameplayScene(SceneManager.GetActiveScene().name) && Input.GetKeyDown(KeyCode.K))
        {
            ToggleTree();
        }
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (canvasRoot != null)
        {
            Destroy(canvasRoot);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }

    public int ApplyExperienceBonus(int baseExperience)
    {
        float multiplier = 1f + ExperienceSkillLevel * ExperienceBonusPerLevel;
        return Mathf.Max(baseExperience, Mathf.CeilToInt(baseExperience * multiplier));
    }

    public void AddSkillPoint(int amount = 1)
    {
        if (amount <= 0)
        {
            return;
        }

        AvailableSkillPoints += amount;
        RefreshUI();
        NotifyProgressChanged(true);
    }

    public void ResetProgress()
    {
        AvailableSkillPoints = 0;
        SpeedSkillLevel = 0;
        DamageSkillLevel = 0;
        ExperienceSkillLevel = 0;
        HealthSkillLevel = 0;
        RefreshUI();
    }

    public bool LoadProgress(
        int saveVersion,
        int availablePoints,
        int speedLevel,
        int damageLevel,
        int experienceLevel,
        int healthLevel,
        int characterLevel)
    {
        bool migratedLegacySave = saveVersion < SaveVersion;

        SpeedSkillLevel = Mathf.Clamp(speedLevel, 0, MaxSkillLevel);
        DamageSkillLevel = Mathf.Clamp(damageLevel, 0, MaxSkillLevel);
        ExperienceSkillLevel = Mathf.Clamp(experienceLevel, 0, MaxSkillLevel);
        HealthSkillLevel = Mathf.Clamp(healthLevel, 0, MaxSkillLevel);
        AvailableSkillPoints = migratedLegacySave
            ? Mathf.Max(0, characterLevel)
            : Mathf.Max(0, availablePoints);

        RefreshUI();
        return migratedLegacySave;
    }

    public void OpenTree()
    {
        if (panelRoot == null || !IsGameplayScene(SceneManager.GetActiveScene().name))
        {
            return;
        }

        Menu pauseMenu = FindFirstObjectByType<Menu>();
        if (pauseMenu != null && pauseMenu.isMenuOpen)
        {
            return;
        }

        previousTimeScale = Time.timeScale;
        Time.timeScale = 0f;
        panelRoot.SetActive(true);
        if (openButton != null)
        {
            openButton.gameObject.SetActive(false);
        }

        SetStatus(string.Empty);
        RefreshUI();
    }

    public void CloseTree()
    {
        if (panelRoot == null || !panelRoot.activeSelf)
        {
            return;
        }

        panelRoot.SetActive(false);
        Time.timeScale = previousTimeScale;
        if (openButton != null && IsGameplayScene(SceneManager.GetActiveScene().name))
        {
            openButton.gameObject.SetActive(true);
        }
    }

    public void ToggleTree()
    {
        if (IsOpen)
        {
            CloseTree();
        }
        else
        {
            OpenTree();
        }
    }

    private void PurchaseSkill(SkillType skillType)
    {
        int currentLevel = GetSkillLevel(skillType);
        if (AvailableSkillPoints <= 0 || currentLevel >= MaxSkillLevel)
        {
            return;
        }

        StatsManager stats = StatsManager.Instance;
        if (stats == null)
        {
            SetStatus("No se encontraron las estad\u00edsticas del jugador.");
            return;
        }

        AvailableSkillPoints--;

        switch (skillType)
        {
            case SkillType.Speed:
                SpeedSkillLevel++;
                stats.UpdateSpeed(SpeedPerLevel);
                break;
            case SkillType.Damage:
                DamageSkillLevel++;
                stats.damage += DamagePerLevel;
                break;
            case SkillType.Experience:
                ExperienceSkillLevel++;
                break;
            case SkillType.Health:
                HealthSkillLevel++;
                stats.maxHealth += HealthPerLevel;
                stats.currentHealth = Mathf.Min(stats.currentHealth + HealthPerLevel, stats.maxHealth);
                stats.RefreshHealthUI();
                break;
        }

        SetStatus("Mejora aplicada. El progreso se est\u00e1 guardando.");
        RefreshUI();
        NotifyProgressChanged(true);
    }

    private int GetSkillLevel(SkillType skillType)
    {
        switch (skillType)
        {
            case SkillType.Speed:
                return SpeedSkillLevel;
            case SkillType.Damage:
                return DamageSkillLevel;
            case SkillType.Experience:
                return ExperienceSkillLevel;
            case SkillType.Health:
                return HealthSkillLevel;
            default:
                return 0;
        }
    }

    private void NotifyProgressChanged(bool autoSave)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.UpdateData();
        }

        if (!autoSave)
        {
            return;
        }

        DataSaver dataSaver = FindDataSaver();
        if (dataSaver != null)
        {
            dataSaver.SaveDataSilently();
        }
    }

    private DataSaver FindDataSaver()
    {
        if (GameManager.Instance != null &&
            GameManager.Instance.persistentObjects != null &&
            GameManager.Instance.persistentObjects.Length > 8 &&
            GameManager.Instance.persistentObjects[8] != null)
        {
            return GameManager.Instance.persistentObjects[8].GetComponent<DataSaver>();
        }

        return FindFirstObjectByType<DataSaver>();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        UpdateSceneState(scene);
    }

    private void UpdateSceneState(Scene scene)
    {
        if (canvasRoot == null)
        {
            return;
        }

        bool isGameplay = IsGameplayScene(scene.name);
        if (!isGameplay && IsOpen)
        {
            CloseTree();
        }

        canvasRoot.SetActive(isGameplay);
        if (isGameplay && openButton != null)
        {
            openButton.gameObject.SetActive(!IsOpen);
        }
    }

    private static bool IsGameplayScene(string sceneName)
    {
        return sceneName == "Town" || sceneName == "TravelPath" || sceneName == "Outside";
    }

    private void BuildUserInterface()
    {
        if (canvasRoot != null)
        {
            return;
        }

        canvasRoot = CreateRectObject("SkillTreeCanvas", null);
        DontDestroyOnLoad(canvasRoot);

        Canvas canvas = canvasRoot.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 250;

        CanvasScaler scaler = canvasRoot.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        canvasRoot.AddComponent<GraphicRaycaster>();

        openButton = CreateButton(
            "OpenSkillTreeButton",
            canvasRoot.transform,
            new Vector2(1f, 0f),
            new Vector2(1f, 0f),
            new Vector2(-42f, 38f),
            new Vector2(255f, 58f),
            GoldColor,
            "HABILIDADES");
        openButton.onClick.AddListener(OpenTree);

        panelRoot = CreateImageObject(
            "SkillTreeOverlay",
            canvasRoot.transform,
            new Color(0.02f, 0.015f, 0.015f, 0.88f));
        Stretch(panelRoot.GetComponent<RectTransform>());

        GameObject panel = CreateImageObject("SkillTreePanel", panelRoot.transform, PanelColor);
        SetRect(panel.GetComponent<RectTransform>(), Vector2.zero, new Vector2(1260f, 760f));
        Outline outline = panel.AddComponent<Outline>();
        outline.effectColor = GoldColor;
        outline.effectDistance = new Vector2(3f, -3f);

        CreateText("Title", panel.transform, "\u00c1RBOL DE HABILIDADES", new Vector2(0f, 320f), new Vector2(780f, 70f), 42f, TextAlignmentOptions.Center, GoldColor);
        pointsText = CreateText("Points", panel.transform, string.Empty, new Vector2(0f, 266f), new Vector2(600f, 46f), 25f, TextAlignmentOptions.Center, TextColor);

        Button closeButton = CreateButton(
            "CloseButton",
            panel.transform,
            Vector2.one,
            Vector2.one,
            new Vector2(-24f, -24f),
            new Vector2(54f, 54f),
            new Color(0.47f, 0.16f, 0.14f, 1f),
            "X");
        closeButton.onClick.AddListener(CloseTree);

        CreateText("Root", panel.transform, "H\u00c9ROE", new Vector2(0f, 205f), new Vector2(170f, 56f), 26f, TextAlignmentOptions.Center, TextColor);
        CreateLine(panel.transform, new Vector2(0f, 157f), new Vector2(2f, 42f));
        CreateLine(panel.transform, new Vector2(0f, 136f), new Vector2(840f, 3f));

        SkillType[] skillTypes =
        {
            SkillType.Speed,
            SkillType.Damage,
            SkillType.Experience,
            SkillType.Health
        };
        string[] titles = { "AGILIDAD", "FUERZA", "SABIDURIA", "VITALIDAD" };
        string[] descriptions =
        {
            "+0.5 de velocidad por nivel",
            "+1 de da\u00f1o por nivel",
            "+20% de EXP por nivel",
            "+1 HP m\u00e1ximo por nivel"
        };
        Color[] accents =
        {
            new Color(0.28f, 0.72f, 0.48f, 1f),
            new Color(0.84f, 0.31f, 0.25f, 1f),
            new Color(0.91f, 0.68f, 0.20f, 1f),
            new Color(0.25f, 0.66f, 0.82f, 1f)
        };

        for (int i = 0; i < skillTypes.Length; i++)
        {
            float x = -420f + i * 280f;
            CreateLine(panel.transform, new Vector2(x, 115f), new Vector2(3f, 42f));
            CreateSkillCard(panel.transform, skillTypes[i], titles[i], descriptions[i], accents[i], new Vector2(x, -62f));
        }

        statusText = CreateText("Status", panel.transform, string.Empty, new Vector2(0f, -338f), new Vector2(960f, 42f), 22f, TextAlignmentOptions.Center, TextColor);
        panelRoot.SetActive(false);
        RefreshUI();
    }

    private void CreateSkillCard(
        Transform parent,
        SkillType skillType,
        string title,
        string description,
        Color accent,
        Vector2 position)
    {
        int index = (int)skillType;
        GameObject card = CreateImageObject(title + "Card", parent, CardColor);
        SetRect(card.GetComponent<RectTransform>(), position, new Vector2(252f, 342f));
        Outline outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.72f);
        outline.effectDistance = new Vector2(2f, -2f);

        CreateText("Title", card.transform, title, new Vector2(0f, 126f), new Vector2(220f, 44f), 27f, TextAlignmentOptions.Center, accent);
        levelTexts[index] = CreateText("Level", card.transform, string.Empty, new Vector2(0f, 78f), new Vector2(220f, 38f), 22f, TextAlignmentOptions.Center, TextColor);
        CreateText("Description", card.transform, description, new Vector2(0f, 22f), new Vector2(214f, 70f), 19f, TextAlignmentOptions.Center, TextColor);
        valueTexts[index] = CreateText("CurrentValue", card.transform, string.Empty, new Vector2(0f, -47f), new Vector2(220f, 54f), 18f, TextAlignmentOptions.Center, new Color(0.84f, 0.82f, 0.76f, 1f));

        upgradeButtons[index] = CreateButton(
            "UpgradeButton",
            card.transform,
            new Vector2(0.5f, 0.5f),
            new Vector2(0.5f, 0.5f),
            new Vector2(0f, -126f),
            new Vector2(205f, 54f),
            accent,
            "MEJORAR");
        buttonTexts[index] = upgradeButtons[index].GetComponentInChildren<TMP_Text>();
        SkillType capturedType = skillType;
        upgradeButtons[index].onClick.AddListener(() => PurchaseSkill(capturedType));
    }

    private void RefreshUI()
    {
        if (pointsText == null)
        {
            return;
        }

        pointsText.text = "PUNTOS DISPONIBLES: " + AvailableSkillPoints;
        StatsManager stats = StatsManager.Instance;

        for (int i = 0; i < upgradeButtons.Length; i++)
        {
            SkillType skillType = (SkillType)i;
            int level = GetSkillLevel(skillType);
            levelTexts[i].text = "NIVEL " + level + " / " + MaxSkillLevel;
            upgradeButtons[i].interactable = AvailableSkillPoints > 0 && level < MaxSkillLevel;
            buttonTexts[i].text = level >= MaxSkillLevel ? "M\u00c1XIMO" : "MEJORAR (1)";
        }

        if (stats == null)
        {
            return;
        }

        valueTexts[(int)SkillType.Speed].text = "Velocidad actual: " + stats.speed.ToString("0.0");
        valueTexts[(int)SkillType.Damage].text = "Da\u00f1o actual: " + stats.damage;
        valueTexts[(int)SkillType.Experience].text = "Bonus actual: +" + Mathf.RoundToInt(ExperienceSkillLevel * ExperienceBonusPerLevel * 100f) + "%";
        valueTexts[(int)SkillType.Health].text = "Vida maxima: " + stats.maxHealth;
    }

    private void SetStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = message;
        }
    }

    private static GameObject CreateRectObject(string objectName, Transform parent)
    {
        GameObject gameObject = new GameObject(objectName, typeof(RectTransform));
        if (parent != null)
        {
            gameObject.transform.SetParent(parent, false);
        }

        return gameObject;
    }

    private static GameObject CreateImageObject(string objectName, Transform parent, Color color)
    {
        GameObject gameObject = CreateRectObject(objectName, parent);
        Image image = gameObject.AddComponent<Image>();
        image.color = color;
        return gameObject;
    }

    private static Button CreateButton(
        string objectName,
        Transform parent,
        Vector2 anchorMin,
        Vector2 anchorMax,
        Vector2 position,
        Vector2 size,
        Color color,
        string label)
    {
        GameObject buttonObject = CreateImageObject(objectName, parent, color);
        RectTransform rect = buttonObject.GetComponent<RectTransform>();
        SetAnchor(rect, anchorMin, anchorMax);
        SetRect(rect, position, size);

        Button button = buttonObject.AddComponent<Button>();
        button.targetGraphic = buttonObject.GetComponent<Image>();
        button.navigation = new Navigation { mode = Navigation.Mode.None };

        ColorBlock colors = button.colors;
        colors.normalColor = color;
        colors.highlightedColor = Color.Lerp(color, Color.white, 0.2f);
        colors.pressedColor = Color.Lerp(color, Color.black, 0.2f);
        colors.selectedColor = colors.highlightedColor;
        colors.disabledColor = new Color(0.25f, 0.23f, 0.22f, 0.8f);
        colors.colorMultiplier = 1f;
        button.colors = colors;

        TMP_Text text = CreateText("Label", buttonObject.transform, label, Vector2.zero, size, 21f, TextAlignmentOptions.Center, Color.white);
        text.fontStyle = FontStyles.Bold;
        return button;
    }

    private static TMP_Text CreateText(
        string objectName,
        Transform parent,
        string content,
        Vector2 position,
        Vector2 size,
        float fontSize,
        TextAlignmentOptions alignment,
        Color color)
    {
        GameObject textObject = CreateRectObject(objectName, parent);
        SetRect(textObject.GetComponent<RectTransform>(), position, size);

        TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
        text.text = content;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = alignment;
        text.raycastTarget = false;
        text.enableAutoSizing = true;
        text.fontSizeMin = Mathf.Max(12f, fontSize * 0.62f);
        text.fontSizeMax = fontSize;
        return text;
    }

    private static void CreateLine(Transform parent, Vector2 position, Vector2 size)
    {
        GameObject line = CreateImageObject("Branch", parent, new Color(GoldColor.r, GoldColor.g, GoldColor.b, 0.62f));
        SetRect(line.GetComponent<RectTransform>(), position, size);
    }

    private static void SetRect(RectTransform rect, Vector2 position, Vector2 size)
    {
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
    }

    private static void SetAnchor(RectTransform rect, Vector2 min, Vector2 max)
    {
        rect.anchorMin = min;
        rect.anchorMax = max;
        rect.pivot = new Vector2(
            min.x == max.x ? min.x : 0.5f,
            min.y == max.y ? min.y : 0.5f);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
