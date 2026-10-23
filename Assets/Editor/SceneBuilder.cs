using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;

/// <summary>
/// Editor tool that builds the complete playable scene for WE R IN TROUBLE: UI BREAKER.
/// Implements the CORRECTED design:
/// - Third-person camera during exploration, fixed camera at puzzle areas via CameraZone triggers.
/// - Detachable UI controls: players remove buttons from source panels, carry them, install into targets.
/// Menu: Tools > TGC > Build Station Scene
/// </summary>
public static class SceneBuilder
{
    private const string SCENE_PATH = "Assets/Scenes/Station.unity";

    [MenuItem("Tools/TGC/Build Station Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // --- 1. Floor ---
        var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name = "Floor";
        floor.transform.localScale = new Vector3(40, 1, 15);
        floor.transform.position = new Vector3(0, -0.5f, 0);
        SetColor(floor, new Color(0.1f, 0.1f, 0.12f));

        // --- 2. Walls ---
        CreateWall("NorthWall", new Vector3(0, 2, 7.5f), new Vector3(40, 4, 0.5f));
        CreateWall("SouthWall", new Vector3(0, 2, -7.5f), new Vector3(40, 4, 0.5f));
        CreateWall("Divider_1_2_N", new Vector3(-10, 2, 5), new Vector3(0.5f, 4, 5));
        CreateWall("Divider_1_2_S", new Vector3(-10, 2, -5), new Vector3(0.5f, 4, 5));
        CreateWall("Divider_2_3_N", new Vector3(0, 2, 5), new Vector3(0.5f, 4, 5));
        CreateWall("Divider_2_3_S", new Vector3(0, 2, -5), new Vector3(0.5f, 4, 5));
        CreateWall("Divider_3_4_N", new Vector3(10, 2, 5), new Vector3(0.5f, 4, 5));
        CreateWall("Divider_3_4_S", new Vector3(10, 2, -5), new Vector3(0.5f, 4, 5));

        // --- 3. Directional Light ---
        var lightObj = new GameObject("DirectionalLight");
        lightObj.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1f;
        light.shadows = LightShadows.Soft;

        // --- 4. Camera (third-person follow) ---
        var camObj = new GameObject("MainCamera");
        camObj.tag = "MainCamera";
        camObj.transform.position = new Vector3(-18, 4, -6);
        camObj.transform.rotation = Quaternion.Euler(20f, 0, 0);
        var cam = camObj.AddComponent<Camera>();
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = new Color(0.05f, 0.05f, 0.08f);
        cam.fieldOfView = 60f;
        // CameraController handles third-person follow and fixed-camera transitions
        var camCtrl = camObj.AddComponent<CameraController>();
        camCtrl.distance = 6f;
        camCtrl.height = 1.5f;
        camCtrl.followSmoothTime = 0.1f;
        camCtrl.yawSensitivity = 2f;
        camCtrl.pitchSensitivity = 2f;
        camCtrl.minPitch = -20f;
        camCtrl.maxPitch = 60f;

        // --- 5. Player ---
        var player = new GameObject("Player");
        player.tag = "Player";
        player.transform.position = new Vector3(-18, 1, 0);
        var cc = player.AddComponent<CharacterController>();
        cc.center = new Vector3(0, 1, 0);
        cc.height = 2f;
        cc.radius = 0.4f;

        var inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/InputSystem_Actions.inputactions");
        var playerInput = player.AddComponent<PlayerInput>();
        if (inputActions != null)
        {
            playerInput.actions = inputActions;
            playerInput.defaultActionMap = "Player";
            playerInput.notificationBehavior = PlayerNotifications.SendMessages;
        }

        var pc = player.AddComponent<PlayerController>();
        pc.moveSpeed = 4f;
        pc.rotationSpeed = 10f;
        pc.cameraController = camCtrl;

        var pi = player.AddComponent<PlayerInteractor>();
        pi.detectionRadius = 2.5f;

        // CarriedControl replaces ModuleInventory — tracks the single control being carried
        var carrier = player.AddComponent<CarriedControl>();

        // Carry visual: small sphere above player shown when carrying a control
        var carryVisual = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        carryVisual.name = "CarryVisual";
        carryVisual.transform.SetParent(player.transform);
        carryVisual.transform.localPosition = new Vector3(0, 2.5f, 0);
        carryVisual.transform.localScale = new Vector3(0.3f, 0.3f, 0.3f);
        SetColor(carryVisual, new Color(1f, 0.8f, 0f));
        carryVisual.SetActive(false);
        Object.DestroyImmediate(carryVisual.GetComponent<Collider>());
        carrier.carryVisual = carryVisual;

        // Player visual body
        var playerBody = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        playerBody.name = "PlayerVisual";
        playerBody.transform.SetParent(player.transform);
        playerBody.transform.localPosition = Vector3.zero;
        playerBody.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
        SetColor(playerBody, new Color(0.2f, 0.6f, 1f));
        Object.DestroyImmediate(playerBody.GetComponent<Collider>());

        // Wire camera to player
        camCtrl.player = player.transform;

        // --- 6. Source UI Panel (Area 1) — contains a detachable Plus Button ---
        // This is a UI panel on the wall that the player can take a button from.
        var sourcePanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sourcePanel.name = "SourcePanel";
        sourcePanel.transform.localScale = new Vector3(2f, 1.5f, 0.3f);
        sourcePanel.transform.position = new Vector3(-16, 1.5f, -6.5f);
        SetColor(sourcePanel, new Color(0.08f, 0.12f, 0.2f));

        // The Plus Button visual (child cube that looks like a + button)
        var plusButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
        plusButton.name = "PlusButtonPresent";
        plusButton.transform.SetParent(sourcePanel.transform);
        plusButton.transform.localPosition = new Vector3(0, 0, 0.2f);
        plusButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.1f);
        SetColor(plusButton, new Color(0.2f, 0.8f, 0.2f)); // green + button

        // Empty slot visual (shown after the button is detached)
        var emptySlot = GameObject.CreatePrimitive(PrimitiveType.Cube);
        emptySlot.name = "PlusButtonAbsent";
        emptySlot.transform.SetParent(sourcePanel.transform);
        emptySlot.transform.localPosition = new Vector3(0, 0, 0.2f);
        emptySlot.transform.localScale = new Vector3(0.35f, 0.35f, 0.05f);
        SetColor(emptySlot, new Color(0.15f, 0.15f, 0.15f)); // dark empty slot
        emptySlot.SetActive(false);

        // UIControlSlot on the source panel — holds the Plus Button
        var sourceSlot = sourcePanel.AddComponent<UIControlSlot>();
        sourceSlot.controlPresentVisual = plusButton;
        sourceSlot.controlAbsentVisual = emptySlot;
        sourceSlot.hasControl = true;

        // --- 7. Ticket Machine (Area 2) — target that needs the Plus Button ---
        var ticketMachine = GameObject.CreatePrimitive(PrimitiveType.Cube);
        ticketMachine.name = "TicketMachine";
        ticketMachine.transform.localScale = new Vector3(1.5f, 2.5f, 1f);
        ticketMachine.transform.position = new Vector3(-5, 1.25f, 0);
        SetColor(ticketMachine, new Color(0.15f, 0.15f, 0.2f));

        // Machine screen
        var machineScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        machineScreen.name = "MachineScreen";
        machineScreen.transform.SetParent(ticketMachine.transform);
        machineScreen.transform.localPosition = new Vector3(0, 0.3f, 0.55f);
        machineScreen.transform.localScale = new Vector3(0.8f, 0.6f, 0.05f);
        SetColor(machineScreen, new Color(0.3f, 0.05f, 0f)); // dim red screen (malfunctioning)

        // Target slot visual (empty — waiting for the Plus Button)
        var targetEmpty = GameObject.CreatePrimitive(PrimitiveType.Cube);
        targetEmpty.name = "TargetEmpty";
        targetEmpty.transform.SetParent(ticketMachine.transform);
        targetEmpty.transform.localPosition = new Vector3(0, -0.5f, 0.55f);
        targetEmpty.transform.localScale = new Vector3(0.35f, 0.35f, 0.05f);
        SetColor(targetEmpty, new Color(0.15f, 0.15f, 0.15f));

        // Target installed visual (shown when the Plus Button is installed)
        var targetInstalled = GameObject.CreatePrimitive(PrimitiveType.Cube);
        targetInstalled.name = "TargetInstalled";
        targetInstalled.transform.SetParent(ticketMachine.transform);
        targetInstalled.transform.localPosition = new Vector3(0, -0.5f, 0.55f);
        targetInstalled.transform.localScale = new Vector3(0.4f, 0.4f, 0.1f);
        SetColor(targetInstalled, new Color(0.2f, 0.8f, 0.2f)); // green when installed
        targetInstalled.SetActive(false);

        // Success light (off by default)
        var successLightObj = new GameObject("SuccessLight");
        successLightObj.transform.SetParent(ticketMachine.transform);
        successLightObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        var successLight = successLightObj.AddComponent<Light>();
        successLight.type = LightType.Point;
        successLight.color = Color.green;
        successLight.intensity = 2f;
        successLight.range = 5f;
        successLightObj.SetActive(false);

        // Failure light (on by default - red)
        var failureLightObj = new GameObject("FailureLight");
        failureLightObj.transform.SetParent(ticketMachine.transform);
        failureLightObj.transform.localPosition = new Vector3(0, 1.5f, 0);
        var failureLight = failureLightObj.AddComponent<Light>();
        failureLight.type = LightType.Point;
        failureLight.color = Color.red;
        failureLight.intensity = 1.5f;
        failureLight.range = 5f;

        // Puzzle controller on the ticket machine
        var puzzleCtrl = ticketMachine.AddComponent<PuzzleController>();
        puzzleCtrl.targetMin = 0.6f;
        puzzleCtrl.targetMax = 0.8f;
        puzzleCtrl.successLight = successLightObj;
        puzzleCtrl.failureLight = failureLightObj;

        // UIControlTarget on the ticket machine — accepts the Plus Button
        var target = ticketMachine.AddComponent<UIControlTarget>();
        target.acceptedControlID = "plus";
        target.installedVisual = targetInstalled;
        target.emptyVisual = targetEmpty;
        target.puzzleController = puzzleCtrl;

        // --- 8. Door between Area 2 and Area 3 ---
        var door23 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door23.name = "Door_2_to_3";
        door23.transform.localScale = new Vector3(0.4f, 4f, 4f);
        door23.transform.position = new Vector3(0, 2, 0);
        SetColor(door23, new Color(0.3f, 0.2f, 0.1f));
        var door23Ctrl = door23.AddComponent<DoorController>();
        door23Ctrl.slideOpen = true;
        door23Ctrl.slideDistance = 4f;
        door23Ctrl.openSpeed = 2f;
        puzzleCtrl.doorsToOpen = new DoorController[] { door23Ctrl };

        // --- 9. Control Panel (Area 3) — second puzzle ---
        var controlPanel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        controlPanel.name = "ControlPanel";
        controlPanel.transform.localScale = new Vector3(2f, 2f, 0.8f);
        controlPanel.transform.position = new Vector3(5, 1f, 0);
        SetColor(controlPanel, new Color(0.12f, 0.12f, 0.16f));

        var panelScreen = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panelScreen.name = "PanelScreen";
        panelScreen.transform.SetParent(controlPanel.transform);
        panelScreen.transform.localPosition = new Vector3(0, 0.3f, 0.45f);
        panelScreen.transform.localScale = new Vector3(1.2f, 0.6f, 0.05f);
        SetColor(panelScreen, new Color(0f, 0.1f, 0.3f));

        var panelSuccessLight = new GameObject("PanelSuccessLight");
        panelSuccessLight.transform.SetParent(controlPanel.transform);
        panelSuccessLight.transform.localPosition = new Vector3(0, 1.3f, 0);
        var pSuccessLight = panelSuccessLight.AddComponent<Light>();
        pSuccessLight.type = LightType.Point;
        pSuccessLight.color = Color.green;
        pSuccessLight.intensity = 2f;
        pSuccessLight.range = 5f;
        panelSuccessLight.SetActive(false);

        var panelFailureLight = new GameObject("PanelFailureLight");
        panelFailureLight.transform.SetParent(controlPanel.transform);
        panelFailureLight.transform.localPosition = new Vector3(0, 1.3f, 0);
        var pFailureLight = panelFailureLight.AddComponent<Light>();
        pFailureLight.type = LightType.Point;
        pFailureLight.color = Color.red;
        pFailureLight.intensity = 1.5f;
        pFailureLight.range = 5f;

        var panelPuzzleCtrl = controlPanel.AddComponent<PuzzleController>();
        panelPuzzleCtrl.targetMin = 0.3f;
        panelPuzzleCtrl.targetMax = 0.5f;
        panelPuzzleCtrl.successLight = panelSuccessLight;
        panelPuzzleCtrl.failureLight = panelFailureLight;

        // Second source panel in Area 2 — contains a Volume button for Area 3
        var sourcePanel2 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sourcePanel2.name = "VolumeSourcePanel";
        sourcePanel2.transform.localScale = new Vector3(2f, 1.5f, 0.3f);
        sourcePanel2.transform.position = new Vector3(-3, 1.5f, 6.5f);
        SetColor(sourcePanel2, new Color(0.08f, 0.12f, 0.2f));

        var volumeButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
        volumeButton.name = "VolumeButtonPresent";
        volumeButton.transform.SetParent(sourcePanel2.transform);
        volumeButton.transform.localPosition = new Vector3(0, 0, 0.2f);
        volumeButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.1f);
        SetColor(volumeButton, new Color(0.2f, 0.6f, 0.9f)); // blue volume button

