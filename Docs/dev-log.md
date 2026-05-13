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


## Session 7 — UI Improvements & Enemy Turn Pacing

### Overview
This phase focused on improving **player feedback, UI clarity, and game pacing**. The goal was to move from a purely functional prototype to a more readable and responsive tactical experience.

---

### Action System Improvements

#### Action Panel (Move / Attack / Fuse / Cancel)
- Introduced a contextual **Action Panel** that appears near the selected unit
- Replaced keyboard inputs (e.g. `F`) with UI-based actions
- Actions are now explicitly chosen before execution

#### Dynamic Action Availability
- **Attack** button only appears if an enemy is in range
- **Fuse** button only appears if a valid fusion target exists
- Prevents invalid or confusing actions

#### Cancel System (Right Click)
- Added universal cancel input:
  - If in action mode → returns to action selection
  - If in selection → deselects unit
  - If enemy info is open → hides it
- Introduces layered interaction (selection → action → resolution)

#### Action Mode Cleanup
- Refactored input logic into clear states:
  - Selection
  - Action Mode (Move / Attack / Fuse)
  - Resolution
- Eliminated ambiguous click behavior

---

### UI Improvements

#### Unit Info Panels
- Added **separate panels for player and enemy units**
- Player info appears on the left
- Enemy info appears on the right
- Enemy info can be accessed even without selecting a player unit

#### Portrait System
- Added portrait images to units
- Introduced element-colored frames and panel tinting
- Improved visual identity of units

#### Action Panel Behavior
- Panel appears near selected unit (dynamic positioning)
- Automatically hides when an action is chosen
- Uses layout system (Vertical Layout Group + Content Size Fitter)
  → no empty gaps when buttons are hidden
- Clamped to screen bounds to prevent going off-screen

#### Turn Indicator
- Added turn indicator UI
- Displays:
  - Placement Phase
  - Player Turn
  - Enemy Turn
- Appears briefly then disappears (non-intrusive)

---

### Enemy Turn Pacing

#### Coroutine-Based Execution
- Replaced instant enemy execution with coroutine system
- Enemy turn now unfolds over time:
  - Initial delay before actions
  - Delay between each enemy action

#### Result
- Enemy actions are now readable and understandable
- Turn indicator is visible and meaningful
- Overall pacing feels more natural and less abrupt

---

### Current State

The game now features:

- Structured action system with explicit intent
- Responsive and contextual UI
- Readable enemy behavior through pacing
- Improved overall game feel

---

### Next Steps

- Add visual feedback:
  - Hit effects (damage feedback)
  - Movement smoothing (instead of teleport)
  - Fusion effects
- Improve enemy AI (decision-making)
- Introduce Action Points (AP) system (optional)

## Session 8 — Combat Feedback & Tactical Depth

### Overview
This session focused on improving combat feel, completing the battle loop, and making unit behavior more tactical.

---

### Systems Implemented

#### 1. Damage Feedback
- Added hit flash when units take damage
- Added floating damage numbers
- Ensured feedback plays before defeated units disappear

#### 2. Defense Stat
- Added `DEF` stat to units
- Updated damage calculation:
  - Damage now considers ATK, DEF, and elemental multiplier
  - Minimum damage is always 1

#### 3. Win / Loss Conditions
- Added `OnUnitDefeated` event to `Unit`
- `GridManager` now removes defeated units and checks battle outcome
- Player wins when all enemies are defeated
- Player loses when all player units are defeated
- Added `battleEnded` logic to stop input and turn flow

#### 4. Grid Pattern System
- Added `GridPatternType`
- Added reusable `GridPatternUtility`
- Implemented:
  - Diamond
  - Cross
  - Diagonal
- Movement, attack validation, and highlights now use patterns

#### 5. Smooth Movement
- Replaced instant movement with coroutine-based sliding
- Player turn waits for movement to finish
- Fixed enemy movement desync by waiting for enemy movement animations

#### 6. Attack Animation
- Added lunge animation for attacks
- Player and enemy attacks now use animated attack sequences

---

### Current State

The game now has:
- A complete battle loop
- Clear damage feedback
- Win/loss conditions
- Pattern-based tactical movement and attacks
- Smooth movement and basic attack animation

---

### Next Steps

- Add ranged/projectile attack feedback
- Improve UI to show DEF and pattern types
- Add fusion visual effects
- Introduce AP system later

## Session 9 — AP System, Unit States & Battle Result UI

### Overview
This session focused on turning the battle system into a more complete tactical loop by adding Action Points, unit action states, post-move decisions, fusion costs, and a proper victory/defeat screen.

