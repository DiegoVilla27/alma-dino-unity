# Salamandra de magma — enemigo del Mundo 4

**Estado (7 de octubre de 2026):** implementada en Unity 6000.6.0f1 como prefab reutilizable, con animaciones Walk y Attack, patrulla entre dos puntos, detección de Alma a izquierda o derecha, bolas de fuego dirigidas con partículas y cuerpo letal. Comprobada con una prueba PlayMode (fuera del repositorio): patrulla y se gira, se detiene, se gira hacia Alma, dispara, la mata y vuelve a caminar cuando Alma se va. Todavía no hay instancia del prefab en ninguna escena (ver [Pendiente](#pendiente)).

Ficha de diseño original: [inventario, enemigo 5](../INVENTARIO_GAMEPLAY_PREFABS.md#5-salamandra-de-magma). Uso previsto en [Level 4-3](../Levels/World_4_Volcano/Level_4_3.md) y [Level 4-4](../Levels/World_4_Volcano/Level_4_4.md). Jugador: [Alma](../Player/Alma.md). Comparte el patrón del [escarabajo de cristal](CrystalBeetle_Caves.md) (patrulla y ataque a distancia) y del [sapo venenoso](PoisonToad_Swamp.md) (ataque frame a frame con aviso).

## Qué es

Enemigo terrestre de la Cima Volcánica. Patrulla entre dos puntos; si Alma entra en su zona de visión por cualquier lado, se detiene, se gira hacia ella y le escupe bolas de fuego en línea recta cada 2,2 s, avisando antes con un parpadeo rojo y llamas que se juntan en la boca. Cuando Alma sale de la zona, sigue caminando. Tocar su cuerpo mata siempre.

## Archivos

Todo está en `Assets/Prefabs/Enemies/MagmaSalamander_Volcano/`:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `MagmaSalamander_Volcano.prefab` | Prefab reutilizable: SpriteRenderer, Animator y `MagmaSalamander2D`. Sin colisionador propio. |
| `Scripts/MagmaSalamander2D.cs` | Patrulla, visión, ciclo de disparo, animación frame a frame, cuerpo letal, aviso y sistemas de partículas. Namespace `AlmaGame.Enemies` (ensamblado `Assembly-CSharp`). |
| `Scripts/MagmaFireball2D.cs` | Bola de fuego: vuela recta, deja estela de llamas, mata a Alma y estalla en chispas al chocar. Se crea por código (sin prefab) bajo el objeto de escena «Enemy Projectiles». |
| `Animations/Walk/MagmaSalamander_Volcano_Walk_Sheet_0.controller` | Animator del prefab, con los estados `Walk` (por defecto) y `Attack`. Sin transiciones ni parámetros. |
| `Animations/Walk/MagmaSalamander_Volcano_Walk_Animation.anim` + `_Sheet.png` | Caminar: 8 frames a 12 fps, en bucle. |
| `Animations/Attack/MagmaSalamander_Volcano_Attack_Animation.anim` + `_Sheet.png` | Disparo hacia la derecha: 4 frames. El script elige cada frame. |
| `Animations/Attack/MagmaSalamander_Volcano_Attack_Sheet_0.controller` | Generado por Unity al crear el clip; el prefab no lo usa. |
| `Sprites/MagmaSalamander_Volcano_Walk_Base.png` | Imagen base de referencia; no se usa en el juego. |

## Cómo probarla

1. Arrastra el prefab a una escena con Alma. Colócala con las patas en el suelo: el centro de la salamandra va 1,43 unidades por encima del suelo.
2. Selecciónala: en la vista Scene aparecen los gizmos (ver [Gizmos](#gizmos)). Ajusta `Point A` / `Point B` para que la ruta quede sobre suelo firme.
3. Pulsa Play y acércate con Alma por cualquier lado.

Requisitos: una sola Alma en la escena (la busca al empezar con `FindAnyObjectByType<AlmaMotor2D>()`), con su `Rigidbody2D` y colisionador, como en el prefab de Alma.

## Comportamiento

| Estado | Qué pasa | Animación | ¿Letal? |
| --- | --- | --- | --- |
| **Caminando** | Avanza a 2,2 u/s hacia el extremo de la ruta al que mira. | Walk | Cuerpo |
| **Girando** | Al llegar a un extremo se para 0,3 s y se da la vuelta. | Walk en pausa | Cuerpo |
| **Atacando** | Alma está en su zona de visión: se para y dispara en ciclo (ver abajo). Al salir Alma, sigue caminando en la dirección en la que mira. | Attack frame a frame | Cuerpo y bolas |

Desde Caminando o Girando, en cuanto ve a Alma pasa a Atacando.

### Ciclo de disparo

| Fase | Duración | Frame de Attack | Qué pasa |
| --- | --- | --- | --- |
| **Aviso** | 0,65 s | 1 | Lado **fijado**; parpadeo rojo y llamitas que aparecen alrededor de la boca y entran en ella, más rápidas y grandes al final. Si Alma sale de la zona, se cancela y vuelve a caminar. |
| **Disparo** | 0,25 s | 2 (0,08 s), luego 3 | Abre la boca; con la boca abierta del todo sale la bola de fuego con 6 chispas. |
| **Recuperación** | 0,15 s | 4 | Cierra la boca. |
| **Espera** | hasta completar 2,2 s entre disparos (~1,1 s) | 1 | Se gira para seguir mirando a Alma. Si Alma sale de la zona, vuelve a caminar. |

El primer disparo sale 0,73 s después de ver a Alma (aviso + apertura de boca); luego uno cada 2,2 s. Si Alma sale de la zona y vuelve a entrar, el ciclo empieza de nuevo con el aviso.

- **Lado:** derecha si Alma está a su derecha y viceversa. Se reutilizan las hojas de la derecha con `SpriteRenderer.flipX`; la zona del cuerpo y la boca se reflejan con ella.
- **Animación:** al atacar, el script pone el Animator a velocidad 0 y llama a `Animator.Play("Attack", 0, tiempo)` para mostrar el frame exacto. Al volver a caminar reproduce `Walk` con velocidad 1.
- **Cuerpo letal:** un rectángulo sobre el cuerpo (sin la cola levantada) mata a Alma al tocarlo en cualquier estado. Saltarle encima también mata.
- **Muerte:** se llama a `AlmaMotor2D.Die()` (se ignora si ya está muerta). El Dash no da inmunidad.
- **Reinicio al morir Alma:** al reaparecer ella (evento `Respawned`), las bolas en vuelo desaparecen y la salamandra vuelve a su posición inicial, mirando a la derecha y caminando.

### La bola de fuego

- **Dirigida y recta:** apunta al centro de Alma en el momento del disparo y no la persigue; moverse después del disparo la esquiva. El ángulo se limita a ±40° respecto a la horizontal hacia donde mira, así que si Alma está muy arriba la bola sale a 40°.
- Velocidad 7 u/s y 2 s de vida: alcance máximo 14 u.
- **Choque:** mata a Alma al tocarla y estalla al chocar con cualquier colisionador sólido (suelo, paredes, Alma) o al acabar su vida. El movimiento se comprueba con un barrido circular (radio 0,2 u) para que no atraviese nada.
- **Forma:** bola brillante de 0,6 u (centro amarillo-blanco, borde naranja que se desvanece en rojo), orientada según su velocidad y con un parpadeo de tamaño, como una llama.

## Partículas

Dos sistemas por salamandra, creados por código como hijos de ella, en espacio de mundo, con la textura de la bola de fuego (generada una vez) y un degradado de color amarillo-blanco → naranja → rojo oscuro que se desvanece:

| Sistema | Cuándo | Partículas | Vida | Tamaño | Gravedad |
| --- | --- | --- | --- | --- | --- |
| `MagmaFireTrail` | Estela: una llama cada 0,02 s detrás de la bola. Aviso: llamitas que entran en la boca (0,12–0,28 u, 0,2 s). Al estallar: 5 bocanadas. | máx. 128 | 0,25–0,4 s | 0,3–0,5 u | −0,3 (suben) |
| `MagmaFireBurst` | 14 chispas en todas direcciones al estallar; 6 chispas hacia delante al disparar. | máx. 64 | 0,3–0,6 s | 0,08–0,16 u | ×1 |

Todas encogen al 20 % al final de su vida.

## Valores (`MagmaSalamander2D` en el prefab)

Se editan en el Inspector del componente; cada instancia de la escena puede sobrescribirlos. Las distancias de visión, cuerpo y boca se multiplican por la escala del objeto; la ruta, la velocidad y la bola, no.

| Campo | Valor | Uso |
| --- | ---: | --- |
| **Patrulla** | | |
| `Point A` / `Point B` | −3 / 3 | Extremos de la ruta, relativos a la posición inicial (6 u de recorrido). |
| `Walk Speed` | 2,2 | Velocidad al caminar (u/s). |
| `Turn Pause` | 0,3 | Pausa en cada extremo antes de girarse. |
| **Visión** | | |
| `Sight Range` | 8 | Distancia horizontal máxima, a cada lado, para atacar. |
| `Sight Height` | −2 / 3 | Rango vertical respecto al centro de la salamandra. |
| **Tiempos de ataque (s)** | | |
| `Windup Time` | 0,65 | Aviso. |
| `Fire Time` | 0,25 | Boca abierta (la bola sale a los 0,08 s). |
| `Recover Time` | 0,15 | Boca cerrándose. |
| `Shot Interval` | 2,2 | Tiempo entre disparos. |
| **Aviso** | | |
| `Warning Tint` | (1; 0,6; 0,6) | Color del parpadeo. |
| **Cuerpo (mirando a la derecha)** | | |
| `Body Offset` | (0,15; −0,6) | Centro de la zona letal respecto al centro de la salamandra. |
| `Body Size` | (3,2; 1,4) | Tamaño de esa zona. |
| **Bola de fuego (mirando a la derecha)** | | |
| `Mouth Offset` | (1,75; −0,3) | Punto de salida (boca abierta del frame 3), a 1,13 u del suelo. |
| `Fireball Speed` | 7 | Velocidad (u/s). |
| `Fireball Lifetime` | 2 | Segundos antes de estallar sola. |
| `Max Aim Angle` | 40 | Ángulo máximo de puntería respecto a la horizontal. |

## Animaciones y sprites

| Hoja | Formato | Uso |
| --- | --- | --- |
| Walk | 1024×512, 8 frames de 256×256, 60 px/unidad, pivote centrado | Caminar en bucle, mirando a la derecha. |
| Attack | 1024×256, 4 frames de 256×256, 60 px/unidad, pivote centrado | Hacia la derecha. 1: boca cerrada (aviso y espera) · 2: boca entreabierta · 3: boca abierta (dispara) · 4: boca cerrada. |

Cada frame mide 4,27 × 4,27 unidades. Medido sobre el dibujo: las patas llegan a −1,43 unidades bajo el centro y el cuerpo ocupa ±1,87 unidades a cada lado (la cola, enroscada hacia arriba, a la izquierda), igual en las dos hojas.

El clip Attack estaba vacío en disco (sin frames) y se rellenó a mano; el estado del controller del prefab se renombró de `MagmaSalamander_Volcano_Walk_Animation` a `Walk` y se le añadió el estado `Attack`.

## Gizmos

Al seleccionar la salamandra en la vista Scene:

- **Naranja (línea con dos esferas):** ruta de patrulla.
- **Amarillo:** zona de visión.
- **Rojo:** zona letal del cuerpo.
- **Naranja rojizo (a cada lado):** boca y límites de puntería (±40°) con el alcance máximo de la bola.

## Coste

Un SpriteRenderer y un Animator por salamandra, dos sistemas de partículas sin emisión continua y un material propio (con la textura del fuego). Una consulta `Physics2D.OverlapBox` por paso de física (el cuerpo) y un `CircleCast` por bola en vuelo.

## Pendiente

- Colocarla en una escena y probarla a mano; ajustar `Mouth Offset` y la zona del cuerpo a ojo.
- **Aturdimiento sin hacer:** el diseño original pide que el Rugido o el Pisotón la aturdan 3 s (inofensiva) y limpien sus bolas, con retroceso. Ahora no reacciona a ninguna habilidad.
- La ruta no comprueba el suelo: si un extremo queda sobre un hueco, camina en el aire.
- No tiene colisionador: Alma la atraviesa (y muere). Si debe bloquear el paso, hay que añadirlo.
- Busca a Alma una sola vez al empezar: si Alma se crea después, no la verá.
- Sin `MagmaSalamanderConfig.asset` ni prefab de bola de fuego ni pool: los valores están en el componente y las bolas se crean y destruyen al vuelo.
