using UnityEngine;

/// <summary>
/// Trigger zone that ends the game when the player enters.
/// Place at the exit gate in Area 4.
/// </summary>
public class ExitTrigger : MonoBehaviour
{
    [Tooltip("GameManager reference.")]
    public GameManager gameManager;

    [Tooltip("Tag of the player object.")]
    public string playerTag = "Player";

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            if (gameManager != null)
                gameManager.TriggerEnding();
            else
                Debug.LogWarning("[ExitTrigger] No GameManager assigned!");
        }
    }
}
