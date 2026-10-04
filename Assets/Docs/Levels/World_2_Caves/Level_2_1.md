> **Ficha de diseño para reconstrucción:** no hay escena implementada. Las notas sobre escenas, pruebas o assets existentes describen el prototipo anterior. La política vigente es jugar sin audio.

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
  - Movimiento base + Doble Salto + **Pisotón Sísmico (`GroundPound`)** desbloqueado al principio, tras el descenso al pozo.
- **Catálogo de Bloques y Mecánicas:**
  - *Suelo Agrietado (`BreakableGround2D`):*
    - Bloques de roca fracturada con colisionador sólido.
    - Soportan que Alma camine o salte sobre ellos sin romperse.
    - Al recibir el impacto de un **Pisotón Sísmico** desde arriba, durante la fase de picado (22 m/s tras 0.1s de preparación), el bloque se desactiva con Screen Shake. Una caída normal, incluso rápida, no lo rompe. Los fragmentos visuales quedan pendientes de producción.
  - *El Pozo Inicial (Tutorial Orgánico):*
    - El nivel comienza haciéndote caer en una fosa sin retorno hacia arriba.
    - La única salida es aprender a ejecutar el Pisotón sobre las losas agrietadas del fondo para abrir el túnel hacia la siguiente cámara.
- **Peligros:**
  - Estalagmitas en los laterales de los descensos y en dos huecos de salto. La vertical del primer tutorial y de las dos losas encadenadas tiene una superficie segura debajo.
  - Las losas se restauran al reaparecer; el desbloqueo del Pisotón se conserva.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Tileset:** Rocas de caverna subterránea azul/pizarra, estalagmitas afiladas, bloques de suelo agrietado (estados: entero, dañado, destruido).
- [ ] **Props:** Geodas de cristal con luz 2D, charcos de agua reflectante.
- [ ] **Sprites Alma:** Animación de Ground Pound (preparación en el aire, caída con cola erguida, pose de aterrizaje de impacto).
- [ ] **VFX:** Polvillo de roca y fragmentos de piedra al destruir un bloque agrietado; onda de choque sísmica circular al tocar tierra.


## 6. Implementación jugable actual

- **Escena:** `Assets/Scenes/World_2_Caves/Level_2_1.unity`, incluida después de `Boss_1` en Build Settings.
- **Abrir:** `Alma > 📂 Cargar Nivel 2-1`. Regenerar con `Tools > Alma > Construir Nivel 2-1 - Descenso a la Penumbra`.
- **Entrada:** Alma comienza en `(0, 13.2)` y cae hacia la cámara del altar. El Doble Salto está disponible; el Pisotón se obtiene tocando la geoda. Dash y Rugido aún no están disponibles en una partida nueva.
- **Tutorial:** primera losa de 3m, con aterrizaje seguro a 7m por debajo; permite probar caminar, saltar y ejecutar el Pisotón sin un peligro debajo.
- **Práctica:** segunda losa de 4m y descenso de 5.5m; una pared obliga a abrir el paso inferior.
- **Combinación:** salto de 3m hacia una cornisa 0.8m más alta, dos losas encadenadas atravesables con un solo Pisotón y salto final de 4m hacia una cornisa 1m más alta. Ambos saltos admiten Doble Salto.
- **Checkpoints:** altar `(1.5, 5.2)`, primer descenso `(14, -1.8)` y galería profunda `(28, -7.3)`. Límite de muerte por caída: `y = -27`.
- **Atmósfera:** luz global 0.20, luz de Alma de radio 3.5m, geodas cian/amatista y fondos con parallax. Geometría y decoración provisionales; tileset, animación específica, partículas siguen pendientes según el checklist.
- **Salida:** portal a `Level_2_2`. La escena 2-2 ya está disponible y se carga después de la finalización.
- **Validación automatizada:** entrada sin habilidades futuras, caída normal sobre losa intacta, desbloqueo y primera rotura, descenso encadenado con restauración al morir, y recorrido completo mediante entradas de movimiento/salto/Pisotón.
