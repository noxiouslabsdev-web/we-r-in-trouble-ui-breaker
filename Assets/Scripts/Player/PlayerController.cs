using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// การเคลื่อนที่แบบ third-person ด้วย Input System ใหม่
/// ทิศเดินอ้างอิงตามทิศที่กล้องหันอยู่ (ผ่าน CameraController) และตัวละครหมุนหันตามทิศเดิน
/// ทำงานได้ทั้งโหมดกล้องโคจรและกล้อง Fixed
/// ถ้ากดปุ่มเดินค้างอยู่ตอนกล้องเปลี่ยนโหมด จะใช้ทิศเดิมต่อจนกว่าจะปล่อยปุ่ม
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement")]
    [Tooltip("Movement speed in units per second.")]
    public float moveSpeed = 4f;

    [Tooltip("Rotation speed when turning to face movement direction.")]
    public float rotationSpeed = 10f;

    [Tooltip("Gravity applied when grounded.")]
    public float gravity = -9.81f;

    [Tooltip("Camera controller reference. If null, finds via Camera.main.")]
    public CameraController cameraController;

    private CharacterController controller;
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction interactAction;
    private Vector3 velocity;
    private bool isGrounded;

    // ทิศเดินล่าสุดที่คำนวณจากกล้อง (ใช้ตอนล็อกทิศ)
    private Vector3 lastForward = Vector3.forward;
    private Vector3 lastRight = Vector3.right;

    // true = ใช้ทิศที่เก็บไว้ ไม่อ่านทิศใหม่จากกล้อง
    private bool basisLatched = false;

    // จำนวนครั้งที่กล้องเปลี่ยนโหมดที่เห็นล่าสุด
    private int lastModeChangeCount = 0;

    [HideInInspector] public bool gameplayLocked;

    public InputAction InteractAction => interactAction;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        playerInput = GetComponent<PlayerInput>();

        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            interactAction = playerInput.actions["Interact"];
        }
    }

    void Start()
    {
        if (cameraController == null)
        {
            Camera cam = Camera.main;
            if (cam != null)
                cameraController = cam.GetComponent<CameraController>();
        }

        if (cameraController != null)
            lastModeChangeCount = cameraController.ModeChangeCount;
    }

    void Update()
    {
        if (gameplayLocked)
            return;
        ApplyGravity();
        Move();
    }

    private void ApplyGravity()
    {
        isGrounded = controller.isGrounded;
        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void Move()
    {
        if (moveAction == null)
            return;

        Vector2 input = moveAction.ReadValue<Vector2>();
        bool hasInput = input.sqrMagnitude >= 0.01f;

        // ตรวจว่ากล้องเปลี่ยนโหมดตั้งแต่เฟรมก่อนหรือไม่
        bool modeChanged = false;
        if (cameraController != null && cameraController.ModeChangeCount != lastModeChangeCount)
        {
            modeChanged = true;
            lastModeChangeCount = cameraController.ModeChangeCount;
        }

        if (!hasInput)
        {
            // ปล่อยปุ่มแล้ว เลิกล็อกทิศ และอัปเดตทิศล่าสุดให้ตรงกับกล้องปัจจุบัน
            basisLatched = false;
            GetCameraBasis(out lastForward, out lastRight);
            return;
        }

        // กดเดินค้างอยู่ตอนกล้องเปลี่ยนโหมด: ใช้ทิศเดิมต่อจนกว่าจะปล่อยปุ่ม
        // เพื่อไม่ให้ตัวละครเลี้ยวกะทันหันตอนข้ามขอบโซน
        if (modeChanged)
            basisLatched = true;

        Vector3 forward;
        Vector3 right;

        if (basisLatched)
        {
            forward = lastForward;
            right = lastRight;
        }
        else
        {
            GetCameraBasis(out forward, out right);
            lastForward = forward;
            lastRight = right;
        }

        Vector3 moveDir = forward * input.y + right * input.x;

        // ปรับให้เป็นทิศเดียวกัน กันเดินเฉียงแล้วเร็วเกินปกติ
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        // เคลื่อนที่ตัวละคร
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // หมุนตัวละครให้หันตามทิศเดิน
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }

    /// <summary>
    /// อ่านทิศหน้า-ขวาบนระนาบ XZ จากกล้อง
    /// ใช้ CameraController เป็นหลัก (รู้จักโหมด Fixed) และสำรองด้วย Camera.main
    /// </summary>
    private void GetCameraBasis(out Vector3 forward, out Vector3 right)
    {
        if (cameraController != null)
        {
            forward = cameraController.GetFlatForward();
            right = cameraController.GetFlatRight();
            return;
        }

        Camera cam = Camera.main;
        if (cam != null)
        {
            forward = cam.transform.forward;
            right = cam.transform.right;
            forward.y = 0f;
            right.y = 0f;
            forward.Normalize();
            right.Normalize();
            return;
        }

        forward = Vector3.forward;
        right = Vector3.right;
    }
}