        var volumeEmpty = GameObject.CreatePrimitive(PrimitiveType.Cube);
        volumeEmpty.name = "VolumeButtonAbsent";
        volumeEmpty.transform.SetParent(sourcePanel2.transform);
        volumeEmpty.transform.localPosition = new Vector3(0, 0, 0.2f);
        volumeEmpty.transform.localScale = new Vector3(0.35f, 0.35f, 0.05f);
        SetColor(volumeEmpty, new Color(0.15f, 0.15f, 0.15f));
        volumeEmpty.SetActive(false);

        var sourceSlot2 = sourcePanel2.AddComponent<UIControlSlot>();
        sourceSlot2.controlPresentVisual = volumeButton;
        sourceSlot2.controlAbsentVisual = volumeEmpty;

        // Target on control panel
        var panelTargetEmpty = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panelTargetEmpty.name = "PanelTargetEmpty";
        panelTargetEmpty.transform.SetParent(controlPanel.transform);
        panelTargetEmpty.transform.localPosition = new Vector3(0, -0.5f, 0.45f);
        panelTargetEmpty.transform.localScale = new Vector3(0.35f, 0.35f, 0.05f);
        SetColor(panelTargetEmpty, new Color(0.15f, 0.15f, 0.15f));

        var panelTargetInstalled = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panelTargetInstalled.name = "PanelTargetInstalled";
        panelTargetInstalled.transform.SetParent(controlPanel.transform);
        panelTargetInstalled.transform.localPosition = new Vector3(0, -0.5f, 0.45f);
        panelTargetInstalled.transform.localScale = new Vector3(0.4f, 0.4f, 0.1f);
        SetColor(panelTargetInstalled, new Color(0.2f, 0.6f, 0.9f));
        panelTargetInstalled.SetActive(false);

