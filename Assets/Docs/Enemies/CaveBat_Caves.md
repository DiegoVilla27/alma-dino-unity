# Murciélago de cueva — enemigo del Mundo 2

**Estado (7 de octubre de 2026):** implementado en Unity 6000.6.0f1 como prefab reutilizable, con patrulla aérea entre dos puntos, animación Fly, aviso, picado en arco y muerte por contacto. No hay una instancia colocada en la escena actual; falta probarlo a fondo (ver [Pendiente](#pendiente)).

Ficha de diseño original: [inventario, enemigo 3](../INVENTARIO_GAMEPLAY_PREFABS.md#3-murciélago-de-cueva). Uso previsto en [Level 2-3](../Levels/World_2_Caves/Level_2_3.md). Jugador: [Alma](../Player/Alma.md).

## Qué es

Enemigo aéreo de las Cuevas de Cristal. Patrulla volando entre dos puntos y, cuando Alma pasa por debajo dentro de su alcance, se detiene, avisa y se lanza en picado en forma de U a través de donde estaba ella, para volver a subir al otro lado. Tocar su cuerpo mata a Alma. No se le puede neutralizar: se esquiva leyendo el aviso y el arco (alejándose, esperando o con el doble salto).

## Archivos

Todo está en `Assets/Prefabs/Enemies/CaveBat_Caves/`:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `CaveBat_Caves.prefab` | Prefab reutilizable: SpriteRenderer, Animator y `CaveBat2D`. Sin colisionador. |
| `Scripts/CaveBat2D.cs` | Patrulla, detección, aviso, picado, descanso y zona letal. Namespace `AlmaGame.Enemies` (ensamblado `Assembly-CSharp`). |
| `Animations/Fly/CaveBat_Caves_Fly_Sheet_0.controller` | Animator del prefab con un único estado de vuelo, siempre en reproducción. |
| `Animations/Fly/CaveBat_Caves_Fly_Animation.anim` + `_Sheet.png` | Aleteo mirando a la derecha: 8 frames a 12 fps, en bucle. |

## Cómo probarlo

1. Arrastra el prefab a una escena con Alma, a la altura a la que deba patrullar (por encima de la cabeza de Alma).
2. Selecciónalo: los gizmos muestran la patrulla, la zona de detección y la zona letal (ver [Gizmos](#gizmos)). Ajusta `Point A` y `Point B`.
3. Pulsa Play y pasa por debajo.

Requisitos: una sola Alma en la escena (la busca en `Awake` con `FindAnyObjectByType<AlmaMotor2D>()`), con su `Rigidbody2D` y colisionador.

## Comportamiento

| Estado | Qué pasa | Duración | ¿Letal al tocarlo? |
| --- | --- | --- | --- |
| **Patrulla** | Vuela a 2,5 u/s entre A y B, con un balanceo vertical de ±0,15 u. Se refleja (`flipX`) al llegar a cada extremo. Si está fuera de A–B (tras un picado), vuelve hacia la zona. | — | Sí |
| **Aviso** | Alma está en su zona: se detiene, se gira hacia ella, tiembla y parpadea en rojo. | 0,65 s | Sí |
| **Picado** | Arco en U: baja hasta donde estaba Alma al terminar el aviso y sube al otro lado, simétrico, hasta su altura de patrulla. Se inclina siguiendo la trayectoria (hasta 35°). | 1,6 s | Sí |
| **Descanso** | Revolotea en el sitio donde terminó el picado. | 2 s | Sí |

Al terminar el descanso vuelve a patrullar; si Alma sigue en la zona, ataca de nuevo enseguida.

- **Detección:** Alma a 4 u o menos en horizontal (a cualquier lado) y entre 6 u por debajo y 0,5 u por encima del murciélago. Solo se comprueba mientras patrulla.
- **El picado no persigue a Alma:** apunta a su posición al terminar el aviso. La profundidad se limita entre 1 u (mínimo, aunque Alma esté a su altura) y 6 u por debajo de la altura de patrulla. Si Alma está justo debajo, el picado es una caída vertical y una subida en el mismo sitio.
- **Muerte por contacto:** en todos los estados, un rectángulo sobre el cuerpo y la cabeza (no sobre las alas) llama a `AlmaMotor2D.Die()`. El rectángulo gira con la inclinación del picado.
- **Sin interacción con las habilidades:** el Pisotón y el Rugido no le afectan.
- **Animación:** el aleteo se reproduce siempre; el script solo refleja e inclina el sprite. No toca el Animator.
- **Reinicio al morir Alma:** al reaparecer ella (evento `Respawned`), vuelve a su posición inicial y a patrullar.

## Feedback visual (sin sonido)

- Aviso: temblor horizontal de ±0,05 u y parpadeo blanco ↔ rojo claro (1; 0,6; 0,6) durante 0,65 s.
- Picado: el morro se inclina hacia abajo al bajar y hacia arriba al subir.
- Balanceo vertical constante para que no parezca que se desliza.

El diseño original incluía un símbolo `!` sobre el murciélago; se descartó a petición (5/10/2026).

## Valores (`CaveBat2D` en el prefab)

Se editan en el Inspector; cada instancia puede sobrescribirlos (sobre todo la patrulla). Distancias en unidades; la zona letal se multiplica por la escala del objeto.

| Campo | Valor | Uso |
| --- | ---: | --- |
| **Patrulla** | | |
| `Point A` / `Point B` | −3 / 3 | Extremos de la patrulla, como desplazamiento en X desde la posición inicial. La altura de patrulla es la inicial. |
| `Fly Speed` | 2,5 | Velocidad de patrulla (u/s). |
| `Bob Height` | 0,15 | Amplitud del balanceo vertical. |
| `Bob Frequency` | 0,8 | Balanceos por segundo. |
| **Detección** | | |
| `Detect Range` | 4 | Distancia horizontal máxima a cada lado. |
| `Detect Depth` | 6 | Distancia máxima por debajo. |
| `Detect Above` | 0,5 | Margen por encima. |
| **Ataque** | | |
| `Warning Time` | 0,65 | Aviso (s). |
| `Warning Tint` | (1; 0,6; 0,6) | Color del parpadeo. |
| `Dive Time` | 1,6 | Duración del picado completo (s). |
| `Min Dive Depth` / `Max Dive Depth` | 1 / 6 | Profundidad mínima y máxima del punto más bajo respecto a la patrulla. |
| `Max Tilt` | 35 | Inclinación máxima durante el picado (°). |
| `Rest Time` | 2 | Descanso tras el picado (s). |
| **Peligro (local, mirando a la derecha)** | | |
| `Body Offset` | (0,6; −0,4) | Centro de la zona letal (cuerpo y cabeza). |
| `Body Size` | (1,3; 1,3) | Tamaño de la zona letal. |

### Diferencias con el diseño original

| | Diseño | Implementado | Motivo |
| --- | --- | --- | --- |
| Reposo | Dormido colgado del techo | Patrulla en el aire entre A y B | No hay sprite de colgado; pedido así (5/10/2026). |
| Peligro | Solo durante el vuelo de ataque | Tocar su cuerpo mata siempre | Decisión de diseño (5/10/2026); a la altura de patrulla es difícil chocar. |
| Aviso | `!` y cambio de color, 0,65 s | Temblor y parpadeo rojo, 0,65 s | Se descartó el `!`. |
| Detección | 4 u en horizontal, bajo el dormidero | 4 u a cada lado, hasta 6 u por debajo | — |
| Vuelo | 1,6 s, arco de 2,2 u de ancho | 1,6 s, arco en U simétrico cuyo ancho depende de dónde esté Alma | Apunta a Alma en lugar de a un arco fijo. |
| Descanso | 2 s | 2 s | — |

## Animaciones y sprites

| Hoja | Formato | Uso |
| --- | --- | --- |
| Fly | 1774×887, 8 frames de 443×443, 120 px/unidad, pivote centrado, mirando a la derecha | Aleteo en bucle en todos los estados. |

Cada frame mide ~3,7 × 3,7 unidades. Medido sobre el dibujo: con las alas abiertas ocupa hasta ~3,5 u de ancho; el cuerpo y la cabeza quedan algo a la derecha y por debajo del centro, de ahí el desplazamiento de la zona letal.

## Instancias en escenas

Actualmente ninguna. La antigua instancia de prueba en `Level_1_1` se retiró.

## Gizmos

- **Azul:** recorrido de la patrulla (A–B) a la altura de patrulla.
- **Amarillo:** zona de detección alrededor de su posición actual.
- **Rojo:** zona letal del cuerpo (sin inclinación; en juego gira con el picado).

## Coste

Un SpriteRenderer y un Animator. Una consulta `Physics2D.OverlapBox` por paso de física. Sin colisionadores, partículas ni texturas generadas.

## Pendiente

- Probarlo en escena y ajustar la zona letal a ojo.
- **Atraviesa paredes, techo y suelo:** ni la patrulla ni el picado comprueban colisiones. Colocarlo en espacios abiertos o limitar `Max Dive Depth` para que no se meta en el suelo.
- La detección no comprueba la línea de visión (puede atacar a través de una plataforma).
- Sprite de dormido colgado si se quiere recuperar el diseño original.
- Busca a Alma una sola vez en `Awake`: si Alma se crea más tarde, no la detectará.
