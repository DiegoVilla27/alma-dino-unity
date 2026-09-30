# 🗺️ Nivel 1-2: "El Dosel Peligroso"
> **Mundo 1: Jungla Esmeralda** | **Función Pedagógica:** Practicar (Verticalidad, Hongos Elásticos y Encuentro con el Mono Ladrón)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Siguiendo el rastro de huellas, Alma llega al pie de un gigantesco árbol centenario cuyas ramas se pierden en el dosel. En la misma base del árbol, a escasos tres metros de distancia, se encuentra el **Mono Ladrón**, dando saltitos en su sitio mientras sostiene orgulloso el huevo robado de Alma.
- **Encuentro Cinematográfico en Gameplay:**
  - Alma aparece en `X = 0`. El mono está en `X = 3.2` sobre el suelo.
  - Al percatarse de Alma, el mono chilla, se burla y escapa mediante un salto acrobático alto hacia las ramas superiores (`X = 8, Y = 9`), desafiándola a seguirlo hacia la cima del árbol.
- **Textos en Pantalla / Banners:**
  - *Burla del Mono Ladrón:*
    > *"¡MONO LADRÓN!*  
    > *¡Kikiki! ¿Creías que podías alcanzarme con tus pesadas patas, mamá lagarto?*  
    > *¡Tus huevos son míos! ¡Sube a buscarme a la cima si te atreves!"*
- **Pistas Narrativas en el Entorno:** Una cáscara dorada caída en una rama, hojas quebradas que marcan la trayectoria de huida del mono, y el sonido lejano de sus chillidos resonando en la altura.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Ascenso vertical entre troncos retorcidos, lianas trepadoras y hongos bioluminiscentes de color magenta/fucsia que crecen en las grietas de la corteza. La luz solar pasa de ser dorada y difusa a formar contrastes intensos de sombra bajo las enormes hojas del dosel.
- **Paleta de Color Principal:**
  - Madera y Corteza: Marrón caoba (`#523D24`) y musgo seco (`#364E32`).
  - Hongos Elásticos: Fucsia bioluminiscente (`#E030A0`) con esporas brillantes (`#FF80DF`).
  - Peligros (Espinas y Plantas): Rojo advertencia (`#C82828`) y púrpura carnoso (`#952020`).
  - Huevo Dorado: Oro brillante (`#FFD700`) con reflejos cálidos.
- **Fondos Parallax (4 Capas):**
  - *Capa 0 (Fondo Lejano):* Cielo tropical brillante que se va abriendo a medida que Alma asciende en altitud.
  - *Capa 1 (Fondo Medio):* Siluetas de copas de árboles vecinos y ramas gigantescas entrelazadas.
  - *Capa 2 (Fondo Cercano / Gameplay):* Tronco colosal central sobre el que se trepa, ramas que sirven de plataformas, hongos y zarzas.
  - *Capa 3 (Primer Plano / Foreground):* Hojas gigantes de higuera que pasan en primer plano a distintas alturas, acentuando la sensación de escala y altura.
- **Iluminación 2D (URP):**
  - Cada hongo elástico emite una luz circular suave fucsia (intensidad 0.8, radio 2.5m).
  - La planta carnívora emite un pulso sutil cuando pasa al estado de advertencia amarillo.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base completo + **Doble Salto (Aleteo Materno)** activo desde el inicio.
- **Catálogo de Bloques y Plataformas:**
  - *Hongos Rebotadores (`BouncyPlatform2D`):*
    - Impulso vertical base: `16.5 m/s` (~6.5m de elevación).
    - **Súper Rebote (+18%):** Al mantener pulsado el botón de Salto (`Espacio` / `JUMP`), Alma alcanza `20.0 m/s` (~9.2m de elevación).
    - Trampolín continuo: pararse encima genera rebotes sucesivos sin atascarse.
    - Desacoplados del suelo (`IBouncySurface2D`): no cortan el estado de salto ni reinician la inercia.
  - *Plataformas de Hojas Quebradizas (`CrumblingPlatform2D`):* Hojas que tiemblan con tinte rojizo y colapsan a los `0.65 segundos`.
  - *Plantas Carnívoras Rítmicas (`CarnivorousPlant2D`):* Ciclo de 1.8s abierta ➔ 0.5s advertencia amarilla ➔ 1.0s mordisco rojo letal.
- **Los 5 Desafíos de Diseño:**
  1. *Desafío 1 (Rebote Guiado bajo Techo de Espinas):* Hongo en `X = 8.5` bajo un techo de zarzas a `Y = 7.0`. Requiere controlar el impulso aéreo hacia la derecha para esquivar el techo y alcanzar la rama `Branch_Ledge_1`.
  2. *Desafío 2 (Compuerta Rítmica con Planta Carnívora):* Atravesar una rama estrecha con planta carnívora o saltar sobre ella hacia el segundo hongo (`X = 25.5`). Cuenta con rama de seguridad inferior.
  3. *Desafío 3 (Gran Salto de Altura / Súper Rebote & Checkpoint 1):* Desnivel vertical de +8.0m hacia `Canopy_Cliff_1` (`Y = 13.5`). Exige Súper Rebote o Doble Salto en la cima del vuelo.
  4. *Desafío 4 (Hojas Quebradizas sobre el Gran Abismo):* Cruce ágil sobre dos hojas quebradizas (0.65s) separadas por una liana espinosa colgante.
  5. *Desafío 5 (Cadena Aérea de Hongos en el Vacío):* Encadenamiento de dos hongos suspendidos en troncos aéreos sobre el abismo para alcanzar la Gran Copa del Nido (`Y = 31.0`), Checkpoint 2 y el portal al Mundo 1-3.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Ascent through the Boughs" (Ascenso entre las Ramas).
  - *Estilo:* Percusión orgánica con tambores de madera (slit drums, marimbas ligeras) combinados con flauta travesera y pizzicatos de violín que transmiten verticalidad, agilidad y curiosidad.
  - *Tempo:* 96 BPM. Ritmo sincopado que acompaña el compás de los rebotes.
- **Efectos de Sonido (SFX):**
  - Hongo Rebotador: Efecto elástico contundente (*¡BOING!* resonante con sub-graves y armónico ascendente).
  - Súper Rebote: Tono adicional brillante tipo chirrido de viento y resonancia de energía.
  - Planta Carnívora:
    - Fase abierta: Respiración vegetal siseante.
    - Advertencia: Chasquido de mandíbulas secas.
    - Mordisco: Latigazo seco y contundente (*¡CHOMP!*).
  - Hoja Quebradiza: Crujido de tallos y crujido seco al desprenderse en caída.
  - Mono Ladrón: Chillido acrobático simiesco ("¡Kikiki!") y carcajada juguetona al saltar.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Ambientales:** Hongo elástico (estados: reposo, compresión squish, extensión stretch), ramas horizontales de árbol, tronco vertical de apoyo, espinas afiladas para techo.
- [ ] **Sprites Enemigos:** Planta carnívora (animación de 3 frames: abierta, amenazante con espinas expuestas, mordisco cerrado).
- [ ] **Sprites Mono Ladrón:** Mono con huevo dorado en brazos, pose de burla con risa, sprite rotatorio para acrobacia aérea.
- [ ] **VFX:** Partículas de esporas fucsia al rebotar en el hongo, partículas de polvillo vegetal al romperse la hoja quebradiza.
- [ ] **Audio:** Pista "Ascent through the Boughs", SFX de resorte/boing, SFX de chomp de planta, voz/chillido del mono ladrón.