        var panelTarget = controlPanel.AddComponent<UIControlTarget>();
        panelTarget.acceptedControlID = "volume";
        panelTarget.installedVisual = panelTargetInstalled;
        panelTarget.emptyVisual = panelTargetEmpty;
        panelTarget.puzzleController = panelPuzzleCtrl;

        // --- 10. Door between Area 3 and Area 4 ---
        var door34 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        door34.name = "Door_3_to_4";
        door34.transform.localScale = new Vector3(0.4f, 4f, 4f);
        door34.transform.position = new Vector3(10, 2, 0);
        SetColor(door34, new Color(0.3f, 0.2f, 0.1f));
        var door34Ctrl = door34.AddComponent<DoorController>();
        door34Ctrl.slideOpen = true;
        door34Ctrl.slideDistance = 4f;
        door34Ctrl.openSpeed = 2f;
        panelPuzzleCtrl.doorsToOpen = new DoorController[] { door34Ctrl };

        // --- 11. Train Front (Area 4) — final puzzle ---
        var trainFront = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trainFront.name = "TrainFront";
        trainFront.transform.localScale = new Vector3(6f, 3f, 2f);
        trainFront.transform.position = new Vector3(16, 1.5f, 0);
        SetColor(trainFront, new Color(0.2f, 0.25f, 0.3f));

