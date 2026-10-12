> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_01/Boss_1.unity` como escena de trabajo con el fondo de la arena ([`Backdrop_Boss_1`](../../LevelPieces/Backgrounds.md#jefe-1--world_1backdrop_boss_1prefab): la guarida del mono sobre un abismo de nubes al atardecer, con una tormenta que avanza con las fases del combate); la arena y el jefe aún no están construidos. Las referencias de cámara a **8** son el criterio de la reconstrucción. No habrá audio.

# 👑 Jefe 1: "El Rey de la Copa — Mono Ladrón Gigante"
> **Mundo 1: Jungla Esmeralda** | **Arena de Combate y Cierre del Mundo 1**

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Con el primer huevo asegurado en la espalda de Alma, el titán de la copa del árbol desciende enfurecido. Es el líder de los ladrones: un primate colosal, ágil y astuto que ha acumulado reliquias robadas en su guarida. El simio no permitirá que Alma descienda de vuelta hacia la tierra con su cría.
- **Estado Emocional de Alma:** Determinación defensiva. Alma ya no solo lucha por recuperar a su hijo: ahora lucha por protegerlo físicamente a sus espaldas mientras esquiva los ataques del titán.
- **Textos en Pantalla / Banners:**
  - *Inicio del Combate:*
    > *"¡JEFE DE MUNDO: MONO LADRÓN GIGANTE!*  
    > *Esquiva los proyectiles con tu aleteo y aprovecha el momento en que quede exhausto."*
  - *Victoria:*
    > *"El gran simio huye hacia el abismo de las cuevas.*  
    > *El rastro de tus otros tres hijos desciende hacia las profundidades de la tierra.*  
    > *(Mundo 1: Jungla Esmeralda Completado)"*

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **La Arena de Combate:**
  - Una plataforma central ancha de madera maciza y dos ramas laterales flotantes elevadas. Debajo de la arena solo hay caída libre hacia las nubes (abismo).
  - El fondo muestra una tormenta tropical distante que tiñe el cielo de tonos dorados y violetas al atardecer.
- **Paleta de Color Principal:**
  - Arena: Madera de teca oscura (`#3E2723`) con lianas y ramas decorativas.
  - Jefe: Pelaje marrón oscuro con una melena plateada y ojos ámbar amenazantes (`#FF9800`).
  - Proyectiles (Frutos Gigantes): Cocos prehistóricos con espinas de color verde lima (`#76FF03`) y púrpura.
- **Iluminación 2D (URP):**
  - Efectos de relámpagos lejanos en el fondo que iluminan momentáneamente la silueta del jefe.

---

## 3. 🧱 Mecánicas de Combate & Fases del Jefe (Puzle de Habilidad y Reflejos)

Siguiendo la filosofía del GDD, el jefe es un **puzle de habilidad y timing**, no un combate con barra de vida de desgaste:

- **Estructura: 3 Impactos para Vencer (3 Ciclos con Dificultad Creciente)**:
  - **Fase 1 (El Asedio Ágil):**
    - 4 proyectiles por oleada (Velocidad: 6.5 m/s, Intervalo: 1.3s).
    - El simio lanza frutos rodantes por la plataforma central y realiza tiros parabólicos/rebotantes anti-camping hacia las ramas laterales si Alma intenta esconderse en ellas.
    - Fatiga / Vulnerabilidad: **`3.2 segundos`**.
  - **Fase 2 (Furia del Simio y Balanceo Aéreo):**
    - 5 proyectiles por oleada (Velocidad: 8.0 m/s, Intervalo: 1.0s).
    - El simio se balancea velozmente entre 3 lianas superiores (Izquierda `-4.5m`, Centro `0.0m`, Derecha `+4.5m`) alternando ángulos de tiro cruzados.
    - Combina frutos rodantes con cocos rebotantes de parábola alta que rebotan en el suelo y cubren ambas plataformas.
    - Fatiga / Vulnerabilidad: **`2.8 segundos`**.
  - **Fase 3 (Enfurecimiento Total — Modo Frenesí):**
    - 7 proyectiles en ráfagas rápidas coordinadas (Velocidad: 9.5 m/s, Intervalo: 0.75s).
    - Ráfagas dobles simultáneas (fruto rodante por el suelo + aéreo parabólico por arriba, o lanzamientos simultáneos hacia ambos lados).
    - Fatiga / Vulnerabilidad: **`2.4 segundos`**. Exige rapidez para trepar a la rama lateral y caer con precisión sobre su cabeza.
