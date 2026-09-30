# 🗺️ Nivel 4-1: "Los Ríos de Ceniza"
> **Mundo 4: Cima Volcánica** | **Función Pedagógica:** Introducir (Despertar del Rugido de Choque & Rocas Ígneas)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Siguiendo la columna de humo negro y fuego, Alma asciende por las faldas de la Cima Volcánica. El aire abrasa la garganta y una densa lluvia de ceniza gris cubre el suelo de basalto. Frente a ella, ríos de lava líquida cortan el paso y enormes rocas ígneas pesadas bloquean las gargantas de piedra. No hay forma física de empujarlas con el cuerpo sin arder.
- **Estado Emocional de Alma:** Furia contenida y amor materno llevado al punto de ebullición. El dolor del calor no es nada comparado con el dolor de no tener a sus cuatro crías a salvo. De lo más hondo de su pecho nace un rugido ancestral.
- **El Despertar de la Habilidad (El Fuego Primordial):**
  - Alma interactúa con una fumarola volcánica sagrada:
  - *Texto en Pantalla:*
    > *"¡HABILIDAD DESPERTADA: RUGIDO DE CHOQUE!*  
    > *La voz de la madre dinosaurio sacude la roca y el fuego.*  
    > *Pulsa ROAR para desatar una onda sonora cónica que empuja rocas gigantes, activa campanas lejanas y repele amenazas."*
- **Pistas Narrativas en el Entorno:** Huellas de garras gigantescas quemadas en la piedra, fragmentos de roca fundida y el eco lejano de un rugido tiránico desde el cráter superior.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un páramo volcánico abrasador. Piedra de basalto negro con grietas que desprenden un resplandor naranja y ríos de magma amarillo incandescente. Lluvia constante de partículas de ceniza y ascuas ardientes en el aire.
- **Paleta de Color Principal:**
  - Magma y Lava: Amarillo fuego (`#FFD166`), naranja brillante (`#F77F00`) y rojo carmesí (`#D62828`).
  - Basalto y Roca: Negro carbón (`#1B1B1E`) y gris ceniza (`#3A3A3A`).
  - Onda del Rugido: Anillos concéntricos de distorsión sónica con tinte doradonaranja (`#FFB703`).
- **Fondos Parallax (4 Capas):**
  - *Capa 0 (Fondo Lejano):* El cráter gigantesco del volcán activo bajo un cielo cubierto de nubes negras de tormenta ígnea.
  - *Capa 1 (Fondo Medio):* Cascadas de lava fluida cayendo de cornisas de basalto lejanas.
  - *Capa 2 (Fondo Cercano / Gameplay):* Canales de lava, puentes de roca natural, rocas ígneas empujables.
  - *Capa 3 (Primer Plano / Foreground):* Chispas y brasas ardientes que flotan cerca de la cámara.
- **Iluminación 2D (URP):**
  - Iluminación ambiental cálida e intensa proveniente del magma inferior (luces 2D en tono naranja brillante a nivel del suelo).

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo + **Rugido de Choque (`RoarAction`)** desbloqueado en el altar del nivel.
- **Catálogo de Bloques y Mecánicas:**
  - *Rocas Ígneas Empujables (`PushableBoulder2D`):*
    - Bloques esféricos pesados de basalto. Alma no puede empujarlas caminando contra ellas.
    - Al emitir un **Rugido de Choque** frente a una roca a menos de 3 metros, la onda sónica transmite una fuerza de impacto que desplaza la roca 4-6 metros en la dirección de la onda.
    - *Mecánica de Puente:* Empujar una roca hacia un río de lava hace que caiga en el magma, solidificando un punto de apoyo seguro para cruzar.
  - *Ríos de Lava Líquida:* Peligro mortal de un solo impacto.
- **Peligros:**
  - Chorros de vapor ardiente y ríos de magma.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Rivers of Ash and Embers" (Ríos de Ceniza y Brasas).
  - *Estilo:* Percusión brutal con tambores de guerra pesados, metales graves orquestales (cornos franceses, tubas) y un coro masculino gutural que transmite épica y peligro colosal.
  - *Tempo:* 110 BPM. Pulso pesado y marcial.
- **Efectos de Sonido (SFX):**
  - Rugido de Choque: Bramido maternal demoledor (*¡ROOOOAAAR!*) acompañado de una onda de choque sónica con eco de baja frecuencia.
  - Desplazamiento de Roca: Rodar pesado y áspero de piedra contra basalto (*rumble-scrape*).
  - Magma: Burbujeo denso y abrasador constante.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Tileset:** Basalto volcánico agrietado con vetas emisoras de lava, ríos de magma líquido animado.
- [ ] **Props:** Rocas esféricas de basalto empujables, fumarolas volcánicas humeantes.
- [ ] **Sprites Alma:** Animación de rugido con apertura mandibular, pecho hinchado y emisión de ondas sónicas.
- [ ] **VFX:** Ondas de choque sónicas translúcidas en arco; lluvia de partículas de ceniza y ascuas flotantes.
- [ ] **Audio:** Pista "Rivers of Ash and Embers", SFX del rugido de Alma, SFX de roca rodando en basalto.
