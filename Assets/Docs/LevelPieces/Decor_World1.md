# Props de decoración del Mundo 1 (Jungla Esmeralda)

**Estado (9 de octubre de 2026):** **40 props completos** en `Assets/Prefabs/Level/Decor/World_1/Props/`, el objetivo por mundo. Ninguno está colocado todavía en los niveles.

Los props son decorados que no interactúan: dan vida a los niveles sin tocar la jugabilidad. Se colocan a mano en la escena, sobre o alrededor del terreno de los kits (`Terrain/World_1`).

## Estilo

Cartoon plano: contornos oscuros gruesos y colores planos con una sola sombra, como el terreno, los fondos y Alma. Nada pictórico ni realista. Generados con IA en hojas de varias piezas con fondo transparente y separados por código (94 px por unidad). El punto de apoyo está **abajo en el centro**, salvo en los colgantes, que lo tienen **arriba**: se colocan en el borde del que cuelgan.

## Capas (orden de dibujo)

| Capa | Sorting Order | Uso |
| --- | --- | --- |
| Fondo cercano | −15 a −13, algo oscurecido (≈ 0,8) | Ruinas, tótems, raíces y arbustos grandes detrás del terreno |
| Sobre el terreno | −3 | Plantas pequeñas y detalles apoyados en las plataformas |
| Colgantes | −4 | Lianas, raíces y musgo colgando de bordes y bajos de plataformas |
| Primer plano | 40 | Siluetas oscuras delante de todo |

## Props (40)

| Archivo | Pieza | Capa |
| --- | --- | --- |
| `fern_s`, `fern_m`, `fern_l` | Helechos (3 tamaños) | Sobre el terreno |
| `flowers_o`, `flowers_w` | Flores naranjas / blancas | Sobre el terreno |
| `grass` | Mata de hierba | Sobre el terreno |
| `mush_cluster`, `mush_one` | Hongos brillantes | Sobre el terreno |
| `feathers`, `pebbles`, `skeleton`, `fruit` | Plumas, piedras, esqueleto, fruto partido | Sobre el terreno (historia) |
| `nest_broken` | Nido destrozado (1-1) | Sobre el terreno |
| `arch`, `totem` | Arco de ruinas, tótem roto | Fondo cercano |
| `bushbig`, `rootsbig`, `trunk` | Arbusto, raíces y tronco grandes | Fondo cercano |
| `cave_frame` | Marco de cueva de raíces (santuario del altar, 1-1) | Fondo cercano |
| `fg_leaves`, `fg_fern`, `fg_hanging` | Siluetas: hojas, helecho, colgante | Primer plano |
| `vine_long`, `vines_short`, `vine_flowers` | Liana larga, racimo de lianas cortas, liana con flores naranjas | Colgantes (apoyo arriba) |
| `roots_hanging`, `moss_curtain` | Raíces colgantes, cortina ancha de musgo | Colgantes (apoyo arriba) |
| `fern_corner`, `moss_cap` | Helecho que desborda una esquina, gorro de musgo para esquinas | Sobre el terreno (bordes) |
| `grass_edge` | Tira de hierba para el borde superior | Sobre el terreno (bordes) |
| `ivy_wall` | Hiedra para paredes | Sobre el terreno (caras de bloque) |
| `berries`, `footprints` | Arbusto de bayas rojas, huellas del mono | Sobre el terreno (historia) |
| `log_fallen`, `stump_mushrooms`, `statue_head` | Tronco caído, tocón con setas, cabeza de estatua de dinosaurio | Fondo cercano o sobre el terreno |
| `tree_young`, `bush_leaves` | Arbolito / bambú, arbusto de hojas grandes | Fondo cercano |
| `fg_branch`, `fg_grass` | Siluetas: rama diagonal, hierba alta | Primer plano |

Créditos de los 40 props: 4,5 (hojas de 22) + 3 (hojas de 18) = **7,5**.
