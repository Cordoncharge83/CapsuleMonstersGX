# Development Log

## Session 1 — Core Prototype 

### Objective
Build a minimal playable tactical prototype:
- Grid-based movement
- Turn system
- Basic enemy AI
- Simple combat

---

## Systems Implemented

### 1. Grid System
- Created a Tilemap-based grid (6x6 prototype)
- Implemented click detection using mouse input
- Converted screen position → world → grid cell

### 2. Unit System
- Created a generic Unit component
- Units store:
  - Cell position
  - HP
  - Attack power
- Units snap correctly to grid cells

### 3. Player Movement
- Click on tile → player moves instantly
- Movement initially unrestricted (teleport-style)

### 4. Turn System
- Implemented TurnManager
- Alternates between:
  - Player turn
  - Enemy turn
- Prevents multiple actions per turn

### 5. Enemy AI
- Basic AI:
  - Moves toward player
  - Chooses direction based on distance

### 6. Combat System
- Units can attack when adjacent
- Player attacks by clicking enemy
- Enemy attacks automatically if adjacent
- Damage system:
  - Damage = attack power
- Units are destroyed when HP ≤ 0

---

## Session 2 — Tactical System Expansion

### Objective
Transform the prototype into a true tactical system:
- Movement and attack ranges
- Visual feedback (tile highlighting)
- Multi-unit support
- Improved enemy AI

---

## Systems Implemented

### 1. Movement & Attack Range
- Added **MOV (movement range)** using Manhattan distance
- Added **RNG (attack range)** for melee and ranged combat
- Movement restricted to valid tiles within range
- Attacks allowed only within attack range

### 2. Tile Highlighting
- Added a dedicated **Highlight Tilemap**
- Movement tiles (green) show valid movement range
- Attack tiles (red/orange) show valid attack range
- Highlights update based on selected unit

### 3. Unit Selection System
- Player must select a unit before acting
- Clicking a unit:
  - Selects it
  - Displays movement and attack range
- Clicking another unit:
  - Instantly switches selection
- Clicking invalid tile:
  - Cancels selection

### 4. Multi-Unit System
- Replaced single unit references with:
  - `List<Unit> playerUnits`
  - `List<Unit> enemyUnits`
- Player can control multiple units
- Can attack any enemy unit in range

### 5. Occupancy System
- Prevented units from moving onto occupied tiles
- Applies to both player and enemy units
- Ensures valid tactical positioning

### 6. Enemy AI Improvements
- AI now supports multiple enemies
- Each enemy:
  - Targets the **closest player unit**
  - Attacks if within range
  - Moves toward target using MOV
- AI respects:
  - Attack range (RNG)
  - Movement range (MOV)
  - Occupied tiles

---

## Current State

The game now supports **multi-unit tactical gameplay**:
This is a **complete tactical core system**.

---

## Known Limitations

- No pathfinding (straight-line movement only)
- Highlight visuals overlap (movement vs attack)
- Enemy actions happen instantly (no animation/sequence)
- No UI (health bars, action menus, etc.)
- No terrain effects or obstacles
- No capsule / fusion systems yet

---

## Notes

The project has successfully transitioned from a prototype to a **functional tactical engine**.

Next focus:
- Capsule-style deployment phase
- Pre-battle setup
- Foundation for fusion mechanics

---

## Session 3 — Capsule System (Core Gameplay Loop)

### Objective
Introduce capsule-based gameplay by adding a placement phase before battle:
- Units are spawned dynamically instead of pre-placed
- Player chooses where to deploy units before combat begins

---

## Systems Implemented

### 1. Capsule System (Basic)
- Introduced `CapsuleManager`
- Capsules act as a source of unit prefabs
- Each capsule spawns a unit instantly on placement
- Units are instantiated dynamically and added to the game

---

### 2. Placement Phase
- Added a new game phase:
  - `Placement`
  - `Battle`
- During Placement:
  - Player clicks valid tiles to place units
  - Units are spawned from capsule list
- After all capsules are placed:
  - Game transitions automatically to Battle phase

---

### 3. Placement Rules
- Units can only be placed:
  - On valid grid tiles
  - On unoccupied tiles
  - Within a defined player zone (bottom half of grid)
- Prevents invalid or unfair starting positions

---

### 4. Dynamic Unit Registration
- Spawned units are:
  - Added to `playerUnits` list (GridManager)
  - Registered to `EnemyAI` as valid targets
- Ensures full integration with combat and AI systems

---

### 5. Multi-Capsule Support
- CapsuleManager supports multiple unit prefabs
- Player can place several units before battle
- Each placement consumes one capsule

---

## Current State

The game now supports a complete gameplay loop:

This marks the transition from a prototype to a **core gameplay experience**.

---

## Known Limitations

- Capsules are not visual objects (instant spawn only)
- No capsule selection UI (predefined order)
- No placement preview or feedback UI
- No animations for spawning or transitions
- Enemy units are still pre-placed (no enemy capsules yet)
- No advanced AI positioning logic
- No element or synergy systems yet

---

## Notes

This system establishes the foundation for:
- Capsule types (standard, rare, fusion)
- Pre-battle strategy
- Future fusion mechanics

The game now has a clear identity aligned with Capsule Monsters gameplay.

---

## Session 4 — Element System (Combat Depth)

