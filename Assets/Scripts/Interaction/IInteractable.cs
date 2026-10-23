using UnityEngine;

/// <summary>
/// Common interface for anything the player can interact with.
/// Keep it minimal: one method to call on interaction.
/// </summary>
public interface IInteractable
{
    // Called when the player presses the interact key while looking at this object.
    void Interact(GameObject player);
}
