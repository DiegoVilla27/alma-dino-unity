# Terreno — Mundo 1

**Estado (7 de octubre de 2026):** implementado el kit de terreno del Mundo 1: **4 diseños × 2 piezas = 8 prefabs** redimensionables (bloque de suelo y plataforma flotante en piedra, madera, raíces y zarzas oscuras), con el componente `SolidPlatform2D`. Es la "plataforma sólida" del inventario, hecha por mundo. Aún no está colocado en ninguna escena (todas son de práctica); se probó en un recorrido del 1-1 que luego se retiró.

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

## Uso

1. Arrastra el prefab a la escena.
2. Ajusta el tamaño con `Size` en **Solid Platform 2D** o arrastrando los tiradores del sprite con la herramienta **Rect (T)**; nunca con la escala del Transform. `PieceArt2D` sincroniza ambos y la colisión acompaña.
3. **Suelo:** un bloque ancho y alto que se meta por debajo del borde inferior de los `CameraBounds2D` del nivel (así nunca se ve su base). **Escalones y paredes:** bloques más estrechos o altos; su base se ve completa.
4. Alma se apoya en la superficie del musgo (borde superior de `Size`).

## Coste

Por pieza: un `SpriteRenderer` y un `BoxCollider2D` estático. `PieceArt2D` no hace nada en juego.

## Pendiente

- Colocar el terreno en los niveles del Mundo 1 en lugar del suelo y los escalones provisionales.
- Tinte por nivel opcional (más oscuro en 1-3, más dorado en 1-4) con el color del `SpriteRenderer`.
- Kits de terreno de los mundos 2 a 4.