        var trainDoor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trainDoor.name = "TrainDoor";
        trainDoor.transform.SetParent(trainFront.transform);
        trainDoor.transform.localPosition = new Vector3(0, 0, 1.05f);
        trainDoor.transform.localScale = new Vector3(2f, 2.5f, 0.1f);
        SetColor(trainDoor, new Color(0.1f, 0.1f, 0.12f));
        Object.DestroyImmediate(trainDoor.GetComponent<BoxCollider>());

        var trainDoorCtrl = trainDoor.AddComponent<DoorController>();
        trainDoorCtrl.slideOpen = true;
        trainDoorCtrl.slideDistance = 3f;
        trainDoorCtrl.openSpeed = 3f;
        trainDoor.AddComponent<BoxCollider>();

        var trainSuccessLight = new GameObject("TrainSuccessLight");
        trainSuccessLight.transform.SetParent(trainFront.transform);
        trainSuccessLight.transform.localPosition = new Vector3(0, 2f, 0);
        var tSuccessLight = trainSuccessLight.AddComponent<Light>();
        tSuccessLight.type = LightType.Point;
        tSuccessLight.color = Color.green;
        tSuccessLight.intensity = 2f;
        tSuccessLight.range = 6f;
        trainSuccessLight.SetActive(false);

        var trainFailureLight = new GameObject("TrainFailureLight");
        trainFailureLight.transform.SetParent(trainFront.transform);
        trainFailureLight.transform.localPosition = new Vector3(0, 2f, 0);
        var tFailureLight = trainFailureLight.AddComponent<Light>();
        tFailureLight.type = LightType.Point;
        tFailureLight.color = Color.red;
        tFailureLight.intensity = 1.5f;
        tFailureLight.range = 6f;

        var trainPuzzleCtrl = trainFront.AddComponent<PuzzleController>();
        trainPuzzleCtrl.targetMin = 0.4f;
        trainPuzzleCtrl.targetMax = 0.6f;
        trainPuzzleCtrl.successLight = trainSuccessLight;
        trainPuzzleCtrl.failureLight = trainFailureLight;
        trainPuzzleCtrl.doorsToOpen = new DoorController[] { trainDoorCtrl };

        // Third source panel in Area 3 — contains a Settings button for Area 4
        var sourcePanel3 = GameObject.CreatePrimitive(PrimitiveType.Cube);
        sourcePanel3.name = "SettingsSourcePanel";
        sourcePanel3.transform.localScale = new Vector3(2f, 1.5f, 0.3f);
        sourcePanel3.transform.position = new Vector3(8, 1.5f, 6.5f);
        SetColor(sourcePanel3, new Color(0.08f, 0.12f, 0.2f));

        var settingsButton = GameObject.CreatePrimitive(PrimitiveType.Cube);
        settingsButton.name = "SettingsButtonPresent";
        settingsButton.transform.SetParent(sourcePanel3.transform);
        settingsButton.transform.localPosition = new Vector3(0, 0, 0.2f);
        settingsButton.transform.localScale = new Vector3(0.4f, 0.4f, 0.1f);
        SetColor(settingsButton, new Color(0.9f, 0.5f, 1f)); // purple settings button

