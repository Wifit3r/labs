using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Повний UI: екран логіну, HUD (таймер, зіткнення, монети, життя),
/// головне меню (пауза), таблиця рекордів, повідомлення фінішу/програшу.
/// Всі елементи створюються програмно.
/// </summary>
public class UIManager : MonoBehaviour
{
    // --- Панелі ---
    private GameObject loginPanel;
    private GameObject hudPanel;
    private GameObject menuPanel;
    private GameObject recordsPanel;
    private GameObject endPanel;

    // --- HUD елементи ---
    private Text timerText;
    private Text collisionsText;
    private Text coinsText;
    private Text livesText;
    private Slider boostSlider;

    // --- Login ---
    private InputField loginInput;

    // --- End screen ---
    private Text endMessageText;

    // --- Records ---
    private Text recordsContentText;

    private Canvas canvas;
    private PlayerController player;
    private GameManager gameManager;
    private bool gameStarted;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        gameManager = FindObjectOfType<GameManager>();

        CreateCanvas();
        CreateLoginPanel();
        CreateHUDPanel();
        CreateMenuPanel();
        CreateRecordsPanel();
        CreateEndPanel();

        // Початковий стан: показати логін, сховати решту
        loginPanel.SetActive(true);
        hudPanel.SetActive(false);
        menuPanel.SetActive(false);
        recordsPanel.SetActive(false);
        endPanel.SetActive(false);

        // Зупинити гру до логіну
        Time.timeScale = 0f;
        gameStarted = false;

