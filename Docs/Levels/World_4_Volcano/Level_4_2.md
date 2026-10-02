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


## 6. Implementación jugable

Escena `Assets/Scenes/World_4_Volcano/Level_4_2.unity`, accesible desde `Alma/📂 Cargar Nivel 4-2`. El portal del 4-1 carga esta escena. Las cuatro habilidades están disponibles también al entrar directamente.

- Introducción: rugir mirando a la derecha desde X=6 activa la campana (12, 5.2), abre la puerta X=16 y permite cruzar lava X=20–28 con Doble Salto + Dash. Checkpoint X=30.
- Rugido aéreo: desde X=40, ejecutar Doble Salto y rugir hacia la campana (45.5, 6.5). Desde ese punto, en suelo queda fuera del cono. Altura rebajada de Y=8 a Y=6.5 para permitir Rugido temprano o tardío tras Doble Salto, incluso soltando el salto pronto. Cruzar la puerta X=48 y lava X=50–58. Checkpoint X=62.
- Cadena final: rugir desde cada isla para activar las campanas (68, 5.2), (85.5, 4.6) y (99.5, 4.6). Abren respectivamente las puertas X=68, X=82 y X=96. Los fosos X=70–78, X=84–92 y X=98–106 requieren Doble Salto + Dash.
- Cada campana apaga únicamente su puerta durante 5s. Otro rugido renueva el plazo. Cuenta atrás visible, aviso amarillo en el último segundo y tono provisional al activarse. Ni contacto ni Pisotón activan campanas.
- Las llamas cerradas dañan incluso durante Dash. Morir cierra todas las puertas; los checkpoints quedan en zonas seguras. Muro detrás del inicio para impedir caer por el extremo.
- Cámara size 6, seguimiento de Alma y anticipación horizontal 1.25m. Portal X=118 hacia `Level_4_3`, La Gran Fractura. El mundo todavía no se completa. Arte y audio definitivos pendientes.

Validación: 12 pruebas PlayMode del 4-2, incluyendo recorrido completo sin muertes mediante entradas normales, y 10 pruebas de regresión del 4-1. Suite EditMode: 75 pruebas.
