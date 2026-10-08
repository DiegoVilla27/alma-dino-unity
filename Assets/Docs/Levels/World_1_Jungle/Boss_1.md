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
