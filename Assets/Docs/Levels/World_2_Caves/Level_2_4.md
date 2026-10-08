> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_02/Level_2_4.unity` como escena de trabajo (copia de la de práctica) con su fondo [`Parallax_Level_2_4`](../../LevelPieces/Backgrounds.md) (dos capas: la Gran Geoda lejana y geodas abiertas con estalactitas, oscurecidas); el nivel completo aún no está construido. Las notas sobre escenas, pruebas o assets existentes describen el prototipo retirado. Las referencias de cámara a **8** son el criterio de la reconstrucción, no resultados de aquellas pruebas; sus valores de seguimiento están en [Alma](../../Player/Alma.md#cámara-daño-y-feedback). No habrá audio.

# 🗺️ Nivel 2-4: "El Laberinto de Geodas"
> **Mundo 2: Cuevas de Cristal** | **Función Pedagógica:** Evaluar + Segundo Rescate (El Techo Móvil & Huevo Azul)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En lo más profundo de las cuevas, Alma alcanza la Gran Geoda Sagrada. La caverna es colosal pero inestable: el techo de estalactitas gigantescas vibra con temblores rítmicos que amenazan con aplastar todo a su paso. En el centro de un pedestal de cristal puro descansa el segundo huevo robado: el **Huevo Azul**, emitiendo un fulgor sereno y frío como el hielo.
- **Estado Emocional de Alma:** Una combinación de angustia por el peligro inminente del colapso y una infinita ternura al reconocer la segunda vida que late ante ella.
- **El Momento del Rescate (Cinemática Diegética en Gameplay):**
  - Alma alcanza el pedestal y toca el **Huevo Azul**.
  - La tensión visual se desvanece y una luz cálida destaca el rescate:
  - *Texto del Rescate (Huevo Azul):*
    > *"Sentí tu latido contra la piedra fría.*  
    > *Ya somos dos. No descansaré hasta que estemos los cinco juntos.*  
    > *(Dos de cuatro rescatados)"*
  - De pronto, un estruendo brutal agrieta la caverna: una gigantesca mole acorazada surge rompiendo la pared de roca: el **Armadillo Prehistórico** bloquea la salida hacia la superficie.
- **Pistas Narrativas en el Entorno:** Fragmentos de cristal azul con huellas de cría, estalagmitas rotas por el peso del huevo, y el pulso auditivo del latido del huevo azul.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** La caverna de cristal más hermosa y peligrosa del juego. Paredes enteras de geodas amatista y zafiro que reflejan la luz como prismas gigantes. El techo está cubierto de estalactitas masivas que descienden y ascienden con un ritmo amenazante.
- **Paleta de Color Principal:**
  - Cristal Principal: Zafiro profundo (`#0077B6`) y azul hielo brillante (`#90E0EF`).
  - Huevo Azul: Resplandor celeste celestial (`#48CAE4`) con halo palpitante.
  - Techo de Amenaza: Roca volcánica oscura (`#1B1B1E`) con espinas de pedernal.
- **Iluminación 2D (URP):**
  - Luces volumétricas tenues que se filtran a través de los cristales gigantes, creando un ambiente etéreo casi submarino.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base completo + Doble Salto + Pisotón Sísmico.
- **Objetivo de Diseño: El Examen Maestro del Mundo 2**:
  1. *El Techo de Estalactitas Móvil (`CrushingCeiling2D`):*
     - Secciones del techo descienden pesadamente cada 3.0 segundos y tardan 2.0 segundos en retraerse.
     - Obliga a Alma a avanzar con velocidad y precisión, calculando cuándo correr y cuándo usar el **Pisotón Sísmico** para romper un suelo agrietado y refugiarse en un hueco inferior seguro antes del impacto del techo.
  2. *Balancines bajo Presión:*
     - Balancines que deben ser golpeados con el Pisotón mientras el techo desciende, catapultando a Alma justo a tiempo hacia la siguiente cámara.
  3. *Bichos Acorazados como Plataformas de Emergencia:*
     - Voltear escarabajos sobre la marcha para usarlos de plataforma mientras se esquivan estalactitas que caen.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 32.0`):* Tras la primera cámara de techo móvil.
  - *Checkpoint 2 (`X = 68.0`):* Justo antes de la antecámara del Huevo Azul.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Pedestal de geoda central, Huevo Azul con halo de luz 2D, techo aplastador con estalactitas afiladas.
- [ ] **VFX:** Polvo de cristal cayendo del techo antes de descender; partículas de destellos azulados flotantes.

## 6. Implementación jugable

- Escena: `Assets/Scenes/World_2_Caves/Level_2_4.unity`. Abrir con `Alma > 📂 Cargar Nivel 2-4`; regenerar con `Tools > Alma > Construir Nivel 2-4 - El Laberinto de Geodas`.
- Tres techos cinemáticos con movimiento en `FixedUpdate`, activación por proximidad y señal escrita antes de descender. Ciclo: 3 s arriba (últimos 0,8 s de aviso), descenso de 0,6 s, impacto de 0,4 s y retirada de 2 s. El ciclo completo dura 6 s; los 3 s del diseño son la ventana de preparación, no la duración total.
- Dos refugios bajo suelo quebradizo (X 12–16 y 75–79). Pound abre un hueco de 3 m de profundidad. El techo solo llega al suelo superior, dejando el refugio seguro. Los pasos de salida permiten recuperar altura con doble salto; el sello superior impide saltarse el descenso.
- Balancín en X 40, runa y puerta temporizada en X 46, salida elevada a 6,2 m. El aplastador presiona el extremo de Pound, dejando libre el extremo de catapulta y la trayectoria hacia la cornisa. Si Alma cae a la cámara inferior, la barrera permite volver hacia la izquierda aunque se hayan agotado los 4 s; se cierra al regresar y permite reintentar el balancín. Una señal y una rampa de regreso permiten regresar y subir de nuevo al balancín con un salto normal.
- Escarabajo acorazado en X 61,5: se voltea con la onda sísmica durante 3,5 s. El techo bajo impide ignorar su caparazón con un salto alto.
- Checkpoints en X 32 y X 68. Al morir se restauran suelos, techos, escarabajo, runa, contrapeso y balancín. El daño mantiene el respawn inmediato del resto del juego.
- Huevo Azul en `(92, 2)`, sobre pedestal. El rescate se guarda y activa la salida a `Boss_2`, incluso al repetir la escena con el huevo ya rescatado. El armadillo aparece como anuncio visual del siguiente combate; el jefe aún no está construido.
- Se reutiliza la atmósfera de cuatro capas de cuevas con geodas celestes. Arte de enemigos, techo, huevo y armadillo son provisionales. Efectos visuales del rugido, partículas y derrumbe quedan pendientes de producción.

- Validación: 28 pruebas PlayMode de los cuatro niveles de cuevas y 38 EditMode aprobadas. El recorrido del 2-4 usa entradas de movimiento, salto y Pound (incluido botón UI), sin teletransportes; comprueba ambos refugios, la catapulta, la plataforma de escarabajo, el rescate y la salida. Capturas de Unity verifican Alma visible, aviso escrito, refugio y santuario.

- Recuperación del balancín comprobada: desde la cámara inferior, esperar más de 4 s, volver por la puerta, subir la rampa, saltar al balancín y repetir la catapulta hasta la cornisa. La puerta se vuelve a cerrar al regresar al lado inicial; el avance por arriba sigue requerido. Las siete pruebas de puertas y catapultas del 2-2 también pasan con la apertura de retorno desactivada.
