# 🗺️ Nivel 2-3: "El Filo Resonante"
> **Mundo 2: Cuevas de Cristal** | **Función Pedagógica:** Complicar (Bichos Acorazados, Volteo Sísmico & Murciélagos)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Las cuevas se vuelven estrechas y hostiles. Los cristales ya no son meras gemas ornamentales: se han fusionado con la fauna subterránea. Pequeños escarabajos acorazados con caparazones afilados como cuchillas patrullan los pasajes estrechos, mientras bandadas de murciélagos ciegos anidan en los techos de estalactitas.
- **Estado Emocional de Alma:** Alerta máxima. No todo obstáculo se esquiva saltando por encima; algunos requieren enfrentarse a la hostilidad con el impacto exacto.
- **Pistas Narrativas en el Entorno:** Marcas de mordiscos en los cristales y huellas que muestran que los ladrones tuvieron que huir despavoridos de las criaturas acorazadas.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Túneles claustrofóbicos con prismas de cristal que reflejan la luz como espejos. Los caparazones de los enemigos emiten destellos de luz cortante que advierten de su invulnerabilidad frontal.
- **Paleta de Color Principal:**
  - Cristal Afilado: Turquesa brillante (`#00F5D4`) y cian cortante (`#00BBF9`).
  - Caparazón Enemigo: Blanco perla reflectante (`#E0AAFF`) y espinas de diamante.
  - Vientre Vulnerable: Piel blanda naranja rojiza (`#F77F00`).
- **Iluminación 2D (URP):**
  - Efectos de refracción en los cristales al pasar cerca de las fuentes de luz.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico.
- **Nuevos Enemigos & Desafíos Complejos:**
  1. *Escarabajo de Cristal Acorazado (`CrystalBeetle2D`):*
     - Patrulla lentamente las plataformas estrechas.
     - **Inmunidad:** Su caparazón de cristal refleja cualquier impacto. Si Alma salta sobre él con un salto normal, recibe daño y vuelve al checkpoint, coherente con el sistema actual de peligros del juego.
     - **Mecánica de Volteo Sísmico:** Si Alma ejecuta un **Pisotón Sísmico** cerca de él (con radio de 2 metros alrededor del impacto), la onda de choque en la roca lo lanza por el aire y lo deja patas arriba durante **`3.5 segundos`**, exponiendo su vientre blando sobre el cual Alma puede saltar de forma segura o usarlo como plataforma de paso.
  2. *Murciélagos de Caverna (`CaveBat2D`):*
     - Duermen colgados del techo. Al pasar Alma por debajo, despiertan y realizan un vuelo en arco descendente.
     - Requieren esperar el vuelo y ejecutar un Doble Salto para esquivar la trayectoria en túneles verticales.
  3. *Puzle de Sincronía (Suelo Quebradizo + Escarabajo):*
     - Un escarabajo camina sobre un suelo agrietado. El Pisotón rompe el suelo y voltea el escarabajo. Una pequeña repisa de recuperación permite terminar el picado sin exigir un aterrizaje ciego sobre un enemigo aún cayendo. Desde ella, Alma salta al vientre del escarabajo antes de los 3.5s y continúa por debajo del cierre de roca, usando un salto corto y un Doble Salto para salir del lecho de estalagmitas.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 26.0`):* Tras la primera galería de escarabajos.
  - *Checkpoint 2 (`X = 58.0`):* Antes del túnel vertical infestado de murciélagos.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Chitin and Crystal" (Quitina y Cristal).
  - *Estilo:* Melodía tensa y percusiva con crótalos metálicos, contrabajo en staccato y arpegios rápidos de clavicémbalo/arpa que transmiten peligro inminente y reflejos afilados.
  - *Tempo:* 105 BPM.
- **Efectos de Sonido (SFX):**
  - Pisotón y Onda de Choque: Golpe de baja frecuencia con temblor de pantalla y resonancia de diapasón.
  - Escarabajo Volteándose: Tintineo de cristales rodando (*¡clink-clink-klink!*) y pataleo cómico indefenso.
  - Chillido de Murciélago: Sonido ultrasónico sutil al despertar.

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Enemigos:** Escarabajo de cristal (caminata, reacción al sismo, patas arriba pataleando, derrota), Murciélago de cueva (colgado, vuelo en arco).
- [ ] **VFX:** Destello reflectante en el caparazón del escarabajo; onda expansiva sísmica azul en el suelo de piedra.
- [ ] **Audio:** Pista "Chitin and Crystal", SFX de tintineo de cristal al voltear, chirrido de murciélago.


## 6. Implementación jugable actual

- **Escena:** `Assets/Scenes/World_2_Caves/Level_2_3.unity`. Abrir desde `Alma > 📂 Cargar Nivel 2-3`; regenerar con `Tools > Alma > Construir Nivel 2-3 - El Filo Resonante`.
- **Entrada:** `(0, 0.7)`, con Doble Salto y Pisotón. Una entrada directa en partida nueva también habilita ambas habilidades; Dash y Rugido siguen reservados para mundos posteriores.
- **Galería de escarabajos:** patrullas de 0.7 m/s en X≈12 y X≈21 bajo techos bajos. El caparazón es peligroso ante un salto normal; un impacto sísmico cercano expone el vientre durante 3.5s. Una barra indica la duración restante. Recuperar el caparazón vuelve a ser peligroso, incluso si Alma sigue encima.
- **Checkpoint 1:** `(26, 0.7)`.
- **Puzle de sincronía:** tres escalones hasta una losa a Y=3.6. El Pisotón desde X≈40.5 rompe la losa, voltea al escarabajo y lleva a una repisa segura. El escarabajo cae sobre un pequeño apoyo de roca; su vientre sirve como plataforma antes de recuperarse. Un cierre superior evita continuar por encima de la losa; el paso inferior admite un salto corto seguido de Doble Salto hacia la cornisa de X=47.5.
- **Checkpoint 2:** `(58, 2.2)`, antes del ascenso.
- **Murciélagos:** tres vuelos en arco con aviso de 0.65s (símbolo `!` y cambio de color), recorrido de 1.6s y descanso de 2s. Solo el vuelo causa daño. Esperar en las cornisas y saltar durante el descanso permite subir desde Y=1.5 hasta Y=7.5.
- **Salida:** portal en `(97, 9)` hacia `Level_2_4`. La escena 2-4 ya está construida y se carga tras completar el nivel.
- **Reintentos:** morir restaura losa, patrullas y murciélagos y conserva las habilidades. Estalagmitas protegen el fondo del puzle y del ascenso; límite de caída Y=-10.
- **Arquitectura:** módulo `AlmaDino.Features.Enemies` sin referencias internas a Player. La onda y el peligro condicional usan interfaces de Core; parámetros de enemigos en `CrystalEnemyConfig.asset` y radio sísmico en el perfil de física de Alma.
- **Estado de producción:** iluminación turquesa, caparazón perla, vientre naranja y cuerpos provisionales. Refracción, sprites definitivos, VFX y audio siguen pendientes según el checklist.
