using UnityEngine;

public class UIControlTarget : MonoBehaviour, IInteractable
{
    public string acceptedControlID = "plus";
    public GameObject installedVisual;
    public GameObject emptyVisual;
    public PuzzleController puzzleController;
    public GameObject puzzlePanel;
    public bool isInstalled = false;

    public bool IsInstalled => isInstalled;
    public string PromptText => isInstalled ? "Activate machine" : "Install control";

    public void Interact(GameObject player)
    {
        if (isInstalled)
        {
            OpenPuzzle();
            return;
        }

        CarriedControl carrier = player.GetComponent<CarriedControl>();
        if (carrier == null)
        {
            Debug.LogWarning("[UIControlTarget] Player has no CarriedControl component!");
            return;
        }
        if (!carrier.IsCarrying)
        {
            if (StationUIController.Instance != null)
                StationUIController.Instance.ShowNotification("NO CONTROL CARRIED");
            return;
        }
        if (!carrier.HasControl(acceptedControlID))
        {
            if (StationUIController.Instance != null)
                StationUIController.Instance.ShowNotification("INCOMPATIBLE CONTROL");
            Debug.Log($"[UIControlTarget] Carried control '{carrier.GetCarriedControlID()}' is not compatible. Needs '{acceptedControlID}'.");
            return;
        }

        string controlName = carrier.carriedControl.displayName;
        carrier.InstallControl();
        isInstalled = true;
        if (installedVisual != null) installedVisual.SetActive(true);
        if (emptyVisual != null) emptyVisual.SetActive(false);
        if (puzzleController != null) puzzleController.OnModuleInstalled();
        if (StationUIController.Instance != null)
        {
            StationUIController.Instance.ShowNotification("CONTROL INSTALLED  //  " + controlName + "  //  " + gameObject.name);
            OpenPuzzle();
        }
        else if (puzzlePanel != null)
            puzzlePanel.SetActive(true);

        Debug.Log($"[UIControlTarget] Control '{acceptedControlID}' installed into {gameObject.name}");
    }

    private void OpenPuzzle()
    {
        if (StationUIController.Instance != null && puzzleController != null)
            StationUIController.Instance.OpenPuzzle(puzzleController);
        else
        {
            if (puzzlePanel != null) puzzlePanel.SetActive(true);
            if (puzzleController != null) puzzleController.OnPanelOpened();
        }
    }
}
