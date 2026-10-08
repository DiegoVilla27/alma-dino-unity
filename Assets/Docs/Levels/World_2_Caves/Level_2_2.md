> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_02/Level_2_2.unity` como escena de trabajo (copia de la de práctica) con su fondo [`Parallax_Level_2_2`](../../LevelPieces/Backgrounds.md) (dos capas: galería lejana y estalagmitas con puente de roca); el nivel completo aún no está construido. Las notas sobre escenas, pruebas o assets existentes describen el prototipo retirado. Las referencias de cámara a **8** son el criterio de la reconstrucción, no resultados de aquellas pruebas; sus valores de seguimiento están en [Alma](../../Player/Alma.md#cámara-daño-y-feedback). No habrá audio.

# 🗺️ Nivel 2-2: "La Galería de Ecos"
> **Mundo 2: Cuevas de Cristal** | **Función Pedagógica:** Practicar (Balancines de Piedra, Catapultas & Pisotón Sísmico)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Alma se adentra en una inmensa cámara subterránea donde los techos alcanzan alturas vertiginosas. Las huellas y los cristales fracturados revelan que los ladrones activaron antiguos mecanismos de roca.
- **Estado Emocional de Alma:** Curiosidad analítica y cálculo. La fuerza bruta no basta; debe aplicar su peso con precisión quirúrgica en el momento y lugar indicados.
- **Pistas Narrativas en el Entorno:** Marcas circulares en el suelo donde los ladrones han rodado objetos pesados, cristales partidos recientemente y gotas de savia de la jungla que gotean desde fisuras del techo.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un templo natural de roca y cristal con enormes losas de basalto en equilibrio sobre pivotes de piedra pulida. Grandes geodas moradas y verdes actúan como lámparas orgánicas en los laterales de la caverna.
- **Paleta de Color Principal:**
  - Roca Cavernosa: Gris grafito (`#2B2D42`) y azul medianoche (`#1D3557`).
  - Balancines Mecánicos: Basalto pulido oscuro (`#111118`) con marcas rúnicas amarillas (`#E9D8A6`).
  - Cristales de Iluminación: Esmeralda cristalina (`#52B788`) y ámbar brillante (`#EE9B00`).
- **Fondos Parallax:** implementados con **2 capas**, como en el Mundo 1: la lejana corresponde a la capa 0 y la media a la capa 1. La capa 2 la forman las piezas jugables y no se usa capa de primer plano. Ficha original:
  - *Capa 0 (Fondo Lejano):* Galería cavernosa infinita que se pierde en la negrura.
  - *Capa 1 (Fondo Medio):* Estalagmitas gigantescas y puentes naturales de roca en silueta.
  - *Capa 2 (Fondo Cercano / Gameplay):* Los balancines basculantes, bloques contrapeso y compuertas de piedra.
  - *Capa 3 (Primer Plano / Foreground):* Enredaderas subterráneas pálidas que cruzan la pantalla verticalmente.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico.
- **Mecánica Estrella: Los Balancines de Basalto (`SeesawPlatform2D`):**
  - Plataformas largas de piedra apoyadas sobre un fulcro central.
  - Al caminar sobre un extremo, bascula lentamente.
  - **Interacción con Pisotón Sísmico:**
    - Si Alma salta y ejecuta un **Pisotón Sísmico** sobre un extremo del balancín:
      1. El extremo impactado desciende de golpe contra el suelo.
      2. El extremo opuesto sale catapultado violentamente hacia arriba.
      3. Si sobre el extremo opuesto hay un bloque de piedra, este es lanzado al aire para golpear un interruptor en el techo o romper un techo frágil.
      4. Si Alma corre hacia el extremo elevado, es catapultada durante una ventana de 2.8s tras el impacto, a 15 m/s (altura base aproximada de 5.2m). El Doble Salto se recarga.
- **Compuertas Rúnicas Temporizadas:**
  - Las runas del techo se activan únicamente con un contrapeso lanzado que aún asciende. Abren su compuerta durante 4 segundos. La compuerta final necesita ambas runas activas simultáneamente. Una barra verde muestra el tiempo restante; el cierre espera si Alma ocupa el hueco.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 30.0`):* Tras dominar el primer puzle de balancín simple.
  - *Checkpoint 2 (`X = 65.0`):* Antes del gran balancín doble encadenado.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Balancín de basalto con pivote central, bloque cúbico de piedra contrapeso, interruptor de techo, compuerta rúnica deslizante.
- [ ] **VFX:** Polvo de tiza y astillas de piedra despedidas al bascular con violencia; brillo rúnico al activar el interruptor.


## 6. Implementación jugable actual

- **Escena:** `Assets/Scenes/World_2_Caves/Level_2_2.unity`. Abrir con `Alma > 📂 Cargar Nivel 2-2`; regenerar desde `Tools > Alma > Construir Nivel 2-2 - La Galería de Ecos`.
- **Entrada:** `(0, 0.7)`. Doble Salto y Pisotón disponibles desde el inicio, incluida la entrada directa en una partida nueva. Dash y Rugido se reservan para mundos posteriores.
- **Tutorial seguro:** balancín centrado en X=10 y compuerta en X=20. Un salto normal permite subir; caminar inclina la tabla, pero solo un Pisotón en su extremo lanza el contrapeso hacia la runa de techo.
- **Checkpoint 1:** `(30, 0.7)`.
- **Práctica de catapulta:** balancín en X=40, runa en `(42.3, 6)` y compuerta en X=46. Tras golpear el extremo izquierdo, correr al derecho impulsa a Alma hacia una cornisa con superficie a Y=6.2. La cámara sigue el ascenso. Debajo hay suelo seguro para volver a intentar.
- **Checkpoint 2:** `(65, 0.7)`.
- **Cadena final:** balancines en X=72 y X=82. Usar el impulso del primero para pisotear el segundo; la compuerta en X=90 solo abre cuando las dos runas están activas. El trayecto completo está comprobado dentro de la ventana de 4s.
- **Salida:** hueco de 4m con estalagmitas entre X=96 y X=100; admite Doble Salto. Portal en X=109 hacia `Level_2_3`; la escena 2-3 ya está disponible y se carga después de completar la galería.
- **Reintentos:** cada contrapeso vuelve a su posición a los 4.7s. Morir restaura tablas, pesos, runas y compuertas y conserva las habilidades.
- **Física:** tablas cinemáticas con rotación en `FixedUpdate`, inclinación limitada a ±18° y retorno gradual; contrapesos dinámicos de masa 4, gravedad 1.8 y velocidad inicial de 14 m/s. La asistencia de catapulta usa una velocidad acotada para ofrecer resultados predecibles. Ajustes editables en `EchoSeesawConfig.asset`.
- **Visuales:** runas ámbar/esmeralda, señales de Pisotón y carrera, iluminación local y cuatro capas de profundidad. Tileset, VFX definitivos siguen pendientes según el checklist.
- **Pruebas:** entrada y UI, inclinación por aterrizaje normal, Pisotón desde UI, temporización y repetición, cornisa alta, ambas runas, reinicio al morir, cierre seguro y recorrido completo sin teletransportes.
