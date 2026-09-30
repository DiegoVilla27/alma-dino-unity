# 🗺️ Nivel 4-2: "Las Campanas de Basalto"
> **Mundo 4: Cima Volcánica** | **Función Pedagógica:** Practicar (Campanas Resonantes & Puertas de Llamas Cronometradas)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En el interior de un desfiladero volcánico, antiguos monumentos de basalto cuelgan sobre las chimeneas térmicas. Son campanas de piedra esculpidas por civilizaciones olvidadas para modular las corrientes de gas del volcán. Llamaradas intermitentes de fuego bloquean los corredores estrechos, impidiendo el avance a menos que el sonido apague temporalmente el flujo de oxígeno.
- **Estado Emocional de Alma:** Precisión y serenidad bajo fuego. No basta con gritar; debe proyectar su rugido en el ángulo y momento exactos para mantener las vías abiertas.
- **Pistas Narrativas en el Entorno:** Marcas de tizne en las campanas que muestran que han vibrado durante siglos y huellas de garras que esquivaron las llamas por escasos centímetros.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un santuario volcánico de piedra negra con campanas de basalto colosales suspendidas de cadenas de hierro oxidado sobre abismos de lava. El calor crea distorsiones térmicas en el aire de la pantalla.
- **Paleta de Color Principal:**
  - Campanas de Basalto: Gris carbón metálico (`#212529`) con grabados en oro antiguo (`#DDA15E`).
  - Llamaradas: Fuego azul y naranja de gas metano (`#00B4D8` / `#FF7B00`).
  - Columnas de Humo: Humo negro espeso (`#161A1D`).
- **Iluminación 2D (URP):**
  - Efecto de resplandor intenso cuando las llamaradas están activas, que se apaga y vuelve tenue cuando la campana es golpeada.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo + Rugido de Choque.
- **Mecánica Estrella: Campanas Resonantes de Basalto (`ResonanceBell2D`):**
  - Campanas de piedra colocadas en posiciones elevadas o fuera del alcance de salto de Alma.
  - **Activación a Distancia por Rugido:**
    - Al orientar a Alma hacia la campana y emitir el **Rugido de Choque**, la onda sónica viaja hasta 8 metros e impacta la campana.
    - La campana vibra emitiendo un tono profundo que sofoca las llamaradas del entorno durante **`5.0 segundos`**.
    - Durante esos 5 segundos, Alma debe correr, ejecutar Doble Salto y Dash Aéreo para cruzar el corredor de fuego antes de que las llamas vuelvan a encenderse.
- **Puzles de Ángulo y Parábola:**
  - Campanas que exigen saltar y rugir en pleno vuelo o rebotar en un hongo para alcanzar el ángulo de impacto sónico.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 30.0`):* Tras la primera puerta de fuego cronometrada.
  - *Checkpoint 2 (`X = 62.0`):* Antes del corredor de triple campana encadenada.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Chimes of the Caldera" (Campanas de la Caldera).
  - *Estilo:* Percusión ritualista con gongs gigantescos, platillos tibetanos y una sección de violonchelos que genera tensión en una cuenta regresiva auditiva implacable.
  - *Tempo:* 100 BPM. Ritmo tenso y disciplinado.
- **Efectos de Sonido (SFX):**
  - Campana de Basalto: Campanada colosal de piedra profunda con vibración de reverberación prolongada (*¡DUMMMMNGGG!*).
  - Sofocación de Llamas: Silbido de fuego apagándose por corte de aire (*¡whooosh-fizzz!*).
  - Ignición de Llamas: Estallido de gas encendiéndose de nuevo (*¡FOOOM!*).

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Campana de basalto suspendida (reposo, vibración con ondas, retorno), quemador de llamaradas (activo, apagado).
- [ ] **VFX:** Onda de calor distorsionante en la pantalla; anillo sónico expandiéndose de la campana; llamarada de gas encendiéndose y apagándose.
- [ ] **Audio:** Pista "Chimes of the Caldera", SFX de campana de piedra, SFX de gas encendiéndose.
