# Code Map

Evidence labels: `[READ]` means directly present in source, scene YAML, or project settings. `[NOT VERIFIED]` means runtime behavior requiring Play Mode or Unity Editor validation. Claims below are labeled.

## 1. Script Inventory

`[READ]` All 25 C# files under `Assets/` are listed. No `.asmdef` files or prefabs were found under `Assets/`.

| path | class name | base class/interfaces | one-line purpose | LEGACY? | referenced by |
|---|---|---|---|---|---|
| `[READ]` Assets/Editor/BuildGame.cs | BuildGame | static | Editor batch Windows build entry point. | No | Unity CLI entry point; no script caller seen. |
| `[READ]` Assets/Editor/SceneBuilder.cs | SceneBuilder | static | Editor menu generates Station scene/assets/build list. | No | Unity Editor menu; creates scene components. |
| `[READ]` Assets/Scripts/Camera/CameraController.cs | CameraController | MonoBehaviour | Orbit/follow and fixed camera mode. | No | CameraZone, PlayerController, PuzzleController; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/Camera/CameraZone.cs | CameraZone | MonoBehaviour | Trigger switches camera modes. | No | SceneBuilder; Station.unity. |
| `[READ]` Assets/Scripts/Core/GameManager.cs | GameManager | MonoBehaviour | Displays ending panel and sets final quest objective. | No | ExitTrigger; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/Core/QuestManager.cs | QuestManager | MonoBehaviour | Stores objective progression and updates HUD. | No | PuzzleController, GameManager; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/Interaction/IInteractable.cs | IInteractable | interface | Interaction contract taking player GameObject. | No | PlayerInteractor; implemented by SimpleInteractable, ModulePickup, ModuleSocket, UIControlSlot, UIControlTarget. |
| `[READ]` Assets/Scripts/Interaction/SimpleInteractable.cs | SimpleInteractable | MonoBehaviour, IInteractable | Generic interaction log and object toggles. | No | PlayerInteractor; no Station instance seen. |
| `[READ]` Assets/Scripts/Legacy/ModuleData.cs | ModuleData | ScriptableObject | Legacy module data asset. | Yes, superseded by UIControlData | ModuleInventory, ModulePickup, ModuleSocket; no Station instance seen. |
| `[READ]` Assets/Scripts/Legacy/ModuleInventory.cs | ModuleInventory | MonoBehaviour | Legacy list inventory. | Yes, replaced by CarriedControl | ModulePickup, ModuleSocket; no Station instance seen. |
| `[READ]` Assets/Scripts/Legacy/ModulePickup.cs | ModulePickup | MonoBehaviour, IInteractable | Legacy world pickup. | Yes, superseded by UIControlSlot | PlayerInteractor, ModuleInventory; no Station instance seen. |
| `[READ]` Assets/Scripts/Legacy/ModuleSocket.cs | ModuleSocket | MonoBehaviour, IInteractable | Legacy module socket opens puzzle. | Yes, superseded by UIControlTarget | PlayerInteractor, ModuleInventory, PuzzleController; no Station instance seen. |
| `[READ]` Assets/Scripts/Player/CarriedControl.cs | CarriedControl | MonoBehaviour | Holds one detachable control. | No | UIControlSlot, UIControlTarget; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/Player/PlayerController.cs | PlayerController | MonoBehaviour | CharacterController movement/gravity from Move action. | No | CameraController; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/Player/PlayerInteractor.cs | PlayerInteractor | MonoBehaviour | Finds nearby interactable, dispatches input, updates prompt. | No | IInteractable implementations, HUDController, legacy classes; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/Puzzles/PuzzleController.cs | PuzzleController | MonoBehaviour | Watches slider, opens doors, advances quest, gates look input. | No | UIControlTarget, ModuleSocket, DoorController, QuestManager, CameraController, ClosePanelButton; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/UI/ClosePanelButton.cs | ClosePanelButton | MonoBehaviour | Button click forwards to puzzle close. | No | PuzzleController; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/UI/HUDController.cs | HUDController | MonoBehaviour | Writes objective, prompt, and timed notification text. | No | PlayerInteractor, QuestManager; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/UIControls/UIControlData.cs | UIControlData | ScriptableObject | New detachable-control data. | No | CarriedControl, UIControlSlot; SceneBuilder. |
| `[READ]` Assets/Scripts/UIControls/UIControlSlot.cs | UIControlSlot | MonoBehaviour, IInteractable | Detaches source visual to carrier. | No | PlayerInteractor, CarriedControl; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/UIControls/UIControlTarget.cs | UIControlTarget | MonoBehaviour, IInteractable | Installs matching control and opens puzzle. | No | PlayerInteractor, CarriedControl, PuzzleController; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/World/DoorController.cs | DoorController | MonoBehaviour | Moves/deactivates door and manages lock state. | No | PuzzleController; Station.unity; SceneBuilder. |
| `[READ]` Assets/Scripts/World/ExitTrigger.cs | ExitTrigger | MonoBehaviour | Ends game on player trigger entry. | No | GameManager; Station.unity; SceneBuilder. |
| `[READ]` Assets/TutorialInfo/Readme.cs | Readme; nested Section | ScriptableObject; nested Serializable class | Tutorial Readme asset data. | No | ReadmeEditor; no Station instance seen. |
| `[READ]` Assets/TutorialInfo/Editor/ReadmeEditor.cs | ReadmeEditor | Editor | UI Toolkit custom inspector for Readme. | No | Unity custom editor for Readme. |

