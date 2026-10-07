# Zonas de peligro — piezas de nivel

**Estado (7 de octubre de 2026):** conjunto de trampas **cerrado** en doce: implementadas en Unity 6000.6.0f1 con un script común, un prefab base, nueve variantes estáticas y tres trampas dinámicas (gas tóxico ascendente, géiser volcánico y techo aplastante). Las otras cuatro trampas del inventario se descartaron. **Todas tienen arte final** salvo la zona de muerte, que es invisible a propósito. Los tres líquidos (lava, lago y lodo) son redimensionables y tienen vida (shader + partículas). Todas están a escala 1 × 1, la bola del géiser tiene un dibujo nuevo y el gas es un volumen redimensionable como los líquidos (7/10/2026). Todavía no están colocadas en ninguna escena; están comprobadas con pruebas PlayMode (ver [Pruebas](#pruebas)).

Fichas de diseño originales: [inventario, Trampas](../INVENTARIO_GAMEPLAY_PREFABS.md#trampas). Jugador: [Alma](../Player/Alma.md). Piezas no letales: [Piezas de nivel](Pieces.md).

## Qué son

Volúmenes que matan a Alma al tocarlos: pinchos, zarzas, cristales, lodo, lagos tóxicos, lava, el pilar con espinas y la zona de caída mortal bajo los abismos, más tres trampas con ciclo (gas, géiser y techo). Solo afectan a Alma; enemigos y proyectiles los ignoran. Todas matan llamando a `AlmaMotor2D.Die()`.

## Resumen: las doce trampas

| Prefab | Etiqueta | Tipo | Mata cuando… | Mundo |
| --- | --- | --- | --- | --- |
| `Trap_Spikes_Jungle` | Pinchos jungla | Estática | se toca | 1 |
| `Trap_SpikedPillar_Jungle` | Pilar con espinas | Estática + sólida | se tocan sus lados (encima es seguro) | 1 |
| `Trap_CrystalSpikes_Caves` | Cristales punzantes | Estática | se toca | 2 |
| `Trap_CrushingCeiling_Caves` | Techo aplastante | Dinámica | cae sobre Alma (cae al pasar ella por debajo) | 2 |
| `Trap_Briers_Swamp` | Zarzas pantano | Estática | se toca | 3 |
| `Trap_ToxicMud_Swamp` | Lodo tóxico | Estática (líquido) | se toca | 3 |
| `Trap_ToxicLake_Swamp` | Lago tóxico | Estática (líquido) | se toca | 3 |
| `Trap_RisingToxicGas_Swamp` | Gas tóxico | Dinámica | alcanza a Alma mientras sube | 3 |
| `Trap_BurningSpikes_Volcano` | Pinchos ardientes | Estática | se toca | 4 |
| `Trap_LavaPool_Volcano` | Foso de lava | Estática (líquido) | se toca | 4 |
| `Trap_FireGeyser_Volcano` | Géiser | Dinámica | la bola de fuego toca a Alma (el respiradero es seguro) | 4 |
| `Trap_DeathZone_Universal` | Zona de muerte (invisible) | Estática | se entra (bajo los abismos) | Todos |

## Archivos

Todo está en `Assets/Prefabs/Level/Hazards/`:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `Scripts/HazardZone2D.cs` | Lógica común: zona letal, tamaño, parte sólida opcional, activar/desactivar, visibilidad, etiqueta, evento `Killed` y sincronización del tamaño en el editor. Namespace `AlmaGame.Level` (ensamblado `Assembly-CSharp`). |
| `Scripts/HazardFx.cs` | Utilidades visuales compartidas (también para piezas, puzles y progresión): texturas y sprites generados por código (nube suave, trozo de roca, raya, **burbuja con aro**, brillo, **bola de fuego cartoon** del géiser y **píxel** blanco para barras) y sistemas de partículas pequeños. |
| `Scripts/RisingGas2D.cs` | Gas tóxico ascendente. |
| `Scripts/FireGeyser2D.cs` | Géiser volcánico. |
| `Scripts/CrushingCeiling2D.cs` | Techo aplastante. |
| `Scripts/LiquidFx2D.cs` | Burbujas, brasas/vapor y salpicadura de los líquidos. |
| `Shaders/LiquidSprite.shader` | Shader de sprite para líquidos (`AlmaGame/LiquidSprite`): latido de brillo y ondulación. |
| `Materials/Liquid_Lava.mat`, `Liquid_Toxic.mat`, `Liquid_Mud.mat`, `Liquid_Gas.mat` | Un material por líquido (y el gas) con ese shader. |
| `Sprites/Trap_*.png` | Arte de cada trampa (los líquidos usan `*_Tile.png`). |
| `Hazard_Base.prefab` | Prefab base: SpriteRenderer, un `BoxCollider2D` trigger (zona letal), un `BoxCollider2D` sólido (desactivado) y `HazardZone2D`. Usa el cuadrado blanco de `../Shared/Sprites/Level_Placeholder.png`. |
| `Trap_*.prefab` (9 estáticas) | **Prefab Variants** de `Hazard_Base`. |
| `Trap_RisingToxicGas_Swamp`, `Trap_FireGeyser_Volcano`, `Trap_CrushingCeiling_Caves` | Trampas dinámicas: prefabs independientes (no variantes) con su script propio; el gas y el techo usan además `HazardZone2D`. |

## Dos formas de dibujar

| | Dibujo fijo | Objeto recortado | Líquidos redimensionables |
| --- | --- | --- | --- |
| Prefabs | Pinchos, zarzas, cristales, pinchos ardientes, pilar | Techo aplastante, géiser | Lava, lago tóxico, lodo tóxico, **gas tóxico** |
| `Draw Mode` | **Simple** (tamaño fijo) | **Sliced** sin borders (el dibujo se ajusta a su tamaño) | **Tiled** (tile 9-slice que se repite) |
| Escala del Transform | **1 × 1** | **1 × 1** | **1 × 1** |
| Sprite | 512 × 512 px a **75 PPU** (6,83 u en pantalla) | Recortado al dibujo, **75 PPU** | Tile a 256/320 PPU |
| Cambiar el tamaño | No se adapta: `Size` solo cambia la zona letal | `Size` (techo) o `Vent Size` (géiser), o la herramienta Rect: el dibujo se estira a ese tamaño | `Size` o herramienta Rect: dibujo y zona letal juntos |
| Zona letal | `Size` menos `Hitbox Inset` (0,1), o la personalizada | Igual a `Size` (techo) / radio de la bola (géiser) | Igual a `Size` (menos `Hitbox Inset`) |

## Trampas estáticas (9)

| Prefab | Sprite | Draw / escala | `Size` | Zona letal | Particularidad |
| --- | --- | --- | --- | --- | --- |
| `Trap_Spikes_Jungle` | `Trap_Spikes_Jungle.png` | Simple / 1 | 6 × 1 | 5,8 × 0,8 | — |
| `Trap_Briers_Swamp` | `Trap_Briers_Swamp.png` | Simple / 1 | 6 × 1,6 | 5,8 × 1,4 | El sprite tiene borders (39, 192, 37, 196) que en Simple no se usan. |
| `Trap_CrystalSpikes_Caves` | `Trap_CrystalSpikes_Caves.png` | Simple / 1 | 6 × 1,2 | 5,8 × 1,0 | — |
| `Trap_BurningSpikes_Volcano` | `Trap_BurningSpikes_Volcano.png` | Simple / 1 | 6 × 1 | 5,8 × 0,8 | — |
| `Trap_SpikedPillar_Jungle` | `Trap_SpikedPillar_Jungle.png` | Simple / 1 | 3,2 × 8 | 3,2 × 6,8, desplazada −0,6 (personalizada) | Tronco **sólido** de 2 × 8 (`Solid Size`) que se puede pisar. La zona letal personalizada mata al tocar sus lados, no al estar encima. |
| `Trap_ToxicMud_Swamp` | `Trap_ToxicMud_Swamp_Tile.png` | Tiled / 1 | 4 × 0,6 | 3,9 × 0,5 | Líquido. Ver [Líquidos redimensionables](#líquidos-redimensionables). |
| `Trap_ToxicLake_Swamp` | `Trap_ToxicLake_Swamp_Tile.png` | Tiled / 1 | 6 × 1,5 | 5,9 × 1,4 | Líquido. |
| `Trap_LavaPool_Volcano` | `Trap_LavaPool_Volcano_Tile.png` | Tiled / 1 | 5 × 1,2 | 4,9 × 1,1 | Líquido. |
| `Trap_DeathZone_Universal` | ninguno | Tiled / 1 | 10 × 2 | 10 × 2 | **Invisible en juego** (`Visible In Game` = no); en el editor se ve magenta translúcido (1; 0; 1; 0,35). Zona letal completa (`Hitbox Inset` 0). Se coloca bajo los abismos. |

Todas las estáticas con arte tienen el color en blanco (sin tinte) y `Show Label` desactivado; la zona de muerte conserva la etiqueta, pero no se ve al jugar. `Hitbox Inset` es 0,1 en las cinco de dibujo fijo.

**Cambio de escala (7/10/2026):** estas cinco estaban a escala 2 × 2 con sprites a 150 PPU, así que su zona letal real era el doble de `Size`. Ahora están a escala 1 × 1 con los sprites a 75 PPU y todas las medidas dobladas (`Size`, `Hitbox Inset`, zona personalizada y parte sólida). **El resultado en el mundo es idéntico** (comprobado midiendo en Unity los límites de sprite y colisionadores antes y después) y `Size` vuelve a ser la medida real.

## Líquidos redimensionables

Los líquidos pueden tener cualquier tamaño, así que usan un **tile 9-slice** que el `SpriteRenderer` repite (`Draw Mode = Tiled`) hasta llenar `Size`, con escala **1 × 1**: el sprite y la zona letal miden siempre lo mismo.

| Líquido | Sprite | Píxeles | PPU | Border inferior | Material | Alto mínimo |
| --- | --- | --- | ---: | --- | --- | ---: |
| Lava | `Trap_LavaPool_Volcano_Tile.png` | 512 × 420 (2 × 1,64 u) | 256 | 134 px: superficie | `Liquid_Lava` | 0,52 u |
| Lago tóxico | `Trap_ToxicLake_Swamp_Tile.png` | 512 × 460 (2 × 1,8 u) | 256 | 231 px: superficie + paso de verde a morado | `Liquid_Toxic` | 0,9 u |
| Lodo tóxico | `Trap_ToxicMud_Swamp_Tile.png` | 512 × 441 (1,6 × 1,38 u) | **320** | 132 px: superficie con burbujas y goteos | `Liquid_Mud` | 0,41 u |

- **Superficie** (el border): se repite solo en horizontal y mantiene su alto. **Cuerpo** (el centro): se repite en horizontal y en vertical sin juntas.
- **Imagen boca abajo + `Flip Y`:** Unity repite el centro empezando por el lado opuesto al border, así que el último tile queda recortado junto a él. Con la imagen invertida y `Flip Y` activado en el renderer, el tile completo queda pegado a la superficie (la unión es continua porque ambas partes salen de filas contiguas del dibujo original) y el recorte cae en el fondo. **No desactivar `Flip Y`.**
- **Wrap Mode U = Repeat** en los tres tiles: lo necesita la ondulación del shader.
- **Alto mínimo:** `Size.y` debe ser al menos el alto del border; por debajo Unity aplasta la superficie.
- **Lodo:** PPU más alto para que la superficie quepa en un lodo de 0,6 u. Las tres burbujas lima de su superficie se repiten cada 1,6 u (decisión: se aceptan).
- **Origen del arte:** generado con IA (una generación por líquido, 7/10/2026) con el sprite original del prefab como referencia de estilo, y procesado: fondo eliminado por encima de la superficie, juntas cortadas por el camino de menor diferencia (*image quilting*) y reducido a 512 px de ancho.

### Shader `AlmaGame/LiquidSprite`

Pipeline integrado; respeta Tiled, Flip X/Y, el color del renderer e instancing. Coste: una lectura de textura y unas pocas operaciones por píxel, sin texturas extra ni lectura de pantalla.

- **Latido:** solo los píxeles más claros que `Bright Threshold` reciben `Glow Color`, con una onda que recorre el líquido (posición de mundo × `Pulse Wave Scale`), así que no se enciende todo a la vez.
- **Ondulación:** desplazamiento horizontal de la textura en onda (`Sway`).

| Propiedad | Lava | Lago tóxico | Lodo tóxico |
| --- | --- | --- | --- |
| `Glow Color` | (1; 0,75; 0,25) | (0,75; 1; 0,3) | (0,8; 1; 0,3) |
| `Bright Threshold` | 0,5 | 0,55 | 0,6 |
| `Pulse Strength` | 0,6 | 0,45 | 0,3 |
| `Pulse Speed` | 0,8 | 0,5 | 0,35 |
| `Pulse Wave Scale` | 0,45 / u | 0,45 / u | 0,45 / u |
| `Sway Amount` (UV) | 0,007 | 0,006 | 0,003 |
| `Sway Speed` | 0,8 | 0,6 | 0,4 |
| `Sway Wave Scale` | 2,2 / u | 2,2 / u | 2,2 / u |

### Partículas `LiquidFx2D`

Las cantidades escalan con el ancho (`Size.x`). Cada sistema tiene un tope de 24 partículas (un lago de 10 u ronda las 10–20 vivas). Las burbujas y las brasas dejan de emitir cuando el líquido sale de pantalla (`OnBecameInvisible`). La salpicadura solo sale cuando el líquido mata a Alma, en su posición sobre la superficie (evento `HazardZone2D.Killed`).

| Campo | Lava | Lago tóxico | Lodo tóxico |
| --- | --- | --- | --- |
| `Bubble Rate` (por u y s) | 0,6 | 1,4 | 0,9 |
| `Bubble Size` | 0,2–0,36 | 0,22–0,4 | 0,18–0,32 |
| `Bubble Color` | (1; 0,92; 0,6) | (0,85; 1; 0,55) | (0,85; 1; 0,45) |
| `Bubble Depth` (bajo la superficie) | 0,14 | 0,1 | 0,06 |
| `Ring Bubbles` | no (manchas difusas) | **sí** (aro con reflejo) | **sí** |
| `Ember Rate` (por u y s) | 0,4 (brasas) | 0,6 (vapor) | 0,3 (vapor) |
| `Ember Speed` | 0,4–0,9 | 0,25–0,5 | 0,15–0,3 |
| `Ember Size` | 0,06–0,11 | 0,25–0,45 | 0,2–0,35 |
| `Ember Color` | (1; 0,7; 0,3) | (0,5; 0,9; 0,25; 70 %) | (0,6; 0,65; 0,25; 60 %) |
| `Splash Count` | 14 | 14 | 10 |
| `Splash Color` | (1; 0,5; 0,1) | (0,7; 1; 0,3) | (0,5; 0,42; 0,12) |
| `Max Particles Per System` | 24 | 24 | 24 |
| `Particle Material` | Sprites-Default | Sprites-Default | Sprites-Default |

Fijos en código: las burbujas viven 0,7–1,1 s y crecen de 0,4× a 1,15×; las brasas viven 1,2–2,2 s y encogen a 0,3×; la salpicadura vive 0,4–0,6 s, sale a 2–3,2 u/s con gravedad 1,2. Las burbujas con aro usan `HazardFx.Bubble()` porque las manchas difusas se perdían sobre una superficie lima.

## Trampas dinámicas

Las tres están a escala 1 × 1: el gas usa un tile repetible como los líquidos; el techo y el géiser, su sprite recortado en *Sliced*. Los efectos (humo, chispas, polvo, bola de fuego) se generan por código con `HazardFx`, con un presupuesto fijo de partículas.

### Gas tóxico ascendente (`Trap_RisingToxicGas_Swamp`)

Un **volumen** que sube desde un fondo fijo y crece: se dibuja como los líquidos, con un tile repetible (`Trap_RisingToxicGas_Swamp_Tile.png`, 512 × 498 px a 256 PPU, border inferior de 155 px = 0,6 u) en *Tiled*, con la imagen boca abajo y **Flip Y**, a escala 1 × 1. Arriba, un borde de nubes con contorno y sombras moradas (fijo, siempre en la cima); debajo, niebla verde con remolinos morados que se repite al crecer. Color (1; 1; 1; 0,7): translúcido. Orden de dibujo 5 (delante de Alma, para que se vea que está dentro). Sobre el borde, nubes de partículas en dos tonos de verde que burbujean y suben.

Arte generado con IA (una generación, 7/10/2026) con el sprite anterior (una nube con forma de arbusto que no podía crecer, ya borrado) como referencia de estilo, y procesado como los líquidos. Material `Materials/Liquid_Gas.mat` (shader `AlmaGame/LiquidSprite`):

| Propiedad | Valor |
| --- | --- |
| `Glow Color` | (0,8; 1; 0,4) — laten los reflejos lima |
| `Bright Threshold` | 0,6 |
| `Pulse Strength` / `Pulse Speed` / `Pulse Wave Scale` | 0,25 / 0,3 / 0,3 por u |
| `Sway Amount` / `Sway Speed` / `Sway Wave Scale` | 0,012 / 0,35 / 1,2 por u (ondulación lenta y amplia, de gas) |

| Fase | Qué pasa |
| --- | --- |
| **Espera** | Quieto a su altura inicial hasta que Alma cruza la línea de activación (o desde el inicio, según `Activation`). |
| **Aviso (3 s)** | El gas parpadea más opaco (de 70 % a 95 %) y las nubes se agitan (×2,5). |
| **Subida** | Sube a 0,9 u/s manteniendo el fondo fijo; mata a Alma en todo su volumen. |
| **Lleno** | Se detiene en `Max Height` (10 u sobre el fondo). |

- Al reaparecer Alma (evento `Respawned`) vuelve a su altura inicial y a la espera.
- `Activate()` y `Stop()` permiten que un encuentro lo arranque o lo detenga.
- Tamaño inicial: `Size` de su `HazardZone2D` = 12 × 1 (mínimo 0,6 de alto, el borde de nubes), `Hitbox Inset` 0,1. El ancho se mantiene; la altura crece con `SetSize()` y el dibujo crece con ella. Con la escala 1 × 1 vuelven a valer los valores del diseño: sube 0,9 u/s hasta 10 u (a escala 2 eran el doble).

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Activation` | Al cruzar X | `OnStart` o `WhenAlmaPassesX`. |
| `Activation Offset X` | 0 | Línea de activación relativa a su centro. |
| `Reset On Respawn` | sí | Volver al estado inicial al reaparecer Alma. |
| `Warning Time` | 3 | Aviso (s). |
| `Rise Speed` | 0,9 | Velocidad de subida (u/s). |
| `Max Height` | 10 | Altura máxima desde el fondo. |
| `Puff Color A` / `B` | (0,55; 0,95; 0,35; 75 %) / (0,35; 0,75; 0,3; 75 %) | Colores de las nubes del borde. |
| `Puffs Per Unit` | 3 | Nubes por unidad de ancho y segundo. |

Gizmos: **verde** = altura máxima; **amarillo** = línea de activación.

### Géiser volcánico (`Trap_FireGeyser_Volcano`)

Sprite `Trap_FireGeyser_Volcano_Vent.png` (el respiradero recortado del sprite original, ya borrado; 439 × 135 px a 75 PPU), en *Sliced* a escala 1 × 1 y estirado a `Vent Size` (5,853 × 1,8). No es letal y no tiene colisionador. Escupe una bola de fuego hacia arriba que vuelve a caer en él.

| Fase | Duración | Qué pasa |
| --- | --- | --- |
| **Reposo** | 2,2 s | Nada. |
| **Aviso** | 0,8 s | El respiradero parpadea en naranja y suelta humo oscuro y chispas hacia arriba. |
| **Erupción** | 1,2 s | La bola sube 4 u y cae (parábola), estirándose según su velocidad. Al volver salpica 8 gotas de lava. |

- **Solo la bola mata** (círculo de `Fireball Radius` comprobado cada paso de física).
- **Dibujo de la bola** (7/10/2026, sustituye a las dos manchas difusas): sprite cartoon generado por código (`HazardFx.Fireball()`, 96 × 160 px): cabeza redonda con núcleo blanco-amarillo, tres lenguas de llama afiladas que pasan de naranja a rojo y contorno rojo oscuro como el resto del arte. Mide 2,8 × `Fireball Radius` de ancho; sus lenguas quedan siempre detrás del movimiento (hacia abajo al subir, hacia arriba al caer, girando en lo alto) y parpadea (±5 % de ancho, ±9 % de alto). Detrás lleva un halo naranja suave (alfa 45 %, 3,4 × radio).
- **Estela:** nubes de 0,8–1,3 × radio que se enfrían de amarillo a naranja, rojo y humo en 0,3–0,5 s (10 por unidad recorrida, máx. 36). **Chispas:** puntos de 0,05–0,1 u que saltan de la bola (5 por unidad recorrida, máx. 24) y caen con gravedad 0,6.
- `Start Delay` desfasa varios géiseres entre sí.
- **Respeta el `Draw Mode` del prefab** (desde el 7/10/2026 ya no fuerza *Tiled*).
- **Tamaño en el editor:** con `[ExecuteAlways]` (solo actúa en modo edición; en juego no cambia nada), arrastrar el respiradero con la herramienta Rect actualiza `Vent Size`, y cambiar `Vent Size` en el Inspector redimensiona el sprite. Al dar Play se conserva lo dibujado.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Rest Time` / `Warning Time` / `Erupt Time` | 2,2 / 0,8 / 1,2 | Ciclo (s). `Erupt Time` es el vuelo completo. |
| `Start Delay` | 0 | Retraso del primer ciclo (s). |
| `Height` | 4 | Altura máxima de la bola sobre el respiradero. |
| `Fireball Radius` | **0,5** | Radio letal (antes 0,35; con la escala 2 se veía de 0,7). |
| `Fireball Color` / `Core Color` | (1; 0,45; 0,1) / (1; 0,9; 0,4) | Colores de la bola y su núcleo. |
| `Vent Size` | (5,853; 1,8) | Tamaño del dibujo del respiradero; la bola y el humo salen de su borde superior. |
| `Warning Glow` | (1; 0,55; 0,2) | Color del parpadeo del aviso. |
| `Label` / `Show Label` | «Géiser» / no | Etiqueta provisional. |

Partículas: estela (máx. 36), chispas (máx. 24), humo (máx. 20), salpicadura (máx. 12). Gizmos: **naranja** = trayectoria y radio letal.

### Techo aplastante (`Trap_CrushingCeiling_Caves`)

Sprite `Trap_CrushingCeiling_Caves_Block.png` (recortado del sprite original, ya borrado; 450 × 115 px a 75 PPU), en *Sliced* a escala 1 × 1 y ajustado a `Size` = **6 × 1,533** (se ve igual que antes a escala 2). Bloque colocado en el techo, sólido mientras está arriba (`Solid` sí, `Solid Size` 6 × 1,533); zona letal 5,9 × 1,433. Cae cuando Alma pasa por debajo.

| Fase | Duración | Qué pasa |
| --- | --- | --- |
| **Listo** | — | Espera arriba hasta que Alma entra en la zona de debajo: su ancho más `Detect Margin` a cada lado, entre el bloque y el suelo. |
| **Aviso** | 0,25 s | Tiembla y suelta polvillo. Después cae aunque Alma ya se haya movido. |
| **Caída** | hasta tocar algo | Cae acelerando (50 u/s²) hasta el primer colisionador sólido de debajo (suelo, plataforma o Alma). Deja de ser sólido mientras cae. Si toca a Alma, la mata. Límite: 20 u. |
| **Impacto** | — | Se quiebra donde tocó: 14 trozos de roca, 10 nubes de polvo y temblor de cámara (0,1 u, 0,15 s) si Alma está a menos de 10 u. Desaparece. |
| **Oculto** | 2,4 s | Antes de volver a formarse. |
| **Reaparece** | 0,3 s | Se forma arriba creciendo hacia abajo. Vuelve a ser sólido y queda listo. |

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Warning Time` | 0,25 | Temblor antes de caer (s). 0 = cae al instante. |
| `Hidden Time` / `Reform Time` | 2,4 / 0,3 | Tiempo desaparecido y en reaparecer (s). |
| `Drop Acceleration` | 50 | Aceleración de la caída (u/s²). |
| `Max Drop Distance` | 20 | Distancia máxima si no encuentra nada debajo. |
| `Detect Margin` | 0,5 | Margen a cada lado para detectar a Alma. |
| `Dust Color` | (0,75; 0,7; 0,62; 80 %) | Color del polvo. |
| `Shake Amplitude` / `Shake Duration` | 0,1 / 0,15 | Temblor de cámara al impactar. |

- La zona de detección termina en el suelo que hay debajo (medido al empezar), para que no caiga si Alma pasa por un piso inferior.
- La caída se comprueba con un barrido (`BoxCast`) de la caja del bloque cada frame; los contactos que ya existen al empezar (techo, paredes) se ignoran.

Partículas: polvillo de aviso (máx. 20), trozos (máx. 20), nube (máx. 16). Gizmos: **naranja** = caída máxima; **amarillo** = zona de detección.

## Comportamiento común (`HazardZone2D`)

- **Muerte:** la zona letal es un trigger; si Alma entra o permanece en ella se llama a `AlmaMotor2D.Die()` (si no estaba ya muerta) y se lanza el evento `Killed(posición)`, que usan los líquidos para salpicar.
- **Tamaño:** `Size` fija el `size` del sprite y la zona letal. En modo *Tiled* el arte se repite; en modo *Simple* el `size` no afecta al dibujo.
- **Zona letal:** por defecto `Size` menos `Hitbox Inset` por cada lado. Con `Custom Hitbox` se define a mano (desplazamiento y tamaño).
- **Parte sólida (opcional):** con `Solid` se activa un segundo colisionador no trigger con su propio desplazamiento y tamaño.
- **Activo:** `IsActive` / `Active` enciende o apaga la zona letal sin cambiar lo que se ve.
- **Visible en juego:** si se desactiva, el sprite y la etiqueta se ocultan al empezar; la zona sigue matando.
- **Etiqueta provisional:** si `Show Label` está activo, al empezar se crea un `TextMesh` blanco 0,3 u por encima con el nombre. En el editor el nombre sale junto a los gizmos.
- **En juego:** `SetSize()` redimensiona sprite, colisionadores y etiqueta (lo usa el gas). `SolidEnabled` activa o desactiva la parte sólida (lo usa el techo).
- **Tamaño en el editor:** con `[ExecuteAlways]` (solo actúa en modo edición; en juego `Awake` hace la preparación habitual), si el sprite es *Tiled* o *Sliced* se puede cambiar el tamaño con `Size` en el Inspector **o arrastrando los tiradores con la herramienta Rect (T)**. Detecta qué lado cambió y sincroniza el otro y la zona letal; al dar Play se conserva lo dibujado. Admite deshacer.

## Valores (`HazardZone2D`, prefab base)

| Campo | Base | Uso |
| --- | ---: | --- |
| `Size` | (3; 0,5) | Tamaño del sprite y base de la zona letal. |
| `Hitbox Inset` | 0,05 | Margen que se resta a cada lado de la zona letal. |
| `Custom Hitbox` | no | Usar `Hitbox Offset` y `Hitbox Size` en lugar del margen. |
| `Hitbox Offset` / `Hitbox Size` | (0; 0) / (1; 1) | Zona letal personalizada. |
| `Solid` | no | Activa el colisionador sólido. |
| `Solid Offset` / `Solid Size` | (0; 0) / (1; 1) | Parte sólida. |
| `Active` | sí | Zona letal encendida. |
| `Visible In Game` | sí | Mostrar sprite y etiqueta al jugar. |
| `Label` / `Show Label` | «Peligro» / sí | Etiqueta provisional (todas las trampas con arte la tienen apagada). |
| `Lethal Collider` / `Solid Collider` | (del prefab) | Los dos `BoxCollider2D`. No cambiar. |

El prefab base conserva el marcador provisional: `Level_Placeholder.png` (4 × 4 px a 4 PPU) en modo Tiled, tinte rojo (0,9; 0,2; 0,2).

## Cómo usarlas

1. Arrastra la variante que necesites (no `Hazard_Base`) a la escena.
2. **Líquidos:** ajusta el tamaño con `Size` o con la herramienta Rect; nunca con la escala.
3. **Trampas de dibujo fijo:** el dibujo no se adapta; para un tramo más largo, coloca varias instancias.
4. Comprueba los gizmos: **rojo** = zona letal; **gris** = parte sólida.
5. Para la zona de caída mortal, colócala bajo cada abismo, cubriendo todo su ancho.

## Coste

Por trampa estática: un SpriteRenderer y uno o dos `BoxCollider2D` estáticos. Sin consultas físicas por frame (la detección la hace el trigger). Los líquidos añaden su shader (barato) y tres sistemas de partículas pequeños; las dinámicas, sus sistemas de partículas (ver cada una).

## Pruebas

Comprobado el 7/10/2026 con pruebas PlayMode (en una copia del proyecto, junto a las 8 de movimiento de Alma; las 20 pasan):

- Pinchos, zarzas, cristales y pinchos ardientes matan a Alma al caminar hacia ellos.
- El pilar con espinas: encima se puede estar de pie; tocarlo de lado mata.
- La lava mata.

Los problemas detectados al revisar los prefabs (techo, gas, géiser y trampas estáticas a escala 2 × 2) están corregidos.

## Pendiente

- Colocar las trampas en niveles y probarlas.
- Gas: la regla del diseño de «quedar 4 u por debajo del checkpoint» al reaparecer no está implementada; hoy vuelve a su altura inicial.
- Géiser: el concepto (#13 de `Art/Traps/Traps_ConceptSheet_v1.png`) muestra un **chorro de lava vertical**; el juego lanza una bola de fuego. Decidir si se cambia.
- **Fuera de alcance (decisión del 5/10/2026):** erupción de lava ascendente, magma ascendente del jefe final, chorro de fuego de aterrizaje y puerta de llamas no se harán.
- El inventario prevé `Assets/_Project/Prefabs/Traps/`; están en `Assets/Prefabs/Level/Hazards/`.
