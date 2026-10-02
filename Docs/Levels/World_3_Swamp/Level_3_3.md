# 🗺️ Nivel 3-3: "El Vuelo de las Esporas"
> **Mundo 3: Pantano de Viento y Niebla** | **Función Pedagógica:** Complicar (Globos de Esporas Recargables & Sapos Venenosos)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Alma llega al corazón del pantano, donde enormes hongos flotantes desprenden globos de esporas luminiscentes que flotan suspendidos sobre un abismo de aguas profundas y mortales. Sapos venenosos gigantescos anidan en las pocas raíces secas y escupen burbujas de gas cáustico.
- **Estado Emocional de Alma:** Fluidez y reflejos rápidos. Los lagos alternan con refugios de tierra firme. Las esporas siguen líneas horizontales; los obstáculos de tierra recuperan el Pisotón y el Dash.
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
- **Mecánica Estrella: Globos de Esporas (`DashRefillPickup2D`):**
  - Orbes vegetales flotantes suspendidos en el aire sobre el abismo.
  - **Recarga Aérea Instantánea:** Al tocar un globo disponible en el aire (incluido durante un **Dash Aéreo**):
    1. El globo estalla en un destello de polen bioluminiscente.
    2. El Dash Aéreo y el Doble Salto de Alma **se reinician instantáneamente en pleno vuelo** sin necesidad de tocar tierra.
    3. Permite encadenar *Dash ➔ Recarga ➔ Dash* hacia la derecha, sin subir, bajar ni retroceder entre esporas.
    4. El globo reaparece a los 2.5 segundos.
- **Enemigos Activos:**
  - *Sapo Venenoso (`PoisonToad2D`):* Dispara proyectiles parabólicos de lodo venenoso a intervalos de 2.0 segundos. Obliga a sincronizar el dash aéreo para atravesar las esporas sin colisionar con el vómito tóxico.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 28.0`):* Isla de raíces secas tras la primera cadena de esporas.
  - *Checkpoint 2 (`X = 76.0`):* Antes del gran salto encadenado de 4 esporas en el aire.

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

---

## 6. Recorrido del prototipo

- Escena: `Assets/Scenes/World_3_Swamp/Level_3_3.unity`, conectada desde 3-2. Salida a `Level_3_4`, todavía pendiente; el Huevo Morado se rescata allí.
- Menús: **Alma → 📂 Cargar Nivel 3-3** y **Tools → Alma → Construir Nivel 3-3 - El Vuelo de las Esporas**.
- Cámara size 6 y anticipo direccional; pared detrás del inicio. Doble Salto, Pisotón y Dash disponibles; Rugido bloqueado.

### Retos por sección

1. **Esporas horizontales:** lago X=10–26, con esporas en (14, 2.7) y (20, 2.7). Salto → Doble Salto → Dash y continuar con Dash recargado hacia la derecha. Llegada a tierra a la misma altura.
2. **Pisotón sobre tierra firme:** checkpoint X=28. El suelo agrietado X=30–34 soporta a Alma hasta romperlo con Salto → POUND. La raíz X=34–39 bloquea el camino superior; el pasaje seguro tiene suelo en Y=-3. Un Doble Salto al salir del túnel permite alcanzar el escalón seguro de X=41 (Y=-1). La cobertura X=42 protege mientras se prepara el salto sobre el sapo hacia la siguiente cadena. Un sapo en X=43.5 dispara hacia la izquierda al salir: hay que saltar su cuerpo y esquivar sus disparos.
3. **Esporas y barrera de Dash:** lago X=46–62, esporas en (50, 2.7) y (56, 2.7). Barrera de cañas X=65 sobre suelo firme: se rompe con Dash. Sapo en X=72.5, disparando hacia la derecha; raíz protectora X=74.5 y checkpoint X=76.
4. **Cadena horizontal final:** lago X=82–110, con esporas en (86, 2.7), (92, 2.7), (98, 2.7) y (104, 2.7). Todas se atraviesan avanzando hacia la derecha y repitiendo Dash. Llegada despejada a tierra firme; portal en X=122.

### Sapos, recargas y reintentos

- Dos sapos al nivel del camino, peligrosos por contacto. Aviso `!` de 0.6s antes de cada disparo, cadencia de 2s y detección de 18m. Burbujas a 8m/s, impulso vertical 3m/s y gravedad 0.6; Dash no protege del veneno.
- Ocho esporas alineadas a Y=2.7 restauran Dash y Doble Salto, anulan el cooldown y reaparecen a los 2.5s.
- Morir restaura esporas y suelo agrietado, limpia proyectiles y reinicia los sapos. La barrera de Dash queda abierta una vez rota.
- Los avisos de Pisotón y cañas aparecen al llegar a cada obstáculo.
- Se reutilizan `BreakableGround2D`, `BreakableGroundRespawnReset2D` y `DashBreakableBarrier2D`; no cambia el movimiento del jugador.
- Arte geométrico y parallax provisionales; sprites, partículas y audio definitivos pendientes.

### Validación

**11 pruebas PlayMode pasan**, incluido el recorrido completo sin muertes. Cubren recorrido completo con entradas reales, cadenas horizontales, bloqueo de la raíz y apertura con Pisotón, barrera con Dash, ataques reales, seguridad de checkpoint, recargas y reinicio tras morir.
