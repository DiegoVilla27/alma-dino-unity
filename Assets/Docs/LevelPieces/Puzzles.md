# Puzles — roca de basalto y balancín

**Estado (6 de octubre de 2026):** implementados en Unity 6000.6.0f1 la roca de basalto movible con el Rugido y el conjunto del balancín (balancín, contrapeso, runa y compuerta rúnica), con **cajas de color y etiqueta** como marcador. Comprobados con una prueba PlayMode: el Rugido empuja la roca 5 u y, sobre lava, forma un puente; un Pisotón en el balancín lanza el contrapeso, que activa la runa y abre la compuerta, y Alma sale catapultada (~14,6 u/s) desde el extremo levantado.

Fichas de diseño originales: [inventario, Recursos](../INVENTARIO_GAMEPLAY_PREFABS.md#recursos) y [Level 2-2](../Levels/World_2_Caves/Level_2_2.md). Otras piezas: [plataformas y recursos](Pieces.md), [trampas](Hazards.md). Jugador: [Alma](../Player/Alma.md).

## Resumen

| Prefab | Carpeta | Etiqueta | Color del marcador | Qué hace | Mundo |
| --- | --- | --- | --- | --- | --- |
| `Resource_RoarBoulder_Volcano` | `Level/Puzzles/` | Roca movible | Basalto (0,35; 0,25; 0,25), 1,8 × 1,8 | El Rugido la empuja en arco; sobre su lava forma un puente. | 4 |
| `Resource_SeesawCatapult_Caves` | `Level/Puzzles/Seesaw/` | Balancín | Gris piedra (0,55; 0,55; 0,6), 6 × 0,4 | Se inclina al pisarlo; un Pisotón en un extremo lanza lo que haya en el otro. | 2 |
| `Resource_CatapultCounterweight_Caves` | `Level/Puzzles/Seesaw/` | Contrapeso | Marrón oscuro (0,4; 0,35; 0,3), 0,9 × 0,9 | Bloque que sale disparado del balancín y activa runas al subir. | 2 |
| `Resource_RuneSwitch_Caves` | `Level/Puzzles/Seesaw/` | Runa | Azul apagado (0,3; 0,35; 0,5), 1 × 1 | Se activa 4 s solo si la atraviesa un contrapeso lanzado que sube. | 2 |
| `Resource_TimedRuneGate_Caves` | `Level/Puzzles/Seesaw/` | Compuerta rúnica | Violeta grisáceo (0,45; 0,4; 0,55), 1 × 4 | Se abre mientras todas sus runas están activas. | 2 |

Scripts en `Assets/Prefabs/Level/Puzzles/Scripts/` (namespace `AlmaGame.Level`, ensamblado `Assembly-CSharp`): `PushableBoulder2D`, `SeesawPlatform2D`, `CatapultWeight2D`, `RuneSwitch2D` y `TimedRuneGate2D`. Usan el cuadrado compartido `Level/Shared/Sprites/Level_Placeholder.png`, los efectos de `HazardFx` y la etiqueta de `HazardZone2D`. Todo se dimensiona con `Size` (no con la escala).

## Roca de basalto movible (`Resource_RoarBoulder_Volcano`)

Primer objeto que reacciona al Rugido: implementa `IRoarTarget` (el Rugido de Alma la detecta con su cono frontal de 3 u).

| Fase | Qué pasa |
| --- | --- |
| **En reposo** | Sólida: Alma puede pisarla o chocar con ella. |
| **Empujada** | Un Rugido mientras reposa la lanza hacia donde mira Alma: avanza 5 u en 0,8 s (salida suavizada) describiendo un arco de 0,6 u de alto, con polvo al salir y al caer. Si hay una pared antes (barrido de su caja), se detiene en ella. |
| **Sobre su lava** | Si cae encima de su lava vinculada (`Linked Lava`, una trampa de lava), salpica lava y vapor, se hunde en 0,4 s y aparece un **puente sólido** de 4,2 × 0,2 sobre la superficie de la lava. Ya no se puede empujar. |
| **Fuera de la lava** | Queda en reposo donde cayó y se puede volver a empujar. |

- No hace daño (tampoco al moverse, aunque puede desplazar a Alma).
- Al reaparecer Alma vuelve a su posición inicial y el puente desaparece.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (1,8; 1,8) | Tamaño. |
| `Push Distance` / `Push Time` | 5 / 0,8 | Avance por Rugido (u) y duración (s). |
| `Arc Height` | 0,6 | Altura del arco. |
| `Linked Lava` | — | Trampa de lava (`HazardZone2D`) sobre la que forma el puente. **Asignar en la escena.** |
| `Bridge Size` | (4,2; 0,2) | Tamaño del puente sólido. |
| `Sink Time` | 0,4 | Tiempo en hundirse (s). |

Gizmos (al seleccionarla): alcance de un empujón a cada lado y línea hasta su lava.

## Conjunto del balancín (Level 2-2)

Montaje típico: balancín sobre el suelo, contrapeso apoyado en un extremo, runa en el techo encima de ese extremo y compuerta cerrando el paso. Un Pisotón en el extremo **contrario** al contrapeso lo lanza contra la runa, y la compuerta se abre 4 s. Con dos runas enlazadas, la compuerta necesita las dos activas a la vez.

### Balancín (`Resource_SeesawCatapult_Caves`)

- Tablero sobre un pivote central (el pivote se dibuja debajo y no gira). Gira con un `Rigidbody2D` cinemático, así que lo que hay encima se mueve con él.
- **Caminar encima:** se inclina despacio hacia el lado de Alma (hasta el 40 % del ángulo máximo, a 25°/s).
- **Pisotón en un extremo** (fuera del 15 % central): ese extremo baja de golpe hasta 18° en 0,12 s, con polvo y un temblor de cámara leve. Los contrapesos del otro extremo salen lanzados a 14 u/s.
- **Catapulta (2,8 s):** si en esa ventana Alma corre hasta el extremo levantado (más allá del 40 % de su mitad), sale lanzada a 15 u/s y recupera el doble salto y el Dash. Una vez por Pisotón.
- Después vuelve a nivelarse (20°/s).

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (6; 0,4) | Tablero. |
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
| `Size` | (0,9; 0,9) | Tamaño. |
| `Reset Time` | 4,7 | Tiempo hasta volver a su sitio (s). |
| `Reappear Time` | 0,25 | Aparición al volver (s). |

### Runa (`Resource_RuneSwitch_Caves`)

- Trigger. **Solo** la activa un contrapeso lanzado que aún sube; Alma tocándola no hace nada.
- Activa durante 4 s: se pone verde, brilla, suelta un destello y una barra verde bajo ella muestra el tiempo que queda; el brillo parpadea el último segundo. Otro impacto reinicia el tiempo.
- Se apaga al reaparecer Alma.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (1; 1) | Tamaño. |
| `Signal Time` | 4 | Tiempo activa (s). |
| `Active Color` | verde | Color activa. |

### Compuerta rúnica (`Resource_TimedRuneGate_Caves`)

- Sólida. Se abre (sube su altura en 0,25 s, sin colisionador) mientras **todas** sus runas están activas a la vez.
- Al apagarse una runa se cierra, **salvo que Alma esté en el hueco**: entonces espera; nunca aplasta. El colisionador vuelve solo cuando está cerrada del todo.
- Runas: las de `Runes`; si la lista está vacía, enlaza todas las runas a menos de `Auto Link Radius` (20 u). Gizmo: líneas verdes a sus runas.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (1; 4) | Tamaño. |
| `Runes` | vacío | Runas que la abren (vacío = automático). |
| `Auto Link Radius` | 20 | Radio del enlace automático. |
| `Slide Time` | 0,25 | Tiempo de apertura/cierre (s). |

## Coste

Roca: un SpriteRenderer, un `BoxCollider2D`, otro para el puente y dos sistemas de partículas (máx. 16 y 20); un `BoxCast` por Rugido. Balancín: un SpriteRenderer, un `BoxCollider2D`, un `Rigidbody2D` cinemático, el sprite del pivote y un sistema de partículas (máx. 16). Contrapeso: un `Rigidbody2D` (cinemático en reposo). Runa: trigger, dos sprites y un sistema de partículas (máx. 16). Compuerta: un `BoxCollider2D` y una `OverlapBox` por frame mientras debe cerrarse.

## Pendiente

- Arte de las cinco piezas.
- Colocarlas en los niveles (Level 2-2 y 4-1).
- Catapulta de raíces (Mundo 3), variante del balancín que lanza a Alma.
- «Retorno desde la derecha» y zona de recuperación de la compuerta del diseño original.
