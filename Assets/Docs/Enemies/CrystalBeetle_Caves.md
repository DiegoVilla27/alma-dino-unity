# Escarabajo de cristal — enemigo del Mundo 2

**Estado (5 de octubre de 2026):** implementado en Unity 6000.6.0f1 como prefab reutilizable, con patrulla entre dos puntos, animación Walk, disparo de cristales en arco, muerte por contacto y volteo con el Pisotón. Hay una instancia de prueba en `Level_1_1`, pero todavía no está probado a fondo (ver [Pendiente](#pendiente)).

Ficha de diseño original: [inventario, enemigo 2](../INVENTARIO_GAMEPLAY_PREFABS.md#2-escarabajo-de-cristal). Uso previsto en [Level 2-3](../Levels/World_2_Caves/Level_2_3.md) y [Level 2-4](../Levels/World_2_Caves/Level_2_4.md). Jugador: [Alma](../Player/Alma.md).

## Qué es

Enemigo terrestre acorazado de las Cuevas de Cristal. Patrulla entre dos puntos y su caparazón de cristal es letal: tocarlo mata a Alma, también al saltarle encima. Si Alma se acerca, se detiene, se gira hacia ella y le lanza fragmentos de cristal en arco cada pocos segundos. Solo el **Pisotón** lo neutraliza: la onda lo voltea y lo deja patas arriba unos segundos, inofensivo y usable como plataforma. Luego se da la vuelta y sigue patrullando. El Rugido no le afecta.

## Archivos

Todo está en `Assets/Prefabs/Enemies/CrystalBeetle_Caves/`:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `CrystalBeetle.prefab` | Prefab reutilizable: SpriteRenderer, Animator y `CrystalBeetle2D`. |
| `Scripts/CrystalBeetle2D.cs` | Patrulla, giro, disparo, zona letal, volteo, recuperación y plataforma. Namespace `AlmaGame.Enemies` (ensamblado `Assembly-CSharp`). |
| `Scripts/CrystalShard2D.cs` | Fragmento de cristal: crece sobre el lomo como aviso, vuela en arco, mata a Alma y se rompe al chocar. Se crea por código (sin prefab) bajo un único objeto de escena «Enemy Projectiles». |
| `Animations/Walk/CrystalBeetle_Caves_Walk_Sheet_0.controller` | Animator del prefab con un único estado (la caminata). El script solo cambia su velocidad. |
| `Animations/Walk/CrystalBeetle_Caves_Walk_Animation.anim` + `_Sheet.png` | Caminata hacia la derecha: 8 frames a 12 fps, en bucle. |

## Cómo probarlo

1. Arrastra el prefab a una escena con Alma, con las patas apoyadas en un suelo plano.
2. Selecciónalo: los gizmos muestran la patrulla, la zona letal y la zona del Pisotón (ver [Gizmos](#gizmos)). Ajusta `Point A` y `Point B` para que la línea azul quede sobre el suelo.
3. Pulsa Play: camina de A a B y vuelve. Haz un Pisotón a su lado para voltearlo.

Requisitos: una sola Alma en la escena (la busca en `Awake` con `FindAnyObjectByType<AlmaMotor2D>()` para escuchar su Pisotón), con su `Rigidbody2D` y colisionador.

## Comportamiento

| Estado | Qué pasa | Animación | ¿Letal? |
| --- | --- | --- | --- |
| **Caminando** | Avanza a 2 u/s hacia el punto de destino. | Walk en bucle | Sí |
| **Girando** | Al llegar a A o B se detiene 0,3 s, se refleja (`flipX`) y vuelve a caminar en sentido contrario. | Pausada | Sí |
| **Atacando** | Alma está en su rango de disparo: se detiene, se gira hacia ella y lanza un cristal cada 2,5 s. Al salir Alma del rango, sigue caminando en la dirección en la que mira. | Pausada | Sí |
| **Volteándose** | Tras un Pisotón cercano: salta 0,6 u, sale despedido 0,4 u alejándose de Alma y gira media vuelta en 0,4 s, con un pequeño rebote al caer. | Pausada | No |
| **Patas arriba** | Queda boca arriba 3,5 s, quieto. Su vientre es una plataforma sólida. En los últimos 0,8 s tiembla y parpadea en rojo. | Pausada | No |
| **Recuperándose** | Se da la vuelta con el mismo salto y giro (0,4 s), retira la plataforma y sigue patrullando. | Pausada; se reanuda al terminar | Sí, desde que termina |

- **Muerte por contacto:** mientras camina, gira o ataca, un rectángulo sobre su cuerpo llama a `AlmaMotor2D.Die()` si Alma lo toca. Saltarle encima también lo toca, así que también mata.
- **Excepción del Pisotón:** mientras Alma cae en Pisotón (`IsGroundPounding`), el contacto no mata. Así un Pisotón directo sobre el escarabajo lo atraviesa, impacta en el suelo y lo voltea, en lugar de matar a Alma.
- **Qué lo voltea:** el evento `GroundPoundLanded` de Alma, si el impacto ocurre a 2 u o menos del borde de su cuerpo y a menos de 1,5 u de diferencia de altura entre ambos centros. Si ya está patas arriba, otro Pisotón reinicia los 3,5 s. Si está recuperándose, lo vuelve a voltear. El Rugido y el resto de acciones no le afectan.
- **Dirección del volteo:** sale despedido alejándose de Alma y gira en ese mismo sentido.
- **Recuperarse con Alma encima:** la plataforma desaparece al empezar la recuperación; si Alma sigue tocándolo al terminar, muere (como indica el diseño).
- **Patrulla tras el volteo:** continúa desde donde cayó, siempre dentro de A–B.

### Disparo de cristales

- **Rango:** círculo de 5 u alrededor de su centro, siempre que Alma no esté más de 1 u por debajo de él (para no disparar a otra planta del nivel a través del suelo). No comprueba si hay paredes en medio.
- **Mientras Alma siga en el rango:** queda detenido, se gira hacia el lado en el que está ella cada vez que cambia y repite el ciclo de disparo.
- **Ciclo:** el primer cristal sale 0,5 s después de detectarla; luego uno cada 2,5 s. Los 0,5 s previos a cada disparo son el aviso: un fragmento aparece sobre el lomo, apuntando hacia arriba, y crece del 20 % al 100 %.
- **Trayectoria:** arco balístico (gravedad 15 u/s²) que cae donde estaba Alma al salir el cristal. El tiempo de vuelo es la distancia / 7 u/s, entre 0,45 y 1,2 s. No la persigue: moverse después del disparo lo esquiva.
- **El cristal:** mata a Alma al tocarla y se rompe al chocar con cualquier colisionador sólido (suelo, paredes, Alma) o a los 3 s, soltando 6 trocitos de cristal. El movimiento se comprueba con un barrido circular (radio 0,15 u) para que no atraviese nada.
- **Se cancela** si Alma sale del rango durante el aviso o si un Pisotón lo voltea; el fragmento a medio cargar desaparece. Volteado o recuperándose no dispara.
- **Desactivable** por instancia con `Can Shoot` (por ejemplo, en pasillos de techo bajo donde solo deba patrullar).

## Feedback visual (sin sonido)

- Reflejo del sprite al girar en los extremos, tras una pausa breve.
- Volteo: salto en arco, media vuelta suavizada y rebote de escala al caer (hasta 115 % × 85 %).
- Aviso de disparo: un fragmento de cristal cian crece sobre el lomo durante 0,5 s antes de salir.
- Cristal en vuelo orientado según su trayectoria; al romperse, 6 trocitos cian que caen y se desvanecen.
- Aviso de recuperación: temblor horizontal de ±0,04 u y parpadeo blanco ↔ rojo claro (1; 0,7; 0,7) durante los últimos 0,8 s.

## Valores (`CrystalBeetle2D` en el prefab)

Se editan en el Inspector; cada instancia puede sobrescribirlos (sobre todo la patrulla). Distancias en unidades; las zonas se multiplican por la escala del objeto.

| Campo | Valor | Uso |
| --- | ---: | --- |
| **Patrulla** | | |
| `Point A` / `Point B` | −2 / 2 | Extremos de la patrulla, como desplazamiento en X desde la posición inicial. |
| `Walk Speed` | 2 | Velocidad (u/s). |
| `Turn Pause` | 0,3 | Pausa en cada extremo antes de girar (s). |
| **Peligro (local, mirando a la derecha)** | | |
| `Body Offset` | (0,05; −0,2) | Centro de la zona letal. |
| `Body Size` | (2,4; 2) | Tamaño de la zona letal. |
| **Pisotón** | | |
| `Flip Radius` | 2 | Distancia máxima del impacto al borde de su cuerpo. |
| `Flip Max Height` | 1,5 | Diferencia de altura máxima entre los centros. |
| `Flipped Time` | 3,5 | Tiempo patas arriba (s). |
| **Movimiento del volteo** | | |
| `Flip Duration` | 0,4 | Duración del salto y giro, al voltearse y al recuperarse (s). |
| `Flip Hop Height` | 0,6 | Altura del salto. |
| `Flip Knockback` | 0,4 | Desplazamiento lejos de Alma. |
| `Flipped Y Offset` | −0,1 | Ajuste vertical boca arriba (los cristales del lomo quedan apoyados en el suelo). |
| `Warning Time` | 0,8 | Aviso antes de recuperarse (s). |
| `Warning Tint` | (1; 0,7; 0,7) | Color del parpadeo. |
| **Disparo** | | |
| `Can Shoot` | sí | Activa o desactiva el disparo en esta instancia. |
| `Shoot Range` | 5 | Radio del círculo de detección. |
| `Shoot Min Height` | −1 | Altura mínima de Alma respecto a su centro para disparar. |
| `Shot Interval` | 2,5 | Tiempo entre disparos (s). |
| `Charge Time` | 0,5 | Aviso antes de cada disparo (s). |
| `Shard Origin` | (0,1; 1,1) | Punto del lomo donde crece y sale el cristal (se refleja según el lado). |
| `Shard Speed` | 7 | Velocidad media que fija el tiempo de vuelo (u/s). |
| `Shard Gravity` | 15 | Gravedad del arco (u/s²). Más alta, arco más alto y cerrado. |
| `Shard Lifetime` | 3 | Tiempo máximo en vuelo (s). |
| `Shard Color` | (0,6; 0,95; 1) | Color del cristal y de sus trocitos. |
| **Plataforma (local, de pie)** | | |
| `Platform Offset` | (0,03; −0,36) | Al girar 180° queda sobre el vientre, por encima del centro. |
| `Platform Size` | (2,5; 1,6) | Tamaño. |

Diferencias con el diseño original: la velocidad pasó de 0,7 a 2 u/s (ajustada el 5/10/2026); el disparo de cristales no existía en el diseño y se añadió para que sea una amenaza a distancia; el «impulso al voltearse» de 1,5 u/s del prototipo se sustituyó por un salto de 0,6 u en 0,4 s. El tiempo volteado (3,5 s) y el radio del Pisotón (2 m) coinciden.

## Animaciones y sprites

| Hoja | Formato | Uso |
| --- | --- | --- |
| Walk | 1024×512, 8 frames de 256×256, 80 px/unidad, pivote centrado, mirando a la derecha | Caminata en bucle; se pausa (velocidad 0 del Animator) al girar, voltearse y estar patas arriba. |

Cada frame mide 3,2 × 3,2 unidades. Medido sobre el dibujo: el cuerpo ocupa ~−1,5 a 1,5 u en X; las patas llegan a ~−1,37 u y los cristales del lomo a ~1,27 u respecto al centro.

No hay hojas de volteo ni de patas arriba: se resuelve girando el sprite de la caminata 180°. Si se dibujan después (reacción al sismo, patas arriba pataleando), se conectarían como estados nuevos del Animator.

## Instancias en escenas

| Escena | Objeto | Posición | Valores cambiados respecto al prefab |
| --- | --- | --- | --- |
| `Scenes/World_01/Level_1_1.unity` | `CrystalBeetle` | (−1,6; −0,08) | Ninguno (instancia de prueba: es un enemigo del Mundo 2) |

## Gizmos

- **Azul:** recorrido de la patrulla (A–B) con sus dos extremos.
- **Rojo:** zona letal del cuerpo.
- **Amarillo:** zona donde un Pisotón lo voltea.
- **Círculo cian:** rango de disparo (solo si `Can Shoot` está activo).

## Coste

Un SpriteRenderer, un Animator y un `BoxCollider2D` (creado en `Awake`, activo solo patas arriba). Una consulta `Physics2D.OverlapBox` por paso de física mientras es peligroso. Por cristal en vuelo: un SpriteRenderer y un `CircleCast` por paso de física; como mucho uno o dos a la vez por escarabajo. Un sistema de partículas pequeño (máx. 24) para las roturas. Texturas del cristal generadas una vez por código.

## Pendiente

- Colocarlo en un nivel de cuevas y probarlo; ajustar la zona letal, la plataforma y `Flipped Y Offset` a ojo.
- **No sigue el terreno:** se mueve en línea recta a la altura inicial. Colocar A–B sobre suelo plano; no detecta bordes ni paredes («camina cuando tiene apoyo» del diseño original no está implementado).
- Indicador del tiempo restante patas arriba (el diseño mencionaba una barra; hoy solo hay aviso en los últimos 0,8 s).
- Efectos: destello en el caparazón, onda sísmica azul al voltearse.
- Restaurar su estado cuando Alma muere o reaparece (hoy sigue con su ciclo).
- Busca a Alma una sola vez en `Awake`: si Alma se crea más tarde, no escuchará su Pisotón ni la detectará para disparar.
- El disparo no comprueba la línea de visión: dispara aunque haya una pared entre ambos (el cristal se romperá contra ella).
- Equilibrar disparo y niveles: en pasillos estrechos de techo bajo (Level 2-3) puede ser excesivo; usar `Can Shoot` donde haga falta.
