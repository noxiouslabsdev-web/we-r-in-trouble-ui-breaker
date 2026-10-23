# Setup Guide - WE R IN TROUBLE: UI BREAKER

## ⚡ QUICK BUILD (Recommended)
Instead of manually following all steps below, use the automated scene builder:
1. Open the project in Unity 6.3
2. Wait for scripts to compile (check Console for errors)
3. Menu bar → **Tools → TGC → Build Station Scene**
4. The script creates ALL GameObjects, UI, references, and saves as Assets/Scenes/Station.unity
5. Press **Play** to test

The manual steps below are still documented for reference and customization.

---

## Prerequisites
- Unity 6.3.27f1 (6000.3.27f1)
- Project opens with URP PC quality active
- Input System (New) is configured

## Scripts Created (all in Assets/Scripts/)
| Script | Purpose |
|--------|---------|
| IInteractable.cs | Interface for interactable objects |
| PlayerController.cs | WASD movement with CharacterController + Input System |
| PlayerInteractor.cs | Detects nearby interactables, fires on key press |
| SimpleInteractable.cs | Basic interactable (doors, buttons, test objects) |
| ModuleData.cs | ScriptableObject defining a UI Module |
| ModuleInventory.cs | Tracks collected modules on the player |
| ModulePickup.cs | World object that gives a module on interact |
| ModuleSocket.cs | Socket on machines that accepts modules |
| PuzzleController.cs | Slider puzzle logic — checks target range, opens doors |
| DoorController.cs | Door that slides open or deactivates |
| HUDController.cs | Objective text, prompt, notifications |
| QuestManager.cs | Tracks objective progression |
| GameManager.cs | Global state, ending trigger |
| ExitTrigger.cs | Trigger zone that ends the game |

## Manual Unity Editor Steps

### Step 1: Create the Player GameObject
1. Open Assets/Scenes/SampleScene.unity
2. Create Empty GameObject → name it **Player**
3. Add components:
   - **Character Controller** (Set Center to 0,1,0; Height to 2; Radius to 0.4)
   - **Player Input** component
     - Actions: assign Assets/InputSystem_Actions.inputactions
     - Behavior: **Send Messages** or **Unity Events** (Send Messages is simplest)
     - Default Map: **Player**
   - **Player Controller** (script)
   - **Player Interactor** (script)
     - Detection Radius: 1.5
   - **Module Inventory** (script)
   - **Audio Source** (optional, for footstep sounds)
4. Tag the Player GameObject as **Player** (Tag → Add Tag → "Player" if missing)
5. Add a small Sphere Collider (Trigger, Radius 0.5) for interaction detection
   - Or rely on OverlapSphere in PlayerInteractor (no trigger collider strictly needed)

### Step 2: Set Up the Camera
1. Select the existing **Main Camera** in the scene
2. Position it for a fixed 3-quarter view:
   - Position: (0, 12, -10)
   - Rotation: (50, 0, 0)
3. Set **Camera > Projection**: Perspective
4. Field of View: 60
5. The camera should frame all four areas from above

### Step 3: Create the Greybox Environment
Use simple Cube primitives scaled to represent walls/floors.

**Floor:**
- Cube → name **Floor** → Scale (40, 1, 15) → Position (0, -0.5, 0)

**Walls (per area dividers):**
- North Wall: Cube, Scale (40, 4, 0.5), Position (0, 2, 7.5)
- South Wall: Cube, Scale (40, 4, 0.5), Position (0, 2, -7.5)

**Area dividers (with doorway gaps):**
- Between Area 1 & 2: Cube Scale (0.5, 4, 6), Position (-10, 2, 4) + matching at z=-4
- Between Area 2 & 3: Cube Scale (0.5, 4, 6), Position (0, 2, 4) + matching at z=-4
- Between Area 3 & 4: Cube Scale (0.5, 4, 6), Position (10, 2, 4) + matching at z=-4

