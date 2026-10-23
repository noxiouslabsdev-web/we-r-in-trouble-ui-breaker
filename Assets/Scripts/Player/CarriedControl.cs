using UnityEngine;

/// <summary>
/// Tracks the single UI control the player is currently carrying.
/// The player can only carry one control at a time.
/// When a control is detached from a source panel, it is stored here.
/// When a control is installed into a target, it is removed from here.
/// Replaces the old ModuleInventory.
/// </summary>
public class CarriedControl : MonoBehaviour
{
    [Tooltip("The control data currently being carried. Null if carrying nothing.")]
    public UIControlData carriedControl;

    [Tooltip("Visual object shown above/around the player when carrying a control (optional).")]
    public GameObject carryVisual;

    /// <summary>
    /// True if the player is currently carrying a control.
    /// </summary>
    public bool IsCarrying => carriedControl != null;

    /// <summary>
    /// Pick up (detach) a control from a source panel.
    /// </summary>
    public void PickUpControl(UIControlData control)
    {
        if (carriedControl != null)
        {
            Debug.Log("[CarriedControl] Already carrying a control — cannot pick up another.");
            return;
        }

        carriedControl = control;
        if (StationUIController.Instance != null)
        {
            StationUIController.Instance.SetCarriedControl(control);
            StationUIController.Instance.ShowNotification("CONTROL DETACHED  //  " + control.displayName);
        }
        if (carryVisual != null)
            carryVisual.SetActive(true);

        Debug.Log($"[CarriedControl] Now carrying: {control.displayName}");
    }

    /// <summary>
    /// Install the carried control into a target. Returns true if successful.
    /// </summary>
    public bool InstallControl()
    {
        if (carriedControl == null)
        {
            Debug.Log("[CarriedControl] Nothing to install.");
            return false;
        }

        UIControlData installed = carriedControl;
        carriedControl = null;
        if (StationUIController.Instance != null)
            StationUIController.Instance.SetCarriedControl(null);

        if (carryVisual != null)
            carryVisual.SetActive(false);

        Debug.Log($"[CarriedControl] Installed: {installed.displayName}");
        return true;
    }

    /// <summary>
    /// Check if the player is carrying a control with the given ID.
    /// </summary>
    public bool HasControl(string controlID)
    {
        return carriedControl != null && carriedControl.controlID == controlID;
    }

    /// <summary>
    /// Get the ID of the currently carried control, or null if carrying nothing.
    /// </summary>
    public string GetCarriedControlID()
    {
        return carriedControl != null ? carriedControl.controlID : null;
    }
}
