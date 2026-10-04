> **Ficha de diseño para reconstrucción:** no hay escena implementada. Las notas sobre escenas, pruebas o assets existentes describen el prototipo anterior. La política vigente es jugar sin audio.

# 🗺️ Nivel 3-2: "El Cañón de las Ráfagas"
> **Mundo 3: Pantano de Viento y Niebla** | **Función Pedagógica:** Practicar (Géiseres de Viento & Inmunidad del Dash Aéreo)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Alma entra en un desfiladero estrecho donde el viento del pantano es canalizado con una fuerza descomunal. El aire aúlla entre las grietas rocosas y géiseres naturales de gas expulsan chorros continuos que empujan hacia atrás a cualquier criatura que intente avanzar.
- **Estado Emocional de Alma:** Determinación desafiante. Las tormentas y los vendavales no la alejarán de sus crías; si el viento sopla en contra, ella cortará a través de él.
- **Pistas Narrativas en el Entorno:** Árboles con copas permanentemente inclinadas hacia la izquierda por la fuerza del viento, rocas erosionadas por el flujo de aire y pedazos de tela/lianas agitándose furiosamente.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un cañón rocoso cubierto de limo y musgo grisáceo donde el viento se hace visible mediante líneas de partículas blancas y hojas arrancadas que viajan a toda velocidad en dirección contraria al jugador.
- **Paleta de Color Principal:**
  - Viento y Ráfagas: Blanco grisáceo semitransparente (`#E0E1DD`) y cian pálido.
  - Paredes del Cañón: Pizarra gris con tonos verdosos (`#415A77` / `#778DA9`).
  - Barreras de Madera: Troncos podridos quebradizos (`#5C4D3C`).
- **Iluminación 2D (URP):**
  - Luces direccionales tenues que proyectan sombras alargadas agitadas por el viento.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo.
- **Mecánicas del Entorno:**
  1. *Corrientes de Viento Frontales (`WindCurrentZone2D`):*
     - Zonas de túnel donde una fuerza continua empuja a Alma hacia la izquierda con una aceleración de `-12.0 m/s²`.
     - Esa corriente modifica el recorrido y la velocidad, pero no obliga a retroceder frente a una aceleración de carrera de 70 m/s². Las secciones obligatorias usan barreras rompibles o abismos; si se desea un viento que impida avanzar, debe calibrarse y probarse por encima del control aéreo (~54 m/s²).
  2. *Inmunidad del Dash Aéreo:*
     - Al ejecutar el **Dash Aéreo**, Alma se vuelve inmune a la fuerza de arrastre del viento durante los `0.2 segundos` de su desplazamiento, cortando limpiamente la ráfaga y alcanzando la siguiente roca segura.
  3. *Barreras de Cañas Podridas (`DashBreakableBarrier2D`):*
     - Muros de caña seca en medio de los abismos que no se pueden saltar.
     - El impacto frontal del Dash Aéreo las pulveriza al instante sin frenar la inercia de Alma.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 28.0`):* Refugio de roca protegido del viento tras el primer túnel de ráfagas.
  - *Checkpoint 2 (`X = 64.0`):* Antes del gran salto del cañón abierto.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Muro de cañas rompibles, toberas/géiseres de viento en roca.
- [ ] **VFX:** Sistema de partículas de ráfaga de viento horizontal (estelas blancas y hojas que viajan rápido).

---

## 6. Implementación del prototipo

- Escena: `Assets/Scenes/World_3_Swamp/Level_3_2.unity`. Entrada desde 3-1 y salida conectada a `Level_3_3`.
- Abrir con **Alma → 📂 Cargar Nivel 3-2**. Regenerar con **Tools → Alma → Construir Nivel 3-2 - El Cañón de las Ráfagas**.
- Doble Salto, Pisotón y Dash disponibles al entrar, también con una partida limpia abierta directamente en esta escena. Rugido bloqueado. Física compartida sin cambios; cámara size 6 y anticipo direccional.
- Pared sólida detrás del punto de aparición. Primera barrera de cañas en X=10 sobre suelo continuo hasta X=16: permite aprender a saltar y embestir sin un abismo debajo.
- Tres fosos de lodo: X=16–25 (9m), X=40–50 (10m) y X=68–79 (11m). Barreras aéreas en X=45 y X=74.5, de 7m de altura; exigen un impacto de Dash y no se evitan con Doble Salto.
- Cuatro zonas de viento frontal, aceleración de 12m/s² hacia la izquierda, sin compensación de gravedad. El Dash ignora exclusivamente el viento durante la acción; no protege del lodo.
- Refugios libres de viento con checkpoints en X=28 y X=64. Las cañas rotas permanecen abiertas durante el intento de la escena; recargar la escena las restaura.
- Arte geométrico, parallax de cañón y sauces inclinados, estelas y partículas hacia la izquierda. Efectos visuales definitivos pendientes.
- Validación: **5 pruebas PlayMode del nivel** y **45 EditMode** pasan. Incluye recorrido completo con entradas reales sin muertes, viento frontal, rechazo de impactos sin Dash, refugios y pared inicial.
