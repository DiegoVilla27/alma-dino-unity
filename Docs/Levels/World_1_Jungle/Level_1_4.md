# 🗺️ Nivel 1-4: "La Copa del Gran Árbol"
> **Mundo 1: Jungla Esmeralda** | **Función Pedagógica:** Evaluar + Primer Rescate (Ascenso Maestro & Huevo Verde)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Alma ha superado las zarzas y se encuentra en el tronco madre del Gran Árbol Sagrado. La niebla se disipa y la luz del mediodía inunda las ramas más altas. En la cima, protegido por el nido del gran simio, yace el primer huevo robado: el **Huevo Verde**, cuya cáscara pulsa con un tenue latido cálido.
- **Estado Emocional de Alma:** Una mezcla arrolladora de alivio, adrenalina y ferocidad protectora. Su primer hijo está al alcance de sus manos; nada en la jungla la detendrá ahora.
- **El Momento del Rescate (Cinemática Diegética en Gameplay):**
  - Al final del nivel, Alma alcanza una cuna de plumas gigantes.
  - Al tocar el **Huevo Verde**, la música tensa de plataformas cesa abruptamente. Se hace un silencio sagrado, roto únicamente por una melodía suave de piano solo (el tema de la "Nana Maternal"):
  - *Texto del Rescate (Huevo Verde):*
    > *"Aún estás tibio...*  
    > *Mamá llegó a tiempo. Ya estás a salvo.*  
    > *(Uno de cuatro rescatados)*"
  - Inmediatamente después, un rugido colosal sacude la copa: el **Mono Ladrón Gigante** desciende de las alturas bloqueando el paso hacia el siguiente mundo.
- **Pistas Narrativas en el Entorno:** Plumas doradas y verdes de cría dinosaurio, cáscaras de frutos gigantes partidos por la mitad, y el latido del huevo audible como un pulso cardíaco rítmico que se hace más fuerte al acercarse.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** La majestuosidad de la cima del dosel arbóreo por encima de las nubes. Mar de hojas verdes y doradas a los pies del jugador, ramas gigantescas bañadas por un sol radiante de mediodía y una brisa constante que mece el follaje.
- **Paleta de Color Principal:**
  - Cielo: Azul zafiro brillante (`#3A86FF`) con nubes blancas algodonosas (`#F8F9FA`).
  - Vegetación de Copa: Verde esmeralda resplandeciente (`#2EC4B6`) y hojas doradas (`#FFB703`).
  - Nido del Rescate: Fibras vegetales doradas y plumas protectoras de Alma.
  - Huevo Verde: Esmeralda bioluminiscente (`#00F5D4`) con un halo palpitante.
- **Fondos Parallax (4 Capas):**
  - *Capa 0 (Fondo Lejano):* Mar de nubes blancas en movimiento horizontal continuo bajo un cielo abierto infinito.
  - *Capa 1 (Fondo Medio):* Copas de árboles lejanos que sobresalen de las nubes como islas verdes.
  - *Capa 2 (Fondo Cercano / Gameplay):* Las ramas más gruesas del árbol, hojas doradas, hongos elásticos de copa alta.
  - *Capa 3 (Primer Plano / Foreground):* Hojas y lianas que se balancean con el viento cruzando la pantalla.
- **Iluminación 2D (URP):**
  - Luz Global intensa y cálida (intensidad 1.1).
  - Efecto de destello de lente solar (*lens flare*) sutil en la esquina superior.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base completo + **Doble Salto (Aleteo Materno)** llevado a su máxima expresión.
- **Objetivo de Diseño: El Examen Maestro del Mundo 1**:
  - El nivel evalúa la síntesis fluida de todas las mecánicas aprendidas en 1-1, 1-2 y 1-3 sin introducir elementos nuevos:
    1. *Carrera Ascendente Vertical:* Tramo de escalada vertical rápida con hongos elásticos, donde el jugador debe alternar entre rebotes normales y **Súper Rebotes** para calcular cornisas distantes.
    2. *Timing y Cadencia en el Vacío:* Hojas quebradizas combinadas con plantas carnívoras que exigen encadenar saltos dobles precisos en el aire sin suelo de apoyo debajo.
    3. *La Gran Parábola:* Salto largo desde la rama más alta sobre el vacío absoluto, usando el aleteo en el ápice para alcanzar la plataforma del nido.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 35.0`):* Mitad de la subida vertical.
  - *Checkpoint 2 (`X = 72.0`):* Justo antes del tramo final que conduce a la cuna del Huevo Verde.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Above the Canopy, Beneath the Sun" (Sobre el Dosel, Bajo el Sol).
  - *Estilo:* Orquestación triunfal y emotiva: cuerdas completas en crescendo, percusión animada y una flauta alegre que denota la cercanía de la meta.
  - *Tempo:* 112 BPM. Ritmo inspirador y enérgico.
  - *Momento del Rescate ("Lullaby for the Green Shell"):* La orquesta se apaga por completo; un solo de piano interpreta una melodía de cuna íntima a 60 BPM mientras late el huevo.
- **Efectos de Sonido (SFX):**
  - Latido del Huevo: Sonido diegético de latido cardíaco amortiguado (*thump-thump*) que aumenta de volumen en estéreo según la cercanía de Alma.
  - Rescate: Chispa mágica cristalina y suspiro aliviado de Alma.
  - Rugido del Jefe: Al concluir el rescate, un rugido simiesco colosal que sacude la pantalla (Screen Shake) anunciando la arena del Jefe 1.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Cuna de ramas de la copa, Huevo Verde con textura de cáscara y máscara de iluminación (Sprite + Light 2D).
- [ ] **Sprites Fondo:** Capas de nubes panorámicas para tileado horizontal infinito.
- [ ] **VFX:** Haz de luz sagrado descendiendo sobre el nido del huevo; partículas de luz flotantes doradas y verdes.
- [ ] **Audio:** Pista "Above the Canopy", tema de rescate en piano "Lullaby for the Green Shell", SFX de latido cardíaco diegético, rugido de transición a Jefe.