        var settingsEmpty = GameObject.CreatePrimitive(PrimitiveType.Cube);
        settingsEmpty.name = "SettingsButtonAbsent";
        settingsEmpty.transform.SetParent(sourcePanel3.transform);
        settingsEmpty.transform.localPosition = new Vector3(0, 0, 0.2f);
        settingsEmpty.transform.localScale = new Vector3(0.35f, 0.35f, 0.05f);
        SetColor(settingsEmpty, new Color(0.15f, 0.15f, 0.15f));
        settingsEmpty.SetActive(false);

        var sourceSlot3 = sourcePanel3.AddComponent<UIControlSlot>();
        sourceSlot3.controlPresentVisual = settingsButton;
        sourceSlot3.controlAbsentVisual = settingsEmpty;

        // Target on train front
        var trainTargetEmpty = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trainTargetEmpty.name = "TrainTargetEmpty";
        trainTargetEmpty.transform.SetParent(trainFront.transform);
        trainTargetEmpty.transform.localPosition = new Vector3(0, -0.5f, 1.05f);
        trainTargetEmpty.transform.localScale = new Vector3(0.35f, 0.35f, 0.05f);
        SetColor(trainTargetEmpty, new Color(0.15f, 0.15f, 0.15f));

        var trainTargetInstalled = GameObject.CreatePrimitive(PrimitiveType.Cube);
        trainTargetInstalled.name = "TrainTargetInstalled";
        trainTargetInstalled.transform.SetParent(trainFront.transform);
        trainTargetInstalled.transform.localPosition = new Vector3(0, -0.5f, 1.05f);
        trainTargetInstalled.transform.localScale = new Vector3(0.4f, 0.4f, 0.1f);
        SetColor(trainTargetInstalled, new Color(0.9f, 0.5f, 1f));
        trainTargetInstalled.SetActive(false);

        var trainTarget = trainFront.AddComponent<UIControlTarget>();
        trainTarget.acceptedControlID = "settings";
        trainTarget.installedVisual = trainTargetInstalled;
        trainTarget.emptyVisual = trainTargetEmpty;
        trainTarget.puzzleController = trainPuzzleCtrl;

        // --- 11b. Platform details (Area 4) ---
        var platformFloor = GameObject.CreatePrimitive(PrimitiveType.Cube);
        platformFloor.name = "PlatformFloor";
        platformFloor.transform.localScale = new Vector3(8, 0.5f, 6);
        platformFloor.transform.position = new Vector3(16, -0.25f, 0);
        SetColor(platformFloor, new Color(0.08f, 0.08f, 0.1f));

        var track = GameObject.CreatePrimitive(PrimitiveType.Cube);
        track.name = "Track";
        track.transform.localScale = new Vector3(8, 0.2f, 2);
        track.transform.position = new Vector3(16, -0.9f, 2);
        SetColor(track, new Color(0.03f, 0.03f, 0.03f));

        var bench = GameObject.CreatePrimitive(PrimitiveType.Cube);
        bench.name = "Bench";
        bench.transform.localScale = new Vector3(2, 0.5f, 0.8f);
        bench.transform.position = new Vector3(14, 0.25f, -3);
        SetColor(bench, new Color(0.15f, 0.1f, 0.08f));

        // --- 12. Exit Trigger (Area 4) ---
        var exitObj = new GameObject("ExitTrigger");
        exitObj.transform.position = new Vector3(18, 1, 0);
        var exitCollider = exitObj.AddComponent<BoxCollider>();
        exitCollider.isTrigger = true;
        exitCollider.size = new Vector3(2f, 4f, 6f);
        var exitTrigger = exitObj.AddComponent<ExitTrigger>();

        // --- 12b. Camera Zones at puzzle areas ---
        // Camera zone for Area 2 (Ticket Machine)
        CreateCameraZone("CameraZone_TicketMachine", new Vector3(-5, 1, 0), new Vector3(8, 4, 8),
            new Vector3(-5, 5, -8), Quaternion.Euler(45, 0, 0));

        // Camera zone for Area 3 (Control Panel)
        CreateCameraZone("CameraZone_ControlPanel", new Vector3(5, 1, 0), new Vector3(8, 4, 8),
            new Vector3(5, 5, -8), Quaternion.Euler(45, 0, 0));

        // Camera zone for Area 4 (Train)
        CreateCameraZone("CameraZone_Train", new Vector3(16, 1, 0), new Vector3(8, 4, 8),
            new Vector3(16, 5, -8), Quaternion.Euler(45, 0, 0));

        // --- 13. UI Canvas ---
        var canvasObj = new GameObject("Canvas");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        var scaler = canvasObj.GetComponent<CanvasScaler>();
        scaler.referenceResolution = new Vector2(1920, 1080);
        canvasObj.AddComponent<GraphicRaycaster>();

        // Event System
        var eventSystemObj = new GameObject("EventSystem");
        eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
        eventSystemObj.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();

        // --- 14. HUD ---
        var hudObj = new GameObject("HUD_Manager");
        var hud = hudObj.AddComponent<HUDController>();

