using UnityEngine;
using UnityEngine.UI;

public class PuzzleController : MonoBehaviour
{
    public Slider puzzleSlider;
    public float targetMin = 0.6f;
    public float targetMax = 0.8f;
    public GameObject successLight;
    public GameObject failureLight;
    public AudioSource successSound;
    public DoorController[] doorsToOpen;
    public QuestManager questManager;
    public GameObject puzzlePanel;
    public CameraController cameraController;
    public bool isSolved = false;
    public bool moduleInstalled = false;
    public string machineName = "MACHINE INTERFACE";
    public string objectiveText = "CALIBRATE MACHINE";

    public string MachineName => string.IsNullOrEmpty(machineName) ? gameObject.name : machineName;
    public string ObjectiveText => string.IsNullOrEmpty(objectiveText) ? "CALIBRATE MACHINE" : objectiveText;

    void Update()
    {
        bool panelOpen = puzzlePanel != null && puzzlePanel.activeSelf;
        if (panelOpen) LockCamera();
        if (!moduleInstalled || isSolved || puzzleSlider == null || !panelOpen) return;
        if (StationUIController.Instance != null)
            StationUIController.Instance.SetSystemStatus("UNSTABLE");
    }

    public void OnModuleInstalled()
    {
        moduleInstalled = true;
        Debug.Log($"[PuzzleController] Control installed on {gameObject.name}");
    }

    public void OnPanelOpened()
    {
        LockCamera();
        if (StationUIController.Instance != null)
            StationUIController.Instance.OpenPuzzle(this);
    }

    public bool ConfirmPuzzle()
    {
        if (isSolved || puzzleSlider == null) return false;
        float value = puzzleSlider.value;
        if (value < targetMin || value > targetMax)
        {
            if (failureLight != null) failureLight.SetActive(true);
            if (StationUIController.Instance != null)
                StationUIController.Instance.PuzzleFeedback("CALIBRATION FAILED  //  MOVE INTO VALID RANGE", new Color(1f, .3f, .25f));
            return false;
        }
        Solve();
        return true;
    }

    private void Solve()
    {
        isSolved = true;
        if (successLight != null) successLight.SetActive(true);
        if (failureLight != null) failureLight.SetActive(false);
        if (successSound != null) successSound.Play();
        foreach (DoorController door in doorsToOpen ?? new DoorController[0])
            if (door != null) door.Open();
        if (questManager != null) questManager.AdvanceObjective();
        if (StationUIController.Instance != null)
        {
            StationUIController.Instance.PuzzleFeedback("SYSTEM RESTORED  //  ACCESS GRANTED", new Color(.2f, 1f, .65f));
            StationUIController.Instance.ShowNotification("PUZZLE SOLVED  //  " + gameObject.name);
            StationUIController.Instance.SetSystemStatus("RESTORED");
        }
        Debug.Log($"[PuzzleController] Puzzle SOLVED on {gameObject.name}!");
    }

    public void ClosePanel()
    {
        if (puzzlePanel != null) puzzlePanel.SetActive(false);
        UnlockCamera();
        if (StationUIController.Instance != null) StationUIController.Instance.PuzzleClosed(this);
    }

    private void LockCamera()
    {
        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController != null) cameraController.inputLocked = true;
    }

    private void UnlockCamera()
    {
        if (cameraController == null && Camera.main != null)
            cameraController = Camera.main.GetComponent<CameraController>();
        if (cameraController != null) cameraController.inputLocked = false;
    }
}