---

### Systems Implemented

#### 1. Action Points System
- Added player AP per turn
- Player AP resets at the start of each player turn
- AP is displayed through the AP UI
- Actions consume AP:
  - Move costs AP
  - Direct attack costs AP
  - Fusion costs AP based on the fused unit summon cost
- AP can be refunded when undoing a move

#### 2. Unit Action States
- Added unit turn states:
  - Ready
  - Moved
  - Acted
- Ready units can move, attack, or fuse
- Moved units can attack for free or finish
- Acted units cannot act again

#### 3. Post-Move Action Flow
- After moving, the unit does not immediately end its action if an enemy is in range
- Player can:
  - Attack after moving
  - Finish the unit’s action
  - Cancel/undo the move
- If no enemy is in range after moving, the unit becomes acted automatically

#### 4. Undo Move System
- Added post-move cancel
- If the player cancels after moving:
  - Unit returns to original cell
  - Unit state resets to Ready
  - AP is refunded
- Prevents accidental movement from feeling too punishing

#### 5. Fusion Cost Integration
- Fusion now checks the summon cost of the resulting fused unit
- Fusion only succeeds if the player has enough AP
- After fusion:
  - Original units are removed
  - Fused unit is spawned
  - AP is spent
  - Fused unit becomes Acted

#### 6. Battle Result UI
- Added Victory / Defeat screen
- Battle result appears when all enemies or all player units are defeated
- End battle panel is placed above all UI
- Panel blocks interaction with buttons behind it
- Battle end state prevents further turn progression

#### 7. UI Click Protection
- Added protection against clicking gameplay tiles/units through UI
- Grid input now ignores clicks when the pointer is over UI
- Prevents action panel buttons from also triggering unit selection underneath

---

### Current State

The game now has a much stronger tactical structure:

- Player turns are limited by AP
- Units have clear action states
- Movement can be undone before committing
- Attacks after movement feel intentional
- Fusion is tied into the AP economy
- Battles now end with a visible result screen

The prototype is now much closer to a complete playable tactical match.

---

### Known Limitations

- Input can still be spammed during damage preview unless combat input locking is added
- Victory/Defeat screen has no Retry or Main Menu buttons yet
- Fusion still has no visual effect
- AP UI is functional but not visually polished
- Enemy AI does not yet consider AP, elements, or advanced positioning

---

### Next Steps

- Add input lock during attack/damage preview sequences
- Add fusion visual feedback
- Add Retry / Main Menu buttons to battle result screen
- Improve enemy AI decision-making
- Later: full UI redesign pass

## Session 10 — AI Intelligence, Combat Feel & AP UI Upgrade

### Overview
This session focused on improving **enemy AI decision-making**, enhancing **combat feedback**, and upgrading the **AP UI** to better match the original game.

The goal was to move from a functional system to a more **tactical, readable, and satisfying gameplay experience**.

---

### Systems Implemented

#### 1. Enemy AI — Target Selection Upgrade
- Replaced closest-target logic with **score-based targeting**
- Enemies now evaluate:
  - Distance to target
  - Remaining HP
  - Kill potential
- Prioritizes finishing low-health units and high-value targets

---

#### 2. Enemy AI — Best Target in Range
- Enemies now select the **best target among those in attack range**
- Prevents:
  - Attacking suboptimal targets
  - Ignoring easy kills
- After movement, enemies re-evaluate targets before attacking

---

#### 3. Enemy AI — Move → Attack Flow
- Enemies can now:
  - Move into position
  - Then attack in the same turn
- Added delay between move and attack for readability
- Removed limitation where enemies could only move OR attack

---

#### 4. Enemy AI — Smart Positioning
- Implemented **tile scoring system** for movement
- Movement now considers:
  - Distance to target
  - Nearby player units
  - Threatened tiles
- Enemies avoid:
  - Being surrounded
  - Ending turns in dangerous positions

---

#### 5. Enemy AI — Fallback Movement
- If no valid attack position is reachable:
  - Enemy moves toward target
- Prevents idle or stuck behavior

---

#### 6. Combat Feedback — Hit Pause
- Added **hit pause (hit stop)** on impact
- Short freeze (~0.06s) when damage is applied
- Improves:
  - Impact
  - Readability
  - Game feel

---

#### 7. AP UI — Visual Bar System
- Replaced simple AP text with a **visual AP bar**
- Inspired by original *Capsule Monster Coliseum*

New display format:

AP   2 (+3)
[██████----]

Features:
- Bar represents **max AP**
- Fill represents **current AP**
- Gain display (`+maxAP`) added as placeholder
- Smooth fill animation implemented

