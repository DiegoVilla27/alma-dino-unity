# Planta carnívora — enemigo del Mundo 1

**Estado (5 de octubre de 2026):** implementada en Unity 6000.6.0f1 como prefab reutilizable, con animaciones Idle y Bite, detección de Alma, mordisco a izquierda o derecha y onda de impacto letal. Todavía no está colocada en ninguna escena ni probada en juego (ver [Pendiente](#pendiente)).

Ficha de diseño original: [inventario, enemigo 1](../INVENTARIO_GAMEPLAY_PREFABS.md#1-planta-carnívora). Jugador: [Alma](../Player/Alma.md).

## Qué es

Enemigo vegetal fijo de la Jungla. Descansa sin peligro y, cuando Alma entra en su zona, avisa, muerde hacia el lado donde está Alma y lanza por el suelo una onda de impacto que mata al pasar. Se esquiva leyendo el aviso: alejándose, saltando la onda o pasando al otro lado de la planta durante el aviso.

## Archivos

Todo está en `Assets/Prefabs/Enemies/Plant_Carnivorous_Jungle/`:

| Carpeta / archivo | Responsabilidad |
| --- | --- |
| `Plant_Carnivorous_Jungle.prefab` | Prefab reutilizable: SpriteRenderer, Animator y `CarnivorousPlant2D`. Sin colisionador propio. |
| `Scripts/CarnivorousPlant2D.cs` | Detección, ciclo de ataque, animación frame a frame, zonas letales, onda y aviso. Namespace `AlmaGame.Enemies` (ensamblado `Assembly-CSharp`). |
| `Animations/Idle/Plant_Carnivorous_Jungle_Idle_Sheet_0.controller` | Animator del prefab, con los estados `Idle` (por defecto) y `Bite`. |
| `Animations/Idle/Plant_Carnivorous_Jungle_Idle_Animation.anim` + `_Sheet.png` | Reposo: 8 frames a 12 fps, en bucle. |
| `Animations/Bite/Plant_Carnivorous_Jungle_Bite_Animation.anim` + `_Sheet.png` | Mordisco hacia la derecha: 8 frames. El script elige cada frame. |
| `Animations/Bite/Plant_Carnivorous_Jungle_Bite_Sheet_0.controller` | Generado por Unity al crear el clip; el prefab no lo usa. |
| `Sprites/Plant_Carnivorous_Jungle_Idle_Base.png` | Imagen de referencia (256×256, 100 px/unidad). |

## Cómo probarla

1. Arrastra el prefab a una escena con Alma. Colócala con la base de hojas apoyada en el suelo.
2. Selecciónala: en la vista Scene aparecen los gizmos (ver [Gizmos](#gizmos)). Comprueba que la base del rectángulo naranja coincide con el suelo; si no, ajusta `Ground Offset Y`.
3. Pulsa Play y acércate con Alma por cualquier lado.

Requisitos: una sola Alma en la escena (la planta la busca al empezar con `FindAnyObjectByType<AlmaMotor2D>()`), con su `Rigidbody2D` y colisionador, como en el prefab de Alma.

## Comportamiento

| Fase | Duración | Frames de Bite | Qué pasa | ¿Letal? |
| --- | --- | --- | --- | --- |
| **Reposo** | mínimo 1 s tras cada ataque | Animación Idle | Vigila la zona de detección. | No |
| **Aviso** | 0,25 s | 1 → 3 | Elige y **fija** el lado; gira y se echa hacia atrás; parpadea en rojo. | No |
| **Mordisco** | 0,7 s | 4 (0,04 s), luego 5 y 6 alternando cada 0,08 s | Se lanza y cierra la boca. Al cerrar sale la onda. | Sí: cabeza y frente de la onda |
| **Vuelta** | 0,2 s | 7 → 8 | Vuelve a mirar a la cámara. | No |

Ciclo completo: unos 2,15 s si Alma sigue dentro de la zona.

- **Elección del lado:** derecha si Alma está a su derecha y viceversa (signo de la diferencia en X). Si Alma está casi justo encima (menos de 0,1 unidades en X), repite el último lado. El lado no cambia hasta el siguiente ciclo.
- **Izquierda:** se reutiliza la hoja de la derecha con `SpriteRenderer.flipX`. La base está centrada en el frame, así que la planta no se desplaza al reflejarse.
- **Animación:** el script pone el Animator a velocidad 0 y llama a `Animator.Play("Bite", 0, tiempo)` en cada frame para mostrar el frame exacto de cada fase. Al terminar vuelve a `Idle` con velocidad 1. El Animator no tiene transiciones ni parámetros.
- **Cabeza letal:** durante el mordisco, desde el cierre de la boca, un rectángulo en la cabeza mata a Alma al tocarla.
- **Onda de impacto:** al cerrar la boca, su frente avanza por el suelo desde 1,1 unidades del centro hasta 3,3 (2,2 de recorrido) en 0,2 s, frenando al final. Mata en una franja de 1,2 unidades de alto desde el suelo. Entre pasos de física se comprueba todo el tramo recorrido, así que un frente rápido no puede «saltarse» a Alma.
- **Muerte:** al detectar a Alma en una zona letal se llama a `AlmaMotor2D.Die()` (se ignora si ya está muerta). La secuencia de muerte y la reaparición las gestiona Alma.

## Feedback visual (sin sonido)

- **Aviso:** parpadeo entre blanco y rojo claro (1; 0,6; 0,6) a unos 4 ciclos por segundo, junto con la pose de giro y tensión.
- **Onda:** tres arcos `)` de color naranja-rojizo (1; 0,5; 0,3) al 90 %. El primero marca **exactamente** el frente letal; los otros dos lo siguen con 0,05 s de retraso y menos opacidad (70 % y 40 %). Al detenerse se desvanecen en 0,15 s. Miden 0,6 unidades de ancho y la altura de la franja letal ×1,15.
- La textura del arco se genera una vez por código (64×128 px) y la comparten todas las plantas.

## Valores (`CarnivorousPlant2D` en el prefab)

Se editan en el Inspector del componente; cada instancia de la escena puede sobrescribirlos. Las distancias están en unidades y se multiplican por la escala del objeto.

| Campo | Valor | Uso |
| --- | ---: | --- |
| **Detección** | | |
| `Detect Range` | 3,3 | Distancia horizontal máxima, a cada lado, para empezar a morder. |
| `Detect Height` | −1,5 / 2,5 | Rango vertical respecto al centro de la planta en el que se detecta a Alma. |
| **Tiempos (s)** | | |
| `Windup Time` | 0,25 | Aviso. |
| `Bite Time` | 0,7 | Mordisco completo (incluye el lanzamiento). |
| `Return Time` | 0,2 | Vuelta a la pose de reposo. |
| `Rest Time` | 1 | Descanso mínimo entre mordiscos. |
| **Aviso** | | |
| `Warning Tint` | (1; 0,6; 0,6) | Color del parpadeo. |
| **Cabeza (mirando a la derecha)** | | |
| `Hitbox Offset` | (0,8; 0,4) | Centro de la zona letal de la cabeza respecto al centro de la planta. |
| `Hitbox Size` | (1,3; 1) | Tamaño de esa zona. |
| `Lunge Time` | 0,04 | Tiempo desde el inicio del mordisco hasta que la boca se cierra y empieza lo letal. |
| **Onda (mirando a la derecha)** | | |
| `Wave Origin X` | 1,1 | Punto de salida, a la altura de la boca. |
| `Wave Distance` | 2,2 | Recorrido del frente. |
| `Wave Time` | 0,2 | Tiempo del recorrido. |
| `Wave Height` | 1,2 | Altura de la franja letal desde el suelo. Con un salto normal (~1,56 m) se puede pasar por encima. |
| `Ground Offset Y` | −1,45 | Distancia del centro de la planta al suelo (estimada midiendo el dibujo). |
| `Wave Color` | (1; 0,5; 0,3; 0,9) | Color de los arcos. |

### Diferencias con el diseño original

| | Inventario | Implementado | Motivo |
| --- | --- | --- | --- |
| Reposo | 2 s | 1 s | Atacaba demasiado poco. |
| Aviso | 0,5 s | 0,25 s | Tardaba mucho en atacar. |
| Mordisco | 1,2 s | 0,7 s | Ciclo más ágil. |
| Reapertura | 0,2 s | 0,2 s | — |
| Alcance | Solo colisionador de la cabeza | Cabeza + onda hasta 3,3 u | El dibujo apenas avanza (la cabeza llega a ~1,4 u); había que meterse dentro de la planta para morir. |

## Animaciones y sprites

| Hoja | Formato | Uso |
| --- | --- | --- |
| Idle | 1024×512, 8 frames de 256×256, 80 px/unidad, pivote centrado | Reposo en bucle, mirando a la cámara. |
| Bite | 1024×512, 8 frames de 256×256, 80 px/unidad, pivote centrado | Mordisco hacia la derecha. 1: de frente · 2–3: aviso · 4: lanzamiento con boca abierta · 5–6: boca cerrada · 7–8: vuelta. |

Cada frame mide 3,2 × 3,2 unidades. Medido sobre el dibujo: la base de hojas llega a −1,49 unidades bajo el centro; la cabeza en reposo ocupa hasta ~0,97 unidades a cada lado; en el mordisco llega a ~1,3–1,5 unidades a la derecha.

Las nuevas hojas deben mantener el mismo formato (base centrada y en la misma posición en todos los frames, 80 px/unidad) para que el reflejo y las zonas letales sigan alineados.

## Gizmos

Al seleccionar la planta en la vista Scene:

- **Amarillo:** zona de detección.
- **Rojo (dos):** zona letal de la cabeza a cada lado.
- **Naranja (dos):** recorrido de la onda a cada lado, desde el suelo hasta `Wave Height`.

## Coste

Un SpriteRenderer y un Animator por planta, más tres sprites de arco (hijos de la planta, ocultos fuera del ataque). Una consulta `Physics2D.OverlapBox` por paso de física durante el mordisco (dos mientras avanza la onda). Ninguna fuera del ataque.

## Pendiente

- Colocarla en `Level_1_1` u otra escena y probarla; ajustar `Ground Offset Y` y la zona de la cabeza a ojo.
- **La onda atraviesa paredes y huecos:** no comprueba si hay suelo o un muro delante. Colocar la planta en tramos de suelo plano de al menos 3,3 unidades a cada lado, o añadir esa comprobación.
- La planta no tiene colisionador: Alma la atraviesa en reposo. Si debe bloquear el paso o poder pisarse, hay que añadirlo.
- Pedir una hoja Bite con frames más anchos (512×256) donde el tallo se estire de verdad, para que el alcance visual de la cabeza acompañe a la onda.
- Busca a Alma una sola vez al empezar: si Alma se crea después que la planta, no la detectará.
- Sin reacción a las habilidades de Alma (por ejemplo, aturdirla con el Rugido o el Pisotón).
