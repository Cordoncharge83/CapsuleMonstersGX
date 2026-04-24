# CapsuleMonstersGX

A grid-based tactical RPG prototype inspired by *Yu-Gi-Oh! Capsule Monster Coliseum*, reimagined with a focus on GX-era mechanics and modern gameplay structure.

---

## 🎯 Project Goal

The objective of this project is to build a solid, scalable tactical combat system featuring:

- Grid-based movement
- Turn-based gameplay
- Unit-based combat
- Future support for capsule summoning and fusion mechanics

The current version focuses on delivering a **playable core prototype** before expanding into more advanced systems.

---

## 🕹️ Current Features

### Grid & Movement
- Tilemap-based grid system
- Click-to-move interaction
- Units snap precisely to grid cells

### Turn System
- Alternating turns between player and enemy
- Input restricted to active turn
- Basic turn flow management

### Combat
- Units have HP and Attack Power
- Adjacent attack system
- Player attacks by clicking enemy unit
- Enemy attacks automatically when adjacent
- Units are destroyed when HP reaches 0

### Enemy AI
- Moves toward player
- Attacks when in range
- Simple decision-making logic

---

## 🧠 Tech Stack

- **Engine:** Unity (2D)
- **Language:** C#
- **System Design:** Component-based architecture

---

## 🚧 Current Limitations

- No movement range (units can move freely)
- No attack range beyond adjacency
- No pathfinding system
- No animations or visual feedback
- No UI system
- No fusion mechanics yet

---

## 🚀 Roadmap

### Phase 1 — Core Systems (Completed)
- Grid system
- Unit movement
- Turn system
- Enemy AI
- Basic combat

### Phase 2 — Tactical Depth (Next)
- Movement range (MOV stat)
- Attack range (RNG stat)
- Improved enemy behavior
- Tile highlighting & selection feedback

### Phase 3 — GX Mechanics (Planned)
- Capsule summoning system
- Fusion mechanics
- Monster variety and balancing

---

## ▶️ How to Run

1. Open the project in Unity Hub
2. Load the main scene
3. Press Play

---

## 🎮 Controls

- **Left Click (Tile):** Move unit
- **Left Click (Enemy, adjacent):** Attack

---

## 📌 Notes

This project is developed as a **technical and design-focused prototype**, prioritizing gameplay systems over visuals.

The goal is to build a strong foundation before expanding into a full game experience.

---