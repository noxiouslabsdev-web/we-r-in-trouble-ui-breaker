using UnityEngine;

public class GameManager : MonoBehaviour
{
    public bool gameEnded = false;
    public GameObject endingPanel;
    public QuestManager questManager;

    private PuzzleController[] requiredPuzzles;

    void Awake()
    {
        requiredPuzzles = FindObjectsOfType<PuzzleController>();
    }

    public void TriggerEnding()
    {
        if (gameEnded) return;
        foreach (PuzzleController puzzle in requiredPuzzles)
        {
            if (puzzle != null && !puzzle.isSolved)
            {
                if (StationUIController.Instance != null)
                    StationUIController.Instance.ShowNotification("EXIT LOCKED  //  RESTORE ALL SYSTEMS");
                return;
            }
        }

        gameEnded = true;
        if (endingPanel != null) endingPanel.SetActive(true);
        if (questManager != null && questManager.objectives.Length > 0)
            questManager.SetObjective(questManager.objectives.Length - 1);
        if (StationUIController.Instance != null) StationUIController.Instance.ShowEnding();
        Debug.Log("[GameManager] Ending triggered! Player escaped.");
    }
}