`[READ]` No prefab files exist under Assets. “No Station instance seen” means not present in Station YAML or a prefab found there; other data sources were not checked.

## 2. Per-Script Explanation (Runtime Scripts)

### CameraController
- `[READ]` Responsibility/lifecycle: Start discovers target and Look action, initializes yaw/pose; LateUpdate updates orbit-follow or fixed pose.
- `[READ]` Public fields: `player: Transform` target; `distance, height, yawSensitivity, pitchSensitivity, minPitch, maxPitch, followSmoothTime, fixedTransitionSpeed: float` tuning; `inputLocked: bool` gates orbit input.
- `[READ]` Public methods: SwitchToFixed/SwitchToThirdPerson called by CameraZone; GetYaw/GetFlatForward/GetFlatRight called by PlayerController. No OnLook message; directly reads Look. Uses FindGameObjectWithTag and player.GetComponent<PlayerInput>().

### CameraZone
- `[READ]` Public fields `cameraAnchor: Transform`, `playerTag: string`; Start finds MainCamera/controller; OnTriggerEnter/Exit call controller switching methods. No public methods; requires Collider. Uses FindGameObjectWithTag/GetComponent.

### GameManager
- `[READ]` Fields `gameEnded: bool`, `endingPanel: GameObject`, `questManager: QuestManager`. No lifecycle. TriggerEnding called by ExitTrigger; sets ending UI and final quest objective. No discovery.

### QuestManager
- `[READ]` Fields `objectives: string[]`, `currentObjective: int`, `hud: HUDController`. Start updates HUD; AdvanceObjective called by PuzzleController; SetObjective called by GameManager; both update through HUDController.SetObjective. No discovery.

### IInteractable
- `[READ]` Interface declares `Interact(GameObject player)`; no fields/lifecycle. PlayerInteractor calls it; implemented by SimpleInteractable, ModulePickup, ModuleSocket, UIControlSlot, UIControlTarget.

### SimpleInteractable
- `[READ]` Fields `promptText: string`, `toggleObjects: GameObject[]`, `openOnInteract: bool`. No lifecycle. Interact called via IInteractable, logs and toggles assigned objects. `openOnInteract` is unused. No discovery/events.

### ModuleData (legacy)
- `[READ]` ScriptableObject fields `displayName, moduleID, description: string`, `icon: Sprite`; no lifecycle/methods. Used by ModuleInventory, ModulePickup, ModuleSocket.

