using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// กล้อง third-person แบบโคจรรอบผู้เล่น (ขยับเมาส์เพื่อหมุน yaw และ pitch)
/// เมื่อผู้เล่นเข้า CameraZone จะเปลี่ยนเป็นกล้อง Fixed และเบลนด์กลับอย่างนุ่มนวลเมื่อออกจากโซน
/// ล็อกเคอร์เซอร์ระหว่างสำรวจ และปล่อยเคอร์เซอร์เมื่อแผงปริศนาเปิดอยู่ (inputLocked = true)
/// ติดตั้งที่ Main Camera
/// </summary>
public class CameraController : MonoBehaviour
{
    [Header("Target")]
    [Tooltip("The player transform to follow.")]
    public Transform player;

    [Header("Orbit Settings")]
    [Tooltip("Distance from the player.")]
    public float distance = 6f;

    [Tooltip("Height offset above the player pivot.")]
    public float height = 1.5f;

    [Tooltip("Starting horizontal angle in degrees. 0 = camera behind the player looking toward +Z, 90 = looking toward +X.")]
    public float initialYaw = 0f;

    [Tooltip("Mouse sensitivity for horizontal orbit (yaw).")]
    public float yawSensitivity = 2f;

    [Tooltip("Mouse sensitivity for vertical orbit (pitch).")]
    public float pitchSensitivity = 2f;

    [Tooltip("Minimum pitch angle in degrees (prevents looking too far down).")]
    public float minPitch = -20f;

    [Tooltip("Maximum pitch angle in degrees (prevents flipping over).")]
    public float maxPitch = 60f;

    [Tooltip("How quickly the camera position follows the player.")]
    public float followSmoothTime = 0.1f;

    [Header("Fixed Camera Settings")]
    [Tooltip("How quickly the camera transitions to a fixed position.")]
    public float fixedTransitionSpeed = 3f;

    [Tooltip("Seconds to blend back to the orbit camera after leaving a fixed camera zone. 0 = instant.")]
    public float returnDuration = 0.8f;

    [Header("Input")]
    [Tooltip("When true, mouse look is disabled and the cursor is released (e.g. when a puzzle UI panel is open).")]
    public bool inputLocked = false;

    // ---------- สถานะวงโคจร ----------
    private float yaw = 0f;
    private float pitch = 20f;

    // ---------- สถานะกล้อง Fixed ----------
    private bool isThirdPerson = true;
    private Vector3 fixedPosition;
    private Quaternion fixedRotation;

    // ---------- สถานะการเบลนด์กลับจากกล้อง Fixed ----------
    private bool isReturningFromFixed = false;
    private float returnTimer = 0f;
    private Vector3 returnStartPosition;
    private Quaternion returnStartRotation;

    // ความเร็วสำหรับ SmoothDamp ตอนตามผู้เล่น
    private Vector3 followVelocity = Vector3.zero;

    // นับจำนวนครั้งที่เปลี่ยนโหมดกล้อง ให้ PlayerController ใช้ตรวจว่ากล้องเพิ่งเปลี่ยนโหมด
    private int modeChangeCount = 0;

    // ---------- อินพุต ----------
    private PlayerInput playerInput;
    private InputAction lookAction;

    // ---------- เคอร์เซอร์ ----------
    private bool wantCursorLocked = false;
    private bool cursorStateInitialized = false;

    /// <summary>
    /// true เมื่อใช้กล้องโคจร (third-person)
    /// </summary>
    public bool IsThirdPerson => isThirdPerson;