### Objective
Introduce elemental interactions to make combat more strategic:
- Units gain elemental identity
- Damage varies depending on matchups
- Adds decision-making to targeting and positioning

---

## Systems Implemented

### 1. Element Type System
- Created `ElementType` enum:
  - Fire, Water, Wind, Earth, Light, Dark
- Each unit now has an assigned element
- Elements are configured via Inspector (prefabs and enemies)

---

### 2. Element Effectiveness Logic
- Implemented `ElementSystem`
- Defines damage multipliers based on matchups:
Fire > Wind
Wind > Earth
Earth > Water
Water > Fire

Light > Dark
Dark > Light
- Multipliers:
  - Strong → 1.5x
  - Weak → 0.5x
  - Neutral → 1.0x
  
 
---

## Session 5 — Enemy Setup & Match Structure

### Objective
Move enemy setup from static scene placement to a dynamic, data-driven system:
- Enemies are spawned from prefabs
- Battle setups are configurable and repeatable
- Scene no longer contains pre-placed combat units

---

## Systems Implemented

### 1. Enemy Prefabs
- Converted enemy units into reusable prefabs
- Each prefab defines:
  - Stats (HP, ATK, MOV, RNG)
  - Element type
  - Team (Enemy)

---

### 2. Enemy Spawn Data
- Created `EnemySpawnData` structure:
  - Enemy prefab
  - Spawn cell position
- Allows defining battle layouts directly in Inspector

---

### 3. EnemySetupManager
- Implemented a manager to handle enemy spawning
- On scene start:
  - Instantiates enemy prefabs
  - Snaps them to predefined grid cells
  - Registers them in:
    - `GridManager`
    - `EnemyAI`

---

### 4. Dynamic Unit Registration
- All units (player + enemy) are now:
  - Spawned dynamically
  - Registered at runtime
- Removed reliance on scene-placed units

---

### 5. Clean Scene Architecture
- Scene now contains only managers and environment:
  - No pre-placed player or enemy units
- Ensures:
  - Consistent match initialization
  - Easier level design and testing

---

## Current State

The game now supports a complete structured flow:
Scene start
→ EnemySetupManager spawns enemies
→ Player enters placement phase
→ Player places units via capsules
→ Battle phase begins
→ Tactical combat with multiple units and elements


This system allows battles to be fully defined and reproduced.

---

## Known Limitations

- Enemy placement is fixed (no variation/randomness)
- No enemy capsule system yet
- No pre-battle UI or preview
- AI does not consider element advantage
- No level progression or match selection system

---

## Notes

The game now has a complete and scalable battle setup pipeline.

This prepares the project for:
- Level design
- Enemy variety and encounter design
- Future systems like fusion and abilities

## Session 6 — Fusion System (Core Mechanic)

### Objective
Introduce fusion as a core gameplay mechanic:
- Combine two units into a stronger unit
- Integrate fusion into the tactical flow
- Ensure compatibility with existing systems (Grid, AI, turns)

---

## Systems Implemented

### 1. Fusion Recipe System
- Created `FusionRecipe` structure:
  - Defines:
    - Unit A (ID)
    - Unit B (ID)
    - Resulting unit prefab
- Enables data-driven fusion combinations
- Independent from unit names or scene objects

---

### 2. Unit Identity System
- Added `unitId` to `Unit`
- Provides a stable identifier for gameplay logic
- Avoids reliance on prefab names (`Clone` issues)

---

### 3. Fusion Manager
- Implemented `FusionManager`
- Handles:
  - Fusion validation
  - Recipe matching
  - Unit replacement (destroy + instantiate)
- Ensures correct:
  - Positioning (grid cell preserved)
  - Tilemap assignment at runtime

---

### 4. Fusion Integration
- Fusion replaces both units with the fused unit
- Integrated with:
  - `GridManager` (add/remove units)
  - `EnemyAI` (target tracking updates)
- Prevents ghost units or invalid references

---

### 5. Fusion Mode (Player Interaction)
- Added fusion mode via input (`F` key)
- Flow:
  - Select unit A
  - Press F → enter fusion mode
  - Select unit B → attempt fusion
- Prevents accidental fusion during normal selection

---

### 6. Fusion Constraints
- Fusion requires units to be **adjacent** (Manhattan distance = 1)
- Ensures tactical positioning is required

---

### 7. Fusion Target Highlighting
- Added visual feedback for valid fusion targets
- When in fusion mode:
  - Movement/attack highlights are cleared
  - Only valid fusion targets are highlighted
- Improves clarity and usability

---

### 8. Fusion Mode Control
- Clicking invalid tile cancels fusion mode
- Prevents unintended movement during fusion
- Restores normal highlights after cancel

---

## Current State

Fusion is now a fully functional gameplay mechanic:

Select unit
→ Enter fusion mode
→ Select adjacent valid unit
→ Fusion occurs
→ Turn ends

This integrates seamlessly into the tactical loop.

---

## Known Limitations

- Fusion triggered via keyboard (no UI yet)
- No fusion animations or visual effects
- No feedback when no fusion is available (only logs)
- No cost system (fusion is free aside from turn usage)
- No fusion preview (result not shown before action)

---

## Notes

This system establishes the foundation for:
- GX-style fusion mechanics
- Fusion capsules (future feature)
- Advanced abilities tied to fusion units

Fusion is now part of the core combat identity of the game.