### ModuleInventory (legacy)
- `[READ]` Field `modules: List<ModuleData>`. AddModule/HasModule/RemoveModule called by ModulePickup/ModuleSocket. No lifecycle/events; depends on ModuleData.

### ModulePickup (legacy)
- `[READ]` Fields `moduleData: ModuleData`, `promptText: string`, `visualObject: GameObject`, `pickupSound: AudioSource`. Interact called via IInteractable; gets ModuleInventory from player, grants data, optional sound/hide, destroys pickup. PlayerInteractor reads prompt. No lifecycle/events.

### ModuleSocket (legacy)
- `[READ]` Fields `acceptedModuleID`, `promptTextInstall`, `promptTextInstalled`, `promptTextNoModule: string`; `puzzlePanel`, `installedIndicator: GameObject`; `puzzleController: PuzzleController`; property `IsInstalled`. Interact finds ModuleInventory, validates/removes module, opens panel and calls OnModuleInstalled; if already installed, calls OnPanelOpened. PlayerInteractor reads IsInstalled and hardcodes prompt, not the prompt fields.

### CarriedControl
- `[READ]` Fields `carriedControl: UIControlData`, `carryVisual: GameObject`; property IsCarrying. PickUpControl/InstallControl/HasControl/GetCarriedControlID called by UIControlSlot/UIControlTarget; toggles visual and logs. No lifecycle/discovery/events.

### PlayerController
- `[READ]` Fields `moveSpeed, rotationSpeed, gravity: float`, `cameraController: CameraController`; property `InteractAction: InputAction`. Awake gets CharacterController/PlayerInput and caches Move/Interact; Start finds camera if missing; Update applies gravity and Move. Reads Move directly, does not receive OnMove. InteractAction has no script caller found. Uses GetComponent, Camera.main fallback.

### PlayerInteractor
- `[READ]` Fields `detectionRadius: float`, `interactableMask: LayerMask`, `hud: HUDController`, `defaultPrompt: string`. Awake gets PlayerInput/Interact; Update uses OverlapSphere, picks nearest IInteractable, calls Interact on WasPressedThisFrame, updates HUD. OnDrawGizmosSelected draws radius. Direct action read, not OnInteract. Uses GetComponentInParent<IInteractable>, serialized HUD; prompt branches include new, legacy, and SimpleInteractable types.

### PuzzleController
- `[READ]` Fields `puzzleSlider: Slider`; `targetMin/targetMax: float`; `successLight/failureLight/puzzlePanel: GameObject`; `successSound: AudioSource`; `doorsToOpen: DoorController[]`; `questManager: QuestManager`; `cameraController: CameraController`; `isSolved/moduleInstalled: bool`. Update watches panel edges and slider target; Solve updates feedback, calls DoorController.Open and QuestManager.AdvanceObjective. OnModuleInstalled/OnPanelOpened called by UIControlTarget and ModuleSocket; ClosePanel called by ClosePanelButton. Camera fallback uses Camera.main/GetComponent.

### ClosePanelButton
- `[READ]` Fields `puzzleController: PuzzleController`; Awake gets Button; Start subscribes OnClick; click calls ClosePanel. Requires Button. No discovery beyond GetComponent.

### HUDController
- `[READ]` Fields `objectiveText/promptText/notificationText: Text`, `promptContainer: GameObject`, `notificationDuration: float`. Update decrements timer; SetObjective/ShowPrompt/HidePrompt called by QuestManager/PlayerInteractor; ShowNotification has no caller found; updates optional serialized refs. No discovery/events.

### UIControlData
- `[READ]` Fields `displayName/controlID/description: string`, `icon: Sprite`, `controlColor: Color`. Data-only ScriptableObject, used by CarriedControl/UIControlSlot.

### UIControlSlot
- `[READ]` Fields `controlData: UIControlData`, `controlPresentVisual/controlAbsentVisual: GameObject`, `hasControl: bool`; PromptText property. Interact called by PlayerInteractor; gets CarriedControl, transfers data, changes visuals/state. No lifecycle/events.

