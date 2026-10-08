# Fondos con parallax

**Estado (7 de octubre de 2026):** implementado el fondo con parallax de los cuatro niveles del Mundo 1 (`Parallax_Level_1_1` a `Parallax_Level_1_4`), con el componente `ParallaxLayer2D`. Cada prefab tiene **dos capas** (lejana y media); no se usa capa de primer plano. Cada uno está colocado en su escena de trabajo (`Level_1_1` a `Level_1_4`). **Mundo 2:** implementados `Parallax_Level_2_1` (cueva de cristal), `Parallax_Level_2_2` (Galería de Ecos) y `Parallax_Level_2_3` (El Filo Resonante), cada uno colocado en su escena de trabajo nueva (`Scenes/World_02/Level_2_1` a `Level_2_3`).

Fichas relacionadas: [Alma (cámara)](../Player/Alma.md#cámara-daño-y-feedback), niveles [2-1](../Levels/World_2_Caves/Level_2_1.md), [2-2](../Levels/World_2_Caves/Level_2_2.md) y [2-3](../Levels/World_2_Caves/Level_2_3.md), niveles [1-1](../Levels/World_1_Jungle/Level_1_1.md), [1-2](../Levels/World_1_Jungle/Level_1_2.md), [1-3](../Levels/World_1_Jungle/Level_1_3.md) y [1-4](../Levels/World_1_Jungle/Level_1_4.md). Inventario: [Fondo con parallax](../INVENTARIO_GAMEPLAY_PREFABS.md#7-fondo-con-parallax).

## Archivos

Todo está en `Assets/Prefabs/Level/Backgrounds/`:

| Archivo | Responsabilidad |
| --- | --- |
| `Scripts/ParallaxLayer2D.cs` | Una capa de parallax: seguimiento de la cámara, repetición horizontal, anclaje a un borde de la pantalla y revelado al ascender. Namespace `AlmaGame.Level`. |
| `World_1/Parallax_Level_1_X.prefab`, `World_2/Parallax_Level_2_X.prefab` | Fondo de cada nivel: un objeto raíz con las capas `Far` y `Mid` como hijos. |
| `World_2/Sprites/BG_Level_2_X_Far.png` / `_Mid.png` | Imágenes de cada nivel del Mundo 2. |
| `World_1/Sprites/BG_Level_1_X_Far.png` / `_Mid.png` | Imágenes de cada capa (la lejana opaca; la media con huecos transparentes). |
| `World_1/Sprites/FG_Jungle_*.png` | Kit de plantas del Mundo 1 (helecho, hojas, helecho con flores, hierba, hojas colgantes). Ya no se usa como capa de parallax: se coloca como decoración suelta en el nivel (ver [1-1](../Levels/World_1_Jungle/Level_1_1.md#6-implementación-del-recorrido)). |
| `World_1/FX/FX_Mote.png` / `FX_Mote.mat` | Mota de polen (punto suave, 64 px) y su material de partículas (`Sprites/Default`). |

## Estilo

Cartoon plano con contorno oscuro y 2–3 tonos por zona, como el resto del arte del juego (las primeras capas del 1-1, de aspecto "óleo", se descartaron). Las imágenes se generaron con IA usando como referencia de estilo los propios sprites del juego (hongo, nido, tronco, helecho, cornisa; en el 2-1, los fondos ya aprobados del 1-1 junto con los sprites de cueva: pinchos de cristal, techo aplastante, altar del Pisotón y suelo rompible; en el 2-2, los fondos del 2-1 con los sprites del balancín, el contrapeso, el interruptor y la compuerta rúnica; en el 2-3, los fondos del 2-1 y el 2-2 con los pinchos de cristal) y se procesaron para repetirse en horizontal sin junta (corte por el camino de menor diferencia, elegido para que pase por zonas limpias como el cielo). La luz va pintada en la imagen: el proyecto usa el renderizador integrado, sin luces 2D.

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
| 2-1 | Far | `BG_Level_2_1_Far` 2328 × 1152 | 57,6 | 40,4 × 20 | (0,92; 0,95) | 0 |
| 2-1 | Mid | `BG_Level_2_1_Mid` 2288 × 1152 | 57,6 | 39,7 × 20 (× 0,9) | (0,6; 0,15) | 0 |
| 2-2 | Far | `BG_Level_2_2_Far` 2568 × 1152 | 57,6 | 44,6 × 20 | (0,92; 0,95) | 0 |
| 2-2 | Mid | `BG_Level_2_2_Mid` 2208 × 1152 | 57,6 | 38,3 × 20 (× 0,9) | (0,6; 0,15) | 0 |
| 2-3 | Far | `BG_Level_2_3_Far` 2488 × 1152 | 57,6 | 43,2 × 20 | (0,92; 0,95) | 0 |
| 2-3 | Mid | `BG_Level_2_3_Mid` 2208 × 1152 | 57,6 | 38,3 × 20 (× 0,9), color 0,72 | (0,6; 0,15) | 0 |

Con anclaje `Bottom`, `Follow.y` no se usa (la altura la fija el anclaje). En 1-2 y 1-4, niveles de ascenso, el revelado muestra la parte alta del cielo al subir; 1-1 y 1-3 son horizontales y no lo usan.

| Nivel | Contenido de las capas |
| --- | --- |
| 1-1 | Lejana: amanecer dorado, volcanes violeta con humo, colinas de selva con bruma. Media: troncos colosales con musgo, lianas y niebla dorada. |
| 1-2 | Lejana: cielo tropical que se abre al ascender. Media: dosel de copas y ramas con una abertura central que deja ver el cielo. |
| 1-3 | Lejana: hondonada de árboles retorcidos, hojas oliva oscuras y luz ocre. Media: troncos y lianas a los lados con bruma verde baja y abertura central transparente. |
| 1-4 | Lejana: cielo azul de mediodía y mar de nubes. Media: copas lejanas que asoman sobre las nubes, con abertura transparente. |
| 2-1 | Lejana: interior de una caverna inmensa en azul pizarra (`#1C2541`, `#0B132B`), siluetas de roca por capas con bruma fría, estalactitas y vetas y racimos de cristal cian y amatista a lo lejos. Media: columnas colosales de roca envueltas en cristales cian y amatista que van del suelo al techo, con huecos transparentes anchos. |
| 2-2 | Lejana: la "Galería de Ecos", una sala inmensa en gris grafito (`#2B2D42`) y azul medianoche (`#1D3557`), con arcos y pilares de roca que se pierden en la oscuridad, estalactitas y unas pocas geodas esmeralda (`#52B788`) y ámbar (`#EE9B00`) que la iluminan. Es más oscura y de menos contraste que la del 2-1. Media: estalagmitas gigantes y un puente natural de roca en arco, con geodas esmeralda y ámbar, y huecos transparentes. |
| 2-3 | Lejana: "El Filo Resonante", un túnel estrecho con techo bajo de estalactitas, grandes prismas de cristal turquesa (`#00F5D4`) y cian (`#00BBF9`) que salen de las paredes en diagonal como cuchillas, murciélagos colgando en silueta y túneles que se alejan; la franja inferior es oscura y lisa. Media: estalactitas arriba y estalagmitas abajo con prismas de cristal y algún murciélago; la franja central es transparente, así que el juego ocurre "dentro" del túnel. **Oscurecida** con el color del `SpriteRenderer` (0,72), para que sus cristales no compitan con los pinchos de cristal letales. |

## Uso

1. Arrastra el `Parallax_Level_1_X` del nivel a su escena (ya están colocados en `Level_1_1` a `Level_1_4`).
2. La cámara debe tener tamaño ortográfico **8** (vista de 16 u de alto): las capas miden 20 u.
3. En un nivel nuevo, crea un prefab igual con sus dos imágenes; si es vertical, ajusta `Ascent Reveal Per Unit` y `Ascent Start Y` (altura de la cámara a partir de la que empieza a revelarse).

## Coste

Dos SpriteRenderers *Tiled* por nivel y un `LateUpdate` sencillo por capa. Sin física ni partículas.

## Pendiente

- `Level_1_1` ya tiene su recorrido completo. Las escenas `Level_1_2` a `Level_1_4` son copias de la escena de práctica (suelo y escalones provisionales); el nivel completo de cada ficha está por construir.
- `Level_1_2` a `Level_1_4` aún no tienen `CameraBounds2D` (1-1 sí) (límites y altura fija de la cámara, ver [Alma](../Player/Alma.md#cámara-daño-y-feedback)).
- Solo `Level_1_1` está en **Build Settings**; añadir 1-2 a 1-4 para que el portal pueda cargarlas.
- Fondos de 2-4, de los mundos 3 y 4 y de los jefes.
- Las escenas `Level_2_1` a `Level_2_3` son copias de la escena de práctica (suelo y escalones provisionales); los niveles completos están por construir.
- Los cristales cian del 2-1 (capa media) y del 2-3 (las dos capas; la media ya está al 0,72) son del mismo color que los pinchos de cristal letales: si al montar el nivel se confunden, bajar más el color del `SpriteRenderer` de la capa.
