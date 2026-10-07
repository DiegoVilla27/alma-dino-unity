# Fondos con parallax

**Estado (7 de octubre de 2026):** implementado el fondo con parallax de los cuatro niveles del Mundo 1 (`Parallax_Level_1_1` a `Parallax_Level_1_4`), con el componente `ParallaxLayer2D`. Cada prefab tiene **dos capas** (lejana y media); no se usa capa de primer plano. Cada uno está colocado en su escena de trabajo (`Level_1_1` a `Level_1_4`).

Fichas relacionadas: [Alma (cámara)](../Player/Alma.md#cámara-daño-y-feedback), niveles [1-1](../Levels/World_1_Jungle/Level_1_1.md), [1-2](../Levels/World_1_Jungle/Level_1_2.md), [1-3](../Levels/World_1_Jungle/Level_1_3.md) y [1-4](../Levels/World_1_Jungle/Level_1_4.md). Inventario: [Fondo con parallax](../INVENTARIO_GAMEPLAY_PREFABS.md#7-fondo-con-parallax).

## Archivos

Todo está en `Assets/Prefabs/Level/Backgrounds/`:

| Archivo | Responsabilidad |
| --- | --- |
| `Scripts/ParallaxLayer2D.cs` | Una capa de parallax: seguimiento de la cámara, repetición horizontal, anclaje a un borde de la pantalla y revelado al ascender. Namespace `AlmaGame.Level`. |
| `World_1/Parallax_Level_1_X.prefab` | Fondo de cada nivel: un objeto raíz con las capas `Far` y `Mid` como hijos. |
| `World_1/Sprites/BG_Level_1_X_Far.png` / `_Mid.png` | Imágenes de cada capa (la lejana opaca; la media con huecos transparentes). |
| `World_1/Sprites/FG_Jungle_*.png` | Kit de primer plano del Mundo 1 (helecho, hojas, helecho con flores, hierba, hojas colgantes). **Sin uso:** el primer plano se retiró de los fondos. |

## Estilo

Cartoon plano con contorno oscuro y 2–3 tonos por zona, como el resto del arte del juego (las primeras capas del 1-1, de aspecto "óleo", se descartaron). Las imágenes se generaron con IA usando como referencia de estilo los propios sprites del juego (hongo, nido, tronco, helecho, cornisa) y se procesaron para repetirse en horizontal sin junta (corte por el camino de menor diferencia, elegido para que pase por zonas limpias como el cielo). La luz va pintada en la imagen: el proyecto usa el renderizador integrado, sin luces 2D.

## Cómo funciona una capa (`ParallaxLayer2D`)

- **Seguimiento (`Follow`):** cuánto acompaña la capa a la cámara en cada eje. 1 = se mueve con ella (no se desplaza en pantalla, como un cielo lejano); 0 = fija en el mundo como el nivel; negativo = pasa más rápido que el nivel (primer plano).
- **Repetición horizontal (`Wrap`):** una tira en *Tiled* se recoloca bajo la cámara por tiles enteros, así que no se acaba por largo que sea el nivel. Al empezar, el script amplía el `Tiled` a los tiles necesarios para cubrir el ancho real de la pantalla más uno a cada lado, sea cual sea la proporción del dispositivo. Un sprite suelto puede repetirse cada `Wrap Period` unidades.
- **Anclaje (`Anchor`):** con `Bottom`, el borde inferior de la capa queda siempre en el borde inferior de la pantalla (`Top`, igual arriba; `Anchor Offset` la desplaza). Las dos capas de fondo están ancladas abajo: como miden 20 u y la vista 16 u, su borde superior queda siempre fuera de la pantalla, suba lo que suba la cámara. Si una capa anclada fuera más baja que la vista, el script la amplía sola al empezar.
- **Revelado al ascender (`Ascent Reveal Per Unit`, `Ascent Start Y`):** en niveles verticales, con la capa anclada abajo, por cada unidad que la cámara sube por encima de `Ascent Start Y` la imagen baja esa fracción, mostrando su parte superior (por ejemplo, el cielo que se abre al subir el árbol en 1-2). Nunca baja más que el sobrante de la imagen sobre la vista (20 − 16 = 4 u, menos el `Anchor Offset`), así que su borde superior no llega a verse.
- **Cubrir la vista (`Cover View`):** solo actúa sin anclaje: mantiene la capa cubriendo la altura de la vista mientras sigue a la cámara por `Follow.y`.

Comprobado en Unity (1-1) con el fondo de la cámara en magenta a cuatro alturas de cámara, hasta y = 30: 0 píxeles sin imagen.

### Campos

| Campo | Uso |
| --- | --- |
| `Follow` | Seguimiento de la cámara en X e Y (ver arriba). |
| `Wrap` | Repetir la tira *Tiled* en horizontal. |
| `Wrap Period` | Para un sprite suelto: se repite cada tantas unidades (0 = no). |
| `Anchor` / `Anchor Offset` | `None`, `Bottom` o `Top`, y distancia a ese borde (negativo = parte fuera de pantalla). |
| `Ascent Reveal Per Unit` / `Ascent Start Y` | Revelado de la parte alta de la imagen al subir la cámara (solo con `Bottom`). |
| `Cover View` | Cubrir la altura de la vista cuando no hay anclaje. |
| `Camera` | Cámara a seguir (vacío = `Camera.main`). |

## Valores por nivel

Las dos capas usan *Tiled*, `Wrap` activo, anclaje `Bottom` con offset 0 y escala 0,9 en la media. Orden de dibujo: lejana −100, media −90 (detrás de todo lo jugable).

| Nivel | Capa | Imagen | PPU | Tamaño (u) | `Follow` | `Ascent Reveal Per Unit` |
| --- | --- | --- | ---: | --- | --- | ---: |
| 1-1 | Far | `BG_Level_1_1_Far` 1748 × 1152 | 57,6 | 30,3 × 20 | (0,92; 0,95) | — |
| 1-1 | Mid | `BG_Level_1_1_Mid` 2508 × 1152 | 57,6 | 43,5 × 20 (× 0,9) | (0,6; 0,15) | — |
| 1-2 | Far | `BG_Level_1_2_Far` 4096 × 1360 | 68 | 60,2 × 20 | (0,92; 0,95) | 0,13 |
| 1-2 | Mid | `BG_Level_1_2_Mid` 4096 × 1360 | 68 | 60,2 × 20 (× 0,9) | (0,6; 0,15) | 0,07 |
| 1-3 | Far | `BG_Level_1_3_Far` 4096 × 1360 | 68 | 60,2 × 20 | (0,92; 0,95) | 0 |
| 1-3 | Mid | `BG_Level_1_3_Mid` 4096 × 1360 | 68 | 60,2 × 20 (× 0,9) | (0,6; 0,15) | 0 |
| 1-4 | Far | `BG_Level_1_4_Far` 4096 × 1360 | 68 | 60,2 × 20 | (0,92; 0,95) | 0,13 |
| 1-4 | Mid | `BG_Level_1_4_Mid` 4096 × 1360 | 68 | 60,2 × 20 (× 0,9) | (0,6; 0,15) | 0,07 |

Con anclaje `Bottom`, `Follow.y` no se usa (la altura la fija el anclaje). En 1-2 y 1-4, niveles de ascenso, el revelado muestra la parte alta del cielo al subir; 1-1 y 1-3 son horizontales y no lo usan.

| Nivel | Contenido de las capas |
| --- | --- |
| 1-1 | Lejana: amanecer dorado, volcanes violeta con humo, colinas de selva con bruma. Media: troncos colosales con musgo, lianas y niebla dorada. |
| 1-2 | Lejana: cielo tropical que se abre al ascender. Media: dosel de copas y ramas con una abertura central que deja ver el cielo. |
| 1-3 | Lejana: hondonada de árboles retorcidos, hojas oliva oscuras y luz ocre. Media: troncos y lianas a los lados con bruma verde baja y abertura central transparente. |
| 1-4 | Lejana: cielo azul de mediodía y mar de nubes. Media: copas lejanas que asoman sobre las nubes, con abertura transparente. |

## Uso

1. Arrastra el `Parallax_Level_1_X` del nivel a su escena (ya están colocados en `Level_1_1` a `Level_1_4`).
2. La cámara debe tener tamaño ortográfico **8** (vista de 16 u de alto): las capas miden 20 u.
3. En un nivel nuevo, crea un prefab igual con sus dos imágenes; si es vertical, ajusta `Ascent Reveal Per Unit` y `Ascent Start Y` (altura de la cámara a partir de la que empieza a revelarse).

## Coste

Dos SpriteRenderers *Tiled* por nivel y un `LateUpdate` sencillo por capa. Sin física ni partículas.

## Pendiente

- Kit de primer plano `FG_Jungle_*` sin uso: borrarlo o reintroducir un primer plano.
- Las escenas `Level_1_2` a `Level_1_4` son copias de la escena de práctica (suelo y escalones provisionales); el nivel completo de cada ficha está por construir.
- Ninguna escena tiene aún `CameraBounds2D` (límites y altura fija de la cámara, ver [Alma](../Player/Alma.md#cámara-daño-y-feedback)).
- Solo `Level_1_1` está en **Build Settings**; añadir 1-2 a 1-4 para que el portal pueda cargarlas.
- Fondos de los mundos 2 a 4 y de los jefes.
