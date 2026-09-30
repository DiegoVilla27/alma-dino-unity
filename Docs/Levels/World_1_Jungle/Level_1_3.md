# 🗺️ Nivel 1-3: "Las Zarzas Profundas"
> **Mundo 1: Jungla Esmeralda** | **Función Pedagógica:** Complicar (Suelo Cero, Hojas Quebradizas y Compromiso de Salto)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Siguiendo las ramas altas, Alma desciende hacia una hondonada sombría donde el bosque se vuelve denso e impenetrable. Las zarzas espinosas cubren por completo el suelo como un océano vegetal de púas venenosas. En lo alto de un peñasco inalcanzable, el Mono Ladrón reaparece por un instante; señala hacia la corona del árbol más alto y se pierde en la niebla del dosel.
- **Estado Emocional de Alma:** Concentración y tensión implacable. El camino ya no ofrece tierra firme donde detenerse a respirar; cada paso requiere confianza ciega en su agilidad maternal.
- **Textos en Pantalla / Banners:**
  - *Prólogo del Nivel:*
    > *"El suelo firme ha desaparecido bajo un manto de espinas impenetrables.*  
    > *Las hojas marchitas no soportarán tu peso por mucho tiempo.*  
    > *Confía en tu aleteo y no te detengas: vacilar es caer."*
- **Pistas Narrativas en el Entorno:** Una pluma de Alma enganchada en una zarza, hojas arrancadas recientemente que caen girando hacia el fondo, y el eco lejano de un llanto apagado que proviene de las alturas (el primer huevo está cerca).

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** La penumbra del sotobosque impenetrable. Enredaderas carnosas con espinas de tamaño descomunal, hojas gigantescas de color verde parduzco que crujen al contacto, y una ligera neblina verde esmeralda que flota sobre el foso de zarzas.
- **Paleta de Color Principal:**
  - Espinas y Zarzas del Foso: Púrpura oscuro venenoso (`#2E142B`), espinas carmesí (`#8B1E2E`).
  - Hojas Quebradizas: Verde oliva seco (`#63733A`) que vira a rojo agrietado (`#B33927`) al pisarlas.
  - Troncos y Rocas Seguras: Pardo pizarra (`#2B2A27`) con musgo marchito.
  - Acentos de Luz: Haces de luz solar mortecina que perforan la espesura en tonos ocres (`#C79A45`).
- **Fondos Parallax (4 Capas):**
  - *Capa 0 (Fondo Lejano):* Siluetas oscuras de árboles retorcidos cubiertos de enredaderas espinosas.
  - *Capa 1 (Fondo Medio):* Bruma baja que se arremolina sobre el lecho de espinas.
  - *Capa 2 (Fondo Cercano / Gameplay):* Las plataformas suspendidas: ramas estrechas, hongos colocados en ángulos exigentes y hojas quebradizas.
  - *Capa 3 (Primer Plano / Foreground):* Zarzas desenfocadas que sobresalen desde el borde inferior de la pantalla, transmitiendo la sensación de que el peligro está rozando los pies del jugador.
- **Iluminación 2D (URP):**
  - Luz ambiental tenue y lúgubre (intensidad 0.45).
  - Puntos de luz en los nidos de checkpoint que contrastan fuertemente con la oscuridad del nivel.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + **Doble Salto (Aleteo Materno)**. *(Sin Dash, Sin Pisotón, Sin Rugido)*.
- **Filosofía de Nivel: "Suelo Cero"**:
  - El 85% de la superficie inferior es un foso continuo de zarzas letales. No existe suelo común de tierra donde descansar indefinidamente; la travesía se realiza de plataforma en plataforma.
- **Catálogo de Bloques y Mecánicas Complejas:**
  1. *Hojas Quebradizas Encadenadas (`CrumblingPlatform2D`):*
     - Tiemblan y colapsan a los **`0.65 segundos`**. Obligan al jugador a mirar adelante y saltar con determinación.
  2. *El Combo "Hongo ➔ Hoja Quebradiza":*
     - Rebote en un hongo elástico que aterriza directamente sobre una hoja quebradiza. El jugador debe absorber la inercia, no entrar en pánico y utilizar el **Aleteo Materno** en el momento justo para salir disparado a la siguiente roca.
  3. *Plantas Carnívoras en Salto Largo:*
     - Una planta carnívora situada en medio de dos hojas quebradizas. Exige sincronizar la velocidad de la carrera con el ciclo de apertura de la planta (1.8s) para pasar flotando con el doble salto.
  4. *Rutas Bifurcadas (El Dilema del Jugador):*
     - *Ruta Baja (Ritmo y Reflejo):* Cadena rápida de 3 hojas quebradizas sobre espinas. Fácil de ver, difícil de ejecutar bajo nervios.
     - *Ruta Alta (Ingenio y Precisión):* Hongo rebotador oculto que requiere un Súper Rebote milimétrico para alcanzar una rama segura elevada.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 28.0`):* Ubicado en una isla de roca sólida en medio del mar de zarzas tras la primera sección de hojas.
  - *Checkpoint 2 (`X = 60.0`):* Ubicado antes de la secuencia final de salto largo entre plantas y hojas.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Thornbound Shadows" (Sombras entre las Zarzas).
  - *Estilo:* Percusión apagada y tensa (golpes de conga sorda, maderas huecas) combinada con trémolos de chelo en registro grave y un oboe solitario que dibuja una melodía llena de incertidumbre.
  - *Tempo:* 88 BPM. Pulso constante y apremiante que induce al jugador a mantener el movimiento.
- **Efectos de Sonido (SFX):**
  - Ruido de fondo: Crujido constante de espinas y viento sordo que sopla a través de los matorrales.
  - Hojas Quebradizas: Crujido agudo de fibras vegetales desgarrándose (*¡CRACK!*), seguido por el silbido de la hoja al desprenderse hacia el fondo.
  - Zarzas al contacto: Sonido desgarrador punzante antes de activar el respawn.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Tileset:** Muros de espinas con púas afiladas (tiles superiores, inferiores y laterales), rocas musgosas oscuras.
- [ ] **Sprites Ambientales:** Hojas marchitas en estado entero, agrietado y fragmentado; lianas espinosas colgantes verticales.
- [ ] **VFX:** Polvillo de astillas vegetales y hojas marchitas al romperse la plataforma; niebla de partículas flotante sobre el foso de zarzas.
- [ ] **Audio:** Pista "Thornbound Shadows", SFX de crujido y rotura de hoja, SFX ambiental de viento entre zarzas.
