# 🦖 Alma: Mother's Roar

<div align="center">

![Unity Version](https://img.shields.io/badge/Unity-6000.6.0f1%20(Unity%206)-black?style=for-the-badge&logo=unity)
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
- **Cumulative Metroidvania-Lite Progression**: A planned campaign of 4 strictly linear worlds encompassing 16 handcrafted precision levels; Worlds 1 and 2 currently have playable scenes and boss arenas. Each world unlocks an ability that accumulates and organically synthesizes with prior mechanics.
- **Restrained Environmental Storytelling**: Story beats delivered via environmental atmosphere, diegetic visual cues, and short prologue/rescue text banners. Zero intrusive cutscenes or melodrama.
- **Skill-Puzzle Boss Encounters**: 4 planned world-ending encounters, with Boss 1 and Boss 2 implemented, designed as tight execution puzzles testing mastery of newly acquired abilities rather than repetitive health-sponge attrition.
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
  - **Ground Detector Decoupling (`IBouncySurface2D`)**: The player's [`GroundDetector2D`](Assets/_Project/Features/Player/Components/GroundDetector2D.cs) ignores bouncy surfaces. This prevents state-machine false transitions into `Idle`/`Run` and avoids early Jump-Cut momentum cancellation.
  - **Squish & Stretch Animation**: Immediate squash compression (`0.06s`) followed by vertical elastic rebound (`0.14s`) for maximum kinetic juice.
  - **Mid-Air Momentum Preservation**: Activating Double Jump during bounce ascent preserves a stronger upward velocity; otherwise restores at least `7.6 m/s` without stacking impulses. Refreshes aerial abilities upon contact.
- **Crumbling Canopy Leaves (`CrumblingPlatform2D`)**:
  - Unstable tree-canopy foliage that quivers with a warning red tint and collapses after **1.0s in 1-1/1-2, 0.75s in 1-3, and 0.65s in 1-4**. Respawns after 2.5s.
- **Rhythmic Carnivorous Plants (`CarnivorousPlant2D`)**:
  - Environmental hazards operating on a 3-phase cycle: Open/Safe (1.8s) ➔ Yellow Warning (0.5s) ➔ Lethal Red Snap (1.0s).
- **Diegetic Nest Checkpoints (`Checkpoint2D`)**:
  - Ancient abandoned nests that kindle an emerald/golden flame upon contact, permanently setting Alma's active respawn point.

---

## 📷 Consistent Camera Follow

Every current scene uses an orthographic camera size of **6**, including both boss arenas and `SampleScene`. `Camera2DFollow` also enforces this size at runtime, preserving the same zoom in portrait and landscape orientations.

- The camera follows Alma with smooth movement (`0.12s`) and a small dead zone (`0.5` units per axis).
- Horizontal movement adds **1.25 units of look-ahead** toward the direction of travel, showing more of the path on the left or right. The offset recenters when Alma stops.
- Level-specific vertical offsets and camera bounds remain in use. Scene builders preserve the shared zoom and neutral horizontal offset when regenerating levels.
- Boss 2 uses the same follow behavior; the previous arena-wide automatic zoom has been removed.

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

### WORLD 1: EMERALD JUNGLE (Playable Prototype)

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

### WORLD 2: CRYSTAL CAVES (Playable Prototype)

**Milestone:** Rescue the **Blue Egg**, then defeat the Prehistoric Armadillo using the cumulative Double Jump and Ground Pound abilities.

| Scene | Level | Main Challenges |
|---|---|---|
| `Assets/Scenes/World_2_Caves/Level_2_1.unity` | Descenso a la Penumbra | Cave descent, Ground Pound altar, four breakable floors and three checkpoints. |
| `Assets/Scenes/World_2_Caves/Level_2_2.unity` | La Galería de Ecos | Seesaws, counterweights, timed rune gates and a high catapult ledge. |
| `Assets/Scenes/World_2_Caves/Level_2_3.unity` | El Filo Resonante | Armored beetles, seismic flipping, fragile-floor bridge and telegraphed bats. |
| `Assets/Scenes/World_2_Caves/Level_2_4.unity` | El Laberinto de Geodas | Crushing ceilings, Ground Pound shelters, pressure catapult and persistent Blue Egg rescue. |
| `Assets/Scenes/World_2_Caves/Boss_2.unity` | Armadillo Prehistórico | Pillar-impact cycles, direct crown strikes, falling crystals and a final rolling arc. |

#### Boss 2: How to Win

1. Dodge the rolling armadillo until it hits the arena pillars **three times**. Use the elevated refuges to avoid its path.
2. After the third impact, its glowing crown becomes vulnerable for **4.5 seconds**.
3. Jump outward from a refuge, align above the crown and activate **Ground Pound** while airborne (`S`, Down Arrow or the mobile `POUND` button).
4. Repeat for **three successful crown strikes**. Ordinary landings and nearby seismic shockwaves do not damage this boss.

The two one-way refuges are centered at `X = ±5`, with a width of `3m` and a top surface at `Y = 2.15m`, reachable with Double Jump. They leave clear descent paths to the boss's stun positions at `X = ±8`, so the platforms do not block the finishing dive.

| Fight Parameter | Current Configuration |
|---|---|
| Pillar impacts per vulnerability window | `3` |
| Successful Ground Pounds to win | `3` |
| Vulnerability duration | `4.5s` |
| Rolling speed across cycles | `7 / 8.5 / 10 m/s` |
| Crown strike horizontal tolerance | `0.85m` from the boss center |
| Falling crystals | Start after the first successful strike; `0.9s` warning, `2s` interval and `10 m/s` fall speed |
| Final-cycle rolling arc | Third rolling leg, `2.7m` height |

Fight tuning lives in [ArmadilloBossConfig.asset](Assets/_Project/ScriptableObjects/ArmadilloBossConfig.asset). `ArmadilloFight` owns the encounter rules, while `PrehistoricArmadilloBoss2D` handles movement, collisions and scene feedback. Falling crystals use a dedicated prefab and yellow ground warnings.

Player death resets the current rolling cycle and clears crystals while preserving successful strikes **within the active encounter**. Defeating the boss records World 2 completion through `GameProgression.CompleteWorld(2)` and enables the exit.

**Current limits:** Boss art is a geometric prototype; final sprites, VFX and audio remain pending. The exit now connects to the playable `Level_3_1` Dash tutorial. Boss 3 is playable; Level 4-1 is playable; later World 4 levels remain planned.

---

### WORLD 3: WIND & MIST SWAMP (Levels 3-1 through 3-4 Implemented)

**Level 3-1 — Los Fangales Tóxicos** (`Assets/Scenes/World_3_Swamp/Level_3_1.unity`) introduces Air Dash through the Ancestral Wind Spore on a safe island. Alma retains Double Jump and Ground Pound, then learns to combine Jump → Double Jump → Dash across four **11m toxic-mud gaps**. Three checkpoints shorten retries; unlocked Dash persists after death. Four prototype parallax layers establish the swamp atmosphere, with the shared camera size of **6**.

Four `Level3_1PlayTests` pass: protection against walking or jumping off the entrance boundary, fresh-entry abilities and destination, failure to cross the tutorial gap with Double Jump alone, and a complete input-driven route with Dash and no deaths. Boss 2 leads into this scene; its exit connects to the playable `Level_3_2`. Final swamp art and audio remain pending.

---

**Level 3-2 — El Cañón de las Ráfagas** (`Assets/Scenes/World_3_Swamp/Level_3_2.unity`) practices Dash against four leftward wind zones at `12 m/s²`. A reed barrier over safe ground teaches the impact before three canyon gaps of **9, 10 and 11m**, including two aerial reed walls. Dash ignores wind during its `0.2s` action; it does not prevent hazard damage. Sheltered checkpoints at `X = 28` and `64`, a solid entrance boundary and the shared camera size **6** support retries. Broken reeds stay open until scene reload. The exit connects to the playable `Level_3_3`; final art and audio remain pending.

---

**Level 3-3 — El Vuelo de las Esporas** (`Assets/Scenes/World_3_Swamp/Level_3_3.unity`) has eight spores aligned at `Y = 2.7` across **16 / 16 / 28m lakes**. Cross each chain by repeating rightward Dash. Difficulty comes from solid-ground obstacles: Ground Pound opens a cracked floor leading beneath a root, steps lead back up, and Dash breaks a reed gate. Two ground-level toads threaten the route with contact damage and pooled poison shots at `8 m/s`, preceded by a `0.6s` warning every `2s`. Checkpoints at `X = 28` and `76` provide safe retries. Death restores spores and the cracked floor and clears poison. Contextual hints explain each obstacle. The exit connects to the playable `Level_3_4` and its Purple Egg rescue. Final art remains pending; audio is excluded by user request.


---

**Level 3-4 — El Sauce Ancestral** (`Assets/Scenes/World_3_Swamp/Level_3_4.unity`) climbs solid branches with Double Jump while toxic gas rises by sections. Each section has a `3s` warning, gas speed `0.9 m/s` and a `4m` initial clearance; checkpoints at `(40, 7.5)` and `(84, 12)` provide time to rest. Two horizontal spores cross a `16m` lake, followed by a Dash reed gate. Reused Pound seesaws provide an optional early shortcut and the required launch to the crown at `Y = 19.5`. Contact with the Purple Egg at `X = 118` persists the rescue, stops the gas, reveals the Alpha Pterodactyl silhouette and opens the exit to the playable `Boss_3`. Three poison toads guard the lower branches, middle ascent and crown, with warned shots aimed toward Alma’s current side and contact damage. Gas restarts on each attempt, including replays with the Purple Egg already saved. Cover before the nest provides a safe place to prepare the last jump. Death resets the active gas section, spores and seesaws and clears poison shots. Camera size stays **6**; final art/audio remain pending.

**Boss 3 — Alpha Pterodactyl** (`Assets/Scenes/World_3_Swamp/Boss_3.unity`) closes World 3 with three frontal Air Dash counters to the cyan head. Four branches are reachable with Double Jump. Horizontal wind at `12 m/s²` precedes a `1.4s` dive warning that locks the target height and shows the counter direction. Dives accelerate from `7` to `8` to `9 m/s`. Single Jump cannot reach the dive head at `3.5m` above the selected branch. Ground Dash and ordinary contact do not damage the boss; body contact remains dangerous during Dash. Successful counters bounce Alma, restore aerial abilities and remove the farthest side branch, leaving the central branch after victory. Death preserves successful hits and broken branches while restarting the current attack cycle. Three hits persist World 3 completion and open the exit to the playable `Level_4_1`. Camera size stays **6** with directional follow; art and audio are prototypes.

**Level 4-1 — Los Ríos de Ceniza** (`Assets/Scenes/World_4_Volcano/Level_4_1.unity`) introduces Shockwave Roar at the sacred fumarole at `X = 6`. Alma retains Double Jump, Ground Pound and Air Dash; Roar unlocks through physical contact and persists after death. The golden forward cone reaches `3m`, with a `45°` half-angle and `0.25s` action. Three heavy basalt rocks resist walking and Dash. Roar moves each rock exactly `5m` in `0.8s`, creating a flat `4.2m` support over **14 / 16 / 16m lava rivers**. Basalt overhangs prevent bypassing the rocks. Jump onto each support, then combine Double Jump and Dash to reach the far bank. Two hot-steam geysers use a `2.2s` safe window, `0.8s` warning and `1.2s` eruption, with explicit safe/warning/danger labels. Four checkpoints support retries: current obstacles reset on death while completed bridges behind the checkpoint and the unlocked Roar persist. Camera size stays **6** with directional follow; four volcanic parallax layers and warm lighting establish the new biome. The Boss 3 portal loads this scene, whose exit loads `Level_4_2`. Final art remains pending; audio is excluded by user request.

**Level 4-2 — Las Campanas de Basalto** (`Assets/Scenes/World_4_Volcano/Level_4_2.unity`) practices Roar with five elevated basalt bells. Bells respond within an **8m forward cone** and suppress their linked flame door for **5 seconds**; rocks retain their **3m** range. A ground introduction leads to a bell requiring Double Jump and an airborne Roar, followed by three timed crossings. Each lava gap requires Double Jump and Air Dash. Visible countdowns warn during the final second; another Roar refreshes the timer. Checkpoints at `X = 30` and `X = 62` reset bell timers on death. Closed flames damage Alma even during Dash. All four abilities are available on direct entry. Camera size remains **6**, with directional lookahead. The exit loads `Level_4_3`; visuals and bell audio are prototypes.

**Level 4-3 — La Gran Fractura** (`Assets/Scenes/World_4_Volcano/Level_4_3.unity`) has three distinct beats. Its opening combines Double Jump, Air Dash, Ground Pound and Roar to reflect one meteor into a gate. The middle offers a choice: high ledges that crumble after `1.5s`, or stable lower stones with timed steam and a salamander. Both routes meet at checkpoint `X = 68`. The finale changes the pace: Pound breaks a volcanic seal, releases a barrier and triggers rising lava after a `1.4s` warning. Ascend steps no more than `1.3m` apart, stun the ledge guard, then cross the last `8m` gap with Double Jump and Dash. Lava rises at `0.9m/s` and stops at the final sanctuary. Checkpoints reset upcoming hazards while preserving the completed entrance gate. Camera size remains **6** with directional follow. Both middle routes and the complete upper route plus escape passed without deaths using normal inputs. The exit loads `Level_4_4`; final art and audio remain pending.

**Level 4-4 — La Antecámara del Fuego** (`Assets/Scenes/World_4_Volcano/Level_4_4.unity`) evaluates all four abilities through distinct rooms: Double Jump onto obsidian columns, one Ground Pound through two stacked cracked pillars, Air Dash through a breakable grid above lava during a safe fire-current window, and Roar to align a basalt support over a `16m` lava moat. Three active salamanders guard different situations; the lower gallery guard reacts to the seismic impact. Checkpoints at `X = 35` and `X = 75` preserve completed trials. A final Double Jump and Dash reach the **Red Egg** on its isolated pedestal. Contact saves the rescue, silences combat and fire, keeps the rescue silent and establishes a sanctuary checkpoint. Carried egg silhouettes display the actual saved eggs in green, blue, purple and red. After a quiet beat, the Thief King emerges as a harmless teaser. The rescued egg unlocks the portal to the future `Boss_Final`; it does **not** complete World 4. Camera size remains **6** with directional follow. Final art remains pending; audio is excluded by user request.



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
│   ├── Enemies/                    # Cave enemies and poison toads
│   │   ├── Controllers/            # PoisonToad2D, pooled PoisonBubble2D
│   │   ├── Services/               # PoisonShotCycle warning and shot timing
│   │   └── ScriptableObjects/      # PoisonToadConfigSO
│   ├── Camera/                     # Smooth Follow, Directional Look-Ahead & Shake
│   │   ├── AlmaDino.Features.Camera.asmdef
│   │   └── [Camera2DFollow, CameraShake2D]
│   ├── Boss/                       # World-ending encounters
│   │   ├── Controllers/            # PrehistoricArmadilloBoss2D, BossFallingCrystal2D
│   │   ├── Services/               # ArmadilloFight encounter rules
│   │   └── ScriptableObjects/      # ArmadilloBossConfigSO
│   └── MobileUI/                   # Contextual Virtual Touch Controls
│       ├── AlmaDino.Features.MobileUI.asmdef
│       └── Controllers/            # VirtualTouchJoystick, VirtualTouchButton, MobileHUD
└── Tests/
    ├── EditMode/                   # Pure rules and state transitions
    │   ├── AlmaDino.Tests.EditMode.asmdef
    │   └── [PlayerStateMachineTests, GameProgressionTests, ArmadilloFightTests, ...]
    └── PlayMode/                   # Scene, input and physics integration
        └── [CameraFollowPlayTests, Boss2PlayTests, cave route tests, ...]
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
- **`Alma ▶ 📂 Cargar Nivel 2-3`**: Loads armored beetles, seismic flipping, a fragile-floor bridge puzzle and telegraphed bat flights. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 2-3 - El Filo Resonante`**.
- **`Alma ▶ 📂 Cargar Nivel 2-4`**: Loads crushing ceilings, Pound shelters, a pressure catapult and the persistent Blue Egg rescue. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 2-4 - El Laberinto de Geodas`**.
- **`Alma ▶ 📂 Cargar Arena Jefe 1`**: Loads the Giant Thief Monkey arena. Regenerate with **`Alma ▶ 🏗️ Reestructurar Arena Jefe 1`**.
- **`Alma ▶ 📂 Cargar Arena Jefe 2`**: Loads the Armadillo arena. Regenerate with **`Tools ▶ Alma ▶ Construir Jefe 2 - Armadillo Prehistórico`**. Dodge three pillar impacts, then Ground Pound the glowing crown within 4.5 seconds.
- **`Alma ▶ 📂 Cargar Nivel 3-1`**: Loads the Dash altar and four toxic gaps. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 3-1 - Los Fangales Tóxicos`**.
- **`Alma ▶ 📂 Cargar Nivel 3-2`**: Loads frontal wind, breakable reeds and sheltered checkpoints. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 3-2 - El Cañón de las Ráfagas`**.
- **`Alma ▶ 📂 Cargar Nivel 3-3`**: Loads horizontal refill chains, a Ground Pound passage, a Dash gate and poison toads. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 3-3 - El Vuelo de las Esporas`**.
- **`Alma ▶ 📂 Cargar Nivel 4-1`**: Loads the Roar altar, basalt bridges and timed steam. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 4-1 - Los Ríos de Ceniza`**.
- **`Alma ▶ 📂 Cargar Arena Jefe 3`**: Loads the wind and Air Dash counter arena. Regenerate with **`Tools ▶ Alma ▶ Construir Jefe 3 - Pterodáctilo Alfa`**.
- **`Alma ▶ 📂 Cargar Nivel 3-4`**: Loads the staged gas ascent, Pound catapults and Purple Egg rescue. Regenerate with **`Tools ▶ Alma ▶ Construir Nivel 3-4 - El Sauce Ancestral`**.
- **`Alma ▶ 🔄 Resetear Progresión de Partida`**: Clears `PlayerPrefs` progression data to test fresh-save onboarding from scratch.
- **`Alma ▶ 🛠️ Reparar Escena y Visuales`**: Re-imports sprites, verifies URP 2D Unlit materials, and auto-repairs EventSystem and touch controls.

---

## 🚀 Getting Started & Setup Guide

### Prerequisites
- **Unity `6000.6.0f1`**, matching `ProjectSettings/ProjectVersion.txt`, with **Universal Render Pipeline (URP)** installed.
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
   - Ensure the editor version is set to **`6000.6.0f1`**.
3. **Domain & Assembly Compilation**:
   - Unity will automatically compile the assembly definitions (`AlmaDino.Core`, `AlmaDino.Features.Player`, etc.).
4. **Open and Run a Scene**:
   - From the Unity top menu bar, select:
     **`Alma ▶ 📂 Cargar Nivel 1-1`**, any cave level, or **`Alma ▶ 📂 Cargar Arena Jefe 2`** for the new encounter.
   - Press the **Play ▶** button in the Unity Editor toolbar.

### Running Automated Tests (TDD)
1. In the Unity Editor, navigate to **Window ▶ General ▶ Test Runner**.
2. Select **EditMode** for player states, progression and encounter rules, or **PlayMode** for scene physics, cave routes, camera behavior and boss interactions.
3. Click **Run All**, or filter by a fixture to verify a specific feature.

The latest volcano validation passed **79 EditMode tests**, **13 Level 4-4 PlayMode tests** and **16 Level 4-3 PlayMode tests**. Previous volcano validation passed **12 Level 4-2 PlayMode tests**. Previous volcano validation passed **10 Level 4-1 PlayMode tests**. Previous boss validation passed **8 Boss 3 PlayMode tests**. Previous World 3 validation passed **13 Level 3-4 PlayMode tests** and **11 Level 3-3 PlayMode tests**. Previous runs passed **5 Level 3-2 PlayMode tests** and **4 Level 3-1 PlayMode tests**. The previous camera and Boss 2 validation also passed **2 camera PlayMode tests** and **4 Boss 2 PlayMode tests**. These are complete EditMode and targeted PlayMode runs, rather than a fresh run of the entire PlayMode suite.

- **`ArmadilloFightTests`**: Pillar impacts, vulnerability timing, successful strikes and encounter reset rules.
- **`CameraFollowPlayTests`**: Directional follow at Alma's actual movement speed and constant size `6` in portrait and landscape.
- **`Boss2PlayTests`**: Direct Ground Pound damage, rejection of ordinary landings, and real-input Double Jump routes onto both refuges followed by outward crown strikes.
- **`Level3_2PlayTests`**: Headwind response, Dash-only reed breaking, sheltered respawn, entrance boundary and a complete route without deaths.
- **`Level3_3PlayTests`**: Horizontal Dash chains, a Pound tunnel, a Dash gate, real toad attacks, airborne refills, death reset, protected checkpoints and a complete route without deaths.
- **`Level3_4PlayTests`**: Faster gas that rises on saved-egg replays, toads aiming at Alma’s current side, actual shots and cover, safe checkpoint resets, optional and required Pound launches, persistent Purple Egg rescue, and a complete route without deaths.
- **`Level4_1PlayTests`**: Roar unlock, immovable boulders, cone range/direction, safe lava bridges, required supports, checkpoint persistence, actual steam/lava damage and a complete route without deaths.
- **`VolcanoMechanicsTests`**: Roar cone targeting, exact boulder displacement, movement reset, steam phases and direct-entry abilities.
- **`Boss3PlayTests`**: Four reachable branches, fixed camera size, dangerous head/body contact, rejection of ground Dash and insufficient single-jump height, scene transition from 3-4, and three real Air Dash counters with checkpoint persistence and a working victory portal.
- **`PterodactylFightTests`**: Dive-only frontal hits, rejection of repeat hits, cycle reset, three-hit victory and direct-entry abilities.
- **`RisingGasCycleTests`**: Warning time, rising speed, safe ceilings, stopping and checkpoint reset.
- Existing cave PlayMode fixtures cover level physics and full-route traversal through World 2.

---

## 🗺️ Production Roadmap

| Phase | Scope | Status |
|---|---|:---:|
| **Phase 1: Locomotion Prototype** | FSM with 8 states, Celeste-style jump curves, Jump Buffer, Coyote Time, Virtual Touch HUD. | ✅ **Completed** |
| **Phase 2: Vertical Slice (World 1)** | Levels 1-1 and 1-2, Bouncy Mushrooms, Crumbling Leaves, Carnivorous Plants, Thief Monkey Teaser. | ✅ **Playable Prototype** |
| **Phase 3: Jungle Conclusion** | Levels 1-3, 1-4, and Boss 1 (Giant Thief Monkey arena battle & Green Egg rescue). | ✅ **Playable Prototype** |
| **Phase 4: Crystal Caves (World 2)** | Levels 2-1 through 2-4 playable and tested; Blue Egg rescue complete. Boss 2 has reachable refuges, clear crown-strike paths, three-hit progression and World 2 completion. Final art/audio remain pending; the exit connects to Level 3-1. | ✅ **Playable Prototype** |
| **Phase 5: Mist Swamp (World 3)** | Levels 3-1 through 3-4 implement Dash, wind/reeds, horizontal spores, poison toads, rising gas, Pound catapults and Purple Egg rescue; Boss 3 adds three Air Dash counters, collapsing branches and persistent World 3 completion. Final art/audio remain pending. | ✅ **Playable Prototype** |
| **Phase 6: Volcanic Summit (World 4)** | Level 4-1 implements Shockwave Roar, mandatory basalt supports over lava, timed steam and checkpoint resets. Level 4-2 adds timed resonance bells, airborne Roar and a triple flame-door chain. Level 4-3 combines an opening meteor puzzle, two alternate routes and a rising-lava escape with magma salamanders. Level 4-4 adds four ability trials and the Red Egg rescue. The Final Boss remains planned. | 🚀 **In Progress** |
| **Phase 7: Polish, Audio & Launch** | Particle VFX, accessibility settings, and standalone PC/Mobile builds; audio excluded by user request. | ⏳ Planned |

---

<div align="center">

> This digital ecosystem has been designed, structured, and developed to high-performance standards by **[Cabuweb](https://cabuweb.com)** - **Software Developer: Diego Villa**.

</div>

### Audio policy

The game is silent by user request. Scenes and prefabs contain no audio sources or listeners; roar, bells, boss impacts and egg rescues retain only their visual and gameplay feedback. Do not add music or sound effects without an explicit new request. Earlier audio concepts in design documents are not implementation requirements.
