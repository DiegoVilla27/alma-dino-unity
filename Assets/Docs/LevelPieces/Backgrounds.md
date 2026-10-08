# Fondos con parallax

**Estado (7 de octubre de 2026):** implementado el fondo con parallax de los cuatro niveles del Mundo 1 (`Parallax_Level_1_1` a `Parallax_Level_1_4`), con el componente `ParallaxLayer2D`. Cada prefab tiene **dos capas** (lejana y media); no se usa capa de primer plano. Cada uno está colocado en su escena de trabajo (`Level_1_1` a `Level_1_4`). **Mundo 2:** implementados los cuatro: `Parallax_Level_2_1` (cueva de cristal), `Parallax_Level_2_2` (Galería de Ecos), `Parallax_Level_2_3` (El Filo Resonante) y `Parallax_Level_2_4` (Gran Geoda Sagrada), cada uno colocado en su escena de trabajo nueva (`Scenes/World_02/Level_2_1` a `Level_2_4`). **Mundo 3:** implementados los cuatro: `Parallax_Level_3_1` (Los Fangales Tóxicos), `Parallax_Level_3_2` (El Cañón de las Ráfagas), `Parallax_Level_3_3` (El Vuelo de las Esporas) y `Parallax_Level_3_4` (El Sauce Ancestral, de ascenso), cada uno colocado en su escena de trabajo nueva (`Scenes/World_03/Level_3_1` a `Level_3_4`). **Mundo 4:** implementado `Parallax_Level_4_1` (Los Ríos de Ceniza), colocado en la escena de trabajo nueva `Scenes/World_04/Level_4_1`.

