# 🗺️ Nivel 3-3: "El Vuelo de las Esporas"
> **Mundo 3: Pantano de Viento y Niebla** | **Función Pedagógica:** Complicar (Globos de Esporas Recargables & Sapos Venenosos)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Alma llega al corazón del pantano, donde enormes hongos flotantes desprenden globos de esporas luminiscentes que flotan suspendidos sobre un abismo de aguas profundas y mortales. Sapos venenosos gigantescos anidan en las pocas raíces secas y escupen burbujas de gas cáustico.
- **Estado Emocional de Alma:** Fluidez y reflejos rápidos. No hay suelo donde detenerse; la supervivencia depende de encadenar impulsos en el aire sin tocar tierra durante tramos enteros.
- **Pistas Narrativas en el Entorno:** Huellas de mono en las esporas explotadas, ramas de sauce arrancadas y el resplandor morado del tercer huevo visible al final del horizonte.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un lago pantanoso infinito cubierto de niebla púrpura y verde. Globos de esporas flotantes que brillan como linternas en el aire, liberando polvo bioluminiscente al ser atravesados.
- **Paleta de Color Principal:**
  - Globos de Esporas: Verde lima fosforescente (`#70E000`) y esporas cian (`#38B000`).
  - Agua Pantanosa: Negro petróleo (`#0A0908`) con reflejos verdosos.
  - Sapos Venenosos: Púrpura verrugoso (`#5A189A`) con manchas amarillas.
- **Iluminación 2D (URP):**
  - Cada globo de esporas proyecta una luz 2D pulsante suave.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo.
- **Mecánica Estrella: Globos de Esporas (`SporeRefill2D`):**
  - Orbes vegetales flotantes suspendidos en el aire sobre el abismo.
  - **Recarga Aérea Instantánea:** Al atravesar un globo de esporas mediante un **Dash Aéreo**:
    1. El globo estalla en un destello de polen bioluminiscente.
    2. El Dash Aéreo y el Doble Salto de Alma **se reinician instantáneamente en pleno vuelo** sin necesidad de tocar tierra.
    3. Permite encadenar secuencias acrobáticas: *Dash ➔ Recarga ➔ Doble Salto ➔ Dash ➔ Recarga*.
    4. El globo reaparece a los 2.5 segundos.
- **Enemigos Activos:**
  - *Sapo Venenoso (`PoisonToad2D`):* Dispara proyectiles parabólicos de lodo venenoso a intervalos de 2.0 segundos. Obliga a sincronizar el dash aéreo para atravesar las esporas sin colisionar con el vómito tóxico.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 30.0`):* Isla de raíces secas tras la primera cadena de esporas.
  - *Checkpoint 2 (`X = 66.0`):* Antes del gran salto encadenado de 4 esporas en el aire.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Dance of the Spores" (La Danza de las Esporas).
  - *Estilo:* Fusión de percusión orgánica ligera con sintetizadores arpegiados rápidos que suben de tono con cada rebote/recarga aérea, creando una sensación de ingravidez y dinamismo.
  - *Tempo:* 128 BPM. Pacing de acrobacia aérea continua.
- **Efectos de Sonido (SFX):**
  - Estallido de Espora: Campanilleo cristalino agudo (*¡CHIME-POP!*) con eco estéreo que confirma la recarga de habilidades.
  - Disparo de Sapo: Glu-glu gutural seguido por el silbido del proyectil tóxico.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Globo de esporas flotante (reposo, pulso, explosión de partículas, regeneración).
- [ ] **Sprites Enemigos:** Sapo venenoso (respiración hinchada, disparo parabólico).
- [ ] **VFX:** Explosión circular de esporas verdes al atravesar el globo; destello brillante en las plumas de Alma al recargar el Dash.
- [ ] **Audio:** Pista "Dance of the Spores", SFX de recarga de espora, SFX de disparo del sapo.
