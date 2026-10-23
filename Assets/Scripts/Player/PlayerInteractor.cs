using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Detects nearby interactables using OverlapSphere and fires
/// interaction when the player presses the Interact key.
/// Updates the HUD prompt when an interactable is in range.
/// Supports both the new detachable UI control system (UIControlSlot, UIControlTarget)
/// and the old module system (ModulePickup, ModuleSocket) for backward compatibility.
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    [Header("Detection")]
    [Tooltip("Radius for detecting interactables.")]
    public float detectionRadius = 2f;

    [Tooltip("Layer mask for interactable detection. Default = All layers.")]
    public LayerMask interactableMask = ~0;

    [Header("UI")]
    [Tooltip("HUD controller for prompt display (optional).")]
    public HUDController hud;

    [Tooltip("Default prompt text when near an interactable.")]
    public string defaultPrompt = "Press E to interact";

    private IInteractable currentTarget;
    private PlayerInput playerInput;
    private InputAction interactAction;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        if (playerInput != null)
            interactAction = playerInput.actions["Interact"];
    }

    void Update()
    {
        FindNearestInteractable();

        if (interactAction != null && interactAction.WasPressedThisFrame())
        {
            if (currentTarget != null)
            {
                currentTarget.Interact(gameObject);
            }
        }

        UpdatePrompt();
    }

    private void FindNearestInteractable()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, detectionRadius, interactableMask);

        IInteractable nearest = null;
        float nearestDist = float.MaxValue;

        foreach (Collider hit in hits)
        {
            IInteractable interactable = hit.GetComponentInParent<IInteractable>();
            if (interactable != null)
            {
                float dist = Vector3.Distance(transform.position, hit.transform.position);
                if (dist < nearestDist)
                {
                    nearestDist = dist;
                    nearest = interactable;
                }
            }
        }

        currentTarget = nearest;
    }

    private void UpdatePrompt()
    {
        if (hud == null)
            return;

        if (currentTarget != null)
        {
            string prompt = GetPromptText(currentTarget);
            hud.ShowPrompt(prompt);
        }
        else
        {
            hud.HidePrompt();
        }
    }

    private string GetPromptText(IInteractable target)
    {
        // New system: detachable UI controls
        if (target is UIControlSlot slot)
            return slot.PromptText;

        if (target is UIControlTarget uiTarget)
            return uiTarget.PromptText;

        // Old system: modules (backward compatibility)
        if (target is ModulePickup pickup)
            return string.IsNullOrEmpty(pickup.promptText) ? defaultPrompt : pickup.promptText;

        if (target is ModuleSocket socket)
        {
            if (socket.IsInstalled)
                return "Open interface";
            return "Install module";
        }

        if (target is SimpleInteractable simple)
            return string.IsNullOrEmpty(simple.promptText) ? defaultPrompt : simple.promptText;

        return defaultPrompt;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
