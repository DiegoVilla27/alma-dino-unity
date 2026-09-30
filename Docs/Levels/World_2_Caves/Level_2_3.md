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
     - **Inmunidad:** Su caparazón de cristal refleja cualquier impacto. Si Alma salta sobre él con un salto normal, recibe daño y es empujada hacia atrás.
     - **Mecánica de Volteo Sísmico:** Si Alma ejecuta un **Pisotón Sísmico** cerca de él (en la misma plataforma o a menos de 2 metros), la onda de choque en la roca lo lanza por el aire y lo deja patas arriba durante **`3.5 segundos`**, exponiendo su vientre blando sobre el cual Alma puede saltar de forma segura o usarlo como plataforma de paso.
  2. *Murciélagos de Caverna (`CaveBat2D`):*
     - Duermen colgados del techo. Al pasar Alma por debajo, despiertan y realizan un vuelo en arco descendente.
     - Requieren esperar el vuelo y ejecutar un Doble Salto para esquivar la trayectoria en túneles verticales.
  3. *Puzle de Sincronía (Suelo Quebradizo + Escarabajo):*
     - Un escarabajo camina sobre un suelo agrietado. Si Alma hace el pisotón directamente sobre el suelo, rompe el suelo pero debe calcular aterrizar sobre el escarabajo ya volteado para no caer en el lecho de estalagmitas inferior.
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