### UIControlTarget
- `[READ]` Fields `acceptedControlID: string`, `installedVisual/emptyVisual/puzzlePanel: GameObject`, `puzzleController: PuzzleController`, `isInstalled: bool`; IsInstalled/PromptText properties. Interact called by PlayerInteractor; gets carrier, checks compatibility, consumes control, changes visuals, notifies puzzle, opens panel. No lifecycle/events.

### DoorController
- `[READ]` Fields `slideOpen: bool`, `slideDistance/openSpeed: float`, `lockedAtStart: bool`, `lockedIndicator/unlockedIndicator: GameObject`; IsOpen/IsLocked properties. Start caches positions/state; Update moves door; Open/Close/SetLocked public; PuzzleController calls Open. No discovery/events.

### ExitTrigger
- `[READ]` Fields `gameManager: GameManager`, `playerTag: string`. OnTriggerEnter matches tag and calls TriggerEnding. No other lifecycle/public methods; serialized manager only.

### Readme
- `[READ]` Fields `commonStyle/darkStyle/lightStyle: StyleSheet`, `icon: Texture2D`, `title: string`, `sections: Section[]`, `loadedLayout: bool`; nested Section has `heading/text/linkText/url: string`. Data-only ScriptableObject consumed by ReadmeEditor.

## 3. Input Flow

- `[READ]` SceneBuilder assigns the actions asset, default map `Player`, notification `SendMessages`; Station YAML serializes the same action asset/map and notification enum value 0.
- `[READ]` No OnMove/OnLook/OnInteract receiver exists. PlayerController reads Move and Interact; PlayerInteractor also reads Interact; CameraController gets PlayerInput from its player target and reads Look. Thus Look reaches the MainCamera component via the action object, not messages.
- `[READ]` Station YAML places PlayerInput on Player and CameraController on MainCamera. Player-map Interact has `Press`; PlayerInteractor uses WasPressedThisFrame.
- `[READ]` No `UnityEngine.Input` reads or Cursor lock/visibility calls found under Assets C# files. Scene UI input module has `m_CursorLockBehavior: 0`, which does not establish current cursor state. `[NOT VERIFIED]` Actual Play Mode cursor lock/visibility.

## 4. Camera System

- `[READ]` CameraController.Start finds tagged Player only if its serialized player is null, gets its PlayerInput/Look action, derives initial yaw from XZ offset, then snaps to orbit position.
- `[READ]` LateUpdate returns without player; third person runs orbit input and follow, fixed mode follows stored pose. Orbit updates yaw += Look X*sensitivity and pitch -= Look Y*sensitivity unless inputLocked, clamps pitch, SmoothDamps position and looks at player. Orbit position uses yaw/pitch quaternion, distance and height.
- `[READ]` Fixed switching stores position/rotation and sets mode. Fixed follow Lerp/Slerp uses fixedTransitionSpeed*deltaTime. Return sets orbit mode and clears follow velocity; yaw/pitch persist. Orbit position eases via SmoothDamp, but orbit rotation is assigned LookRotation. `[NOT VERIFIED]` Visual blend quality.
- `[READ]` CameraZone finds MainCamera once at Start; enter uses anchor (or own transform fallback) and calls SwitchToFixed; exit calls SwitchToThirdPerson. There is no active-zone stack.
- `[READ]` PlayerController movement basis uses controller yaw getters even in fixed mode (fallback is flattened Camera.main basis). It samples Move continuously each Update, normalizes diagonals, and rotates player toward movement; no held-key buffer/reset on camera transition. Fixed-camera movement therefore uses last orbit yaw, not fixed pose.
- `[READ]` Look isn't read in fixed mode because UpdateOrbit isn't called. In orbit mode PuzzleController.inputLocked suppresses Look while panel is open; it does not suppress movement. `[NOT VERIFIED]` Trigger and UI timing/input feel.

## 5. Game Flow Wiring

