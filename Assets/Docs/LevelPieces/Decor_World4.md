# Props de decoración del Mundo 4 (Cima Volcánica)

**Estado (11 de octubre de 2026):** **42 props** en `Assets/Prefabs/Level/Decor/World_4/Props/`: los 40 del objetivo por mundo, el marco de cueva y la estalactita colosal del jefe final. Ninguno está colocado todavía en los niveles.

Decorados que no interactúan: dan vida a los niveles 4-1 a 4-4 y al jefe final sin tocar la jugabilidad. Se colocan a mano sobre o alrededor del terreno del kit del Mundo 4 (`Terrain/World_4`: ceniza, basalto, obsidiana y templo). Son la versión volcánica de los [props del Mundo 1](Decor_World1.md) (mismas categorías, como los de los mundos [2](Decor_World2.md) y [3](Decor_World3.md)). **Sin vegetación** (regla del Mundo 4): solo basalto, obsidiana con grietas de magma, ceniza, lava, azufre, hierro oxidado, huesos y piedra de templo con runas doradas. Recogen las pistas de los niveles: campanas y cadenas (4-2), armaduras rotas (4-3), huesos de bestias colosales y el templo de obsidiana (4-4), fumarolas (4-1).

## Estilo e importación

Cartoon plano como el resto. Generados con Higgsfield (`gpt_image_2_5`, 4 hojas de 10, fondo transparente; referencias: los props del **Mundo 2** como estilo —para que no se colara verde—, el terreno y dos fondos del Mundo 4; en cada hoja se prohibieron plantas y verde) y separados por código (píxeles sólidos más el borde suavizado, sin el halo; las partes sueltas, como gotas de lava o chispas, van con su prop).

Sprites a **94 px por unidad**, sin mipmaps, malla rectangular. Punto de apoyo **abajo en el centro**, salvo los colgantes (**arriba en el centro**).

## Capas (orden de dibujo)

| Capa | Sorting Order | Uso |
| --- | --- | --- |
| Fondo cercano | −15 a −13, algo oscurecido (≈ 0,8) | Columnas, monolito, arco y pilar del templo, fósiles grandes, campana |
| Sobre el terreno | −3 | Obsidiana, rocas de magma, ceniza y detalles apoyados en las plataformas |
| Colgantes | −4 | Estalactitas, cadenas, lava goteando |
| Primer plano | 40 | Siluetas negro rojizas delante de todo |

## Props (42)

| Archivo | Pieza | Capa | Equivale en el Mundo 1 |
| --- | --- | --- | --- |
| `obsidian_s`, `obsidian_spikes` | Esquirlas y pinchos de obsidiana | Sobre el terreno | helechos |
| `magma_rock` | Roca con grietas de magma | Sobre el terreno | arbusto |
| `fumarole_small` | Fumarola pequeña con vapor | Sobre el terreno | hongos |
| `ash_pile` | Montón de ceniza con brasas | Sobre el terreno | hierba |
| `pebbles` | Piedras con grietas brillantes | Sobre el terreno | piedras |
| `sulfur` | Cristales de azufre | Sobre el terreno | flores |
| `lava_puddle` | Charco de lava con costra | Sobre el terreno | fruto partido |
| `skeleton` | Esqueleto carbonizado | Sobre el terreno (historia) | esqueleto |
| `helmet_spear` | Casco y lanza rotos (4-3) | Sobre el terreno (historia) | nido |
| `skull_charred` | Cráneo carbonizado con brasas en los ojos | Sobre el terreno (historia) | — |
| `chains_pile` | Eslabones de cadena oxidados | Sobre el terreno (historia) | bayas |
| `bell_broken` | Campana de basalto rota con runa (4-2) | Sobre el terreno (historia) | — |
| `feather` | Pluma roja y naranja | Sobre el terreno (historia) | plumas |
| `stump_burnt` | Tocón calcinado con grietas de magma | Sobre el terreno / fondo cercano | tocón con setas |
| `obsidian_corner`, `ash_cap` | Obsidiana que desborda una esquina, gorro de ceniza para esquinas | Sobre el terreno (bordes) | helecho de esquina, gorro de musgo |
| `ash_edge` | Tira de ceniza, piedras y brasas para el borde superior | Sobre el terreno (bordes) | tira de hierba |
| `magma_vein`, `sulfur_wall` | Veta de magma y costra de azufre para paredes | Sobre el terreno (paredes) | hiedra |
| `basalt_columns` | Columnas de basalto hexagonales | Fondo cercano | tronco |
| `monolith` | Monolito de obsidiana con runas | Fondo cercano | tótem |
| `temple_arch`, `temple_pillar` | Arco y pilar roto del templo con runas doradas | Fondo cercano | arco de ruinas |
| `skull_giant` | Cráneo gigante en la ceniza | Fondo cercano | cabeza de estatua |
| `magma_boulder` | Roca de magma grande | Fondo cercano | arbusto grande |
| `fumarole_big` | Chimenea de fumarola con columna de vapor | Fondo cercano | arbolito |
| `bell_shrine` | Campana colgada de un pórtico de piedra (4-2) | Fondo cercano | marco de cueva |
| `ribcage` | Costillar de bestia colosal (4-4) | Fondo cercano | raíces grandes |
| `brazier` | Brasero de templo con fuego | Fondo cercano | arbusto de hojas |
| `stalactite_lava`, `stalactites_obsidian` | Estalactita con gota de lava; tres de obsidiana | Colgantes (apoyo arriba) | lianas |
| `chains_hanging` | Dos cadenas oxidadas | Colgantes (apoyo arriba) | cortina de musgo |
| `lava_drip` | Lava goteando de un trozo de techo | Colgantes (apoyo arriba) | liana con flores |
| `bell_hanging` | Cadena con una campanita con runa | Colgantes (apoyo arriba) | — |
| `stalactite_colossal` | Estalactita colosal de obsidiana, sola, con la base rota y una grieta de magma (12/10/2026; pivote en el centro) | Jefe final: la que cae sobre el Rey Ladrón, colgando del racimo `stalactites_obsidian` | — |
| `fg_columns`, `fg_rocks`, `fg_spikes` | Siluetas: columnas de basalto, rocas, pinchos de obsidiana | Primer plano | hierba, hojas, helecho |
| `fg_stalactites` | Silueta: fila de estalactitas (apoyo arriba) | Primer plano | colgante |
| `fg_pillar` | Silueta: pilar caído en diagonal | Primer plano | rama |

### Marcos de escena

Las dos piezas que en el Mundo 1 enmarcan momentos clave: el **altar** de la habilidad va dentro de `temple_arch` (equivale a `arch`) y el **primer vistazo del jefe** se ve a través de `basalt_cave_frame` (añadido después, equivale a `cave_frame`). El hueco del arco es transparente.

| Archivo | Pieza | Uso | Equivale en el Mundo 1 / 2 |
| --- | --- | --- | --- |
| `temple_arch` | Arco del templo con runas doradas (de la hoja C) | Altar de la habilidad | `arch` / `rune_arch` |
| `basalt_cave_frame` | Marco ancho de columnas de basalto con grietas de magma, costra de ceniza y estalactitas de obsidiana que gotean lava; sin vegetación (8,95 × 4,82 u, a la altura del `cave_frame`) | Primer vistazo del jefe | `cave_frame` / `grotto_frame` |

Créditos: 4 generaciones de 10 props + 1 del marco de cueva (calidad alta, 16:9).
