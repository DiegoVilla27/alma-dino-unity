> **Estado (7 de octubre de 2026):** el recorrido jugable completo está montado en `Assets/Scenes/World_01/Level_1_1.unity` (ver [§6 Implementación](#6-implementación-del-recorrido)). Faltan los textos en pantalla y los props narrativos. La cámara usa tamaño ortográfico **8** y el juego no usa audio.

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
- **Fondo parallax (2 capas):**
  - *Fondo lejano:* Siluetas distantes de la cordillera volcánica humeante bajo un cielo matutino.
  - *Fondo medio:* Troncos colosales de árboles centenarios desdibujados por una niebla dorada tenue.
- **Escenario jugable:** Plataformas de roca y tierra cubierta de hierba, lianas colgantes y flores silvestres.
- **Iluminación visual prevista:**
  - Atmósfera cálida de amanecer mediante los sprites de fondo y el parallax ya integrado en la escena de práctica.
  - Rayos solares y brillos del nido y altar mediante arte y partículas; el proyecto actual usa el pipeline integrado, sin luces URP 2D.

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
  - El nivel completo empezará sin enemigos hostiles. La escena actual de práctica no contiene enemigos; los peligros definitivos del nivel aún no están montados. El contacto letal o una caída profunda activan la reaparición.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [x] **Tileset:** kit de [terreno del Mundo 1](../../LevelPieces/Terrain.md) (piedra y raíces en este nivel). Ficha original: Tierra con hierba superior, tierra interior, bordes de raíz y rocas musgosas (16x16 o 32x32).
- [ ] **Props:** Nido de ramas y plumas destrozado, altar de piedra runal, gema flotante con halo emisor, flores tropicales.
- [x] **Sprites Alma:** Idle, Run y Jump integrados en su Animator.
- [ ] **Double Jump:** clip propio; por ahora reutiliza Jump y añade un efecto visual generado por código.

## 6. Implementación del recorrido

Escena `Level_1_1`: todo cuelga de un objeto raíz `Level_1_1` con los hijos `Terrain`, `Pieces`, `Hazards`, `Progression`, `Decor` y `CameraBounds`. Medidas en unidades; la `y` es la superficie donde se pisa.

**Estructura:** introducir, practicar, combinar y culminar. Antes del altar solo se usa el salto simple. Tras él, cada tramo pide el doble salto de una forma nueva: subir paredes, atravesar un tronco desde abajo, cruzar entre pilares y escalar a las copas. La altura del recorrido cambia en cada tramo: meseta, descenso a una hondonada, ascenso y cima, siempre con alturas de pantalla distintas.

**Regla de diseño:** ningún obstáculo depende de que el salto simple "no llegue" por poco. Lo que exige el doble salto pide **altura**: paredes de 2,2–2,5 m (el salto simple alcanza ≈1,56 m; el doble, ≈2,89 m).

| # | Tramo | x | Contenido | Qué enseña |
| --- | --- | --- | --- | --- |
| 1 | **El nido vacío** | −14 a 30 | Meseta de piedra (y 4). Alma empieza junto a la pared izquierda (x −8) y encuentra su nido vacío (checkpoint `Nest_Start`, x 0). Montículo de raíces de 1,2 m. Primer foso de **pinchos** visibles (3 m). Loma (y 4,4). | Correr y saltar; el primer peligro, con poco riesgo. |
| 2 | **El descenso** | 30 a 47 | Tres **piedras flotantes** que bajan (y 3,2 / 2 / 0,8) sobre un vacío (zona de muerte). | Saltos de precisión hacia abajo; la cámara baja con Alma. |
| 3 | **El santuario** | 47 a 66 | Hondonada de raíces (y −1) con el checkpoint `Nest_Hollow`. En el centro, sobre un estrado de piedra, el **Altar del Doble Salto** (x 56). | Momento de calma y descubrimiento. |
| 4 | **Primer aleteo** | 66 a 88 | Pared de 2,3 m (solo con doble salto). **Tronco atravesable** (y 3,8) que se cruza desde abajo. Otro muro de 2,2 m hasta la meseta alta (y 6) con el checkpoint `Nest_Upper`. | Usar el doble salto tres veces seguidas, cada una con un matiz. |
| 5 | **Hojas sobre espinas** | 88 a 118 | Cuatro **hojas que se desprenden** (1 s) en arco (y 6,6 / 7,6 / 6,6 / 6) sobre un lecho de pinchos a la vista. | No detenerse; ritmo constante. |
| 6 | **Pilares de espinas** | 118 a 132 | Dos **pilares con espinas** que salen de fosos de pinchos. Solo su cima es segura (y 7 y 7,6); sus lados matan. | Doble salto preciso para caer encima. |
| 7 | **Las copas** | 132 a 182 | Suelo de seguridad (y 7) con el checkpoint `Nest_Canopy`. Dos **troncos atravesables** escalonados (y 9,3 / 11,6), dos hojas (y 12,2 / 12,8) y la **cima** (y 12), un muro de 5 m que solo se alcanza por las copas. Allí está el **portal** (x 171, hacia `Level_1_2`). | Final: combinar todo en altura; caer no mata, solo obliga a repetir la subida. |

**Piezas usadas:**
- **Terreno:** 18 bloques `Platform_Ground_*_Jungle`: piedra en el recorrido principal, raíces en el nido, la hondonada y la pared, y zarzas oscuras en los suelos de los fosos de pinchos. Además, 3 `Platform_Floating_Stone_Jungle` con hojas colgantes.
- **Piezas:** 3 `Platform_OneWay_Universal` y 6 `Platform_CrumblingLeaf_Jungle`.
- **Peligros:** 4 `Trap_Spikes_Jungle` (fosos de 3, 20, 8 y 6 m), 2 `Trap_SpikedPillar_Jungle` y 1 `Trap_DeathZone_Universal` (descenso).
- **Progresión:** 5 `Resource_CheckpointNest_Universal`, 1 `Resource_AbilityAltar_DoubleJump`, 1 `Resource_LevelExitPortal_Universal` y `System_GameProgress`.
- Sin enemigos, como pide la ficha.

**Ambiente:**
- **Plantas:** 25 decoraciones del kit `FG_Jungle_*`: helechos, helechos con flores, hierba, hojas y hojas colgantes. Están detrás del terreno (orden −6), con la base tapada por el musgo. Hay tres más grandes y oscurecidas en primer plano (orden 20), solo en zonas seguras: al inicio, antes de las hojas y en la cima.
- **Polen:** el sistema de partículas `AmbientMotes`, hijo de la cámara, emite motas doradas que flotan en toda la vista (6 por segundo, vida de 7–11 s, con ruido suave; material `FX_Mote.mat`).

**Cámara:** `CameraBounds2D` de 188 × 28 centrado en (84; 10), es decir, x −10 a 178 e y −4 a 24, **sin** altura fija: el nivel sube y baja, así que la cámara usa su zona muerta vertical. Los bloques bajan hasta y = −8, por debajo de la vista.

**Pruebas:** `Level11Tests` (en la copia de trabajo de pruebas) juega cada tramo en la escena real con un "jugador" simulado que corre, salta y corrige en el aire hacia el punto de aterrizaje:
- Cada tramo se completa con el salto que le corresponde.
- El salto simple no sale de la hondonada.
- La cima no se puede escalar desde abajo.
- Progreso: inicio sin doble salto, partida antigua, el altar y la reanudación tras él.

Pasan las 12.

**Pendiente:**
- Textos en pantalla: prólogo y aviso del altar.
- Props narrativos: plumas, ramitas y huellas hacia la derecha.
- Añadir `Level_1_2` a **Build Settings** para que el portal pueda cargarla.
