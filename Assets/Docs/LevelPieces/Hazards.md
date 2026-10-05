# Zonas de peligro — piezas de nivel

**Estado (5 de octubre de 2026):** conjunto de trampas **cerrado** en doce: implementadas en Unity 6000.6.0f1 con un script común, un prefab base, nueve variantes estáticas y tres trampas dinámicas (gas tóxico ascendente, géiser volcánico y techo aplastante). Las otras cuatro trampas del inventario se descartaron. Usan **cajas de color con una etiqueta de texto** como marcador hasta que llegue el arte. Todavía no están colocadas en ninguna escena ni probadas en juego (ver [Pendiente](#pendiente)).

Fichas de diseño originales: [inventario, Trampas](../INVENTARIO_GAMEPLAY_PREFABS.md#trampas). Jugador: [Alma](../Player/Alma.md).

## Qué son

Volúmenes que matan a Alma al tocarlos: pinchos, zarzas, cristales, lodo, lagos tóxicos, lava, el pilar con espinas y la zona de caída mortal bajo los abismos. No tienen ciclo de ataque: son peligrosos siempre (salvo que otro script los desactive). Solo afectan a Alma; enemigos y proyectiles los ignoran.

## Resumen: las doce trampas

| Prefab | Etiqueta | Tipo | Mata cuando… | Mundo |
| --- | --- | --- | --- | --- |
| `Trap_Spikes_Jungle` | Pinchos jungla | Estática | se toca | 1 |
| `Trap_SpikedPillar_Jungle` | Pilar con espinas | Estática + sólida | se tocan sus lados (encima es seguro) | 1 |
| `Trap_CrystalSpikes_Caves` | Cristales punzantes | Estática | se toca | 2 |
| `Trap_CrushingCeiling_Caves` | Techo aplastante | Dinámica | cae sobre Alma (cae al pasar ella por debajo) | 2 |
| `Trap_Briers_Swamp` | Zarzas pantano | Estática | se toca | 3 |
| `Trap_ToxicMud_Swamp` | Lodo tóxico | Estática | se toca | 3 |
| `Trap_ToxicLake_Swamp` | Lago tóxico | Estática | se toca | 3 |
| `Trap_RisingToxicGas_Swamp` | Gas tóxico | Dinámica | alcanza a Alma mientras sube | 3 |
| `Trap_BurningSpikes_Volcano` | Pinchos ardientes | Estática | se toca | 4 |
| `Trap_LavaPool_Volcano` | Foso de lava | Estática | se toca | 4 |
| `Trap_FireGeyser_Volcano` | Géiser | Dinámica | la bola de fuego toca a Alma (el respiradero es seguro) | 4 |
| `Trap_DeathZone_Universal` | Zona de muerte (invisible) | Estática | se entra (bajo los abismos) | Todos |

Todas están en `Assets/Prefabs/Level/Hazards/` y matan llamando a `AlmaMotor2D.Die()`.

## Archivos

Todo está en `Assets/Prefabs/Level/Hazards/`:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `Scripts/HazardZone2D.cs` | Lógica común: zona letal, tamaño, parte sólida opcional, activar/desactivar, visibilidad y etiqueta. Namespace `AlmaGame.Level` (ensamblado `Assembly-CSharp`). |
| `Hazard_Base.prefab` | Prefab base: SpriteRenderer (modo *Tiled*), un `BoxCollider2D` trigger (zona letal), un `BoxCollider2D` sólido (desactivado) y `HazardZone2D`. |
| `Trap_*.prefab` (9) | **Prefab Variants** de `Hazard_Base`: cada uno solo cambia nombre, color, tamaño, etiqueta y sus opciones propias. |
| `Scripts/HazardFx.cs` | Utilidades visuales compartidas: texturas generadas por código (nube suave, trozo de roca, brillo) y creación de sistemas de partículas pequeños. |
| `Scripts/RisingGas2D.cs` | Lógica del gas tóxico ascendente. |
| `Scripts/FireGeyser2D.cs` | Lógica del géiser volcánico. |
| `Scripts/CrushingCeiling2D.cs` | Lógica del techo aplastante. |
| `Trap_RisingToxicGas_Swamp.prefab`, `Trap_FireGeyser_Volcano.prefab`, `Trap_CrushingCeiling_Caves.prefab` | Trampas dinámicas: prefabs independientes (no variantes) con su script propio; el gas y el techo usan además `HazardZone2D`. |
| `Sprites/Hazard_Placeholder.png` | Cuadrado blanco de 4×4 px (1 × 1 unidad, malla *Full Rect*) que se tiñe con el color de cada trampa. |

Al cambiar algo en `Hazard_Base` se aplica a las nueve variantes; lo que una variante sobrescribe (color, tamaño…) se mantiene.

## Trampas estáticas (9)

| Prefab | Etiqueta | Tamaño por defecto | Color del marcador | Particularidad |
| --- | --- | --- | --- | --- |
| `Trap_Spikes_Jungle` | Pinchos jungla | 3 × 0,5 | Rojo (0,9; 0,2; 0,2) | — |
| `Trap_Briers_Swamp` | Zarzas pantano | 3 × 0,8 | Granate (0,55; 0,15; 0,35) | — |
| `Trap_CrystalSpikes_Caves` | Cristales punzantes | 3 × 0,6 | Cian (0,4; 0,8; 1) | — |
| `Trap_BurningSpikes_Volcano` | Pinchos ardientes | 3 × 0,5 | Naranja (1; 0,5; 0,1) | — |
| `Trap_ToxicMud_Swamp` | Lodo tóxico | 4 × 0,6 | Oliva (0,45; 0,5; 0,15) | — |
| `Trap_ToxicLake_Swamp` | Lago tóxico | 6 × 1,5 | Verde (0,3; 0,75; 0,25) | — |
| `Trap_LavaPool_Volcano` | Foso de lava | 5 × 1,2 | Rojo anaranjado (1; 0,3; 0) | — |
| `Trap_SpikedPillar_Jungle` | Pilar con espinas | 1,6 × 4 | Marrón (0,55; 0,3; 0,2) | Tronco **sólido** de 1 × 4 (se puede pisar su parte superior). Zona letal personalizada de 1,6 × 3,4, desplazada 0,3 hacia abajo: mata al tocar sus lados, pero no al estar de pie encima. |
| `Trap_DeathZone_Universal` | Zona de muerte | 10 × 2 | Magenta translúcido (1; 0; 1; 0,35) | **Invisible en juego** (sin sprite ni etiqueta); solo se ve en el editor. Zona letal del tamaño completo (sin margen). Se coloca bajo los abismos. |

Los tamaños son orientativos: cada instancia del nivel ajusta `Size` a lo que necesite.

## Trampas dinámicas

Usan el mismo marcador (caja de color + etiqueta) y efectos generados por código, de estilo cartoon y con un presupuesto fijo de partículas.

### Gas tóxico ascendente (`Trap_RisingToxicGas_Swamp`)

Volumen verde translúcido (0,35; 0,75; 0,25; 55 %) que sube desde un fondo fijo. El borde superior son nubes suaves en dos tonos de verde que burbujean y suben (3 por unidad de ancho y segundo).

| Fase | Qué pasa |
| --- | --- |
| **Espera** | Quieto a su altura inicial hasta que Alma cruza la línea de activación (o desde el inicio, según `Activation`). |
| **Aviso (3 s)** | El gas parpadea más opaco y las nubes se agitan (×2,5). |
| **Subida** | Sube a 0,9 u/s manteniendo el fondo fijo; mata a Alma en todo su volumen. |
| **Lleno** | Se detiene en `Max Height` (10 u sobre el fondo). |

- Al reaparecer Alma (evento `Respawned`) vuelve a su altura inicial y a la espera.
- `Activate()` y `Stop()` permiten que un encuentro lo arranque o lo detenga (por ejemplo, al rescatar el huevo).
- Tamaño inicial: el `Size` de su `HazardZone2D` (12 × 1 por defecto). El ancho se mantiene; la altura crece.
- Se dibuja delante de Alma (orden 5) para que se vea que está dentro.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Activation` | Al cruzar X | `OnStart` o `WhenAlmaPassesX`. |
| `Activation Offset X` | 0 | Línea de activación relativa a su centro. |
| `Reset On Respawn` | sí | Volver al estado inicial al reaparecer Alma. |
| `Warning Time` | 3 | Aviso (s). |
| `Rise Speed` | 0,9 | Velocidad de subida (u/s). |
| `Max Height` | 10 | Altura máxima desde el fondo. |
| `Puff Color A` / `B` | verdes al 75 % | Colores de las nubes del borde. |
| `Puffs Per Unit` | 3 | Nubes por unidad de ancho y segundo (máx. ~1,6 × ancho × 3 a la vez). |

Gizmos: **verde** = altura máxima; **amarillo** = línea de activación.

### Géiser volcánico (`Trap_FireGeyser_Volcano`)

Respiradero de roca oscura (1,2 × 0,4, no letal: se puede pisar). Escupe una bola de fuego hacia arriba que vuelve a caer en él.

| Fase | Duración | Qué pasa |
| --- | --- | --- |
| **Reposo** | 2,2 s | Nada. |
| **Aviso** | 0,8 s | El respiradero parpadea en naranja y suelta humo oscuro y chispas naranjas hacia arriba. |
| **Erupción** | 1,2 s | La bola sube 4 u y cae (trayectoria parabólica), estirándose según su velocidad, con un núcleo amarillo que palpita y una estela de llamas. Al volver al respiradero salpica 8 gotas de lava. |

- **Solo la bola mata** (círculo de 0,35 u de radio, comprobado cada paso de física).
- `Start Delay` desfasa varios géiseres entre sí.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Rest Time` / `Warning Time` / `Erupt Time` | 2,2 / 0,8 / 1,2 | Ciclo (s). `Erupt Time` es el vuelo completo, subida y bajada. |
| `Start Delay` | 0 | Retraso del primer ciclo (s). |
| `Height` | 4 | Altura máxima de la bola sobre el respiradero. |
| `Fireball Radius` | 0,35 | Radio letal. |
| `Fireball Color` / `Core Color` | naranja / amarillo | Colores de la bola y su núcleo. |
| `Vent Size` | (1,2; 0,4) | Tamaño del respiradero. |
| `Warning Glow` | naranja | Color del parpadeo del aviso. |
| `Label` / `Show Label` | «Géiser» / sí | Etiqueta provisional. |

Partículas: estela (máx. 30), humo (máx. 20), salpicadura (máx. 12). Gizmos: **naranja** = trayectoria y radio letal arriba y abajo.

### Techo aplastante (`Trap_CrushingCeiling_Caves`)

Bloque gris (3 × 1) colocado en el techo. Sólido mientras está arriba. Cae cuando Alma pasa por debajo.

| Fase | Duración | Qué pasa |
| --- | --- | --- |
| **Listo** | — | Espera arriba hasta que Alma entra en la zona de debajo: su ancho más 0,5 u a cada lado, entre el bloque y el suelo. |
| **Aviso** | 0,25 s | Tiembla y suelta polvillo por debajo. Después cae aunque Alma ya se haya movido. |
| **Caída** | hasta tocar algo | Cae acelerando (50 u/s²; una caída de 4 u dura ~0,4 s) hasta tocar el primer colisionador sólido de debajo: el suelo, una plataforma o Alma. Deja de ser sólido mientras cae. Si toca a Alma (por debajo o de lado), la mata. Límite de seguridad: 20 u. |
| **Impacto** | — | Se quiebra justo donde tocó: 14 trozos de roca girando hacia arriba y a los lados, 10 nubes de polvo que ruedan y un temblor de cámara leve (0,1 u, 0,15 s) si Alma está a menos de 10 u. Desaparece. |
| **Oculto** | 2,4 s | Tiempo antes de volver a formarse. |
| **Reaparece** | 0,3 s | Se forma arriba creciendo hacia abajo y apareciendo poco a poco. Vuelve a ser sólido y queda listo. |

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Warning Time` | 0,25 | Temblor antes de caer (s). 0 = cae al instante. |
| `Hidden Time` / `Reform Time` | 2,4 / 0,3 | Tiempo desaparecido y tiempo en reaparecer (s). |
| `Drop Acceleration` | 50 | Aceleración de la caída (u/s²). Más alto, más rápido. |
| `Max Drop Distance` | 20 | Distancia máxima si no encuentra nada debajo. |
| `Detect Margin` | 0,5 | Margen a cada lado del bloque para detectar a Alma. |
| `Dust Color` | beige | Color del polvo. |
| `Shake Amplitude` / `Shake Duration` | 0,1 / 0,15 | Temblor de cámara al impactar. |

- **Zona de detección:** al empezar se mide la distancia hasta el suelo que hay debajo del bloque; la zona termina ahí, para que no caiga si Alma pasa por un piso inferior.
- **Caída sin atravesar nada:** se comprueba con un barrido de la caja del bloque (`BoxCast`) en cada frame. Los contactos que ya existen al empezar (el techo del que cuelga, paredes) se ignoran.
- Diferencia con el diseño original (ciclo fijo: abierto 3 s con aviso de 0,8 s, descenso 0,6 s, cerrado 0,4 s, retirada 2 s): ahora reacciona a Alma y cae más rápido (cambio del 5/10/2026).

Partículas: polvillo de aviso (máx. 20), trozos (máx. 20), nube (máx. 16). Gizmos: **naranja** = caída máxima; **amarillo** = zona de detección (en juego termina en el suelo).

## Comportamiento

- **Muerte:** la zona letal es un trigger. Cuando el colisionador de Alma entra o permanece en ella (`OnTriggerEnter2D` / `OnTriggerStay2D`) se llama a `AlmaMotor2D.Die()`, salvo que ya esté muerta. La secuencia de muerte y la reaparición las gestiona Alma.
- **Tamaño:** `Size` fija a la vez el sprite y la zona letal. El sprite está en modo *Tiled*, así que se **repite** en lugar de estirarse: con arte real, un tramo de pinchos de 3 o de 12 unidades usa el mismo prefab.
- **Zona letal:** por defecto es `Size` reducido `Hitbox Inset` (0,05 u) por cada lado, para que rozar el borde del dibujo no mate. Con `Custom Hitbox` se define a mano (desplazamiento y tamaño), independiente del dibujo.
- **Parte sólida (opcional):** con `Solid` se activa un segundo colisionador no trigger, con su propio desplazamiento y tamaño, sobre el que Alma puede apoyarse o chocar.
- **Activo:** `IsActive` (o `Active` en el Inspector) enciende o apaga la zona letal sin cambiar lo que se ve. Lo usarán las trampas con ciclos (géiser, chorro de fuego, puerta de llamas…).
- **Visible en juego:** si se desactiva, el sprite y la etiqueta se ocultan al empezar; la zona sigue matando.
- **Etiqueta provisional:** al empezar la partida se crea un texto pequeño y blanco (`TextMesh` con la fuente integrada de Unity) 0,3 u por encima de la zona, con el nombre de la trampa. En el editor el nombre aparece junto a los gizmos. Se quita desactivando `Show Label` cuando haya arte.
- **Cambio de tamaño en juego:** `SetSize()` redimensiona sprite, colisionadores y etiqueta (lo usa el gas). `SolidEnabled` activa o desactiva la parte sólida (lo usa el techo).
- **Sincronización en el editor:** al cambiar los valores en el Inspector (`OnValidate`), el tamaño del sprite y de los colisionadores se actualiza al momento.

## Valores (`HazardZone2D`)

| Campo | Base | Uso |
| --- | ---: | --- |
| **Área** | | |
| `Size` | (3; 0,5) | Tamaño del sprite y base de la zona letal. |
| `Hitbox Inset` | 0,05 | Margen que se resta a cada lado de la zona letal. |
| `Custom Hitbox` | no | Usar `Hitbox Offset` y `Hitbox Size` en lugar del margen. |
| `Hitbox Offset` / `Hitbox Size` | (0; 0) / (1; 1) | Zona letal personalizada. |
| **Parte sólida** | | |
| `Solid` | no | Activa el colisionador sólido. |
| `Solid Offset` / `Solid Size` | (0; 0) / (1; 1) | Posición y tamaño de la parte sólida. |
| **Comportamiento** | | |
| `Active` | sí | Zona letal encendida. |
| `Visible In Game` | sí | Mostrar sprite y etiqueta al jugar. |
| **Etiqueta** | | |
| `Label` | «Peligro» | Texto provisional. |
| `Show Label` | sí | Mostrar la etiqueta al jugar. |
| **Referencias** | | |
| `Lethal Collider` / `Solid Collider` | (del prefab) | Los dos `BoxCollider2D` del prefab base. No cambiar. |

## Cómo usarlas

1. Arrastra la variante que necesites (no `Hazard_Base`) a la escena.
2. Ajusta `Size` en el componente **Hazard Zone 2D** (no la escala del Transform, para que el arte se repita en lugar de estirarse).
3. Comprueba los gizmos: **rojo** = zona letal; **gris** = parte sólida.
4. Para la zona de caída mortal, colócala bajo cada abismo, cubriendo todo su ancho.

**Cuando llegue el arte:** en la variante, asigna el sprite al SpriteRenderer, pon el color en blanco, desactiva `Show Label` y ajusta `Hitbox Inset` o la zona personalizada al dibujo. El sprite debe importarse con malla *Full Rect* para que el modo *Tiled* funcione.

## Gizmos

- **Rojo:** zona letal (también en la zona de caída mortal, aunque sea invisible).
- **Gris:** parte sólida, si la tiene.
- **Nombre:** la etiqueta de la trampa, en la vista Scene.

## Coste

Por trampa: un SpriteRenderer, uno o dos `BoxCollider2D` estáticos y, al jugar, un `TextMesh`. Sin consultas físicas por frame: la detección la hace el motor de físicas con el trigger.

## Pendiente

- Arte de cada trampa.
- Colocarlas en niveles y probarlas.
- **Fuera de alcance (decisión del 5/10/2026):** erupción de lava ascendente, magma ascendente del jefe final, chorro de fuego de aterrizaje y puerta de llamas no se harán. El juego usa las doce trampas de esta ficha.
- Gas: la regla del diseño de «quedar 4 u por debajo del checkpoint» al reaparecer no está implementada; hoy vuelve a su altura inicial.
- Géiser y techo no se reinician al reaparecer Alma (siguen su ciclo).
- El inventario prevé `Assets/_Project/Prefabs/Traps/`; están en `Assets/Prefabs/Level/Hazards/`.
