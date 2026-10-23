using UnityEngine;

/// <summary>
/// Trigger volume that switches the camera to a fixed position when the player enters,
/// and returns to third-person when they exit.
/// Place at puzzle areas where a fixed camera is desired.
/// The camera anchor (a child empty GameObject) defines the fixed camera position/rotation.
/// </summary>
[RequireComponent(typeof(Collider))]
public class CameraZone : MonoBehaviour
{
    [Header("Camera Anchor")]
    [Tooltip("Transform defining the fixed camera position and rotation for this zone. If null, uses this object's transform.")]
    public Transform cameraAnchor;

    [Tooltip("Tag of the player object.")]
    public string playerTag = "Player";

    private CameraController cameraController;

    void Start()
    {
        GameObject camObj = GameObject.FindGameObjectWithTag("MainCamera");
        if (camObj != null)
            cameraController = camObj.GetComponent<CameraController>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag) && cameraController != null)
        {
            Transform anchor = cameraAnchor != null ? cameraAnchor : transform;
            cameraController.SwitchToFixed(anchor.position, anchor.rotation);
            Debug.Log($"[CameraZone] Entered {gameObject.name}, switching to fixed camera.");
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag) && cameraController != null)
        {
            cameraController.SwitchToThirdPerson();
            Debug.Log($"[CameraZone] Exited {gameObject.name}, returning to third-person camera.");
        }
    }
}