        var objectiveTextObj = CreateText("ObjectiveText", canvasObj.transform,
            new Vector2(0, -40), new Vector2(600, 60), 24, TextAnchor.UpperCenter);
        objectiveTextObj.text = "Objective: Find a way out of the station.";
        hud.objectiveText = objectiveTextObj;

        var promptContainer = new GameObject("PromptContainer");
        promptContainer.transform.SetParent(canvasObj.transform, false);
        var promptRT = promptContainer.AddComponent<RectTransform>();
        promptRT.anchorMin = new Vector2(0.5f, 0);
        promptRT.anchorMax = new Vector2(0.5f, 0);
        promptRT.pivot = new Vector2(0.5f, 0);
        promptRT.anchoredPosition = new Vector2(0, 40);
        promptRT.sizeDelta = new Vector2(400, 40);

        var promptTextObj = CreateText("PromptText", promptContainer.transform,
            Vector2.zero, new Vector2(400, 40), 20, TextAnchor.MiddleCenter);
        promptTextObj.text = "Press E to interact";
        hud.promptText = promptTextObj;
        hud.promptContainer = promptContainer;

        var notifTextObj = CreateText("NotificationText", canvasObj.transform,
            new Vector2(0, 100), new Vector2(600, 40), 22, TextAnchor.UpperCenter);
        notifTextObj.text = "";
        hud.notificationText = notifTextObj;

        // --- 15. Puzzle Panel 1 (Ticket Machine — Plus Button puzzle) ---
        var puzzlePanel = CreatePuzzlePanel("PuzzlePanel", canvasObj.transform, "Increase Ticket Count");
        var slider1 = puzzlePanel.slider;
        var closeBtn1 = puzzlePanel.closeButton;
        puzzleCtrl.puzzleSlider = slider1;
        puzzleCtrl.puzzlePanel = puzzlePanel.panel;
        target.puzzlePanel = puzzlePanel.panel;
        var closeHelper1 = closeBtn1.AddComponent<ClosePanelButton>();
        closeHelper1.puzzleController = puzzleCtrl;

        // --- 15b. Puzzle Panel 2 (Control Panel — Volume puzzle) ---
        var brightnessPanel = CreatePuzzlePanel("BrightnessPanel", canvasObj.transform, "Adjust Volume Level");
        var slider2 = brightnessPanel.slider;
        var closeBtn2 = brightnessPanel.closeButton;
        panelPuzzleCtrl.puzzleSlider = slider2;
        panelPuzzleCtrl.puzzlePanel = brightnessPanel.panel;
        panelTarget.puzzlePanel = brightnessPanel.panel;
        var closeHelper2 = closeBtn2.AddComponent<ClosePanelButton>();
        closeHelper2.puzzleController = panelPuzzleCtrl;

        // --- 15c. Puzzle Panel 3 (Train — Settings puzzle) ---
        var settingsPanel = CreatePuzzlePanel("SettingsPanel", canvasObj.transform, "Align Train Doors");
        var slider3 = settingsPanel.slider;
        var closeBtn3 = settingsPanel.closeButton;
        trainPuzzleCtrl.puzzleSlider = slider3;
        trainPuzzleCtrl.puzzlePanel = settingsPanel.panel;
        trainTarget.puzzlePanel = settingsPanel.panel;
        var closeHelper3 = closeBtn3.AddComponent<ClosePanelButton>();
        closeHelper3.puzzleController = trainPuzzleCtrl;

        // --- 16. Ending Panel (hidden) ---
        var endingPanel = new GameObject("EndingPanel");
        endingPanel.transform.SetParent(canvasObj.transform, false);
        var endRT = endingPanel.AddComponent<RectTransform>();
        endRT.anchorMin = Vector2.zero;
        endRT.anchorMax = Vector2.one;
        endRT.offsetMin = Vector2.zero;
        endRT.offsetMax = Vector2.zero;
        var endImage = endingPanel.AddComponent<Image>();
        endImage.color = new Color(0f, 0f, 0f, 0.85f);
        endingPanel.SetActive(false);

        var endText = CreateText("EndingText", endingPanel.transform,
            Vector2.zero, new Vector2(600, 100), 48, TextAnchor.MiddleCenter);
        endText.text = "YOU MADE IT OUT";

        // --- 17. UIControlData ScriptableObjects ---
        var plusData = ScriptableObject.CreateInstance<UIControlData>();
        plusData.displayName = "Plus Button";
        plusData.controlID = "plus";
        plusData.description = "Increases a value or quantity.";
        plusData.controlColor = new Color(0.2f, 0.8f, 0.2f);
        AssetDatabase.CreateAsset(plusData, "Assets/ScriptableObjects/PlusButton.asset");

        var volumeData = ScriptableObject.CreateInstance<UIControlData>();
        volumeData.displayName = "Volume Button";
        volumeData.controlID = "volume";
        volumeData.description = "Controls audio volume levels.";
        volumeData.controlColor = new Color(0.2f, 0.6f, 0.9f);
        AssetDatabase.CreateAsset(volumeData, "Assets/ScriptableObjects/VolumeButton.asset");