- **Peligro Corporal & Puntos Vulnerables:**
  - El torso y extremidades del simio poseen `BossBodyHazard2D`: el contacto horizontal con su cuerpo es letal.
  - Solo su cabeza coronada (`BossHeadHurtbox2D`) es vulnerable cuando el mono cae al suelo en fatiga, premiando a Alma con un rebote de `13.5` de fuerza (`ApplyBounce`).
- **Condición de Derrota de Alma:**
  - Tocar los frutos o el cuerpo del simio devuelve a Alma al inicio de la fase actual de la arena (checkpoints guardados tras cada impacto exitoso para máxima justicia y cero frustración injusta).

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [x] **Mecánicas & FSM Jefe:** Controlador modular `GiantMonkeyBoss2D` con 3 fases, lanzamiento de proyectiles, fatiga de 3.2/2.8/2.4s y derrota.
- [x] **Sprites & Prefab Proyectil:** `RollingFruitProjectile2D` con giro continuo y peligro letal `IHazard2D`.
- [x] **Hurtbox de Cabeza:** `BossHeadHurtbox2D` para rebote satisfactorio de Alma (`ApplyBounce`) al pisar la cabeza vulnerable.
- [x] **VFX & Cámara:** Screen Shake mediante `CameraShakeEventChannelSO`, encuadre cinemático de arena y atardecer crepuscular en luz 2D.
- [ ] **Sprites finales:** animaciones legibles de cada fase.

## Implementación (10/10/2026)

`Assets/Prefabs/Bosses/GiantMonkey/Boss_GiantMonkey_Jungle.prefab` (`GiantMonkeyBoss2D` + `MonkeyProjectile2D`), colocado en `Boss_1`. El sistema antiguo por piezas (cut-out) solo se usa si no hay arte; el combate actual funciona con `Fight Enabled` desactivado.

### Animaciones (frames de 256×256, hojas de 1024×512, 45 px por unidad, 12 fps)

Todas están en el Animator `Animations/Taunt/Boss_GiantMonkey.controller`.

| Estado | Origen | Apoyo | Uso |
| --- | --- | --- | --- |
| `Boss_GiantMonkey_Taunt` | Higgsfield | pies | Burla de la intro; su frame 1 se usa en la derrota |
| `Boss_GiantMonkey_Hang` | Código (1 frame rotado + respiración), velocidad 0,5 | puño | Colgado de la liana |
| `Boss_GiantMonkey_Throw` | Higgsfield (2 brazos) | puño | Lanzar el fruto (lo suelta en el frame 5) |
| `Boss_GiantMonkey_Tired` | Higgsfield | pies | Agotado en el suelo, jadeando |
| `Boss_GiantMonkey_Swing` | Referencia del usuario (`salto.png`), frame 3 reflejado; hoja de 1024×256, 4 frames, 43,5 px por unidad | puño del frame 1 | Salto entre lianas: se suelta, se estira, vuela y se agarra; el frame va con el avance del salto y se refleja según la dirección |

Colgado y lanzando se dibujan a escala `Hang Scale` (1,3), anclados en el puño; el punto de suelta del fruto escala igual. Cada fase va un `Phase Speed Up` (15 %) más rápida: animaciones, lanzamiento, salto entre lianas, caída y regreso.

Por código, sin animación propia: caída, golpe recibido (destello rojo y temblor), esfumarse como un ninja (humo) y estrellas de mareo.

### Secuencia

