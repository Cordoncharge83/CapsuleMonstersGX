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