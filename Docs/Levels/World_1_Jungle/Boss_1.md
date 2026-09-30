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

## 3. 🧱 Mecánicas de Combate & Fases del Jefe (Puzle de Habilidad)

Siguiendo la filosofía del GDD, el jefe es un **puzle de habilidad y timing**, no un combate con barra de vida de desgaste:

- **Estructura: 3 Impactos para Vencer (3 Ciclos)**:
  - **Fase 1 (El Asedio de Frutos):**
    - El mono se cuelga de una liana central inalcanzable en el aire.
    - Lanza frutos rodantes gigantescos que recorren la plataforma inferior rebotando rítmicamente.
    - El jugador debe usar el **Doble Salto** para saltar sobre los frutos o trepar a las ramas laterales para esquivarlos.
  - **Fase 2 (La Fatiga / Vulnerabilidad):**
    - Tras lanzar 3-4 frutos, el simio se agota y desciende pesadamente sobre la plataforma central, quedando aturdido durante **`4.0 segundos`**.
    - Alma debe subir a una plataforma elevada y ejecutar un salto preciso cayendo con un pisotón normal sobre su cabeza coronada.
  - **Fase 3 (Enfurecimiento):**
    - Tras cada impacto, el simio ruge, entra en cólera y aumenta la velocidad de lanzamiento de frutos (+20%).
    - En el último ciclo (tercer impacto), lanza dos frutos consecutivos que obligan a realizar una parábola perfecta en el aire.
- **Condición de Derrota de Alma:**
  - Tocar los frutos o el cuerpo del simio mientras ataca devuelve a Alma al inicio de la fase actual de la arena (checkpoints por fase de impacto para máxima justicia).

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Wrath of the Canopy King" (La Ira del Rey de la Copa).
  - *Estilo:* Percusión tribal intensa y frenética (taikos, djembes y congas aceleradas) con metales de corte prehistórico y coros graves que marcan la tensión del combate.
  - *Tempo:* 135 BPM. Pacing de urgencia y acción rítmica.
- **Efectos de Sonido (SFX):**
  - Impacto sobre la cabeza del jefe: Golpe sordo con eco y temblor de pantalla (*hit stop* de 0.08s).
  - Chillido de dolor del simio y aullido de retirada.
  - Rodar de frutos: Tronar pesado sobre la corteza de la plataforma.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Jefe:** Sprite sheet del Mono Gigante (Idle colgado, animación de lanzamiento de frutos, aturdimiento en el suelo, animación de daño y huida).
- [ ] **Sprites Proyectiles:** Fruto espinoso rodante con efecto de giro y partículas de impacto.
- [ ] **VFX:** Screen Shake acentuado, partículas de hojas desprendidas en cada rugido del jefe, destello de impacto en la cabeza.
- [ ] **Audio:** Pista de combate "Wrath of the Canopy King", rugido del simio gigante, SFX de lanzamiento y rebote de cocos.
