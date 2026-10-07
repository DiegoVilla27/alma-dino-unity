# Inventario de gameplay y nombres de prefabs

Fecha de revisión: 4 de octubre de 2026.

Este documento conserva el inventario del prototipo anterior y define los **nombres futuros** de 82 prefabs. En esta rama solo existen Alma (`Assets/Prefabs/Player/Alma.prefab`), la planta carnívora (`Assets/Prefabs/Enemies/Plant_Carnivorous_Jungle/`), el escarabajo de cristal (`Assets/Prefabs/Enemies/CrystalBeetle_Caves/`), el murciélago de cueva (`Assets/Prefabs/Enemies/CaveBat_Caves/`), el sapo venenoso (`Assets/Prefabs/Enemies/PoisonToad_Swamp/`), la salamandra de magma (`Assets/Prefabs/Enemies/MagmaSalamander_Volcano/`), doce trampas (`Assets/Prefabs/Level/Hazards/`) y la escena de práctica `Level_1_1`. Cada elemento implementado tiene su propia ficha en `Docs/` con valores y configuración reales. Las descripciones y cifras sirven como referencia de diseño para reconstruirlos; habrá que verificarlas durante la nueva implementación.

**Estado:** 82 fichas planificadas (81 elementos del entorno y Alma), de las que 4 trampas están descartadas; 43 prefabs implementados en esta rama (más los prefabs base de trampas, plataformas que se desmoronan, barreras Dash, altares y huevos, y `System_GameProgress`): Alma, en `Assets/Prefabs/Player/Alma.prefab` (no en la ruta propuesta `Player_Alma.prefab`), ver [Alma](Player/Alma.md); y la planta carnívora, en `Assets/Prefabs/Enemies/Plant_Carnivorous_Jungle/Plant_Carnivorous.prefab`, ver [Planta carnívora](Enemies/Plant_Carnivorous_Jungle.md); y el escarabajo de cristal, en `Assets/Prefabs/Enemies/CrystalBeetle_Caves/CrystalBeetle.prefab`, ver [Escarabajo de cristal](Enemies/CrystalBeetle_Caves.md); y el murciélago de cueva, en `Assets/Prefabs/Enemies/CaveBat_Caves/CaveBat_Caves.prefab`, ver [Murciélago de cueva](Enemies/CaveBat_Caves.md); y el sapo venenoso, en `Assets/Prefabs/Enemies/PoisonToad_Swamp/PoisonToad_Swamp.prefab`, ver [Sapo venenoso](Enemies/PoisonToad_Swamp.md); y la salamandra de magma, en `Assets/Prefabs/Enemies/MagmaSalamander_Volcano/MagmaSalamander_Volcano.prefab`, ver [Salamandra de magma](Enemies/MagmaSalamander_Volcano.md); y doce trampas (nueve estáticas, gas tóxico ascendente, géiser y techo aplastante) en `Assets/Prefabs/Level/Hazards/`, con marcador provisional, ver [Zonas de peligro](LevelPieces/Hazards.md); y diez piezas de nivel (plataforma atravesable, tres plataformas que se desmoronan, hongo saltarín, piso rompible, espora del Dash, barrera de cañas, reja Dash y corriente de viento) en `Assets/Prefabs/Level/Pieces/`, ver [Piezas de nivel](LevelPieces/Pieces.md); y la roca de basalto movible y el conjunto del balancín (balancín, contrapeso, runa y compuerta rúnica) en `Assets/Prefabs/Level/Puzzles/`, ver [Puzles](LevelPieces/Puzzles.md); y el nido de checkpoint, los cuatro altares de habilidad, los cuatro huevos y el portal de salida en `Assets/Prefabs/Level/Progression/`, ver [Progresión](LevelPieces/Progression.md). El guardado y el gestor de progreso están en `Assets/Systems/`, ver [Guardado y progreso](Systems/SaveAndProgress.md). Las carpetas y los nombres indicados son rutas propuestas.

## Reglas de daño y funcionamiento

- Alma no tiene actualmente un sistema de HP ni daño por puntos. Los peligros activos detectados por `PlayerCollisionService2D` ejecutan `KillAndRespawn()` y devuelven a Alma al último checkpoint.
- «Letal» indica esa muerte/reaparición; no significa «quita 1 HP» ni una cantidad configurable de salud.
- Un peligro condicional solo mata cuando `IsDangerous` es verdadero; algunos enemigos y proyectiles tienen ventanas seguras o contraataques válidos.
- Los recursos indican **0 daño directo**. Hundirse, colapsar, empujar o activar una erupción puede exponer a Alma a otro peligro, pero el recurso no es por ello un atacante.
- Los jefes progresan por impactos/condiciones válidas de sus puzzles, no por una barra de HP numérica.
- Los valores provienen del prototipo retirado. Son referencias históricas y pueden cambiar al reconstruir cada elemento.
- Cada futuro elemento visual deberá exponer un campo de sprite; Alma necesitará soporte para sus animaciones. Las láminas conceptuales están en `Docs/Art/`.
- El juego permanece sin sonido. Campanas, Rugido, ataques y rescates usan feedback visual y gameplay.

## Convención de nombres y carpetas

En el proyecto futuro, agrupar cada elemento en su propia carpeta con prefab, scripts, imágenes, animaciones y configuraciones propias. Compartir únicamente las piezas comunes a varios elementos. El campo de sprite debe quedar expuesto para asignar arte después.

- Archivos en inglés, sin espacios ni tildes, usando `Tipo_Identidad_Contexto.prefab`.
- Un nombre identifica una función estable. Ejemplos: `Enemy_PoisonToad_Swamp.prefab` y `Trap_FireGeyser_Volcano.prefab`.
- Usar variantes de prefab para ajustes reutilizables; posiciones y dimensiones particulares de un nivel quedan como overrides.
- No usar nombres de colocación como `Platform_42`, `Enemy_3` o el nombre de un nivel para el prefab base.
- Los conjuntos completos de puzzles/jefes están en `Resources/Assemblies` y `Bosses/Assemblies`; una pieza con referencias a otra requiere conexiones o colocación del conjunto completo.

| Categoría | Carpeta objetivo | Fichas |
|---|---|---:|
| Enemigos | `Assets/_Project/Prefabs/Enemies/` | 5 |
| Trampas | `Assets/_Project/Prefabs/Traps/` | 16 |
| Proyectiles | `Assets/_Project/Prefabs/Projectiles/` | 7 |
| Jefes | `Assets/_Project/Prefabs/Bosses/` | 4 |
| Recursos | `Assets/_Project/Prefabs/Resources/` | 41 |
| Narrativa y apoyo | `Assets/_Project/Prefabs/Narrative/` | 8 |
| Jugador | `Assets/_Project/Prefabs/Player/` | 1 |

## Índice de nombres definitivos