        var settingsData = ScriptableObject.CreateInstance<UIControlData>();
        settingsData.displayName = "Settings Button";
        settingsData.controlID = "settings";
        settingsData.description = "Controls system configuration.";
        settingsData.controlColor = new Color(0.9f, 0.5f, 1f);
        AssetDatabase.CreateAsset(settingsData, "Assets/ScriptableObjects/SettingsButton.asset");
        AssetDatabase.SaveAssets();

        // Assign control data to source slots
        sourceSlot.controlData = plusData;
        sourceSlot2.controlData = volumeData;
        sourceSlot3.controlData = settingsData;

        // --- 18. Quest Manager ---
        var questObj = new GameObject("QuestManager");
        var quest = questObj.AddComponent<QuestManager>();
        quest.hud = hud;

        // --- 19. Game Manager ---
        var gmObj = new GameObject("GameManager");
        var gm = gmObj.AddComponent<GameManager>();
        gm.questManager = quest;
        gm.endingPanel = endingPanel;

        // --- 20. Wire remaining references ---
        exitTrigger.gameManager = gm;
        puzzleCtrl.questManager = quest;
        panelPuzzleCtrl.questManager = quest;
        trainPuzzleCtrl.questManager = quest;
        puzzleCtrl.cameraController = camCtrl;
        panelPuzzleCtrl.cameraController = camCtrl;
        trainPuzzleCtrl.cameraController = camCtrl;
        pi.hud = hud;

        // --- 21. Signage ---
        CreateSign("StationSign", new Vector3(-16, 2, -6), "");
        CreateSign("TicketSign", new Vector3(-7, 3, 3), "");
        CreateSign("ControlSign", new Vector3(3, 3, 3), "");
        CreateSign("PlatformSign", new Vector3(13, 3, 3), "");

        // --- Save ---
        EditorSceneManager.SaveScene(scene, SCENE_PATH);
        AddSceneToBuild(SCENE_PATH);

