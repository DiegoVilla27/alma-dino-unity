# Alma — diseño del personaje jugable

**Estado:** locomoción base implementada en Unity 6000.6.0f1. El prefab existente y las hojas Idle, Run y Jump ya tienen movimiento, salto simple y transiciones físicas. Las habilidades avanzadas de este documento siguen siendo especificación para las siguientes fases.

## Implementación actual y prueba

Abre `Assets/Scenes/World_01/Level_1_1.unity`, pulsa Play y enfoca la pestaña Game. Usa **A/D o flechas** para correr y **Espacio** para saltar. Mantener Espacio aumenta la altura; soltarlo corta el ascenso. La entrada conserva los ejes `Horizontal` y `Jump` del Input Manager existente.

El prefab está en `Assets/Prefabs/Player/Alma.prefab`. Todos sus archivos de funcionamiento están en la misma carpeta de personaje:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `Scripts/AlmaInput.cs` | Captura entradas y las entrega al motor. |
| `Scripts/AlmaMotor2D.cs` | Aceleración, frenado, control aéreo, salto variable, detección de suelo y reaparición. |
| `Scripts/AlmaAnimation.cs` | Orientación del sprite y parámetros del Animator según el movimiento real. |
| `Scripts/AlmaCameraFollow.cs` | Seguimiento suave, tamaño 6 y anticipación horizontal según velocidad. |
| `Configuration/AlmaMovement.asset` | Valores editables de movimiento y salto. |
| `Configuration/AlmaFrictionless.physicsMaterial2D` | Evita adherirse a paredes. |
| `Animations/` | Sprites originales, clips y controlador de estados. |
| `Tests/PlayMode/` | Siete pruebas de movimiento, salto, animación, paredes y reaparición. |

El cuerpo usa Rigidbody2D con interpolación, colisión continua, rotación bloqueada y una cápsula estable alrededor del cuerpo. La cola no engancha los bordes. El salto admite 0,14 s después de abandonar un borde y recuerda una pulsación durante 0,12 s antes de aterrizar. Mantener el botón no provoca saltos repetidos ni permite un segundo salto aéreo.

Idle reproduce ocho frames a 8 fps; Run reproduce ocho a 12 fps con ritmo ajustado a la velocidad. Jump utiliza las poses de ascenso de la hoja original y Fall su pose de descenso; cambia según la velocidad vertical. Las transiciones responden de inmediato al movimiento y al contacto con suelo, sin esperar a que termine un clip. Los PNG originales y el clip completo de salto se conservan.

La escena incluye un suelo y tres plataformas de práctica con desniveles alcanzables. Si Alma cae por debajo de Y = -12, reaparece en su posición inicial. Checkpoints, peligros, doble salto, Dash, Pisotón, Rugido y botones táctiles se incorporarán en sus siguientes fases.

Las siete pruebas se pueden ejecutar en Test Runner → PlayMode → `AlmaMovementTests`. Cubren carrera/frenado/orientación, estados animados, salto largo y corto, buffer al aterrizar, coyote time sin doble salto, paredes y reaparición. El menú **Alma → Configure player and practice scene** reconstruye la configuración base del prefab y del Animator; sobrescribe sus clips/configuración de estados, por lo que se reserva para restaurar esta base.

Alma es una madre dinosaurio ágil. Su control debe permitir saltos precisos y encadenar habilidades sin retrasos artificiales. No tiene puntos de vida: al tocar un peligro activo reaparece en el último checkpoint. El juego no usa música ni efectos de sonido; cada acción necesita señales visuales claras.

## Controles y habilidades

| Acción | Teclado de referencia | Regla |
| --- | --- | --- |
| Moverse | A/D o flechas | Aceleración y frenado breves, con control aéreo. |
| Salto | Espacio | Altura variable al mantener o soltar; coyote time y buffer de entrada. |
| Doble salto | Espacio en el aire | Un segundo impulso, recuperado al aterrizar o tocar un recurso que lo recargue. |
| Pisotón | S, abajo o C en el aire | Breve preparación y descenso vertical rápido; rompe suelos y activa mecanismos. |
| Dash aéreo | Shift en el aire | Impulso horizontal en la dirección fijada al comenzar; una carga aérea y recarga al aterrizar o tocar una espora. |
| Rugido | E o F | Cono frontal que empuja objetos y activa objetivos compatibles. |

