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
- Movement currently unrestricted (no range limit yet)

### 4. Turn System
- Implemented TurnManager
- Alternates between:
  - Player turn
  - Enemy turn
- Prevents multiple actions per turn

### 5. Enemy AI
- Basic AI:
  - Moves 1 tile toward player
  - Chooses direction based on distance

### 6. Combat System
- Units can attack when adjacent
- Player attacks by clicking enemy
- Enemy attacks automatically if adjacent
- Damage system:
  - Damage = attack power
- Units are destroyed when HP ≤ 0

---

## Current State

The game has a complete basic loop:
- Player moves or attacks
- Turn ends
- Enemy moves or attacks
- Turn returns to player

This is the first playable version of the tactical system.

---

## Known Limitations

- No movement range (teleport movement)
- No attack range beyond adjacency
- No pathfinding
- No animations or feedback
- No UI
- No fusion system yet

---

## Next Steps (Session 2)

- Add movement range (MOV stat)
- Add attack range (RNG stat)
- Improve enemy behavior
- Add visual feedback (tile highlights, selection)

---

## Notes

Focus is on building a solid, testable tactical system before adding GX-specific mechanics like fusion.