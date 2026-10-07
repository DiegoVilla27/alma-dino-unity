# Zonas de peligro — piezas de nivel

**Estado (7 de octubre de 2026):** conjunto de trampas **cerrado** en doce: implementadas en Unity 6000.6.0f1 con un script común, un prefab base, nueve variantes estáticas y tres trampas dinámicas (gas tóxico ascendente, géiser volcánico y techo aplastante). Las otras cuatro trampas del inventario se descartaron. **Todas tienen arte final** salvo la zona de muerte, que es invisible a propósito. Los tres líquidos (lava, lago y lodo) son redimensionables y tienen vida (shader + partículas). Todavía no están colocadas en ninguna escena ni probadas en juego, y hay [problemas conocidos](#problemas-conocidos) en las trampas a escala 2 × 2.

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
| `Scripts/HazardFx.cs` | Utilidades visuales compartidas: texturas generadas por código (nube suave, trozo de roca, raya, **burbuja con aro**, brillo) y sistemas de partículas pequeños. |
| `Scripts/RisingGas2D.cs` | Gas tóxico ascendente. |
| `Scripts/FireGeyser2D.cs` | Géiser volcánico. |
| `Scripts/CrushingCeiling2D.cs` | Techo aplastante. |
| `Scripts/LiquidFx2D.cs` | Burbujas, brasas/vapor y salpicadura de los líquidos. |
| `Shaders/LiquidSprite.shader` | Shader de sprite para líquidos (`AlmaGame/LiquidSprite`): latido de brillo y ondulación. |
| `Materials/Liquid_Lava.mat`, `Liquid_Toxic.mat`, `Liquid_Mud.mat` | Un material por líquido con ese shader. |
| `Sprites/Trap_*.png` | Arte de cada trampa (los líquidos usan `*_Tile.png`). |
| `Hazard_Base.prefab` | Prefab base: SpriteRenderer, un `BoxCollider2D` trigger (zona letal), un `BoxCollider2D` sólido (desactivado) y `HazardZone2D`. Usa el cuadrado blanco de `../Shared/Sprites/Level_Placeholder.png`. |
| `Trap_*.prefab` (9 estáticas) | **Prefab Variants** de `Hazard_Base`. |
| `Trap_RisingToxicGas_Swamp`, `Trap_FireGeyser_Volcano`, `Trap_CrushingCeiling_Caves` | Trampas dinámicas: prefabs independientes (no variantes) con su script propio; el gas y el techo usan además `HazardZone2D`. |

## Dos formas de dibujar

| | Trampas de dibujo fijo | Líquidos redimensionables |
| --- | --- | --- |
| Prefabs | Pinchos, zarzas, cristales, pinchos ardientes, pilar, gas, géiser, techo | Lava, lago tóxico, lodo tóxico |
| `Draw Mode` | **Simple** (el dibujo tiene un tamaño fijo) | **Tiled** (tile 9-slice que se repite) |
| Escala del Transform | **2 × 2** | **1 × 1** |
| Sprite | 512 × 512 px a **150 PPU** (3,41 u; ×2 = 6,83 u en pantalla) | Tile a 256/320 PPU |
| Cambiar el tamaño | No se adapta: cambiar `Size` solo cambia la zona letal | `Size` o la herramienta Rect: dibujo y zona letal van juntos |
| Zona letal real | **El doble de `Size`** (la escala la multiplica) | Igual a `Size` (menos `Hitbox Inset`) |

## Trampas estáticas (9)

| Prefab | Sprite | Draw / escala | `Size` | Zona letal real | Particularidad |
| --- | --- | --- | --- | --- | --- |
| `Trap_Spikes_Jungle` | `Trap_Spikes_Jungle.png` | Simple / 2 | 3 × 0,5 | 5,8 × 0,8 | — |
| `Trap_Briers_Swamp` | `Trap_Briers_Swamp.png` | Simple / 2 | 3 × 0,8 | 5,8 × 1,4 | El sprite tiene borders (39, 192, 37, 196) que en Simple no se usan. |
| `Trap_CrystalSpikes_Caves` | `Trap_CrystalSpikes_Caves.png` | Simple / 2 | 3 × 0,6 | 5,8 × 1,0 | — |
| `Trap_BurningSpikes_Volcano` | `Trap_BurningSpikes_Volcano.png` | Simple / 2 | 3 × 0,5 | 5,8 × 0,8 | — |
| `Trap_SpikedPillar_Jungle` | `Trap_SpikedPillar_Jungle.png` | Simple / 2 | 1,6 × 4 | 3,2 × 6,8 (desplazada −0,6) | Tronco **sólido** de 1 × 4 (2 × 8 en pantalla) que se puede pisar. Zona letal personalizada (1,6 × 3,4, desplazada 0,3 hacia abajo antes de escalar): mata al tocar sus lados, no al estar encima. |
| `Trap_ToxicMud_Swamp` | `Trap_ToxicMud_Swamp_Tile.png` | Tiled / 1 | 4 × 0,6 | 3,9 × 0,5 | Líquido. Ver [Líquidos redimensionables](#líquidos-redimensionables). |
| `Trap_ToxicLake_Swamp` | `Trap_ToxicLake_Swamp_Tile.png` | Tiled / 1 | 6 × 1,5 | 5,9 × 1,4 | Líquido. |
| `Trap_LavaPool_Volcano` | `Trap_LavaPool_Volcano_Tile.png` | Tiled / 1 | 5 × 1,2 | 4,9 × 1,1 | Líquido. |
| `Trap_DeathZone_Universal` | ninguno | Tiled / 1 | 10 × 2 | 10 × 2 | **Invisible en juego** (`Visible In Game` = no); en el editor se ve magenta translúcido (1; 0; 1; 0,35). Zona letal completa (`Hitbox Inset` 0). Se coloca bajo los abismos. |

Todas las estáticas con arte tienen el color en blanco (sin tinte) y `Show Label` desactivado; la zona de muerte conserva la etiqueta, pero no se ve al jugar.

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

Arte final en modo Simple a escala 2 × 2 (ver [Problemas conocidos](#problemas-conocidos)). Los efectos (humo, chispas, polvo, bola de fuego) se generan por código con `HazardFx`, con un presupuesto fijo de partículas.

### Gas tóxico ascendente (`Trap_RisingToxicGas_Swamp`)

Sprite `Trap_RisingToxicGas_Swamp.png`, color (1; 1; 1; 0,55) y orden de dibujo 5 (delante de Alma, para que se vea que está dentro). Sube desde un fondo fijo; el borde superior son nubes en dos tonos de verde que burbujean y suben.

| Fase | Qué pasa |
| --- | --- |
| **Espera** | Quieto a su altura inicial hasta que Alma cruza la línea de activación (o desde el inicio, según `Activation`). |
| **Aviso (3 s)** | El gas parpadea más opaco y las nubes se agitan (×2,5). |
| **Subida** | Sube a 0,9 u/s manteniendo el fondo fijo; mata a Alma en todo su volumen. |
| **Lleno** | Se detiene en `Max Height` (10 u sobre el fondo). |

- Al reaparecer Alma (evento `Respawned`) vuelve a su altura inicial y a la espera.
- `Activate()` y `Stop()` permiten que un encuentro lo arranque o lo detenga.
- Tamaño inicial: `Size` de su `HazardZone2D` = 12 × 1, `Hitbox Inset` 0,1. El ancho se mantiene; la altura crece con `SetSize()`.

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

Sprite `Trap_FireGeyser_Volcano.png` (el respiradero, no letal: se puede pisar). Escupe una bola de fuego hacia arriba que vuelve a caer en él.

| Fase | Duración | Qué pasa |
| --- | --- | --- |
| **Reposo** | 2,2 s | Nada. |
| **Aviso** | 0,8 s | El respiradero parpadea en naranja y suelta humo oscuro y chispas hacia arriba. |
| **Erupción** | 1,2 s | La bola sube 4 u y cae (parábola), estirándose según su velocidad, con un núcleo amarillo que palpita y una estela de llamas. Al volver salpica 8 gotas de lava. |

- **Solo la bola mata** (círculo de `Fireball Radius` comprobado cada paso de física).
- `Start Delay` desfasa varios géiseres entre sí.
- **Respeta el `Draw Mode` del prefab** (desde el 7/10/2026 ya no fuerza *Tiled*); solo ajusta `size`, que en modo Simple no tiene efecto.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Rest Time` / `Warning Time` / `Erupt Time` | 2,2 / 0,8 / 1,2 | Ciclo (s). `Erupt Time` es el vuelo completo. |
| `Start Delay` | 0 | Retraso del primer ciclo (s). |
| `Height` | 4 | Altura máxima de la bola sobre el respiradero. |
| `Fireball Radius` | 0,35 | Radio letal. |
| `Fireball Color` / `Core Color` | (1; 0,45; 0,1) / (1; 0,9; 0,4) | Colores de la bola y su núcleo. |
| `Vent Size` | (1,2; 0,4) | Tamaño del respiradero (posición de salida de la bola y del humo). |
| `Warning Glow` | (1; 0,55; 0,2) | Color del parpadeo del aviso. |
| `Label` / `Show Label` | «Géiser» / no | Etiqueta provisional. |

Partículas: estela (máx. 30), humo (máx. 20), salpicadura (máx. 12). Gizmos: **naranja** = trayectoria y radio letal.

### Techo aplastante (`Trap_CrushingCeiling_Caves`)

Sprite `Trap_CrushingCeiling_Caves.png`. Bloque colocado en el techo, sólido mientras está arriba (`Solid` sí, 3 × 1); cae cuando Alma pasa por debajo.

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
3. **Trampas de dibujo fijo:** el dibujo no se adapta; para un tramo más largo, coloca varias instancias. Recuerda que su zona letal es el doble de `Size`.
4. Comprueba los gizmos: **rojo** = zona letal; **gris** = parte sólida.
5. Para la zona de caída mortal, colócala bajo cada abismo, cubriendo todo su ancho.

## Coste

Por trampa estática: un SpriteRenderer y uno o dos `BoxCollider2D` estáticos. Sin consultas físicas por frame (la detección la hace el trigger). Los líquidos añaden su shader (barato) y tres sistemas de partículas pequeños; las dinámicas, sus sistemas de partículas (ver cada una).

## Problemas conocidos

Detectados el 7/10/2026 al revisar los prefabs; **sin corregir**:

- **Techo aplastante a escala 2 × 2:** su script vuelve a poner la escala a 1 × 1 al reaparecer y al terminar de formarse (`transform.localScale = Vector3.one`). Tras la primera caída quedaría a la mitad de tamaño. Además mide la caída con `Size` (sin escalar).
- **Gas tóxico en modo Simple:** sube cambiando `Size` con `SetSize()`, pero en Simple eso no cambia el dibujo: la zona letal crece y el sprite no.
- **Géiser a escala 2 × 2:** la bola se ve el doble de grande que su radio letal (0,35) y el punto de salida (`Vent Size`) no tiene en cuenta la escala.
- **Trampas estáticas a escala 2 × 2:** la zona letal real es el doble de `Size` (ver la tabla); hay que tenerlo en cuenta al diseñar los niveles.

Solución propuesta: dejar estas trampas a escala 1 × 1 y ajustar el tamaño en pantalla con el PPU de cada sprite (como en los líquidos).

## Pendiente

- Corregir los [problemas conocidos](#problemas-conocidos).
- Colocar las trampas en niveles y probarlas.
- Gas: la regla del diseño de «quedar 4 u por debajo del checkpoint» al reaparecer no está implementada; hoy vuelve a su altura inicial.
- Sprite original `Sprites/Trap_ToxicMud_Swamp.png` sin uso (el lodo usa `_Tile`); se puede borrar.
- **Fuera de alcance (decisión del 5/10/2026):** erupción de lava ascendente, magma ascendente del jefe final, chorro de fuego de aterrizaje y puerta de llamas no se harán.
- El inventario prevé `Assets/_Project/Prefabs/Traps/`; están en `Assets/Prefabs/Level/Hazards/`.
