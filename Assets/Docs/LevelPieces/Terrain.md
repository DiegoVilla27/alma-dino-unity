# Terreno

**Estado (8 de octubre de 2026):** implementados los kits de terreno de **los cuatro mundos** (32 prefabs; secciones de los mundos 2 a 4 más abajo). Mundo 1 (7/10/2026): **4 diseños × 2 piezas = 8 prefabs** redimensionables (bloque de suelo y plataforma flotante en piedra, madera, raíces y zarzas oscuras), con el componente `SolidPlatform2D`. Es la "plataforma sólida" del inventario, hecha por mundo. Aún no está colocado en ninguna escena (todas son de práctica); se probó en un recorrido del 1-1 que luego se retiró.

Fichas relacionadas: [Piezas de nivel](Pieces.md) (margen de arte y herramienta Rect), [Fondos con parallax](Backgrounds.md), niveles [1-1](../Levels/World_1_Jungle/Level_1_1.md) a [1-4](../Levels/World_1_Jungle/Level_1_4.md). Inventario: [Plataforma sólida](../INVENTARIO_GAMEPLAY_PREFABS.md#1-plataforma-sólida).

## Archivos

Todo está en `Assets/Prefabs/Level/Terrain/`:

| Archivo | Responsabilidad |
| --- | --- |
| `Scripts/SolidPlatform2D.cs` | Terreno sólido: ajusta el `BoxCollider2D` a `Size` y el sprite a `Size` + margen de `PieceArt2D` (vía `LevelPieceUtility.ApplySize`). Namespace `AlmaGame.Level`. |
| `World_1/Platform_Ground_<Diseño>_Jungle.prefab` | Bloque de suelo (suelo, escalones, paredes, bloques). |
| `World_1/Platform_Floating_<Diseño>_Jungle.prefab` | Plataforma flotante. |
| `World_1/Sprites/Platform_*_Jungle.png` | Un sprite por prefab, con su mismo nombre. |

Cada prefab: `SpriteRenderer` (*Tiled*, `Tile Mode` *Adaptive*, `Order in Layer` −5, detrás de nidos, altares, trampas y Alma), `BoxCollider2D` sólido, `PieceArt2D` y `SolidPlatform2D`.

## Diseños

| Diseño | Bloque de suelo | Plataforma flotante | Uso previsto |
| --- | --- | --- | --- |
| **Stone** (piedra y musgo) | Piedras violeta oscuro con musgo lima que gotea; base de piedras y helechos | Piedras que se estrechan con raíces colgando | 1-1, sotobosque |
| **Wood** (madera) | Corteza con vetas y nudos, setas naranjas de repisa, extremos con anillos de tronco cortado | Rama/tronco grueso con setas | 1-2 y 1-4, el Gran Árbol |
| **Roots** (raíces) | Tierra oscura con raíces gruesas entrelazadas | Terrón de raíces con raíces colgantes | Variedad en cualquier nivel |
| **Thorns** (zarzas oscuras) | Basalto azul violáceo, musgo marchito oliva, zarzas moradas con espinas carmesí por los lados | Igual, con zarzas colgando | 1-3, la hondonada |

Los cuatro tienen la misma forma, altura de musgo y tamaño (se generaron usando el bloque y la flotante de piedra como plantilla), así que se pueden mezclar en un mismo nivel. Las espinas de **Thorns** son decoración: no hacen daño (para un tramo letal, combinar con `Trap_Briers_Swamp`).

## Arte

Generado con IA el 7/10/2026 (8 generaciones: suelos a 2k, flotantes a 1k), usando como referencia de estilo los sprites del juego y el concepto de "Plataforma sólida", y procesado a **256 PPU**, malla *Full Rect*, color blanco. Las uniones de las partes repetidas son sin costura (corte por el camino de menor diferencia).

### Bloque de suelo — 9-slice, *Tiled* Adaptive en ambos ejes

- **Fijo arriba:** las matas y la losa de musgo con sus goteos.
- **Fijo abajo:** la base de piedras/raíces y plantas.
- **Fijos a los lados:** los extremos (8,5 % del ancho).
- **Centro:** el material (piedra, corteza, raíces), repetido en horizontal y en vertical.
- La imagen lleva filas transparentes bajo la base para que la caja de colisión quede centrada; las matas sobre el musgo son margen de arte.

| Prefab | `Size` (colisión) | Margen (`PieceArt2D`) | Sprite (px) | Borders (izq., abajo, der., arriba) |
| --- | --- | --- | --- | --- |
| `Platform_Ground_Stone_Jungle` | 8 × 2,621 | (0; 0,379) | 2048 × 865 | 174, 278, 174, 285 |
| `Platform_Ground_Wood_Jungle` | 8 × 2,719 | (0; 0,379) | 2048 × 890 | 174, 285, 174, 292 |
| `Platform_Ground_Roots_Jungle` | 8 × 2,676 | (0; 0,395) | 2048 × 887 | 174, 286, 174, 293 |
| `Platform_Ground_Thorns_Jungle` | 8 × 2,656 | (0; 0,402) | 2048 × 886 | 174, 287, 174, 293 |

### Plataforma flotante — 3-slice, *Tiled* Adaptive en horizontal

- Extremos redondeados fijos (10 % del ancho) y centro repetido.
- La colisión cubre de la superficie del musgo hasta el final de la roca maciza; las matas de arriba y la roca que se estrecha y las raíces de abajo son margen de arte.
- Pensada para alargarse en horizontal; **no cambiar su alto** (el centro se repetiría también en vertical).

| Prefab | `Size` (colisión) | Margen (`PieceArt2D`) | Sprite (px) |
| --- | --- | --- | --- |
| `Platform_Floating_Stone_Jungle` | 4 × 0,434 | (0; 0,281) | 1024 × 255 |
| `Platform_Floating_Wood_Jungle` | 4 × 0,43 | (0; 0,293) | 1024 × 260 |
| `Platform_Floating_Roots_Jungle` | 4 × 0,434 | (0; 0,387) | 1024 × 309 |
| `Platform_Floating_Thorns_Jungle` | 4 × 0,445 | (0; 0,273) | 1024 × 254 |

## Mundo 2 — Cuevas de Cristal

**Estado (8 de octubre de 2026):** implementado el kit del Mundo 2: **4 diseños × 2 piezas = 8 prefabs** en `Terrain/World_2/` (`Platform_Ground_<Diseño>_Caves`, `Platform_Floating_<Diseño>_Caves`), con los sprites en `World_2/Sprites/`. Mismo componente (`SolidPlatform2D`), mismas piezas y mismo procesado que el Mundo 1; `Order in Layer` −5. Aún no está colocado en ninguna escena (todas son de práctica).

Arte generado con IA el 8/10/2026 (8 generaciones, ~6 créditos) usando como referencia de **forma** el bloque y la flotante de piedra del Mundo 1 (por eso todas las piezas tienen el mismo perfil y se pueden mezclar) y como referencia de **estilo** los fondos del 2-1 y 2-2 y los sprites de cueva. Reglas: sin vegetación, **sin cristales cian** (no se confunden con los pinchos de cristal letales) y **siempre oscuros**, para contrastar con el suelo rompible (caliza clara agrietada). Lo que asoma por encima de la cara superior es margen de arte: Alma pisa la cara superior.

| Diseño | Bloque de suelo | Plataforma flotante | Uso previsto |
| --- | --- | --- | --- |
| **Slate** | Pizarra azul oscuro en losas con vetas de cuarzo; pequeñas estalagmitas encima (margen de arte) | Losas de pizarra con estalactitas debajo | 2-1, la caverna fría |
| **Rune** | Bloques de basalto grafito tallados con runas en oro viejo; cara superior enlosada | Losa de templo con runas | 2-2, el templo de los balancines |
| **Amethyst** | Roca ciruela oscura con geodas de amatista y magenta incrustadas en la cara (nada asoma por encima) | Roca de geoda con puntas de amatista debajo | 2-4, la gran geoda |
| **Fossil** | Roca en estratos con amonites, esqueletos de pez y huesos | Roca en estratos con fósiles | 2-3 y variedad |

| Prefab | `Size` (colisión) | Margen (`PieceArt2D`) | Sprite (px) | Borders (izq., abajo, der., arriba) |
| --- | --- | --- | --- | --- |
| `Platform_Ground_Slate_Caves` | 8,0 × 2,621 | (0; 0,402) | 2048 × 877 | 174, 284, 174, 291 |
| `Platform_Ground_Rune_Caves` | 8,0 × 2,547 | (0; 0,008) | 2048 × 656 | 174, 178, 174, 185 |
| `Platform_Ground_Amethyst_Caves` | 8,0 × 2,688 | (0; 0,027) | 2048 × 702 | 174, 193, 174, 200 |
| `Platform_Ground_Fossil_Caves` | 8,0 × 2,645 | (0; 0,195) | 2048 × 777 | 174, 233, 174, 240 |

| Prefab | `Size` (colisión) | Margen (`PieceArt2D`) | Sprite (px) |
| --- | --- | --- | --- |
| `Platform_Floating_Slate_Caves` | 4,0 × 0,43 | (0; 0,266) | 1024 × 246 |
| `Platform_Floating_Rune_Caves` | 4,0 × 0,453 | (0; 0,277) | 1024 × 258 |
| `Platform_Floating_Amethyst_Caves` | 4,0 × 0,48 | (0; 0,262) | 1024 × 257 |
| `Platform_Floating_Fossil_Caves` | 4,0 × 0,449 | (0; 0,266) | 1024 × 251 |

Comprobado en Unity redimensionando cada pieza (suelo 11 × 6, flotante de 9 de ancho): se repiten sin juntas. En el fósil se nota algo el patrón al repetirse en vertical; en bloques bajos casi no se ve.

## Mundo 3 — Pantano de Viento y Niebla

**Estado (8 de octubre de 2026):** implementado el kit del Mundo 3: **4 diseños × 2 piezas = 8 prefabs** en `Terrain/World_3/` (`Platform_Ground_<Diseño>_Swamp`, `Platform_Floating_<Diseño>_Swamp`), con los sprites en `World_3/Sprites/`. Mismo componente, piezas y procesado que los mundos 1 y 2; `Order in Layer` −5. Aún no está colocado en ninguna escena (todas son de práctica).

Arte generado con IA el 8/10/2026 (8 generaciones, ~6 créditos) con la forma del bloque y la flotante del Mundo 1 y el estilo de los fondos del 3-1 y 3-2 y los sprites del pantano. Reglas: el musgo es **gris verdoso apagado** (nunca lima ni amarillo), para no confundirse con el lago tóxico (lima) ni el lodo tóxico (oliva amarillento) ni parecer la jungla del Mundo 1. Juncos, matas y musgo colgante por encima o por debajo son margen de arte.

| Diseño | Bloque de suelo | Plataforma flotante | Uso previsto |
| --- | --- | --- | --- |
| **Peat** | Turba negra parda con raíces, musgo de pantano gris verdoso y matas de juncos (margen de arte) | Islote de turba con raíces colgando | 3-1, los fangales |
| **Mangrove** | Raíces de manglar entrelazadas en madera gris verdosa pálida, con musgo colgante | Masa de raíces con musgo colgando | 3-1 y 3-4, raíces del Sauce |
| **Boardwalk** | Pasarela de tablones podridos sobre troncos atados con cuerda, con setitas | Tramo de pasarela sobre dos troncos | 3-2 y 3-3, zonas construidas |
| **Mire** | Piedras gris verdosas en barro, con musgo y helechos en la base | Piedras con musgo colgante | 3-2, el cañón, y variedad |

| Prefab | `Size` (colisión) | Margen (`PieceArt2D`) | Sprite (px) | Borders (izq., abajo, der., arriba) |
| --- | --- | --- | --- | --- |
| `Platform_Ground_Peat_Swamp` | 8,0 × 2,645 | (0; 0,609) | 2048 × 989 | 174, 339, 174, 346 |
| `Platform_Ground_Mangrove_Swamp` | 8,0 × 2,676 | (0; 0,102) | 2048 × 737 | 174, 211, 174, 218 |
| `Platform_Ground_Boardwalk_Swamp` | 8,0 × 2,688 | (0; 0,109) | 2048 × 744 | 174, 214, 174, 221 |
| `Platform_Ground_Mire_Swamp` | 8,0 × 2,645 | (0; 0,285) | 2048 × 823 | 174, 256, 174, 263 |

| Prefab | `Size` (colisión) | Margen (`PieceArt2D`) | Sprite (px) |
| --- | --- | --- | --- |
| `Platform_Floating_Peat_Swamp` | 4,0 × 0,457 | (0; 0,324) | 1024 × 283 |
| `Platform_Floating_Mangrove_Swamp` | 4,0 × 0,367 | (0; 0,352) | 1024 × 274 |
| `Platform_Floating_Boardwalk_Swamp` | 4,0 × 0,438 | (0; 0,293) | 1024 × 262 |
| `Platform_Floating_Mire_Swamp` | 4,0 × 0,469 | (0; 0,316) | 1024 × 282 |

Comprobado en Unity redimensionando cada pieza (suelo 11 × 6, flotante de 9 de ancho): se repiten sin juntas, incluidos los postes de la pasarela.

## Mundo 4 — Cima Volcánica

**Estado (8 de octubre de 2026):** implementado el kit del Mundo 4: **4 diseños × 2 piezas = 8 prefabs** en `Terrain/World_4/` (`Platform_Ground_<Diseño>_Volcano`, `Platform_Floating_<Diseño>_Volcano`), con los sprites en `World_4/Sprites/`. Mismo componente, piezas y procesado que los demás mundos; `Order in Layer` −5. Aún no está colocado en ninguna escena (todas son de práctica).

Arte generado con IA el 8/10/2026 (8 generaciones, ~6 créditos) con la forma del bloque y la flotante del Mundo 1 y el estilo de los fondos del 4-1 y 4-2 y los sprites del volcán. Reglas: **sin vegetación** (comprobado: 0 píxeles verdes) y **sin lava ni llamas**; las vetas incandescentes son mínimas y apagadas, para que la lava, los pinchos ardientes y los géiseres sigan siendo lo único que brilla y se lea como peligro. Son más oscuras y sólidas que la cornisa que se desmorona.

| Diseño | Bloque de suelo | Plataforma flotante | Uso previsto |
| --- | --- | --- | --- |
| **Basalt** | Columnas hexagonales de basalto negro carbón con vetas naranjas muy tenues | Losa de columnas de basalto | 4-1 y 4-3, terreno base |
| **Ash** | Roca volcánica porosa gris con una capa de ceniza pálida y piedras calcinadas (margen de arte) | Roca porosa con ceniza | 4-1, los ríos de ceniza |
| **Obsidian** | Cristal volcánico negro facetado con reflejos y finas vetas rojo oscuro | Obsidiana con puntas de cristal debajo | 4-4 y arena del jefe final |
| **Temple** | Sillares de basalto con grabados en oro antiguo, como los pilares del fondo del 4-2 | Losa de templo grabada | 4-2, el santuario de las campanas |

| Prefab | `Size` (colisión) | Margen (`PieceArt2D`) | Sprite (px) | Borders (izq., abajo, der., arriba) |
| --- | --- | --- | --- | --- |
| `Platform_Ground_Basalt_Volcano` | 8,0 × 2,668 | (0; 0,07) | 2048 × 719 | 174, 202, 174, 209 |
| `Platform_Ground_Ash_Volcano` | 8,0 × 2,648 | (0; 0,359) | 2048 × 862 | 174, 275, 174, 282 |
| `Platform_Ground_Obsidian_Volcano` | 8,0 × 2,672 | (0; 0,031) | 2048 × 700 | 174, 193, 174, 200 |
| `Platform_Ground_Temple_Volcano` | 8,0 × 2,641 | (0; 0,039) | 2048 × 696 | 174, 193, 174, 199 |

| Prefab | `Size` (colisión) | Margen (`PieceArt2D`) | Sprite (px) |
| --- | --- | --- | --- |
| `Platform_Floating_Basalt_Volcano` | 4,0 × 0,453 | (0; 0,242) | 1024 × 240 |
| `Platform_Floating_Ash_Volcano` | 4,0 × 0,465 | (0; 0,262) | 1024 × 253 |
| `Platform_Floating_Obsidian_Volcano` | 4,0 × 0,441 | (0; 0,246) | 1024 × 239 |
| `Platform_Floating_Temple_Volcano` | 4,0 × 0,398 | (0; 0,25) | 1024 × 230 |

Comprobado en Unity redimensionando cada pieza (suelo 11 × 6, flotante de 9 de ancho): se repiten sin juntas.

## Uso

1. Arrastra el prefab a la escena.
2. Ajusta el tamaño con `Size` en **Solid Platform 2D** o arrastrando los tiradores del sprite con la herramienta **Rect (T)**; nunca con la escala del Transform. `PieceArt2D` sincroniza ambos y la colisión acompaña.
3. **Suelo:** un bloque ancho y alto que se meta por debajo del borde inferior de los `CameraBounds2D` del nivel (así nunca se ve su base). **Escalones y paredes:** bloques más estrechos o altos; su base se ve completa.
4. Alma se apoya en la superficie del musgo (borde superior de `Size`).

## Coste

Por pieza: un `SpriteRenderer` y un `BoxCollider2D` estático. `PieceArt2D` no hace nada en juego.

## Pendiente

- Colocar el terreno en los niveles de cada mundo en lugar del suelo y los escalones provisionales.
- Tinte por nivel opcional (más oscuro en 1-3, más dorado en 1-4) con el color del `SpriteRenderer`.
