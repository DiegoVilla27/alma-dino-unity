# 🦖 Alma: Mother's Roar

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-6000.0%20(Unity%206%20LTS)-black?style=for-the-badge&logo=unity)
![Render Pipeline](https://img.shields.io/badge/Render%20Pipeline-URP%202D-2496ED?style=for-the-badge&logo=unity)
![C# Version](https://img.shields.io/badge/C%23-12-239120?style=for-the-badge&logo=c-sharp)
![Architecture](https://img.shields.io/badge/Architecture-Modular%20Feature--First-blueviolet?style=for-the-badge)
![Tests](https://img.shields.io/badge/Tests-NUnit%20EditMode%20Passing-brightgreen?style=for-the-badge)
![Platforms](https://img.shields.io/badge/Platforms-PC%20%7C%20Mobile%20%7C%20Console%20Ready-orange?style=for-the-badge)

<br/>

**A 2D precision platformer with light puzzle solving and Metroidvania-lite progression, centered around an emotional narrative of maternal determination.**

[Key Features](#-key-features) •
[Design Pillars](#-design-pillars) •
[Mechanics & Physics](#-mechanics--physics-game-feel) •
[Ability Progression](#-cumulative-ability-progression-system) •
[World & Level Structure](#-world--level-structure) •
[Software Architecture](#-software-architecture--clean-code) •
[Editor Tooling](#-unity-editor-tooling) •
[Getting Started](#-getting-started--setup-guide) •
[Production Roadmap](#-production-roadmap)

</div>

---

## 📖 Executive Summary & Pitch

In a harsh, beautiful prehistoric world, the earth shook only once. When **Alma**, an agile dinosaur mother, returned to her nest with food, she found absolute silence: a band of nimble thieves led by a mischievous thief monkey had stolen her four still-warm eggs.

Without hesitation and driven purely by maternal instinct and speed, Alma embarks on an arduous journey on foot across four hostile biomes to rescue her stolen children before they hatch in enemy hands.

Inspired by the tactile precision of **Celeste**, the atmospheric environmental storytelling of **Hollow Knight**, the momentum of **Ori and the Blind Forest**, and the maternal charm of **Yoshi's Island**.

---

## 🌟 Key Features

- **Ultra-Responsive Locomotion**: Analogue variable-height jumping, *Coyote Time*, *Jump Buffer*, asymmetric gravity curves, and zero-friction wall/slope interaction.
- **Cumulative Metroidvania-Lite Progression**: 4 strictly linear worlds encompassing 16 handcrafted precision levels. Each world unlocks an ability that accumulates and organically synthesizes with prior mechanics.
- **Restrained Environmental Storytelling**: Story beats delivered via environmental atmosphere, diegetic visual cues, and short prologue/rescue text banners. Zero intrusive cutscenes or melodrama.
- **Skill-Puzzle Boss Encounters**: 4 world-ending encounters designed as tight, 3-phase execution puzzles testing mastery of newly acquired abilities rather than repetitive health-sponge attrition.
- **Diegetic HUD & Contextual Mobile UI**: Player abilities reflected through in-character feedback (feather luminescence and subtle rumbling) coupled with responsive, contextual mobile on-screen controls.
- **True Cross-Platform Ready**: Seamless runtime input switching between Keyboard/Mouse, Gamepads (Xbox, PlayStation, Switch), and dynamic on-screen touch interfaces.

---

## 🏛️ Design Pillars

1. **Perfect Tactile Feel (Game Feel First)**  
   Alma's controls must feel rewarding, responsive, and fair before adding art or sound. Every death must be perceived as a learning opportunity by the player, never as physics infidelity.
2. **Pedagogical Progression (Nintendo Philosophy)**  
   Every new mechanic is introduced in isolation (**Introduce**), practiced in a safe context (**Practice**), combined with cross-system hazards (**Complicate**), and evaluated under pressure (**Evaluate**).
3. **Restrained Emotional Resonance**  
   Atmospheric silence, subtle lighting, and the primal bond between mother and offspring. Alma's voice is expressed through her agility, perseverance, and roar.

---

## 🕹️ Mechanics & Physics (Game Feel)

### Precision Locomotion Calibration

The current movement profile and level-design constraints are documented in [Physics and Gameplay](Docs/Physics_and_Gameplay.md).

| Parameter | Calibrated Value | Gameplay Purpose |
|---|---|---|
| **Horizontal Speed (`MoveSpeed`)** | `7.0 m/s` | Reaches max speed in `0.10s`; brakes in `0.08s` and uses `0.13s` air acceleration. |
| **Jump Force (`JumpForce`)** | `8.2 m/s` | Base jump height of ~1.56m; variable height based on button hold duration. |
| **Gravity Scale (`GravityScale`)** | `2.2` | Snappy, grounded descent preventing "floaty" platforming feel. |
| **Jump Cut Multiplier (`JumpCutMult`)** | `2.4x gravity` | Enables micro-hops and tight trajectory control by releasing Jump early. |
| **Coyote Time** | `0.14 s` | Grace window to execute jumps after walking off platform edges. |
| **Jump Buffer** | `0.12 s` | Registers Jump inputs executed right before landing on surfaces. |
| **Physics Material** | `Friction: 0.0` | Prevents the character from sticking to vertical surfaces when pressing towards them. |
| **Fall Death Boundary (`FallDeathY`)** | `-8.0 m` | Immediate respawn trigger at active checkpoint upon abyss falls. |

---

### Interactive Environmental Elements

- **Bouncy Mushrooms & Super Bounce (`BouncyPlatform2D`)**:
  - Solid elastic trampolines propelling Alma vertically at high speed (`17 m/s` by default, ~6.7m theoretical peak height; individual platforms can override it).
  - **Super Bounce (+18%)**: Holding or pressing **Jump** upon landing accelerates Alma to **`20.06 m/s`** from the default `17 m/s` (~9.3m theoretical peak height).
  - **Ground Detector Decoupling (`IBouncySurface2D`)**: The player's [`GroundDetector2D`](file:///Users/diegovilla/Desktop/unity/AlmaDino/Assets/_Project/Features/Player/Components/GroundDetector2D.cs) ignores bouncy surfaces. This prevents state-machine false transitions into `Idle`/`Run` and avoids early Jump-Cut momentum cancellation.
  - **Squish & Stretch Animation**: Immediate squash compression (`0.06s`) followed by vertical elastic rebound (`0.14s`) for maximum kinetic juice.
  - **Mid-Air Momentum Preservation**: Activating Double Jump during bounce ascent preserves a stronger upward velocity; otherwise restores at least `7.6 m/s` without stacking impulses. Refreshes aerial abilities upon contact.
- **Crumbling Canopy Leaves (`CrumblingPlatform2D`)**:
  - Unstable tree-canopy foliage that quivers with a warning red tint and collapses after **1.0s in 1-1/1-2, 0.75s in 1-3, and 0.65s in 1-4**. Respawns after 2.5s.
- **Rhythmic Carnivorous Plants (`CarnivorousPlant2D`)**:
  - Environmental hazards operating on a 3-phase cycle: Open/Safe (1.8s) ➔ Yellow Warning (0.5s) ➔ Lethal Red Snap (1.0s).
- **Diegetic Nest Checkpoints (`Checkpoint2D`)**:
  - Ancient abandoned nests that kindle an emerald/golden flame upon contact, permanently setting Alma's active respawn point.

---

## ⚡ Cumulative Ability Progression System

Abilities are **strictly cumulative**: no new mechanic replaces an existing one; instead, they synergize to solve increasingly complex navigational challenges:

```
[Base Movement: Run & Jump]
           │
           ▼ (World 1: Emerald Jungle)
[Maternal Double Jump / Flutter]
           │
           ▼ (World 2: Crystal Caves)
[Seismic Ground Pound]   ────────►  (High Jump + Downward Strike to shatter brittle stone)
           │
           ▼ (World 3: Wind & Mist Swamp)
[Air Dash]               ────────►  (Double Jump + Dash to cross gale-force chasms)
           │
           ▼ (World 4: Volcanic Summit)
[Shockwave Roar]         ────────►  (Complete Synthesis: Jump + Dash + Pound + Roar)
```

| World | Biome | Unlocked Ability | Mechanics & Gameplay Role |
|---|---|---|---|
| **World 1** | **Emerald Jungle** | **Double Jump / Maternal Flutter** | Mid-air second jump. Enhances and stacks with mushroom bounce velocity during ascent. |
| **World 2** | **Crystal Caves** | **Seismic Ground Pound** | Downward dive at `22.0 m/s`. Breaks fractured floors (`BreakableGround2D`), triggers seesaw catapults, and flips armored enemies. |
| **World 3** | **Wind Swamp** | **Air Dash** | 6-meter horizontal burst in `0.2s` freezing Y-axis gravity. Provides momentary immunity against gale-force wind currents. |
| **World 4** | **Volcanic Summit** | **Shockwave Roar** | Short-range conical sonic wave. Pushes igneous boulders into magma rivers, triggers distant bells, and deflects falling debris. |

### Cross-Level Progression Persistence (`GameProgression.cs`)

To guarantee seamless gameplay across scene transitions and isolated level testing:
- Unlocked abilities are cached in static memory and persisted in `PlayerPrefs`.
- **`EnsureLevelBaseline(sceneName)`**: Automatically unlocks baseline abilities when booting any advanced level directly (e.g., Level 1-2+ guarantees Double Jump from frame one).
- Reset utility available in the Unity Editor menu for fresh-save playtesting.

---

## 🗺️ World & Level Structure

### WORLD 1: EMERALD JUNGLE (Current Milestone: Vertical Slice)

- **Atmosphere:** Lush greens, warm canopy-filtered sunlight, birdsong, and creaking boughs.
- **Milestone:** Rescue of **Egg 1 (Green)**.

```
Level 1-1 ("Awakening in the Nest")   --> Base locomotion tutorial & awakening of Maternal Double Jump.
Level 1-2 ("The Dangerous Canopy")    --> Verticality, Thief Monkey encounter, and 5 mushroom platforming challenges.
Level 1-3 ("The Deep Brambles")       --> Crumbling leaves, relentless thorns, and high-precision timing.
Level 1-4 ("The Great Tree Summit")   --> Ascending chase towards the canopy summit and Green Egg rescue.
Boss 1    ("Giant Thief Monkey")      --> Canopy arena; dodge rolling fruit and execute head-stomp counterattacks.
```

#### Implemented Scenes Breakdown:

- **Level 1-1 (`Assets/Scenes/World_1_Jungle/Level_1_1.unity`)**:
  - Begins at Alma's empty cradle with the introductory narrative prologue.
  - Teaches single jump distance and short gaps.
  - Reaching the Maternal Gem Altar (`AbilityRelic2D`) awakens the **Double Jump**.
  - Player traverses a previously impassable chasm to reach the exit gate.

- **Level 1-2 (`Assets/Scenes/World_1_Jungle/Level_1_2.unity`) — "The Dangerous Canopy"**:
  - **Cinematic In-Game Encounter**: The Thief Monkey starts **directly in front of Alma** at spawn (`X = 3.2`) holding the **Stolen Golden Egg** (`Stolen_Golden_Egg`). A dialogue banner triggers with the monkey's taunt, followed by a comedic laugh-hop and a high parabolic leap into the canopy.
  - **5 Precision Platforming Challenges**:
    1. *Guided High Bounce under Thorn Spikes (`X = 8.5`)*: A ground mushroom launches towards an overhead thorn ceiling (`Y = 7.0`); players must steer right in mid-air to land safely on `Branch_Ledge_1` (`Y = 4.2`).
    2. *Carnivorous Plant Timing Gate (`X = 21.0`)*: Rhythmic plant obstacle requiring timing or a high double jump onto the second mushroom, supported by a lower safety branch.
    3. *Super Bounce Height Check & Checkpoint 1 (`X = 25.5` to `32.5`)*: An elevation climb of +8.0m to `Canopy_Cliff_1` (`Y = 13.5`) demanding Super Bounce execution (holding Jump) or apex double jumping. Features **Nest Checkpoint 1**.
    4. *Crumbling Leaves Sequence (`X = 38.5` to `45.5`)*: Two decaying leaves with `1.0s` collapse timers separated by a hanging vine hazard over an abyss.
    5. *Aerial Mushroom Chain in the Void (`X = 57.0` to `69.0`)*: Chaining mid-air suspended mushrooms past a snapping plant hazard to ascend to the **Great Canopy Nest** (`Y = 31.0`), **Checkpoint 2**, and the exit portal to World 1-3.

---

## 🏗️ Software Architecture & Clean Code

The codebase enforces **Modular Feature-First Architecture** with strict compile-time boundaries established by **Assembly Definitions (`.asmdef`)**:

```text
Assets/_Project/
├── Core/                           # Kernel, interfaces, progression, event channels
│   ├── AlmaDino.Core.asmdef
│   ├── Interfaces/                 # IBounceable2D, IBouncySurface2D, IHazard2D, etc.
│   ├── Progression/                # GameProgression persistence service
│   ├── Events/                     # ScriptableObject Event Channels (CameraShake, AbilityUnlocked)
│   └── Editor/                     # SceneSetupValidator, Renderer2DValidator
├── Shared/                         # Cross-feature models, constants, and physics materials
│   ├── AlmaDino.Shared.asmdef
│   └── Data/                       # FacingDirection2D, PlayerFrictionless.physicsMaterial2D
├── Features/
│   ├── Player/                     # Player FSM & Controllers
│   │   ├── AlmaDino.Features.Player.asmdef
│   │   ├── Controllers/            # PlayerController, PlayerInputReader
│   │   ├── Services/States/        # FSM States: Idle, Run, Jump, Fall, DoubleJump, GroundPound, Dash, Roar
│   │   ├── Components/             # GroundDetector2D
│   │   └── ScriptableObjects/      # AlmaPhysicsConfigSO
│   ├── Environment/                # Interactive World Elements
│   │   ├── AlmaDino.Features.Environment.asmdef
│   │   └── [BouncyPlatform2D, ThiefMonkeyTeaser2D, CarnivorousPlant2D, Checkpoint2D, ...]
│   ├── Camera/                     # Camera Rigs & Cinemachine/Follow Systems
│   │   ├── AlmaDino.Features.Camera.asmdef
│   │   └── [Camera2DFollow, CameraShake2D]
│   └── MobileUI/                   # Contextual Virtual Touch Controls
│       ├── AlmaDino.Features.MobileUI.asmdef
│       └── Controllers/            # VirtualTouchJoystick, VirtualTouchButton, MobileHUD
└── Tests/
    └── EditMode/                   # Automated NUnit Tests
        ├── AlmaDino.Tests.EditMode.asmdef
        └── [PlayerStateMachineTests, GameProgressionTests]
```

### Architectural Principles:
- **Thin MonoBehaviours**: `PlayerController` handles only Unity lifecycle events (`Awake`, `Update`, `FixedUpdate`) and delegates business logic to plain C# state classes.
- **Interface Decoupling**: Features interact exclusively through interfaces located in `Core.Interfaces` (`IBounceable2D`, `IBouncySurface2D`, `IPlayerRespawnable`, `IHazard2D`).
- **Zero-Allocation Runtime**: Non-allocating physics queries (`OverlapBoxNonAlloc` with `ContactFilter2D`) and reusable buffers prevent GC frame spikes.

---

## 🎮 Controls

| Action | Keyboard (PC) | Gamepad (Xbox / PlayStation) | Mobile Touch HUD |
|---|---|---|---|
| **Move Left / Right** | `A` / `D` or Arrow Keys | Left Stick / D-Pad | Dynamic Virtual Joystick |
| **Jump / Super Bounce** | `Space` | South Button (`A` / `✕`) | `JUMP` Button |
| **Double Jump (Flutter)** | `Space` (in air) | South Button (`A` / `✕`) | `JUMP` Button (in air) |
| **Ground Pound** *(World 2+)* | `S` or Down Arrow | Left Stick Down + `B` / `○` | `POUND` Button |
| **Air Dash** *(World 3+)* | Left `Shift` | Right Trigger / `X` | `DASH` Button |
| **Shockwave Roar** *(World 4+)* | `E` or `F` | North Button (`Y` / `△`) | `ROAR` Button |
| **Skip Text / Continue** | Left Click / Space | South Button (`A` / `✕`) | `CONTINUAR ▶` Button or Tap |

---

## 🛠️ Unity Editor Tooling

The top-level **`Alma`** menu bar in Unity provides instant developer workflows:

- **`Alma ▶ 📂 Cargar Nivel 1-1`**: Loads the tutorial and awakening scene (`Level_1_1.unity`).
- **`Alma ▶ 📂 Cargar Nivel 1-2`**: Loads the dangerous canopy climb and 5 challenge geometry (`Level_1_2.unity`).
- **`Alma ▶ 📂 Cargar Nivel 2-1`**: Loads the cave descent, ground-pound altar, four breakable floors and three checkpoints. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 2-1 - Descenso a la Penumbra`**.
- **`Alma ▶ 📂 Cargar Nivel 2-2`**: Loads the seesaw gallery, counterweight puzzles, timed gates and high catapult ledge. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 2-2 - La Galería de Ecos`**.
- **`Alma ▶ 🔄 Resetear Progresión de Partida`**: Clears `PlayerPrefs` progression data to test fresh-save onboarding from scratch.
- **`Alma ▶ 🛠️ Reparar Escena y Visuales`**: Re-imports sprites, verifies URP 2D Unlit materials, and auto-repairs EventSystem and touch controls.

---

## 🚀 Getting Started & Setup Guide

### Prerequisites
- **Unity 6 LTS** (recommended: `6000.0.x` or later) with **Universal Render Pipeline (URP)** installed.
- **Git** with **Git LFS** enabled.

### Standard Setup Instructions

1. **Clone the Repository**:
   ```bash
   git clone https://github.com/DiegoVilla27/alma-dino-unity.git
   cd alma-dino-unity
   ```
2. **Open in Unity Hub**:
   - Launch Unity Hub.
   - Click **Add** ➔ **Add project from disk** and select the cloned root folder.
   - Ensure the editor version is set to **Unity 6 LTS**.
3. **Domain & Assembly Compilation**:
   - Unity will automatically compile the assembly definitions (`AlmaDino.Core`, `AlmaDino.Features.Player`, etc.).
4. **Open and Run a Scene**:
   - From the Unity top menu bar, select:
     **`Alma ▶ 📂 Cargar Nivel 1-1`** or **`Alma ▶ 📂 Cargar Nivel 1-2`**.
   - Press the **Play ▶** button in the Unity Editor toolbar.

### Running Automated Tests (TDD)
1. In the Unity Editor, navigate to **Window ▶ General ▶ Test Runner**.
2. Select the **EditMode** tab for unit tests or **PlayMode** for the level 2-1 physics and full-route checks.
3. Click **Run All** to execute unit tests for player state transitions (`PlayerStateMachineTests`) and persistence baseline logic (`GameProgressionTests`).

---

## 🗺️ Production Roadmap

| Phase | Scope | Status |
|---|---|:---:|
| **Phase 1: Locomotion Prototype** | FSM with 8 states, Celeste-style jump curves, Jump Buffer, Coyote Time, Virtual Touch HUD. | ✅ **Completed** |
| **Phase 2: Vertical Slice (World 1)** | Levels 1-1 and 1-2, Bouncy Mushrooms, Crumbling Leaves, Carnivorous Plants, Thief Monkey Teaser. | 🚀 **In Progress** |
| **Phase 3: Jungle Conclusion** | Levels 1-3, 1-4, and Boss 1 (Giant Thief Monkey arena battle & Green Egg rescue). | ⏳ Planned |
| **Phase 4: Crystal Caves (World 2)** | Levels 5-8, Seismic Ground Pound mechanic, brittle floors, and Boss 2 (Prehistoric Armadillo). | ⏳ Planned |
| **Phase 5: Mist Swamp (World 3)** | Levels 9-12, Air Dash mechanic, horizontal wind geysers, and Boss 3 (Alpha Pterodactyl). | ⏳ Planned |
| **Phase 6: Volcanic Summit (World 4)** | Levels 13-16, Shockwave Roar, complete mechanic synthesis puzzles, and Final Boss (The Thief King). | ⏳ Planned |
| **Phase 7: Polish, Audio & Launch** | Adaptive soundtrack, particle VFX, accessibility settings, and standalone PC/Mobile builds. | ⏳ Planned |

---

<div align="center">

> This digital ecosystem has been designed, structured, and developed to high-performance standards by **[Cabuweb](https://cabuweb.com)** - **Software Developer: Diego Villa**.

</div>
