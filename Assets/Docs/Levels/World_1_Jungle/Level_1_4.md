> **Ficha de diseño para reconstrucción:** existe una escena de trabajo `Assets/Scenes/World_01/Level_1_4.unity` (copia de la escena de práctica con el fondo parallax del nivel), pero esta ficha describe el nivel completo previsto. Las notas antiguas de pruebas corresponden al prototipo retirado. La cámara ortográfica debe usar tamaño **8**; sus valores de seguimiento están en [Alma](../../Player/Alma.md#cámara-daño-y-feedback). No habrá audio.

# 🗺️ Nivel 1-4: "La Copa del Gran Árbol"
> **Mundo 1: Jungla Esmeralda** | **Función Pedagógica:** Evaluar + Primer Rescate (Ascenso Maestro & Huevo Verde)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Alma ha superado las zarzas y se encuentra en el tronco madre del Gran Árbol Sagrado. La niebla se disipa y la luz del mediodía inunda las ramas más altas. En la cima, protegido por el nido del gran simio, yace el primer huevo robado: el **Huevo Verde**, cuya cáscara pulsa con un tenue latido cálido.
- **Estado Emocional de Alma:** Una mezcla arrolladora de alivio, adrenalina y ferocidad protectora. Su primer hijo está al alcance de sus manos; nada en la jungla la detendrá ahora.
- **El Momento del Rescate (Cinemática Diegética en Gameplay):**
  - Al final del nivel, Alma alcanza una cuna de plumas gigantes.
  - Al tocar el **Huevo Verde**, la tensión visual cede; una luz cálida destaca el reencuentro:
  - *Texto del Rescate (Huevo Verde):*
    > *"Aún estás tibio...*  
    > *Mamá llegó a tiempo. Ya estás a salvo.*  
    > *(Uno de cuatro rescatados)*"
  - Inmediatamente después, un rugido colosal sacude la copa: el **Mono Ladrón Gigante** desciende de las alturas bloqueando el paso hacia el siguiente mundo.
- **Pistas Narrativas en el Entorno:** Plumas doradas y verdes de cría dinosaurio, cáscaras de frutos gigantes partidos por la mitad y un pulso luminoso del huevo que se intensifica al acercarse.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** La majestuosidad de la cima del dosel arbóreo por encima de las nubes. Mar de hojas verdes y doradas a los pies del jugador, ramas gigantescas bañadas por un sol radiante de mediodía y una brisa constante que mece el follaje.
- **Paleta de Color Principal:**
  - Cielo: Azul zafiro brillante (`#3A86FF`) con nubes blancas algodonosas (`#F8F9FA`).
  - Vegetación de Copa: Verde esmeralda resplandeciente (`#2EC4B6`) y hojas doradas (`#FFB703`).
  - Nido del Rescate: Fibras vegetales doradas y plumas protectoras de Alma.
  - Huevo Verde: Esmeralda bioluminiscente (`#00F5D4`) con un halo palpitante.
- **Fondo parallax (2 capas):**
  - *Fondo lejano:* Cielo azul de mediodía y mar de nubes blancas bajo la copa del bosque.
  - *Fondo medio:* Copas lejanas que sobresalen de las nubes como islas verdes, con una abertura transparente que deja ver el cielo sin velarlo.
- **Escenario jugable:** Las ramas del Gran Árbol, hojas doradas, hongos, nido y huevo son piezas independientes del fondo. No se usa capa de primer plano.
- **Parallax preparado:** `Assets/Prefabs/Level/Backgrounds/World_1/Parallax_Level_1_4.prefab` contiene Far y Mid, repetibles horizontalmente y diseñados para cámara ortográfica de tamaño **8**. Está colocado en la escena de trabajo `Level_1_4` y revela la parte alta del cielo al subir (`Ascent Reveal Per Unit` 0,13 / 0,07); el nivel completo sigue pendiente. Valores en [Fondos](../../LevelPieces/Backgrounds.md#valores-por-nivel).
- **Iluminación visual:** El proyecto usa el renderizador integrado. La luz cálida y el halo del huevo deben representarse mediante sprites y efectos compatibles, sin depender de luces 2D de URP.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base completo + **Doble Salto (Aleteo Materno)** llevado a su máxima expresión.
- **Hojas de evaluación:** Colapso tras 0.65 s, con el mismo aviso visual aprendido antes.
- **Objetivo de Diseño: El Examen Maestro del Mundo 1**:
  - El nivel evalúa la síntesis fluida de todas las mecánicas aprendidas en 1-1, 1-2 y 1-3 sin introducir elementos nuevos:
    1. *Carrera Ascendente Vertical:* Tramo de escalada vertical rápida con hongos elásticos, donde el jugador debe alternar entre rebotes normales y **Súper Rebotes** para calcular cornisas distantes.
    2. *Timing y Cadencia en el Vacío:* Hojas quebradizas combinadas con plantas carnívoras que exigen encadenar saltos dobles precisos en el aire sin suelo de apoyo debajo.
    3. *La Gran Parábola:* Salto largo desde la rama más alta sobre el vacío absoluto, usando el aleteo en el ápice para alcanzar la plataforma del nido.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 35.0`):* Mitad de la subida vertical.
  - *Checkpoint 2 (`X = 72.0`):* Justo antes del tramo final que conduce a la cuna del Huevo Verde.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [x] **Sprites Props:** Cuna de ramas de la copa, Huevo Verde con textura de cáscara y máscara de iluminación (Sprite + Light 2D bioluminiscente con GreenEggRescue2D).
- [x] **Sprites Fondo:** Dos capas panorámicas de cielo, nubes y copas lejanas, preparadas para repetición horizontal.
- [ ] **VFX & Iluminación:** Pulso luminoso visible del huevo verde y acentos cálidos del rescate mediante efectos compatibles con el renderizador integrado.
