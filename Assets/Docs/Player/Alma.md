# Alma — diseño del personaje jugable

**Estado (6 de octubre de 2026):** Alma está implementada en Unity 6000.6.0f1 con locomoción, salto variable, doble salto, Pisotón, Dash aéreo y Rugido. Las siete hojas de animación (Idle, Run, Jump, Fall, GroundPound, Dash y Roar) están conectadas en un único Animator. Correr, Pisotón, Dash, Rugido y muerte/reaparición tienen efectos visuales generados por código. Las habilidades se desbloquean con altares y los nidos de checkpoint fijan dónde reaparece, con guardado en JSON. Faltan los receptores del Rugido, mando, controles táctiles y el clip propio de doble salto (ver [Pendiente](#pendiente)).

## Implementación actual y prueba

Abre `Assets/Scenes/World_01/Level_1_1.unity`, pulsa Play y enfoca la pestaña Game.

| Acción | Teclado implementado | Dónde se lee |
| --- | --- | --- |
| Moverse | A/D o flechas | Eje `Horizontal` del Input Manager. |
| Salto / doble salto | Espacio (en el suelo / en el aire) | Botón `Jump` del Input Manager. |
| Pisotón | S, flecha abajo o C, en el aire | Eje `Vertical` < -0,5 (solo al pulsar) o `KeyCode.C`. |
| Dash aéreo | Shift izquierdo o derecho, en el aire | `KeyCode.LeftShift` / `RightShift`. |
| Rugido | E o F | `KeyCode.E` / `KeyCode.F`. |

El prefab está en `Assets/Prefabs/Player/Alma.prefab`. Todos sus archivos de funcionamiento están en la misma carpeta de personaje:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `Scripts/AlmaInput.cs` | Captura entradas y las entrega al motor (`SetInput`, `RequestGroundPound`, `RequestDash`, `RequestRoar`). |
| `Scripts/AlmaMotor2D.cs` | Aceleración, frenado, control aéreo, salto variable, doble salto, Pisotón, Dash, Rugido, detección de suelo y reaparición. |
| `Scripts/AlmaAnimation.cs` | Orientación del sprite y parámetros del Animator según el estado real del motor. |
| `Scripts/AlmaRunDust.cs` | Polvo en los pies al correr: un `ParticleSystem` creado en tiempo de ejecución (máx. 20 partículas, una llamada de dibujo, textura generada por código). |
| `Scripts/AlmaGroundPoundFx.cs` | Efectos del impacto del Pisotón: onda en el suelo, ráfaga de polvo y temblor de cámara. |
| `Scripts/AlmaDashFx.cs` | Efectos del Dash: siluetas fantasma y líneas de viento. |
| `Scripts/AlmaRoarFx.cs` | Efectos del Rugido: ondas de sonido en arco, polvo empujado y temblor de cámara. |
| `Scripts/AlmaDeathFx.cs` | Secuencia de muerte y reaparición (golpe, «puf», luz que viaja al punto de reaparición y reaparición con rebote). |
| `Scripts/AlmaDoubleJumpFx.cs` | Efectos del doble salto: anillo de aire, bocanadas y estirón del sprite. |
| `Scripts/AlmaFxRoot.cs` | Agrupa en un único objeto de escena, «Alma FX», los efectos que deben quedarse fijos en el mundo (ondas, anillos, fantasmas, arcos y ráfagas). Se crea en tiempo de ejecución y se destruye con Alma. |
| `Scripts/IRoarTarget.cs` | Contrato para objetos que reaccionan al Rugido (`ResonatesWithRoar`, `ReceiveRoar(origin, direction)`). |
| `Scripts/AlmaMovementSettings.cs` | ScriptableObject con todos los valores de movimiento y habilidades. |
| `Scripts/AlmaCameraFollow.cs` | Seguimiento suave, anticipación horizontal según velocidad y `Shake(amplitud, duración)` para temblores breves. |
| `Configuration/AlmaMovement.asset` | Instancia de `AlmaMovementSettings` usada por el prefab. Los valores se editan aquí, no en el script. |
| `Configuration/AlmaFrictionless.physicsMaterial2D` | Evita adherirse a paredes. |
| `Animations/` | Hojas de sprites, clips y el controlador `Idle/Player_Idle_Sheet_0.controller`. |
| `Tests/PlayMode/` | Ocho pruebas de movimiento, salto, doble salto, animación, paredes y reaparición. |

No hay scripts de editor que regeneren el prefab o el Animator: toda la configuración se mantiene a mano en los assets.

El cuerpo usa Rigidbody2D con interpolación, colisión continua, rotación bloqueada y una cápsula estable alrededor del cuerpo. La cola no engancha los bordes. El salto admite 0,14 s después de abandonar un borde y recuerda una pulsación durante 0,12 s antes de aterrizar. Mantener el botón no provoca saltos repetidos. Las pulsaciones de habilidades se guardan hasta el siguiente paso de física y se consumen una sola vez; si no se cumplen las condiciones en ese paso, se descartan.

### Comportamiento implementado de cada habilidad

- **Doble salto.** Pulsar salto en el aire con la carga disponible fija la velocidad vertical en `max(actual, 7,6 m/s)`, de modo que nunca debilita un rebote más fuerte. Hay una carga por estancia en el aire y se recupera al tocar suelo o al reaparecer. Funciona también al caer de un borde sin haber saltado. No se puede usar durante el Dash ni el Pisotón. Mantener o soltar salto controla la altura igual que en el salto normal. Si se pulsa justo antes de aterrizar con la carga disponible, se gasta el doble salto en lugar de guardar un salto para el suelo. Se desbloquea con su [altar](../LevelPieces/Progression.md) cuando hay `System_GameProgress` en la escena (ver «Habilidades y checkpoints» más abajo).
- **Pisotón.** Solo se inicia en el aire y fuera de un Dash. Durante 0,1 s de preparación Alma queda inmóvil (gravedad 0). Después cae en vertical a 22 m/s constantes, con el movimiento horizontal bloqueado e ignorando el límite normal de 20 m/s. Termina cuando la detección de suelo existente confirma el impacto; en ese paso se restaura la gravedad normal. Invalida el coyote time para que no se consuma un salto a mitad de caída.
- **Dash aéreo.** Solo en el aire, con carga y fuera de un Pisotón. Recorre 6 m en 0,45 s (≈13,3 m/s) en la orientación de Alma al comenzar, sin gravedad y con velocidad vertical 0. La orientación queda fijada durante el impulso. Al terminar, la velocidad horizontal se limita a 7 m/s para que no recorra distancia extra. Hay una carga aérea, recuperada al tocar suelo (o con la espora o el hongo), y 0,4 s de espera tras cada Dash; si la carga se recuperó durante el Dash (por ejemplo, tocando una espora), no hay espera y el siguiente Dash sale enseguida. No concede inmunidad.
- **Rugido.** Se puede usar en suelo y en el aire, fuera del Dash y del Pisotón. Dura 0,67 s, lo mismo que su animación. Durante ese tiempo fija la orientación y no permite otro Rugido. En el suelo Alma se frena y no puede caminar hasta que termina; en el aire conserva el control horizontal (Rugido aéreo). Saltar sí está permitido. Al iniciarse lanza un único `Physics2D.OverlapCircle` (incluye triggers) desde el centro del cuerpo. Cada objeto con `IRoarTarget` cuyo punto más cercano esté dentro del cono frontal (45° de semiancho) recibe `ReceiveRoar` una sola vez por Rugido. El alcance es de 3 m, o de 8 m si el objeto declara `ResonatesWithRoar` (campanas). No tiene cooldown aparte de su duración.

Todas las acciones se cancelan al reaparecer o al desactivar el componente.

- **Muerte.** `Die()` es el punto de entrada para peligros y caídas (hoy la caída por debajo de Y = −12, la [planta carnívora](../Enemies/Plant_Carnivorous_Jungle.md) el [escarabajo de cristal](../Enemies/CrystalBeetle_Caves.md), el [murciélago de cueva](../Enemies/CaveBat_Caves.md) y las [zonas de peligro](../LevelPieces/Hazards.md)). Activa `IsDead`, cancela habilidades, desactiva la física del cuerpo (`Rigidbody2D.simulated = false`) e ignora la entrada, y lanza el evento `Died`. El control vuelve con `Respawn()`, que reactiva la física y coloca a Alma en `RespawnPosition` (la posición inicial o el último [nido de checkpoint](../LevelPieces/Progression.md) tocado). Si nadie escucha `Died`, reaparece al instante. `Respawn()` lanza al final el evento `Respawned`, que usan las piezas de nivel para reiniciarse.
- **Rebote y recargas (piezas de nivel).** `Bounce(speed, heldMultiplier, refillAirAbilities)` lanza a Alma hacia arriba en el siguiente paso de física (lo usa el [hongo saltarín](../LevelPieces/Pieces.md#hongo-saltarín-resource_bouncymushroom_jungle)): fija la velocidad vertical en `max(actual, speed × multiplicador si mantiene el salto)`, cancela un Pisotón en curso y no aplica la gravedad de salto soltado mientras sube por el rebote. `RefillAirAbilities(dash, doubleJump)` recarga el Dash (quitando su espera de 0,4 s) y el doble salto, y devuelve si recargó algo (lo usa la [espora](../LevelPieces/Pieces.md#espora-de-recarga-del-dash-resource_dashrefillspore_swamp)). `Respawned` lo escuchan el gas tóxico ascendente, las plataformas que se desmoronan, el piso rompible y la espora para restaurarse.
- **Habilidades y checkpoints (progresión).** Doble salto, Pisotón, Dash y Rugido tienen cada uno un interruptor (`_doubleJumpUnlocked`, `_groundPoundUnlocked`, `_dashUnlocked`, `_roarUnlocked`), consultable y modificable con `IsUnlocked(AlmaAbility)` y `SetUnlocked(AlmaAbility, bool)`; una habilidad bloqueada simplemente no se activa (la espora tampoco recarga un Dash bloqueado). Por defecto los cuatro están activados, para poder probar el prefab en cualquier escena; con [`System_GameProgress`](../Systems/SaveAndProgress.md) en la escena, solo quedan los guardados y los iniciales del nivel, y se desbloquean con los [altares](../LevelPieces/Progression.md). `SetRespawnPosition(Vector2)` cambia el punto de reaparición (lo usan los nidos y la carga de partida).

**Polvo al correr.** `AlmaRunDust` emite 2,5 partículas por metro recorrido mientras Alma está en el suelo y supera el 50 % de `MoveSpeed`. Las partículas (0,35–0,6 unidades) salen a la altura de los pies, 0,35 unidades por detrás del centro de Alma, derivan hacia atrás y un poco hacia arriba, crecen y se desvanecen en 0,4–0,6 s. Color beige (0,9; 0,84; 0,72) con 75 % de opacidad, dibujadas detrás de Alma. Se ajusta en el Inspector del componente (`Min Speed Ratio`, `Puffs Per Meter`, `Size Range`, `Color`, `Back Offset`). Al dejar de correr se dejan de emitir partículas y las existentes terminan solas.

**Doble salto.** El motor lanza `DoubleJumped` en el paso de física del impulso y `AlmaDoubleJumpFx` responde con:

- **Anillo de aire:** el anillo compartido del Pisotón, más pequeño (de 0,4 a 1,8 unidades), aplanado al 35 %, blanco-azulado al 85 %, que se abre en 0,25 s bajo los pies y queda fijo en el aire.
- **Bocanadas:** 5 partículas que salen hacia abajo y a los lados (semicírculo inferior) a 1–2 m/s, de 0,2–0,35 unidades, y se desvanecen en 0,25–0,4 s.
- **Estirón:** Alma pasa a 90 % de ancho × 110 % de alto y vuelve a su tamaño en 0,12 s. Escala el objeto completo, colisionador incluido; por ser tan breve y pequeño no afecta al juego. Se cancela si Alma muere.

Sin temblor de cámara. Coste: un sprite y un sistema de partículas (máx. 8). Todo se ajusta en el Inspector del componente. Esto distingue visualmente el doble salto mientras no haya un clip propio.

**Impacto del Pisotón.** El motor lanza el evento `GroundPoundLanded` en el paso de física en que confirma el impacto. `AlmaGroundPoundFx` responde con:

- **Onda:** anillo suave generado por código que se expande desde los pies, de 0,6 a 4 unidades de ancho, aplanado al 30 % de alto, en 0,35 s, con salida suavizada y desvaneciéndose. Queda fija donde Alma aterrizó y se dibuja delante de ella.
- **Polvo:** 10 partículas en abanico hacia los lados y arriba (semicírculo superior), a 1,5–3 m/s, de 0,3–0,55 unidades, con algo de gravedad; duran 0,35–0,5 s.
- **Temblor de cámara:** desplazamiento aleatorio de hasta 0,12 unidades que se reduce linealmente a cero en 0,18 s. No altera el seguimiento suave de la cámara.

Todo se ajusta en el Inspector del componente. Coste: un sprite y un sistema de partículas (máx. 16), visibles solo durante el efecto.

**Estela del Dash.** `AlmaDashFx` actúa mientras `IsDashing` es verdadero:

- **Siluetas fantasma:** al empezar el Dash y cada 0,09 s se deja una copia del frame actual de Alma, quieta donde estaba, tintada de azul claro (0,75; 0,9; 1) al 55 % de opacidad, que se desvanece en 0,25 s. Se reutilizan 5 sprites (en un Dash de 0,45 s salen unas 5) y se dibujan detrás de Alma.
- **Líneas de viento:** 16 trazos por segundo (máx. 8 a la vez), finos (0,04–0,08) y alargados (0,8–1,5 unidades), blanco-azulados al 70 %, que aparecen a lo largo de la altura del cuerpo, 0,6 unidades por detrás de Alma, se quedan atrás a 1,5 m/s y se desvanecen en 0,18–0,28 s.

Todo se ajusta en el Inspector del componente. Coste: hasta 5 sprites y un sistema de partículas (máx. 8), sin dibujar nada fuera del Dash.

**Efecto del Rugido.** `AlmaRoarFx` se activa al empezar cada Rugido:

- **Ondas en arco:** tres arcos, separados 0,07 s, salen de la boca (0,9; 0,4 unidades desde el centro, invertido según la orientación) y se abren desde 0,2 m hasta `RoarRange` (3 m) en 0,35 s, con salida suavizada y desvaneciéndose. Su apertura usa `RoarHalfAngle` (45°), así que muestran el alcance real del cono. Color blanco cálido al 80 %, delante de Alma, y quedan fijos donde salieron.
- **Polvo empujado (solo en el suelo):** 8 partículas delante de los pies que salen hacia delante a 2–4 m/s y algo hacia arriba, y se desvanecen en 0,4–0,6 s.
- **Temblor de cámara:** hasta 0,07 unidades durante 0,2 s, más suave que el del Pisotón.

Todo se ajusta en el Inspector del componente. Coste: 3 sprites y un sistema de partículas (máx. 10), sin dibujar nada entre rugidos.

**Muerte y reaparición.** `AlmaDeathFx` escucha `Died` y reproduce, sin sprites nuevos (≈1,1–1,6 s según la distancia):

1. **Congelación (0,08 s):** el Animator se detiene y la cámara tiembla (0,15 unidades, 0,2 s).
2. **Golpe (0,15 s):** Alma se tinta de rojo claro (1; 0,55; 0,55) y se aplasta a 120 % × 75 %.
3. **«Puf» (0,12 s):** se estrecha hasta desaparecer y estallan 12 nubes de polvo y 5 estrellitas doradas.
4. **Luz (0,45–1 s):** una bolita de luz cálida con estela vuela en curva hasta `RespawnPosition` a ~14 m/s. Mueve el transform de Alma, así que la cámara la sigue. Si Alma cayó por un hueco, la luz sale desde el borde inferior de la pantalla.
5. **Reaparición (0,22 s):** anillo de luz, 3 estrellitas y Alma crece de 0 a 115 % y vuelve a 100 %. Al terminar se llama a `Respawn()`.

Las texturas (estrella, luz, polvo y anillo) se generan por código y se comparten con los otros efectos. Coste: 3 sprites y 3 sistemas de partículas pequeños, visibles solo durante la secuencia. Todo se ajusta en el Inspector del componente. La resonancia de 8 m todavía no tiene efecto visual propio; se añadirá con los receptores.

### Valores en `AlmaMovement.asset`

Todos los campos están guardados explícitamente en el asset. Los valores por defecto del script solo se aplican a assets nuevos. Para ajustar el juego, editar `Configuration/AlmaMovement.asset` en el Inspector.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `MoveSpeed` | 7 | Velocidad horizontal máxima (m/s). |
| `AccelerationTime` / `BrakingTime` | 0,10 / 0,08 | Tiempo para alcanzar o perder la velocidad en suelo (s). |
| `AirAccelerationTime` | 0,13 | Control aéreo (s). |
| `JumpSpeed` / `DoubleJumpSpeed` | 8,2 / 7,6 | Impulso vertical (m/s). |
| `GravityScale` | 2,2 | Multiplicador de la gravedad de Unity (9,81). |
| `FallGravityMultiplier` / `ReleasedJumpGravityMultiplier` | 1,8 / 2,4 | Gravedad al caer / al soltar salto. |
| `MaxFallSpeed` | 20 | Velocidad máxima de caída normal (m/s). |
| `CoyoteTime` / `JumpBufferTime` | 0,14 / 0,12 | Ventanas de salto (s). |
| `GroundPoundWindupTime` / `GroundPoundSpeed` | 0,1 / 22 | Preparación (s) y descenso (m/s). |
| `DashDistance` / `DashDuration` / `DashCooldown` | 6 / 0,45 / 0,4 | Metros, segundos y espera tras el Dash. |
| `RoarDuration` | 0,6667 | Duración del Rugido (s): bloquea caminar en el suelo, la orientación y otro Rugido. Igual a la del clip. |
| `RoarRange` / `RoarResonanceRange` / `RoarHalfAngle` | 3 / 8 / 45 | Alcance normal, alcance de resonancia (m) y semiancho del cono (°). |
| `GroundLayers` / `GroundProbeDistance` / `MinimumGroundNormal` | Todo / 0,04 / 0,65 | Detección de suelo. |

En el motor, `_fallRespawnY = -12` define la altura bajo la cual Alma muere (y reaparece tras la secuencia de muerte).

### Animator

El prefab usa `Animations/Idle/Player_Idle_Sheet_0.controller`. Todas las transiciones salen de **Any State**, son inmediatas (sin Exit Time, duración 0) y no pueden volver al mismo estado. `AlmaAnimation` actualiza los parámetros en cada frame a partir del motor.

| Parámetro | Tipo | Origen |
| --- | --- | --- |
| `Speed` | Float | `|velocidad.x|` |
| `VerticalSpeed` | Float | `velocidad.y` |
| `RunRate` | Float | `Speed / MoveSpeed`, limitado a 0,5–1,2 |
| `Grounded` | Bool | `IsGrounded` |
| `GroundPound` | Bool | `IsGroundPounding`, o el clip todavía terminando tras el impacto mientras Alma sigue en el suelo |
| `Dash` | Bool | `IsDashing` |
| `Roar` | Bool | `IsRoaring`, o el clip todavía terminando (si no hay Dash ni Pisotón) |

| Prioridad | Estado | Clip | Condiciones de entrada |
| ---: | --- | --- | --- |
| 1 | Dash | `Player_Dash_Animation` (velocidad ×1,48, sin bucle) | `Dash` |
| 2 | GroundPound | `Player_GrounPound_Animation` | `GroundPound` |
| 3 | Roar | `Player_Roar_Animation` (sin bucle) | `Roar`, sin `Dash` ni `GroundPound` |
| 4 | Idle (por defecto) | `Player_Idle_Animation` | sin habilidades, `Grounded`, `Speed` < 0,2 |
| 5 | Run | `Player_Run_Animation` (velocidad por `RunRate`) | sin habilidades, `Grounded`, `Speed` > 0,2 |
| 6 | Jump | `Animations/Jump/Player_Jump_Animation` (4 frames, 0,33 s, sin bucle) | sin habilidades, no `Grounded`, `VerticalSpeed` > 0 |
| 7 | Fall | `Animations/Fall/Player_Fall_Animation` (2 frames, 0,17 s, sin bucle) | sin habilidades, no `Grounded`, `VerticalSpeed` < 0,001 |

«Sin habilidades» significa `Roar`, `Dash` y `GroundPound` en false. Idle, Run, GroundPound, Dash y Roar tienen 8 frames a 12 fps (0,67 s). Jump y Fall tienen clips propios a 12 fps, sin bucle: cada uno se reproduce una vez al entrar en su estado y se queda en su último frame mientras dura la subida o la caída. Secuencia comprobada en un salto normal (prueba PlayMode del 6/10/2026 en `Level_1_1`): Jump 1→4 entre 0 y 0,29 s, se queda en el 4 hasta el punto más alto (~0,37 s); Fall 1→2 desde ~0,41 s, se queda en el 2 hasta tocar el suelo (~0,65 s); Idle al aterrizar. El Dash se reproduce a ×1,48 para que el clip dure exactamente sus 0,45 s; si se cambia `DashDuration`, ajustar esta velocidad a 0,667 / `DashDuration`. El Pisotón termina en la lógica antes que su clip; `AlmaAnimation` mantiene su estado visual hasta completar una reproducción, sin retrasar el control. El Rugido ya dura lo mismo que su clip (`RoarDuration` = 0,667), así que esa retención solo actúa como red de seguridad si se acorta `RoarDuration`. En un Pisotón de más de 0,67 s el clip se repite en la caída y al aterrizar se corta donde esté. El doble salto reutiliza el estado Jump: si ocurre durante Fall, Jump empieza desde su primer frame; si Alma aún sube, la animación continúa.

**Si un estado se ve congelado** (por ejemplo, Alma salta con la pose de Idle o del Dash): ese estado no tiene clip asignado. Con el editor abierto, Unity puede volver a guardar el controlador desde una copia vieja en memoria; el 5/10/2026 guardó Jump y Fall con *Motion: None*. Solución: cerrar la ventana del Animator, seleccionar el estado en `Player_Idle_Sheet_0.controller` y comprobar su **Motion** en el Inspector (Jump → `Player_Jump_Animation`, Fall → `Player_Fall_Animation`); si sale *None*, arrastrar el clip y guardar.

### Pruebas y escena

La escena incluye un suelo y tres plataformas de práctica con desniveles alcanzables. Si Alma cae por debajo de Y = -12, muere y reaparece en su posición inicial tras la secuencia de muerte.

Las ocho pruebas se ejecutan en Test Runner → PlayMode → `AlmaMovementTests`. Cubren carrera/frenado/orientación, estados animados, salto largo y corto, buffer al aterrizar, coyote time, doble salto (una sola vez por estancia en el aire), paredes y reaparición. Las pruebas de movimiento base desactivan el doble salto en su preparación. Pisotón, Dash y Rugido todavía no tienen pruebas automáticas.

### Pendiente

- Inmunidad del Dash al viento (todavía no hay viento).
- Receptores del Rugido (rocas, campanas, interruptores, enemigos) implementando `IRoarTarget`, efecto visual de la resonancia (8 m) al alcanzar una campana.
- Rotura de suelos y activación de mecanismos con el Pisotón (pueden suscribirse a `GroundPoundLanded`, como ya hace el [escarabajo de cristal](../Enemies/CrystalBeetle_Caves.md) para voltearse).
- Mando y controles táctiles.
- Clip propio de DoubleJump (Dead/Respawn se resolvió por código). Indicador diegético de Dash disponible (plumas del lomo, requiere arte).
- Tamaño de cámara: el código usa 8 y el diseño 6 (ver [Cámara](#cámara-daño-y-feedback)).
- El resto de peligros (deben llamar a `Die()`, como ya hacen los enemigos y las zonas de peligro estáticas). Un destello blanco puro al morir necesitaría un shader propio; hoy se usa un tinte rojo claro.

Alma es una madre dinosaurio ágil. Su control debe permitir saltos precisos y encadenar habilidades sin retrasos artificiales. No tiene puntos de vida: al tocar un peligro activo reaparece en el último checkpoint. El juego no usa música ni efectos de sonido; cada acción necesita señales visuales claras.

## Controles y habilidades

| Acción | Teclado de referencia | Regla | Estado |
| --- | --- | --- | --- |
| Moverse | A/D o flechas | Aceleración y frenado breves, con control aéreo. | Implementado |
| Salto | Espacio | Altura variable al mantener o soltar; coyote time y buffer de entrada. | Implementado |
| Doble salto | Espacio en el aire | Un segundo impulso, recuperado al aterrizar o tocar un recurso que lo recargue. | Implementado (recarga también con el hongo y la espora) |
| Pisotón | S, abajo o C en el aire | Breve preparación y descenso vertical rápido; rompe suelos y activa mecanismos. | Movimiento implementado; faltan efectos sobre el entorno |
| Dash aéreo | Shift en el aire | Impulso horizontal en la dirección fijada al comenzar; una carga aérea y recarga al aterrizar o tocar una espora. | Implementado (recarga también con la espora y el hongo); falta el viento |
| Rugido | E o F | Cono frontal que empuja objetos y activa objetivos compatibles. | Detección implementada; faltan receptores |

El mando y los controles táctiles deben ofrecer las mismas acciones con iconos y estados visibles. El Dash no concede inmunidad a enemigos, pinchos, veneno ni lava; durante el impulso ignora el viento. Ninguna habilidad debe sustituir el botón de otra.

Las habilidades se incorporan de forma acumulativa: Doble Salto en el mundo 1, Pisotón en el 2, Dash en el 3 y Rugido en el 4. Los altares del [inventario](../INVENTARIO_GAMEPLAY_PREFABS.md) son las piezas previstas para desbloquearlas. La progresión exacta de cada escena se define en su ficha de nivel.

## Física de referencia

Una unidad de juego equivale aproximadamente a un metro. Mantener la misma física en los cuatro mundos y ajustar el diseño de plataformas a ella.

Todos estos valores están aplicados en `AlmaMovement.asset` (ver [Valores](#valores-en-almamovementasset)).

| Parámetro | Objetivo inicial |
| --- | ---: |
| Velocidad horizontal máxima | 7 m/s |
| Aceleración / frenado | 0,10 / 0,08 s |
| Aceleración aérea | 0,13 s |
| Gravedad base | 9,81 m/s² × 2,2 |
| Impulso de salto / doble salto | 8,2 / 7,6 m/s |
| Gravedad al caer / al soltar salto | ×1,8 / ×2,4 |
| Velocidad máxima de caída normal | 20 m/s |
| Coyote time / buffer de salto | 0,14 / 0,12 s |
| Dash | 6 m en 0,45 s (el diseño original indicaba 0,2 s; se alargó el 5/10/2026 porque a 30 m/s ni la animación ni el desplazamiento se apreciaban); recarga de 0,4 s |
| Pisotón | 0,1 s de preparación; 22 m/s de descenso |
| Rugido | 0,67 s, igual que su animación (el diseño original indicaba 0,25 s; se alargó el 5/10/2026 porque caminar durante la animación se veía raro); cono frontal de 3 m y 45° de semiancho |
| Resonancia de campanas | Hasta 8 m dentro del cono frontal |

El doble salto debe elevar la velocidad vertical al menos a 7,6 m/s sin anular un rebote que ya sea más fuerte. Las pulsaciones se capturan entre pasos de física y se consumen una sola vez. El hongo saltarín del inventario parte de 17 m/s; mantener salto permite un rebote de ×1,18. Estos valores requieren pruebas jugables, especialmente junto a superficies móviles y plataformas altas.

**Alcances estimados en plano**, con salto mantenido y salida a velocidad máxima: salto simple ≈1,56 m de altura y 4,64 m de recorrido; doble salto cerca del ápice ≈2,89 m de altura total y 7,8 m de recorrido. Son estimaciones del centro del personaje, no garantías de aterrizaje. Cada plataforma debe ofrecer margen para el collider completo, aceleración, variación de salto y errores normales del jugador. Un salto obligatorio no debe depender del coyote time ni de tocar un borde exacto. Las superficies necesarias para pisotones y contraataques deben quedar dentro del alcance real del doble salto.

## Estados y prioridad de acciones

Estados previstos: Idle, Run, Jump, DoubleJump, Fall, GroundPound, Dash, Roar y Dead/Respawn. Implementados en el Animator: Idle, Run, Jump, Fall, GroundPound, Dash y Roar (el doble salto usa Jump). Walk puede ser una variación visual de locomoción, sin necesitar otro estado de control. Las transiciones se resuelven por entradas y condiciones físicas; la animación responde al estado, nunca retrasa el movimiento.

El Dash mantiene su dirección durante todo el impulso. El Pisotón requiere estar en el aire y debe terminar al impactar. El Rugido toma la orientación actual de Alma. En la implementación, Dash y Pisotón son excluyentes entre sí, y el Rugido no puede iniciarse durante ninguno de los dos. Al aterrizar se recuperan salto y Dash; las esporas pueden recargarlos en el aire. Los checkpoints deben restablecer un estado seguro y no permitir reaparecer dentro de un peligro activo.

## Animaciones y arte

Las [láminas de diseño y poses](../../Art/Player/) son referencias conceptuales. La implementación actual usa siete hojas de 1024×512 con frames de 256×256 y 80 píxeles por unidad, en `Assets/Prefabs/Player/Animations/`: Idle, Run, GroundPound, Dash y Roar (8 frames), Jump (4) y Fall (2). Jump (`Animations/Jump/Player_Jump_Sheet.png`, 4 frames: agachada, erguida y dos en el aire con las patas recogidas) y Fall (`Animations/Fall/Player_Fall_Sheet.png`, 2 frames bajando con las patas estiradas) son hojas separadas desde el 5/10/2026; sus sprites se llaman `Player_Jump_Sheet_v2_0`–`_3` y `Player_Jump_Sheet_v2_4`–`_5`. Siguen pendientes DoubleJump y Rescue; Dead/Respawn se hace por código con el sprite existente (`AlmaDeathFx`). Las hojas nuevas deben mantener ese mismo formato.

| Acción visual | Requisito para la nueva producción |
| --- | --- |
| Idle y Run | Silueta clara; transición inmediata al cambiar de movimiento. |
| Jump, DoubleJump y Fall | Ascenso, segundo impulso y descenso distinguibles; no reutilizar la misma pose para los tres momentos finales. |
| GroundPound | Anticipación, caída y golpe visibles sin alargar la ventana de control. |
| Dash | Pose y estela que muestren dirección y carga disponible. |
| Roar | Apertura, onda frontal y recuperación; diferenciar alcance normal y resonancia. |
| Dead/Respawn | Breve lectura de fallo y retorno al checkpoint. |
| Rescue | Interacción propia con cada huevo, sin bloquear el control más de lo necesario. |

Conservar tamaño de lienzo, pivote, escala y línea de apoyo coherentes entre frames. Separar anticipación, acción y recuperación. El sprite de Alma y sus animaciones quedan editables en su carpeta de personaje; los dibujos conceptuales permanecen en `Assets/Art/Player/`.

## Cámara, daño y feedback

> **Discrepancia pendiente:** `AlmaCameraFollow.Awake` fuerza hoy `orthographicSize = 8`, aunque el diseño y la escena indican 6. Decidir el valor y alinear código y documento.

La cámara de **todos los niveles y jefes** tendrá tamaño ortográfico 6, seguirá a Alma con suavizado y mostrará aproximadamente 1,25 unidades adicionales hacia la dirección de desplazamiento. Al detenerse, el encuadre vuelve al centro. Cada nivel puede limitar el recorrido de cámara para evitar enseñar zonas fuera del mapa, sin cambiar el zoom.

Todo contacto con un peligro activo es letal y produce reaparición; no hay barra de HP. Una ventana segura de un enemigo o trampa debe indicarse visualmente. La disponibilidad del Dash, los avisos de ataque, los temporizadores, los checkpoints y los rescates también deben entenderse sin sonido y sin depender solo del color.

## Integración futura

El archivo actual es `Assets/Prefabs/Player/Alma.prefab`, siguiendo la estructura creada para esta reconstrucción. Su controlador, configuración de física, sprites y animaciones están agrupados con él. Cada escena tendrá una sola instancia de Alma, y sus mecanismos enlazarán esa instancia y la cámara. Antes de construir niveles completos, verificar entradas de teclado, mando y táctil, alcance de salto, Dash, Pisotón, Rugido, muerte/reaparición y cámara con tamaño 6.