        // Підписка на програш
        GameData.Instance.OnGameOver += ShowGameOver;
    }

    void OnDestroy()
    {
        if (GameData.Instance != null)
            GameData.Instance.OnGameOver -= ShowGameOver;
    }

    void Update()
    {
        if (!gameStarted) return;

        // Оновлення HUD
        var data = GameData.Instance;
        if (timerText != null)
        {
            float remaining = Mathf.Max(0f, data.levelTimeLimit - data.ElapsedTime);
            int min = (int)(remaining / 60f);
            int sec = (int)(remaining % 60f);
            timerText.text = $"Час: {min:00}:{sec:00}";
        }
        if (collisionsText != null)
            collisionsText.text = $"Зіткнення: {data.Collisions}";
        if (coinsText != null)
            coinsText.text = $"Монети: {data.CoinsCollected}";
        if (livesText != null)
            livesText.text = $"Життя: {data.Lives}";

        if (boostSlider != null && player != null)
        {
            boostSlider.maxValue = player.maxBoostDuration;
            boostSlider.value = player.GetBoostRemaining();
        }

        // Escape — меню пауза
        var kb = UnityEngine.InputSystem.Keyboard.current;
        if (kb != null && kb.escapeKey.wasPressedThisFrame && !data.IsGameOver && !gameManager.IsFinished)
        {
            ToggleMenu();
        }
    }

    // ===================== СТВОРЕННЯ UI =====================

    void CreateCanvas()
    {
        GameObject canvasGO = new GameObject("UICanvas");
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.GetComponent<CanvasScaler>().referenceResolution = new Vector2(1920, 1080);
        canvasGO.AddComponent<GraphicRaycaster>();

        // EventSystem
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }
    }

    // --- LOGIN PANEL ---
    void CreateLoginPanel()
    {
        loginPanel = CreatePanel("LoginPanel", new Color(0.1f, 0.1f, 0.2f, 0.95f));

        CreateText(loginPanel.transform, "LoginTitle", "ВВЕДІТЬ ЛОГІН",
            new Vector2(0, 80), 36, Color.white);

        // Input field
        GameObject inputGO = new GameObject("LoginInput");
        inputGO.transform.SetParent(loginPanel.transform, false);
        Image inputBg = inputGO.AddComponent<Image>();
        inputBg.color = Color.white;
        RectTransform inputRT = inputGO.GetComponent<RectTransform>();
        inputRT.sizeDelta = new Vector2(400, 50);
        inputRT.anchoredPosition = new Vector2(0, 10);

        GameObject textGO = new GameObject("Text");
        textGO.transform.SetParent(inputGO.transform, false);
        Text inputText = textGO.AddComponent<Text>();
        inputText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        inputText.color = Color.black;
        inputText.fontSize = 24;
        inputText.alignment = TextAnchor.MiddleCenter;
        inputText.supportRichText = false;
        RectTransform textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = Vector2.zero;
        textRT.anchorMax = Vector2.one;
        textRT.offsetMin = new Vector2(10, 0);
        textRT.offsetMax = new Vector2(-10, 0);

        GameObject placeholderGO = new GameObject("Placeholder");
        placeholderGO.transform.SetParent(inputGO.transform, false);
        Text placeholder = placeholderGO.AddComponent<Text>();
        placeholder.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        placeholder.color = new Color(0.5f, 0.5f, 0.5f);
        placeholder.fontSize = 24;
        placeholder.fontStyle = FontStyle.Italic;
        placeholder.alignment = TextAnchor.MiddleCenter;
        placeholder.text = "Ваш нікнейм...";
        RectTransform phRT = placeholderGO.GetComponent<RectTransform>();
        phRT.anchorMin = Vector2.zero;
        phRT.anchorMax = Vector2.one;
        phRT.offsetMin = new Vector2(10, 0);
        phRT.offsetMax = new Vector2(-10, 0);

        loginInput = inputGO.AddComponent<InputField>();
        loginInput.textComponent = inputText;
        loginInput.placeholder = placeholder;
        loginInput.characterLimit = 20;

        // Кнопка "Грати"
        CreateButton(loginPanel.transform, "PlayBtn", "ГРАТИ",
            new Vector2(0, -60), OnLoginConfirm, new Color(0.2f, 0.7f, 0.2f));
    }

    void OnLoginConfirm()
    {
        string login = loginInput.text.Trim();
        if (string.IsNullOrEmpty(login))
            login = "Player";

        GameData.Instance.PlayerLogin = login;
        loginPanel.SetActive(false);
        hudPanel.SetActive(true);
        Time.timeScale = 1f;
        gameStarted = true;
        GameData.Instance.ResetForNewRun();
        GameData.Instance.StartRun();
    }

    // --- HUD PANEL ---
    void CreateHUDPanel()
    {
        hudPanel = new GameObject("HUDPanel");
        hudPanel.transform.SetParent(canvas.transform, false);
        RectTransform rt = hudPanel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        timerText = CreateText(hudPanel.transform, "Timer", "Час: 02:00",
            new Vector2(-20, -20), 28, Color.white,
            TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(1, 1)).GetComponent<Text>();

        collisionsText = CreateText(hudPanel.transform, "Collisions", "Зіткнення: 0",
            new Vector2(-20, -55), 24, Color.white,
            TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(1, 1)).GetComponent<Text>();

        coinsText = CreateText(hudPanel.transform, "Coins", "Монети: 0",
            new Vector2(20, -20), 28, Color.yellow,
            TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(0, 1)).GetComponent<Text>();

        livesText = CreateText(hudPanel.transform, "Lives", "Життя: 3",
            new Vector2(20, -55), 24, new Color(1f, 0.3f, 0.3f),
            TextAnchor.UpperLeft, new Vector2(0, 1), new Vector2(0, 1)).GetComponent<Text>();

        // Boost slider
        GameObject sliderGO = new GameObject("BoostSlider");
        sliderGO.transform.SetParent(hudPanel.transform, false);
        RectTransform srt = sliderGO.AddComponent<RectTransform>();
        srt.anchorMin = new Vector2(0.5f, 0);
        srt.anchorMax = new Vector2(0.5f, 0);
        srt.pivot = new Vector2(0.5f, 0);
        srt.anchoredPosition = new Vector2(0, 20);
        srt.sizeDelta = new Vector2(300, 20);

        // Background
        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(sliderGO.transform, false);
        Image bgImg = bg.AddComponent<Image>();
        bgImg.color = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        RectTransform bgrt = bg.GetComponent<RectTransform>();
        bgrt.anchorMin = Vector2.zero;
        bgrt.anchorMax = Vector2.one;
        bgrt.offsetMin = Vector2.zero;
        bgrt.offsetMax = Vector2.zero;

        // Fill area
        GameObject fillArea = new GameObject("Fill Area");
        fillArea.transform.SetParent(sliderGO.transform, false);
        RectTransform fart = fillArea.AddComponent<RectTransform>();
        fart.anchorMin = Vector2.zero;
        fart.anchorMax = Vector2.one;
        fart.offsetMin = Vector2.zero;
        fart.offsetMax = Vector2.zero;

        GameObject fill = new GameObject("Fill");
        fill.transform.SetParent(fillArea.transform, false);
        Image fillImg = fill.AddComponent<Image>();
        fillImg.color = new Color(0f, 0.8f, 1f);
        RectTransform fillrt = fill.GetComponent<RectTransform>();
        fillrt.anchorMin = Vector2.zero;
        fillrt.anchorMax = Vector2.one;
        fillrt.offsetMin = Vector2.zero;
        fillrt.offsetMax = Vector2.zero;

        boostSlider = sliderGO.AddComponent<Slider>();
        boostSlider.fillRect = fillrt;
        boostSlider.interactable = false;
        boostSlider.transition = Selectable.Transition.None;

        CreateText(hudPanel.transform, "BoostLabel", "Прискорення",
            new Vector2(0, 45), 16, Color.cyan,
            TextAnchor.LowerCenter, new Vector2(0.5f, 0), new Vector2(0.5f, 0));

        // ESC hint
        CreateText(hudPanel.transform, "EscHint", "ESC — меню",
            new Vector2(-20, -90), 18, new Color(1, 1, 1, 0.5f),
            TextAnchor.UpperRight, new Vector2(1, 1), new Vector2(1, 1));
    }

    // --- MENU PANEL (пауза) ---
    void CreateMenuPanel()
    {
        menuPanel = CreatePanel("MenuPanel", new Color(0, 0, 0, 0.85f));

        CreateText(menuPanel.transform, "MenuTitle", "МЕНЮ",
            new Vector2(0, 120), 42, Color.white);

        CreateButton(menuPanel.transform, "ResumeBtn", "ПРОДОВЖИТИ",
            new Vector2(0, 50), OnResume, new Color(0.2f, 0.7f, 0.2f));

        CreateButton(menuPanel.transform, "RestartBtn", "ПЕРЕЗАПУСК",
            new Vector2(0, -10), OnRestart, new Color(0.8f, 0.6f, 0.1f));

        CreateButton(menuPanel.transform, "RecordsBtn", "РЕКОРДИ",
            new Vector2(0, -70), OnShowRecords, new Color(0.3f, 0.5f, 0.9f));

        CreateButton(menuPanel.transform, "QuitBtn", "ВИЙТИ",
            new Vector2(0, -130), OnQuit, new Color(0.8f, 0.2f, 0.2f));
    }

    void OnResume()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
    }

    void OnRestart()
    {
        menuPanel.SetActive(false);
        Time.timeScale = 1f;
        gameManager.RestartGame();
    }

    void OnShowRecords()
    {
        menuPanel.SetActive(false);
        ShowRecords();
    }

    void OnQuit()
    {
        GameData.Instance.SaveData();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    void ToggleMenu()
    {
        bool show = !menuPanel.activeSelf;
        menuPanel.SetActive(show);
        Time.timeScale = show ? 0f : 1f;
    }

    // --- RECORDS PANEL ---
    void CreateRecordsPanel()
    {
        recordsPanel = CreatePanel("RecordsPanel", new Color(0.05f, 0.05f, 0.15f, 0.95f));

        CreateText(recordsPanel.transform, "RecordsTitle", "ТАБЛИЦЯ РЕКОРДІВ",
            new Vector2(0, 180), 36, Color.yellow);

        // Header
        CreateText(recordsPanel.transform, "RecordsHeader", " #   Логін             Монети   Час        Дата",
            new Vector2(0, 135), 20, Color.gray);

        recordsContentText = CreateText(recordsPanel.transform, "RecordsContent", "",
            new Vector2(0, 0), 22, Color.white).GetComponent<Text>();

        CreateButton(recordsPanel.transform, "RecordsBackBtn", "НАЗАД",
            new Vector2(0, -200), OnRecordsBack, new Color(0.5f, 0.5f, 0.5f));
    }

    void ShowRecords()
    {
        var records = GameData.Instance.Records;
        string text = "";
        for (int i = 0; i < records.Count; i++)
        {
            var r = records[i];
            string login = string.IsNullOrEmpty(r.login) ? "???" : r.login;
            if (login.Length > 12) login = login.Substring(0, 12);
            int min = (int)(r.time / 60f);
            int sec = (int)(r.time % 60f);
            text += $"{i + 1,2}.  {login,-14} {r.coins,6}   {min:00}:{sec:00}     {r.date}\n";
        }
        if (records.Count == 0)
            text = "Ще немає рекордів.";

        recordsContentText.text = text;
        recordsPanel.SetActive(true);
    }

    void OnRecordsBack()
    {
        recordsPanel.SetActive(false);
        // Повернутись у меню або HUD залежно від стану гри
        if (GameData.Instance.IsGameOver || gameManager.IsFinished)
            endPanel.SetActive(true);
        else
            menuPanel.SetActive(true);
    }

    // --- END PANEL (фініш / програш) ---
    void CreateEndPanel()
    {
        endPanel = CreatePanel("EndPanel", new Color(0, 0, 0, 0.9f));

        endMessageText = CreateText(endPanel.transform, "EndMessage", "",
            new Vector2(0, 100), 40, Color.white).GetComponent<Text>();

        CreateButton(endPanel.transform, "EndRestartBtn", "ПЕРЕЗАПУСК",
            new Vector2(0, 10), OnRestart, new Color(0.2f, 0.7f, 0.2f));

        CreateButton(endPanel.transform, "EndRecordsBtn", "РЕКОРДИ",
            new Vector2(0, -50), OnEndShowRecords, new Color(0.3f, 0.5f, 0.9f));

        CreateButton(endPanel.transform, "EndQuitBtn", "ВИЙТИ",
            new Vector2(0, -110), OnQuit, new Color(0.8f, 0.2f, 0.2f));
    }

    void OnEndShowRecords()
    {
        endPanel.SetActive(false);
        ShowRecords();
    }

    void ShowGameOver()
    {
        var data = GameData.Instance;
        endMessageText.text = $"ПРОГРАШ!\n\nМонети: {data.CoinsCollected}    Зіткнення: {data.Collisions}";
        endMessageText.color = new Color(1f, 0.3f, 0.3f);
        endPanel.SetActive(true);
        menuPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    public void ShowFinish()
    {
        var data = GameData.Instance;
        endMessageText.text = $"ФІНІШ!\n\nМонети: {data.CoinsCollected}    Час: {data.ElapsedTime:F1}с";
        endMessageText.color = Color.green;
        endPanel.SetActive(true);
        menuPanel.SetActive(false);
        Time.timeScale = 0f;
    }

    // ===================== ДОПОМІЖНІ МЕТОДИ =====================

    GameObject CreatePanel(string name, Color bgColor)
    {
        GameObject panel = new GameObject(name);
        panel.transform.SetParent(canvas.transform, false);
        RectTransform rt = panel.AddComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        Image img = panel.AddComponent<Image>();
        img.color = bgColor;
        return panel;
    }

    GameObject CreateText(Transform parent, string name, string content,
        Vector2 position, int fontSize, Color color,
        TextAnchor alignment = TextAnchor.MiddleCenter,
        Vector2? anchorMin = null, Vector2? anchorMax = null)
    {
        GameObject go = new GameObject(name);
        go.transform.SetParent(parent, false);
        Text txt = go.AddComponent<Text>();
        txt.text = content;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = fontSize;
        txt.color = color;
        txt.alignment = alignment;
        txt.horizontalOverflow = HorizontalWrapMode.Overflow;
        txt.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(800, 300);
        if (anchorMin.HasValue)
        {
            rt.anchorMin = anchorMin.Value;
            rt.anchorMax = anchorMax ?? anchorMin.Value;
            rt.pivot = anchorMin.Value;
        }
        rt.anchoredPosition = position;
        return go;
    }

    void CreateButton(Transform parent, string name, string label,
        Vector2 position, UnityEngine.Events.UnityAction action, Color color)
    {
        GameObject btnGO = new GameObject(name);
        btnGO.transform.SetParent(parent, false);
        Image btnImg = btnGO.AddComponent<Image>();
        btnImg.color = color;

        RectTransform rt = btnGO.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(260, 45);
        rt.anchoredPosition = position;

        Button btn = btnGO.AddComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.onClick.AddListener(action);

        GameObject txtGO = new GameObject("Label");
        txtGO.transform.SetParent(btnGO.transform, false);
        Text txt = txtGO.AddComponent<Text>();
        txt.text = label;
        txt.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        txt.fontSize = 24;
        txt.color = Color.white;
        txt.alignment = TextAnchor.MiddleCenter;
        RectTransform trt = txtGO.GetComponent<RectTransform>();
        trt.anchorMin = Vector2.zero;
        trt.anchorMax = Vector2.one;
        trt.offsetMin = Vector2.zero;
        trt.offsetMax = Vector2.zero;
    }
}
