# 🗺️ Nivel 3-4: "El Sauce Ancestral"
> **Mundo 3: Pantano de Viento y Niebla** | **Función Pedagógica:** Evaluar + Tercer Rescate (La Subida del Gas & Huevo Morado)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En el centro del pantano se alza el Sauce Ancestral, un coloso milenario cuyas raíces descienden a las aguas más profundas y cuyas ramas altas tocan las corrientes de aire libre. Sin embargo, el pantano comienza a hervir: una marea tóxica de gas verde pantanoso asciende inexorablemente desde las aguas. En lo más alto de la copa, protegido en un nido de musgo, descansa el tercer huevo robado: el **Huevo Morado**, cuyo cascarón ya comienza a vibrar tenuemente.
- **Estado Emocional de Alma:** Desesperación contra el reloj. El gas tóxico sube rápido; no hay tiempo para dudar ni un segundo.
- **El Momento del Rescate (Cinemática Diegética en Gameplay):**
  - Alma alcanza la rama cumbre del Sauce, dejando atrás el gas tóxico.
  - Al posar su hocico sobre el **Huevo Morado**, la tensión se evapora; el piano de la "Nana Maternal" regresa con una calidez conmovedora:
  - *Texto del Rescate (Huevo Morado):*
    > *"El cascarón tiembla...*  
    > *Falta muy poco para que rompas a cantar.*  
    > *Solo nos falta uno.*  
    > *(Tres de cuatro rescatados)"*
  - Un chillido ensordecedor rompe la niebla sobre el sauce: una silueta alada masiva desciende batiendo alas gigantescas: el **Pterodáctilo Alfa** ataca la copa del árbol.
- **Pistas Narrativas en el Entorno:** Hojas de sauce arrancadas, humo tóxico verde que asciende desde la parte inferior de la pantalla, y el canto apagado de la cría desde el interior del huevo morado.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un ascenso vertical frenético entre ramas de sauce llorón y lianas movedizas mientras una densa nube de gas verde radiactivo asciende desde el fondo de la pantalla. Al superar las ramas altas, el cielo gris se abre mostrando las cimas de las montañas volcánicas rojas a lo lejos.
- **Paleta de Color Principal:**
  - Gas Venenoso Ascendente: Verde ácido fosforescente (`#38B000`) con volutas de humo amarillento.
  - Hojas de Sauce: Verde grisáceo melancólico (`#6B705C`) y madera pálida (`#B7B7A4`).
  - Huevo Morado: Amatista cósmica brillante (`#9D4EDD`) con vetas luminiscentes.
- **Iluminación 2D (URP):**
  - Contraste entre la luz verde tóxica que sube desde abajo y la luz dorada pura que espera en la cima del árbol.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo.
- **Objetivo de Diseño: El Examen Maestro del Mundo 3**:
  1. *El Muro de Gas Tóxico Ascendente (`RisingHazardFloor2D`):*
     - El gas sube a una velocidad constante de `3.2 m/s`.
     - Si Alma es alcanzada por el gas, sufre daño y reaparece en el último checkpoint del tramo.
     - Obliga al jugador a ejecutar cadenas fluidas de Dash Aéreo y Doble Salto sin detenerse.
  2. *Encadenamiento de Globos de Esporas en Escalada:*
     - Esporas suspendidas en columnas verticales sobre corrientes de aire que deben ser cruzadas con Dash para ganar altura continua.
  3. *Uso del Pisotón para Despejar Atajos:*
     - En dos puntos clave de la subida, Alma puede ejecutar un Pisotón rápido sobre un suelo agrietado para tomar un atajo directo que le ahorra segundos vitales frente al gas.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 30.0, Y = 18.0`):* Primer tercio del ascenso.
  - *Checkpoint 2 (`X = 65.0, Y = 40.0`):* Antes de la carrera final hacia la copa del Sauce.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "The Rising Spores" (Las Esporas Ascendentes).
  - *Estilo:* Batería electrónica combinada con percusión acústica acelerada, sintetizadores analógicos tensos y violines en staccato agudo que transmiten una urgencia de contrarreloj implacable.
  - *Tempo:* 144 BPM. Pacing de escape frenético.
  - *Tema de Rescate ("Lullaby for the Purple Shell"):* Piano solo tierno y esperanzador a 62 BPM con armónicos de arpa.
- **Efectos de Sonido (SFX):**
  - Gas Ascendente: Siseo continuo de vapor cáustico amenazante (*sssshhhh*).
  - Canto del Huevo: Chirrido agudo musical de cría desde el interior de la cáscara.
  - Chillido del Pterodáctilo: Graznido prehistórico colosal rasgando el aire.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Ramas colgantes de sauce llorón, nido de musgo morado, Huevo Morado bioluminiscente.
- [ ] **VFX:** Capa de gas tóxico verde animada con partículas de humo y burbujas ascendentes.
- [ ] **Audio:** Pista de escape "The Rising Spores", tema de rescate del Huevo Morado, graznido del Pterodáctilo Alfa.
