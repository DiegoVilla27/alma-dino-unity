# 🗺️ Nivel 2-2: "La Galería de Ecos"
> **Mundo 2: Cuevas de Cristal** | **Función Pedagógica:** Practicar (Balancines de Piedra, Catapultas & Pisotón Sísmico)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Alma se adentra en una inmensa cámara subterránea donde los techos alcanzan alturas vertiginosas. Los ecos de sus pisadas retumban en la distancia, pero entre ellos distingue un sonido sutil: un crujido de cristal que revela que los ladrones estuvieron aquí activando antiguos mecanismos de roca.
- **Estado Emocional de Alma:** Curiosidad analítica y cálculo. La fuerza bruta no basta; debe aplicar su peso con precisión quirúrgica en el momento y lugar indicados.
- **Pistas Narrativas en el Entorno:** Marcas circulares en el suelo donde los ladrones han rodado objetos pesados, cristales partidos recientemente y gotas de savia de la jungla que gotean desde fisuras del techo.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un templo natural de roca y cristal con enormes losas de basalto en equilibrio sobre pivotes de piedra pulida. Grandes geodas moradas y verdes actúan como lámparas orgánicas en los laterales de la caverna.
- **Paleta de Color Principal:**
  - Roca Cavernosa: Gris grafito (`#2B2D42`) y azul medianoche (`#1D3557`).
  - Balancines Mecánicos: Basalto pulido oscuro (`#111118`) con marcas rúnicas amarillas (`#E9D8A6`).
  - Cristales de Iluminación: Esmeralda cristalina (`#52B788`) y ámbar brillante (`#EE9B00`).
- **Fondos Parallax (4 Capas):**
  - *Capa 0 (Fondo Lejano):* Galería cavernosa infinita que se pierde en la negrura.
  - *Capa 1 (Fondo Medio):* Estalagmitas gigantescas y puentes naturales de roca en silueta.
  - *Capa 2 (Fondo Cercano / Gameplay):* Los balancines basculantes, bloques contrapeso y compuertas de piedra.
  - *Capa 3 (Primer Plano / Foreground):* Enredaderas subterráneas pálidas que cruzan la pantalla verticalmente.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico.
- **Mecánica Estrella: Los Balancines de Basalto (`SeesawPlatform2D`):**
  - Plataformas largas de piedra apoyadas sobre un fulcro central.
  - Al caminar sobre un extremo, bascula lentamente.
  - **Interacción con Pisotón Sísmico:**
    - Si Alma salta alto y ejecuta un **Pisotón Sísmico** sobre un extremo del balancín:
      1. El extremo impactado desciende de golpe contra el suelo.
      2. El extremo opuesto sale catapultado violentamente hacia arriba.
      3. Si sobre el extremo opuesto hay un bloque de piedra, este es lanzado al aire para golpear un interruptor en el techo o romper un techo frágil.
      4. Si Alma corre hacia el extremo elevado, puede ser catapultada a alturas imposibles para un salto normal.
- **Compuertas Rúnicas Temporizadas:**
  - Interruptores de peso que se abren durante 4 segundos al ser golpeados por los bloques catapultados.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 30.0`):* Tras dominar el primer puzle de balancín simple.
  - *Checkpoint 2 (`X = 65.0`):* Antes del gran balancín doble encadenado.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Resonance of Stone" (Resonancia de Piedra).
  - *Estilo:* Percusión con piedras talladas (litófonos) y campanas tibetanas con reverberación masiva, entrelazadas con un violonchelo melancólico que marca un compás constante de puzle.
  - *Tempo:* 78 BPM. Ritmo pausado y contemplativo.
- **Efectos de Sonido (SFX):**
  - Balancín: Chirrido pesado de piedra caliza basculando sobre piedra (*grrrk-clunk*).
  - Catapulta de Bloque: Lanzamiento con chasquido de aire y golpe en seco contra el interruptor superior.
  - Compuerta: Retumbar de engranajes rústicos de piedra abriéndose.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Balancín de basalto con pivote central, bloque cúbico de piedra contrapeso, interruptor de techo, compuerta rúnica deslizante.
- [ ] **VFX:** Polvo de tiza y astillas de piedra despedidas al bascular con violencia; brillo rúnico al activar el interruptor.
- [ ] **Audio:** Pista "Resonance of Stone", SFX de basculación de roca, SFX de lanzamiento por catapulta.
