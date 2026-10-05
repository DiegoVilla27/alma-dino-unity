# Piezas de nivel — plataformas y recursos

**Estado (5 de octubre de 2026):** implementadas en Unity 6000.6.0f1 cinco piezas reutilizables (plataforma atravesable, plataforma que se desmorona con tres variantes, hongo saltarín, piso rompible con Pisotón y espora de recarga del Dash). Usan **cajas de color con etiqueta** como marcador hasta que llegue el arte. Todavía no están colocadas en ninguna escena ni probadas en juego (ver [Pendiente](#pendiente)).

Fichas de diseño originales: [inventario, Recursos](../INVENTARIO_GAMEPLAY_PREFABS.md#recursos). Trampas: [Zonas de peligro](Hazards.md). Jugador: [Alma](../Player/Alma.md).

## Resumen

| Prefab | Etiqueta | Color del marcador | Tamaño por defecto | Qué hace | Mundo |
| --- | --- | --- | --- | --- | --- |
| `Platform_OneWay_Universal` | Plataforma atravesable | Azul (0,4; 0,6; 1) | 3 × 0,3 | Se atraviesa saltando desde abajo; se pisa desde arriba. No se puede bajar a través. | Todos |
| `Platform_CrumblingLeaf_Jungle` | Hoja que se desmorona | Verde (0,45; 0,75; 0,3) | 2,5 × 0,4 | Se rompe 1 s después de pisarla; vuelve a los 2,5 s. | 1 |
| `Platform_CrumblingLilypad_Swamp` | Nenúfar que se desmorona | Verde azulado (0,3; 0,65; 0,5) | 2,5 × 0,4 | Igual, pero se rompe a los 0,65 s. | 3 |
| `Platform_CrumblingLedge_Volcano` | Cornisa que colapsa | Rojo tierra (0,55; 0,35; 0,3) | 2,5 × 0,4 | Igual, pero se rompe a los 1,5 s. | 4 |
| `Resource_BouncyMushroom_Jungle` | Hongo saltarín | Rosa (0,95; 0,45; 0,65) | 1,6 × 0,8 | Al caer encima lanza a Alma hacia arriba (17 m/s; ×1,18 manteniendo salto) y le recarga el doble salto y el Dash. | 1 |
| `Resource_PoundBreakableFloor_Universal` | Piso rompible | Gris piedra (0,55; 0,5; 0,45) | 3 × 0,6 | Se rompe con un Pisotón encima y Alma sigue cayendo. | 2–4 |
| `Resource_DashRefillSpore_Swamp` | Espora Dash | Amarillo (1; 0,9; 0,3) | 0,7 × 0,7 | Flota; al tocarla en el aire recarga el Dash (y el doble salto). Vuelve a los 2,5 s. | 3 |

Todas están en `Assets/Prefabs/Level/Pieces/` y **no hacen daño**.

## Archivos

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `Pieces/Scripts/LevelPieceUtility.cs` | Utilidades comunes: ajustar sprite (modo *Tiled*) y colisionador al tamaño, reconocer a Alma y comprobar si está de pie encima. |
| `Pieces/Scripts/OneWayPlatform2D.cs` | Plataforma atravesable. |
| `Pieces/Scripts/CrumblingPlatform2D.cs` | Plataforma que se desmorona. |
| `Pieces/Scripts/BouncyMushroom2D.cs` | Hongo saltarín. |
| `Pieces/Scripts/BreakableFloor2D.cs` | Piso rompible con Pisotón. |
| `Pieces/Scripts/DashRefillSpore2D.cs` | Espora de recarga del Dash. |
| `Pieces/Platform_Crumbling_Base.prefab` | Prefab base de las plataformas que se desmoronan; las tres variantes (hoja, nenúfar, cornisa) solo cambian nombre, color, tiempo de colapso y etiqueta. |
| `Pieces/*.prefab` | Un prefab por pieza: SpriteRenderer (modo *Tiled*), `BoxCollider2D` y su script. |
| `Shared/Sprites/Level_Placeholder.png` | Cuadrado blanco compartido con las trampas (antes `Hazards/Sprites/Hazard_Placeholder.png`, mismo GUID). |

Los efectos (texturas de polvo, trozos y brillo, y los sistemas de partículas) usan `HazardFx` y la etiqueta usa `HazardZone2D.CreatePlaceholderLabel`, de la carpeta de trampas.

Ajustar el tamaño con `Size` en cada componente, no con la escala del Transform: el sprite se repite en lugar de estirarse y el colisionador lo acompaña.

## Plataforma atravesable (`Platform_OneWay_Universal`)

- Colisionador sólido con un `PlatformEffector2D` en modo de un solo sentido, que el script añade y configura al empezar.
- Alma la atraviesa al saltar desde abajo (y de lado) y se apoya en ella al caer desde arriba. **No se puede bajar a través de ella** (decisión de diseño del 5/10/2026).

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (3; 0,3) | Tamaño. |
| `Surface Arc` | 160 | Ángulo (°) de la superficie que sostiene: solo la cara de arriba. |
| `Label` / `Show Label` | «Plataforma atravesable» / sí | Etiqueta provisional. |

Gizmo: flecha azul hacia arriba (se atraviesa desde abajo).

## Plataforma que se desmorona (`Platform_Crumbling*`)

| Fase | Duración | Qué pasa |
| --- | --- | --- |
| **Sólida** | — | Espera a que Alma aterrice o esté de pie encima (no basta con tocarla de lado o por debajo). |
| **Temblor** | `Collapse Time` | Tiembla en horizontal, cada vez más fuerte (de 0,5× a 1,5× de `Shake Intensity`). |
| **Desaparecida** | `Respawn Time` | Suelta 12 trozos de su color que caen girando; se ocultan el sprite y el colisionador. |
| **Reaparece** | `Reform Time` | Cuando ha pasado el tiempo **y Alma no está en su espacio**, aparece poco a poco y vuelve a ser sólida. |

Al reaparecer Alma (evento `Respawned`) se restaura al momento.

| Campo | Base | Hoja | Nenúfar | Cornisa | Uso |
| --- | ---: | ---: | ---: | ---: | --- |
| `Size` | (2,5; 0,4) | = | = | = | Tamaño. |
| `Collapse Time` | 1 | 1 | 0,65 | 1,5 | Tiempo de temblor antes de romperse (s). |
| `Respawn Time` | 2,5 | = | = | = | Tiempo desaparecida (s). |
| `Shake Intensity` | 0,05 | = | = | = | Amplitud del temblor. |
| `Reform Time` | 0,25 | = | = | = | Tiempo en reaparecer (s). |

Los tiempos salen del inventario: hoja 1 s, nenúfar 0,65 s; para la cornisa se usa 1,5 s (Level 4-3). El GDD usa 1,0 / 0,75 / 0,65 s para las hojas según el nivel: se ajusta `Collapse Time` en cada instancia.

## Hongo saltarín (`Resource_BouncyMushroom_Jungle`)

- Sólido. Cuando Alma **cae o se apoya sobre el sombrero** (no por los lados ni mientras sube), llama a `AlmaMotor2D.Bounce`:
  - velocidad vertical de 17 m/s, o 20,06 m/s (×1,18) si mantiene el salto;
  - mientras sube por el rebote, soltar el salto **no** corta la subida (no se aplica la gravedad de salto soltado);
  - le recarga el doble salto y el Dash;
  - cancela un Pisotón que caiga sobre él (rebota en lugar de terminar).
- Espera 0,15 s entre rebotes.
- **Visual:** se aplasta y rebota con un pequeño vaivén (hasta 70 % de alto y 120 % de ancho) con la base fija, y suelta 8 esporas rosadas hacia arriba.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (1,6; 0,8) | Tamaño. |
| `Bounce Speed` | 17 | Velocidad del rebote (m/s). |
| `Held Jump Multiplier` | 1,18 | Multiplicador si se mantiene el salto. |
| `Restore Air Abilities` | sí | Recargar doble salto y Dash. |
| `Cooldown` | 0,15 | Tiempo mínimo entre rebotes (s). |
| `Squash Time` | 0,25 | Duración del aplastamiento (s). |
| `Spore Color` | rosa claro | Color de las esporas. |

Con la física de Alma, el rebote sube unos 6,7 m sin mantener el salto y unos 9,3 m manteniéndolo (estimado). Ajustar `Bounce Speed` al diseñar los niveles.

## Piso rompible con Pisotón (`Resource_PoundBreakableFloor_Universal`)

- Sólido. Escucha `GroundPoundLanded`: si Alma termina un Pisotón **de pie sobre este piso**, se rompe.
- **Al romperse:** 16 trozos de roca girando y 10 nubes de polvo, temblor de cámara leve (0,12 u, 0,18 s), y desaparecen el sprite y el colisionador. Alma sigue cayendo.
- Permanece roto. Al reaparecer Alma (evento `Respawned`) vuelve a su estado si `Restore On Respawn` está activo.
- `Break()` es público por si otro mecanismo debe romperlo.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (3; 0,6) | Tamaño. |
| `Restore On Respawn` | sí | Restaurar al reaparecer Alma. |
| `Dust Color` | gris claro | Color del polvo. |
| `Shake Amplitude` / `Shake Duration` | 0,12 / 0,18 | Temblor de cámara al romperse. |

## Espora de recarga del Dash (`Resource_DashRefillSpore_Swamp`)

- Trigger (no es plataforma). Flota arriba y abajo (frecuencia 3, amplitud 0,15) y late suavemente.
- Al tocarla, llama a `AlmaMotor2D.RefillAirAbilities`: recarga el Dash (y quita su espera de 0,4 s, para encadenar Dashes) y el doble salto. **Solo se gasta si recargó algo:** en el suelo, o con el Dash disponible, Alma la atraviesa sin consumirla.
- **Cómo se usa:** salta, haz un Dash y pasa por la espora (durante o después del Dash): en cuanto termine el Dash puedes hacer otro, sin la espera habitual de 0,4 s. Varias esporas seguidas permiten encadenar Dashes en el aire.
- Al gastarse estalla en 10 destellos amarillos, desaparece 2,5 s y vuelve creciendo en 0,2 s.
- Al reaparecer Alma vuelve a estar disponible.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Size` | (0,7; 0,7) | Tamaño (y del trigger). |
| `Respawn Time` | 2,5 | Tiempo hasta volver (s). |
| `Float Frequency` / `Float Amplitude` | 3 / 0,15 | Flotación. |
| `Refill Double Jump` | sí | Recargar también el doble salto. |
| `Regrow Time` | 0,2 | Tiempo en volver a crecer (s). |
| `Burst Color` | amarillo | Color del estallido. |

## Cambios en Alma

`AlmaMotor2D` tiene dos métodos nuevos que usan estas piezas (documentados en [Alma](../Player/Alma.md)):

- `Bounce(speed, heldMultiplier, refillAirAbilities)`: rebote del hongo.
- `RefillAirAbilities(dash, doubleJump)`: recarga de la espora; devuelve si recargó algo.

## Coste

Por pieza: un SpriteRenderer, un `BoxCollider2D` y un sistema de partículas pequeño (máx. 12–24) que solo emite en su momento. La plataforma atravesable añade un `PlatformEffector2D`. Sin consultas físicas por frame, salvo la plataforma que se desmorona cuando espera para reaparecer (una `OverlapBox`).

## Pendiente

- Arte de cada pieza.
- Colocarlas en niveles y probarlas.
- Plataforma atravesable: si Alma llega al punto más alto del salto justo dentro de la plataforma, la detección de suelo podría darla por apoyada; usar plataformas finas (0,3) y probarlo.
- El hongo no tiene animación propia de compresión con arte; hoy se aplasta el sprite entero.
- Recursos restantes del inventario sin hacer (balancín, catapultas, corrientes de viento, interruptores, etc.).