| Elemento | Categoría | Archivo definitivo |
|---|---|---|
| Alma | Jugador | `Player_Alma.prefab` |
| Planta carnívora | Enemigos | `Plant_Carnivorous_Jungle.prefab` |
| Escarabajo de cristal | Enemigos | `Enemy_CrystalBeetle_Caves.prefab` |
| Murciélago de cueva | Enemigos | `Enemy_CaveBat_Caves.prefab` |
| Sapo venenoso | Enemigos | `Enemy_PoisonToad_Swamp.prefab` |
| Salamandra de magma | Enemigos | `Enemy_MagmaSalamander_Volcano.prefab` |
| Pinchos de jungla | Trampas | `Trap_Spikes_Jungle.prefab` |
| Zarzas del pantano | Trampas | `Trap_Briers_Swamp.prefab` |
| Pilar con espinas | Trampas | `Trap_SpikedPillar_Jungle.prefab` |
| Cristales punzantes y estalagmitas | Trampas | `Trap_CrystalSpikes_Caves.prefab` |
| Pinchos ardientes | Trampas | `Trap_BurningSpikes_Volcano.prefab` |
| Lodo tóxico | Trampas | `Trap_ToxicMud_Swamp.prefab` |
| Lago tóxico | Trampas | `Trap_ToxicLake_Swamp.prefab` |
| Gas tóxico ascendente | Trampas | `Trap_RisingToxicGas_Swamp.prefab` |
| Río o foso de lava | Trampas | `Trap_LavaPool_Volcano.prefab` |
| Erupción de lava ascendente | Trampas | `Trap_RisingLavaEruption_Volcano.prefab` |
| Magma ascendente del jefe final | Trampas | `Trap_RisingMagma_FinalBoss.prefab` |
| Techo aplastante | Trampas | `Trap_CrushingCeiling_Caves.prefab` |
| Géiser volcánico peligroso | Trampas | `Trap_FireGeyser_Volcano.prefab` |
| Chorro de fuego de aterrizaje | Trampas | `Trap_LandingFlameJet_Volcano.prefab` |
| Puerta de llamas | Trampas | `Trap_BellFlameDoor_Volcano.prefab` |
| Zona de caída mortal | Trampas | `Trap_DeathZone_Universal.prefab` |
| Fruto rodante | Proyectiles | `Projectile_RollingFruit_Jungle.prefab` |
| Cristal que cae | Proyectiles | `Projectile_FallingCrystal_Caves.prefab` |
| Burbuja venenosa | Proyectiles | `Projectile_PoisonBubble_Swamp.prefab` |
| Bola de fuego | Proyectiles | `Projectile_MagmaFireball_Volcano.prefab` |
| Meteorito reflejable de compuerta | Proyectiles | `Projectile_ReflectableMeteor_Volcano.prefab` |
| Meteorito de contraataque final | Proyectiles | `Projectile_CounterMeteor_FinalBoss.prefab` |
| Roca de lluvia de meteoritos | Proyectiles | `Projectile_MeteorRainRock_FinalBoss.prefab` |
| Rey de la Copa — Mono Ladrón Gigante | Jefes | `Boss_GiantMonkey_Jungle.prefab` |
| Acorazado Subterráneo — Armadillo Prehistórico | Jefes | `Boss_PrehistoricArmadillo_Caves.prefab` |
| Señor de las Ráfagas — Pterodáctilo Alfa | Jefes | `Boss_AlphaPterodactyl_Swamp.prefab` |
| Rey Ladrón — Tirano Ancestral | Jefes | `Boss_ThiefKing_Volcano.prefab` |
| Plataforma sólida | Recursos | `Platform_Solid_Universal.prefab` |
| Plataforma atravesable desde abajo | Recursos | `Platform_OneWay_Universal.prefab` |
| Hongo saltarín | Recursos | `Resource_BouncyMushroom_Jungle.prefab` |
| Cristal de rebote | Recursos | `Resource_BouncyCrystal_Caves.prefab` |
| Respiradero de vapor de rebote | Recursos | `Resource_SteamVent_Volcano.prefab` |
| Hoja que se desmorona | Recursos | `Platform_CrumblingLeaf_Jungle.prefab` |
| Nenúfar que se desmorona | Recursos | `Platform_CrumblingLilypad_Swamp.prefab` |
| Cornisa que colapsa | Recursos | `Platform_CrumblingLedge_Volcano.prefab` |
| Basalto que se hunde | Recursos | `Platform_SinkingBasalt_Volcano.prefab` |
| Piso rompible con Pisotón | Recursos | `Resource_PoundBreakableFloor_Universal.prefab` |
| Pilar sísmico rompible | Recursos | `Resource_SeismicPillar_Volcano.prefab` |
| Sello de erupción rompible | Recursos | `Resource_EruptionSeal_Volcano.prefab` |
| Barrera de cañas rompible | Recursos | `Resource_DashReedBarrier_Swamp.prefab` |
| Reja rompible con Dash | Recursos | `Resource_DashTrialGrid_Volcano.prefab` |
| Balancín de lanzamiento | Recursos | `Resource_SeesawCatapult_Caves.prefab` |
| Catapulta de raíces | Recursos | `Resource_RootCatapult_Swamp.prefab` |
| Contrapeso de catapulta | Recursos | `Resource_CatapultCounterweight_Caves.prefab` |
| Corriente de viento | Recursos | `Resource_WindCurrent_Universal.prefab` |
| Espora de recarga de Dash | Recursos | `Resource_DashRefillSpore_Swamp.prefab` |
| Roca de basalto movible | Recursos | `Resource_RoarBoulder_Volcano.prefab` |
| Interruptor rúnico | Recursos | `Resource_RuneSwitch_Caves.prefab` |
| Compuerta rúnica temporizada | Recursos | `Resource_TimedRuneGate_Caves.prefab` |
| Campana de resonancia | Recursos | `Resource_ResonanceBell_Volcano.prefab` |
| Compuerta de impacto de meteorito | Recursos | `Resource_MeteorImpactGate_Volcano.prefab` |
| Control de pruebas del templo | Recursos | `Resource_TempleTrialGate_Volcano.prefab` |
| Sello de contraataque con Dash | Recursos | `Resource_DashHeatSeal_FinalBoss.prefab` |
| Placa dorsal vulnerable | Recursos | `Resource_WeakDorsalPlate_FinalBoss.prefab` |
| Sello de vapor rompible | Recursos | `Resource_SteamSeal_FinalBoss.prefab` |
| Anclaje de estalactita | Recursos | `Resource_StalactiteAnchor_FinalBoss.prefab` |
| Estalactita colosal de remate | Recursos | `Resource_ColossalStalactite_FinalBoss.prefab` |
| Altar de Doble Salto | Recursos | `Resource_AbilityAltar_DoubleJump.prefab` |
| Altar de Pisotón | Recursos | `Resource_AbilityAltar_GroundPound.prefab` |
| Altar de Dash | Recursos | `Resource_AbilityAltar_AirDash.prefab` |
| Altar de Rugido | Recursos | `Resource_AbilityAltar_Roar.prefab` |
| Nido / checkpoint | Recursos | `Resource_CheckpointNest_Universal.prefab` |
| Huevo verde | Recursos | `Resource_RescueEgg_Green.prefab` |
| Huevo azul | Recursos | `Resource_RescueEgg_Blue.prefab` |
| Huevo morado | Recursos | `Resource_RescueEgg_Purple.prefab` |
| Huevo rojo | Recursos | `Resource_RescueEgg_Red.prefab` |
| Santuario de rescate | Recursos | `Resource_EggSanctuary_Universal.prefab` |
| Portal de salida | Recursos | `Resource_LevelExitPortal_Universal.prefab` |
| Mono ladrón de presentación | Narrativa y apoyo | `NPC_ThiefMonkeyTeaser_Jungle.prefab` |
| Silueta de guardián Armadillo | Narrativa y apoyo | `Narrative_GuardianTeaser_Armadillo.prefab` |
| Silueta de guardián Pterodactyl | Narrativa y apoyo | `Narrative_GuardianTeaser_Pterodactyl.prefab` |
| Silueta de guardián Thief_King | Narrativa y apoyo | `Narrative_GuardianTeaser_Thief_King.prefab` |
| Mensaje de prólogo | Narrativa y apoyo | `Narrative_PrologueTrigger_Universal.prefab` |
| Onda visual de Rugido | Narrativa y apoyo | `VFX_RoarWave_Universal.prefab` |
| Fondo con parallax | Narrativa y apoyo | `Scenery_ParallaxLayer_Universal.prefab` |
| Decoración visual | Narrativa y apoyo | `Scenery_Decoration_Universal.prefab` |

## Enemigos

### 1. Planta carnívora

- **Estado:** implementada. Ficha con valores reales: [Planta carnívora](Enemies/Plant_Carnivorous_Jungle.md).
- **Archivo definitivo:** `Plant_Carnivorous_Jungle.prefab`.
- **Ruta real:** `Assets/Prefabs/Enemies/Plant_Carnivorous_Jungle/Plant_Carnivorous.prefab` (nombre y ruta propuestos: `Assets/_Project/Prefabs/Enemies/Plant_Carnivorous_Jungle.prefab`).
- **Implementación:** `CarnivorousPlant2D`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** Enemigo vegetal fijo que muerde por ciclos.
- **Cómo funciona:** Reposo seguro → aviso (gira hacia el lado de Alma y parpadea en rojo) → mordisco a izquierda o derecha con cabeza letal y una onda de impacto por el suelo que mata al pasar → reapertura.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Implementado: detección 3.3 u a cada lado; reposo 1 s; aviso 0.25 s; mordisco 0.7 s; reapertura 0.2 s; onda de 2.2 u en 0.2 s y 1.2 u de alto. Valores originales del prototipo: reposo 2 s; aviso 0.5 s; mordisco 1.2 s; reapertura 0.2 s.
- **Conexiones, variantes o límites:** Durante el reposo es segura. Su posición fija no la convierte en pinchos: tiene comportamiento propio de ataque.

### 2. Escarabajo de cristal

- **Estado:** implementado. Ficha con valores reales: [Escarabajo de cristal](Enemies/CrystalBeetle_Caves.md).
- **Archivo definitivo:** `Enemy_CrystalBeetle_Caves.prefab`.
- **Ruta real:** `Assets/Prefabs/Enemies/CrystalBeetle_Caves/CrystalBeetle.prefab` (nombre y ruta propuestos: `Assets/_Project/Prefabs/Enemies/Enemy_CrystalBeetle_Caves.prefab`).
- **Implementación:** `CrystalBeetle2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Enemigo terrestre que patrulla entre límites.
- **Cómo funciona:** Patrulla entre dos puntos reflejándose en cada extremo; tocarlo (también saltarle encima) mata a Alma. Un Pisotón a 2 m o menos lo voltea con un salto y media vuelta; patas arriba es inofensivo, su vientre es plataforma y su caminata se pausa. Tras 3,5 s tiembla, se da la vuelta y sigue patrullando. Si Alma está a 5 u o menos, se detiene, se gira hacia ella y le lanza un cristal en arco cada 2,5 s (con 0,5 s de aviso). El Rugido no le afecta.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Implementado (en el componente, sin asset de configuración): velocidad 2 u/s; disparo cada 2.5 s en un radio de 5 u; volteado 3.5 s; radio del Pisotón 2 m; salto de volteo de 0.6 u en 0.4 s; patrulla A–B en cada instancia. Prototipo: CrystalEnemyConfig.asset con impulso al voltearse de 1.5 u/s.
- **Conexiones, variantes o límites:** IsDangerous es falso mientras está volteado. El Pisotón lo neutraliza temporalmente; no utiliza barra de vida.

### 3. Murciélago de cueva

- **Estado:** implementado. Ficha con valores reales: [Murciélago de cueva](Enemies/CaveBat_Caves.md).
- **Archivo definitivo:** `Enemy_CaveBat_Caves.prefab`.
- **Ruta real:** `Assets/Prefabs/Enemies/CaveBat_Caves/CaveBat_Caves.prefab` (nombre y ruta propuestos: `Assets/_Project/Prefabs/Enemies/Enemy_CaveBat_Caves.prefab`).
- **Implementación:** `CaveBat2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Enemigo aéreo con ataques de ida y vuelta.
- **Cómo funciona:** Implementado: patrulla en el aire entre dos puntos; si Alma pasa por debajo a 4 u o menos, se detiene, tiembla y parpadea en rojo 0,65 s, y se lanza en un picado en U de 1,6 s a través de donde estaba ella; luego revolotea 2 s y sigue patrullando. Diseño original: dormía colgado del techo y despertaba al pasar Alma.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** CrystalEnemyConfig.asset: detección horizontal 4 unidades; aviso 0.65 s; vuelo 1.6 s; descanso 2 s; ancho del arco 2.2 unidades. Profundidad de picado configurable en el componente.
- **Conexiones, variantes o límites:** Implementado: tocar su cuerpo mata siempre (no las alas). Diseño original: solo era peligroso durante el vuelo de ataque.

