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
  1. *Corrientes de Viento Frontales (`WindCurrent2D`):*
     - Zonas de túnel donde una fuerza continua empuja a Alma hacia la izquierda con una aceleración de `-12.0 m/s`.
     - Intentar saltar o caminar normal frena a Alma y la hace retroceder.
  2. *Inmunidad del Dash Aéreo:*
     - Al ejecutar el **Dash Aéreo**, Alma se vuelve inmune a la fuerza de arrastre del viento durante los `0.2 segundos` de su desplazamiento, cortando limpiamente la ráfaga y alcanzando la siguiente roca segura.
  3. *Barreras de Cañas Podridas (`BreakableBarrier2D`):*
     - Muros de caña seca en medio de los abismos que no se pueden saltar.
     - El impacto frontal del Dash Aéreo las pulveriza al instante sin frenar la inercia de Alma.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 28.0`):* Refugio de roca protegido del viento tras el primer túnel de ráfagas.
  - *Checkpoint 2 (`X = 64.0`):* Antes del gran salto del cañón abierto.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Gale-Force Defiance" (Desafío al Vendaval).
  - *Estilo:* Cuerdas frotadas con ritmo galopante y viento silbante continuo integrado como parte de la pista rítmica, con intervenciones agresivas de percusión de madera.
  - *Tempo:* 115 BPM. Sensación de lucha contra los elementos.
- **Efectos de Sonido (SFX):**
  - Silbido del Viento: Aullido dinámico que sube de volumen cuando Alma entra en un túnel de ráfaga.
  - Dash Cortando el Viento: Efecto de látigo sónico que corta el ruido del viento durante una fracción de segundo (*¡SLICCK!*).
  - Ruptura de Barrera: Crujido seco de cañas y maderas que se parten.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Muro de cañas rompibles, toberas/géiseres de viento en roca.
- [ ] **VFX:** Sistema de partículas de ráfaga de viento horizontal (estelas blancas y hojas que viajan rápido).
- [ ] **Audio:** Pista "Gale-Force Defiance", SFX de aullido de viento en túnel, SFX de ruptura de cañas.