---

### Current State

The game now features:

- Tactical enemy AI with:
  - Target prioritization
  - Smart positioning
  - Move-then-attack behavior
- Improved combat feel through hit pause
- Clear and readable AP UI

The gameplay loop is now:

Strategic → Responsive → Readable → Satisfying

---

### Known Limitations

- AI does not yet consider:
  - Elemental advantage
  - Advanced positioning (no pathfinding)
- Movement can still be blocked
- AP gain system is placeholder
- AP UI lacks advanced feedback (color/pulse)
- No sound effects yet

---

### Next Steps

- Further refine AI behavior
- Add sound effects for actions
- Improve UI consistency
- Add unit state indicators (Ready / Moved / Acted)

## Session 11 — UI Rework, Audio Feedback & Enemy Intent Preview

### Overview
This session focused on improving the game's overall readability, presentation quality, and tactical clarity.

The goal was to push the prototype further from a functional tactical engine toward a more polished and readable game experience inspired by the original *Capsule Monster Coliseum*.

---

## Systems Implemented

### 1. Unit Info Panel Redesign
- Completely redesigned the Unit Info UI panels
- Moved away from generic text-based information display
- Introduced a structured layout inspired by the original game

New panel includes:
- Portrait display
- Unit name
- HP
- ATK
- DEF
- AP cost
- Movement range
- Attack range
- Element icon
- Movement pattern icon
- Attack pattern icon

---

### 2. Element Icon System
- Created dedicated icons for all six elements:
  - Fire
  - Water
  - Wind
  - Earth
  - Light
  - Dark
- Replaced old element-based panel tinting
- Element identity is now represented through icons instead of full panel recoloring
- Improved UI cohesion and readability

---

### 3. Pattern Icon System
- Added movement/attack pattern icons:
  - Cross
  - Diagonal
  - Diamond
- Unit panels now visually communicate movement and attack behavior
- Reinforces tactical readability without requiring text explanations

---

### 4. UI Visual Rework
- Introduced a unified UI style and color palette
- Reworked panel visuals to better match the GX / Capsule Monster aesthetic
- Improved:
  - Borders
  - Framing
  - Layout consistency
  - Information hierarchy
- Reduced visual clutter caused by element-colored panels

---

### 5. Selection Ring Feedback
- Added visual selection rings beneath units
- Selected units are now clearly readable on the battlefield
- Added acted-state readability:
  - Acted units become slightly dimmed
- Prepares the game for future tactical readability improvements

---

### 6. UI Audio Feedback
- Added UI sound effects:
  - Confirm / accept
  - Cancel / back
  - Menu navigation
- Connected sounds to:
  - Action buttons
  - UI interactions
- Prevented cancel sound from triggering on unrelated right-clicks

---

### 7. Combat Impact Sound
- Added hit impact sound effect during combat
- Sound triggers when damage is applied
- Integrated into attack flow for better combat feedback

---

### 8. AP Number Animation
- Upgraded AP UI behavior:
  - AP values now animate gradually instead of changing instantly
- Example:
  - `3 → 2 → 1 → 0`
- AP bar fill animation and AP number animation are now synchronized
- Greatly improves readability of AP consumption

---

### 9. Enemy Intent Preview System
- Added enemy tactical previews inspired by the original *Capsule Monster Coliseum*

Enemy turn flow now includes:
- Enemy selection ring
- Movement range preview before moving
- Attack range preview before attacking

This significantly improves:
- Tactical readability
- Understanding of enemy behavior
- Enemy turn pacing

---

### 10. Enemy AI Refactor — Pattern-Based Movement
- Refactored enemy movement logic to fully support:
  - Cross movement
  - Diagonal movement
  - Diamond movement
- Removed old direction-based fallback movement logic
- Enemy AI now uses:
  - `GridPatternUtility`
  - Shared movement validation
  - Shared movement execution flow

Movement previews now correctly reflect actual enemy movement behavior.

---

## Current State

The game now features:
- Stronger UI readability
- Improved tactical clarity
- Better audiovisual feedback
- More polished enemy turn presentation
- Pattern-consistent enemy AI behavior

The prototype now feels significantly closer to a complete tactical game experience.

---

## Known Limitations

- No pathfinding system yet
- No advanced attack VFX
- No death animations
- No camera feedback (shake/zoom)
- Sound library remains minimal
- Enemy AI still does not consider elemental advantage

---

## Next Steps

- Continue UI polish and consistency pass
- Add additional combat feedback
- Improve turn transition presentation
- Add advanced AI behaviors later
- Begin preparing for larger-scale level structure and progression