### 4. Sapo venenoso

- **Estado:** implementado. Ficha con valores reales: [Sapo venenoso](Enemies/PoisonToad_Swamp.md).
- **Archivo definitivo:** `Enemy_PoisonToad_Swamp.prefab`.
- **Ruta real:** `Assets/Prefabs/Enemies/PoisonToad_Swamp/PoisonToad_Swamp.prefab` (nombre y ruta propuestos: `Assets/_Project/Prefabs/Enemies/Enemy_PoisonToad_Swamp.prefab`).
- **Implementación:** `PoisonToad2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Enemigo estacionario que dispara veneno.
- **Cómo funciona:** Compara su X con la de Alma, elige izquierda/derecha y lanza una burbuja tras el aviso.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** PoisonToadConfig.asset: intervalo 2 s; aviso 0.6 s; detección 18 unidades; velocidad horizontal de burbuja 8; vertical 3; gravedad 0.6; duración 3 s; pool de 4.
- **Conexiones, variantes o límites:** El cuerpo también es peligroso. El Dash no concede inmunidad al veneno. Necesita referencia a Alma y al prefab de burbuja. Implementado: valores en el componente (sin `PoisonToadConfig.asset`), detección 8 unidades en vez de 18, gravedad 6 u/s², glob y partículas creados por código sin prefab ni pool.

### 5. Salamandra de magma

- **Estado:** implementada (sin aturdimiento). Ficha con valores reales: [Salamandra de magma](Enemies/MagmaSalamander_Volcano.md).
- **Archivo definitivo:** `Enemy_MagmaSalamander_Volcano.prefab`.
- **Ruta real:** `Assets/Prefabs/Enemies/MagmaSalamander_Volcano/MagmaSalamander_Volcano.prefab` (nombre y ruta propuestos: `Assets/_Project/Prefabs/Enemies/Enemy_MagmaSalamander_Volcano.prefab`).
- **Implementación:** `MagmaSalamander2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Enemigo móvil que dispara fuego dirigido.
- **Cómo funciona:** Recorre sus puntos de ruta y apunta hacia la posición de Alma. Rugido o choque sísmico la aturden y limpian sus proyectiles.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** MagmaSalamanderConfig.asset: movimiento 2.2 unidades/s; intervalo 2.2 s; aviso 0.65 s; detección 8 unidades; proyectil 7 unidades/s y 2 s de vida; aturdimiento 3 s; retroceso 6 unidades/s.
- **Conexiones, variantes o límites:** Mientras está aturdida deja de ser peligrosa. Necesita ruta, configuración, Alma y prefab de bola de fuego. Implementado: patrulla entre dos puntos, se detiene a disparar cuando Alma está a 8 u por cualquier lado y sigue caminando cuando se va; valores en el componente (sin `MagmaSalamanderConfig.asset`), bola y partículas creadas por código sin prefab; el aturdimiento con Rugido o Pisotón está pendiente.


## Trampas

### 1. Pinchos de jungla

- **Estado:** implementado con marcador provisional (caja de color y etiqueta). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_Spikes_Jungle.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_Spikes_Jungle.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_Spikes_Jungle.prefab`.
- **Implementación:** `HazardTrigger2D`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** Espinas estáticas en suelo, techo o paredes.
- **Cómo funciona:** El trigger detecta contacto con Alma.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Tamaño, orientación y posición del collider. Sin ciclo de ataque.

### 2. Zarzas del pantano

- **Estado:** implementado con marcador provisional (caja de color y etiqueta). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_Briers_Swamp.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_Briers_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_Briers_Swamp.prefab`.
- **Implementación:** `HazardTrigger2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Variante vegetal estática de pinchos.
- **Cómo funciona:** Contacto con el volumen de peligro.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Valores de colocación y dimensiones configurables en cada instancia.
- **Conexiones, variantes o límites:** Existe como prefab disponible; su presencia concreta depende de la escena. No confundir con las cañas rompibles por Dash.

### 3. Pilar con espinas

- **Estado:** implementado con marcador provisional (caja de color y etiqueta). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_SpikedPillar_Jungle.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_SpikedPillar_Jungle.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_SpikedPillar_Jungle.prefab`.
- **Implementación:** `HazardTrigger2D + colliders de estructura`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** Estructura sólida con una zona punzante.
- **Cómo funciona:** El tronco sostiene/bloquea; el componente de peligro en la parte espinosa mata al tocarlo.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Valores de colocación y dimensiones configurables en cada instancia.
- **Conexiones, variantes o límites:** Las torres y dientes de distintas alturas son variantes de esta familia, no especies de enemigo distintas.

### 4. Cristales punzantes y estalagmitas

- **Estado:** implementado con marcador provisional (caja de color y etiqueta). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_CrystalSpikes_Caves.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_CrystalSpikes_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_CrystalSpikes_Caves.prefab`.
- **Implementación:** `HazardTrigger2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Cristales estáticos que cubren fosos y pasajes.
- **Cómo funciona:** Contacto con el trigger del lecho de cristales.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Longitud del lecho, altura y collider; las instancias pueden incluir varias puntas visuales.

### 5. Pinchos ardientes

- **Estado:** implementado con marcador provisional (caja de color y etiqueta). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_BurningSpikes_Volcano.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_BurningSpikes_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_BurningSpikes_Volcano.prefab`.
- **Implementación:** `HazardTrigger2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Variante de pinchos en la fractura volcánica.
- **Cómo funciona:** Contacto con el volumen Burning_Spikes_0.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Valores de colocación y dimensiones configurables en cada instancia.
- **Conexiones, variantes o límites:** Se localiza en la escena indicada; el catálogo agrupa su estructura con otros peligros. Ya dispone del prefab independiente indicado; conectar sus dependencias al colocarlo.

### 6. Lodo tóxico

- **Estado:** implementado con marcador provisional (caja de color y etiqueta). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_ToxicMud_Swamp.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_ToxicMud_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_ToxicMud_Swamp.prefab`.
- **Implementación:** `HazardTrigger2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Superficie contaminada bajo los saltos.
- **Cómo funciona:** Contacto con los volúmenes Toxic_Mud_*.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Valores de colocación y dimensiones configurables en cada instancia.
- **Conexiones, variantes o límites:** Familia compartida de peligros estáticos; las dimensiones y el aspecto diferencian sus instancias.

### 7. Lago tóxico

- **Estado:** implementado con marcador provisional (caja de color y etiqueta). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_ToxicLake_Swamp.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_ToxicLake_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_ToxicLake_Swamp.prefab`.
- **Implementación:** `HazardTrigger2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Lago letal atravesado mediante saltos, Dash y esporas.
- **Cómo funciona:** Contacto con los volúmenes Spore_Lake_*.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Valores de colocación y dimensiones configurables en cada instancia.
- **Conexiones, variantes o límites:** Variante de superficie tóxica con nombre propio para localizarla y reutilizarla.

### 8. Gas tóxico ascendente

- **Estado:** implementado con marcador provisional y efectos de partículas generados por código. Prefab real: `Assets/Prefabs/Level/Hazards/Trap_RisingToxicGas_Swamp.prefab`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md#trampas-dinámicas).
- **Archivo definitivo:** `Trap_RisingToxicGas_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_RisingToxicGas_Swamp.prefab`.
- **Implementación:** `RisingHazardFloor2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Volumen de gas que obliga a ascender.
- **Cómo funciona:** Su ciclo avisa, arma la subida y mueve el collider con física cinemática; el rescate puede detenerlo.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** RisingGasConfig.asset: aviso 3 s; subida 0.9 unidades/s; separación respecto al checkpoint 4 unidades. Límites y condición de parada pertenecen al encuentro.

### 9. Río o foso de lava

- **Estado:** implementado con arte final redimensionable (tile 9-slice repetido, se adapta a cualquier tamaño). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_LavaPool_Volcano.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_LavaPool_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_LavaPool_Volcano.prefab`.
- **Implementación:** `HazardTrigger2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Superficie de magma estática.
- **Cómo funciona:** Contacto con el trigger de lava.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Ancho, profundidad, posición, sprite y luz de la instancia.
- **Conexiones, variantes o límites:** Puede combinarse con rocas movibles para formar puentes; la lava sigue siendo el elemento dañino.

### 10. Erupción de lava ascendente

- **Estado:** descartada (5/10/2026). No se implementará; los tramos que la usaban se rediseñarán con las trampas implementadas ([Zonas de peligro](LevelPieces/Hazards.md)).
- **Archivo definitivo:** `Trap_RisingLavaEruption_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_RisingLavaEruption_Volcano.prefab`.
- **Implementación:** `FractureEruption2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Erupción activada mediante un sello rompible.
- **Cómo funciona:** Romper el sello con Pisotón abre la barrera, inicia aviso y eleva el volumen de lava hasta la zona de escape.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** FractureConfig.asset: aviso 1.4 s; subida 0.9 unidades/s; altura inicial -3.2; techo 3.4 en el recorrido original.
- **Conexiones, variantes o límites:** Las alturas son coordenadas del encuentro original y deben ajustarse al moverlo. Necesita sello, barrera, Alma y referencias visuales.

### 11. Magma ascendente del jefe final

- **Estado:** descartada (5/10/2026). No se implementará; los tramos que la usaban se rediseñarán con las trampas implementadas ([Zonas de peligro](LevelPieces/Hazards.md)).
- **Archivo definitivo:** `Trap_RisingMagma_FinalBoss.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_RisingMagma_FinalBoss.prefab`.
- **Implementación:** `Controlado por ThiefKingBoss2D`.
- **Mundo/contexto:** 4 / jefe final.
- **Qué es / para qué sirve:** Magma que presiona la fase de ascenso.
- **Cómo funciona:** El controlador del jefe mueve Rising_Caldera_Magma durante la fase correspondiente.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** ThiefKingConfig.asset: retraso de lava 7 s y subida 0.38 unidades/s.
- **Conexiones, variantes o límites:** Pieza integrada en el conjunto del jefe final; no es una segunda implementación independiente de FractureEruption2D.

