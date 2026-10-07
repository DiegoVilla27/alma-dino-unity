# Progresión — nido de checkpoint, altares de habilidad, huevos y portal de salida

**Estado (7 de octubre de 2026):** implementados en Unity 6000.6.0f1 el nido de checkpoint, los cuatro altares de habilidad, los cuatro huevos a rescatar y el portal de salida, todos con **arte final** (ver [Arte](#arte)). Comprobados con pruebas PlayMode en `Level_1_1` (altar → habilidad desbloqueada y guardada; nido → punto de reaparición y guardado; muerte → reaparición en el nido; huevo → rescate guardado y texto del GDD; huevo ya rescatado → no aparece; portal → nivel completado guardado y carga de la escena siguiente). Funcionan junto al [guardado y el gestor de progreso](../Systems/SaveAndProgress.md).

Fichas de diseño originales: [inventario, Recursos](../INVENTARIO_GAMEPLAY_PREFABS.md#recursos). GDD: 8.3 (checkpoints) y 9.3 (guardado). Jugador: [Alma](../Player/Alma.md).

## Resumen

| Prefab | Etiqueta | Arte | `Size` | Qué hace |
| --- | --- | --- | --- | --- |
| `Resource_CheckpointNest_Universal` | Nido checkpoint | Nido de ramas sobre anillo de piedra, farolillo cian | 2,2 × 0,906 | Al pasar por encima se enciende y pasa a ser el punto de reaparición; autoguarda. |
| `Resource_AbilityAltar_DoubleJump` | Altar: Doble Salto | Pedestal selvático + runa verde con doble chevrón | 1,602 × 1,359 | Desbloquea el Doble Salto (Mundo 1). |
| `Resource_AbilityAltar_GroundPound` | Altar: Pisotón | Pedestal con cristales azules + runa de flecha abajo | 1,602 × 1,309 | Desbloquea el Pisotón (Mundo 2). |
| `Resource_AbilityAltar_AirDash` | Altar: Dash | Pedestal con raíces y flores moradas + runa de flecha | 1,602 × 1,395 | Desbloquea el Dash aéreo (Mundo 3). |
| `Resource_AbilityAltar_Roar` | Altar: Rugido | Pedestal volcánico + runa ámbar de ondas | 1,602 × 1,254 | Desbloquea el Rugido (Mundo 4). |
| `Resource_RescueEgg_Green` | Huevo verde | Huevo verde de motas | 0,605 × 0,801 | Huevo 1 (Mundo 1): al tocarlo se rescata y se guarda. |
| `Resource_RescueEgg_Blue` | Huevo azul | Huevo azul de motas | 0,602 × 0,801 | Huevo 2 (Mundo 2). |
| `Resource_RescueEgg_Purple` | Huevo morado | Huevo morado de motas | 0,602 × 0,801 | Huevo 3 (Mundo 3). |
| `Resource_RescueEgg_Red` | Huevo rojo | Huevo rojo de motas | 0,605 × 0,801 | Huevo 4 (Mundo 4). |
| `Resource_LevelExitPortal_Universal` | Portal de salida | Arco de piedra con lianas, runas y remolino turquesa | 2,188 × 2,801 | Final de **todos** los niveles: guarda el nivel como completado y carga el siguiente. |

Todos están en `Assets/Prefabs/Level/Progression/`. Los cuatro altares son variantes de `Resource_AbilityAltar_Base` (cambian habilidad, título, color, sprites y tamaño). Los cuatro huevos son variantes de `Resource_RescueEgg_Base` (cambian identificador, color, texto, sprite y etiqueta). Los prefabs base conservan el marcador provisional (`Level_Placeholder`); todos llevan `PieceArt2D` (ver [Piezas](Pieces.md#cambiar-el-tamaño)).

## Archivos

| Archivo | Responsabilidad |
| --- | --- |
| `Progression/Scripts/CheckpointNest2D.cs` | Nido: detección, punto de reaparición, brasas/llama y aviso a `GameProgress`. Implementa `ICheckpoint`. |
| `Progression/Scripts/AbilityAltar2D.cs` | Altar: orbe flotante, recogida, título y desbloqueo. |
| `Progression/Scripts/RescueEgg2D.cs` | Huevo: aura, recogida, texto de rescate y guardado. |
| `Progression/Scripts/LevelExitPortal2D.cs` | Portal de salida: remolino, fin de nivel, guardado, fundido y carga de la escena siguiente. |
| `Progression/*.prefab` | Un prefab por pieza; altares y huevos como variantes de su base. |
| `Progression/Sprites/*.png` | Arte: un sprite por huevo, nido y portal; dos por altar (`_Pedestal` y `_Rune`). |

Usan los efectos de `HazardFx`, la etiqueta de `HazardZone2D` y, para el tamaño, `LevelPieceUtility.ApplySize` + `PieceArt2D` de las [piezas](Pieces.md): respetan el `Draw Mode` del prefab (todos en *Sliced* sin borders: la imagen se ajusta a `Size` como un único dibujo) y el tamaño se puede cambiar con `Size` o con la herramienta Rect sin que se restablezca al dar Play.

## Arte

Generado con IA el 7/10/2026 (7 generaciones: una por altar, una con los cuatro huevos, una para el nido y otra para el portal) con su viñeta de `Assets/Art/Resources/Resources_ConceptSheet_04_v1.png` (#31–41) como referencia, y procesado a **256 PPU**, malla *Full Rect*, color blanco y escala 1 × 1. Etiquetas provisionales desactivadas.

| Pieza | Sprite(s) | Píxeles | Notas |
| --- | --- | --- | --- |
| Altares | `Resource_AbilityAltar_<Habilidad>_Pedestal.png` + `_Rune.png` | pedestal ~410 × 320–357; runa ~120–148 × 214–229 | La runa se recortó de la misma imagen (pieza separada del pedestal) y se dibuja aparte para que levite. |
| Huevos | `Resource_RescueEgg_<Color>.png` | ~155 × 205 | Generados juntos (misma forma y estilo) y recortados. Sin rocas: flotan sobre su aura. |
| Nido | `Resource_CheckpointNest_Universal.png` | 563 × 649 | La caja de juego es el nido con su anillo de piedra; el farolillo y su poste son margen de arte (`PieceArt2D` margin 0,815, filas transparentes abajo para centrar). |
| Portal | `Resource_LevelExitPortal_Universal.png` | 560 × 717 | La caja de juego es el arco entero. |

Los colores de los altares siguen al concepto (antes eran otros): Doble Salto verde, Pisotón azul, Dash morado, Rugido ámbar; coinciden con los huevos de cada mundo.

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
| `Size` | (2,2; 0,906) | Caja del nido (anillo + nido; el farolillo es margen de arte). |
| `Trigger Size` | (2,2; 3) | Zona de activación, apoyada sobre el nido. |
| `Respawn Offset` | (0; 1,5) | Punto de reaparición respecto al centro del nido (gizmo amarillo). |
| `Ember Color` | rojo anaranjado | Brasas apagadas. |
| `Flame Color A` / `B` | dorado / naranja | Llama encendida. |
| `Label` / `Show Label` | «Nido checkpoint» / no | Etiqueta provisional. |

## Altares de habilidad (`Resource_AbilityAltar_*`)

| Fase | Qué pasa |
| --- | --- |
| **Esperando** | La **runa** levita sobre el pedestal: sube y baja ±0,12 u, se inclina suavemente (±5°) y tiene detrás un halo de su color que respira (1,6–1,8 × su alto); chispas alrededor. Sin `Rune Sprite`, se usa el orbe de luz anterior. |
| **Recogida (0,4 s)** | Al tocar Alma la zona del altar, la runa vuela hasta ella encogiéndose. |
| **Desbloqueo** | Estallido de 18 destellos sobre Alma; la habilidad se desbloquea (`GameProgress.UnlockAbility`, que también guarda; sin `GameProgress`, directamente en Alma). El nombre de la habilidad aparece en grande sobre el altar, sube un poco y se desvanece en 2 s (el «aviso breve» del GDD). |
| **Gastado** | Sin runa, pedestal oscurecido (60 %), sin zona de activación. |

Si la habilidad ya estaba desbloqueada al empezar (por la partida guardada o por las habilidades iniciales del nivel), el altar aparece directamente gastado.

| Campo | Base | Uso |
| --- | ---: | --- |
| `Ability` | DoubleJump | Habilidad (`DoubleJump`, `GroundPound`, `Dash`, `Roar`). |
| `Title` | «Doble Salto» | Nombre que aparece al desbloquear. |
| `Orb Color` | cian | Color del halo, las chispas y el título. |
| `Size` | (1,2; 1) | Tamaño del pedestal (cada variante usa el de su dibujo). |
| `Rune Sprite` | — | Runa que levita (cada variante tiene la suya). |
| `Rune Lift` | 1,2 | Altura del centro de la runa sobre el centro del pedestal (u). |
| `Trigger Size` | (1,4; 2,8) | Zona de activación sobre el pedestal. |
| `Collect Time` | 0,4 | Duración de la recogida (s). |
| `Title Time` | 2 | Tiempo que se ve el título (s). |
| `Show Label` | sí (no en las variantes) | Etiqueta provisional «Altar: …» bajo el pedestal. |

| Variante | `Ability` | `Title` | `Orb Color` | `Size` | `Rune Lift` |
| --- | --- | --- | --- | --- | --- |
| `Resource_AbilityAltar_DoubleJump` | DoubleJump | Doble Salto | verde (0,55; 1; 0,35) | (1,602; 1,359) | 0,925 |
| `Resource_AbilityAltar_GroundPound` | GroundPound | Pisotón | azul (0,45; 0,85; 1) | (1,602; 1,309) | 0,96 |
| `Resource_AbilityAltar_AirDash` | Dash | Dash | morado (0,8; 0,5; 1) | (1,602; 1,395) | 0,935 |
| `Resource_AbilityAltar_Roar` | Roar | Rugido | ámbar (1; 0,75; 0,25) | (1,602; 1,254) | 0,883 |

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
| `Size` / `Trigger Size` | (0,6; 0,8) (variantes 0,602–0,605 × 0,801) / (1,2; 1,6) | Tamaño del huevo y de su zona de rescate. |
| `Collect Time` | 0,5 | Duración del vuelo hasta Alma (s). |
| `Text Time` | 4,5 | Tiempo que se ve el texto (s). |
| `Label` / `Show Label` | «Huevo verde» / sí (no en las variantes) | Etiqueta provisional bajo el huevo. |

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
| **Esperando** | El arco con su remolino turquesa; dentro de la abertura, un núcleo de luz que late y chispas azules y violetas girando hacia el centro. |
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
| `Size` | (2,188; 2,801) | Tamaño del portal (el arco entero) y de su zona de entrada. |
| `Opening Scale` | (0,39; 0,73) | Tamaño de la abertura respecto a `Size`: ahí se dibujan el núcleo de luz y el remolino de chispas (por defecto 0,8 × 0,7). |
| `Swirl Color A` / `B` | azul / violeta | Colores de las chispas. |
| `Fade Time` | 0,8 | Duración del fundido a negro (s). |
| `Label` / `Show Label` | «Portal de salida» / no | Etiqueta provisional. |

## Coste

Nido: un SpriteRenderer, un trigger y tres sistemas de partículas pequeños (máx. 8, 24 y 16). Altar: un SpriteRenderer, un trigger, dos sprites de brillo, dos sistemas de partículas (máx. 10 y 20) y dos `TextMesh`. Sin consultas físicas por frame.

## Pendiente

- Colocarlos en los niveles (y `System_GameProgress` en cada uno).
- Variante de piedra del checkpoint (el inventario la menciona; misma función).
- Santuario que reaccione al rescate (evento `Rescued`) e indicador de huevos en pantalla (GDD 8.1).
- Fundido de entrada al empezar el nivel siguiente (hoy aparece directamente).
- Añadir cada nivel nuevo a Build Settings para que el portal pueda cargarlo.
- «La escena cambia visualmente» al rescatar (GDD 5.2): hoy solo hay estallido y texto.
