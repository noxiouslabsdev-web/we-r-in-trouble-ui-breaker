using UnityEngine;

/// <summary>
/// Simple door that can be opened/closed or locked/unlocked.
/// Open moves the door object (or deactivates it) — configurable.
/// </summary>
public class DoorController : MonoBehaviour
{
    [Header("Door Settings")]
    [Tooltip("If true, door slides up when opened. If false, door is deactivated.")]
    public bool slideOpen = true;

    [Tooltip("Distance to slide when opening (if slideOpen is true).")]
    public float slideDistance = 3f;

    [Tooltip("Speed of door opening animation.")]
    public float openSpeed = 2f;

    [Tooltip("Is this door locked at start?")]
    public bool lockedAtStart = false;

    [Header("Visuals")]
    public GameObject lockedIndicator;
    public GameObject unlockedIndicator;

    private bool isOpen = false;
    private bool isLocked = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * slideDistance;
        isLocked = lockedAtStart;

        UpdateIndicators();
    }

    void Update()
    {
        if (isOpen && slideOpen)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, openPosition, openSpeed * Time.deltaTime);
        }
        else if (!isOpen && slideOpen)
        {
            transform.position = Vector3.MoveTowards(
                transform.position, closedPosition, openSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// Opens the door (also unlocks it if locked).
    /// </summary>
    public void Open()
    {
        isLocked = false;
        isOpen = true;

        if (!slideOpen)
            gameObject.SetActive(false);

        UpdateIndicators();
        Debug.Log($"[DoorController] Door {gameObject.name} opened.");
    }

    /// <summary>
    /// Closes the door.
    /// </summary>
    public void Close()
    {
        isOpen = false;
        if (!slideOpen)
            gameObject.SetActive(true);

        UpdateIndicators();
    }

    /// <summary>
    /// Locks or unlocks the door without opening/closing.
    /// </summary>
    public void SetLocked(bool locked)
    {
        isLocked = locked;
        UpdateIndicators();
    }

    public bool IsOpen => isOpen;
    public bool IsLocked => isLocked;

    private void UpdateIndicators()
    {
        if (lockedIndicator != null)
            lockedIndicator.SetActive(isLocked);
        if (unlockedIndicator != null)
            unlockedIndicator.SetActive(!isLocked);
    }
}
