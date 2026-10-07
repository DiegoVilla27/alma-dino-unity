> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_01/Level_1_1.unity` como escena de práctica, pero aún no implementa el nivel completo descrito aquí. Las notas antiguas de escenas y pruebas corresponden al prototipo retirado. La cámara actual usa tamaño ortográfico **8** y el juego no usa audio.

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

- [ ] **Tileset:** Tierra con hierba superior, tierra interior, bordes de raíz y rocas musgosas (16x16 o 32x32).
- [ ] **Props:** Nido de ramas y plumas destrozado, altar de piedra runal, gema flotante con halo emisor, flores tropicales.
- [x] **Sprites Alma:** Idle, Run y Jump integrados en su Animator.
- [ ] **Double Jump:** clip propio; por ahora reutiliza Jump y añade un efecto visual generado por código.
