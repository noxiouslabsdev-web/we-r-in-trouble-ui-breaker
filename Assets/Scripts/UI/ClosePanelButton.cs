using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Simple helper that calls PuzzleController.ClosePanel when a button is clicked.
/// Attach to the close button and wire the puzzleController reference.
/// </summary>
[RequireComponent(typeof(Button))]
public class ClosePanelButton : MonoBehaviour
{
    [Tooltip("The puzzle controller whose panel should be closed.")]
    public PuzzleController puzzleController;

    private Button button;

    void Awake()
    {
        button = GetComponent<Button>();
    }

    void Start()
    {
        if (button != null)
            button.onClick.AddListener(OnClick);
    }

    private void OnClick()
    {
        if (puzzleController != null)
            puzzleController.ClosePanel();
    }
}
