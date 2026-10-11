# Props de decoración del Mundo 2 (Cuevas de Cristal)

**Estado (11 de octubre de 2026):** **40 props** en `Assets/Prefabs/Level/Decor/World_2/Props/`, el objetivo por mundo. Ninguno está colocado todavía en los niveles.

Decorados que no interactúan: dan vida a los niveles 2-1 a 2-4 y al jefe 2 sin tocar la jugabilidad. Se colocan a mano sobre o alrededor del terreno del kit del Mundo 2 (`Terrain/World_2`: amatista, fósil, runas y pizarra). Son la versión del Mundo 2 de los [props del Mundo 1](Decor_World1.md): mismas categorías y mismo estilo, adaptados a las cuevas. **Sin plantas verdes:** el musgo pasa a ser liquen azul pálido; los cristales son cian (`#48CAE4`) y amatista (`#7209B7`), la roca azul pizarra y los fósiles beige, como el terreno y los fondos. Recogen las pistas de los niveles: goteras, estalactitas partidas, cristales rotos, fósiles, runas y raíces pálidas.

## Estilo e importación

Cartoon plano: contornos oscuros gruesos y colores planos con una sola sombra, como los del Mundo 1. Generados con Higgsfield (`gpt_image_2_5`, 4 hojas de 10 con fondo transparente; referencias: los props del Mundo 1, el terreno y un fondo del Mundo 2) y separados por código: cada pieza se recorta solo con sus píxeles sólidos más el borde suavizado, sin el halo de brillo de la generación; las piezas sueltas de un mismo prop (mitades de la geoda, esquirlas, piedrecitas, gotas) se agrupan.

Sprites a **94 px por unidad** (igual que el Mundo 1), sin mipmaps, malla rectangular. Punto de apoyo **abajo en el centro**, salvo los colgantes (**arriba en el centro**: se colocan en el borde del que cuelgan).

## Capas (orden de dibujo)

Las mismas que en el Mundo 1:

| Capa | Sorting Order | Uso |
| --- | --- | --- |
| Fondo cercano | −15 a −13, algo oscurecido (≈ 0,8) | Estalagmitas, columnas, arcos, fósiles grandes detrás del terreno |
| Sobre el terreno | −3 | Cristales, setas y detalles apoyados en las plataformas |
| Colgantes | −4 | Estalactitas, raíces, cristales e hilos colgando de bordes y bajos |
| Primer plano | 40 | Siluetas oscuras delante de todo |

## Props (40)

| Archivo | Pieza | Capa | Equivale en el Mundo 1 |
| --- | --- | --- | --- |
| `crystal_s`, `crystal_m`, `crystal_l` | Racimos de cristal cian (3 tamaños) | Sobre el terreno | helechos |
| `amethyst` | Racimo de amatista | Sobre el terreno | flores |
| `mush_cluster`, `mush_violet` | Setas de cueva brillantes (grupo cian, una violeta) | Sobre el terreno | hongos |
| `lichen_tuft` | Mata de liquen con cristalitos | Sobre el terreno | hierba |
| `geode_split` | Geoda partida en dos | Sobre el terreno (historia) | fruto partido |
| `pebbles` | Piedras mojadas con un cristal | Sobre el terreno | piedras |
| `shards` | Esquirlas de cristal rotas | Sobre el terreno (historia) | plumas |
| `skeleton` | Esqueleto con un cristal | Sobre el terreno (historia) | esqueleto |
| `ammonite` | Amonite fósil medio enterrado | Sobre el terreno (historia) | nido |
| `puddle` | Charco de gotera | Sobre el terreno (historia) | bayas |
| `stalactite_fallen` | Estalactita caída y rota | Sobre el terreno (historia) | tronco caído |
| `stalagmites_small` | Grupo de estalagmitas con cristales | Sobre el terreno / fondo cercano | tocón con setas |
| `crystal_corner`, `lichen_cap` | Cristales que desbordan una esquina, gorro de liquen para esquinas | Sobre el terreno (bordes) | helecho de esquina, gorro de musgo |
| `edge_strip` | Tira de cristalitos, piedras y liquen para el borde superior | Sobre el terreno (bordes) | tira de hierba |
| `crystal_vein`, `lichen_wall` | Veta de cristal y parche de liquen para caras de bloque | Sobre el terreno (paredes) | hiedra |
| `stalagmite_big`, `crystal_column` | Estalagmita alta, columna de cristales | Fondo cercano | tronco, arbolito |
| `rune_arch`, `rune_pillar` | Arco (altar de la habilidad) y pilar roto de pizarra con runas doradas (como el terreno de runas) | Fondo cercano | arco de ruinas (`arch`), tótem |
| `fossil_skull` | Cráneo fósil de dinosaurio en la roca | Fondo cercano | cabeza de estatua |
| `geode_big` | Geoda grande abierta | Fondo cercano | arbusto grande |
| `boulders` | Montón de rocas | Fondo cercano | arbusto de hojas |
| `grotto_frame` | Marco de gruta de cristal (primer vistazo del jefe) | Fondo cercano | marco de cueva (`cave_frame`) |
| `fossil_ribs` | Costillar fósil gigante | Fondo cercano | raíces grandes |
| `mush_giant` | Seta gigante brillante | Fondo cercano | — |
| `stalactite_long`, `stalactites_short` | Estalactita larga; tres cortas con gota | Colgantes (apoyo arriba) | lianas |
| `roots_pale` | Raíces pálidas colgantes | Colgantes (apoyo arriba) | cortina de musgo |
| `crystals_hanging` | Cristales colgando de un trozo de techo | Colgantes (apoyo arriba) | liana con flores |
| `glow_threads` | Hilos de gusanos de luz con cuentas cian | Colgantes (apoyo arriba) | — |
| `fg_stalagmites`, `fg_rocks`, `fg_crystals` | Siluetas: estalagmitas, rocas, cristales | Primer plano | hojas, helecho, hierba |
| `fg_stalactites` | Silueta: fila de estalactitas (apoyo arriba) | Primer plano | colgante |
| `fg_roots` | Raíces pálidas en diagonal (salió clara, no silueta oscura; aceptada así) | Primer plano | rama |

Créditos: 4 generaciones de 10 props (calidad alta, 16:9).