- `[READ]` PlayerInteractor chooses nearest IInteractable, then on Interact press invokes `Interact(player)`.
- `[READ]` UIControlSlot -> CarriedControl.PickUpControl; slot changes visuals/state. UIControlTarget validates carrier ID -> InstallControl -> changes visuals -> PuzzleController.OnModuleInstalled -> opens panel. Already-installed target calls OnPanelOpened.
- `[READ]` PuzzleController.Update solves on slider entering target range after install; Solve enables/disables lights, plays optional sound, calls each DoorController.Open, then QuestManager.AdvanceObjective. ClosePanelButton -> PuzzleController.ClosePanel.
- `[READ]` ExitTrigger -> GameManager.TriggerEnding -> ending panel on + QuestManager.SetObjective(last index).
- `[READ]` PlayerInteractor still references ModulePickup/ModuleSocket for prompt display. ModulePickup/ModuleSocket reference ModuleInventory; ModuleInventory stores ModuleData; ModuleSocket also references PuzzleController. Removing legacy classes without removing these code references breaks compilation. No legacy instance was found in Station YAML. `[NOT VERIFIED]` Other external scenes/assets were not searched.

## 6. Scene Wiring Summary: Assets/Scenes/Station.unity

- `[READ]` MainCamera: Camera + CameraController; player reference points to Player; distance 6, height 1.5, follow smoothing .1, sensitivity 2, pitch -20..60, fixed speed 3.
- `[READ]` Player: CharacterController, CarriedControl, PlayerInteractor, PlayerController, PlayerInput; tag Player; controller speed 4, rotation 10, gravity -9.81, camera ref to MainCamera; interactor radius 2.5/HUD ref; PlayerInput uses Player map.
- `[READ]` Three CameraZone objects each have trigger collider/component and serialized child anchor: TicketMachine, ControlPanel, Train.
- `[READ]` SourcePanel, VolumeSourcePanel, SettingsSourcePanel have UIControlSlot with data and present/absent visual refs.
- `[READ]` TicketMachine, ControlPanel, TrainFront each have PuzzleController + UIControlTarget; accepted IDs plus/volume/settings; same-object puzzle ref, panel, slider, lights, quest, camera refs serialized.
- `[READ]` Door arrays point respectively to Door_2_to_3, Door_3_to_4, TrainDoor. HUD_Manager refs objective/prompt/notification Text and prompt container; QuestManager references HUD; GameManager references QuestManager/EndingPanel; ExitTrigger references GameManager.
- `[READ]` EventSystem has EventSystem/InputSystemUIInputModule. Canvas has Canvas/CanvasScaler/GraphicRaycaster. Three panel close helpers reference their matching PuzzleController. `[NOT VERIFIED]` Runtime object behavior.

## 7. Compile Status

- `[NOT VERIFIED]` Unity 6000.3.27f1 batch invocation exited code 1 immediately after accepting project path and produced no compiler diagnostics; therefore no successful compile is claimed. The Unity.exe resolved through PATH was a CLI shim and rejected `-batchmode`.

## 8. Known Problems / Risks

- `[READ]` Nearest-overlap interaction has no line-of-sight/raycast check (`Assets/Scripts/Player/PlayerInteractor.cs:38-72`). `[NOT VERIFIED]` Desired behavior.
- `[READ]` Exiting any camera zone returns to third-person; overlapping zones have no tracking (`Assets/Scripts/Camera/CameraZone.cs:28-45`). `[NOT VERIFIED]` Runtime overlap geometry.
- `[READ]` Fixed camera movement basis remains orbit yaw (`Assets/Scripts/Player/PlayerController.cs:86-97`; `Assets/Scripts/Camera/CameraController.cs:184-200`).
- `[READ]` `SimpleInteractable.openOnInteract` is unused (`Assets/Scripts/Interaction/SimpleInteractable.cs:15-31`).
- `[READ]` Any solved puzzle advances quest without checking objective identity; ending separately sets last objective (`Assets/Scripts/Core/QuestManager.cs:34-54`; `Assets/Scripts/Core/GameManager.cs:31-34`).
- `[READ]` SceneBuilder creates ScriptableObjects at fixed paths without checking for existing assets (`Assets/Editor/SceneBuilder.cs:541-568`). `[NOT VERIFIED]` Editor behavior on rerun.
- `[NOT VERIFIED]` Compile status remains unknown; see section 7.

