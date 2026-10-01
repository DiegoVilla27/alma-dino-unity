# 🗺️ Nivel 1-1: "Despertar en el Nido"
> **Mundo 1: Jungla Esmeralda** | **Función Pedagógica:** Introducir (Locomoción Base + Despertar del Doble Salto)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Alma regresa a su nido tras buscar alimento en las lindes del bosque. El suelo tiembla ligeramente. Al llegar a la cuna de ramas, descubre el peor desenlace: el nido está completamente vacío y los cuatro huevos han desaparecido. En el suelo hay pequeñas pisadas simiescas que se internan hacia el corazón de la jungla.
- **Estado Emocional de Alma:** Silencio atónito, incredulidad inicial que rápidamente se transforma en una determinación maternal feroz e inquebrantable. No hay lágrimas; hay urgencia y concentración absoluta.
- **Textos en Pantalla / Banners:**
  - *Prólogo Inicial (Cámara lenta sobre el nido vacío):*
    > *"La tierra tembló una sola vez.*  
    > *Cuando regresé al nido con comida, el silencio era absoluto. No estaban.*  
    > *Si tengo que cruzar el continente entero a pie, mis pequeños volverán a sentir el calor de mis plumas."*
  - *Altar de la Gema Materna (Interacción):*
    > *"¡HABILIDAD DESPERTADA: ALETEO MATERNO!*  
    > *El latido de tus hijos resuena en tu sangre. Pulsa SALTO en el aire para desplegar tus plumas y ejecutar un segundo impulso."*
- **Pistas Narrativas en el Entorno:** Ramitas quebradas, plumas desgarradas de Alma caídas en la lucha, cáscaras vacías de bayas dejadas por los ladrones y huellas pequeñas que marcan el rumbo hacia la derecha.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Amanecer dorado en el sotobosque tropical. Raíces milenarias cubiertas de musgo húmedo, helechos gigantescos y motas de polen flotando en haces de luz cálida que atraviesan las copas.
- **Paleta de Color Principal:**
  - Cielo / Luz cenital: Oro pálido (`#F5E6A3`) y ámbar suave (`#D9A05B`).
  - Suelo y Vegetación: Verde bosque profundo (`#244023`), musgo esmeralda (`#4E7A38`) y corteza tierra húmeda (`#4A3319`).
  - Acentos de Interacción: Esmeralda brillante (`#00FF88`) para la Gema Materna y el portal de salida.
- **Fondos Parallax (4 Capas):**
  - *Capa 0 (Fondo Lejano):* Siluetas distantes de la cordillera volcánica humeante bajo un cielo matutino.
  - *Capa 1 (Fondo Medio):* Troncos colosales de árboles centenarios desdibujados por una niebla dorada tenue.
  - *Capa 2 (Fondo Cercano / Gameplay):* Plataformas de roca y tierra cubierta de hierba, lianas colgantes, flores silvestres.
  - *Capa 3 (Primer Plano / Foreground):* Hojas de helecho desenfocadas en las esquinas inferiores que reaccionan con balanceo al paso de la cámara.
- **Iluminación 2D (URP):**
  - Luz Global cálida tenue (intensidad 0.65).
  - Luces Spot 2D que simulan rayos solares (*god rays*) cayendo en ángulo de 45° sobre el nido y el altar.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - *Fase 1 (Inicio):* Movimiento horizontal (`7.0 m/s`), Salto simple (`8.2 m/s`) con gravedad adaptable.
  - *Fase 2 (Tras el Altar):* **Doble Salto (Aleteo Materno)** permanente.
- **Catálogo de Bloques y Plataformas:**
  - *Suelo de Musgo Firme (`Floor_Nest`, `Floor_Plains`):* Suelo plano y seguro con fricción cero para respuesta instantánea de movimiento.
  - *El Gran Abismo de Aprendizaje:* El desnivel y la cornisa inferior guían hacia el Altar. Un hueco de 4.8 m por sí solo no garantiza bloquear el salto simple: hay que contar coyote time, ancho del personaje y altura de llegada. Los obstáculos de evaluación deben requerir altura o combinar salto y aleteo, sin depender de una distancia falsa.
  - *Altar Materno (`AbilityRelic2D`):* Pedestal de piedra ancestral con una gema flotante que pulsa suavemente. Al tocarlo, congela brevemente el tiempo (hit stop) y desbloquea el Doble Salto.
- **Hojas de práctica:** Colapso tras 1.0 s, con aviso visual desde el primer contacto.
- **Peligros:**
  - Sin enemigos hostiles. En las escenas actuales hay espinas bajo los fosos de práctica y las hojas. El contacto o una caída profunda devuelven al último checkpoint mediante respawn rápido.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "The Silent Cradle" (El Nido en Silencio).
  - *Estilo:* Melodía minimalista y conmovedora en piano acústico solo, acompañada de suaves pizzicatos de chelo y una flauta de madera andina/indígena.
  - *Tempo:* 72 BPM. Tono melancólico pero con una línea de bajo que denota resolución y avance.
  - *Transición adaptativa:* Al despertar el doble salto en el altar, entran cuerdas completas que elevan la sensación de esperanza y poder maternal.
- **Efectos de Sonido (SFX):**
  - Pasos: Impacto suave sobre tierra blanda y hojas secas con variación de 4 tonos aleatorios.
  - Salto Simple: Despegue ágil con silbido de aire ligero.
  - Aleteo (Doble Salto): Sacudida rápida de plumas y una ráfaga de aire cálido (*woosh* suave pero enérgico).
  - Altar: Resonancia cristalina mágica que reverbera en estéreo al tocar la gema.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Tileset:** Tierra con hierba superior, tierra interior, bordes de raíz y rocas musgosas (16x16 o 32x32).
- [ ] **Props:** Nido de ramas y plumas destrozado, altar de piedra runal, gema flotante con halo emisor, flores tropicales.
- [ ] **Sprites Alma:** Animaciones de Idle (respiración atenta), Run (zancadas ágiles), Jump (despegue firme) y Double Jump (aleteo de plumas del lomo).
- [ ] **Audio:** Pista de música "The Silent Cradle", SFX de pasos en hierba/tierra, SFX de despertar de gema, SFX de viento en salto.
