> **Ficha de diseño para reconstrucción:** existe una escena de trabajo para el nivel 1-2, pero esta ficha describe el nivel completo previsto. Las notas de pruebas antiguas corresponden al prototipo retirado. La cámara ortográfica debe usar tamaño **8**; sus valores de seguimiento están en [Alma](../../Player/Alma.md#cámara-daño-y-feedback). No habrá audio.

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
- **Pistas Narrativas en el Entorno:** Una cáscara dorada caída en una rama, hojas quebradas que marcan la trayectoria de huida del mono, y marcas de garras entre las ramas.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Ascenso vertical entre troncos retorcidos, lianas trepadoras y hongos bioluminiscentes de color magenta/fucsia que crecen en las grietas de la corteza. La luz solar pasa de ser dorada y difusa a formar contrastes intensos de sombra bajo las enormes hojas del dosel.
- **Paleta de Color Principal:**
  - Madera y Corteza: Marrón caoba (`#523D24`) y musgo seco (`#364E32`).
  - Hongos Elásticos: Fucsia bioluminiscente (`#E030A0`) con esporas brillantes (`#FF80DF`).
  - Peligros (Espinas y Plantas): Rojo advertencia (`#C82828`) y púrpura carnoso (`#952020`).
  - Huevo Dorado: Oro brillante (`#FFD700`) con reflejos cálidos.
- **Fondo parallax (2 capas):**
  - *Fondo lejano:* Cielo tropical brillante que se va abriendo a medida que Alma asciende en altitud.
  - *Fondo medio:* Siluetas de copas de árboles vecinos y ramas gigantescas entrelazadas.
- **Escenario jugable:** Tronco colosal central sobre el que se trepa, ramas que sirven de plataformas, hongos y zarzas.
- **Parallax preparado:** `Assets/Prefabs/Level/Backgrounds/World_1/Parallax_Level_1_2.prefab` contiene solo el cielo lejano y el dosel medio. El dosel deja ver el cielo a través de una abertura central. Diseñado para cámara ortográfica de tamaño **8**. Está colocado en la escena de trabajo `Level_1_2` y revela la parte alta del cielo al subir (`Ascent Reveal Per Unit` 0,13 / 0,07). Valores en [Fondos](../../LevelPieces/Backgrounds.md#valores-por-nivel).
- **Iluminación visual:** el proyecto usa el renderizador integrado. Representar el resplandor fucsia de los hongos y el aviso de la planta mediante sprites, animación y partículas, sin depender de luces 2D de URP.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base completo + **Doble Salto (Aleteo Materno)** activo desde el inicio.
- **Catálogo de Bloques y Plataformas:**
  - *Hongos Rebotadores (`BouncyPlatform2D`):*
    - Impulso vertical base: `17.0 m/s` (~6.7m de elevación teórica).
    - **Súper Rebote (+18%):** Al mantener pulsado el botón de Salto (`Espacio` / `JUMP`), Alma alcanza `20.06 m/s` (~9.3m de elevación teórica).
    - Trampolín continuo: pararse encima genera rebotes sucesivos sin atascarse.
    - Desacoplados del suelo (`IBouncySurface2D`): no cortan el estado de salto ni reinician la inercia.
  - *Plataformas de Hojas Quebradizas (`CrumblingPlatform2D`):* Hojas que tiemblan con tinte rojizo y colapsan a los `1.0 segundos`.
  - *Plantas Carnívoras Rítmicas (`CarnivorousPlant2D`):* Ciclo de 1.8s abierta ➔ 0.5s advertencia amarilla ➔ 1.0s mordisco rojo letal.
- **Los 5 Desafíos de Diseño:**
  1. *Desafío 1 (Rebote Guiado bajo Techo de Espinas):* Hongo en `X = 8.5` bajo un techo de zarzas a `Y = 7.0`. Requiere controlar el impulso aéreo hacia la derecha para esquivar el techo y alcanzar la rama `Branch_Ledge_1`.
  2. *Desafío 2 (Compuerta Rítmica con Planta Carnívora):* Atravesar una rama estrecha con planta carnívora o saltar sobre ella hacia el segundo hongo (`X = 25.5`). Cuenta con rama de seguridad inferior.
  3. *Desafío 3 (Gran Salto de Altura / Súper Rebote & Checkpoint 1):* Desnivel vertical de +8.0m hacia `Canopy_Cliff_1` (`Y = 13.5`). Exige Súper Rebote o Doble Salto en la cima del vuelo.
  4. *Desafío 4 (Hojas Quebradizas sobre el Gran Abismo):* Cruce ágil sobre dos hojas quebradizas (1.0s) separadas por una liana espinosa colgante.
  5. *Desafío 5 (Cadena Aérea de Hongos en el Vacío):* Encadenamiento de dos hongos suspendidos en troncos aéreos sobre el abismo para alcanzar la Gran Copa del Nido (`Y = 31.0`), Checkpoint 2 y el portal al Mundo 1-3.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Ambientales:** Hongo elástico (estados: reposo, compresión squish, extensión stretch), ramas horizontales de árbol, tronco vertical de apoyo, espinas afiladas para techo.
- [ ] **Sprites Enemigos:** Planta carnívora (animación de 3 frames: abierta, amenazante con espinas expuestas, mordisco cerrado).
- [ ] **Sprites Mono Ladrón:** Mono con huevo dorado en brazos, pose de burla con risa, sprite rotatorio para acrobacia aérea.
- [ ] **VFX:** Partículas de esporas fucsia al rebotar en el hongo, partículas de polvillo vegetal al romperse la hoja quebradiza.
