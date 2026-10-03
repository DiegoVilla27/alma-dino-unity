> **Estado:** documento rector de diseño para reconstruir el juego. No hay proyecto Unity implementado en esta rama. Las notas de fases y tiempos son planificación, no estado de producción.

---

# ALMA: MOTHER'S ROAR
## Documento de Diseño de Juego (GDD)

**Versión:** 1.0
**Género:** Plataformas 2D de precisión con progresión tipo Metroidvania-lite
**Plataformas:** PC (prioritario), consolas (si el alcance lo permite)
**Motor:** Unity (C#) o Godot (GDScript)
**Duración estimada:** 4-6 horas
**Público objetivo:** Jugadores de plataformas 2D que valoran la narrativa atmosférica y emocional (referencias: *Celeste*, *Hollow Knight*, *Ori and the Blind Forest*, *Yoshi's Island*)

---

## 1. RESUMEN EJECUTIVO

**Alma: Mother's Roar** es un juego de plataformas 2D de precisión y puzles ligeros centrado en una narrativa emocional. El jugador controla a Alma, una madre dinosaurio que atraviesa cuatro biomas hostiles para rescatar a sus cuatro hijos robados. Cada mundo desbloquea una habilidad acumulativa que refleja su creciente determinación y su conexión con su prole.

El juego combina:
- **Control de personaje responsivo** al estilo *Celeste* (coyote time, jump buffer, ajuste fino de gravedad).
- **Progresión lineal** con puertas de habilidad (no backtracking, no mundo abierto).
- **Narrativa ambiental** contada con textos breves entre niveles.
- **Jefes diseñados como puzles** de habilidad, no como combate técnico.

**Pilares de diseño:**
1. **Sensación táctil perfecta** — El control debe sentirse bien antes que cualquier otra cosa.
2. **Progresión pedagógica** — Cada habilidad se enseña, se practica, se complica y se evalúa.
3. **Emoción contenida** — Nada de melodrama. La historia se cuenta con silencio, atmósfera y textos mínimos.

---

## 2. ALCANCE Y LIMITACIONES

### 2.1. Lo que SÍ tendrá el juego

| Elemento | Cantidad |
|---|---|
| Niveles de plataformas | 16 (4 mundos × 4 niveles) |
| Jefes | 4 (uno al final de cada mundo) |
| Habilidades jugador | 4 (acumulativas) |
| Biomas | 4 (Jungla, Cuevas de Cristal, Pantano, Volcán) |
| Escenas narrativas | Prólogo + 4 rescates + Epílogo |
| Duración objetivo | 4-6 horas |

### 2.2. Lo que NO tendrá el juego

- Multijugador.
- Sistemas de RPG (sin stats, sin niveles, sin inventario).
- Combate complejo (sin barra de vida enemiga, sin combos).
- Mundos abiertos o backtracking (progresión estrictamente lineal).
- Diálogos con árboles de decisión.
- Sistema de crafteo o economía.
- Modo New Game+ (en la versión 1.0).

### 2.3. Supuestos y restricciones

- **Equipo:** 1-2 personas (programador + artista, o todo en uno).
- **Presupuesto:** Arte propio o asset packs comprados. Sin encargos externos.
- **Plazo:** 12-16 semanas a tiempo parcial, o 8-10 a tiempo completo.
- **Alcance blindado:** Si una idea no cabe en 16 niveles, se corta. Sin excepciones.

---

## 3. MECÁNICAS DE JUEGO

### 3.1. Personaje: Alma

Alma es una madre dinosaurio ágil, de movimientos rápidos y precisos. No tiene barra de vida en el sentido tradicional: recibe daño por contacto con peligros ambientales (espinas, lava, enemigos) y, al recibir un golpe, reaparece en el último checkpoint. Esto mantiene la tensión sin necesidad de un sistema de salud complejo.

### 3.2. Movimiento base (desde el Nivel 1)

- Correr izquierda/derecha.
- Salto simple.
- Caída con gravedad ajustable.

### 3.3. Sistema de progresión de habilidades

Las habilidades se desbloquean al inicio de cada mundo y son **acumulativas**: ninguna reemplaza a otra. Se combinan entre sí.

| Mundo | Niveles | Habilidad | Descripción | Combinación |
|---|---|---|---|---|
| **1. Jungla** | 1-4 | **Doble Salto / Aleteo** | Segundo impulso en el aire; mantener salto conserva la altura y soltarlo acorta el ascenso. | Solo habilidades base. |
| **2. Cuevas** | 5-8 | **Pisotón Sísmico** | En el aire, pulsar abajo para caer rápido y golpear el suelo. Rompe suelos frágiles y aturde enemigos. | Requiere Doble Salto para alcanzar altura. |
| **3. Pantano** | 9-12 | **Dash Aéreo** | Impulso horizontal rápido en el aire. Otorga invulnerabilidad momentánea frente a vientos. | Requiere Doble Salto + Dash para cruzar abismos largos. |
| **4. Volcán** | 13-16 | **Rugido de Choque** | Proyectil cónico de corto alcance que empuja objetos y activa interruptores. | Requiere combinar todas las anteriores en puzles de *timing*. |

### 3.4. Física del personaje (Game Feel)

Perfil objetivo para todos los mundos. Los detalles y criterios de prueba viven en [la ficha de Alma](Docs/Player/Alma.md):

| Parámetro | Valor sugerido | Propósito |
|---|---|---|
| Move Speed | 7.0 m/s | Velocidad horizontal máxima. |
| Aceleración / frenado | 0.10 / 0.08 s | Inercia breve; control aéreo en 0.13 s. |
| Gravedad base | 21.58 m/s² | Gravedad de Unity × escala 2.2. |
| Jump Force | 8.2 m/s | Altura teórica de 1.56 m manteniendo salto. |
| Double Jump Force | 7.6 m/s | Restablece al menos esa velocidad; no suma impulsos ilimitados. |
| Gravedad de caída / salto soltado | ×1.8 / ×2.4 | Caída con peso y salto de altura variable. |
| Velocidad terminal normal | 20 m/s | El pisotón usa su límite propio de 22 m/s. |
| Dash Distance / Time | 6 m en 0.2 s | Impulso rápido; congela la gravedad en Y durante el dash. |
| Ground Pound Speed | 22.0 m/s hacia abajo | Caída seca con *wind-up* de 0.1 s antes de caer. |
| Coyote Time | 0.14 s | Permite saltar tras abandonar el borde. |
| Jump Buffer | 0.12 s | Registra el salto pulsado justo antes de aterrizar. |

### 3.5. Máquina de estados finita (FSM)

Para evitar código espagueti, el `PlayerController` se implementa como una FSM:

```
[IDLE] <--> [RUN] <--> [JUMP / FALL] <--> [DOUBLE_JUMP]
   |            |              |                 |
   v            v              v                 v
[GROUND_POUND] <-----------> [AIR_DASH] <--> [ROAR_ACTION]
```

Cada estado gestiona sus propias transiciones y animaciones. Ningún estado se solapa con otro. Las pulsaciones se capturan en cada fotograma y se consumen una sola vez en el paso fijo de física; las transiciones y los impulsos se resuelven en ese paso.

### 3.6. Loop de juego

1. **Inicio:** Alma entra al nivel.
2. **Desafío:** Superar puzles de plataformas usando las habilidades actuales.
3. **Hito:** Encontrar el huevo robado (finales de nivel 4, 8, 12, 16) o vencer al jefe (nivel extra tras cada rescate).
4. **Narrativa:** Texto breve emocional tras el rescate.
5. **Progreso:** Desbloquear siguiente nivel o mundo.

---

## 4. ESTRUCTURA DE NIVELES Y MUNDOS

Cada mundo sigue la metodología Nintendo: **Introducir → Practicar → Complicar → Evaluar**.

### MUNDO 1: JUNGLA ESMERALDA (Niveles 1-4 + Jefe)

- **Atmósfera:** Verde, frondosa, luz filtrada por árboles, movimiento de aves y hojas.
- **Mecánica foco:** Doble Salto / Aleteo.
- **Obstáculos:** Ramas altas, espinas en el suelo, plataformas que se desmoronan.
- **Enemigos:** Mono Ladrón (huye de Alma), Plantas carnívoras estáticas.

| Nivel | Función pedagógica | Descripción |
|---|---|---|
| **1** | Introducir | Abismo infranqueable. Alma recoge la gema de energía materna y desbloquea el Doble Salto. Tutorial mediante el entorno, con un aviso breve al despertar la habilidad. |
| **2** | Practicar | Verticalidad y ramas elásticas. Lianas que exigen calcular el segundo salto en el punto más alto de la parábola. |
| **3** | Complicar | Suelo de zarzas con espinas. Plataformas de hojas que colapsan a los 0.75 s; se enseñan con 1.0 s en 1-1/1-2 y se evalúan con 0.65 s en 1-4. |
| **4** | Evaluar + Rescate | Carrera ascendente hacia la copa del árbol más alto. **Huevo 1 (Verde)**. |

**Jefe 1 — Mono Ladrón Gigante**
- **Arena:** Copa de árbol con tres plataformas flotantes.
- **Fase 1:** El mono se cuelga del centro y lanza frutos rodantes. Se esquivan con Doble Salto.
- **Fase 2:** Desciende agotado durante 3.2, 2.8 y 2.4 segundos según el ciclo. Alma salta sobre su cabeza. 3 impactos para vencer; se conserva el ciclo alcanzado tras morir.

---

### MUNDO 2: CUEVAS DE CRISTAL (Niveles 5-8 + Jefe)

- **Atmósfera:** Oscura, fría, cristales que emiten luz de color, ecos.
- **Mecánica foco:** Pisotón Sísmico.
- **Obstáculos:** Bloques agrietados, placas de presión pesadas, interruptores cronometrados.
- **Enemigos:** Murciélagos (patrón aéreo), Armadillos (inmunes sin aturdir).

| Nivel | Función pedagógica | Descripción |
|---|---|---|
| **5** | Introducir | Alma cae en un pozo sin salida. Un suelo de estalagmitas agrietadas solo cede ante un impacto vertical. Desbloquea el Pisotón. |
| **6** | Practicar | Balancines de piedra. Al caer con fuerza en un extremo, el otro catapulta un bloque que abre una compuerta. |
| **7** | Complicar | Bichos de cristal con caparazón reflectante. Saltar sobre ellos hace daño; solo el Pisotón los voltea. |
| **8** | Evaluar + Rescate | Laberinto resonante con techo móvil de estalactitas. **Huevo 2 (Azul)**. |

**Jefe 2 — Armadillo Prehistórico**
- **Arena:** Túnel cerrado con dos niveles de altura.
- **Fase 1:** Se enrolla en bola y rebota en las paredes a alta velocidad. Esquiva con Doble Salto preciso.
- **Fase 2:** Al chocar contra cristal queda mareado pero cubierto. Alma sube a una plataforma superior y ejecuta Pisotón sobre su punto débil. 3 impactos.

---

### MUNDO 3: PANTANO DE VIENTO Y NIEBLA (Niveles 9-12 + Jefe)

- **Atmósfera:** Gris, neblinosa, agua estancada, ráfagas de viento visibles.
- **Mecánica foco:** Dash Aéreo.
- **Obstáculos:** Abismos anchos, corrientes de aire, barreras de madera que solo el Dash rompe.
- **Enemigos:** Insectos voladores rápidos, Sapos venenosos.

| Nivel | Función pedagógica | Descripción |
|---|---|---|
| **9** | Introducir | Fosos de lodo tóxico demasiado anchos para el Doble Salto. La combinación Salto → Salto → Dash cubre distancias largas. |
| **10** | Practicar | Géiseres de aire horizontales que empujan hacia atrás. El Dash da invulnerabilidad momentánea frente al empuje. |
| **11** | Complicar | Esporas alineadas para encadenar Dash horizontal. En tierra: suelo agrietado para Pisotón, barrera de Dash y sapos venenosos. |
| **12** | Evaluar + Rescate | Ascenso por ramas con gas por secciones, esporas horizontales y balancín de Pisotón hacia la copa. **Huevo 3 (Morado)** en un sauce ancestral. |

**Jefe 3 — Pterodáctilo Alfa**
- **Arena:** Plataformas inestables sobre un abismo con niebla.
- **Fase 1:** Vuela fuera del alcance y bate alas creando ráfagas que intentan tirar a Alma.
- **Fase 2:** Se lanza en picado horizontal a altura señalada. Alma usa Doble Salto y Dash Aéreo frontal contra su cabeza cian. 3 impactos.
- **Fase 3:** Cada impacto elimina una rama lateral y acelera el siguiente picado. La muerte reinicia el ciclo actual conservando impactos y ramas rotas. `Boss_3` jugable; victoria guarda Mundo 3 y abre el paso hacia Mundo 4.

---

### MUNDO 4: CIMA VOLCÁNICA (Niveles 13-16 + Jefe Final)

- **Atmósfera:** Roja/naranja, ceniza cayendo, ríos de lava, tensión alta.
- **Mecánica foco:** Rugido de Choque + Combinación Total.
- **Obstáculos:** Rocas gigantes que se empujan con el rugido, chorros de lava, interruptores lejanos, puzles de timing.
- **Enemigos:** Salamandras de fuego, Elementales de lava.

| Nivel | Función pedagógica | Descripción |
|---|---|---|
| **13** | Introducir | `Level_4_1` jugable: fumarola que desbloquea Rugido, cono frontal de 3m, rocas que avanzan 5m y forman apoyos sobre lava, vapor con aviso y checkpoints. |
| **14** | Practicar | `Level_4_2` jugable: cinco campanas elevadas responden al Rugido frontal hasta 8m y apagan su puerta de llamas durante 5s. Práctica en suelo, Doble Salto + Rugido aéreo y cadena final de tres campanas sobre lava, con checkpoints X=30 y X=62. |
| **15** | Complicar | `Level_4_3` jugable: una cadena inicial de cuatro habilidades devuelve un meteorito; zona central con elección entre cornisas colapsantes y piedras con vapor; cierre con Pisotón que provoca una erupción, ascenso bajo presión y Dash final. Salamandras activas y checkpoints X=32 y X=68. |
| **16** | Evaluar + Rescate | `Level_4_4` jugable: cuatro pruebas distintas de Doble Salto, Pisotón, Dash y Rugido, con salamandras, corriente de fuego cronometrada y puente de basalto. **Huevo 4 (Rojo)** en pedestal tras salto final; rescate persistente, calma, aura de huevos y anticipación del Rey Ladrón. Checkpoints X=35 y X=75. |

**Jefe Final — El Rey Ladrón (T-Rex Anciano)**
- **Estado:** `Boss_Final` jugable, conectado al 4-4; tres fases con checkpoint y epílogo silencioso. Solo el golpe definitivo completa Mundo 4.
- **Estado del rescate:** Alma entra con los cuatro huevos ya recuperados en 4-4. Los deja en un saliente protegido durante el combate.
- **Arena:** Cornisas de basalto sobre magma; el espacio se reduce en la última fase.
- **Fase 1:** Doble Salto + Dash para esquivar embestidas; Pisotón sobre la placa dorsal expuesta durante 4 segundos. Primer impacto.
- **Fase 2:** Rugido para devolver un meteorito; Pisotón sobre el punto débil expuesto. Segundo impacto.
- **Fase 3:** Ascenso ante magma creciente, combinando las cuatro habilidades para desprender la estalactita final. Tercer impacto.
- **Recuperación:** Checkpoint por fase; reintentar restaura plataformas, mecanismos y altura del magma a un estado seguro de esa fase. Después, Alma recoge a sus cuatro hijos del refugio y escapa hacia el epílogo.

---

## 5. NARRATIVA

La historia se cuenta exclusivamente con textos breves y atmósfera visual. Sin voces, sin cinemáticas largas.

### 5.1. Prólogo (inicio del Nivel 1)

> *"La tierra tembló una sola vez. Cuando regresé al nido con comida, el silencio era absoluto. No estaban. Si tengo que cruzar el continente entero a pie, mis pequeños volverán a sentir el calor de mis plumas."*

### 5.2. Rescates de huevo

Al tocar cada huevo, la escena cambia visualmente y aparece un texto breve:

- **Huevo 1 (Verde):** *"Aún estás tibio... Mamá llegó a tiempo. Ya estás a salvo."*
- **Huevo 2 (Azul):** *"Sentí tu latido contra la piedra fría. Ya somos dos. No descansaré hasta que estemos los cinco juntos."*
- **Huevo 3 (Morado):** *"El cascarón tiembla... falta muy poco para que rompas a cantar. Solo nos falta uno."*
- **Huevo 4 (Rojo):** *"Los cuatro están aquí. Mi nido vuelve a estar completo."*

### 5.3. Epílogo (tras el jefe final)

Alma llega a un valle seguro al atardecer. Los cuatro huevos se quiebran suavemente:

> *"No hubo tormenta, volcán ni bestia que pudiera apagar este latido. Bienvenidos al mundo, pequeños."*

---

## 6. DIRECCIÓN DE ARTE

### 6.1. Estilo visual

- **2D Pixel Art** estilo 16-bit moderno (referencias: *Owlboy*, *Blasphemous*, *Hyper Light Drifter*) **o** 2D dibujado a mano (referencia: *Hollow Knight*, *Ori*).
- **Paleta diferenciada por bioma:**
  - Jungla: verdes saturados, luz cálida filtrada.
  - Cuevas: azules fríos, cristales con emisión de color.
  - Pantano: grises y verdes apagados, niebla con capas de parallax.
  - Volcán: rojos, naranjas, negros, ceniza en partículas.

### 6.2. Iluminación

Uso intenso de luz dinámica 2D (Unity URP 2D Renderer o Godot CanvasModulate + Light2D) para:
- Diferenciar biomas.
- Destacar huevos y checkpoints.
- Crear siluetas en momentos clave.

### 6.3. Animación

Alma necesita, como mínimo:
- Idle, Run, Jump, Fall, Double Jump, Ground Pound, Air Dash, Roar, Hurt, Rescate.
- Animaciones cortas (4-8 frames) para pixel art, con *smear frames* en acciones rápidas (Dash, Pisotón).

---

## 7. EXPERIENCIA SIN AUDIO

El juego no tendrá música ni efectos de sonido. Los ataques, habilidades, campanas, checkpoints y rescates comunicarán su estado con animación, color acompañado de forma/símbolo, partículas y cambios visibles del escenario. Ninguna acción necesaria dependerá de una señal sonora.

---

## 8. INTERFAZ (HUD) Y FEEDBACK

HUD minimalista y diegético.

### 8.1. Indicador de hijos (huevos)

- Esquina superior izquierda: 4 siluetas de huevo apagadas.
- Al rescatar cada uno, la silueta se ilumina con su color y emite un latido animado.

### 8.2. Indicador de Dash

- Sin barra en pantalla. Las plumas del lomo de Alma brillan tenuemente cuando el Dash está disponible, y se apagan al consumirlo. Feedback diegético.

### 8.3. Checkpoints

- Nidos abandonados con brasas. Al pasar por encima, las brasas se encienden con una llama dorada y guardan la posición.
- Sin texto de "Checkpoint guardado". El feedback es visual.

### 8.4. Juice (feedback adicional)

- **Screen shake** ligero en Pisotón Sísmico y Rugido.
- **Hit stop** de 0.05 s al golpear jefes.
- **Partículas:** polvo al aterrizar, chispas de cristal, hojas al correr, humo de lava.
- **Vignette** sutil en momentos de tensión narrativa (no hay barra de vida).

---

## 9. ESPECIFICACIÓN TÉCNICA

### 9.1. Motor y versión

- **Unity 6000.6.0f1** con URP 2D Renderer, **o**
- **Godot 4.x** con Light2D.

Ambos tienen herramientas nativas para plataformas 2D, tilemaps, luces 2D y animación por sprites.

### 9.2. Arquitectura de código

- **PlayerController** como FSM (ver sección 3.5).
- **Sistemas desacoplados:** input, movimiento, habilidades y animación.
- **ScriptableObjects (Unity) o Resources (Godot)** para datos de niveles, habilidades y enemigos.
- **Event bus** simple para comunicación entre sistemas (ej. "huevo rescatado" → UI + animación + narrativa).

### 9.3. Guardado

- **Autoguardado** al pasar un checkpoint.
- **Datos guardados:** nivel actual, checkpoint, huevos rescatados, habilidades desbloqueadas, tiempo jugado.
- **Formato:** JSON local (sin servidor, sin cuenta).

### 9.4. Controles

- **Teclado:** WASD o flechas + Espacio (salto) + Shift (dash) + Abajo (pisotón) + E (rugido).
- **Mando:** Stick izquierdo + A (salto) + B (dash) + Abajo (pisotón) + X (rugido).
- **Remapeable** desde el menú de opciones.

### 9.5. Optimización

- **Object pooling** para partículas, enemigos y proyectiles.
- **Tilemaps** en lugar de sprites individuales para el terreno.
- **Culling** de enemigos fuera de pantalla.
- Objetivo: **60 FPS estables** en hardware modesto (integrada moderna).

---

## 10. PLAN DE PRODUCCIÓN

Plazo objetivo: **12-16 semanas** a tiempo parcial, o **8-10 semanas** a tiempo completo.

### Fase 1: Prototipo en cajas grises (Semanas 1-3)

- Implementar `PlayerController` con las 4 habilidades usando bloques geométricos.
- Ajustar Coyote Time, Jump Buffer y curvas de gravedad hasta que el control se sienta perfecto.
- **Criterio de salida:** Mover a Alma por un nivel de prueba debe ser divertido por sí solo, sin arte ni narrativa.

### Fase 2: Vertical Slice — Mundo 1 completo (Semanas 4-6)

- Integrar arte final de la Jungla, sprite de Alma y Jefe 1.
- Implementar UI, sistema de checkpoints, guardado y texto del primer rescate.
- **Criterio de salida:** El Mundo 1 debe parecer y sentirse como el juego final. Es la carta de presentación.

### Fase 3: Producción en cadena — Mundos 2, 3 y 4 (Semanas 7-12)

- Crear niveles 5-16 usando tilemaps de cueva, pantano y volcán.
- Programar patrones de jefes 2, 3 y 4.
- Integrar narrativa de rescates 2, 3 y 4.
- **Criterio de salida:** Los 16 niveles son jugables de principio a fin.

### Fase 4: Pulido visual y cierre (Semanas 13-16)

- Efectos de partículas y juice.
- Feedback visual final para habilidades, peligros y rescates.
- Menús, opciones, remapeo de controles.
- Testing con jugadores externos.
- Build final para PC.
- **Criterio de salida:** Build estable, sin bugs bloqueantes, lista para publicar.

---

## 11. RIESGOS Y MITIGACIONES

| Riesgo | Mitigación |
|---|---|
| El control no se siente bien | Fase 1 dedicada exclusivamente a ajustarlo. No avanzar sin que sea divertido. |
| Sobrealcance en arte | Usar asset packs o pixel art de resolución baja (16x16 o 32x32). No aspirar a HD-2D. |
| Los jefes no son divertidos | Diseñarlos como puzles de 3 fases cortas. Probar cada uno en aislamiento. |
| Burnout | Plazos realistas. Cortar contenido antes que extender fechas. |
| Bugs de FSM | Tests unitarios del PlayerController. Logs claros de transiciones de estado. |

---

## 12. FUTURO (POST-LANZAMIENTO)

Solo si el juego base funciona y hay demanda:
- Modo contrarreloj.
- Nuevo Game+ con habilidades desde el inicio.
- Niveles de desafío opcionales (estilo *Celeste* Capítulo B).
- Port a consolas.

**Nada de esto entra en la versión 1.0.**

---

**Fin del documento.**

---

### Cambios principales respecto al original

1. **Numeración corregida** — El original tenía listas rotas (1, 3, 5, 7 sin 2, 4, 6).
2. **Secciones unificadas** — Los dos documentos separados se fusionaron en uno.
3. **Añadido lo que faltaba** — Guardado, controles, optimización, riesgos, criterios de salida por fase, presupuesto realista.
4. **Tablas de niveles** — Convertí las descripciones en prosa en tablas comparables.
5. **Pilares de diseño explícitos** — El original no los tenía; ahora hay 3 que guían las decisiones.
6. **Riesgos y mitigaciones** — Sección nueva, esencial para un proyecto indie.
7. **Futuro acotado** — Dejar claro qué NO entra en la 1.0 evita el sobrealcance.
8. **Criterios de salida por fase** — Cada fase tiene una condición clara para avanzar a la siguiente.
9. **Duración estimada** — Añadida (4-6 h), el original no la especificaba.
10. **Referencias concretas** — Añadidas para que el arte y el diseño tengan norte.