El mando y los controles táctiles deben ofrecer las mismas acciones con iconos y estados visibles. El Dash no concede inmunidad a enemigos, pinchos, veneno ni lava; durante el impulso ignora el viento. Ninguna habilidad debe sustituir el botón de otra.

Las habilidades se incorporan de forma acumulativa: Doble Salto en el mundo 1, Pisotón en el 2, Dash en el 3 y Rugido en el 4. Los altares del [inventario](../INVENTARIO_GAMEPLAY_PREFABS.md) son las piezas previstas para desbloquearlas. La progresión exacta de cada escena se define en su ficha de nivel.

## Física de referencia

Una unidad de juego equivale aproximadamente a un metro. Mantener la misma física en los cuatro mundos y ajustar el diseño de plataformas a ella.

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
| Dash | 6 m en 0,2 s; recarga de 0,4 s |
| Pisotón | 0,1 s de preparación; 22 m/s de descenso |
| Rugido | 0,25 s; cono frontal de 3 m y 45° de semiancho |
| Resonancia de campanas | Hasta 8 m dentro del cono frontal |

El doble salto debe elevar la velocidad vertical al menos a 7,6 m/s sin anular un rebote que ya sea más fuerte. Las pulsaciones se capturan entre pasos de física y se consumen una sola vez. El hongo saltarín del inventario parte de 17 m/s; mantener salto permite un rebote de ×1,18. Estos valores requieren pruebas jugables, especialmente junto a superficies móviles y plataformas altas.

**Alcances estimados en plano**, con salto mantenido y salida a velocidad máxima: salto simple ≈1,56 m de altura y 4,64 m de recorrido; doble salto cerca del ápice ≈2,89 m de altura total y 7,8 m de recorrido. Son estimaciones del centro del personaje, no garantías de aterrizaje. Cada plataforma debe ofrecer margen para el collider completo, aceleración, variación de salto y errores normales del jugador. Un salto obligatorio no debe depender del coyote time ni de tocar un borde exacto. Las superficies necesarias para pisotones y contraataques deben quedar dentro del alcance real del doble salto.

## Estados y prioridad de acciones

Estados previstos: Idle, Run, Jump, DoubleJump, Fall, GroundPound, Dash, Roar y Dead/Respawn. Walk puede ser una variación visual de locomoción, sin necesitar otro estado de control. Las transiciones se resuelven por entradas y condiciones físicas; la animación responde al estado, nunca retrasa el movimiento.

El Dash mantiene su dirección durante todo el impulso. El Pisotón requiere estar en el aire y debe terminar al impactar. El Rugido toma la orientación actual de Alma. Al aterrizar se recuperan salto y Dash; las esporas pueden recargarlos en el aire. Los checkpoints deben restablecer un estado seguro y no permitir reaparecer dentro de un peligro activo.

## Animaciones y arte

Las [láminas de diseño y poses](../../Art/Player/) son referencias conceptuales. La implementación actual usa las nuevas hojas Idle, Run y Jump de ocho frames cada una, situadas en `Assets/Prefabs/Player/Animations/`. Las animaciones de habilidades avanzadas todavía están pendientes.

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

La cámara de **todos los niveles y jefes** tendrá tamaño ortográfico 6, seguirá a Alma con suavizado y mostrará aproximadamente 1,25 unidades adicionales hacia la dirección de desplazamiento. Al detenerse, el encuadre vuelve al centro. Cada nivel puede limitar el recorrido de cámara para evitar enseñar zonas fuera del mapa, sin cambiar el zoom.

Todo contacto con un peligro activo es letal y produce reaparición; no hay barra de HP. Una ventana segura de un enemigo o trampa debe indicarse visualmente. La disponibilidad del Dash, los avisos de ataque, los temporizadores, los checkpoints y los rescates también deben entenderse sin sonido y sin depender solo del color.

## Integración futura

El archivo actual es `Assets/Prefabs/Player/Alma.prefab`, siguiendo la estructura creada para esta reconstrucción. Su controlador, configuración de física, sprites y animaciones están agrupados con él. Cada escena tendrá una sola instancia de Alma, y sus mecanismos enlazarán esa instancia y la cámara. Antes de construir niveles completos, verificar entradas de teclado, mando y táctil, alcance de salto, Dash, Pisotón, Rugido, muerte/reaparición y cámara con tamaño 6.