## Session 12 — Arena Design, Terrain System & Enemy AP Economy

### Overview
This session focused on transforming the combat space from a simple prototype grid into a real tactical arena inspired by the Duel Academy battle stage from *Yu-Gi-Oh! GX*.

The goal was to:
- Introduce meaningful terrain
- Improve battlefield readability
- Create tactical lane-based combat
- Upgrade enemy movement behavior
- Introduce shared AP economy between player and enemy

This marks the transition from:
- tactical sandbox

to:
- structured tactical encounters.

---

## Systems Implemented

### 1. Terrain Blocking System
- Added dedicated `BlockingTilemap`
- Introduced blocked/unwalkable terrain support
- Units can no longer:
  - Move onto blocked tiles
  - Spawn onto blocked tiles
- Shared validation now applies to:
  - Player movement
  - Enemy movement
  - Placement phase

New method:
- `IsCellBlocked()`

---

### 2. GX Arena Blockout
- Designed first intentional tactical arena inspired by the Duel Academy duel platform
- Replaced rectangular prototype battlefield with:
  - Circular/octagonal arena shape
  - Central blocked platform
  - Top/bottom battle entrances
  - Structured combat lanes

Arena now creates:
- Left/right lane decisions
- Rotational movement
- Tactical positioning pressure
- Natural engagement flow

---

### 3. Deployment Zone Visualization
- Added visible deployment zone highlight system
- Introduced:
  - `DeploymentHighlightTilemap`
- Valid placement tiles are now highlighted during placement phase
- Highlights disappear automatically once battle begins

Improves:
- Readability
- Clarity
- Placement flow
- Tactical onboarding

---

### 4. Terrain-Aware Enemy AI
- Enemy AI now respects blocked terrain
- Prevented enemies from:
  - Moving into blocked center platform
  - Selecting invalid terrain cells

Terrain validation now applies during:
- Attack positioning
- Fallback movement
- Tile evaluation

---

### 5. Path-Aware Enemy Movement
- Added lightweight BFS-based terrain-aware movement evaluation
- Enemy movement scoring now considers:
  - Real navigable distance
  - Terrain obstacles
  - Path progress
- Prevents enemies from:
  - Getting stuck
  - Oscillating between positions
  - Ignoring arena geometry

Enemy AI now:
- Commits to movement lanes
- Navigates around terrain naturally
- Maintains positional intent

---

### 6. Enemy Movement Stability Improvements
- Added movement stability logic to reduce backtracking behavior
- Enemy AI now discourages:
  - Undoing path progress
  - Returning to previous positions
  - Constant lane switching

Result:
- More readable enemy behavior
- More natural tactical pressure
- Better battlefield flow

---

### 7. Shared AP Economy (Player + Enemy)
- Enemy side now uses Action Points
- Enemy units consume AP using:
  - `GetActionAPCost()`

Enemy turn now ends when:
- No enemy AP remains
OR
- No remaining enemy units can act

This creates:
- Tactical tempo
- Turn prioritization
- Meaningful action economy
- Fairer pacing between player and enemy

---

### 8. Enemy AP UI
- Added dedicated Enemy AP panel
- Reused existing AP UI system
- Enemy AP updates dynamically during enemy turn

---

### 9. Turn-Based AP Visibility
- Player AP panel now appears only during player turn
- Enemy AP panel now appears only during enemy turn

Improves:
- UI clarity
- Tactical readability
- Turn presentation

---

## Current State

The game now features:
- Real tactical terrain
- Arena-based combat flow
- Terrain-aware enemy movement
- Stable enemy navigation
- Shared AP economy
- Readable deployment phase
- Proper lane-based engagements

The combat experience now feels significantly closer to:
- a true tactical RPG
- a modernized Capsule Monster Coliseum battle

rather than a prototype grid test.

---

## Known Limitations

- No full pathfinding system yet
  - Current implementation is lightweight path-aware scoring
- No terrain elevation
- No terrain gameplay effects
- No advanced battle presentation
- No capsule AP economy during placement phase yet
- AI still does not consider:
  - Elemental advantage
  - Fusion opportunities
  - Advanced coordination

---

## Next Steps

- Integrate AP economy into capsule placement phase
- Introduce meaningful deployment prioritization
- Build first fully intentional battle encounter:
  - Jaden vs Zane
- Refine encounter pacing and tactical pressure
- Continue moving toward a polished vertical slice

## Session 13 — Capsule Summoning Economy & Goal-Based Enemy Pathfinding