### 12. Techo aplastante

- **Estado:** implementado con marcador provisional y efectos de partículas generados por código. Prefab real: `Assets/Prefabs/Level/Hazards/Trap_CrushingCeiling_Caves.prefab`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md#trampas-dinámicas).
- **Archivo definitivo:** `Trap_CrushingCeiling_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_CrushingCeiling_Caves.prefab`.
- **Implementación:** `CrushingCeiling2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Techo móvil que cierra un corredor.
- **Cómo funciona:** Apertura → aviso → descenso → cierre → retirada.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** CrushingCeilingConfig.asset: abierto 3 s, con aviso durante sus últimos 0.8 s; descenso 0.6 s; cerrado 0.4 s; retirada 2 s. Posición de cierre y activación configurables.
- **Conexiones, variantes o límites:** Su IsDangerous es siempre verdadero: tocar el techo físico es letal en cualquier fase. Las fases regulan su posición, no una inmunidad al contacto.

### 13. Géiser volcánico peligroso

- **Estado:** implementado con marcador provisional y efectos de partículas generados por código. Prefab real: `Assets/Prefabs/Level/Hazards/Trap_FireGeyser_Volcano.prefab`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md#trampas-dinámicas).
- **Archivo definitivo:** `Trap_FireGeyser_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_FireGeyser_Volcano.prefab`.
- **Implementación:** `LavaGeyser2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Columna intermitente de fuego/vapor que bloquea el paso.
- **Cómo funciona:** Reposo → aviso → erupción; el collider solo se activa durante la erupción.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** GeyserConfig.asset: reposo 2.2 s; aviso 0.8 s; erupción 1.2 s.
- **Conexiones, variantes o límites:** Este géiser hace daño. El respiradero de rebote Resource_SteamVent_Volcano es un recurso diferente.

### 14. Chorro de fuego de aterrizaje

- **Estado:** descartada (5/10/2026). No se implementará; los tramos que la usaban se rediseñarán con las trampas implementadas ([Zonas de peligro](LevelPieces/Hazards.md)).
- **Archivo definitivo:** `Trap_LandingFlameJet_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_LandingFlameJet_Volcano.prefab`.
- **Implementación:** `FractureFlameJet2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Chorro que pone presión sobre una cornisa de llegada.
- **Cómo funciona:** El ciclo avanza al entrar Alma en sus límites de X; muestra aviso antes de encenderse.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** FractureConfig.asset: recuperación 2 s; aviso 1.2 s; fuego 1.1 s. Límites de activación en la instancia.

### 15. Puerta de llamas

- **Estado:** descartada (5/10/2026). No se implementará; los tramos que la usaban se rediseñarán con las trampas implementadas ([Zonas de peligro](LevelPieces/Hazards.md)).
- **Archivo definitivo:** `Trap_BellFlameDoor_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_BellFlameDoor_Volcano.prefab`.
- **Implementación:** `BellFlameDoor2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Barrera de fuego controlada por una campana.
- **Cómo funciona:** Permanece peligrosa hasta que la campana vinculada abre su ventana de supresión.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Duración de paso determinada por ResonanceBellConfig.asset: 5 s en la configuración actual.
- **Conexiones, variantes o límites:** Necesita su campana; la puerta hace daño y la campana no.

### 16. Zona de caída mortal

- **Estado:** implementado con marcador provisional (caja de color y etiqueta). Prefab real: `Assets/Prefabs/Level/Hazards/Trap_DeathZone_Universal.prefab`, variante de `Hazard_Base` con el script común `HazardZone2D`. Ficha: [Zonas de peligro](LevelPieces/Hazards.md).
- **Archivo definitivo:** `Trap_DeathZone_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Traps/Trap_DeathZone_Universal.prefab`.
- **Implementación:** `HazardTrigger2D`.
- **Mundo/contexto:** 1–4.
- **Qué es / para qué sirve:** Volumen normalmente invisible bajo el nivel.
- **Cómo funciona:** Al entrar Alma, activa muerte y respawn.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Ancho, altura y profundidad de colocación.
- **Conexiones, variantes o límites:** No requiere sprite visible. Los nombres DeadZone_KillFloor, Abyss_Death_Zone y Deep_Fall_Death_Zone son instancias de esta función.


## Proyectiles

### 1. Fruto rodante

- **Archivo definitivo:** `Projectile_RollingFruit_Jungle.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Projectiles/Projectile_RollingFruit_Jungle.prefab`.
- **Implementación:** `RollingFruitProjectile2D`.
- **Mundo/contexto:** 1 / jefe.
- **Qué es / para qué sirve:** Fruto físico arrojado por el mono.
- **Cómo funciona:** Se lanza desde el jefe, rueda y rebota con Rigidbody2D; detecta contacto dañino.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Velocidades y duración definidas por el ataque/componente; prefab: gravedad 1.6 y rebote vertical configurado 6.
- **Conexiones, variantes o límites:** Es un proyectil del enemigo, no otro NPC.

### 2. Cristal que cae

- **Archivo definitivo:** `Projectile_FallingCrystal_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Projectiles/Projectile_FallingCrystal_Caves.prefab`.
- **Implementación:** `BossFallingCrystal2D`.
- **Mundo/contexto:** 2 / jefe.
- **Qué es / para qué sirve:** Estalactita/cristal que cae tras un marcador de aviso.
- **Cómo funciona:** Initialize recibe aviso y velocidad; finalizado el aviso, cae verticalmente y oculta el marcador.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** ArmadilloBossConfig.asset: aviso 0.9 s; caída 10 unidades/s; intervalo de ataque 2 s.
- **Conexiones, variantes o límites:** El prefab no debe colocarse como ataque listo sin inicializarlo desde el controlador.

### 3. Burbuja venenosa

