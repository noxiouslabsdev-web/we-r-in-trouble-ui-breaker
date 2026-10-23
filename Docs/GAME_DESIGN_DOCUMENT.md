# Game Design Document - WE R IN TROUBLE: UI BREAKER

## Core Concept
A 3D puzzle adventure in a malfunctioning futuristic subway station. The player
finds and installs **UI Modules** (Volume, Brightness, Settings) into broken
machines, manipulates interface controls (sliders, toggles), and observes
cause-and-effect results to solve puzzles and unlock routes.

## One Zone: The Broken Station
A single continuous scene with four connected areas.

## Top-Down Map

`
                    NORTH
         +------------------------------------------+
         |              AREA 4                      |
         |        PLATFORM & EXIT                  |
         |   [Exit Gate]  [Train Front]            |
         |   [Platform Bench]                      |
         |          |                              |
         |     (locked gate)                       |
         |          |                              |
    W    +----------+--------+                     E
    E    |    AREA 3         |                     A
    S    |  CONTROL CORNER   |                     S
    T    |  [Control Panel]  |                     T
    |    |  [Locked Gate]    |                     |
    |    |       |           |                     |
    |    +-------+-----------+                     |
    |            |                                 |
    |    +-------+-----------+                     |
    |    |    AREA 2         |                     |
    |    |  TICKET COUNTER   |                     |
    |    |  [Ticket Machine] |                     |
    |    |  [Module Socket]  |                     |
    |    |  [Slider Control] |                     |
    |    |       |           |                     |
    |    +-------+-----------+                     |
    |            |                                 |
    |    +-------+-----------+                     |
    |    |    AREA 1         |                     |
    |    |  ARRIVAL HALL     |                     |
    |    |  [Player Spawn]   |                     |
    |    |  [Signage]        |                     |
    |    |  [Locked Exit →]  |                     |
    |    +-------------------+                     |
    |                                            |
         +------------------------------------------+
                    SOUTH
`

## Flow: West → East
Player spawns in Area 1 (Arrival Hall, west side) and progresses eastward
through Area 2 (Ticket Counter), Area 3 (Control Corner), to Area 4
(Platform & Exit).

## Approximate Dimensions (Greybox)
- Total area: ~40m x 15m corridor layout
- Each area: ~10m x 12m
- Ceiling height: ~4m
- Corridor connections: ~3m wide openings

## Area Details

### Area 1 — Arrival Hall (West)
- Player spawn point
- Station signage ("STATION 7 — SECTOR BREAKDOWN")
- Environmental storytelling: flickering lights, scattered debris
- Locked exit door visible (foreshadows the goal)
- Tutorial-grade interactable: a simple door or button

### Area 2 — Ticket Counter (Center)
- **Main showcase puzzle**
- Ticket vending machine (malfunctioning)
- UI Module pickup: **Volume Module** (lying on a bench or floor)
- Socket on the ticket machine that accepts the Volume Module
- Slider control appears when module is installed
- Puzzle: Set the slider to the correct "volume level" to fix the machine
- Success: dispenses a ticket / opens passage to Area 3
- Failure feedback: machine sparks / red light

### Area 3 — Control Corner
- Electrical control panel
- Locked automated gate to Area 4
- Reuses Module+Socket+Slider architecture
- Power allocation puzzle: redistribute power between two sliders
- Success: gate opens

### Area 4 — Platform & Exit (East)
- Subway platform with track
- Simplified train front (low-poly)
- Final challenge: reuse slider mechanic to align train or open doors
- Exit gate → ending state (simple "You made it out" screen)

## Interaction Design — Detachable UI Controls
The defining mechanic: the player can **detach actual buttons and controls from existing UI panels** in the game, carry them through the 3D world, and install them into compatible targets.

1. Find an existing UI panel containing a control (e.g. a Plus Button on a Source Panel)
2. Press E to detach the control — the source panel shows an empty slot
3. The control is carried by the player (shown as a small sphere above the player)
4. Walk to the target machine and press E to install the control
5. The target shows the installed control and activates (puzzle panel appears)
6. Use the slider to solve the puzzle → door opens
7. The player can only carry ONE control at a time

## Controls (Keyboard & Mouse)
| Action | Key |
|--------|-----|
| Move | WASD / Arrow Keys |
| Interact (detach/install/use) | E |
| UI navigation | Mouse (click/drag sliders) |
| Close UI panel | Close Button |

