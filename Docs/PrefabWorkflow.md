# Reusable level pieces

The library covers all 20 gameplay scenes. Only `World_1_Jungle` has been migrated in this phase: `Level_1_1`, `Level_1_2`, `Level_1_3`, `Level_1_4` and `Boss_1`. Worlds 2–4 retain their scene layouts and are ready for a later migration.

## Library

- `Assets/_Project/Prefabs/Player/Alma/`: configured player prefab, exclusive scripts/assembly, sprites, physics configuration and animation/design documentation.

- `Assets/_Project/Prefabs/Enemies/`: enemy NPCs.
- `Assets/_Project/Prefabs/Traps/`: damaging environment and traps.
- `Assets/_Project/Prefabs/Projectiles/`: spawned attacks and projectiles.
- `Assets/_Project/Prefabs/Bosses/`: bosses and their connected encounters under `Assemblies/World_*`.
- `Assets/_Project/Prefabs/Resources/`: platforms, traversal aids, mechanisms, pickups and puzzle assemblies under `Assemblies/World_*`.
- `Assets/_Project/Prefabs/Narrative/`: narrative triggers, scenery and visual support.
- `Assets/_Project/Prefabs/Resources/LevelPrefabCatalog.asset`: structural catalog with source scenes/objects.
- [Gameplay inventory](../INVENTARIO_GAMEPLAY_PREFABS.md): all 82 named gameplay assets, behavior and configuration. The library contains 166 prefabs including configuration variants and 33 assemblies. Existing asset GUIDs are retained.
- [Complete structural inventory](PrefabCatalogInventory.md): individual asset paths and scene coverage.

Use an **assembly** when placing an entire puzzle or encounter. Individual pieces intentionally expose their scene-specific connections: a gate needs its switches; an egg or boss needs its destination portal. A standalone mechanism is a building block, not a complete puzzle.

## Place and configure

1. Open the destination gameplay scene (with Alma and its camera).
2. Select a prefab in the Project window and use **Alma > Prefabs > Place selected prefab and bind player-camera**. This instantiates it under `--- LEVEL ---` and assigns missing player/camera references using serialized properties.
3. Alternatively, drag the prefab into the scene and run **Bind player-camera on selected instance**. Existing assignments are preserved; binding is scoped to the object's scene.
4. Adjust placement and the exposed component properties. Assemblies preserve the source layout. Some existing controllers use world-space limits, spawn coordinates or thresholds: update those fields when relocating the encounter. Choose the destination scene on portals and connect any external mechanisms when placing an individual piece.
5. Use prefab variants for reusable tuning differences. Keep layout changes as instance overrides. Apply a change to the base prefab only when it should affect every instance.

No player or camera is bundled into enemy/encounter prefabs. Scene actors remain scene dependencies, so the library does not create extra players or cameras.

## Artwork and animations

Every environment SpriteRenderer has a **Prefab Sprite 2D** component with an optional **Sprite** field. Leave it empty to preserve the prototype. Assign artwork to the visual child you want to replace; composite enemies retain separate body, eyes, warnings and other visual parts.

The artwork slot uses the original local rendering size and does not resize the transform or collider. Use Sprite (2D and UI) imports with Full Rect meshes for predictable sliced rendering. Existing scripts continue controlling tint, visibility and motion; their references and configurations are retained. No audio is added.

For animations driven by sprite frames, configure the existing animation component/Animator; the artwork slot supplies a static starting sprite, not a new animation system. Edit the prefab asset to share artwork across instances.

## Generation and migration

- **1. Build catalog from all worlds** inventories current levels and creates missing piece types/connected assemblies. Existing assets and artwork are retained. It does not rewrite World 2–4 scene files.
- **2. Migrate world 1 scenes** converts ordinary level roots to prefab instances. It compares the original object/component state, references and geometry before saving each scene, and aborts on a difference. The only added runtime component is the artwork slot.
- **Validate library and world 1** checks scene coverage, prefab links, missing scripts and artwork slots, and refreshes the inventory document.

Interactive commands offer Unity's normal save prompt before opening scenes, then restore the previous scene setup. World 1 builders instantiate the common solid-platform prefab and connect remaining constructed objects to the catalog before saving. The original environment factory and monkey projectile builder retain existing assets rather than overwrite designer edits or recreate their GUIDs.

Prefabs are grouped by hierarchy/component structure, not by the individual placement of each platform. The catalog preserves the first example's defaults; level-specific values remain on scene instances. Catalog generation is additive: intentionally update existing prefabs in Prefab Mode rather than expecting it to overwrite customizations.

## Validation

Migration snapshots verify original GameObjects/components survive, all serialized component values/references remain unchanged, and no gameplay component is introduced. Edit Mode tests cover artwork replacement without changing collision geometry. Play Mode smoke tests load all four World 1 levels, exercise movement and verify the monkey boss can still unlock its linked portal.

Validation on Unity 6000.6.0f1: **84 Edit Mode tests passed; 7 Play Mode tests passed** (World 1 migration and camera). A separate comparison of the five saved scenes against their originals confirmed identical serialized gameplay/rendering state, including player and camera objects. A second catalog generation retained 103 piece types and 33 assemblies without adding duplicates.

Use **Alma > Prefabs > Validate named inventory and folders** to verify the 82 canonical assets, category folders, missing scripts and artwork slots. `GameplayPrefabIndex.json` is the machine-readable index used by this validation.

Alma uses PlayerSpriteAnimator and its SpriteRenderer/frame arrays instead of PrefabSprite2D on the animated visual. See [the Player guide](../Assets/_Project/Prefabs/Player/Alma/README.md). Drag the player prefab as a scene root; the level-piece placement command is intended for environment pieces. Connect the scene camera and mechanisms explicitly.
