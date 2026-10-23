using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Runtime-built station UI. Presentation is created once under the existing Canvas;
/// gameplay scripts report state through this controller instead of owning UI.
/// </summary>
public class StationUIController : MonoBehaviour
{
    public static StationUIController Instance { get; private set; }

    private readonly Color cyan = new Color(0.15f, 0.9f, 1f, 1f);
    private readonly Color white = new Color(0.88f, 0.95f, 1f, 1f);
    private readonly Color orange = new Color(1f, 0.58f, 0.16f, 1f);
    private readonly Color red = new Color(1f, 0.22f, 0.2f, 1f);
    private readonly Color panel = new Color(0.015f, 0.04f, 0.07f, 0.94f);

    private Canvas canvas;
    private HUDController hud;
    private PlayerController playerController;
    private PlayerInteractor interactor;
    private CarriedControl carrier;
    private CameraController cameraController;
    private GameManager gameManager;

    private Text areaText;
    private Text objectiveText;
    private Text statusText;
    private Text promptText;
    private Text carriedText;
    private Text carriedHint;
    private Text reticleText;
    private Text notificationText;
    private Text puzzleMachineText;
    private Text puzzleObjectiveText;
    private Text puzzleValueText;
    private Text puzzleRangeText;
    private Text puzzleFeedbackText;
    private Text tutorialText;
    private Text endingText;
    private GameObject promptRoot;
    private GameObject puzzleRoot;
    private GameObject pauseRoot;
    private GameObject tutorialRoot;
    private GameObject endingRoot;
    private GameObject carriedIcon;
    private Slider puzzleSlider;
    private PuzzleController activePuzzle;
    private Coroutine notificationRoutine;
    private bool paused;
    private bool tutorialDismissed;
    private float previousTimeScale = 1f;
    private float statusTimer;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;
        hud = GetComponent<HUDController>();
    }

    void Start()
    {
        canvas = FindCanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[StationUIController] No Canvas found; UI was not built.");
            return;
        }

        playerController = FindPlayer()?.GetComponent<PlayerController>();
        interactor = FindPlayer()?.GetComponent<PlayerInteractor>();
        carrier = FindPlayer()?.GetComponent<CarriedControl>();
        cameraController = Camera.main != null ? Camera.main.GetComponent<CameraController>() : null;
        gameManager = FindObjectOfType<GameManager>();
        BuildUI();
        SetObjective("Objective: SEARCH THE ARRIVAL HALL");
        SetSystemStatus("UNSTABLE");
        SetCarriedControl(carrier != null ? carrier.carriedControl : null);
        ShowTutorial();
        SetCursor(false);
    }

    void Update()
    {
        if (canvas == null)
            return;

        UpdateArea();
        UpdatePuzzleValue();
        statusTimer -= Time.unscaledDeltaTime;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (activePuzzle != null)
                ClosePuzzle();
            else if (endingRoot == null || !endingRoot.activeSelf)
                TogglePause();
        }

        if (!tutorialDismissed && Keyboard.current != null &&
            (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame))
            DismissTutorial();
    }

    private Canvas FindCanvas()
    {
        GameObject canvasObject = GameObject.Find("Canvas");
        return canvasObject != null ? canvasObject.GetComponent<Canvas>() : FindObjectOfType<Canvas>();
    }

    private GameObject FindPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        return player;
    }

    private void BuildUI()
    {
        RectTransform root = CreateRect("StationUI", canvas.transform, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
        CreatePanel(root, "TopLeft", new Vector2(0, 1), new Vector2(0, 1), new Vector2(32, -28), new Vector2(430, 150), out RectTransform topLeft);
        AddText(topLeft, "SystemLabel", "CIS // STATION 07", 18, cyan, TextAnchor.UpperLeft, new Vector2(16, -12), new Vector2(390, 28));
        areaText = AddText(topLeft, "Area", "ARRIVAL HALL", 14, orange, TextAnchor.UpperLeft, new Vector2(16, -45), new Vector2(390, 24));
        objectiveText = AddText(topLeft, "Objective", "Objective: SEARCH THE ARRIVAL HALL", 14, white, TextAnchor.UpperLeft, new Vector2(16, -78), new Vector2(390, 52));

        CreatePanel(root, "TopRight", new Vector2(1, 1), new Vector2(1, 1), new Vector2(-32, -28), new Vector2(220, 58), out RectTransform topRight);
        statusText = AddText(topRight, "Status", "●  UNSTABLE", 15, orange, TextAnchor.MiddleCenter, new Vector2(0, 0), new Vector2(220, 58));

        CreatePanel(root, "BottomLeft", new Vector2(0, 0), new Vector2(0, 0), new Vector2(32, 28), new Vector2(330, 82), out RectTransform bottomLeft);
        carriedText = AddText(bottomLeft, "Carried", "CARRYING: NONE", 15, cyan, TextAnchor.UpperLeft, new Vector2(16, -12), new Vector2(285, 25));
        carriedHint = AddText(bottomLeft, "CarryHint", "Find a compatible interface", 12, white, TextAnchor.UpperLeft, new Vector2(16, -43), new Vector2(285, 22));
        carriedIcon = CreateIcon(bottomLeft, new Vector2(-16, -41));

        promptRoot = CreatePanel(root, "Prompt", new Vector2(.5f, 0), new Vector2(.5f, 0), new Vector2(0, 126), new Vector2(500, 54), out RectTransform promptPanel);
        promptText = AddText(promptPanel, "PromptText", "E  //  INTERACT", 17, cyan, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(500, 54));
        promptRoot.SetActive(false);

        reticleText = AddText(root, "Reticle", "+", 25, new Color(0.45f, .9f, 1f, .8f), TextAnchor.MiddleCenter, Vector2.zero, new Vector2(34, 34));
        notificationText = AddText(root, "Notification", "", 17, white, TextAnchor.MiddleCenter, new Vector2(0, -210), new Vector2(720, 54));

        BuildPuzzle(root);
        BuildPause(root);
        BuildTutorial(root);
        BuildEnding(root);
    }

    private void BuildPuzzle(RectTransform root)
    {
        puzzleRoot = CreatePanel(root, "PuzzleModal", new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(700, 470), out RectTransform body);
        AddText(body, "Kicker", "MACHINE INTERFACE // MANUAL OVERRIDE", 13, cyan, TextAnchor.UpperLeft, new Vector2(28, -22), new Vector2(640, 22));
        puzzleMachineText = AddText(body, "Machine", "MACHINE", 28, white, TextAnchor.UpperLeft, new Vector2(28, -58), new Vector2(640, 44));
        puzzleObjectiveText = AddText(body, "PuzzleObjective", "OBJECTIVE", 14, orange, TextAnchor.UpperLeft, new Vector2(28, -108), new Vector2(640, 28));
        puzzleRangeText = AddText(body, "Range", "VALID RANGE 0.60 - 0.80", 13, cyan, TextAnchor.UpperLeft, new Vector2(28, -150), new Vector2(640, 24));
        GameObject sliderObject = new GameObject("PuzzleSlider");
        sliderObject.transform.SetParent(body, false);
        RectTransform sliderRect = sliderObject.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(.5f, .5f); sliderRect.anchorMax = new Vector2(.5f, .5f); sliderRect.pivot = new Vector2(.5f, .5f); sliderRect.anchoredPosition = new Vector2(0, -15); sliderRect.sizeDelta = new Vector2(560, 34);
        Image sliderBg = sliderObject.AddComponent<Image>(); sliderBg.color = new Color(.04f, .12f, .18f, 1f);
        puzzleSlider = sliderObject.AddComponent<Slider>(); puzzleSlider.minValue = 0f; puzzleSlider.maxValue = 1f; puzzleSlider.value = 0f;
        GameObject fill = new GameObject("Fill"); fill.transform.SetParent(sliderObject.transform, false); RectTransform fillRect = fill.AddComponent<RectTransform>(); fillRect.anchorMin = Vector2.zero; fillRect.anchorMax = Vector2.one; fillRect.offsetMin = new Vector2(4, 8); fillRect.offsetMax = new Vector2(-4, -8); Image fillImage = fill.AddComponent<Image>(); fillImage.color = cyan; puzzleSlider.fillRect = fillRect;
        GameObject handle = new GameObject("Handle"); handle.transform.SetParent(sliderObject.transform, false); RectTransform handleRect = handle.AddComponent<RectTransform>(); handleRect.sizeDelta = new Vector2(26, 46); Image handleImage = handle.AddComponent<Image>(); handleImage.color = white; puzzleSlider.handleRect = handleRect; puzzleSlider.targetGraphic = handleImage;
        puzzleValueText = AddText(body, "Value", "VALUE 0.00", 16, white, TextAnchor.MiddleCenter, new Vector2(0, -68), new Vector2(220, 34));
        puzzleFeedbackText = AddText(body, "Feedback", "", 14, white, TextAnchor.MiddleCenter, new Vector2(0, -110), new Vector2(600, 30));
        Button confirm = CreateButton(body, "ACTIVATE", new Vector2(-90, -170), new Vector2(180, 46), cyan); confirm.onClick.AddListener(ConfirmPuzzle);
        Button close = CreateButton(body, "CLOSE  [ESC]", new Vector2(120, -170), new Vector2(180, 46), new Color(.25f, .35f, .42f, 1f)); close.onClick.AddListener(ClosePuzzle);
        puzzleRoot.SetActive(false);
    }

    private void BuildPause(RectTransform root)
    {
        pauseRoot = CreatePanel(root, "PauseModal", new Vector2(.5f, .5f), new Vector2(.5f, .5f), Vector2.zero, new Vector2(440, 300), out RectTransform body);
        AddText(body, "Title", "GAME PAUSED", 28, cyan, TextAnchor.MiddleCenter, new Vector2(0, 78), new Vector2(420, 48));
        Button resume = CreateButton(body, "RESUME", new Vector2(0, 20), new Vector2(230, 42), cyan); resume.onClick.AddListener(() => SetPaused(false));
        Button quit = CreateButton(body, "QUIT TO DESKTOP", new Vector2(0, -42), new Vector2(230, 42), red); quit.onClick.AddListener(QuitGame);
        pauseRoot.SetActive(false);
    }

    private void BuildTutorial(RectTransform root)
    {
        tutorialRoot = CreatePanel(root, "Tutorial", new Vector2(.5f, 1), new Vector2(.5f, 1), new Vector2(0, -215), new Vector2(620, 165), out RectTransform body);
        tutorialText = AddText(body, "TutorialText", "FIRST CONTACT\nWASD  MOVE     MOUSE  LOOK     E  INTERACT\nDETACH A CONTROL FROM ITS SOURCE\nCARRY ONE CONTROL AT A TIME\nINSTALL IT INTO A COMPATIBLE MACHINE\n\nCLICK OR PRESS ANY KEY TO DISMISS", 14, white, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(600, 150));
    }

    private void BuildEnding(RectTransform root)
    {
        endingRoot = CreatePanel(root, "EndingModal", Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero, out RectTransform body);
        endingRoot.GetComponent<Image>().color = new Color(.005f, .02f, .035f, .97f);
        endingText = AddText(body, "EndingText", "YOU MADE IT OUT\n\nSYSTEM RESTORED. CONTROL TRANSFER COMPLETE.\n\nTHE STATION IS QUIET AGAIN.", 24, cyan, TextAnchor.MiddleCenter, Vector2.zero, new Vector2(900, 260));
        Button quit = CreateButton(body, "QUIT TO DESKTOP", new Vector2(0, -190), new Vector2(240, 46), red); quit.onClick.AddListener(QuitGame);
        endingRoot.SetActive(false);
    }

    public void OpenPuzzle(PuzzleController puzzle)
    {
        if (puzzle == null || endingRoot.activeSelf) return;
        activePuzzle = puzzle;
        puzzleRoot.SetActive(true);
        puzzleMachineText.text = puzzle.MachineName;
        puzzleObjectiveText.text = puzzle.ObjectiveText;
        puzzleRangeText.text = string.Format("VALID RANGE  {0:0.00} - {1:0.00}", puzzle.targetMin, puzzle.targetMax);
        puzzleSlider.value = puzzle.puzzleSlider != null ? puzzle.puzzleSlider.value : 0f;
        puzzle.puzzleSlider = puzzleSlider;
        puzzle.puzzlePanel = puzzleRoot;
        puzzleFeedbackText.text = "SET THE SLIDER, THEN ACTIVATE";
        SetGameplayLocked(true);
    }

    public void ClosePuzzle()
    {
        if (activePuzzle != null)
            activePuzzle.ClosePanel();
        else
            SetGameplayLocked(false);
    }

    public bool ConfirmPuzzle()
    {
        return activePuzzle != null && activePuzzle.ConfirmPuzzle();
    }

    public void PuzzleFeedback(string text, Color color)
    {
        if (puzzleFeedbackText != null) { puzzleFeedbackText.text = text; puzzleFeedbackText.color = color; }
    }

    public void PuzzleClosed(PuzzleController puzzle)
    {
        if (activePuzzle == puzzle) activePuzzle = null;
        if (puzzleRoot != null) puzzleRoot.SetActive(false);
        SetGameplayLocked(false);
    }

    public void SetObjective(string text) { if (objectiveText != null) objectiveText.text = text.ToUpperInvariant(); }
    public void ShowPrompt(string text) { if (promptRoot != null) promptRoot.SetActive(true); if (promptText != null) promptText.text = "E  //  " + text.ToUpperInvariant(); if (reticleText != null) reticleText.color = orange; }
    public void HidePrompt() { if (promptRoot != null) promptRoot.SetActive(false); if (reticleText != null) reticleText.color = new Color(.45f, .9f, 1f, .8f); }
    public void SetSystemStatus(string status) { if (statusText != null) { statusText.text = "●  " + status; statusText.color = status == "RESTORED" ? cyan : status == "OFFLINE" ? red : orange; } }
    public void SetCarriedControl(UIControlData control) { bool carrying = control != null; if (carriedText != null) carriedText.text = carrying ? "CARRYING: " + control.displayName.ToUpperInvariant() : "CARRYING: NONE"; if (carriedHint != null) carriedHint.text = carrying ? "Find a compatible interface" : "Detach a control from a source panel"; if (carriedIcon != null) carriedIcon.SetActive(carrying); }
    public void ShowTutorial() { tutorialDismissed = false; if (tutorialRoot != null) tutorialRoot.SetActive(true); }
    public void DismissTutorial() { tutorialDismissed = true; if (tutorialRoot != null) tutorialRoot.SetActive(false); }
    public void ShowNotification(string message) { if (notificationRoutine != null) StopCoroutine(notificationRoutine); notificationRoutine = StartCoroutine(NotificationRoutine(message)); }
    public void ShowEnding() { if (endingRoot != null) endingRoot.SetActive(true); SetGameplayLocked(true); SetSystemStatus("RESTORED"); }

    public void TogglePause() { SetPaused(!paused); }
    private void SetPaused(bool value) { paused = value; if (pauseRoot != null) pauseRoot.SetActive(value); if (value) { previousTimeScale = Time.timeScale; Time.timeScale = 0f; } else { Time.timeScale = previousTimeScale > 0f ? previousTimeScale : 1f; } SetGameplayLocked(value); }
    private void SetGameplayLocked(bool locked) { if (playerController != null) playerController.gameplayLocked = locked; if (cameraController != null) cameraController.inputLocked = locked; SetCursor(locked); }
    private void SetCursor(bool visible) { Cursor.visible = visible; Cursor.lockState = visible ? CursorLockMode.None : CursorLockMode.Locked; }
    private void QuitGame() { Application.Quit(); }

    private IEnumerator NotificationRoutine(string message) { if (notificationText != null) { notificationText.text = message.ToUpperInvariant(); notificationText.color = white; } yield return new WaitForSecondsRealtime(2.6f); if (notificationText != null) notificationText.text = ""; }
    private void UpdatePuzzleValue() { if (activePuzzle != null && puzzleValueText != null && puzzleSlider != null) puzzleValueText.text = "VALUE  " + puzzleSlider.value.ToString("0.00"); }
    private void UpdateArea() { if (areaText == null || playerController == null) return; float x = playerController.transform.position.x; areaText.text = x < -10f ? "ARRIVAL HALL" : x < 0f ? "TICKET CONCOURSE" : x < 10f ? "CONTROL CORNER" : "PLATFORM 07"; }

    private GameObject CreatePanel(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size, out RectTransform rect)
    { GameObject obj = new GameObject(name); obj.transform.SetParent(parent, false); rect = obj.AddComponent<RectTransform>(); rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; Image image = obj.AddComponent<Image>(); image.color = panel; Outline outline = obj.AddComponent<Outline>(); outline.effectColor = new Color(.15f, .65f, .8f, .35f); outline.effectDistance = new Vector2(1f, -1f); return obj; }
    private RectTransform CreateRect(string name, Transform parent, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size) { GameObject obj = new GameObject(name); obj.transform.SetParent(parent, false); RectTransform rect = obj.AddComponent<RectTransform>(); rect.anchorMin = anchorMin; rect.anchorMax = anchorMax; rect.offsetMin = position; rect.offsetMax = size; return rect; }
    private Text AddText(RectTransform parent, string name, string content, int size, Color color, TextAnchor alignment, Vector2 position, Vector2 dimensions) { GameObject obj = new GameObject(name); obj.transform.SetParent(parent, false); RectTransform rect = obj.AddComponent<RectTransform>(); rect.anchorMin = new Vector2(.5f, .5f); rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = dimensions; Text text = obj.AddComponent<Text>(); text.text = content; text.fontSize = size; text.color = color; text.alignment = alignment; text.horizontalOverflow = HorizontalWrapMode.Wrap; text.verticalOverflow = VerticalWrapMode.Overflow; return text; }
    private Button CreateButton(RectTransform parent, string label, Vector2 position, Vector2 size, Color accent) { GameObject obj = new GameObject(label); obj.transform.SetParent(parent, false); RectTransform rect = obj.AddComponent<RectTransform>(); rect.anchorMin = new Vector2(.5f, .5f); rect.anchorMax = new Vector2(.5f, .5f); rect.pivot = new Vector2(.5f, .5f); rect.anchoredPosition = position; rect.sizeDelta = size; Image image = obj.AddComponent<Image>(); image.color = new Color(accent.r * .18f, accent.g * .18f, accent.b * .18f, .98f); Button button = obj.AddComponent<Button>(); ColorBlock colors = button.colors; colors.normalColor = image.color; colors.highlightedColor = new Color(accent.r * .3f, accent.g * .3f, accent.b * .3f, 1f); colors.pressedColor = accent; button.colors = colors; AddText(rect, "Label", label, 13, accent, TextAnchor.MiddleCenter, Vector2.zero, size); return button; }
    private GameObject CreateIcon(RectTransform parent, Vector2 position) { GameObject obj = new GameObject("CarriedIcon"); obj.transform.SetParent(parent, false); RectTransform rect = obj.AddComponent<RectTransform>(); rect.anchorMin = new Vector2(1, .5f); rect.anchorMax = new Vector2(1, .5f); rect.pivot = new Vector2(1, .5f); rect.anchoredPosition = position; rect.sizeDelta = new Vector2(38, 38); Image image = obj.AddComponent<Image>(); image.color = orange; return obj; }
}