## UI Control System
- **UIControlData** (ScriptableObject): Describes a control (name, ID, color)
- **UIControlSlot**: On source panels — holds a detachable control, shows empty when removed
- **UIControlTarget**: On machines — accepts a compatible control, activates when installed
- **CarriedControl**: On player — tracks the single control being carried
- **CameraController**: Third-person follow + fixed camera switching
- **CameraZone**: Trigger volume that switches camera mode
## Controls (Keyboard & Mouse)
| Action | Key |
|--------|-----|
| Move | WASD / Arrow Keys |
| Interact | E |
| UI navigation | Mouse (click/drag sliders) |
| Close UI panel | Escape |

## Visual Direction
- Low-poly / abstract / stylized sci-fi
- Dark metallic surfaces with neon accent lighting
- Flickering/broken lights for atmosphere
- Color cues: green = working, red = broken, yellow = warning

## Camera
- **Third-person camera** during normal exploration — camera follows behind the player
- **Fixed camera at puzzle areas** — switches via CameraZone trigger volumes
- CameraZone triggers are placed at Ticket Machine, Control Panel, and Train Front areas
- Each CameraZone has a child camera anchor defining the fixed position/rotation
- Camera transitions smoothly between third-person and fixed modes
- Designers can add/remove CameraZones to control which areas use fixed cameras
- Movement remains camera-relative in both modes (WASD works in both)
## Implementation Status (Corrected Design — Verified 2026-10-23)

### Camera System
- Third-person follow camera during exploration (CameraController on MainCamera)
- Fixed camera at 3 puzzle areas via CameraZone triggers (TicketMachine, ControlPanel, Train)
- Smooth transitions between modes
- Camera anchors are child GameObjects of each CameraZone

### Detachable UI Control System (Core Mechanic)
**3 source panels with detachable controls:**
- SourcePanel (Area 1): Contains Plus Button (green) → detach for Ticket Machine
- VolumeSourcePanel (Area 2): Contains Volume Button (blue) → detach for Control Panel
- SettingsSourcePanel (Area 3): Contains Settings Button (purple) → detach for Train Front

**3 targets that accept controls:**
- TicketMachine (Area 2): Accepts "plus" control → enables slider puzzle → opens Door 2→3
- ControlPanel (Area 3): Accepts "volume" control → enables slider puzzle → opens Door 3→4
- TrainFront (Area 4): Accepts "settings" control → enables slider puzzle → opens Train Door

**Carry system:** Player can carry one control at a time (CarriedControl component). CarryVisual sphere appears above player when carrying.

### Complete Puzzle Flow (Corrected)
1. Spawn in Arrival Hall → third-person camera follows
2. Find SourcePanel on wall → press E to detach Plus Button → source shows empty slot
3. Walk to TicketMachine → press E to install Plus Button → target shows button → slider puzzle appears
4. Drag slider to 0.6-0.8 → success light green, door opens → camera returns to third-person
5. Walk through → find VolumeSourcePanel → detach Volume Button
6. Install in ControlPanel → solve slider (0.3-0.5) → door opens
7. Walk through → find SettingsSourcePanel → detach Settings Button
8. Install in TrainFront → solve slider (0.4-0.6) → train door opens
9. Walk through train door → reach ExitTrigger → ending screen

### ScriptableObject Assets (UIControlData)
- PlusButton.asset (controlID: "plus", color: green)
- VolumeButton.asset (controlID: "volume", color: blue)
- SettingsButton.asset (controlID: "settings", color: purple)

### Scripts (Corrected)
| Script | Purpose |
|--------|---------|
| CameraController | Third-person follow + fixed camera switching |
| CameraZone | Trigger volume for camera mode switching |
| PlayerController | Third-person movement (camera-relative, rotates to face direction) |
| PlayerInteractor | Detects interactables, supports both new UIControl and old Module systems |
| CarriedControl | Tracks the single control being carried (replaces ModuleInventory) |
| UIControlData | ScriptableObject for detachable controls (replaces ModuleData) |
| UIControlSlot | Source panel slot — detachable controls (replaces ModulePickup) |
| UIControlTarget | Machine target — accepts controls (replaces ModuleSocket) |
| PuzzleController | Slider puzzle logic (unchanged) |
| DoorController | Door opening (unchanged) |
| HUDController | HUD display (unchanged) |
| QuestManager | Objective tracking (unchanged) |
| GameManager | Global state/ending (unchanged) |
| ExitTrigger | Exit zone (unchanged) |

### Build Verified
- Windows build: Builds/Windows/TGC_UIBreaker.exe (161 MB)
- Scene data: level0 (119 KB)
- 0 compilation errors, 0 warnings
- Return code: 0