    /// <summary>
    /// จำนวนครั้งที่กล้องเปลี่ยนโหมด (เพิ่มขึ้นทุกครั้งที่สลับระหว่างกล้องโคจรและกล้อง Fixed)
    /// </summary>
    public int ModeChangeCount => modeChangeCount;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // หา PlayerInput เพื่ออ่าน action ชื่อ Look
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                lookAction = playerInput.actions["Look"];
        }

        // ตั้งมุมเริ่มต้นจากค่าใน Inspector
        yaw = initialYaw;

        // วางกล้องที่ตำแหน่งเริ่มต้นทันที เพื่อไม่ให้เห็นกล้องกระโดดตอนกด Play
        if (player != null)
        {
            Vector3 initialPos = CalculateOrbitPosition(player.position);
            transform.position = initialPos;
            transform.rotation = Quaternion.LookRotation(GetLookTarget() - initialPos);
        }
    }

    void LateUpdate()
    {
        UpdateCursorState();

        if (player == null)
            return;

        if (isThirdPerson)
        {
            UpdateOrbit();
            FollowPlayerOrbit();
        }
        else
        {
            FollowFixed();
        }
    }

    private void UpdateOrbit()
    {
        if (inputLocked)
            return;

        if (lookAction != null)
        {
            Vector2 lookInput = lookAction.ReadValue<Vector2>();
            yaw += lookInput.x * yawSensitivity;
            pitch -= lookInput.y * pitchSensitivity;
        }

        // จำกัดมุมก้มเงยไม่ให้กล้องพลิก
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void FollowPlayerOrbit()
    {
        Vector3 targetPos = CalculateOrbitPosition(player.position);

        // กำลังเบลนด์กลับจากกล้อง Fixed: ค่อยๆ เลื่อนทั้งตำแหน่งและการหมุนตามเวลาที่กำหนด
        if (isReturningFromFixed)
        {
            returnTimer += Time.deltaTime;
            float t = Mathf.Clamp01(returnTimer / returnDuration);
            float blend = Mathf.SmoothStep(0f, 1f, t);

            // ใช้ targetPos ที่อัปเดตทุกเฟรม ถึงผู้เล่นเดินอยู่ก็ไม่มีกระตุกตอนเบลนด์จบ
            Quaternion targetRot = Quaternion.LookRotation(GetLookTarget() - targetPos);
            transform.position = Vector3.Lerp(returnStartPosition, targetPos, blend);
            transform.rotation = Quaternion.Slerp(returnStartRotation, targetRot, blend);

            if (t >= 1f)
            {
                isReturningFromFixed = false;
                followVelocity = Vector3.zero;
            }
            return;
        }

        // โหมดปกติ: เลื่อนตำแหน่งนุ่มๆ แล้วหันมองผู้เล่น
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref followVelocity, followSmoothTime);
        transform.rotation = Quaternion.LookRotation(GetLookTarget() - transform.position);
    }

    private Vector3 GetLookTarget()
    {
        // จุดที่กล้องมองคือเหนือจุดเท้าผู้เล่นขึ้นไปครึ่งหนึ่งของ height
        return player.position + Vector3.up * height * 0.5f;
    }

    private Vector3 CalculateOrbitPosition(Vector3 playerPos)
    {
        // คำนวณตำแหน่งแบบพิกัดทรงกลมรอบผู้เล่น
        Quaternion rotation = Quaternion.Euler(pitch, yaw, 0f);
        Vector3 offset = rotation * new Vector3(0, 0, -distance);
        return playerPos + Vector3.up * height + offset;
    }

    private void FollowFixed()
    {
        float t = fixedTransitionSpeed * Time.deltaTime;
        transform.position = Vector3.Lerp(transform.position, fixedPosition, t);
        transform.rotation = Quaternion.Slerp(transform.rotation, fixedRotation, t);
    }

    /// <summary>
    /// ควบคุมการล็อกเคอร์เซอร์ตามสถานะ inputLocked
    /// </summary>
    private void UpdateCursorState()
    {
        // ล็อกเคอร์เซอร์เสมอ ยกเว้นตอนแผงปริศนาเปิดอยู่ (inputLocked = true)
        bool shouldLock = !inputLocked;

        // เปลี่ยนค่าเฉพาะตอนสถานะเปลี่ยน เพื่อให้กด Esc ปลดเคอร์เซอร์ใน Editor ได้
        if (!cursorStateInitialized || shouldLock != wantCursorLocked)
        {
            ApplyCursorState(shouldLock);
            wantCursorLocked = shouldLock;
            cursorStateInitialized = true;
        }
        // ถ้าควรล็อกแต่เคอร์เซอร์หลุด (เช่นกด Esc) ให้คลิกซ้ายเพื่อล็อกกลับ
        else if (shouldLock
                 && Cursor.lockState != CursorLockMode.Locked
                 && Mouse.current != null
                 && Mouse.current.leftButton.wasPressedThisFrame)
        {
            ApplyCursorState(true);
        }
    }

    private void ApplyCursorState(bool locked)
    {
        Cursor.lockState = locked ? CursorLockMode.Locked : CursorLockMode.None;
        Cursor.visible = !locked;
    }

    void OnDisable()
    {
        // คืนเคอร์เซอร์เมื่อคอมโพเนนต์ถูกปิดหรือหยุด Play
        ApplyCursorState(false);
    }

    /// <summary>
    /// สลับไปใช้กล้อง Fixed ที่ตำแหน่งและมุมที่กำหนด
    /// ถูกเรียกโดย CameraZone เมื่อผู้เล่นเข้าพื้นที่ปริศนา
    /// </summary>
    public void SwitchToFixed(Vector3 position, Quaternion rotation)
    {
        fixedPosition = position;
        fixedRotation = rotation;
        isThirdPerson = false;
        isReturningFromFixed = false;
        modeChangeCount++;
    }

    /// <summary>
    /// กลับไปใช้กล้องโคจร ถูกเรียกโดย CameraZone เมื่อผู้เล่นออกจากพื้นที่ปริศนา
    /// จำตำแหน่งและมุมของกล้อง Fixed ไว้ แล้วเบลนด์กลับตาม returnDuration
    /// </summary>
    public void SwitchToThirdPerson()
    {
        // อยู่โหมดกล้องโคจรอยู่แล้ว ไม่ต้องทำอะไร
        if (isThirdPerson)
            return;

        returnStartPosition = transform.position;
        returnStartRotation = transform.rotation;
        returnTimer = 0f;
        isReturningFromFixed = returnDuration > 0f;

        isThirdPerson = true;
        followVelocity = Vector3.zero;
        modeChangeCount++;
    }

    /// <summary>
    /// มุม yaw ที่ใช้คำนวณทิศเดิน
    /// โหมดกล้องโคจรใช้ yaw ของวงโคจร โหมด Fixed ใช้ทิศที่กล้อง Fixed หันอยู่
    /// </summary>
    public float GetYaw()
    {
        return isThirdPerson ? yaw : fixedRotation.eulerAngles.y;
    }

    /// <summary>
    /// ทิศเดินไปข้างหน้าบนระนาบ XZ ที่สัมพันธ์กับกล้อง (ใช้กับ PlayerController)
    /// </summary>
    public Vector3 GetFlatForward()
    {
        return Quaternion.Euler(0f, GetYaw(), 0f) * Vector3.forward;
    }

    /// <summary>
    /// ทิศเดินไปทางขวาบนระนาบ XZ ที่สัมพันธ์กับกล้อง
    /// </summary>
    public Vector3 GetFlatRight()
    {
        return Quaternion.Euler(0f, GetYaw(), 0f) * Vector3.right;
    }
}