### Overview
This session focused on transforming capsules from a simple deployment mechanic into the true core of the battle economy.

The goal was to:
- Recreate the AP growth loop from the original *Capsule Monster Coliseum*
- Separate deployment from summoning
- Introduce tactical summon pacing
- Upgrade enemy AI routing from heuristic movement to goal-oriented pathfinding

This marks the transition from:
- unit deployment system

to:
- full capsule battle economy.

---

## Systems Implemented

### 1. True Capsule Deployment Phase
- Refactored placement phase:
  - Players now deploy capsules instead of directly spawning units
- Capsules remain on the battlefield until summoned
- Units are no longer automatically active at battle start

This creates:
- Early-game setup phase
- Summoning tempo
- Tactical AP growth decisions

---

### 2. Capsule Summoning System
- Added capsule interaction flow
- Clicking a player capsule now:
  - Opens unit preview
  - Displays contained monster information
  - Allows summoning if enough AP is available

Summoning now:
- Spawns the contained unit
- Removes the capsule
- Consumes AP
- Registers the new unit into combat systems

---

### 3. AP Growth Economy
- Implemented progressive AP growth inspired by the original game

New flow:
- Battle starts with low AP
- Summoning monsters increases max AP
- Future turns restore AP based on upgraded maximum

Example:
- Start at 2 AP
- Summon Avian (+1 AP)
- Next turn restores to 3 AP

This creates:
- Tempo management
- Summon prioritization
- Long-term economy scaling

---

### 4. Level AP Cap
- Added global level AP cap system
- Prevents AP growth from scaling infinitely
- Shared between:
  - Player AP
  - Enemy AP

This allows:
- Battle pacing control
- Encounter balancing
- Fusion timing tuning

---

### 5. Capsule Preview UI
- Unit info panel now supports capsule preview mode
- During capsule preview:
  - AP gain replaces ATK display
  - UI communicates economy value instead of combat value

Improves:
- Summon readability
- Tactical planning
- AP economy clarity

---

### 6. Enemy Capsule System
- Enemy side now also deploys capsules instead of starting with active units
- Enemy AI can:
  - Summon units using AP
  - Increase max enemy AP through summoning
  - Progress through the same economy system as the player

This creates:
- Symmetrical battle pacing
- Shared economy rules
- More authentic Capsule Monsters gameplay

---

### 7. Enemy Capsule Preview
- Enemy capsules can now be inspected
- Clicking enemy capsules displays:
  - Contained monster
  - AP gain
  - Unit information

Improves:
- Tactical awareness
- Readability
- Counterplay planning

---

### 8. Enemy Turn Eligibility Fixes
- Summoned enemy units now correctly:
  - Spawn as Acted
  - Wait until next enemy turn before acting
- Enemy turn reset flow was corrected to:
  - Reset existing units
  - Keep newly summoned units inactive

Prevents:
- Immediate summon attacks
- Unfair enemy tempo spikes

---

### 9. Goal-Based BFS Enemy Pathfinding
- Replaced flawed “move scoring toward target” logic with:
  - Goal-oriented BFS pathfinding

Enemy AI now:
- Computes real shortest-path distance toward valid attack positions
- Evaluates:
  - Reachable attack cells
  - Real navigable routes
  - Terrain-aware combat goals

Previous system only measured:
- geometric closeness

New system measures:
- actual path distance to combat engagement

---

### 10. Real Tactical Routing
- Enemy movement now:
  - Properly navigates lanes
  - Understands flanks
  - Reacts to blocked terrain naturally
  - Stops orbiting or drifting toward invalid corners

This was a major AI quality improvement.

Enemy movement now feels:
- intentional
- aggressive
- readable
- tactically coherent

---

## Current State

The game now features:
- Full capsule-based economy
- Progressive AP growth
- Tactical summon pacing
- Symmetrical player/enemy summon systems
- Terrain-aware goal-based enemy routing
- Real encounter flow and pressure

The prototype now strongly resembles:
- a modernized *Capsule Monster Coliseum* battle system
rather than:
- a generic tactical prototype.

---

## Known Limitations

- No summon animations/effects yet
- No capsule opening VFX
- No advanced enemy coordination
- No elemental awareness in AI
- No fusion-aware AI behavior
- No overworld or progression structure yet

---

## Next Steps

- Add summon visual feedback
- Improve battle presentation and combat juice
- Begin intentional encounter balancing:
  - Jaden vs Zane
- Tune:
  - AP pacing
  - Fusion timing
  - Unit costs
  - Arena pressure
- Prepare for eventual 3D tactical sandbox migration