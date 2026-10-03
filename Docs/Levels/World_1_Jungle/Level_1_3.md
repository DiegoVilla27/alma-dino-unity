> **Ficha de diseño para reconstrucción:** no hay escena implementada. Las notas sobre escenas, pruebas o assets existentes describen el prototipo anterior. La política vigente es jugar sin audio.

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
- **Catálogo de Bloques y Mecánicas Complejas (Diseño de precisión: Doble Salto Obligatorio y Columnas de Espinas):**
  1. *El Desfiladero de Espinas con Doble Salto Obligatorio:*
     - Huecos de aproximadamente `5.3m` combinados con desniveles y columnas de espinas. Se validan con el volumen del personaje y el coyote time: la distancia sola no demuestra que el doble salto sea obligatorio. El aleteo permite salvar altura con margen de aterrizaje.
     - Columnas verticales de espinas entre hojas que obligan a saltar hacia arriba, arquear la trayectoria y usar el segundo salto sobre las púas.
     - Hojas reducidas a **`1.4m`** con colapso anunciado a los **`0.75 segundos`**.
  2. *El Vuelo del Hongo entre Agujas Gigantes:*
     - Rebote en hongo elástico (`17.0 m/s`) que exige sobrevolar una torre de espinas de 7 metros de altura, amortiguar la caída sobre una hoja aérea de `1.3m` (0.75s) y aletear de inmediato a una segunda hoja aérea hacia el Checkpoint 1.
  3. *La Gran Bifurcación:*
     - *Ruta Baja ("El Campo Minado de Zarzas"):* 4 hojas quebradizas rápidas (`1.3m`) separadas por dientes verticales de espinas que se deben saltar por encima a ritmo vertiginoso.
     - *Ruta Alta ("El Paso del Vértigo"):* Rebote base (`18.5 m/s`, o `21.83 m/s` manteniendo salto) hacia hojas en el techo del bosque (`Y = 9.8`) divididas por una pared de espinas.
  4. *El Pasaje de las Dos Plantas Carnívoras (Zona 4):*
     - Dos plantas carnívoras con ciclos desfasados encadenadas con 3 hojas quebradizas (`0.75s`): Hoja 1 ➔ Salto sobre Planta 1 ➔ Hoja Central 2 ➔ Salto sobre Planta 2 ➔ Hoja 3. Margen suficiente para leer la siguiente plataforma; el reto está en encadenar movimientos.
  5. *Ascenso Final entre Hojas en Cascada (Zona 5):*
     - Rebote ascensor (`16.5 m/s`) que exige encadenar dos hojas quebradizas en subida antes de alcanzar la cornisa de cumbre.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 41.5`):* Isla estrecha (`2.2m`) tras el vuelo sobre la torre de espinas.
  - *Checkpoint 2 (`X = 65.5`):* Cornisa previa al corredor de las dos plantas carnívoras (`2.2m`).

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Tileset:** Muros de espinas con púas afiladas (tiles superiores, inferiores y laterales), rocas musgosas oscuras.
- [ ] **Sprites Ambientales:** Hojas marchitas en estado entero, agrietado y fragmentado; lianas espinosas colgantes verticales.
- [ ] **VFX:** Polvillo de astillas vegetales y hojas marchitas al romperse la plataforma; niebla de partículas flotante sobre el foso de zarzas.
