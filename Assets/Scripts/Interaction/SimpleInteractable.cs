using UnityEngine;

/// <summary>
/// A basic interactable that logs interaction and can trigger an event.
/// Useful for doors, buttons, or placeholder test objects.
/// Attach to any GameObject and add a trigger collider.
/// </summary>
public class SimpleInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("Text shown in the interaction prompt.")]
    public string promptText = "Interact";

    [Tooltip("Optional: objects to toggle active state on interact.")]
    public GameObject[] toggleObjects;

    [Tooltip("Optional: events fired on interact (using simple bool flag for door-like behavior).")]
    public bool openOnInteract = false;

    public void Interact(GameObject player)
    {
        Debug.Log($"[SimpleInteractable] Interacted with {gameObject.name}");

        if (toggleObjects != null)
        {
            foreach (GameObject obj in toggleObjects)
            {
                if (obj != null)
                    obj.SetActive(!obj.activeSelf);
            }
        }
    }
}
