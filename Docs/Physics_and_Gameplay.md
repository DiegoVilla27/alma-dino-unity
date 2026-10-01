# Física y criterios de jugabilidad de Alma

Perfil vigente: 1 de octubre de 2026. Una unidad de Unity representa un metro de juego; la escala está al servicio de la lectura y del control.

## Objetivo

Movimiento semirreal: aceleración breve, trayectorias parabólicas, caída con peso y fuerzas del entorno proporcionales a la masa. Doble salto, dash, pisotón y rugido son habilidades fantásticas con reglas predecibles. No se cambia la gravedad por mundo ni se reduce artificialmente un salto para impedir un atajo.

## Perfil compartido

La fuente de valores en ejecución es `Assets/_Project/Features/Player/ScriptableObjects/AlmaPhysicsConfig.asset`. Los valores predeterminados de `AlmaPhysicsConfigSO` deben coincidir con ese asset.

| Parámetro | Valor | Resultado buscado |
|---|---:|---|
| Velocidad horizontal | 7 m/s | Carrera ágil con distancias legibles |
| Tiempo de aceleración / frenado | 0.10 / 0.08 s | Inercia corta sin deslizamiento largo |
| Tiempo de aceleración aérea | 0.13 s | Correcciones en el aire sin dirección instantánea |
| Gravedad del proyecto | −9.81 m/s² | Dirección vertical constante |
| Escala base | 2.2 | Aceleración efectiva de 21.58 m/s² |
| Velocidad inicial de salto | 8.2 m/s | Altura variable al mantener o soltar salto |
| Velocidad del doble salto | 7.6 m/s | Eleva la velocidad hasta ese mínimo; conserva un rebote superior |
| Multiplicador al caer | 1.8 | Descenso más rápido que el ascenso |
| Multiplicador al soltar salto | 2.4 | Salto corto para evitar techos y ajustar aterrizajes |
| Límite de caída normal | 20 m/s | Se limita también la gravedad del último paso |
| Coyote time / buffer de salto | 0.14 / 0.12 s | Tolerancia de borde y pulsación previa al aterrizaje |
| Dash | 6 m / 0.2 s | 30 m/s, dirección fijada al empezar, gravedad suspendida; vuelve a la velocidad de carrera al terminar |
| Recarga del dash | Una carga aérea; cooldown 0.4 s | Aterrizaje o espora recuperan la carga; la espora elimina el cooldown |
| Pisotón | Preparación 0.1 s; descenso 22 m/s | Velocidad propia, independiente de la caída normal |
| Hongo de jungla por defecto | 17 m/s | Súper rebote de ×1.18 al mantener salto: 20.06 m/s |

Las pulsaciones se registran en `Update` y se conservan hasta el siguiente `FixedUpdate`. Cada pulsación se consume una sola vez. Las transiciones que cambian velocidad y las duraciones de acciones se resuelven con el paso fijo de 0.02 s. El movimiento horizontal aplica una fuerza limitada por aceleración y masa; las habilidades aplican impulsos explícitos.

## Alcances y diseño de huecos

Con salto mantenido, salida a velocidad máxima y llegada a la misma altura, el cálculo continuo estima:

- Salto simple: altura 1.56 m, ápice a 0.38 s y recorrido horizontal de aproximadamente 4.64 m.
- Doble salto cerca del ápice: altura total 2.89 m y recorrido horizontal de aproximadamente 7.8 m.
- Hongo de 17 m/s: altura 6.70 m; con súper rebote, 9.32 m. No incluye el aleteo posterior.

Son aproximaciones de la trayectoria del centro, no garantías de aterrizaje. La integración fija reduce ligeramente las alturas. También cambian el resultado la aceleración inicial, la altura de destino, los colisionadores, soltar salto y el coyote time. La tolerancia de borde puede aportar hasta 0.98 m horizontales antes del impulso, pero no debe exigirse para completar la ruta normal.

Para exigir una habilidad, usar una altura, barrera o combinación claramente fuera del alcance anterior. Un hueco de 4.8 o 5.3 m no demuestra por sí solo que el salto simple sea imposible. Las plataformas de llegada deben admitir la trayectoria del volumen completo de Alma con margen, evitando colocar peligros en el único punto matemático de aterrizaje.

En el tutorial de dash, un hueco de 11 m supera el doble salto incluyendo las ayudas de borde y permite completar la secuencia con margen gracias al impulso de 6 m. No se describe el dash como invulnerabilidad general: ignora viento durante la acción, pero no elimina el daño de enemigos, espinas o lava.

## Entorno y dificultad

| Nivel de jungla | Colapso de hojas | Función |
|---|---:|---|
| 1-1 y 1-2 | 1.0 s | Aprender a leer el aviso y saltar |
| 1-3 | 0.75 s | Encadenar sin exigir reacción perfecta |
| 1-4 | 0.65 s | Evaluar lo aprendido |

