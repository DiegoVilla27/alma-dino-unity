# Piezas de nivel — plataformas y recursos

**Estado (7 de octubre de 2026):** implementadas en Unity 6000.6.0f1 siete piezas reutilizables (plataforma atravesable, plataforma que se desmorona con tres variantes, hongo saltarín, piso rompible con Pisotón, espora de recarga del Dash, barrera rompible con Dash con dos variantes y corriente de viento). **Todas tienen arte final** (ver [Arte](#arte)) salvo la corriente de viento, cuyo visual son solo los trazos de partículas. La barrera y el viento están comprobados con una prueba PlayMode (caminando contra la barrera no se rompe; con Dash sí y Alma la atraviesa; el viento de 22 empuja a Alma ~4 u en 1 s y su efecto se apaga al salir). El resto todavía no está colocado en ninguna escena (ver [Pendiente](#pendiente)).

Los puzles (roca de basalto movible y balancín con contrapeso, runa y compuerta) tienen su propia ficha: [Puzles](Puzzles.md).

Fichas de diseño originales: [inventario, Recursos](../INVENTARIO_GAMEPLAY_PREFABS.md#recursos). Trampas: [Zonas de peligro](Hazards.md). Jugador: [Alma](../Player/Alma.md).

## Resumen

| Prefab | Etiqueta | Arte | `Size` por defecto | Qué hace | Mundo |
| --- | --- | --- | --- | --- | --- |
| `Platform_OneWay_Universal` | Plataforma atravesable | Tronco con musgo y lianas | 3 × 0,297 | Se atraviesa saltando desde abajo; se pisa desde arriba. No se puede bajar a través. | Todos |
| `Platform_CrumblingLeaf_Jungle` | Hoja que se desmorona | Hoja con lianas | 2,5 × 0,297 | Se rompe 1 s después de pisarla; vuelve a los 2,5 s. | 1 |
| `Platform_CrumblingLilypad_Swamp` | Nenúfar que se desmorona | Nenúfar con flor de loto | 2,5 × 0,645 | Igual, pero se rompe a los 0,65 s. | 3 |
| `Platform_CrumblingLedge_Volcano` | Cornisa que colapsa | Losa de basalto con musgo | 2,5 × 0,395 | Igual, pero se rompe a los 1,5 s. | 4 |
| `Resource_BouncyMushroom_Jungle` | Hongo saltarín | Hongo rojo de motas | 1,602 × 1,402 | Al caer encima lanza a Alma hacia arriba (17 m/s; ×1,18 manteniendo salto) y le recarga el doble salto y el Dash. | 1 |
| `Resource_PoundBreakableFloor_Universal` | Piso rompible | Losas agrietadas | 3 × 0,559 | Se rompe con un Pisotón encima y Alma sigue cayendo. | 2–4 |
| `Resource_DashRefillSpore_Swamp` | Espora Dash | Orbe cian con pétalos | 0,699 × 0,801 | Flota; al tocarla en el aire recarga el Dash (y el doble salto). Vuelve a los 2,5 s. | 3 |
| `Resource_DashReedBarrier_Swamp` | Barrera de cañas | Haz de cañas atado | 0,785 × 3 | Sólida; **solo** se rompe si Alma choca contra ella haciendo un Dash. | 3 |
| `Resource_DashTrialGrid_Volcano` | Reja Dash | Barrotes de piedra con glifos | 0,57 × 3 | Igual que la barrera de cañas (prueba aérea del templo). | 4 |
| `Resource_WindCurrent_Universal` | Corriente de viento | Sin imagen: área invisible + trazos | 6 × 3 | Empuja a Alma y a los objetos físicos en su dirección; puede contrarrestar la gravedad. El Dash y el Pisotón la ignoran. | 3 (reutilizable) |

Todas están en `Assets/Prefabs/Level/Pieces/`, a escala 1 × 1, y **no hacen daño**. `Size` es siempre la caja de juego (el colisionador); el dibujo puede sobresalir (ver [Margen de arte](#margen-de-arte)).

## Archivos

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `Pieces/Scripts/LevelPieceUtility.cs` | Utilidades comunes: `ApplySize` (colisionador = `Size`; sprite = `Size` + margen de `PieceArt2D`; un sprite *Simple* pasa a *Tiled*, *Tiled* y *Sliced* se respetan), reconocer a Alma y comprobar si está de pie encima. |
| `Pieces/Scripts/PieceArt2D.cs` | Margen de arte alrededor de la caja de juego y sincronización del tamaño con la herramienta Rect. Está en todas las piezas, y también en los prefabs de [progresión](Progression.md) y [puzles](Puzzles.md), que usan `LevelPieceUtility.ApplySize`. |
| `Pieces/Scripts/OneWayPlatform2D.cs` | Plataforma atravesable. |
| `Pieces/Scripts/CrumblingPlatform2D.cs` | Plataforma que se desmorona. |
| `Pieces/Scripts/BouncyMushroom2D.cs` | Hongo saltarín. |
| `Pieces/Scripts/BreakableFloor2D.cs` | Piso rompible con Pisotón. |
| `Pieces/Scripts/DashRefillSpore2D.cs` | Espora de recarga del Dash. |
| `Pieces/Scripts/DashBreakableBarrier2D.cs` | Barrera rompible con Dash. |
| `Pieces/Scripts/WindCurrentZone2D.cs` | Corriente de viento. |
| `Pieces/Sprites/*.png` | Un sprite por pieza, con el nombre de su prefab. |
| `Pieces/Platform_Crumbling_Base.prefab` | Base de las plataformas que se desmoronan (marcador provisional: `Level_Placeholder` teñido (0,6; 0,5; 0,35), 2,5 × 0,4). Las variantes cambian sprite, tamaño, tiempo de colapso, color de los trozos y etiqueta. |
| `Pieces/Resource_DashBarrier_Base.prefab` | Base de las barreras (marcador provisional: `Level_Placeholder` teñido (0,55; 0,65; 0,3), 0,8 × 3). Las variantes cambian sprite, tamaño, color de los trozos y etiqueta. |
| `Shared/Sprites/Level_Placeholder.png` | Cuadrado blanco compartido con las trampas (4 × 4 px a 4 PPU). Lo usan los prefabs base y el viento. |

Los efectos (texturas de polvo, trozos y brillo, y los sistemas de partículas) usan `HazardFx`, y la etiqueta usa `HazardZone2D.CreatePlaceholderLabel`, de la carpeta de trampas.

## Arte

Generado con IA (una generación por pieza, 7/10/2026) usando como referencia su viñeta de las hojas de concepto `Assets/Art/Resources/Resources_ConceptSheet_01_v1.png` y `_02_v1.png`, y procesado para Unity: recorte, escala a **256 PPU** (el dibujo completo mide el `Size` por defecto), malla *Full Rect* y uniones sin costura por el camino de menor diferencia (*image quilting*).

| Pieza | Concepto | Sprite (px) | `Draw Mode` | Borders (px: izq., abajo, der., arriba) | Margen (`PieceArt2D`) | Tamaño del sprite por defecto |
| --- | --- | --- | --- | --- | --- | --- |
| Tronco atravesable | #02 | 768 × 162 | **Tiled**: extremos fijos, centro repetido | 101, 0, 110, 0 | (0; 0,168) — hojas arriba y abajo | 3 × 0,63 |
| Hoja | #06 | 640 × 288 | **Sliced**: extremos fijos, centro **estirado** (repetida parecía dos hojas) | 113, 0, 111, 0 | (0; 0,414) — lianas colgando | 2,5 × 1,13 |
| Nenúfar | #07 | 640 × 363 | **Sliced** sin borders: se escala como una sola imagen | — | (0; 0,387) — la flor sobresale | 2,5 × 1,42 |
| Cornisa | #08 | 640 × 183 | Tiled | 127, 0, 64, 0 | (0; 0,16) — helecho y dientes de roca | 2,5 × 0,72 |
| Piso rompible | #10 | 768 × 143 | Tiled | 53, 0, 53, 0 | — | 3 × 0,56 |
| Barrera de cañas | #13 | 201 × 768 | Tiled **en vertical**: puntas y base fijas, cañas repetidas | 0, 87, 0, 178 | — | 0,79 × 3 |
| Reja Dash | #14 | 146 × 768 | Tiled en vertical: bloques con glifo fijos, barrotes repetidos | 0, 136, 0, 146 | — | 0,57 × 3 |
| Hongo | #03 | 410 × 359 | Sliced sin borders | — | — | 1,6 × 1,4 |
| Espora | #19 | 179 × 205 | Sliced sin borders | — | — | 0,7 × 0,8 |

- **Tile Mode = Adaptive** en todas las piezas con arte: en *Tiled* encaja un número entero de repeticiones (estirándolas un poco), así que nunca queda un trozo cortado junto a un extremo.
- **Sin tinte:** el `SpriteRenderer` va en blanco (1; 1; 1; 1). El color de los trozos al romperse lo da `Debris Color` en cada pieza.
- **Etiquetas provisionales** desactivadas (`Show Label` = no) en todas las piezas colocables; solo los prefabs base la tienen activa.
- **Alto:** las piezas horizontales se ven bien con su alto por defecto. Si se cambia `Size.y`, el centro se repite (Tiled) o se estira (Sliced) también en vertical.
- **Objetos únicos** (hongo, espora, nenúfar): conviene mantener su proporción al cambiar el tamaño, porque se estiran como una imagen.

### Margen de arte

`PieceArt2D` (`Margin`, en unidades por lado): el sprite mide `Size + 2 × Margin` y va centrado sobre la caja de juego. Así las hojas sobre el tronco, la flor del nenúfar o los dientes bajo la cornisa son decoración y no cambian dónde se apoya Alma ni contra qué choca. Para centrar la caja, las imágenes llevan filas transparentes donde hace falta.

### Cambiar el tamaño

Con `Size` en el componente de la pieza **o arrastrando los tiradores del sprite con la herramienta Rect (T)**; nunca con la escala del Transform. `PieceArt2D` (con `[ExecuteAlways]`, solo actúa en modo edición) detecta el cambio del sprite, escribe `Size = tamaño del sprite − 2 × Margin` en la pieza (con deshacer y como *override* del prefab) y la pieza recoloca sprite y colisionador. Al dar Play se conserva lo dibujado.

## Plataforma atravesable (`Platform_OneWay_Universal`)

- Colisionador sólido con un `PlatformEffector2D` en modo de un solo sentido, que el script añade y configura al empezar.
- Alma la atraviesa al saltar desde abajo (y de lado) y se apoya en ella al caer desde arriba. **No se puede bajar a través de ella** (decisión de diseño del 5/10/2026).

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (3; 0,297) | Caja de juego (el tronco; las hojas son margen de arte). |
| `Surface Arc` | 160 | Ángulo (°) de la superficie que sostiene: solo la cara de arriba. |
| `Label` / `Show Label` | «Plataforma atravesable» / no | Etiqueta provisional. |

Gizmo: flecha azul hacia arriba (se atraviesa desde abajo).

## Plataforma que se desmorona (`Platform_Crumbling*`)

| Fase | Duración | Qué pasa |
| --- | --- | --- |
| **Sólida** | — | Espera a que Alma aterrice o esté de pie encima (no basta con tocarla de lado o por debajo). |
| **Temblor** | `Collapse Time` | Tiembla en horizontal, cada vez más fuerte (de 0,5× a 1,5× de `Shake Intensity`). |
| **Desaparecida** | `Respawn Time` | Suelta 12 trozos de `Debris Color` que caen girando; se ocultan el sprite y el colisionador. |
| **Reaparece** | `Reform Time` | Cuando ha pasado el tiempo **y Alma no está en su espacio**, aparece poco a poco y vuelve a ser sólida. |

Al reaparecer Alma (evento `Respawned`) se restaura al momento.

| Campo | Base | Hoja | Nenúfar | Cornisa | Uso |
| --- | ---: | ---: | ---: | ---: | --- |
| `Size` | (2,5; 0,4) | (2,5; 0,297) | (2,5; 0,645) | (2,5; 0,395) | Caja de juego. |
| `Collapse Time` | 1 | 1 | 0,65 | 1,5 | Tiempo de temblor antes de romperse (s). |
| `Respawn Time` | 2,5 | 2,5 | 2,5 | 2,5 | Tiempo desaparecida (s). |
| `Shake Intensity` | 0,05 | 0,05 | 0,05 | 0,05 | Amplitud del temblor. |
| `Reform Time` | 0,25 | 0,25 | 0,25 | 0,25 | Tiempo en reaparecer (s). |
| `Debris Color` | (0,45; 0,75; 0,3) | (0,45; 0,72; 0,25) | (0,4; 0,7; 0,3) | (0,36; 0,33; 0,37) | Color de los trozos. |
| `Label` | «Plataforma que se desmorona» | «Hoja que se desmorona» | «Nenúfar que se desmorona» | «Cornisa que colapsa» | Etiqueta provisional. |
| `Show Label` | sí | no | no | no | |

Los tiempos salen del inventario: hoja 1 s, nenúfar 0,65 s; para la cornisa se usa 1,5 s (Level 4-3). El GDD usa 1,0 / 0,75 / 0,65 s para las hojas según el nivel: se ajusta `Collapse Time` en cada instancia.

## Hongo saltarín (`Resource_BouncyMushroom_Jungle`)

- Sólido. Cuando Alma **cae o se apoya sobre el sombrero** (no por los lados ni mientras sube), llama a `AlmaMotor2D.Bounce`:
  - velocidad vertical de 17 m/s, o 20,06 m/s (×1,18) si mantiene el salto;
  - mientras sube por el rebote, soltar el salto **no** corta la subida;
  - le recarga el doble salto y el Dash;
  - cancela un Pisotón que caiga sobre él (rebota en lugar de terminar).
- Espera 0,15 s entre rebotes.
- **Visual:** se aplasta y rebota con un pequeño vaivén (hasta 70 % de alto y 120 % de ancho) con la base fija, y suelta 8 esporas hacia arriba.
- La caja de juego es el dibujo completo (1,6 × 1,4), así que el rebote se activa en lo alto del sombrero.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (1,602; 1,402) | Caja de juego (= dibujo). |
| `Bounce Speed` | 17 | Velocidad del rebote (m/s). |
| `Held Jump Multiplier` | 1,18 | Multiplicador si se mantiene el salto. |
| `Restore Air Abilities` | sí | Recargar doble salto y Dash. |
| `Cooldown` | 0,15 | Tiempo mínimo entre rebotes (s). |
| `Squash Time` | 0,25 | Duración del aplastamiento (s). |
| `Spore Color` | (1; 0,85; 0,95; 85 %) | Color de las esporas. |
| `Label` / `Show Label` | «Hongo saltarín» / no | Etiqueta provisional. |

Con la física de Alma, el rebote sube unos 6,7 m sin mantener el salto y unos 9,3 m manteniéndolo (estimado).

## Piso rompible con Pisotón (`Resource_PoundBreakableFloor_Universal`)

- Sólido. Escucha `GroundPoundLanded`: si Alma termina un Pisotón **de pie sobre este piso**, se rompe.
- **Al romperse:** 16 trozos de roca de `Debris Color` girando y 10 nubes de polvo, temblor de cámara leve, y desaparecen el sprite y el colisionador. Alma sigue cayendo.
- Permanece roto. Al reaparecer Alma (evento `Respawned`) vuelve a su estado si `Restore On Respawn` está activo.
- `Break()` es público por si otro mecanismo debe romperlo.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (3; 0,559) | Caja de juego (= dibujo). |
| `Restore On Respawn` | sí | Restaurar al reaparecer Alma. |
| `Dust Color` | (0,7; 0,66; 0,6; 80 %) | Color del polvo. |
| `Debris Color` | (0,42; 0,38; 0,4) | Color de los trozos de roca. |
| `Shake Amplitude` / `Shake Duration` | 0,12 / 0,18 | Temblor de cámara al romperse. |
| `Label` / `Show Label` | «Piso rompible» / no | Etiqueta provisional. |

## Espora de recarga del Dash (`Resource_DashRefillSpore_Swamp`)

- Trigger (no es plataforma). Flota arriba y abajo y late suavemente.
- Al tocarla, llama a `AlmaMotor2D.RefillAirAbilities`: recarga el Dash (y quita su espera de 0,4 s, para encadenar Dashes) y el doble salto. **Solo se gasta si recargó algo:** en el suelo, o con el Dash disponible, Alma la atraviesa sin consumirla.
- **Cómo se usa:** salta, haz un Dash y pasa por la espora: en cuanto termine el Dash puedes hacer otro, sin la espera habitual. Varias esporas seguidas permiten encadenar Dashes en el aire.
- Al gastarse estalla en 10 destellos cian, desaparece 2,5 s y vuelve creciendo en 0,2 s. Al reaparecer Alma vuelve a estar disponible.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (0,699; 0,801) | Tamaño del trigger (= dibujo). |
| `Respawn Time` | 2,5 | Tiempo hasta volver (s). |
| `Float Frequency` / `Float Amplitude` | 3 / 0,15 | Flotación. |
| `Refill Double Jump` | sí | Recargar también el doble salto. |
| `Regrow Time` | 0,2 | Tiempo en volver a crecer (s). |
| `Burst Color` | (0,55; 0,9; 1; 90 %) | Color del estallido (cian, a juego con el arte; antes amarillo). |
| `Label` / `Show Label` | «Espora Dash» / no | Etiqueta provisional. |

## Barrera rompible con Dash (`Resource_DashBarrier_Base`, `Resource_DashReedBarrier_Swamp`, `Resource_DashTrialGrid_Volcano`)

- Sólida. **Solo se rompe con el Dash:** al chocar con ella mientras `IsDashing`. Caminar contra ella, saltar, caer encima o hacer un Pisotón no le hacen nada.
- Al romperse, el colisionador se apaga en ese mismo paso (el Dash sigue y la atraviesa), suelta 18 trozos de `Debris Color` hacia donde iba el Dash y se desvanece en `Fade Time`.
- Por defecto queda abierta. Con `Can Respawn` vuelve a los `Respawn Delay` s, solo cuando Alma no está dentro de su hueco.
- Al reaparecer Alma vuelve a estar entera (`Restore On Respawn`).
- `Break(direction)` es público, por si otro mecanismo tiene que romperla.

| Campo | Base | Cañas | Reja | Uso |
| --- | ---: | ---: | ---: | --- |
| `Size` | (0,8; 3) | (0,785; 3) | (0,57; 3) | Caja de juego (= dibujo). |
| `Fade Time` | 0,2 | 0,2 | 0,2 | Desvanecimiento (s). |
| `Can Respawn` / `Respawn Delay` | no / 5 | no / 5 | no / 5 | Volver a cerrarse sola y tras cuánto (s). |
| `Restore On Respawn` | sí | sí | sí | Restaurarla al reaparecer Alma. |
| `Debris Color` | (0,55; 0,65; 0,3) | (0,6; 0,65; 0,3) | (0,3; 0,28; 0,32) | Color de los trozos. |
| `Label` | «Barrera Dash» | «Barrera de cañas» | «Reja Dash» | Etiqueta provisional. |
| `Show Label` | sí | no | no | |

## Corriente de viento (`Resource_WindCurrent_Universal`)

- Trigger. Mientras Alma está dentro recibe una aceleración en `Direction` de `Strength` (22 u/s²). Con un rozamiento propio de 4, la velocidad que aporta el viento tiende a `Strength / 4` (≈5,5 u/s con 22), que se **suma** al movimiento de Alma: caminar a favor es más rápido y en contra más lento. Al salir, ese empuje se va apagando en unas décimas.
- `Gravity Compensation` (0–1,5) anula esa fracción de la gravedad mientras está dentro: 1 = flota, más de 1 = corriente ascendente.
- **El Dash y el Pisotón ignoran el viento** (durante el impulso no empuja ni frena).
- También empuja cualquier cuerpo físico dinámico que entre (por ejemplo el contrapeso del balancín).
- `Active` la enciende o apaga (un mecanismo puede cambiarla).
- **Visual:** sin imagen a propósito. El área (`Level_Placeholder` en Tiled) tiene **alfa 0** y orden de dibujo −1, así que en juego solo se ven los trazos blancos que fluyen en su dirección (1,2 trazos por unidad²). Gizmo: rectángulo y flecha con la dirección.
- No hace daño, pero puede empujar a Alma hacia un peligro.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (6; 3) | Área. |
| `Direction` | (1; 0) | Dirección (se normaliza). |
| `Strength` | 22 | Aceleración (u/s²). |
| `Gravity Compensation` | 0 | Fracción de la gravedad que anula. |
| `Active` | sí | Encendida. |
| `Streak Color` | (0,9; 0,97; 1; 55 %) | Color de los trazos. |
| `Streaks Per Unit` | 1,2 | Trazos por unidad². |
| `Label` / `Show Label` | «Corriente de viento» / no | Etiqueta provisional. |

## Cambios en Alma

`AlmaMotor2D` tiene métodos que usan estas piezas (documentados en [Alma](../Player/Alma.md)):

- `Bounce(speed, heldMultiplier, refillAirAbilities)`: rebote del hongo.
- `RefillAirAbilities(dash, doubleJump)`: recarga de la espora; devuelve si recargó algo.
- `AddWind(acceleration, gravityCompensation)`: empuje de la corriente de viento en este paso de física.

## Coste

Por pieza: un SpriteRenderer, un `BoxCollider2D`, `PieceArt2D` (no hace nada en juego) y un sistema de partículas pequeño (máx. 12–24) que solo emite en su momento. La plataforma atravesable añade un `PlatformEffector2D`. Sin consultas físicas por frame, salvo la plataforma que se desmorona y la barrera cuando esperan para reaparecer (una `OverlapBox`).

## Pendiente

- Colocar las piezas en niveles y probarlas.
- Plataforma atravesable: si Alma llega al punto más alto del salto justo dentro de la plataforma, la detección de suelo podría darla por apoyada; usar plataformas finas y probarlo.
- El hongo se aplasta escalando el sprite entero; no tiene animación propia de compresión.
- Reja Dash: el arte es una columna estrecha de barrotes (0,57 de ancho) en lugar de la reja ancha del concepto; se aceptó así.
- Recursos restantes del inventario sin hacer (catapulta de raíces, campanas, etc.).
