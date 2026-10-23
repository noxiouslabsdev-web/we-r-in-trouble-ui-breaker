using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple HUD that shows the current objective and interaction prompt.
/// Uses legacy UnityEngine.UI.Text — no extra asset imports needed.
/// All fields are optional — assign whichever you use in the Inspector.
/// </summary>
public class HUDController : MonoBehaviour
{
    [Header("Objective Display")]
    [Tooltip("Text element for the current objective.")]
    public Text objectiveText;

    [Header("Interaction Prompt")]
    [Tooltip("Text element for interaction prompt (e.g. 'Press E').")]
    public Text promptText;

    [Tooltip("GameObject that holds the prompt (toggled on/off).")]
    public GameObject promptContainer;

    [Header("Module Notification")]
    [Tooltip("Text for showing module pickup / event notifications.")]
    public Text notificationText;

    [Tooltip("How long the notification stays visible (seconds).")]
    public float notificationDuration = 3f;

    private float notificationTimer = 0f;
    private StationUIController stationUI;

    void Awake()
    {
        stationUI = GetComponent<StationUIController>();
        if (stationUI == null)
            stationUI = gameObject.AddComponent<StationUIController>();
    }

    void Update()
    {
        if (notificationTimer > 0f)
        {
            notificationTimer -= Time.deltaTime;
            if (notificationTimer <= 0f)
            {
                SetNotificationText("");
            }
        }
    }

    /// <summary>
    /// Update the objective text.
    /// </summary>
    public void SetObjective(string text)
    {
        if (objectiveText != null)
            objectiveText.text = text;
        if (stationUI != null)
            stationUI.SetObjective(text);
    }

    /// <summary>
    /// Show the interaction prompt with given text.
    /// </summary>
    public void ShowPrompt(string text)
    {
        if (promptText != null)
            promptText.text = text;
        if (promptContainer != null)
            promptContainer.SetActive(true);
        if (stationUI != null)
            stationUI.ShowPrompt(text);
    }

    /// <summary>
    /// Hide the interaction prompt.
    /// </summary>
    public void HidePrompt()
    {
        if (promptContainer != null)
            promptContainer.SetActive(false);
        if (stationUI != null)
            stationUI.HidePrompt();
    }

    /// <summary>
    /// Show a temporary notification (e.g. "Picked up Volume Module").
    /// </summary>
    public void ShowNotification(string text)
    {
        SetNotificationText(text);
        notificationTimer = notificationDuration;
        if (stationUI != null)
            stationUI.ShowNotification(text);
    }

    public void SetCarriedControl(UIControlData control) { if (stationUI != null) stationUI.SetCarriedControl(control); }
    public void SetSystemStatus(string status) { if (stationUI != null) stationUI.SetSystemStatus(status); }
    public void ShowTutorial() { if (stationUI != null) stationUI.ShowTutorial(); }
    public void ShowEnding() { if (stationUI != null) stationUI.ShowEnding(); }

    private void SetNotificationText(string text)
    {
        if (notificationText != null)
            notificationText.text = text;
    }
}