El temporizador empieza al aterrizar. Tinte y temblor avisan antes del colapso. La señal debe ser reconocible sin depender únicamente del color. Los checkpoints son superficies estables; un reintento debe comenzar sin un peligro inevitable encima.

El viento se expresa como aceleración en m/s² y se convierte en fuerza multiplicando por la masa. La compensación de gravedad se calcula por cuerpo, sin acumular la de otros cuerpos. Una corriente horizontal de 12 m/s² afecta el recorrido, pero no vence por sí sola el control aéreo de aproximadamente 54 m/s². Para una sección obligatoria, usar un hueco, una barrera de dash o calibrar un viento más fuerte con refugios legibles.

Las esporas recargan dash y doble salto al contacto. No hay que exigir que el jugador conserve un dash para poder tocar el objeto cuya función es recuperarlo.

## Balancines y catapultas del 2-2

Los balancines de 6m basculan en el paso fijo con inclinación limitada a ±18°. Caminar aplica una inclinación gradual según la distancia al pivote; el Pisotón debe golpear la mitad exterior de un extremo para activar la catapulta. El contrapeso sale desde el extremo contrario a 14 m/s, con gravedad 1.8 y masa 4. La runa reconoce el contrapeso ascendente, evitando activaciones por contacto de Alma o por una caída normal.

Tras el golpe, Alma tiene 2.8s para correr al extremo elevado y recibir un impulso de 15 m/s, con Doble Salto recargado. Ese impulso equivale a unos 5.2m de ascenso con su gravedad base; sostener salto conserva el súper rebote existente. La cornisa de Y=6.2 exige el impulso en su recorrido normal y tiene suelo seguro debajo para reintentar.

Las compuertas abren durante 4s. La final exige que las dos runas estén activas simultáneamente; el recorrido de dos balancines se ha verificado dentro de ese intervalo. Una barra muestra el tiempo restante. Si Alma ocupa el hueco al terminar el plazo, el cierre espera a que lo despeje. Los contrapesos se recuperan tras 4.7s y una muerte restaura todo el puzle. Los parámetros viven en `EchoSeesawConfig.asset`; se priorizan resultados acotados y repetibles sobre una simulación libre de masa y torque.

## Enemigos y onda sísmica del 2-3

Un Pisotón que impacta desde arriba emite una onda de radio 2m. La consulta ocurre antes de romper la losa o resolver el daño del caparazón, para que el impacto directo pueda voltear al escarabajo sin matar primero a Alma. Un aterrizaje normal no emite la onda. El escarabajo voltea durante 3.5s, con un pequeño impulso de 1.5 m/s; permanece sólido y su vientre se reconoce como suelo seguro. Recuperar el caparazón vuelve a causar daño por contacto, incluso si Alma sigue encima.

El puzle quebradizo incluye una repisa que termina el picado y da tiempo a que el escarabajo caiga sobre su apoyo. Desde ella se salta al vientre y después se combina salto corto con Doble Salto para salir por debajo del cierre. Este apoyo evita exigir que Alma, que desciende a 22 m/s, aterrice sobre un enemigo que cae más despacio.

Los murciélagos avisan durante 0.65s con cambio de color y símbolo `!`; vuelan en arco durante 1.6s y descansan 2s. Solo el vuelo hace daño, incluyendo el caso en que el contacto empezó durante el aviso. La muerte restaura los enemigos y el suelo quebradizo. Las condiciones de daño y recepción de onda se exponen mediante interfaces de Core; el módulo Enemies no depende del código interno de Player.

## Jefes y rescates

El mono mantiene tres ciclos y fatiga de 3.2 / 2.8 / 2.4 s; estos tiempos coinciden con el controlador y el constructor de arena. Tras cada impacto se conserva la fase alcanzada.

El cuarto huevo se rescata en 4-4. En el combate final los cuatro están en un refugio protegido. El jefe final tiene tres fases diferentes con un impacto por fase y checkpoint entre fases; al morir se restauran magma, plataformas y mecanismos de la fase actual. El rescate permanece guardado. Esta estructura es la especificación para su implementación posterior.

## Estado y validación

El Mundo 1 dispone de escenas y constructores; sus tiempos de hojas se actualizan tanto en escenas como en código y prefab. Los niveles 2-1, 2-2 y 2-3 también disponen de escenas y constructores, con recorridos completos comprobados mediante entradas de jugador en PlayMode. Las demás fichas de mundos posteriores siguen siendo diseño: no equivalen a niveles ya construidos ni a garantías de que sus secuencias estén probadas.

Las pruebas de física deben cubrir pulsaciones entre pasos, doble salto sin acumulación, distancia y dirección del dash, altura y alcance del salto, velocidad terminal y viento con distintas masas. Después se requiere jugar las escenas para valorar comodidad, lectura de señales y ritmo: una prueba matemática no sustituye la experiencia de un jugador.

Referencia de motor: [Rigidbody2D.AddForce, documentación de Unity](https://docs.unity3d.com/6000.0/Documentation/ScriptReference/Rigidbody2D.AddForce.html).
