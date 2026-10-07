> **Ficha de diseño para reconstrucción:** esta escena aún no existe en el proyecto actual. Las notas sobre escenas, pruebas o assets existentes describen el prototipo retirado. Las referencias de cámara a **8** son el criterio de la reconstrucción, no resultados de aquellas pruebas; sus valores de seguimiento están en [Alma](../../Player/Alma.md#cámara-daño-y-feedback). No habrá audio.

# 🗺️ Nivel 4-3: "La Gran Fractura"
> **Mundo 4: Cima Volcánica** | **Función Pedagógica:** Complicar (La Gran Prueba de Síntesis Mecánica Total)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** La ladera del volcán colapsa. La actividad sísmica es brutal; la montaña se abre en dos mitades dejando una garganta de magma hirviente. Lluvias de meteoritos ardientes caen del cielo mientras el suelo tiembla. Es el tramo más hostil y técnico de todo el continente.
- **Estado Emocional de Alma:** Trance de concentración absoluta. Alma se ha convertido en una fuerza de la naturaleza. Todas las habilidades aprendidas en su viaje deben ejecutarse sin un milisegundo de error.
- **Pistas Narrativas en el Entorno:** Huellas carbonizadas de los ladrones huyendo en pánico; armaduras rotas y rocas partidas por el calor.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** El colapso del mundo. La pantalla tiembla con micro-sismos regulares, el cielo es un vórtice negro y carmesí de cenizas y meteoritos en llamas caen describiendo estelas de humo y fuego.
- **Paleta de Color Principal:**
  - Fuego y Destrucción: Naranja magma (`#FF5400`), amarillo fuego (`#FFDD00`), rojo sangre (`#9D0208`).
  - Humo y Rocas: Carbón bituminoso (`#03071E`).
- **Iluminación 2D (URP):**
  - Luces dinámicas naranjas parpadeantes que simulan la cercanía de ríos de magma ardiente.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Todas las habilidades activas y combinadas: Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo + Rugido de Choque.
- **Filosofía de Nivel: La Síntesis Acumulativa Total (El Nivel Más Exigente)**:
  - Diseñado al estilo de los capítulos finales de *Celeste* o los guanteletes de *Ori*:
  - **La Secuencia Maestra de 4 Habilidades en Cadena:**
    1. *Despegue:* Salto simple desde una roca que colapsa hacia un abismo de lava.
    2. *Aleteo y Dash:* **Doble Salto** en el ápice seguido inmediatamente por un **Dash Aéreo** para cruzar por encima de una hilera de púas ardientes.
    3. *Impacto Sísmico:* En pleno vuelo al final del Dash, pulsar abajo para ejecutar el **Pisotón Sísmico**, descendiendo en picada vertical para quebrar un suelo agrietado antes de ser alcanzado por un chorro de fuego horizontal.
    4. *Desvío Sónico:* Al tocar tierra tras romper el suelo, un meteorito en llamas cae de frente: Alma debe ejecutar inmediatamente el **Rugido de Choque** para desviar el meteorito hacia una compuerta que abre la siguiente cámara.
- **Enemigos Activos:**
  - *Salamandras de Magma (`MagmaSalamander2D`):* Enemigos rápidos que trepan paredes y escupen fuego. El rugido las aturde y las empuja al vacío.

> **Implementado** ([ficha](../../Enemies/MagmaSalamander_Volcano.md)): la salamandra patrulla en el suelo entre dos puntos (no trepa paredes), se detiene a escupir bolas de fuego rectas hacia Alma cuando está a 8 m por cualquier lado y sigue caminando cuando se va. Todavía no reacciona al Rugido ni al Pisotón.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 32.0`):* Refugio de roca sólida tras la primera cadena acrobática.
  - *Checkpoint 2 (`X = 68.0`):* Antes del gran cruce de la fractura colapsante.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Meteoritos en llamas, salamandras de magma (trepando, escupiendo fuego, cayendo).
- [ ] **VFX:** Lluvia continua de meteoritos con estelas de partículas de fuego y humo; temblor de cámara screen shake regular.


## 6. Implementación jugable y ritmo del nivel

Escena `Assets/Scenes/World_4_Volcano/Level_4_3.unity`, disponible desde `Alma/📂 Cargar Nivel 4-3`. El portal del 4-2 carga La Gran Fractura. Las cuatro habilidades están habilitadas al entrar directamente.

El nivel tiene tres situaciones distintas, con descanso en X=32 y X=68:

1. **Entrada — romper la garganta:** una única cadena de Salto + Doble Salto + Dash desde la roca colapsante X=10–12 sobre lava y púas X=12–20. Pisotón rompe el suelo X=20–28 y lleva al refugio Y=-1.5. Rugido devuelve el meteorito y abre la compuerta X=28. El chorro horizontal queda arriba. Checkpoint X=32 y una salamandra en la salida.
2. **Centro — elegir el camino:** entre X=42 y X=64, cornisas altas X=44–48 / 51–55 / 58–62, superficies Y=2.2 / 2.8 / 2.8, ceden 1.5s después de pisarlas. Son la ruta rápida de saltos. Abajo, piedras estables X=44–50 / 52–59 / 61–64, superficie Y=-0.6, ofrecen otro recorrido con chorros bajos de vapor en X=47 y X=57.5 y una salamandra en X=55.2. Se puede cambiar de ruta al caer. La lava profunda queda a Y=-2.2, dejando espacio seguro bajo las cornisas. Ambas rutas llegan al checkpoint X=68. Aquí no hay otra compuerta de meteorito.
3. **Final — despertar la fractura y huir:** Pisotón rompe el sello X=70–74 y abre su barrera. Un temblor breve, el resplandor y el aviso anuncian lava que sube desde Y=-3.2, tras 1.4s de aviso, a 0.9m/s. Escalones X=75–79 / 81–85 / 87–91 / 93–97 tienen superficies Y=-0.2 / 1 / 2.2 / 3.2; el primer salto desde el refugio sube 1.3m y los siguientes suben como máximo 1.2m. Una salamandra protege la cornisa X=89. Las dos últimas cornisas ceden. Un último Doble Salto + Dash cruza el foso X=97–105 y llega al refugio final Y=3.8. La lava se detiene al alcanzar esa zona y no hay un segundo puzle de meteorito.

El meteorito de la entrada aparece 2.4m a la derecha y 1.35m sobre Alma, dentro del Rugido normal de 3m. Cambia a cian al devolverse. Si se falla, hay otra oportunidad con aviso; junto a la compuerta se pide retroceder para mantener un ángulo alcanzable. Rugir directamente sobre la compuerta no la abre. Las salamandras apuntan hacia ambos lados y Rugido/Pisotón las aturden.

Morir conserva la compuerta de entrada si ya quedó detrás del checkpoint y restaura el sello, la erupción, las cornisas, los chorros y los enemigos. Muro detrás del inicio. Cámara size 8, seguimiento de Alma y anticipación horizontal 4.3 m. Portal X=110, Y=5.3 hacia `Level_4_4`, La Antecámara del Fuego. No rescata el Huevo Rojo ni completa el mundo. Arte definitivo pendiente.

Validación: 16 pruebas PlayMode del 4-3, con ruta alta + huida completas sin muertes, ruta baja sin muertes, ascenso real de lava y reinicios de checkpoints. 77 pruebas EditMode aprobadas. Revisión visual histórica; repetirla con cámara de tamaño 8 al reconstruir.
