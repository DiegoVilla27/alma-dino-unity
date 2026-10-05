# Progresión — nido de checkpoint y altares de habilidad

**Estado (6 de octubre de 2026):** implementados en Unity 6000.6.0f1 el nido de checkpoint y los cuatro altares de habilidad, con **cajas de color y etiqueta** como marcador hasta que llegue el arte. Comprobados con una prueba PlayMode en `Level_1_1` (altar → habilidad desbloqueada y guardada; nido → punto de reaparición y guardado; muerte → reaparición en el nido). Funcionan junto al [guardado y el gestor de progreso](../Systems/SaveAndProgress.md).

Fichas de diseño originales: [inventario, Recursos](../INVENTARIO_GAMEPLAY_PREFABS.md#recursos). GDD: 8.3 (checkpoints) y 9.3 (guardado). Jugador: [Alma](../Player/Alma.md).

## Resumen

| Prefab | Etiqueta | Color del marcador | Qué hace |
| --- | --- | --- | --- |
| `Resource_CheckpointNest_Universal` | Nido checkpoint | Marrón (0,5; 0,35; 0,2), 1,4 × 0,5 | Al pasar por encima se enciende y pasa a ser el punto de reaparición; autoguarda. |
| `Resource_AbilityAltar_DoubleJump` | Altar: Doble Salto | Pedestal gris azulado, orbe cian | Desbloquea el Doble Salto (Mundo 1). |
| `Resource_AbilityAltar_GroundPound` | Altar: Pisotón | Orbe naranja | Desbloquea el Pisotón (Mundo 2). |
| `Resource_AbilityAltar_AirDash` | Altar: Dash | Orbe verde claro | Desbloquea el Dash aéreo (Mundo 3). |
| `Resource_AbilityAltar_Roar` | Altar: Rugido | Orbe rojo | Desbloquea el Rugido (Mundo 4). |

Todos están en `Assets/Prefabs/Level/Progression/`. Los cuatro altares son variantes de `Resource_AbilityAltar_Base` y solo cambian la habilidad, el título y el color del orbe.

## Archivos

| Archivo | Responsabilidad |
| --- | --- |
| `Progression/Scripts/CheckpointNest2D.cs` | Nido: detección, punto de reaparición, brasas/llama y aviso a `GameProgress`. Implementa `ICheckpoint`. |
| `Progression/Scripts/AbilityAltar2D.cs` | Altar: orbe flotante, recogida, título y desbloqueo. |
| `Progression/*.prefab` | Un prefab por pieza; altares como variantes de la base. |

Usan el cuadrado compartido `Level/Shared/Sprites/Level_Placeholder.png`, los efectos de `HazardFx` y la etiqueta de `HazardZone2D`.

## Nido de checkpoint (`Resource_CheckpointNest_Universal`)

- **Apagado:** brasas tenues (3 chispas por segundo, rojo anaranjado).
- **Al pasar Alma** (trigger alto de 1,6 × 3 sobre el nido, así cuenta aunque pase saltando):
  - `AlmaMotor2D.SetRespawnPosition(punto)`: Alma reaparecerá 1,5 u por encima del centro del nido;
  - se enciende con una ráfaga de 14 chispas doradas y queda una **llama dorada** (14 nubes por segundo que suben y se encogen);
  - los demás nidos vuelven a brasas: solo arde el último;
  - si hay `GameProgress`, **autoguarda** la posición.
- Tocar un nido ya encendido lo vuelve a hacer el actual (sin ráfaga). Sin texto de «Checkpoint guardado» (GDD 8.3).
- Al cargar una partida del mismo nivel, el nido de la posición guardada aparece encendido.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (1,4; 0,5) | Tamaño del nido. |
| `Trigger Size` | (1,6; 3) | Zona de activación, apoyada sobre el nido. |
| `Respawn Offset` | (0; 1,5) | Punto de reaparición respecto al centro del nido (gizmo amarillo). |
| `Ember Color` | rojo anaranjado | Brasas apagadas. |
| `Flame Color A` / `B` | dorado / naranja | Llama encendida. |
| `Label` / `Show Label` | «Nido checkpoint» / sí | Etiqueta provisional. |

## Altares de habilidad (`Resource_AbilityAltar_*`)

| Fase | Qué pasa |
| --- | --- |
| **Esperando** | Orbe de luz del color de la habilidad flotando sobre el pedestal (balanceo de ±0,12 u y latido), con chispas. |
| **Recogida (0,4 s)** | Al tocar Alma la zona del altar, el orbe vuela hasta ella encogiéndose. |
| **Desbloqueo** | Estallido de 18 destellos sobre Alma; la habilidad se desbloquea (`GameProgress.UnlockAbility`, que también guarda; sin `GameProgress`, directamente en Alma). El nombre de la habilidad aparece en grande sobre el altar, sube un poco y se desvanece en 2 s (el «aviso breve» del GDD). |
| **Gastado** | Sin orbe, pedestal oscurecido, sin zona de activación. |

Si la habilidad ya estaba desbloqueada al empezar (por la partida guardada o por las habilidades iniciales del nivel), el altar aparece directamente gastado.

| Campo | Base | Uso |
| --- | ---: | --- |
| `Ability` | DoubleJump | Habilidad (`DoubleJump`, `GroundPound`, `Dash`, `Roar`). |
| `Title` | «Doble Salto» | Nombre que aparece al desbloquear. |
| `Orb Color` | cian | Color del orbe, las chispas y el título. |
| `Size` | (1,2; 1) | Tamaño del pedestal. |
| `Trigger Size` | (1,4; 2,8) | Zona de activación sobre el pedestal. |
| `Collect Time` | 0,4 | Duración de la recogida (s). |
| `Title Time` | 2 | Tiempo que se ve el título (s). |
| `Show Label` | sí | Etiqueta provisional «Altar: …» bajo el pedestal. |

| Variante | `Ability` | `Title` | `Orb Color` |
| --- | --- | --- | --- |
| `Resource_AbilityAltar_DoubleJump` | DoubleJump | Doble Salto | (0,55; 0,9; 1) |
| `Resource_AbilityAltar_GroundPound` | GroundPound | Pisotón | (1; 0,6; 0,25) |
| `Resource_AbilityAltar_AirDash` | Dash | Dash | (0,6; 1; 0,5) |
| `Resource_AbilityAltar_Roar` | Roar | Rugido | (1; 0,4; 0,35) |

## Coste

Nido: un SpriteRenderer, un trigger y tres sistemas de partículas pequeños (máx. 8, 24 y 16). Altar: un SpriteRenderer, un trigger, dos sprites de brillo, dos sistemas de partículas (máx. 10 y 20) y dos `TextMesh`. Sin consultas físicas por frame.

## Pendiente

- Arte del nido y de los altares.
- Colocarlos en los niveles (y `System_GameProgress` en cada uno).
- Variante de piedra del checkpoint (el inventario la menciona; misma función).
