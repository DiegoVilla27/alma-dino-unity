# Sapo venenoso — enemigo del Mundo 3

**Estado (7 de octubre de 2026):** implementado en Unity 6000.6.0f1 como prefab reutilizable, con animaciones Idle y Attack, detección de Alma a izquierda o derecha, escupitajo de veneno en arco con partículas y cuerpo letal. Comprobado con una prueba PlayMode (fuera del repositorio) en la que escupe hacia Alma y la mata; todavía no hay instancia en ninguna escena (ver [Pendiente](#pendiente)).

Ficha de diseño original: [inventario, enemigo 4](../INVENTARIO_GAMEPLAY_PREFABS.md#4-sapo-venenoso). Uso previsto en el nivel 11 del [Mundo 3](../GDD.md). Jugador: [Alma](../Player/Alma.md).

## Qué es

Enemigo fijo del Pantano: no camina. Reposa con su animación Idle y, cuando Alma entra en su zona por cualquier lado, avisa (se le hincha la papada y parpadea en rojo), se gira hacia ese lado y le escupe un glob de veneno que vuela en arco. El glob mata al tocarla y salpica al chocar. Tocar el cuerpo del sapo mata siempre.

## Archivos

Todo está en `Assets/Prefabs/Enemies/PoisonToad_Swamp/`:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `PoisonToad_Swamp.prefab` | Prefab reutilizable: SpriteRenderer, Animator y `PoisonToad2D`. Sin colisionador propio. |
| `Scripts/PoisonToad2D.cs` | Detección, ciclo de ataque, animación frame a frame, cuerpo letal, aviso, lanzamiento y sistemas de partículas. Namespace `AlmaGame.Enemies` (ensamblado `Assembly-CSharp`). |
| `Scripts/PoisonSpit2D.cs` | Glob de veneno: vuela en arco, gotea, mata a Alma y salpica al chocar. Se crea por código (sin prefab) bajo el objeto de escena «Enemy Projectiles». |
| `Animations/Idle/PoisonToad_Swamp_Idle_Sheet_0.controller` | Animator del prefab, con los estados `Idle` (por defecto) y `Attack`. Sin transiciones ni parámetros. |
| `Animations/Idle/PoisonToad_Swamp_Idle_Animation.anim` + `_Sheet.png` | Reposo: 8 frames a 12 fps, en bucle. |
| `Animations/Attack/PoisonToad_Swamp_Attack_Animation.anim` + `_Sheet.png` | Escupitajo hacia la derecha: 4 frames. El script elige cada frame. |
| `Animations/Attack/PoisonToad_Swamp_Attack_Sheet_0.controller` | Generado por Unity al crear el clip; el prefab no lo usa. |
| `Sprites/PoisonToad_Swamp_Idle_Base.png` | Imagen base de referencia; no se usa en el juego. |

## Cómo probarlo

1. Arrastra el prefab a una escena con Alma. Colócalo con las patas apoyadas en el suelo: el centro del sapo va 1 unidad por encima del suelo.
2. Selecciónalo: en la vista Scene aparecen los gizmos (ver [Gizmos](#gizmos)).
3. Pulsa Play y acércate con Alma por cualquier lado.

Requisitos: una sola Alma en la escena (el sapo la busca al activarse con `FindAnyObjectByType<AlmaMotor2D>()`), con su `Rigidbody2D` y colisionador, como en el prefab de Alma.

## Comportamiento

| Fase | Duración | Frames de Attack | Qué pasa | ¿Letal? |
| --- | --- | --- | --- | --- |
| **Reposo** | mínimo 1 s tras cada ataque | Animación Idle | Vigila la zona de detección. | Cuerpo |
| **Aviso** | 0,6 s | 1 (papada hinchada) | Elige y **fija** el lado; se gira; parpadea en rojo y se hincha hasta un 8 % con un temblor. | Cuerpo |
| **Escupitajo** | 0,3 s | 2 (0,08 s), luego 3 | Abre la boca; al abrirla del todo sale el glob con un chorrito de gotas. | Cuerpo y glob |
| **Recuperación** | 0,15 s | 4 | Cierra la boca y vuelve a Idle mirando a la derecha. | Cuerpo y glob |

Ciclo completo: unos 2,05 s si Alma sigue dentro de la zona (el inventario pedía 2 s).

- **Elección del lado:** derecha si Alma está a su derecha y viceversa (signo de la diferencia en X). Si Alma está casi justo encima (menos de 0,1 unidades en X), repite el último lado. El lado no cambia hasta el siguiente ciclo.
- **Izquierda:** se reutiliza la hoja de la derecha con `SpriteRenderer.flipX`; la zona del cuerpo y la boca se reflejan con ella.
- **Animación:** igual que la planta: el script pone el Animator a velocidad 0 y llama a `Animator.Play("Attack", 0, tiempo)` para mostrar el frame exacto de cada fase. Al terminar vuelve a `Idle` con velocidad 1.
- **Cuerpo letal:** un rectángulo sobre el cuerpo mata a Alma al tocarlo en cualquier fase (el diseño original lo pedía). Saltarle encima también mata.
- **Muerte:** se llama a `AlmaMotor2D.Die()` (se ignora si ya está muerta). El Dash no da inmunidad.
- **Reinicio al morir Alma:** al reaparecer ella (evento `Respawned`), los globs en vuelo desaparecen y el sapo vuelve a reposo con su tiempo de descanso normal.

### El glob de veneno

- **Trayectoria fija** (no apunta a Alma): sale de la boca a 8 u/s hacia delante y 3 u/s hacia arriba, con gravedad 6 u/s². Sube hasta ~0,75 u sobre la boca a 4 u de distancia, vuelve a la altura de la boca a 8 u y llega al suelo a ~10,9 u.
- **Altura sobre el suelo** (boca a 1,35 u): entre 1,35 y 2,1 u en todo el rango de detección, así que alcanza a Alma de pie (mide 2,42 u). Con el salto normal (~1,56 u) no basta para pasarlo por encima; se esquiva con el doble salto, saliendo de la zona o cubriéndose tras un obstáculo (ver [Pendiente](#pendiente)).
- **Choque:** mata a Alma al tocarla y salpica al chocar con cualquier colisionador sólido (suelo, paredes, Alma) o a los 3 s. El movimiento se comprueba con un barrido circular (radio 0,18 u) para que no atraviese nada.
- **Forma:** bola verde de 0,5 u que se orienta según su velocidad, se estira un poco con ella y tiembla al salir de la boca.

## Partículas

Dos sistemas por sapo, creados por código como hijos del sapo, en espacio de mundo, con una textura redonda generada una vez (la misma del glob) y el color del veneno (verde lima con variación más oscura):

| Sistema | Cuándo | Partículas | Vida | Tamaño | Gravedad |
| --- | --- | --- | --- | --- | --- |
| `PoisonSpitTrail` | Gotas que suelta el glob en vuelo, una cada 0,03 s, hacia atrás | máx. 64 | 0,25–0,45 s | 0,06–0,12 u | ×2 |
| `PoisonSpitSplash` | Salpicadura de 10 gotas en semicírculo hacia arriba al chocar; 5 gotas hacia delante al escupir | máx. 48 | 0,35–0,6 s | 0,08–0,18 u | ×1,8 |

Todas se desvanecen al final de su vida y encogen al 40 %.

## Valores (`PoisonToad2D` en el prefab)

Se editan en el Inspector del componente; cada instancia de la escena puede sobrescribirlos. Las distancias están en unidades y se multiplican por la escala del objeto (la velocidad y la gravedad del glob, no).

| Campo | Valor | Uso |
| --- | ---: | --- |
| **Detección** | | |
| `Detect Range` | 8 | Distancia horizontal máxima, a cada lado, para empezar a atacar. El inventario pedía 18; con eso escupiría fuera de pantalla. |
| `Detect Height` | −1,5 / 3 | Rango vertical respecto al centro del sapo en el que se detecta a Alma. |
| **Tiempos (s)** | | |
| `Windup Time` | 0,6 | Aviso. |
| `Spit Time` | 0,3 | Escupitajo (el glob sale a los 0,08 s). |
| `Recover Time` | 0,15 | Vuelta a la pose de reposo. |
| `Rest Time` | 1 | Descanso mínimo entre ataques. |
| **Aviso** | | |
| `Warning Tint` | (1; 0,6; 0,6) | Color del parpadeo. |
| `Windup Swell` | 0,08 | Cuánto se hincha durante el aviso (8 %). |
| **Cuerpo (mirando a la derecha)** | | |
| `Body Offset` | (0,05; −0,15) | Centro de la zona letal respecto al centro del sapo. |
| `Body Size` | (2,5; 1,6) | Tamaño de esa zona. |
| **Escupitajo (mirando a la derecha)** | | |
| `Mouth Offset` | (1,25; 0,35) | Punto de salida del glob (boca abierta del frame 3). |
| `Spit Velocity` | (8; 3) | Velocidad inicial (u/s). |
| `Spit Gravity` | 6 | Gravedad del glob (u/s²); el inventario decía «0,6», que equivale a ~0,6 × 9,81. |
| `Spit Lifetime` | 3 | Segundos antes de salpicar solo. |
| `Poison Color` | (0,65; 1; 0,2) | Color del glob y de las partículas. |

## Animaciones y sprites

| Hoja | Formato | Uso |
| --- | --- | --- |
| Idle | 1024×512, 8 frames de 256×256, 80 px/unidad, pivote centrado | Reposo en bucle, mirando a la derecha. |
| Attack | 1024×256, 4 frames de 256×256, 80 px/unidad, pivote centrado | Hacia la derecha. 1: papada hinchada · 2: boca abriéndose · 3: boca abierta (escupe) · 4: boca cerrada. |

Cada frame mide 3,2 × 3,2 unidades. Medido sobre el dibujo: las patas llegan a −1,0 unidades bajo el centro y el cuerpo ocupa ±1,5 unidades a cada lado, igual en las dos hojas.

La hoja Attack se importó a 90 px/unidad; se cambió a 80 para que el sapo no encoja al atacar. En la copia de trabajo los clips y los controllers estaban vacíos en disco (sin frames ni estados; probablemente el editor abierto los volvió a guardar así): Idle se restauró desde git, y el clip Attack y el estado `Attack` del controller se escribieron a mano.

## Gizmos

Al seleccionar el sapo en la vista Scene:

- **Amarillo:** zona de detección.
- **Rojo:** zona letal del cuerpo.
- **Verde (dos):** trayectoria del glob a cada lado durante toda su vida (en juego salpica antes, al tocar el suelo).

## Coste

Un SpriteRenderer y un Animator por sapo, dos sistemas de partículas sin emisión continua y un material propio (con la textura del glob). Una consulta `Physics2D.OverlapBox` por paso de física (el cuerpo) y un `CircleCast` por glob en vuelo.

## Pendiente

- Colocarlo en una escena y probarlo a mano; ajustar `Mouth Offset` y la zona del cuerpo a ojo.
- **Esquiva:** con los valores del inventario el glob pasa a la altura de la cabeza de Alma y el salto normal no basta. Decidir si se baja el arco (por ejemplo `Spit Velocity` (8; 1,5)) para poder saltarlo, o si se deja para el doble salto.
- El sapo no tiene colisionador: Alma lo atraviesa (y muere). Si debe bloquear el paso, hay que añadirlo.
- Sin reacción a las habilidades de Alma (Pisotón, Rugido, Dash).
- El inventario pedía un pool de 4 globs; se crean y destruyen al vuelo (como los cristales del escarabajo).
