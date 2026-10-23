using UnityEngine;

/// <summary>
/// A placeable UI Module in the world that the player can pick up.
/// On interact, adds the module to the player's inventory and destroys this object.
/// </summary>
public class ModulePickup : MonoBehaviour, IInteractable
{
    [Header("Module")]
    [Tooltip("The module data this pickup grants.")]
    public ModuleData moduleData;

    [Tooltip("Text shown when player is in range.")]
    public string promptText = "Pick up module";

    [Header("Feedback")]
    [Tooltip("Optional: object to hide after pickup (the visual representation).")]
    public GameObject visualObject;

    [Tooltip("Optional: sound played on pickup.")]
    public AudioSource pickupSound;

    public void Interact(GameObject player)
    {
        if (moduleData == null)
        {
            Debug.LogWarning($"[ModulePickup] {gameObject.name} has no moduleData assigned!");
            return;
        }

        ModuleInventory inventory = player.GetComponent<ModuleInventory>();
        if (inventory == null)
        {
            Debug.LogWarning($"[ModulePickup] Player has no ModuleInventory component!");
            return;
        }

        inventory.AddModule(moduleData);

        if (pickupSound != null)
            pickupSound.Play();

        // Hide or destroy the pickup.
        if (visualObject != null)
            visualObject.SetActive(false);

        Destroy(gameObject, pickupSound != null ? pickupSound.clip.length : 0f);
    }
}