## Appendix: Verbatim Source
### Assets/Scripts/Camera/CameraController.cs

````csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person orbit camera that follows the player.
/// Mouse movement orbits the camera around the player (yaw + pitch).
/// Transitions to a fixed camera when entering a CameraZone trigger.
/// Attach to the Main Camera.
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
    [Tooltip("How quickly the camera transitions to/from a fixed position.")]
    public float fixedTransitionSpeed = 3f;

    [Header("Input")]
    [Tooltip("When true, mouse look is disabled (e.g. when a puzzle UI panel is open).")]
    public bool inputLocked = false;

    // Orbit state
    private float yaw = 0f;
    private float pitch = 20f;

    // Fixed camera state
    private bool isThirdPerson = true;
    private Vector3 fixedPosition;
    private Quaternion fixedRotation;

    // Smooth follow velocity
    private Vector3 followVelocity = Vector3.zero;

    // Input
    private PlayerInput playerInput;
    private InputAction lookAction;

    void Start()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                player = playerObj.transform;
        }

        // Find PlayerInput to read the Look action
        if (player != null)
        {
            playerInput = player.GetComponent<PlayerInput>();
            if (playerInput != null)
                lookAction = playerInput.actions["Look"];
        }

        // Set initial camera angle
        if (player != null)
        {
            Vector3 dir = transform.position - player.position;
            dir.y = 0f;
            if (dir.sqrMagnitude > 0.01f)
                yaw = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        }

        // Snap to initial position to avoid jump on Play
        if (player != null)
        {
            Vector3 initialPos = CalculateOrbitPosition(player.position);
            transform.position = initialPos;
            transform.rotation = Quaternion.LookRotation(player.position - initialPos);
        }
    }

    void LateUpdate()
    {
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

        // Clamp pitch
        pitch = Mathf.Clamp(pitch, minPitch, maxPitch);
    }

    private void FollowPlayerOrbit()
    {
        Vector3 targetPos = CalculateOrbitPosition(player.position);
        transform.position = Vector3.SmoothDamp(transform.position, targetPos, ref followVelocity, followSmoothTime);

        // Look at the player
        Vector3 lookTarget = player.position + Vector3.up * height * 0.5f;
        transform.rotation = Quaternion.LookRotation(lookTarget - transform.position);
    }

    private Vector3 CalculateOrbitPosition(Vector3 playerPos)
    {
        // Spherical coordinates around the player
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
    /// Switch to a fixed camera at the given position and rotation.
    /// Called by CameraZone when the player enters a puzzle area.
    /// </summary>
    public void SwitchToFixed(Vector3 position, Quaternion rotation)
    {
        fixedPosition = position;
        fixedRotation = rotation;
        isThirdPerson = false;
    }

    /// <summary>
    /// Return to third-person orbit camera.
    /// Called by CameraZone when the player exits a puzzle area.
    /// The orbit angle is restored from the last yaw/pitch values — no sudden jump.
    /// </summary>
    public void SwitchToThirdPerson()
    {
        isThirdPerson = true;
        // Reset follow velocity to avoid smoothing lag on return
        followVelocity = Vector3.zero;
    }

    /// <summary>
    /// Get the current yaw (horizontal camera angle). Used by PlayerController
    /// to determine forward direction.
    /// </summary>
    public float GetYaw()
    {
        return yaw;
    }

    /// <summary>
    /// Get the camera's horizontal forward direction (flattened to XZ plane).
    /// Used by PlayerController for movement.
    /// </summary>
    public Vector3 GetFlatForward()
    {
        return Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
    }

    /// <summary>
    /// Get the camera's horizontal right direction (flattened to XZ plane).
    /// </summary>
    public Vector3 GetFlatRight()
    {
        return Quaternion.Euler(0f, yaw, 0f) * Vector3.right;
    }
}

