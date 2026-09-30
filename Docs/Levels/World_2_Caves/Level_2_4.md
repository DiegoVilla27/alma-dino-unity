# 🗺️ Nivel 2-4: "El Laberinto de Geodas"
> **Mundo 2: Cuevas de Cristal** | **Función Pedagógica:** Evaluar + Segundo Rescate (El Techo Móvil & Huevo Azul)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En lo más profundo de las cuevas, Alma alcanza la Gran Geoda Sagrada. La caverna es colosal pero inestable: el techo de estalactitas gigantescas vibra con temblores rítmicos que amenazan con aplastar todo a su paso. En el centro de un pedestal de cristal puro descansa el segundo huevo robado: el **Huevo Azul**, emitiendo un fulgor sereno y frío como el hielo.
- **Estado Emocional de Alma:** Una combinación de angustia por el peligro inminente del colapso y una infinita ternura al reconocer la segunda vida que late ante ella.
- **El Momento del Rescate (Cinemática Diegética en Gameplay):**
  - Alma alcanza el pedestal y toca el **Huevo Azul**.
  - La música de persecución se desvanece en un instante; el piano acústico retoma el motivo de la "Nana Maternal":
  - *Texto del Rescate (Huevo Azul):*
    > *"Sentí tu latido contra la piedra fría.*  
    > *Ya somos dos. No descansaré hasta que estemos los cinco juntos.*  
    > *(Dos de cuatro rescatados)"*
  - De pronto, un estruendo brutal agrieta la caverna: una gigantesca mole acorazada surge rompiendo la pared de roca: el **Armadillo Prehistórico** bloquea la salida hacia la superficie.
- **Pistas Narrativas en el Entorno:** Fragmentos de cristal azul con huellas de cría, estalagmitas rotas por el peso del huevo, y el pulso auditivo del latido del huevo azul.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** La caverna de cristal más hermosa y peligrosa del juego. Paredes enteras de geodas amatista y zafiro que reflejan la luz como prismas gigantes. El techo está cubierto de estalactitas masivas que descienden y ascienden con un ritmo amenazante.
- **Paleta de Color Principal:**
  - Cristal Principal: Zafiro profundo (`#0077B6`) y azul hielo brillante (`#90E0EF`).
  - Huevo Azul: Resplandor celeste celestial (`#48CAE4`) con halo palpitante.
  - Techo de Amenaza: Roca volcánica oscura (`#1B1B1E`) con espinas de pedernal.
- **Iluminación 2D (URP):**
  - Luces volumétricas tenues que se filtran a través de los cristales gigantes, creando un ambiente etéreo casi submarino.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base completo + Doble Salto + Pisotón Sísmico.
- **Objetivo de Diseño: El Examen Maestro del Mundo 2**:
  1. *El Techo de Estalactitas Móvil (`CrushingCeiling2D`):*
     - Secciones del techo descienden pesadamente cada 3.0 segundos y tardan 2.0 segundos en retraerse.
     - Obliga a Alma a avanzar con velocidad y precisión, calculando cuándo correr y cuándo usar el **Pisotón Sísmico** para romper un suelo agrietado y refugiarse en un hueco inferior seguro antes del impacto del techo.
  2. *Balancines bajo Presión:*
     - Balancines que deben ser golpeados con el Pisotón mientras el techo desciende, catapultando a Alma justo a tiempo hacia la siguiente cámara.
  3. *Bichos Acorazados como Plataformas de Emergencia:*
     - Voltear escarabajos sobre la marcha para usarlos de plataforma mientras se esquivan estalactitas que caen.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 32.0`):* Tras la primera cámara de techo móvil.
  - *Checkpoint 2 (`X = 68.0`):* Justo antes de la antecámara del Huevo Azul.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Labyrinth of the Sapphires" (El Laberinto de los Zafiros).
  - *Estilo:* Cuerdas frotadas con tensión en crescendo, percusión de tambores orquestales profundos y un xilófono cristalino que marca un compás apremiante.
  - *Tempo:* 120 BPM.
  - *Tema de Rescate ("Lullaby for the Blue Shell"):* Piano solo lento y suave a 58 BPM, con notas agudas que evocan la tranquilidad del hielo.
- **Efectos de Sonido (SFX):**
  - Descenso del Techo: Retumbar de toneladas de roca frotando contra las paredes (*THUUUUUM*).
  - Latido del Huevo Azul: Pulso cristalino con armónicos agudos sutiles.
  - Rugido del Armadillo: Bramido gutural cavernoso al romper la pared al final del nivel.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Pedestal de geoda central, Huevo Azul con halo de luz 2D, techo aplastador con estalactitas afiladas.
- [ ] **VFX:** Polvo de cristal cayendo del techo antes de descender; partículas de destellos azulados flotantes.
- [ ] **Audio:** Pista "Labyrinth of the Sapphires", tema de rescate del Huevo Azul, SFX de terremoto y derrumbe de pared.
