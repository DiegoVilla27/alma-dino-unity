# Progresión — nido de checkpoint, altares de habilidad, huevos y portal de salida

**Estado (6 de octubre de 2026):** implementados en Unity 6000.6.0f1 el nido de checkpoint, los cuatro altares de habilidad, los cuatro huevos a rescatar y el portal de salida, con **cajas de color y etiqueta** como marcador hasta que llegue el arte. Comprobados con pruebas PlayMode en `Level_1_1` (altar → habilidad desbloqueada y guardada; nido → punto de reaparición y guardado; muerte → reaparición en el nido; huevo → rescate guardado y texto del GDD; huevo ya rescatado → no aparece; portal → nivel completado guardado y carga de la escena siguiente). Funcionan junto al [guardado y el gestor de progreso](../Systems/SaveAndProgress.md).

Fichas de diseño originales: [inventario, Recursos](../INVENTARIO_GAMEPLAY_PREFABS.md#recursos). GDD: 8.3 (checkpoints) y 9.3 (guardado). Jugador: [Alma](../Player/Alma.md).

## Resumen

| Prefab | Etiqueta | Color del marcador | Qué hace |
| --- | --- | --- | --- |
| `Resource_CheckpointNest_Universal` | Nido checkpoint | Marrón (0,5; 0,35; 0,2), 1,4 × 0,5 | Al pasar por encima se enciende y pasa a ser el punto de reaparición; autoguarda. |
| `Resource_AbilityAltar_DoubleJump` | Altar: Doble Salto | Pedestal gris azulado, orbe cian | Desbloquea el Doble Salto (Mundo 1). |
| `Resource_AbilityAltar_GroundPound` | Altar: Pisotón | Orbe naranja | Desbloquea el Pisotón (Mundo 2). |
| `Resource_AbilityAltar_AirDash` | Altar: Dash | Orbe verde claro | Desbloquea el Dash aéreo (Mundo 3). |
| `Resource_AbilityAltar_Roar` | Altar: Rugido | Orbe rojo | Desbloquea el Rugido (Mundo 4). |
| `Resource_RescueEgg_Green` | Huevo verde | Verde (0,45; 0,85; 0,35), 0,6 × 0,8 | Huevo 1 (Mundo 1): al tocarlo se rescata y se guarda. |
| `Resource_RescueEgg_Blue` | Huevo azul | Azul (0,35; 0,65; 1) | Huevo 2 (Mundo 2). |
| `Resource_RescueEgg_Purple` | Huevo morado | Morado (0,7; 0,45; 0,95) | Huevo 3 (Mundo 3). |
| `Resource_RescueEgg_Red` | Huevo rojo | Rojo (1; 0,35; 0,3) | Huevo 4 (Mundo 4). |
| `Resource_LevelExitPortal_Universal` | Portal de salida | Violeta translúcido (0,45; 0,35; 0,8; 60 %), 1,2 × 2,4 | Final de **todos** los niveles: guarda el nivel como completado y carga el siguiente. |

Todos están en `Assets/Prefabs/Level/Progression/`. Los cuatro altares son variantes de `Resource_AbilityAltar_Base` y solo cambian la habilidad, el título y el color del orbe. Los cuatro huevos son variantes de `Resource_RescueEgg_Base` y solo cambian identificador, color, texto y etiqueta.

## Archivos

| Archivo | Responsabilidad |
| --- | --- |
| `Progression/Scripts/CheckpointNest2D.cs` | Nido: detección, punto de reaparición, brasas/llama y aviso a `GameProgress`. Implementa `ICheckpoint`. |
| `Progression/Scripts/AbilityAltar2D.cs` | Altar: orbe flotante, recogida, título y desbloqueo. |
| `Progression/Scripts/RescueEgg2D.cs` | Huevo: aura, recogida, texto de rescate y guardado. |
| `Progression/Scripts/LevelExitPortal2D.cs` | Portal de salida: remolino, fin de nivel, guardado, fundido y carga de la escena siguiente. |
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

## Huevos a rescatar (`Resource_RescueEgg_*`)

| Fase | Qué pasa |
| --- | --- |
| **Esperando** | El huevo flota (±0,1 u) sobre un aura suave de su color que late, con chispas. |
| **Recogida (0,5 s)** | Al tocarlo Alma (zona de 1,2 × 1,6), vuela hasta ella encogiéndose. |
| **Rescate** | Estallido de 20 destellos de su color y blancos; se guarda (`GameProgress.RescueEgg`, campo `RescuedEggs`); aparece el texto de rescate del GDD 5.2 sobre el lugar del huevo, sube despacio y se desvanece al final de 4,5 s. Se lanza el evento `Rescued` (para que el futuro portal de salida o santuario reaccionen). |
| **Ya rescatado** | Si la partida guardada ya contiene ese huevo, no aparece. |

Sin `System_GameProgress` en la escena el rescate se ve igual, pero no se guarda (aviso en consola).

| Campo | Base | Uso |
| --- | ---: | --- |
| `Egg Id` | Green | Identificador guardado en `RescuedEggs` (`Green`, `Blue`, `Purple`, `Red`). |
| `Egg Color` | verde | Color del aura, las chispas y el estallido. |
| `Rescue Text` | texto del huevo 1 | Texto que aparece al rescatarlo (admite saltos de línea). |
| `Size` / `Trigger Size` | (0,6; 0,8) / (1,2; 1,6) | Tamaño del huevo y de su zona de rescate. |
| `Collect Time` | 0,5 | Duración del vuelo hasta Alma (s). |
| `Text Time` | 4,5 | Tiempo que se ve el texto (s). |
| `Label` / `Show Label` | «Huevo verde» / sí | Etiqueta provisional bajo el huevo. |

| Variante | `Egg Id` | `Rescue Text` (GDD 5.2) |
| --- | --- | --- |
| `Resource_RescueEgg_Green` | Green | «Aún estás tibio... / Mamá llegó a tiempo. Ya estás a salvo.» |
| `Resource_RescueEgg_Blue` | Blue | «Sentí tu latido contra la piedra fría. Ya somos dos. / No descansaré hasta que estemos los cinco juntos.» |
| `Resource_RescueEgg_Purple` | Purple | «El cascarón tiembla... falta muy poco para que rompas a cantar. / Solo nos falta uno.» |
| `Resource_RescueEgg_Red` | Red | «Los cuatro están aquí. / Mi nido vuelve a estar completo.» |

(«/» marca el salto de línea.)

## Portal de salida (`Resource_LevelExitPortal_Universal`)

Va al final de **cada** nivel (no depende de los huevos). Su única función es terminar el nivel.

| Fase | Qué pasa |
| --- | --- |
| **Esperando** | Rectángulo violeta con un núcleo de luz que late y chispas azules y violetas girando hacia el centro. |
| **Alma entra** | Se desactiva su control (`AlmaInput`), el portal estalla en 26 destellos y, si hay `GameProgress`, se llama a `CompleteLevel(Next Scene)`: el nivel actual se añade a `CompletedLevels` y el guardado pasa al nivel siguiente, sin checkpoint, para empezarlo desde el principio. |
| **Fundido (0,8 s)** | La pantalla funde a negro (sprite negro delante de la cámara). |
| **Carga** | Se carga `Next Scene`. |

- `Next Scene` es el **nombre** de la escena (por ejemplo, `Level_1_2`) y debe estar en **File → Build Settings**. Hoy solo está `Level_1_1`.
- Si `Next Scene` está vacío o no está en Build Settings, el progreso se guarda igual, sale un aviso en consola, la pantalla vuelve a verse y Alma recupera el control (el juego no se queda en negro).
- Sin `System_GameProgress` el portal funciona, pero no guarda (aviso en consola).
- En el editor, la etiqueta del gizmo muestra a qué escena lleva («→ Level_1_2» o «(sin escena siguiente)»).

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Next Scene` | vacío | Escena que se carga al salir. **Rellenar en cada nivel.** |
| `Size` | (1,2; 2,4) | Tamaño del portal y de su zona de entrada. |
| `Swirl Color A` / `B` | azul / violeta | Colores de las chispas. |
| `Fade Time` | 0,8 | Duración del fundido a negro (s). |
| `Label` / `Show Label` | «Portal de salida» / sí | Etiqueta provisional. |

## Coste

Nido: un SpriteRenderer, un trigger y tres sistemas de partículas pequeños (máx. 8, 24 y 16). Altar: un SpriteRenderer, un trigger, dos sprites de brillo, dos sistemas de partículas (máx. 10 y 20) y dos `TextMesh`. Sin consultas físicas por frame.

## Pendiente

- Arte del nido y de los altares.
- Colocarlos en los niveles (y `System_GameProgress` en cada uno).
- Variante de piedra del checkpoint (el inventario la menciona; misma función).
- Santuario que reaccione al rescate (evento `Rescued`) e indicador de huevos en pantalla (GDD 8.1).
- Fundido de entrada al empezar el nivel siguiente (hoy aparece directamente).
- Añadir cada nivel nuevo a Build Settings para que el portal pueda cargarlo.
- «La escena cambia visualmente» al rescatar (GDD 5.2): hoy solo hay estallido y texto.