````

### Assets/Scripts/Camera/CameraZone.cs

````csharp
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

````

### Assets/Scripts/Player/PlayerController.cs

````csharp
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Third-person player movement using the New Input System.
/// Movement direction is based on the camera's horizontal facing (yaw).
/// The player rotates to face the movement direction.
/// Works in both third-person orbit and fixed-camera modes.
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
    }

    void Update()
    {
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
        if (input.sqrMagnitude < 0.01f)
            return;

        // Determine movement directions based on camera yaw
        Vector3 forward;
        Vector3 right;

        if (cameraController != null)
        {
            // Use the camera controller's yaw-based flat directions
            forward = cameraController.GetFlatForward();
            right = cameraController.GetFlatRight();
        }
        else
        {
            // Fallback: use Camera.main transform
            Camera cam = Camera.main;
            if (cam != null)
            {
                forward = cam.transform.forward;
                right = cam.transform.right;
                forward.y = 0f;
                right.y = 0f;
                forward.Normalize();
                right.Normalize();
            }
            else
            {
                forward = Vector3.forward;
                right = Vector3.right;
            }
        }

        Vector3 moveDir = forward * input.y + right * input.x;

        // Normalize to prevent diagonal speed boost
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        // Move the player
        controller.Move(moveDir * moveSpeed * Time.deltaTime);

        // Rotate the player to face movement direction
        if (moveDir.sqrMagnitude > 0.01f)
        {
            Quaternion targetRot = Quaternion.LookRotation(moveDir);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, rotationSpeed * Time.deltaTime);
        }
    }
}

````

### Assets/Scripts/Player/PlayerInteractor.cs

````csharp
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

````

### Assets/Scripts/Player/CarriedControl.cs

````csharp
using UnityEngine;

/// <summary>
/// Tracks the single UI control the player is currently carrying.
/// The player can only carry one control at a time.
/// When a control is detached from a source panel, it is stored here.
/// When a control is installed into a target, it is removed from here.
/// Replaces the old ModuleInventory.
/// </summary>
public class CarriedControl : MonoBehaviour
{
    [Tooltip("The control data currently being carried. Null if carrying nothing.")]
    public UIControlData carriedControl;

    [Tooltip("Visual object shown above/around the player when carrying a control (optional).")]
    public GameObject carryVisual;

    /// <summary>
    /// True if the player is currently carrying a control.
    /// </summary>
    public bool IsCarrying => carriedControl != null;

    /// <summary>
    /// Pick up (detach) a control from a source panel.
    /// </summary>
    public void PickUpControl(UIControlData control)
    {
        if (carriedControl != null)
        {
            Debug.Log("[CarriedControl] Already carrying a control — cannot pick up another.");
            return;
        }

        carriedControl = control;
        if (carryVisual != null)
            carryVisual.SetActive(true);

        Debug.Log($"[CarriedControl] Now carrying: {control.displayName}");
    }

    /// <summary>
    /// Install the carried control into a target. Returns true if successful.
    /// </summary>
    public bool InstallControl()
    {
        if (carriedControl == null)
        {
            Debug.Log("[CarriedControl] Nothing to install.");
            return false;
        }

        UIControlData installed = carriedControl;
        carriedControl = null;

        if (carryVisual != null)
            carryVisual.SetActive(false);

        Debug.Log($"[CarriedControl] Installed: {installed.displayName}");
        return true;
    }

    /// <summary>
    /// Check if the player is carrying a control with the given ID.
    /// </summary>
    public bool HasControl(string controlID)
    {
        return carriedControl != null && carriedControl.controlID == controlID;
    }

    /// <summary>
    /// Get the ID of the currently carried control, or null if carrying nothing.
    /// </summary>
    public string GetCarriedControlID()
    {
        return carriedControl != null ? carriedControl.controlID : null;
    }
}

````