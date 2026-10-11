# Props de decoración del Mundo 3 (Pantano de Viento y Niebla)

**Estado (11 de octubre de 2026):** **42 props** en `Assets/Prefabs/Level/Decor/World_3/Props/`: los 40 del objetivo por mundo más los dos marcos de escena (arco del altar y marco de cueva). Ninguno está colocado todavía en los niveles.

Decorados que no interactúan: dan vida a los niveles 3-1 a 3-4 y al jefe 3 sin tocar la jugabilidad. Se colocan a mano sobre o alrededor del terreno del kit del Mundo 3 (`Terrain/World_3`: pasarela, manglar, ciénaga y turba). Son la versión del Mundo 3 de los [props del Mundo 1](Decor_World1.md) (mismas categorías, como los del [Mundo 2](Decor_World2.md)), adaptados al manglar: **verde apagado** (salvia, gris verdoso, oliva; nunca el verde brillante de la jungla), madera de manglar pálida, turba oscura, madera de pasarela podrida y acentos de lima tóxico y morado. Recogen las pistas de los niveles: juncos doblados por el viento, plumas moradas en el agua, lodo tóxico, globos de esporas, pasarelas rotas y sauces.

## Estilo e importación

Cartoon plano como el resto: contornos oscuros gruesos, colores planos con una sola sombra. Generados con Higgsfield (`gpt_image_2_5`, 4 hojas de 10, fondo transparente; referencias: los props del Mundo 1, el terreno y dos fondos del Mundo 3) y separados por código (píxeles sólidos más el borde suavizado, sin el halo de la generación; las partes sueltas de un prop, como las burbujas o las esporas, van juntas).

Sprites a **94 px por unidad**, sin mipmaps, malla rectangular. Punto de apoyo **abajo en el centro**, salvo los colgantes (**arriba en el centro**). Algunos de fondo (la choza, el sauce) salen del tamaño de un prop pequeño: se escalan al colocarlos.

## Capas (orden de dibujo)

| Capa | Sorting Order | Uso |
| --- | --- | --- |
| Fondo cercano | −15 a −13, algo oscurecido (≈ 0,8) | Manglares, árbol muerto, sauce, choza, postes |
| Sobre el terreno | −3 | Juncos, helechos, setas y detalles apoyados en las plataformas |
| Colgantes | −4 | Musgo, enredaderas, raíces y sauce colgando de bordes y bajos |
| Primer plano | 40 | Siluetas negro verdosas delante de todo |

## Props (42)

| Archivo | Pieza | Capa | Equivale en el Mundo 1 |
| --- | --- | --- | --- |
| `reeds_s`, `reeds_tall` | Juncos (eneas) pequeños; altos doblados por el viento | Sobre el terreno | helechos |
| `fern` | Helecho del pantano | Sobre el terreno | helecho grande |
| `flowers` | Flores moradas del pantano | Sobre el terreno | flores |
| `mush_cluster`, `mush_tall` | Setas pálidas con brillo lima (grupo, una alta) | Sobre el terreno | hongos |
| `moss_tuft` | Mata de musgo y hierba | Sobre el terreno | hierba |
| `stones` | Piedras con musgo | Sobre el terreno | piedras |
| `feathers` | Plumas moradas (pista del 3-1) | Sobre el terreno (historia) | plumas |
| `fish_bones` | Espina de pez | Sobre el terreno (historia) | esqueleto |
| `toxic_puddle` | Charco de lodo tóxico burbujeante | Sobre el terreno (historia) | fruto partido |
| `planks_broken` | Tablones de pasarela rotos con cuerda | Sobre el terreno (historia) | nido |
| `mooring_post` | Poste de amarre con cuerda | Sobre el terreno | bayas |
| `spore_plant` | Planta de esporas brillante | Sobre el terreno | — |
| `stump_fungus` | Tocón con hongos de repisa | Sobre el terreno / fondo cercano | tocón con setas |
| `moss_corner`, `moss_cap` | Musgo que desborda una esquina, gorro de musgo para esquinas | Sobre el terreno (bordes) | helecho de esquina, gorro de musgo |
| `grass_edge` | Tira de hierba y musgo para el borde superior | Sobre el terreno (bordes) | tira de hierba |
| `vine_wall`, `fungus_wall` | Enredadera y columna de hongos de repisa para paredes | Sobre el terreno (paredes) | hiedra |
| `mangrove_trunk` | Tronco de manglar con raíces zancudas | Fondo cercano | tronco |
| `dead_tree` | Árbol muerto doblado por el viento | Fondo cercano | arbolito |
| `willow` | Sauce llorón pequeño | Fondo cercano | arbusto de hojas |
| `mangrove_roots` | Arco de raíces de manglar | Fondo cercano | raíces grandes |
| `log_fallen` | Tronco caído con hongos | Fondo cercano / sobre el terreno | tronco caído |
| `stilt_shack` | Choza abandonada sobre pilotes | Fondo cercano | arco de ruinas |
| `posts_rope` | Postes de pasarela unidos por una cuerda | Fondo cercano | tótem |
| `boulder` | Roca grande con musgo | Fondo cercano | cabeza de estatua |
| `reed_bush` | Mata grande de juncos y hierba | Fondo cercano | arbusto grande |
| `stump_hollow` | Tocón hueco con una pluma morada | Fondo cercano | marco de cueva |
| `moss_long`, `moss_short` | Musgo colgante largo; tres cortos | Colgantes (apoyo arriba) | lianas |
| `vine_hanging` | Enredadera con hojas | Colgantes (apoyo arriba) | liana con flores |
| `roots_hanging` | Raíces pálidas de manglar | Colgantes (apoyo arriba) | — |
| `willow_curtain` | Cortina ancha de ramas de sauce | Colgantes (apoyo arriba) | cortina de musgo |
| `fg_reeds`, `fg_roots`, `fg_fern` | Siluetas: juncos, raíces zancudas, helecho | Primer plano | hierba, hojas, helecho |
| `fg_moss` | Silueta: cortina de musgo (apoyo arriba) | Primer plano | colgante |
| `fg_branch` | Silueta: rama muerta en diagonal | Primer plano | rama |

### Marcos de escena (añadidos después)

Las dos piezas que en el Mundo 1 enmarcan momentos clave: el **altar** de la habilidad va dentro del arco y el **primer vistazo del jefe** se ve a través del marco de cueva. Escalados a la misma altura que los del Mundo 1 (`arch` 369 px, `cave_frame` 453 px); el hueco del arco es transparente.

| Archivo | Pieza | Uso | Equivale en el Mundo 1 / 2 |
| --- | --- | --- | --- |
| `stone_arch` | Arco de piedra gris verdosa con glifos en espiral, musgo colgante y juncos (3,43 × 3,93 u) | Altar de la habilidad | `arch` / `rune_arch` |
| `root_cave_frame` | Marco ancho de raíces pálidas de manglar sobre turba, con musgo colgando (6,55 × 4,82 u) | Primer vistazo del jefe | `cave_frame` / `grotto_frame` |

Créditos: 4 generaciones de 10 props + 1 de los dos marcos (calidad alta, 16:9).