1. **Intro:** está en el suelo frente a Alma, que no puede moverse. La cámara lo enfoca, dice sus frases, hace la burla, se esfuma y baja colgado por la liana central. Aparece la tarjeta de jefe (`BannerKind.BossIntro`).
2. **Oleada:** 2 s después de devolver el control, lanza frutos con la animación de lanzar. Antes de cada lanzamiento **simula** el recorrido con las colisiones reales y elige el primero que alcance a Alma:
   - **normal:** lo deja caer y rueda;
   - **bajo:** cae corto y rueda por debajo de las plataformas;
   - **alto:** parábola directa a Alma.

   Si desde su liana ninguno la alcanza, **salta** a la liana desde la que sí.
3. **Agotado** (aviso «¡AGOTADO!» y la insignia de [`HudBossAlert`](../../Systems/HUD.md#aviso-de-jefe-vulnerable-hudbossalert) si está fuera de cámara): tras lanzar los frutos de la fase, se suelta, cae a su vertical con polvo y temblor de cámara, y jadea con estrellas girando. Es inofensivo.
4. **Golpe:** si Alma cae sobre su cabeza (la altura de la cabeza sigue la respiración, `Tired Head Top`), rebota (13,5), el mono parpadea en rojo y tiembla. Después se esfuma y reaparece colgado desde arriba en la liana central: **empieza la fase siguiente**.
5. Si se acaba el tiempo de agotado sin golpe, se esfuma y repite la misma fase.
6. **Derrota (3.er golpe):** Alma se congela, la cámara lo enfoca, mira a Alma, dice `Defeat Lines` y se esfuma. Aparece el **portal al Mundo 2** (`ExitPortal_World2` en la escena, `Next Scene` = `Level_2_1`) en `Portal X`, y Alma recupera el control.

### Fases (`Cycles`)

| Fase | Frutos | Velocidad | Intervalo | Agotado |
| --- | --- | --- | --- | --- |
| 1 | 4 | 6,5 | 1,3 s | 3,2 s |
| 2 | 5 | 8 | 1 s | 2,8 s |
| 3 | 7 | 9,5 | 0,75 s | 2,4 s |

### El fruto

Tiene un collider circular (trigger). Si toca a Alma, ella muere y el fruto estalla (pulpa, cáscara y destello). Si no, estalla al chocar con un collider sólido de lado (los troncos de los extremos). Al caer al suelo rueda, no estalla. Si Alma muere, se borran los frutos en pantalla; la oleada sigue contando (no se reinicia) y los golpes se conservan.

### Pendiente

- Marcas de progreso en el HUD.
- Coco (no tiene arte).


## Huevo: «El Reencuentro» (11-12/10/2026)

Cuando termina su discurso y desaparece o muere (en el jefe final, cuando la estalactita queda en el suelo), un destello lleva a la sala del **Huevo Verde**: el prefab `Prefabs/Level/Progression/EggRooms/EggRoom_World1_Green` (campo `Reunion Room` del jefe; contenido hecho a mano). Al volver se abre el portal y el huevo vuela a su hueco del HUD. Campos del jefe: `Egg Id`, `Reunion Room`. Ver [HUD](../../Systems/HUD.md#el-reencuentro-hudeggreunion).

**Portal de salida:** aparece en (`Portal X`, `Portal Y`) = (0; −3,61) (12/10/2026; antes se calculaba sobre el suelo).

**Globo de diálogo (12/10/2026):** anclado a un punto vacío a `Bubble Height` (4,2) sobre sus pies, justo encima de su cabeza en el primer frame del Taunt (con el que habla); antes se ponía sobre el borde del fotograma completo y salía 1,4 u más alto.

**Aviso de jefe vulnerable (12/10/2026):** cuando cae de la liana y queda agotado en el suelo (estado `Exhausted`, la ventana para pisarle la cabeza), aparecen el grito «¡AGOTADO!» (`Tired Shout`), la insignia del borde de `HudBossAlert` que apunta a su cabeza y se vacía con el tiempo de la ventana (`Fatigue` del ciclo), y tres estrellas girando sobre su cabeza; al levantarse o recibir el golpe, desaparecen. Antes solo estaba conectado al ciclo antiguo (`Tired`). Prueba `MonkeyAlertTests`.