        Debug.Log("[SceneBuilder] Station scene built successfully at " + SCENE_PATH);
        EditorUtility.DisplayDialog("Scene Built", "Station scene created at " + SCENE_PATH +
            "\n\nPress Play to test. Use WASD to move, E to interact.", "OK");
    }

    // --- Helper struct for puzzle panel creation ---
    private struct PuzzlePanelResult
    {
        public GameObject panel;
        public Slider slider;
        public GameObject closeButton;
    }

    private static PuzzlePanelResult CreatePuzzlePanel(string name, Transform parent, string title)
    {
        var result = new PuzzlePanelResult();

        result.panel = new GameObject(name);
        result.panel.transform.SetParent(parent, false);
        var panelRT = result.panel.AddComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.5f, 0.5f);
        panelRT.anchorMax = new Vector2(0.5f, 0.5f);
        panelRT.pivot = new Vector2(0.5f, 0.5f);
        panelRT.anchoredPosition = Vector2.zero;
        panelRT.sizeDelta = new Vector2(400, 200);
        var panelImage = result.panel.AddComponent<Image>();
        panelImage.color = new Color(0.1f, 0.1f, 0.15f, 0.9f);
        result.panel.SetActive(false);

        var panelTitle = CreateText(name + "_Title", result.panel.transform,
            new Vector2(0, 60), new Vector2(380, 40), 22, TextAnchor.MiddleCenter);
        panelTitle.text = title;

        var sliderObj = new GameObject(name + "_Slider");
        sliderObj.transform.SetParent(result.panel.transform, false);
        var sliderRT = sliderObj.AddComponent<RectTransform>();
        sliderRT.anchorMin = new Vector2(0.5f, 0.5f);
        sliderRT.anchorMax = new Vector2(0.5f, 0.5f);
        sliderRT.pivot = new Vector2(0.5f, 0.5f);
        sliderRT.anchoredPosition = new Vector2(0, -10);
        sliderRT.sizeDelta = new Vector2(300, 40);
        result.slider = sliderObj.AddComponent<Slider>();
        result.slider.minValue = 0f;
        result.slider.maxValue = 1f;
        result.slider.value = 0f;
        SetupSliderVisuals(result.slider);

        result.closeButton = new GameObject(name + "_CloseButton");
        result.closeButton.transform.SetParent(result.panel.transform, false);
        var closeBtnRT = result.closeButton.AddComponent<RectTransform>();
        closeBtnRT.anchorMin = new Vector2(1, 0);
        closeBtnRT.anchorMax = new Vector2(1, 0);
        closeBtnRT.pivot = new Vector2(1, 0);
        closeBtnRT.anchoredPosition = new Vector2(-20, 20);
        closeBtnRT.sizeDelta = new Vector2(100, 40);
        var closeBtnImage = result.closeButton.AddComponent<Image>();
        closeBtnImage.color = new Color(0.2f, 0.2f, 0.3f);
        result.closeButton.AddComponent<Button>();
        var closeBtnText = CreateText(name + "_CloseText", result.closeButton.transform,
            Vector2.zero, new Vector2(100, 40), 18, TextAnchor.MiddleCenter);
        closeBtnText.text = "Close";

        return result;
    }

    // --- Helper: Create a CameraZone with anchor ---
    private static void CreateCameraZone(string name, Vector3 center, Vector3 size,
        Vector3 anchorPos, Quaternion anchorRot)
    {
        var zoneObj = new GameObject(name);
        zoneObj.transform.position = center;
        var zoneCollider = zoneObj.AddComponent<BoxCollider>();
        zoneCollider.isTrigger = true;
        zoneCollider.size = size;
        var zone = zoneObj.AddComponent<CameraZone>();

        // Create the camera anchor as a child
        var anchor = new GameObject(name + "_Anchor");
        anchor.transform.SetParent(zoneObj.transform);
        anchor.transform.position = anchorPos;
        anchor.transform.rotation = anchorRot;
        zone.cameraAnchor = anchor.transform;
    }

    private static GameObject CreateWall(string name, Vector3 pos, Vector3 scale)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.position = pos;
        wall.transform.localScale = scale;
        SetColor(wall, new Color(0.12f, 0.12f, 0.18f));
        return wall;
    }

    private static void SetColor(GameObject obj, Color color)
    {
        var renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mat.color = color;
            mat.SetFloat("_Metallic", 0.5f);
            mat.SetFloat("_Smoothness", 0.4f);
            renderer.sharedMaterial = mat;
        }
    }

    private static Text CreateText(string name, Transform parent,
        Vector2 position, Vector2 size, int fontSize, TextAnchor alignment)
    {
        var obj = new GameObject(name);
        obj.transform.SetParent(parent, false);
        var rt = obj.AddComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 1f);
        rt.anchorMax = new Vector2(0.5f, 1f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = position;
        rt.sizeDelta = size;
        var text = obj.AddComponent<Text>();
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = Color.white;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    private static void SetupSliderVisuals(Slider slider)
    {
        var bgObj = new GameObject("Background");
        bgObj.transform.SetParent(slider.transform, false);
        var bgRT = bgObj.AddComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg = bgObj.AddComponent<Image>();
        bgImg.color = new Color(0.15f, 0.15f, 0.15f);

        var fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(slider.transform, false);
        var fillAreaRT = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRT.anchorMin = new Vector2(0, 0.25f);
        fillAreaRT.anchorMax = new Vector2(1, 0.75f);
        fillAreaRT.offsetMin = new Vector2(10, 0);
        fillAreaRT.offsetMax = new Vector2(-10, 0);

        var fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        var fillRT = fillObj.AddComponent<RectTransform>();
        fillRT.anchorMin = Vector2.zero;
        fillRT.anchorMax = Vector2.one;
        fillRT.offsetMin = Vector2.zero;
        fillRT.offsetMax = Vector2.zero;
        var fillImg = fillObj.AddComponent<Image>();
        fillImg.color = new Color(0f, 0.6f, 0.2f);
        slider.fillRect = fillRT;

        var handleAreaObj = new GameObject("Handle Slide Area");
        handleAreaObj.transform.SetParent(slider.transform, false);
        var handleAreaRT = handleAreaObj.AddComponent<RectTransform>();
        handleAreaRT.anchorMin = Vector2.zero;
        handleAreaRT.anchorMax = Vector2.one;
        handleAreaRT.offsetMin = Vector2.zero;
        handleAreaRT.offsetMax = Vector2.zero;

        var handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(handleAreaObj.transform, false);
        var handleRT = handleObj.AddComponent<RectTransform>();
        handleRT.sizeDelta = new Vector2(20, 20);
        var handleImg = handleObj.AddComponent<Image>();
        handleImg.color = Color.white;
        slider.handleRect = handleRT;

        slider.targetGraphic = handleImg;
        slider.direction = Slider.Direction.LeftToRight;
    }

    private static GameObject CreateSign(string name, Vector3 pos, string text)
    {
        var signObj = new GameObject(name);
        signObj.transform.position = pos;
        signObj.transform.rotation = Quaternion.Euler(0, 180, 0);

        var panel = GameObject.CreatePrimitive(PrimitiveType.Cube);
        panel.name = name + "_Panel";
        panel.transform.SetParent(signObj.transform);
        panel.transform.localPosition = Vector3.zero;
        panel.transform.localScale = new Vector3(3, 1, 0.1f);
        SetColor(panel, new Color(0.05f, 0.2f, 0.1f));
        Object.DestroyImmediate(panel.GetComponent<Collider>());
        return signObj;
    }

    private static void AddSceneToBuild(string scenePath)
    {
        var scenes = EditorBuildSettings.scenes;
        var scene = new EditorBuildSettingsScene(scenePath, true);
        var newScenes = new System.Collections.Generic.List<EditorBuildSettingsScene>(scenes);
        newScenes.RemoveAll(s => s.path == scenePath);
        newScenes.Insert(0, scene);
        foreach (var s in newScenes)
        {
            if (s.path != scenePath)
                s.enabled = false;
        }
        EditorBuildSettings.scenes = newScenes.ToArray();
    }
}
