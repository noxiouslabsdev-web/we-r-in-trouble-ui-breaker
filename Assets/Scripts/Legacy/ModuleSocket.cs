using UnityEngine;

/// <summary>
/// A socket on a machine that accepts a compatible UI Module.
/// When the player interacts and has the right module, it installs the module
/// and opens the associated puzzle UI panel.
/// </summary>
public class ModuleSocket : MonoBehaviour, IInteractable
{
    [Header("Socket Config")]
    [Tooltip("The module ID this socket accepts (e.g. 'volume').")]
    public string acceptedModuleID = "volume";

    [Tooltip("Text shown when player is in range.")]
    public string promptTextInstall = "Install module";
    public string promptTextInstalled = "Module installed";
    public string promptTextNoModule = "Need a module";

    [Header("References")]
    [Tooltip("The puzzle panel UI to show when module is installed.")]
    public GameObject puzzlePanel;

    [Tooltip("The puzzle controller on this machine.")]
    public PuzzleController puzzleController;

    [Tooltip("Visual indicator: light or object that shows installed state.")]
    public GameObject installedIndicator;

    // State
    private bool isInstalled = false;

    public bool IsInstalled => isInstalled;

    public void Interact(GameObject player)
    {
        if (isInstalled)
        {
            // Already installed — open the puzzle panel again.
            if (puzzlePanel != null)
                puzzlePanel.SetActive(true);
            if (puzzleController != null)
                puzzleController.OnPanelOpened();
            return;
        }

        ModuleInventory inventory = player.GetComponent<ModuleInventory>();
        if (inventory == null)
        {
            Debug.LogWarning("[ModuleSocket] Player has no ModuleInventory!");
            return;
        }

        if (inventory.HasModule(acceptedModuleID))
        {
            // Install the module.
            inventory.RemoveModule(acceptedModuleID);
            isInstalled = true;

            if (installedIndicator != null)
                installedIndicator.SetActive(true);

            if (puzzlePanel != null)
                puzzlePanel.SetActive(true);

            if (puzzleController != null)
                puzzleController.OnModuleInstalled();

            Debug.Log($"[ModuleSocket] Module '{acceptedModuleID}' installed into {gameObject.name}");
        }
        else
        {
            Debug.Log($"[ModuleSocket] Player does not have module '{acceptedModuleID}'");
        }
    }
}
