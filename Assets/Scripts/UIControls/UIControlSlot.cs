using UnityEngine;

/// <summary>
/// Represents a slot on a source UI panel that holds a detachable control.
/// The player can interact with this slot to detach (remove) the control.
/// When the control is removed, the slot's visual shows an empty state.
/// This replaces ModulePickup — the control originates from an existing UI panel,
/// not a free-floating pickup in the world.
/// </summary>
public class UIControlSlot : MonoBehaviour, IInteractable
{
    [Header("Control")]
    [Tooltip("The control data this slot holds initially.")]
    public UIControlData controlData;

    [Header("Visuals")]
    [Tooltip("GameObject shown when the control is present in this slot.")]
    public GameObject controlPresentVisual;

    [Tooltip("GameObject shown when the control has been removed (empty slot).")]
    public GameObject controlAbsentVisual;

    [Header("State")]
    [Tooltip("True if the control is still in this slot.")]
    public bool hasControl = true;

    /// <summary>
    /// Prompt text for the interactor.
    /// </summary>
    public string PromptText => hasControl ? "Detach " + (controlData != null ? controlData.displayName : "control") : "Empty slot";

    public void Interact(GameObject player)
    {
        if (!hasControl || controlData == null)
        {
            Debug.Log($"[UIControlSlot] {gameObject.name} — slot is empty.");
            return;
        }

        CarriedControl carrier = player.GetComponent<CarriedControl>();
        if (carrier == null)
        {
            Debug.LogWarning("[UIControlSlot] Player has no CarriedControl component!");
            return;
        }

        if (carrier.IsCarrying)
        {
            Debug.Log("[UIControlSlot] Player is already carrying a control.");
            if (StationUIController.Instance != null)
                StationUIController.Instance.ShowNotification("HANDS OCCUPIED — INSTALL CURRENT CONTROL FIRST");
            return;
        }

        // Detach the control from this slot.
        carrier.PickUpControl(controlData);
        hasControl = false;

        // Update visuals: hide the control, show the empty slot.
        if (controlPresentVisual != null)
            controlPresentVisual.SetActive(false);
        if (controlAbsentVisual != null)
            controlAbsentVisual.SetActive(true);

        Debug.Log($"[UIControlSlot] Detached {controlData.displayName} from {gameObject.name}");
    }
}