Create a material for walls: dark metallic (color #1a1a2e, Metallic 0.8, Smoothness 0.5)

### Step 4: Create the Ticket Machine (Area 2 — Main Puzzle)
1. Create Cube → name **TicketMachine** → Scale (1.5, 2.5, 1) → Position (-5, 1.25, 0)
2. Add a child Cube for the screen → Scale (0.8, 0.6, 0.1) → Position (0, 0.5, 0.55)
3. Add components to TicketMachine:
   - **Module Socket** (script)
     - Accepted Module ID: "volume"
     - Puzzle Panel: (assign in Step 6)
     - Puzzle Controller: (assign in Step 6)
     - Installed Indicator: (assign a child light object)
   - **Box Collider** (for interactor detection, must not be a trigger for the socket)
4. Create a child **Point Light** → color green, intensity 0.5 → name **SuccessLight**
5. Create a child **Point Light** → color red, intensity 1 → name **FailureLight** → enabled by default
6. Add **Puzzle Controller** (script) to TicketMachine:
   - Target Min: 0.6
   - Target Max: 0.8
   - Success Light: drag SuccessLight
   - Failure Light: drag FailureLight
   - Doors To Open: (assign the door between Area 2 and Area 3)

### Step 5: Create the Module Pickup
1. Create a small Cube or Sphere → name **VolumeModulePickup** → Position (-8, 0.5, 2)
2. Add components:
   - **Module Pickup** (script)
   - **Box Collider** (Is Trigger = true, or non-trigger — PlayerInteractor uses OverlapSphere)
3. Create the ModuleData ScriptableObject:
   - Right-click in Project window → **Create → TGC → UI Module**
   - Name it **VolumeModule**
   - Display Name: "Volume Module"
   - Module ID: "volume"
   - Description: "Controls audio volume levels."
4. Assign VolumeModule to the Module Pickup's Module Data field

### Step 6: Create the Puzzle UI Panel (Slider)
1. Create UI Canvas (if none exists):
   - Right-click Hierarchy → **UI → Canvas**
   - Add **Canvas Scaler** (UI Scale Mode = Scale With Screen Size, Reference 1920x1080)
2. Create the Puzzle Panel:
   - Right-click Canvas → **UI → Panel** → name **PuzzlePanel**
   - Set active = false (unchecked) by default
   - Add **Image** component, semi-transparent dark background
   - Anchor: center, Size (400, 200)
3. Add a **Slider** as child of PuzzlePanel:
   - Right-click PuzzlePanel → **UI → Slider**
   - Position: (0, -20, 0)
   - Min Value: 0, Max Value: 1, Whole Numbers: off
   - Set default value: 0
4. Add a **Text** (or UI Text) as child:
   - Text: "Adjust Volume Level"
   - Position: (0, 50, 0)
5. Add a **Close Button**:
   - Right-click PuzzlePanel → **UI → Button** → name **CloseButton**
   - Text: "Close"
   - Position: (150, -70, 0)
   - On Click event: drag TicketMachine → call **PuzzleController.ClosePanel**
6. Assign references:
   - On **ModuleSocket** (on TicketMachine): set Puzzle Panel = PuzzlePanel
   - On **ModuleSocket**: set Puzzle Controller = PuzzleController component
   - On **PuzzleController**: set Puzzle Slider = the Slider
   - On **PuzzleController**: call puzzleController.SetPuzzlePanel(PuzzlePanel) — or assign in code
     (Note: PuzzleController auto-detects slider via the Inspector assignment)

### Step 7: Create Doors Between Areas
1. Create Cube → name **DoorArea2to3** → Scale (0.4, 4, 3) → Position (0, 2, 0)
2. Add **Door Controller** (script):
   - Slide Open: true
   - Slide Distance: 4
   - Open Speed: 2
   - Locked At Start: false (it's closed but not locked — puzzle opens it)
3. Assign this door to the PuzzleController's **Doors To Open** array on the TicketMachine

### Step 8: Create the HUD
1. Create Canvas (or use existing from Step 6)
2. Add **HUD Controller** (script) to a new empty GameObject named **HUD**
3. Create UI elements:
   - **ObjectiveText** (UI Text) — top-center, font size 24
   - **PromptText** (UI Text) — bottom-center, font size 18
     - Parent it under a **PromptContainer** GameObject
   - **NotificationText** (UI Text) — center, font size 20
4. Assign references on HUDController:
   - Objective Text: drag ObjectiveText
   - Prompt Text: drag PromptText
   - Prompt Container: drag PromptContainer
5. Assign HUD reference to:
   - **QuestManager** → HUD
   - Also assign HUD to **PlayerInteractor → Prompt UI** (the PromptContainer)

### Step 9: Create the Quest Manager
1. Create Empty GameObject → name **QuestManager**
2. Add **Quest Manager** (script)
3. Set objectives (default array is pre-filled)
4. Assign HUD reference

### Step 10: Create the Game Manager
1. Create Empty GameObject → name **GameManager**
2. Add **Game Manager** (script)
3. Assign QuestManager reference
4. Create an Ending Panel:
   - UI Panel under Canvas → name **EndingPanel** → set active = false
   - Add Text: "YOU MADE IT OUT"
5. Assign EndingPanel to GameManager

### Step 11: Create the Exit Trigger (Area 4)
1. Create an empty GameObject → name **ExitTrigger** → Position (18, 1, 0)
2. Add **Box Collider** → Is Trigger = true → Size (2, 3, 5)
3. Add **Exit Trigger** (script)
4. Assign GameManager reference

### Step 12: Add Player Spawn
1. Position the Player GameObject at (-18, 1, 0) — west side of the scene (Area 1)

## How to Test
1. Open SampleScene in Unity
2. Press **Play**
3. Move with **WASD**
4. Walk to the VolumeModulePickup (small object near Area 1/2 boundary)
5. Press **E** to pick it up
6. Walk to the TicketMachine (center)
7. Press **E** to install the module
8. The Puzzle Panel appears with a slider
9. Drag the slider to 0.6-0.8 range
10. The success light turns green, the door slides open
11. Walk through to Area 3
12. Continue to Area 4, reach the ExitTrigger → ending screen appears

## Input Controls
| Action | Key |
|--------|-----|
| Move | WASD / Arrow Keys |
| Interact | E |
| Close UI | Close Button or Escape (add later) |

## Notes
- The PlayerInteractor uses Physics.OverlapSphere — no trigger collider strictly required on interactables, but each interactable needs some Collider (Box, Sphere, etc.) so the OverlapSphere can detect it.
- The Input System Interact action has a **Hold** interaction by default. If you want a tap instead, change the interaction in the Input Actions editor to **Press**.
- The PuzzleController checks the slider value every frame when the module is installed and the puzzle is not yet solved.