- **Estado:** implementada sin prefab como `PoisonSpit2D` (glob de veneno creado por código por el sapo, sin pool). Valores reales en la ficha del [Sapo venenoso](Enemies/PoisonToad_Swamp.md#el-glob-de-veneno): 8 / 3 u/s, gravedad 6 u/s², 3 s, partículas de goteo y salpicadura.
- **Archivo definitivo:** `Projectile_PoisonBubble_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Projectiles/Projectile_PoisonBubble_Swamp.prefab`.
- **Implementación:** `PoisonBubble2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Proyectil venenoso del sapo.
- **Cómo funciona:** Launch configura posición, velocidad, gravedad y duración; el pool lo limpia y reutiliza.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** Con el sapo actual: horizontal 8; vertical 3; gravedad 0.6; duración 3 s.
- **Conexiones, variantes o límites:** Es peligrosa mientras está activa; no posee IA propia.

### 4. Bola de fuego

- **Estado:** implementada sin prefab como `MagmaFireball2D` (script propio, no reutiliza el del veneno; creada por código por la salamandra, sin pool). Valores reales en la ficha de la [Salamandra de magma](Enemies/MagmaSalamander_Volcano.md#la-bola-de-fuego): recta a 7 u/s hacia Alma (máx. ±40°), 2 s, estela de llamas y chispas.
- **Archivo definitivo:** `Projectile_MagmaFireball_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Projectiles/Projectile_MagmaFireball_Volcano.prefab`.
- **Implementación:** `PoisonBubble2D reutilizado con aspecto de fuego`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Proyectil dirigido de la salamandra.
- **Cómo funciona:** Usa el mismo controlador de proyectil que el veneno, con trayectoria y valores de fuego.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** MagmaSalamanderConfig.asset: velocidad 7 unidades/s; gravedad 0 en el lanzamiento; duración 2 s.
- **Conexiones, variantes o límites:** Su nombre y arte deben identificar fuego aunque comparta código con las burbujas.

### 5. Meteorito reflejable de compuerta

- **Archivo definitivo:** `Projectile_ReflectableMeteor_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Projectiles/Projectile_ReflectableMeteor_Volcano.prefab`.
- **Implementación:** `ReflectableMeteor2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Meteorito peligroso que se convierte en herramienta de puzzle al devolverlo.
- **Cómo funciona:** Rugido orientado hacia su compuerta cambia la trayectoria, lo vuelve seguro para Alma y permite abrirla.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** FractureConfig.asset: aviso de lanzamiento 1.1 s; velocidad 1.8; devolución 12; duración 4 s.
- **Conexiones, variantes o límites:** Tras reflejarse, IsDangerous es falso. Necesita el vínculo de su MeteorImpactGate2D.

### 6. Meteorito de contraataque final

- **Archivo definitivo:** `Projectile_CounterMeteor_FinalBoss.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Projectiles/Projectile_CounterMeteor_FinalBoss.prefab`.
- **Implementación:** `ThiefKingMechanism2D + ThiefKingHazard2D`.
- **Mundo/contexto:** 4 / jefe final.
- **Qué es / para qué sirve:** Meteorito que se devuelve con Rugido durante el combate final.
- **Cómo funciona:** El mecanismo llama a ReflectMeteor; el controlador del jefe gestiona movimiento, fase y efecto del contraataque.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** ThiefKingConfig.asset: velocidad 3; devolución 12 unidades/s.
- **Conexiones, variantes o límites:** La condición de peligro se desactiva al reflejarlo. Es distinto del meteorito de compuerta aunque comparta la idea de puzzle.

### 7. Roca de lluvia de meteoritos

- **Archivo definitivo:** `Projectile_MeteorRainRock_FinalBoss.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Projectiles/Projectile_MeteorRainRock_FinalBoss.prefab`.
- **Implementación:** `ThiefKingHazard2D + controlador del jefe`.
- **Mundo/contexto:** 4 / jefe final.
- **Qué es / para qué sirve:** Roca de caída señalizada durante el asedio final.
- **Cómo funciona:** El jefe coloca el marcador y mueve la roca siguiendo su fase de ataque.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** ThiefKingConfig.asset: aviso 1.3 s; velocidad de roca 8 unidades/s.
- **Conexiones, variantes o límites:** Usar el conjunto del jefe para conservar su vínculo y su ciclo; no es un proyectil autónomo ya inicializado.


## Jefes

### 1. Rey de la Copa — Mono Ladrón Gigante

- **Archivo definitivo:** `Boss_GiantMonkey_Jungle.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Bosses/Boss_GiantMonkey_Jungle.prefab`.
- **Implementación:** `GiantMonkeyBoss2D + BossBodyHazard2D + BossHeadHurtbox2D`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** Jefe de frutos rodantes y ventanas de cansancio.
- **Cómo funciona:** Arroja frutos, desciende agotado y expone la cabeza; el golpe válido sobre la cabeza durante TiredDescent suma un impacto.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** 3 impactos para derrotarlo; ventanas de agotamiento 3.2 / 2.8 / 2.4 s en la configuración actual.
- **Conexiones, variantes o límites:** No usa daño numérico por ataque ni HP de desgaste. La victoria activa el portal al mundo 2. Conjunto actual: Bosses/Assemblies/World_1_Jungle/Boss_1_LevelExit2D_0.prefab.

### 2. Acorazado Subterráneo — Armadillo Prehistórico

- **Archivo definitivo:** `Boss_PrehistoricArmadillo_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Bosses/Boss_PrehistoricArmadillo_Caves.prefab`.
- **Implementación:** `PrehistoricArmadilloBoss2D + ArmadilloFight`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Jefe rodante que se expone al chocar.
- **Cómo funciona:** Provocar choque contra el extremo/pilar y realizar Pisotón directo en la coronilla durante su aturdimiento; ciclos posteriores añaden cristales y salto.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** 3 impactos válidos; rodar 7 unidades/s, +1.5 por impacto; aviso 1.2 s; aturdimiento 4.5 s; recuperación 1 s.
- **Conexiones, variantes o límites:** Contacto peligroso durante la rodada. Los saltos normales y ondas cercanas no sustituyen el Pisotón directo. Victoria abre mundo 3.

### 3. Señor de las Ráfagas — Pterodáctilo Alfa

- **Archivo definitivo:** `Boss_AlphaPterodactyl_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Bosses/Boss_AlphaPterodactyl_Swamp.prefab`.
- **Implementación:** `PterodactylBoss2D + PterodactylHead2D + PterodactylBody2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Jefe aéreo de vendaval y picado.
- **Cómo funciona:** Empuja con viento, anuncia picado y recibe un Dash aéreo frontal válido contra la cabeza; cada impacto modifica la arena.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** 3 impactos válidos; viento 3 s y aceleración 12; aviso 1.4 s; picado 7 unidades/s, +1 por impacto; recuperación 2.5 s; rebote de Alma 8.2.
- **Conexiones, variantes o límites:** Cabeza y cuerpo son peligrosos durante el picado, con excepción del contraataque válido. Dash terrestre, salto normal o Pisotón no cuentan como el golpe requerido.

### 4. Rey Ladrón — Tirano Ancestral

- **Archivo definitivo:** `Boss_ThiefKing_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Bosses/Boss_ThiefKing_Volcano.prefab`.
- **Implementación:** `ThiefKingBoss2D + ThiefKingFight`.
- **Mundo/contexto:** 4 / final.
- **Qué es / para qué sirve:** Encuentro final que combina Dash, Pisotón, Rugido y ascenso.
- **Cómo funciona:** Contrarrestar ataques para exponer la placa, golpearla, devolver meteorito y resolver ascenso/sello/anclaje para el remate.
- **Daño a Alma:** Letal cuando el peligro está activo: muerte y reaparición en el último checkpoint. No resta una cantidad de HP.
- **Valores y propiedades:** 3 impactos/fases de progreso; aviso 1.3 s; barrido 1.8 s; recuperación 4 s. Parámetros completos en ThiefKingConfig.asset.
- **Conexiones, variantes o límites:** La mandíbula, meteoritos y lava son los peligros del encuentro; no todas sus piezas visuales dañan. Conjunto actual: Bosses/Assemblies/World_4_Volcano/Boss_Final_ThiefKingBoss2D_0.prefab.


## Recursos

### 1. Plataforma sólida

- **Archivo definitivo:** `Platform_Solid_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Platform_Solid_Universal.prefab`.
- **Implementación:** `BoxCollider2D + SpriteRenderer`.
- **Mundo/contexto:** 1–4.
- **Qué es / para qué sirve:** Superficie estable para caminar, saltar o refugiarse.
- **Cómo funciona:** Su collider sólido sostiene y bloquea a Alma.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Posición, escala, tamaño del collider, material y sprite. Variantes visuales: roca, madera, ramas, basalto, suelo.
- **Conexiones, variantes o límites:** Las paredes sólidas usan esta misma familia con otra orientación/dimensión; no hacen falta scripts duplicados para cada muro.

### 2. Plataforma atravesable desde abajo

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Platform_OneWay_Universal.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Platform_OneWay_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Platform_OneWay_Universal.prefab`.
- **Implementación:** `BoxCollider2D + PlatformEffector2D`.
- **Mundo/contexto:** 1–4.
- **Qué es / para qué sirve:** Plataforma a la que se sube atravesándola desde abajo.
- **Cómo funciona:** El effector permite pasar desde abajo y ofrece apoyo al aterrizar encima.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Ángulo de superficie, opciones de PlatformEffector2D, collider y dimensiones.

### 3. Hongo saltarín

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Resource_BouncyMushroom_Jungle.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Resource_BouncyMushroom_Jungle.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_BouncyMushroom_Jungle.prefab`.
- **Implementación:** `BouncyPlatform2D`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** Trampolín vegetal.
- **Cómo funciona:** Contacto desde arriba aplica velocidad vertical de rebote y puede restaurar Doble Salto; compresión/estiramiento visual.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Prefab base: rebote 17 unidades/s; cooldown interno 0.15 s. Restauración de salto y parámetros de animación ajustables.

### 4. Cristal de rebote

- **Archivo definitivo:** `Resource_BouncyCrystal_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_BouncyCrystal_Caves.prefab`.
- **Implementación:** `BouncyPlatform2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Variante cristalina del trampolín.
- **Cómo funciona:** Reutiliza el rebote de BouncyPlatform2D con arte de cristal.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Prefab disponible: rebote 17 unidades/s.
- **Conexiones, variantes o límites:** Existe como prefab; no debe confundirse con los cristales punzantes ni con los cristales que caen.

### 5. Respiradero de vapor de rebote

- **Archivo definitivo:** `Resource_SteamVent_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_SteamVent_Volcano.prefab`.
- **Implementación:** `BouncyPlatform2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Trampolín seguro con temática de vapor.
- **Cómo funciona:** Lanza a Alma al tocarlo desde arriba.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Prefab genérico: rebote 17 unidades/s. El respiradero específico del jefe final usa 22.
- **Conexiones, variantes o límites:** No hace daño; es diferente de LavaGeyser2D. La versión final está desactivada hasta resolver su sello.

### 6. Hoja que se desmorona

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Platform_CrumblingLeaf_Jungle.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Platform_CrumblingLeaf_Jungle.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Platform_CrumblingLeaf_Jungle.prefab`.
- **Implementación:** `CrumblingPlatform2D`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** Apoyo temporal que exige avanzar rápido.
- **Cómo funciona:** Al aterrizar, tiembla; después oculta sprites y desactiva el collider; reaparece tras esperar.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Prefab base: colapso en 1 s; reaparición 2.5 s; intensidad de temblor 0.05.
- **Conexiones, variantes o límites:** Necesita contexto de respawn si se quiere restablecer inmediatamente al morir.

### 7. Nenúfar que se desmorona

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Platform_CrumblingLilypad_Swamp.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Platform_CrumblingLilypad_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Platform_CrumblingLilypad_Swamp.prefab`.
- **Implementación:** `CrumblingPlatform2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Apoyo temporal del pantano.
- **Cómo funciona:** Mismo ciclo de colapso que las hojas, sobre lagos o fosos.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Prefab base: colapso 0.65 s; reaparición 2.5 s.

### 8. Cornisa que colapsa

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Platform_CrumblingLedge_Volcano.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Platform_CrumblingLedge_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Platform_CrumblingLedge_Volcano.prefab`.
- **Implementación:** `CrumblingPlatform2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Cornisa temporal de la fractura/ascenso.
- **Cómo funciona:** Se rompe después de pisarla y restaura su estado según el ciclo/respawn.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tiempo de colapso y reaparición por instancia; no asumir los tiempos de la hoja para todas las cornisas.

### 9. Basalto que se hunde

- **Archivo definitivo:** `Platform_SinkingBasalt_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Platform_SinkingBasalt_Volcano.prefab`.
- **Implementación:** `SinkingBasalt2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Plataforma que pierde altura al soportar peso.
- **Cómo funciona:** Desciende mientras recibe contactos desde arriba y se recupera al quedar libre.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Prefab: descenso 1.2 unidades/s; profundidad máxima 2 unidades; recuperación 0.8 unidades/s.
- **Conexiones, variantes o límites:** Disponible como prefab; su comportamiento no mata por sí mismo, aunque puede acercar a Alma a lava.

### 10. Piso rompible con Pisotón

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Resource_PoundBreakableFloor_Universal.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Resource_PoundBreakableFloor_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_PoundBreakableFloor_Universal.prefab`.
- **Implementación:** `BreakableGround2D`.
- **Mundo/contexto:** 2–4.
- **Qué es / para qué sirve:** Suelo agrietado que abre un pasaje.
- **Cómo funciona:** Impacto de Pisotón desde arriba llama a Break, desactiva el objeto y permite continuar descendiendo.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Collider, aspecto de grietas y efecto de restos opcional; restauración mediante controlador de respawn cuando está conectado.

### 11. Pilar sísmico rompible

- **Archivo definitivo:** `Resource_SeismicPillar_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_SeismicPillar_Volcano.prefab`.
- **Implementación:** `BreakableGround2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Variante de bloque rompible para la prueba de Pisotón.
- **Cómo funciona:** Romper los dos pilares Seismic_Pillar_0/1 permite al controlador abrir la compuerta.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Geometría por instancia y lista de pilares que debe comprobar TempleTrialGates2D.
- **Conexiones, variantes o límites:** Es una pieza del puzzle de templo; no inflige daño ni tiene una mecánica nueva de salud.

### 12. Sello de erupción rompible

- **Archivo definitivo:** `Resource_EruptionSeal_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_EruptionSeal_Volcano.prefab`.
- **Implementación:** `BreakableGround2D conectado a FractureEruption2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Sello que desbloquea salida y activa una erupción.
- **Cómo funciona:** Pisotón lo rompe; el controlador detecta IsBroken y dispara el evento de lava.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Collider y conexión al controlador de erupción.
- **Conexiones, variantes o límites:** El sello no daña; su activación pone en marcha Trap_RisingLavaEruption_Volcano. Está conectado dentro del conjunto de la fractura.

### 13. Barrera de cañas rompible

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Resource_DashReedBarrier_Swamp.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Resource_DashReedBarrier_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_DashReedBarrier_Swamp.prefab`.
- **Implementación:** `DashBreakableBarrier2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Barrera vegetal para practicar Dash.
- **Cómo funciona:** Choque en estado Dash desactiva el collider y desvanece la barrera.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Reaparición opcional: _canRespawn; retraso base 5 s si se habilita; desvanecimiento base 0.2 s.
- **Conexiones, variantes o límites:** Por defecto queda abierta. Una caída desde arriba normal no la rompe.

### 14. Reja rompible con Dash

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Resource_DashTrialGrid_Volcano.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Resource_DashTrialGrid_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_DashTrialGrid_Volcano.prefab`.
- **Implementación:** `DashBreakableBarrier2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Variante de barrera para la prueba aérea del templo.
- **Cómo funciona:** Dash la rompe y abre la ruta.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Mismo componente que las cañas; posición, tamaño y restauración conectados al encuentro.

### 15. Balancín de lanzamiento

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Puzzles/Seesaw/Resource_SeesawCatapult_Caves.prefab`. Ficha: [Puzles](LevelPieces/Puzzles.md).
- **Archivo definitivo:** `Resource_SeesawCatapult_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_SeesawCatapult_Caves.prefab`.
- **Implementación:** `SeesawPlatform2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Plataforma inclinable que transforma un Pisotón en impulso.
- **Cómo funciona:** El impacto inclina el tablero; la posición del jugador/contrapeso determina el lanzamiento disponible.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** EchoSeesawConfig.asset: ángulo máximo 18°; ventana 2.8 s; impulso de Alma 15; impulso de contrapeso 14 unidades/s.
- **Conexiones, variantes o límites:** Para puzzle completo usar el conjunto de balancín, contrapeso, runa y puerta.

### 16. Catapulta de raíces

- **Archivo definitivo:** `Resource_RootCatapult_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_RootCatapult_Swamp.prefab`.
- **Implementación:** `SeesawPlatform2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Variante de balancín para ganar altura en el sauce.
- **Cómo funciona:** Pisotón prepara el lanzamiento; desplazarse al extremo oportuno permite el impulso.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Configuración de balancín y geometría/límites de la instancia.
- **Conexiones, variantes o límites:** Variante visual y de colocación, no un controlador duplicado.

### 17. Contrapeso de catapulta

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Puzzles/Seesaw/Resource_CatapultCounterweight_Caves.prefab`. Ficha: [Puzles](LevelPieces/Puzzles.md).
- **Archivo definitivo:** `Resource_CatapultCounterweight_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_CatapultCounterweight_Caves.prefab`.
- **Implementación:** `CatapultWeight2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Peso que sale disparado por el balancín.
- **Cómo funciona:** Permanece vinculado al tablero, se convierte en cuerpo dinámico al lanzarse y activa runas al ascender.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** EchoSeesawConfig.asset: impulso 14 unidades/s; reinicio 4.7 s; posición local de reposo configurable.
- **Conexiones, variantes o límites:** Necesita referencia al tablero y configuración compartida.

### 18. Corriente de viento

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Resource_WindCurrent_Universal.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Resource_WindCurrent_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_WindCurrent_Universal.prefab`.
- **Implementación:** `WindCurrentZone2D`.
- **Mundo/contexto:** 3; reutilizable.
- **Qué es / para qué sirve:** Volumen de fuerza que empuja cuerpos físicos.
- **Cómo funciona:** Aplica aceleración en su dirección y puede contrarrestar parte de la gravedad; respeta IgnoresWind.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Dirección, fuerza y compensación de gravedad. Valor de clase por defecto: fuerza 22; las escenas pueden sobrescribirlo.
- **Conexiones, variantes o límites:** No inflige daño directo, pero puede desplazar a Alma hacia un peligro; el vendaval del jefe tiene control propio.

### 19. Espora de recarga de Dash

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Pieces/Resource_DashRefillSpore_Swamp.prefab`. Ficha: [Piezas de nivel](LevelPieces/Pieces.md).
- **Archivo definitivo:** `Resource_DashRefillSpore_Swamp.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_DashRefillSpore_Swamp.prefab`.
- **Implementación:** `DashRefillPickup2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Pickup flotante que permite encadenar Dash aéreos.
- **Cómo funciona:** Al contacto llama a RefreshAirDash, se consume temporalmente y vuelve a estar disponible.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Prefab: reaparición 2.5 s; frecuencia de flotación 3; amplitud 0.15.
- **Conexiones, variantes o límites:** Recarga el Dash; no es una plataforma ni cambia automáticamente la dirección del Dash.

### 20. Roca de basalto movible

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Puzzles/Resource_RoarBoulder_Volcano.prefab`. Ficha: [Puzles](LevelPieces/Puzzles.md).
- **Archivo definitivo:** `Resource_RoarBoulder_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_RoarBoulder_Volcano.prefab`.
- **Implementación:** `PushableBoulder2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Roca pesada para construir apoyos/puentes.
- **Cómo funciona:** Rugido horizontal inicia un desplazamiento en arco; al aterrizar sobre su lava vinculada activa una tapa sólida.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** BoulderRoarConfig.asset: avance 5 unidades en 0.8 s; arco 0.6; radio 1.8; ancho de puente 4.2; altura de puente 0.2.
- **Conexiones, variantes o límites:** Necesita su lava y Alma; las coordenadas de apoyo deben adaptarse a cada puzzle. No implementa daño por aplastamiento.

### 21. Interruptor rúnico

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Puzzles/Seesaw/Resource_RuneSwitch_Caves.prefab`. Ficha: [Puzles](LevelPieces/Puzzles.md).
- **Archivo definitivo:** `Resource_RuneSwitch_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_RuneSwitch_Caves.prefab`.
- **Implementación:** `RuneSwitch2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Sensor que habilita una señal temporal.
- **Cómo funciona:** Solo se activa con un CatapultWeight2D lanzado y ascendiendo dentro del trigger.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** EchoSeesawConfig.asset: señal 4 s. Color/indicador y collider configurables.
- **Conexiones, variantes o límites:** Alma tocándolo no sustituye al contrapeso.

### 22. Compuerta rúnica temporizada

- **Estado:** implementado con marcador provisional. Prefab real: `Assets/Prefabs/Level/Puzzles/Seesaw/Resource_TimedRuneGate_Caves.prefab`. Ficha: [Puzles](LevelPieces/Puzzles.md).
- **Archivo definitivo:** `Resource_TimedRuneGate_Caves.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_TimedRuneGate_Caves.prefab`.
- **Implementación:** `TimedRuneGate2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Puerta sólida que abre mientras sus runas están activas.
- **Cómo funciona:** Comprueba todos los interruptores; desactiva el collider mientras existe tiempo compartido de apertura.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Apertura actual 4 s; retorno desde la derecha opcional; zona de recuperación ajustable.
- **Conexiones, variantes o límites:** No aplasta ni hace daño al cerrar; comprueba si Alma está dentro para mantener el paso abierto. Requiere runas, configuración y Alma.

### 23. Campana de resonancia

- **Archivo definitivo:** `Resource_ResonanceBell_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_ResonanceBell_Volcano.prefab`.
- **Implementación:** `ResonanceBell2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Receptor de Rugido que abre una ventana segura.
- **Cómo funciona:** Rugido válido activa la supresión de la puerta de fuego vinculada.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** ResonanceBellConfig.asset: supresión 5 s; alcance 8 unidades; aviso final al quedar 1 s.
- **Conexiones, variantes o límites:** La campana produce respuesta visual y de gameplay; el juego no reproduce sonido.

### 24. Compuerta de impacto de meteorito

- **Archivo definitivo:** `Resource_MeteorImpactGate_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_MeteorImpactGate_Volcano.prefab`.
- **Implementación:** `MeteorImpactGate2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Obstáculo que se abre con un meteorito devuelto.
- **Cómo funciona:** Controla lanzamiento/aviso y recibe OpenByMeteor cuando el proyectil reflejado impacta.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** FractureConfig.asset; meteorito, marcador de aviso, suelo de aterrizaje y Alma como dependencias.
- **Conexiones, variantes o límites:** La compuerta es segura; el meteorito asociado es peligroso antes del Rugido.

### 25. Control de pruebas del templo

- **Archivo definitivo:** `Resource_TempleTrialGate_Volcano.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_TempleTrialGate_Volcano.prefab`.
- **Implementación:** `TempleTrialGates2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Mecanismo que vincula pilares, reja y progreso de checkpoints.
- **Cómo funciona:** Abre la compuerta al romper todos los pilares y restaura piezas pendientes según el checkpoint.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Lista de pilares, compuerta de Pisotón, reja de Dash, textos y referencia de Alma.
- **Conexiones, variantes o límites:** Es un controlador de conjunto; arrastrar solo el controlador sin sus piezas no crea un puzzle completo.

### 26. Sello de contraataque con Dash

- **Archivo definitivo:** `Resource_DashHeatSeal_FinalBoss.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_DashHeatSeal_FinalBoss.prefab`.
- **Implementación:** `ThiefKingMechanism2D: HeatSeal`.
- **Mundo/contexto:** 4 / jefe final.
- **Qué es / para qué sirve:** Objetivo de Dash para exponer al jefe.
- **Cómo funciona:** BreakWithDash llama a CounterCharge; el jefe valida la fase y la altura del contraataque.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tipo de mecanismo HeatSeal y referencia al jefe.
- **Conexiones, variantes o límites:** No provoca daño directo a Alma. Debe utilizarse dentro del encuentro del Rey Ladrón.

### 27. Placa dorsal vulnerable

- **Archivo definitivo:** `Resource_WeakDorsalPlate_FinalBoss.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_WeakDorsalPlate_FinalBoss.prefab`.
- **Implementación:** `ThiefKingMechanism2D: WeakPlate`.
- **Mundo/contexto:** 4 / jefe final.
- **Qué es / para qué sirve:** Objetivo de Pisotón durante la vulnerabilidad.
- **Cómo funciona:** ReceiveGroundPound llama a Strike; el estado del combate decide si suma un impacto.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tipo WeakPlate y referencia al jefe.
- **Conexiones, variantes o límites:** No es un enemigo distinto ni una pieza que quite HP al tocarla.

### 28. Sello de vapor rompible

- **Archivo definitivo:** `Resource_SteamSeal_FinalBoss.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_SteamSeal_FinalBoss.prefab`.
- **Implementación:** `ThiefKingMechanism2D: SteamSeal`.
- **Mundo/contexto:** 4 / jefe final.
- **Qué es / para qué sirve:** Sello que bloquea el respiradero de ascenso.
- **Cómo funciona:** Pisotón llama a OpenSteam cuando el combate está en la fase correspondiente.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tipo SteamSeal; vínculo con jefe y respiradero.
- **Conexiones, variantes o límites:** Pieza Steam_Obsidian_Seal dentro de Collapse_Ascent; el archivo individual indicado ya está disponible y requiere conectar su jefe.

### 29. Anclaje de estalactita

- **Archivo definitivo:** `Resource_StalactiteAnchor_FinalBoss.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_StalactiteAnchor_FinalBoss.prefab`.
- **Implementación:** `ThiefKingMechanism2D: Anchor`.
- **Mundo/contexto:** 4 / jefe final.
- **Qué es / para qué sirve:** Objetivo combinado para el remate.
- **Cómo funciona:** Rugido agrieta el anclaje; Pisotón válido después del agrietado llama a Finish.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tipo Anchor y vínculo con el jefe.
- **Conexiones, variantes o límites:** Pieza Stalactite_Anchor del conjunto. El orden de acciones y la fase se validan en el jefe.

### 30. Estalactita colosal de remate

- **Archivo definitivo:** `Resource_ColossalStalactite_FinalBoss.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_ColossalStalactite_FinalBoss.prefab`.
- **Implementación:** `Transform controlado por ThiefKingBoss2D`.
- **Mundo/contexto:** 4 / jefe final.
- **Qué es / para qué sirve:** Arma ambiental del remate final.
- **Cómo funciona:** El jefe la desplaza al resolver el anclaje y el remate.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Posición, visual y vínculo _stalactite del encuentro.
- **Conexiones, variantes o límites:** No tiene actualmente un controlador de daño independiente contra Alma. Pieza Colossal_Stalactite; no confundir con los cristales letales del armadillo.

### 31. Altar de Doble Salto

- **Estado:** implementado con marcador provisional (script `AbilityAltar2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_AbilityAltar_DoubleJump.prefab`. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_AbilityAltar_DoubleJump.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_AbilityAltar_DoubleJump.prefab`.
- **Implementación:** `AbilityRelic2D`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** Relicario que desbloquea una habilidad de Alma.
- **Cómo funciona:** Contacto con IAbilityUnlockable ejecuta UnlockAbility y muestra la presentación de la reliquia.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** _abilityToUnlock, título, descripción y canal de evento. Animación de recogida 0.4 s.
- **Conexiones, variantes o límites:** Variante con nombre propio del altar compartido. La variante tiene configurado el valor de AbilityType correspondiente; el identificador AirDash del archivo describe la habilidad, no obliga a renombrar el enum existente.

### 32. Altar de Pisotón

- **Estado:** implementado con marcador provisional (script `AbilityAltar2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_AbilityAltar_GroundPound.prefab`. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_AbilityAltar_GroundPound.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_AbilityAltar_GroundPound.prefab`.
- **Implementación:** `AbilityRelic2D`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Relicario que desbloquea una habilidad de Alma.
- **Cómo funciona:** Contacto con IAbilityUnlockable ejecuta UnlockAbility y muestra la presentación de la reliquia.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** _abilityToUnlock, título, descripción y canal de evento. Animación de recogida 0.4 s.
- **Conexiones, variantes o límites:** Variante con nombre propio del altar compartido. La variante tiene configurado el valor de AbilityType correspondiente; el identificador AirDash del archivo describe la habilidad, no obliga a renombrar el enum existente.

### 33. Altar de Dash

- **Estado:** implementado con marcador provisional (script `AbilityAltar2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_AbilityAltar_AirDash.prefab`. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_AbilityAltar_AirDash.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_AbilityAltar_AirDash.prefab`.
- **Implementación:** `AbilityRelic2D`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Relicario que desbloquea una habilidad de Alma.
- **Cómo funciona:** Contacto con IAbilityUnlockable ejecuta UnlockAbility y muestra la presentación de la reliquia.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** _abilityToUnlock, título, descripción y canal de evento. Animación de recogida 0.4 s.
- **Conexiones, variantes o límites:** Variante con nombre propio del altar compartido. La variante tiene configurado el valor de AbilityType correspondiente; el identificador AirDash del archivo describe la habilidad, no obliga a renombrar el enum existente.

### 34. Altar de Rugido

- **Estado:** implementado con marcador provisional (script `AbilityAltar2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_AbilityAltar_Roar.prefab`. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_AbilityAltar_Roar.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_AbilityAltar_Roar.prefab`.
- **Implementación:** `AbilityRelic2D`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Relicario que desbloquea una habilidad de Alma.
- **Cómo funciona:** Contacto con IAbilityUnlockable ejecuta UnlockAbility y muestra la presentación de la reliquia.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** _abilityToUnlock, título, descripción y canal de evento. Animación de recogida 0.4 s.
- **Conexiones, variantes o límites:** Variante con nombre propio del altar compartido. La variante tiene configurado el valor de AbilityType correspondiente; el identificador AirDash del archivo describe la habilidad, no obliga a renombrar el enum existente.

### 35. Nido / checkpoint

- **Estado:** implementado con marcador provisional (script `CheckpointNest2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_CheckpointNest_Universal.prefab`. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_CheckpointNest_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_CheckpointNest_Universal.prefab`.
- **Implementación:** `Checkpoint2D`.
- **Mundo/contexto:** 1–4.
- **Qué es / para qué sirve:** Punto seguro de reaparición.
- **Cómo funciona:** Contacto registra una posición de checkpoint y actualiza su indicador.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Collider de activación, posición de respawn e indicador visual.
- **Conexiones, variantes o límites:** Las variantes de piedra y nido comparten función; no son consumibles de salud.

### 36. Huevo verde

- **Estado:** implementado con marcador provisional (script `RescueEgg2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_RescueEgg_Green.prefab`. Al tocarlo se guarda el rescate y aparece el texto del GDD 5.2; aún no hay portal vinculado. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_RescueEgg_Green.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_RescueEgg_Green.prefab`.
- **Implementación:** `GreenEggRescue2D (tipo de huevo) + lógica de santuario cuando corresponda`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** Objetivo de rescate que habilita el avance hacia el jefe.
- **Cómo funciona:** Contacto real registra el huevo en la progresión, muestra el rescate y habilita el portal vinculado.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tipo de huevo, portal, textos, color/luz y referencias de santuario.
- **Conexiones, variantes o límites:** El rescate se guarda; volver a la escena respeta la progresión. Rescatarlo no equivale a derrotar al jefe del mundo.

### 37. Huevo azul

- **Estado:** implementado con marcador provisional (script `RescueEgg2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_RescueEgg_Blue.prefab`. Al tocarlo se guarda el rescate y aparece el texto del GDD 5.2; aún no hay portal vinculado. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_RescueEgg_Blue.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_RescueEgg_Blue.prefab`.
- **Implementación:** `GreenEggRescue2D (tipo de huevo) + lógica de santuario cuando corresponda`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Objetivo de rescate que habilita el avance hacia el jefe.
- **Cómo funciona:** Contacto real registra el huevo en la progresión, muestra el rescate y habilita el portal vinculado.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tipo de huevo, portal, textos, color/luz y referencias de santuario.
- **Conexiones, variantes o límites:** El rescate se guarda; volver a la escena respeta la progresión. Rescatarlo no equivale a derrotar al jefe del mundo.

### 38. Huevo morado

- **Estado:** implementado con marcador provisional (script `RescueEgg2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_RescueEgg_Purple.prefab`. Al tocarlo se guarda el rescate y aparece el texto del GDD 5.2; aún no hay portal vinculado. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_RescueEgg_Purple.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_RescueEgg_Purple.prefab`.
- **Implementación:** `GreenEggRescue2D (tipo de huevo) + lógica de santuario cuando corresponda`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Objetivo de rescate que habilita el avance hacia el jefe.
- **Cómo funciona:** Contacto real registra el huevo en la progresión, muestra el rescate y habilita el portal vinculado.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tipo de huevo, portal, textos, color/luz y referencias de santuario.
- **Conexiones, variantes o límites:** El rescate se guarda; volver a la escena respeta la progresión. Rescatarlo no equivale a derrotar al jefe del mundo.

### 39. Huevo rojo

- **Estado:** implementado con marcador provisional (script `RescueEgg2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_RescueEgg_Red.prefab`. Al tocarlo se guarda el rescate y aparece el texto del GDD 5.2; aún no hay portal vinculado. Ficha: [Progresión](LevelPieces/Progression.md).
- **Archivo definitivo:** `Resource_RescueEgg_Red.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_RescueEgg_Red.prefab`.
- **Implementación:** `GreenEggRescue2D (tipo de huevo) + lógica de santuario cuando corresponda`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Objetivo de rescate que habilita el avance hacia el jefe.
- **Cómo funciona:** Contacto real registra el huevo en la progresión, muestra el rescate y habilita el portal vinculado.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Tipo de huevo, portal, textos, color/luz y referencias de santuario.
- **Conexiones, variantes o límites:** El rescate se guarda; volver a la escena respeta la progresión. Rescatarlo no equivale a derrotar al jefe del mundo.

### 40. Santuario de rescate

- **Archivo definitivo:** `Resource_EggSanctuary_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_EggSanctuary_Universal.prefab`.
- **Implementación:** `Conjunto de huevo + nido + controlador de santuario`.
- **Mundo/contexto:** 1–4.
- **Qué es / para qué sirve:** Lugar seguro y presentación del rescate.
- **Cómo funciona:** Coordina checkpoint, huevo, portal y respuestas de la escena. En el rojo controla la calma del templo y el anuncio del guardián.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Huevo concreto, escena/portal de destino y referencias a combate/fuego según el mundo.
- **Conexiones, variantes o límites:** Familia con variantes por mundo; usar conjuntos conectados para conservar todos los enlaces. El morado detiene el gas a través del encuentro.

### 41. Portal de salida

- **Estado:** implementado con marcador provisional (script `LevelExitPortal2D`). Prefab real: `Assets/Prefabs/Level/Progression/Resource_LevelExitPortal_Universal.prefab`. Va en todos los niveles, sin condición de apertura: al entrar Alma guarda el nivel como completado y carga `Next Scene`. Ficha: [Progresión](LevelPieces/Progression.md#portal-de-salida-resource_levelexitportal_universal).
- **Archivo definitivo:** `Resource_LevelExitPortal_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Resources/Resource_LevelExitPortal_Universal.prefab`.
- **Implementación:** `LevelExit2D`.
- **Mundo/contexto:** 1–4.
- **Qué es / para qué sirve:** Salida entre niveles y arenas.
- **Cómo funciona:** Contacto con Alma solicita cargar NextSceneName cuando el portal está habilitado.
- **Daño a Alma:** 0 daño directo. El peligro de una caída depende del entorno donde se coloque.
- **Valores y propiedades:** Escena destino, trigger, visibilidad y condiciones de habilitación del encuentro.
- **Conexiones, variantes o límites:** El nombre de escena debe existir en Build Settings. Jefes y huevos pueden mantenerlo cerrado hasta completar su condición.


## Narrativa y apoyo

### 1. Mono ladrón de presentación

- **Archivo definitivo:** `NPC_ThiefMonkeyTeaser_Jungle.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Narrative/NPC_ThiefMonkeyTeaser_Jungle.prefab`.
- **Implementación:** `ThiefMonkeyTeaser2D`.
- **Mundo/contexto:** 1.
- **Qué es / para qué sirve:** NPC narrativo que muestra el robo y orienta la persecución.
- **Cómo funciona:** Salta en reposo, detecta proximidad y huye con el huevo; muestra un mensaje.
- **Daño a Alma:** 0 daño. No tiene ataque dañino.
- **Valores y propiedades:** Clase: distancia de activación 6 unidades; fuga 1.2 s; dirección de fuga (10, 8), con posibles overrides.
- **Conexiones, variantes o límites:** Se documenta aparte de los enemigos hostiles.

### 2. Silueta de guardián Armadillo

- **Archivo definitivo:** `Narrative_GuardianTeaser_Armadillo.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Narrative/Narrative_GuardianTeaser_Armadillo.prefab`.
- **Implementación:** `Visual/presentación controlada por el santuario`.
- **Mundo/contexto:** 2.
- **Qué es / para qué sirve:** Anticipación visual del jefe siguiente.
- **Cómo funciona:** Se revela como parte de la secuencia de rescate.
- **Daño a Alma:** 0 daño.
- **Valores y propiedades:** Posición, sprite, escala y referencia desde el santuario.
- **Conexiones, variantes o límites:** Es una presentación, no una segunda instancia del jefe activo.

### 3. Silueta de guardián Pterodactyl

- **Archivo definitivo:** `Narrative_GuardianTeaser_Pterodactyl.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Narrative/Narrative_GuardianTeaser_Pterodactyl.prefab`.
- **Implementación:** `Visual/presentación controlada por el santuario`.
- **Mundo/contexto:** 3.
- **Qué es / para qué sirve:** Anticipación visual del jefe siguiente.
- **Cómo funciona:** Se revela como parte de la secuencia de rescate.
- **Daño a Alma:** 0 daño.
- **Valores y propiedades:** Posición, sprite, escala y referencia desde el santuario.
- **Conexiones, variantes o límites:** Es una presentación, no una segunda instancia del jefe activo.

### 4. Silueta de guardián Thief_King

- **Archivo definitivo:** `Narrative_GuardianTeaser_Thief_King.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Narrative/Narrative_GuardianTeaser_Thief_King.prefab`.
- **Implementación:** `Visual/presentación controlada por el santuario`.
- **Mundo/contexto:** 4.
- **Qué es / para qué sirve:** Anticipación visual del jefe siguiente.
- **Cómo funciona:** Se revela como parte de la secuencia de rescate.
- **Daño a Alma:** 0 daño.
- **Valores y propiedades:** Posición, sprite, escala y referencia desde el santuario.
- **Conexiones, variantes o límites:** Es una presentación, no una segunda instancia del jefe activo.

### 5. Mensaje de prólogo

- **Archivo definitivo:** `Narrative_PrologueTrigger_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Narrative/Narrative_PrologueTrigger_Universal.prefab`.
- **Implementación:** `NarrativePrologueTrigger`.
- **Mundo/contexto:** 1–4.
- **Qué es / para qué sirve:** Trigger de explicación/presentación al entrar en una zona.
- **Cómo funciona:** Solicita el banner narrativo con título, texto y duración.
- **Daño a Alma:** 0 daño.
- **Valores y propiedades:** Trigger, mensajes, duración y configuración de repetición del componente.

### 6. Onda visual de Rugido

- **Archivo definitivo:** `VFX_RoarWave_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Narrative/VFX_RoarWave_Universal.prefab`.
- **Implementación:** `RoarWaveVisual2D`.
- **Mundo/contexto:** 4; reutilizable.
- **Qué es / para qué sirve:** Representación visual del Rugido.
- **Cómo funciona:** Reacciona al Rugido y muestra la onda expansiva.
- **Daño a Alma:** 0 daño directo. La reacción de enemigos/mecanismos pertenece a sus propios controladores.
- **Valores y propiedades:** Referencia de Alma y propiedades del efecto.

### 7. Fondo con parallax

- **Archivo definitivo:** `Scenery_ParallaxLayer_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Narrative/Scenery_ParallaxLayer_Universal.prefab`.
- **Implementación:** `ParallaxLayer2D`.
- **Mundo/contexto:** 2–4.
- **Qué es / para qué sirve:** Capa de fondo con profundidad aparente.
- **Cómo funciona:** Se desplaza en proporción al movimiento de la cámara.
- **Daño a Alma:** 0 daño.
- **Valores y propiedades:** Cámara y factores horizontal/vertical; clase por defecto (0.6, 0.6), variables por capa.
- **Conexiones, variantes o límites:** Las múltiples capas del catálogo son composiciones/variantes de la misma función.

### 8. Decoración visual

- **Archivo definitivo:** `Scenery_Decoration_Universal.prefab`.
- **Ruta objetivo:** `Assets/_Project/Prefabs/Narrative/Scenery_Decoration_Universal.prefab`.
- **Implementación:** `SpriteRenderer`.
- **Mundo/contexto:** 1–4.
- **Qué es / para qué sirve:** Pieza gráfica de ambientación.
- **Cómo funciona:** Se dibuja con su sprite/material/orden de render.
- **Daño a Alma:** 0 daño.
- **Valores y propiedades:** Sprite, tinte, escala, posición y orden de render.
- **Conexiones, variantes o límites:** Para decoración segura, mantenerla sin componente de peligro; un sprite por sí mismo no hace daño.

## Jugador

### Alma — madre dinosaurio

- **Prefab previsto:** `Assets/_Project/Prefabs/Player/Alma/Player_Alma.prefab`.
- **Función:** personaje jugable, capaz de moverse, saltar, hacer Doble Salto, Pisotón, Dash y Rugido.
- **Daño:** contacto con peligro activo → reaparece en el último checkpoint; no usa HP.
- **Especificación completa:** [controles, física, estados, animaciones y cámara de Alma](Docs/Player/Alma.md).
- **Arte conceptual:** [índice de láminas](Docs/Art/README.md).

## Uso futuro del inventario

Las rutas de prefab de este documento son objetivos de organización para el nuevo proyecto Unity. Las fichas recogen el comportamiento del prototipo anterior y las láminas de `Docs/Art/` recogen el aspecto conceptual. Reconstruir, integrar y verificar cada elemento antes de crear niveles.

Las fichas de `Docs/Levels/` describen los niveles por reconstruir. La [ficha de Alma](Docs/Player/Alma.md) reúne controles, física, animaciones y cámara; [las láminas](Docs/Art/README.md) reúnen las referencias visuales.
