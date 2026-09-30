# 🗺️ Nivel 2-1: "Descenso a la Penumbra"
> **Mundo 2: Cuevas de Cristal** | **Función Pedagógica:** Introducir (Despertar del Pisotón Sísmico & Suelos Agrietados)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Tras derrotar al mono en la copa del árbol, el rastro de huellas conduce hacia una enorme hendidura en la roca. Alma desciende por una grieta vertical y cae en un pozo profundo y oscuro: la entrada a las Cuevas de Cristal. La caída la deja encerrada en una caverna sin salida visible por arriba. El único camino hacia adelante está bloqueado por una losa de estalagmitas densamente agrietadas que no ceden ante el peso normal.
- **Estado Emocional de Alma:** Impotencia momentánea al encontrarse atrapada, seguida por un despertar de fuerza terrenal. Para proteger a sus hijos en el inframundo, necesita peso, impacto y contundencia sísmica.
- **El Despertar de la Habilidad (Altar de Cristal):**
  - En una pequeña gruta lateral, Alma interactúa con una reliquia geoda resonante:
  - *Texto en Pantalla:*
    > *"¡HABILIDAD DESPERTADA: PISOTÓN SÍSMICO!*  
    > *Tu amor maternal adquiere la fuerza de la tierra.*  
    > *En el aire, pulsa ABAJO para caer con fuerza demoledora y quebrar suelos frágiles."*
- **Pistas Narrativas en el Entorno:** Goteras constantes, huellas de garras en el barro húmedo subterráneo y estalactitas partidas en el suelo.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Caverna subterránea fría y cavernosa. Paredes de roca pizarra azulada cubiertas de vetas de cuarzo y geodas bioluminiscentes que tiñen el entorno de destellos cian y violetas.
- **Paleta de Color Principal:**
  - Roca y Caverna: Azul pizarra oscuro (`#1C2541`) y gris piedra mojada (`#0B132B`).
  - Cristales Emisores: Cian eléctrico (`#48CAE4`) y amatista luminosa (`#7209B7`).
  - Suelos Agrietados: Roca caliza quebrada (`#ADB5BD`) con líneas de fisura visibles.
- **Fondos Parallax (4 Capas):**
  - *Capa 0 (Fondo Lejano):* Abismo negro con motas de polvo cristalino flotante.
  - *Capa 1 (Fondo Medio):* Columnas colosales de cristal que conectan el suelo con el techo abovedado.
  - *Capa 2 (Fondo Cercano / Gameplay):* Estalagmitas, bloques de roca quebradiza, cornisas de piedra.
  - *Capa 3 (Primer Plano / Foreground):* Estalactitas oscuras que cuelgan del techo y gotas de agua que caen en primer plano.
- **Iluminación 2D (URP):**
  - Iluminación global mínima (0.20) para crear atmósfera de misterio.
  - Alma cuenta con una luz 2D tenue alrededor de su cuerpo (radio 3.5m) que ilumina su entorno inmediato.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + **Pisotón Sísmico (`GroundPound`)** desbloqueado a mitad de nivel.
- **Catálogo de Bloques y Mecánicas:**
  - *Suelo Agrietado (`BreakableGround2D`):*
    - Bloques de roca fracturada con colisionador sólido.
    - Soportan que Alma camine o salte sobre ellos sin romperse.
    - Al recibir el impacto de un **Pisotón Sísmico** desde el aire (`velocity.y <= -20.0 m/s`), el bloque se fragmenta en pedazos y se destruye con Screen Shake.
  - *El Pozo Inicial (Tutorial Orgánico):*
    - El nivel comienza haciéndote caer en una fosa sin retorno hacia arriba.
    - La única salida es aprender a ejecutar el Pisotón sobre las losas agrietadas del fondo para abrir el túnel hacia la siguiente cámara.
- **Peligros:**
  - Pozos de estalagmitas afiladas en el fondo de las caídas.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Echoes in the Geode" (Ecos en la Geoda).
  - *Estilo:* Ambient subterráneo con sintetizadores fríos, ecos de gotas de agua rítmicas y un vibráfono cristalino que genera tensión y soledad.
  - *Tempo:* 65 BPM. Espacioso, atmosférico e inmersivo.
- **Efectos de Sonido (SFX):**
  - Pisotón Sísmico:
    - *Wind-up (0.1s):* Silbido de compresión de aire.
    - *Caída en picada:* Ráfaga rápida descendente.
    - *Impacto:* Golpe demoledor grave (*¡CRASH-BOOM!*) que sacude la pantalla y hace crujir las piedras.
  - Rompimiento de Bloques: Estallido de rocas quebrándose y cascotes cayendo al suelo.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Tileset:** Rocas de caverna subterránea azul/pizarra, estalagmitas afiladas, bloques de suelo agrietado (estados: entero, dañado, destruido).
- [ ] **Props:** Geodas de cristal con luz 2D, charcos de agua reflectante.
- [ ] **Sprites Alma:** Animación de Ground Pound (preparación en el aire, caída con cola erguida, pose de aterrizaje de impacto).
- [ ] **VFX:** Polvillo de roca y fragmentos de piedra al destruir un bloque agrietado; onda de choque sísmica circular al tocar tierra.
- [ ] **Audio:** Pista "Echoes in the Geode", SFX de impacto sísmico, SFX de cristales rompiéndose.
