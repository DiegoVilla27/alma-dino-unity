# Alma — diseño del personaje jugable

**Estado:** especificación para reconstrucción. En esta rama no hay proyecto Unity, prefab, sprites de producción ni código. Los valores numéricos proceden del prototipo anterior y son objetivos de partida; deben validarse al implementar y probar los nuevos niveles.

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

Las habilidades se incorporan de forma acumulativa: Doble Salto en el mundo 1, Pisotón en el 2, Dash en el 3 y Rugido en el 4. Los altares del [inventario](../../INVENTARIO_GAMEPLAY_PREFABS.md) son las piezas previstas para desbloquearlas. La progresión exacta de cada escena se define en su ficha de nivel.

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

Las [láminas de diseño y poses](../Art/Player/) son referencias conceptuales, no sprites listos para integrar. El prototipo anterior tenía frames de Idle (10), Run (8), Jump (12), Walk (10) y Dead (8); **esos archivos se eliminaron**. Sus ciclos y velocidades anteriores sirven como punto de partida, no como implementación existente.

| Acción visual | Requisito para la nueva producción |
| --- | --- |
| Idle y Run | Silueta clara; transición inmediata al cambiar de movimiento. |
| Jump, DoubleJump y Fall | Ascenso, segundo impulso y descenso distinguibles; no reutilizar la misma pose para los tres momentos finales. |
| GroundPound | Anticipación, caída y golpe visibles sin alargar la ventana de control. |
| Dash | Pose y estela que muestren dirección y carga disponible. |
| Roar | Apertura, onda frontal y recuperación; diferenciar alcance normal y resonancia. |
| Dead/Respawn | Breve lectura de fallo y retorno al checkpoint. |
| Rescue | Interacción propia con cada huevo, sin bloquear el control más de lo necesario. |

Conservar tamaño de lienzo, pivote, escala y línea de apoyo coherentes entre frames. Separar anticipación, acción y recuperación. El sprite de Alma y sus animaciones deben quedar editables en su futura carpeta de personaje; los dibujos conceptuales permanecen en `Docs/Art/Player/`.

## Cámara, daño y feedback

La cámara de **todos los niveles y jefes** tendrá tamaño ortográfico 6, seguirá a Alma con suavizado y mostrará aproximadamente 1,25 unidades adicionales hacia la dirección de desplazamiento. Al detenerse, el encuadre vuelve al centro. Cada nivel puede limitar el recorrido de cámara para evitar enseñar zonas fuera del mapa, sin cambiar el zoom.

Todo contacto con un peligro activo es letal y produce reaparición; no hay barra de HP. Una ventana segura de un enemigo o trampa debe indicarse visualmente. La disponibilidad del Dash, los avisos de ataque, los temporizadores, los checkpoints y los rescates también deben entenderse sin sonido y sin depender solo del color.

## Integración futura

El archivo previsto es `Assets/_Project/Prefabs/Player/Alma/Player_Alma.prefab`. Agrupar con él su controlador, configuración de física, sprites, animaciones y documentación necesaria para usarlo. Cada escena tendrá una sola instancia de Alma, y sus mecanismos enlazarán esa instancia y la cámara. Antes de construir niveles, verificar entradas de teclado, mando y táctil, alcance de salto, Dash, Pisotón, Rugido, muerte/reaparición y cámara con tamaño 6.
