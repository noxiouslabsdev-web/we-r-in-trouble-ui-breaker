using UnityEngine;

/// <summary>
/// Tracks quest/objective progression through the station.
/// Holds a list of objective strings and advances them as puzzles are solved.
/// Updates the HUD when objectives change.
/// </summary>
public class QuestManager : MonoBehaviour
{
    [Header("Objectives")]
    [Tooltip("Ordered list of objective descriptions.")]
    public string[] objectives = new string[]
    {
        "Find a way out of the station.",
        "Find a UI Module to fix the ticket machine.",
        "Solve the ticket machine puzzle.",
        "Reach the control corner and fix the power.",
        "Reach the platform and escape."
    };

    [Tooltip("Current objective index (0-based).")]
    public int currentObjective = 0;

    [Header("References")]
    [Tooltip("HUD controller to update objective text.")]
    public HUDController hud;

    void Start()
    {
        UpdateHUD();
    }

    /// <summary>
    /// Advance to the next objective.
    /// </summary>
    public void AdvanceObjective()
    {
        if (currentObjective < objectives.Length - 1)
        {
            currentObjective++;
            UpdateHUD();
            Debug.Log($"[QuestManager] New objective: {objectives[currentObjective]}");
        }
    }

    /// <summary>
    /// Set a specific objective by index.
    /// </summary>
    public void SetObjective(int index)
    {
        if (index >= 0 && index < objectives.Length)
        {
            currentObjective = index;
            UpdateHUD();
        }
    }

    private void UpdateHUD()
    {
        if (hud != null && currentObjective < objectives.Length)
        {
            hud.SetObjective($"Objective: {objectives[currentObjective]}");
        }
    }
}