Fichas relacionadas: [Alma (cámara)](../Player/Alma.md#cámara-daño-y-feedback), niveles [2-1](../Levels/World_2_Caves/Level_2_1.md), [2-2](../Levels/World_2_Caves/Level_2_2.md), [2-3](../Levels/World_2_Caves/Level_2_3.md) y [2-4](../Levels/World_2_Caves/Level_2_4.md), niveles [3-1](../Levels/World_3_Swamp/Level_3_1.md), [3-2](../Levels/World_3_Swamp/Level_3_2.md), [3-3](../Levels/World_3_Swamp/Level_3_3.md) y [3-4](../Levels/World_3_Swamp/Level_3_4.md), nivel [4-1](../Levels/World_4_Volcano/Level_4_1.md), niveles [1-1](../Levels/World_1_Jungle/Level_1_1.md), [1-2](../Levels/World_1_Jungle/Level_1_2.md), [1-3](../Levels/World_1_Jungle/Level_1_3.md) y [1-4](../Levels/World_1_Jungle/Level_1_4.md). Inventario: [Fondo con parallax](../INVENTARIO_GAMEPLAY_PREFABS.md#7-fondo-con-parallax).

## Archivos

Todo está en `Assets/Prefabs/Level/Backgrounds/`:

| Archivo | Responsabilidad |
| --- | --- |
| `Scripts/ParallaxLayer2D.cs` | Una capa de parallax: seguimiento de la cámara, repetición horizontal, anclaje a un borde de la pantalla y revelado al ascender. Namespace `AlmaGame.Level`. |
| `World_1/Parallax_Level_1_X.prefab`, `World_2/Parallax_Level_2_X.prefab`, `World_3/Parallax_Level_3_X.prefab`, `World_4/Parallax_Level_4_1.prefab` | Fondo de cada nivel: un objeto raíz con las capas `Far` y `Mid` como hijos. |
| `World_2/Sprites/BG_Level_2_X_Far.png` / `_Mid.png`, `World_3/Sprites/BG_Level_3_X_*.png`, `World_4/Sprites/BG_Level_4_1_*.png` | Imágenes de cada nivel de los mundos 2 a 4. |
| `World_1/Sprites/BG_Level_1_X_Far.png` / `_Mid.png` | Imágenes de cada capa (la lejana opaca; la media con huecos transparentes). |
| `World_1/Sprites/FG_Jungle_*.png` | Kit de plantas del Mundo 1 (helecho, hojas, helecho con flores, hierba, hojas colgantes). Ya no se usa como capa de parallax: se coloca como decoración suelta en el nivel (ver [1-1](../Levels/World_1_Jungle/Level_1_1.md#6-implementación-del-recorrido)). |
| `World_1/FX/FX_Mote.png` / `FX_Mote.mat` | Mota de polen (punto suave, 64 px) y su material de partículas (`Sprites/Default`). |

## Estilo

Cartoon plano con contorno oscuro y 2–3 tonos por zona, como el resto del arte del juego (las primeras capas del 1-1, de aspecto "óleo", se descartaron). Las imágenes se generaron con IA usando como referencia de estilo los propios sprites del juego (hongo, nido, tronco, helecho, cornisa; en el 2-1, los fondos ya aprobados del 1-1 junto con los sprites de cueva: pinchos de cristal, techo aplastante, altar del Pisotón y suelo rompible; en el 2-2, los fondos del 2-1 con los sprites del balancín, el contrapeso, el interruptor y la compuerta rúnica; en el 2-3, los fondos del 2-1 y el 2-2 con los pinchos de cristal; en el 2-4, los tres fondos anteriores del Mundo 2 con el techo aplastante y el Huevo Azul; en el 3-1, los fondos del 1-1 y el 2-2 con los sprites del pantano: lago tóxico, zarzas, nenúfar, barrera de juncos, espora y altar del Dash; en el 3-2, los fondos del 3-1 y el 2-2 con la barrera de juncos, las zarzas, la espora y el gas tóxico; en el 3-3, los fondos del 3-1 y el 3-2 con la espora, el nenúfar, el lago tóxico y el Huevo Morado; en el 3-4, los fondos del 3-1, el 3-3 y el 1-2 (por su dosel con abertura) con el gas tóxico, el Huevo Morado y las zarzas; en el 4-1, la capa lejana del 3-4 y el fondo del 1-1 con la lava, los pinchos ardientes, el géiser, la cornisa, la roca del Rugido y el altar del Rugido) y se procesaron para repetirse en horizontal sin junta (corte por el camino de menor diferencia, elegido para que pase por zonas limpias como el cielo). La luz va pintada en la imagen: el proyecto usa el renderizador integrado, sin luces 2D.

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
| 2-4 | Far | `BG_Level_2_4_Far` 2328 × 1152 | 57,6 | 40,4 × 20 | (0,92; 0,95) | 0 |
| 2-4 | Mid | `BG_Level_2_4_Mid` 2208 × 1152 | 57,6 | 38,3 × 20 (× 0,9), color 0,72 | (0,6; 0,15) | 0 |
| 3-1 | Far | `BG_Level_3_1_Far` 2288 × 1152 | 57,6 | 39,7 × 20 | (0,92; 0,95) | 0 |
| 3-1 | Mid | `BG_Level_3_1_Mid` 2208 × 1152 | 57,6 | 38,3 × 20 (× 0,9) | (0,6; 0,15) | 0 |
| 3-2 | Far | `BG_Level_3_2_Far` 2208 × 1152 | 57,6 | 38,3 × 20 | (0,92; 0,95) | 0 |
| 3-2 | Mid | `BG_Level_3_2_Mid` 1460 × 1152 | 57,6 | 25,3 × 20 (× 0,9) | (0,6; 0,15) | 0 |
| 3-3 | Far | `BG_Level_3_3_Far` 2568 × 1152 | 57,6 | 44,6 × 20 | (0,92; 0,95) | 0 |
| 3-3 | Mid | `BG_Level_3_3_Mid` 3177 × 1152 | 57,6 | 55,2 × 20 (× 0,9) | (0,6; 0,15) | 0 |
| 3-4 | Far | `BG_Level_3_4_Far` 2288 × 1152 | 57,6 | 39,7 × 20 | (0,92; 0,95) | 0,13 |
| 3-4 | Mid | `BG_Level_3_4_Mid` 2528 × 1152 | 57,6 | 43,9 × 20 (× 0,9) | (0,6; 0,15) | 0,07 |
| 4-1 | Far | `BG_Level_4_1_Far` 2328 × 1152 | 57,6 | 40,4 × 20 | (0,92; 0,95) | 0 |
| 4-1 | Mid | `BG_Level_4_1_Mid` 3106 × 1152 | 57,6 | 53,9 × 20 (× 0,9) | (0,6; 0,15) | 0 |

Con anclaje `Bottom`, `Follow.y` no se usa (la altura la fija el anclaje). En 1-2, 1-4 y 3-4, niveles de ascenso, el revelado muestra la parte alta del cielo al subir; el resto son horizontales y no lo usan.

| Nivel | Contenido de las capas |
| --- | --- |
| 1-1 | Lejana: amanecer dorado, volcanes violeta con humo, colinas de selva con bruma. Media: troncos colosales con musgo, lianas y niebla dorada. |
| 1-2 | Lejana: cielo tropical que se abre al ascender. Media: dosel de copas y ramas con una abertura central que deja ver el cielo. |
| 1-3 | Lejana: hondonada de árboles retorcidos, hojas oliva oscuras y luz ocre. Media: troncos y lianas a los lados con bruma verde baja y abertura central transparente. |
| 1-4 | Lejana: cielo azul de mediodía y mar de nubes. Media: copas lejanas que asoman sobre las nubes, con abertura transparente. |
| 2-1 | Lejana: interior de una caverna inmensa en azul pizarra (`#1C2541`, `#0B132B`), siluetas de roca por capas con bruma fría, estalactitas y vetas y racimos de cristal cian y amatista a lo lejos. Media: columnas colosales de roca envueltas en cristales cian y amatista que van del suelo al techo, con huecos transparentes anchos. |
| 2-2 | Lejana: la "Galería de Ecos", una sala inmensa en gris grafito (`#2B2D42`) y azul medianoche (`#1D3557`), con arcos y pilares de roca que se pierden en la oscuridad, estalactitas y unas pocas geodas esmeralda (`#52B788`) y ámbar (`#EE9B00`) que la iluminan. Es más oscura y de menos contraste que la del 2-1. Media: estalagmitas gigantes y un puente natural de roca en arco, con geodas esmeralda y ámbar, y huecos transparentes. |
| 2-3 | Lejana: "El Filo Resonante", un túnel estrecho con techo bajo de estalactitas, grandes prismas de cristal turquesa (`#00F5D4`) y cian (`#00BBF9`) que salen de las paredes en diagonal como cuchillas, murciélagos colgando en silueta y túneles que se alejan; la franja inferior es oscura y lisa. Media: estalactitas arriba y estalagmitas abajo con prismas de cristal y algún murciélago; la franja central es transparente, así que el juego ocurre "dentro" del túnel. **Oscurecida** con el color del `SpriteRenderer` (0,72), para que sus cristales no compitan con los pinchos de cristal letales. |
| 2-4 | Lejana: la Gran Geoda Sagrada, una caverna colosal con las paredes cubiertas de geodas zafiro (`#0077B6`), azul hielo (`#90E0EF`) y algo de amatista. Haces de luz azul pálida en diagonal, techo de roca oscura (`#1B1B1E`) con estalactitas y, en el centro y a lo lejos, un pedestal de cristal sobre una meseta. Media: geodas de roca volcánica abiertas, llenas de cristales zafiro y azul hielo, que suben desde abajo, y estalactitas masivas desde arriba, con la franja central libre. Oscurecida a 0,72, como en el 2-3. |
| 3-1 | Lejana: pantano gris y melancólico en un día nublado, con siluetas fantasmales de sauces llorones y manglares que se pierden por capas en la niebla (gris perla `#8D99AE`), bandas de bruma horizontales y agua estancada verde oscura (`#1E2D24`, `#2D4A3E`) con reflejos y juncos. Media: troncos con raíces zancudas, juncos doblados por el viento y bandas de niebla, con huecos transparentes. Sus troncos se parecen mucho a los del 1-1 (musgo lima, hojas de jungla); se aceptaron así. |
| 3-2 | Lejana: el Cañón de las Ráfagas, con agujas de roca de pizarra gris verdosa (`#415A77`, `#778DA9`) y musgo que se pierden por capas en la bruma, árboles retorcidos con la copa doblada hacia la izquierda en lo alto de los acantilados y ráfagas visibles (franjas blancas curvas, `#E0E1DD`) con hojas volando. Media: dos pilares de roca erosionada con repisas de musgo que gotea, matas secas y lianas agitadas hacia la izquierda, y bandas de niebla. Los pilares de los bordes de la imagen original estaban cortados, así que la capa se recortó por los huecos vacíos para quedarse con los dos pilares enteros del centro (por eso mide menos). |
| 3-3 | Lejana: el corazón del pantano, un lago infinito al anochecer con niebla en bandas púrpura y verde tóxico, siluetas de hongos colosales y sauces rotos que se pierden por capas, agua negra petróleo (`#0A0908`) con reflejos verdes y, en el horizonte del centro, el resplandor morado del tercer huevo (se repite con el fondo). Media: dos hongos gigantes casi iguales, de tallo pálido y sombrero púrpura, con musgo colgante y unas pocas cápsulas de esporas pequeñas y apagadas (para no confundirse con las esporas jugables), con niebla en la base. Los hongos estaban muy cerca de los bordes de la imagen, así que se añadió un hueco transparente (con la niebla prolongada) en la unión para que queden a la misma distancia entre sí. |
| 3-4 | Lejana (nivel de ascenso): el pantano visto desde lo alto, un mar de niebla gris verdosa con copas de sauces (`#6B705C`, `#B7B7A4`) y volutas de gas tóxico verde ácido (`#38B000`) que sube desde abajo; encima, cielo nublado, y en la franja superior las nubes se abren a los volcanes rojos del Mundo 4 con resplandor naranja, que se revelan al subir (como en 1-2 y 1-4). Media: la copa del Sauce Ancestral vista desde dentro, con ramas de madera pálida por los lados, cortinas de hojas gris verdosas desde arriba, lianas, volutas de gas y una gran abertura central transparente; en la unión de la repetición, dos ramas de los bordes se cruzan en "X". |
| 4-1 | Lejana: las faldas del volcán, con el cráter colosal echando una columna de humo negro y fuego bajo nubes de tormenta iluminadas en naranja desde abajo, crestas de basalto negro (`#1B1B1E`, `#3A3A3A`) con grietas brillantes que se pierden en la bruma, ríos de lava a lo lejos y lluvia de ceniza. Media: dos acantilados de columnas de basalto, cada uno con una cascada de lava (`#F77F00`, `#D62828`, `#FFD166`) que cae a una poza, con humo; se añadió espacio transparente en la unión para igualar la separación. Sus repisas traían musgo verde y helechos (heredados de la referencia del 1-1): se recolorearon al procesar (solo los tonos verdes) a ceniza gris y ocre quemado, como roca cubierta de ceniza y plantas calcinadas. La lava es casi tan viva como la jugable; se aceptó así. |

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
- Fondos de 4-2 a 4-4 y de los jefes.
- La escena `Level_4_1` es una copia de la escena de práctica; el nivel completo está por construir.
- Si en el 4-1 la cascada de lava de la capa media se confunde con la lava letal, se puede apagar la capa con el color de su `SpriteRenderer` (sin créditos).
- Las escenas `Level_3_1` a `Level_3_4` son copias de la escena de práctica; los niveles completos están por construir.
- El pedestal de cristal del fondo lejano del 2-4 se repite cada ~40 u y puede confundirse con el pedestal real del Huevo Azul. Si molesta al montar el nivel, borrarlo de la imagen (retoque, sin créditos).
- Las escenas `Level_2_1` a `Level_2_4` son copias de la escena de práctica (suelo y escalones provisionales); los niveles completos están por construir.
- Los cristales cian del 2-1 (capa media) y del 2-3 (las dos capas; la media ya está al 0,72) y del 2-4 (lo mismo) son del mismo color que los pinchos de cristal letales: si al montar el nivel se confunden, bajar más el color del `SpriteRenderer` de la capa.
