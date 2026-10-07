# Puzles — roca de basalto y balancín

**Estado (7 de octubre de 2026):** implementados en Unity 6000.6.0f1 la roca de basalto movible con el Rugido y el conjunto del balancín (balancín, contrapeso, runa y compuerta rúnica), todos con **arte final** (ver [Arte](#arte)). Comprobados con una prueba PlayMode: el Rugido empuja la roca 5 u y, sobre lava, forma un puente; un Pisotón en el balancín lanza el contrapeso, que activa la runa y abre la compuerta, y Alma sale catapultada (~14,6 u/s) desde el extremo levantado.

Fichas de diseño originales: [inventario, Recursos](../INVENTARIO_GAMEPLAY_PREFABS.md#recursos) y [Level 2-2](../Levels/World_2_Caves/Level_2_2.md). Otras piezas: [plataformas y recursos](Pieces.md), [trampas](Hazards.md). Jugador: [Alma](../Player/Alma.md).

**Comprobado de nuevo (7/10/2026)** tras el arte y los tamaños nuevos, con pruebas PlayMode en una copia del proyecto: el Rugido empuja la roca 5 u, sobre su lava se hunde y aparece la costra del puente; un Pisotón en el balancín lanza el contrapeso, que enciende la runa, y la compuerta se abre. Las piezas que buscan a Alma al empezar (balancín, roca) necesitan que Alma ya exista en la escena.

## Resumen

| Prefab | Carpeta | Etiqueta | Arte | `Size` | Qué hace | Mundo |
| --- | --- | --- | --- | --- | --- | --- |
| `Resource_RoarBoulder_Volcano` | `Level/Puzzles/` | Roca movible | Roca de basalto con musgo + costra de basalto para el puente | 1,801 × 1,488 | El Rugido la empuja en arco; sobre su lava forma un puente. | 4 |
| `Resource_SeesawCatapult_Caves` | `Level/Puzzles/Seesaw/` | Balancín | Tablón de piedra con glifos dorados y cuerdas + pivote con eje dorado | 6 × 0,594 | Se inclina al pisarlo; un Pisotón en un extremo lanza lo que haya en el otro. | 2 |
| `Resource_CatapultCounterweight_Caves` | `Level/Puzzles/Seesaw/` | Contrapeso | Basalto con banda de bronce y runa en espiral | 0,898 × 1,074 | Bloque que sale disparado del balancín y activa runas al subir. | 2 |
| `Resource_RuneSwitch_Caves` | `Level/Puzzles/Seesaw/` | Runa | Medallón de piedra colgado de cuerdas, rombo turquesa | 1 × 0,965 | Se activa 4 s solo si la atraviesa un contrapeso lanzado que sube. | 2 |
| `Resource_TimedRuneGate_Caves` | `Level/Puzzles/Seesaw/` | Compuerta rúnica | Losa de piedra tallada con reloj de arena y línea turquesa | 0,836 × 4 | Se abre mientras todas sus runas están activas. | 2 |

Scripts en `Assets/Prefabs/Level/Puzzles/Scripts/` (namespace `AlmaGame.Level`, ensamblado `Assembly-CSharp`): `PushableBoulder2D`, `SeesawPlatform2D`, `CatapultWeight2D`, `RuneSwitch2D` y `TimedRuneGate2D`. Usan los efectos de `HazardFx` y la etiqueta de `HazardZone2D`. Todo se dimensiona con `Size` (no con la escala) o con la herramienta Rect (`PieceArt2D`, ver [Piezas](Pieces.md#cambiar-el-tamaño)).

## Arte

Generado con IA el 7/10/2026 (6 generaciones) con su viñeta de las hojas de concepto como referencia (#20 roca, #15 balancín y #17 contrapeso en `Resources_ConceptSheet_02_v1.png`; #21 runa y #22 compuerta en `_03_v1.png`), y procesado a **256 PPU**, malla *Full Rect*, color blanco y escala 1 × 1. Etiquetas provisionales desactivadas. Sprites en `Assets/Prefabs/Level/Puzzles/Sprites/`.

| Pieza | Sprite | Dibujo | Notas |
| --- | --- | --- | --- |
| Roca | `Resource_RoarBoulder_Volcano.png` (461 × 381) | *Sliced* sin borders | Orden de dibujo −1: queda detrás de la lava al hundirse. |
| Puente | `Resource_RoarBoulder_Volcano_Bridge.png` (1075 × 138, borders 90 / 90) | *Tiled* Adaptive al ancho de `Bridge Size` | Sin concepto propio: costra de basalto con grietas de brasa, en el estilo de la roca. |
| Balancín | `Resource_SeesawCatapult_Caves_Beam.png` (1536 × 240, borders 306 / 290) | *Tiled* Adaptive: glifos y cuerdas de los extremos fijos, centro repetido | `PieceArt2D` margen 0,172 (las cuerdas cuelgan bajo la caja). |
| Pivote | `Resource_SeesawCatapult_Caves_Pivot.png` (463 × 431) | *Simple* | Generado junto al tablón (piezas separadas) para que encajen. |
| Contrapeso | `Resource_CatapultCounterweight_Caves.png` (230 × 275) | *Sliced* sin borders | |
| Runa | `Resource_RuneSwitch_Caves.png` (256 × 549) | *Sliced* sin borders | La caja es el medallón; las cuerdas son margen de arte (`PieceArt2D` 0,59). |
| Compuerta | `Resource_TimedRuneGate_Caves.png` (214 × 1024, borders abajo 94 / arriba 188) | *Tiled* Adaptive **en vertical** | La puerta sola (sin marco): sube al abrirse. |

## Roca de basalto movible (`Resource_RoarBoulder_Volcano`)

Primer objeto que reacciona al Rugido: implementa `IRoarTarget` (el Rugido de Alma la detecta con su cono frontal de 3 u).

| Fase | Qué pasa |
| --- | --- |
| **En reposo** | Sólida: Alma puede pisarla o chocar con ella. |
| **Empujada** | Un Rugido mientras reposa la lanza hacia donde mira Alma: avanza 5 u en 0,8 s (salida suavizada) describiendo un arco de 0,6 u de alto, con polvo al salir y al caer. Si hay una pared antes (barrido de su caja), se detiene en ella. |
| **Sobre su lava** | Si cae encima de su lava vinculada (`Linked Lava`, una trampa de lava), salpica lava y vapor, se hunde en 0,4 s y aparece un **puente sólido** de 4,2 × 0,2 sobre la superficie de la lava. Mientras se hunde, la roca se desvanece y la **costra de basalto** (`Bridge Sprite`, del ancho del puente, con su borde superior a ras del puente) aparece: la roca «se funde» en el puente. Ya no se puede empujar. |
| **Fuera de la lava** | Queda en reposo donde cayó y se puede volver a empujar. |

- No hace daño (tampoco al moverse, aunque puede desplazar a Alma).
- Al reaparecer Alma vuelve a su posición inicial (visible otra vez) y el puente y su costra desaparecen.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (1,801; 1,488) | Tamaño (= dibujo). |
| `Push Distance` / `Push Time` | 5 / 0,8 | Avance por Rugido (u) y duración (s). |
| `Arc Height` | 0,6 | Altura del arco. |
| `Linked Lava` | — | Trampa de lava (`HazardZone2D`) sobre la que forma el puente. **Asignar en la escena.** |
| `Bridge Size` | (4,2; 0,2) | Tamaño del puente sólido. |
| `Sink Time` | 0,4 | Tiempo en hundirse (s). |
| `Bridge Sprite` | costra de basalto | Arte del puente (vacío = puente invisible, como antes). |

Gizmos (al seleccionarla): alcance de un empujón a cada lado y línea hasta su lava.

## Conjunto del balancín (Level 2-2)

Montaje típico: balancín sobre el suelo, contrapeso apoyado en un extremo, runa en el techo encima de ese extremo y compuerta cerrando el paso. Un Pisotón en el extremo **contrario** al contrapeso lo lanza contra la runa, y la compuerta se abre 4 s. Con dos runas enlazadas, la compuerta necesita las dos activas a la vez.

### Balancín (`Resource_SeesawCatapult_Caves`)

- Tablero sobre un pivote central (el pivote, `Pivot Sprite`, no gira: queda con su eje dorado sobre el centro del tablón y dibujado delante, como un pasador). Gira con un `Rigidbody2D` cinemático, así que lo que hay encima se mueve con él.
- **Caminar encima:** se inclina despacio hacia el lado de Alma (hasta el 40 % del ángulo máximo, a 25°/s).
- **Pisotón en un extremo** (fuera del 15 % central): ese extremo baja de golpe hasta 18° en 0,12 s, con polvo y un temblor de cámara leve. Los contrapesos del otro extremo salen lanzados a 14 u/s.
- **Catapulta (2,8 s):** si en esa ventana Alma corre hasta el extremo levantado (más allá del 40 % de su mitad), sale lanzada a 15 u/s y recupera el doble salto y el Dash. Una vez por Pisotón.
- Después vuelve a nivelarse (20°/s).

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (6; 0,594) | Tablero (sin las cuerdas, que son margen de arte). |
| `Pivot Sprite` / `Pivot Drop` | pivote / 0,519 | Dibujo del pivote y distancia del centro del tablón al centro del pivote (u). |
| `Max Angle` | 18 | Inclinación máxima (°). |
| `Walk Tilt Fraction` / `Walk Tilt Speed` | 0,4 / 25 | Inclinación al caminar y su velocidad (°/s). |
| `Slam Time` | 0,12 | Duración del golpe (s). |
| `Launch Window` | 2,8 | Ventana de catapulta (s). |
| `Alma Launch Speed` / `Weight Launch Speed` | 15 / 14 | Impulsos (u/s). |
| `Return Speed` | 20 | Vuelta a nivel (°/s). |

### Contrapeso (`Resource_CatapultCounterweight_Caves`)

- Se coloca **encima de un extremo del balancín**: al empezar busca el balancín más cercano (o el asignado en `Seesaw`) y recuerda en qué punto y lado reposa. Mientras reposa viaja con el tablero.
- Al lanzarlo pasa a ser un cuerpo físico (gravedad ×2,2, gira un poco); mientras sube puede activar runas. Le afecta el viento.
- A los 4,7 s del lanzamiento (o al reaparecer Alma) vuelve a su sitio apareciendo poco a poco (0,25 s).

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Seesaw` | (el más cercano) | Balancín sobre el que reposa. |
| `Size` | (0,898; 1,074) | Tamaño (= dibujo). |
| `Reset Time` | 4,7 | Tiempo hasta volver a su sitio (s). |
| `Reappear Time` | 0,25 | Aparición al volver (s). |

### Runa (`Resource_RuneSwitch_Caves`)

- Trigger. **Solo** la activa un contrapeso lanzado que aún sube; Alma tocándola no hace nada.
- En reposo el medallón se ve **atenuado** (`Idle Tint`). Activa durante 4 s: brilla a plena luz (`Active Tint`), con un halo turquesa-verde detrás, suelta un destello y una barra bajo ella muestra el tiempo que queda; el halo parpadea el último segundo. Otro impacto reinicia el tiempo.
- Se apaga al reaparecer Alma.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (1; 0,965) | El medallón (las cuerdas son margen de arte). |
| `Signal Time` | 4 | Tiempo activa (s). |
| `Active Color` | (0,4; 1; 0,8) | Color del halo, el destello y la barra. |
| `Idle Tint` / `Active Tint` | (0,55; 0,6; 0,7) / blanco | Tinte del sprite en reposo y activa. |

### Compuerta rúnica (`Resource_TimedRuneGate_Caves`)

- Sólida. Se abre (sube su altura en 0,25 s, sin colisionador) mientras **todas** sus runas están activas a la vez.
- Al apagarse una runa se cierra, **salvo que Alma esté en el hueco**: entonces espera; nunca aplasta. El colisionador vuelve solo cuando está cerrada del todo.
- Runas: las de `Runes`; si la lista está vacía, enlaza todas las runas a menos de `Auto Link Radius` (20 u). Gizmo: líneas verdes a sus runas.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (0,836; 4) | Tamaño (= dibujo). |
| `Runes` | vacío | Runas que la abren (vacío = automático). |
| `Auto Link Radius` | 20 | Radio del enlace automático. |
| `Slide Time` | 0,25 | Tiempo de apertura/cierre (s). |

## Coste

Roca: un SpriteRenderer, un `BoxCollider2D`, otro para el puente y dos sistemas de partículas (máx. 16 y 20); un `BoxCast` por Rugido. Balancín: un SpriteRenderer, un `BoxCollider2D`, un `Rigidbody2D` cinemático, el sprite del pivote y un sistema de partículas (máx. 16). Contrapeso: un `Rigidbody2D` (cinemático en reposo). Runa: trigger, dos sprites y un sistema de partículas (máx. 16). Compuerta: un `BoxCollider2D` y una `OverlapBox` por frame mientras debe cerrarse.

## Pendiente

- Colocarlas en los niveles (Level 2-2 y 4-1).
- Catapulta de raíces (Mundo 3), variante del balancín que lanza a Alma.
- «Retorno desde la derecha» y zona de recuperación de la compuerta del diseño original.
