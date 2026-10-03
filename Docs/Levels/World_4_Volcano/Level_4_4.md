> **Ficha de diseño para reconstrucción:** no hay escena implementada. Las notas sobre escenas, pruebas o assets existentes describen el prototipo anterior. La política vigente es jugar sin audio.

# 🗺️ Nivel 4-4: "La Antecámara del Fuego"
> **Mundo 4: Cima Volcánica** | **Función Pedagógica:** Evaluar + Cuarto Rescate (El Nido Completo & Huevo Rojo)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En el sanctasanctórum del cráter volcánico, antes de la cámara del tirano, Alma entra en una catedral de basalto rodeada de lava hirviente. El suelo tiembla con violencia precursora de una erupción inminente. En el centro de la antecámara, protegido sobre un pedestal de obsidiana rodeado por un foso de fuego, reposa el último y más esperado de sus hijos: el **Huevo Rojo**, cuya cáscara pulsa con un calor radiante y vivo.
- **Estado Emocional de Alma:** Una emoción incontenible que trasciende el miedo y el cansancio. Sus cuatro hijos están a punto de volver a estar juntos.
- **El Momento del Rescate (Cinemática Diegética en Gameplay):**
  - Alma supera la última cadena de plataformas y alcanza el pedestal de obsidiana.
  - Al apoyar su rostro contra el **Huevo Rojo**, el estruendo volcánico se apaga en un instante. La melodía de la "Nana Maternal" en piano se despliega con su máxima intensidad emocional:
  - *Texto del Rescate (Huevo Rojo):*
    > *"Los cuatro están aquí.*  
    > *Mi nido vuelve a estar completo.*  
    > *(Cuatro de cuatro rescatados)"*
  - De pronto, el suelo bajo el pedestal se fractura con un crujido sísmico colosal: emerge desde el lago de magma el líder supremo de los saqueadores, el **Rey Ladrón (T-Rex Anciano)**, con cicatrices de batalla y ojos incandescentes, dispuesto a destruir a Alma y a su prole para siempre.
- **Pistas Narrativas en el Entorno:** Huesos de bestias colosales en el magma, plumas de Alma que se unen a las de sus otros tres huevos en su espalda, y el latido al unísono de los cuatro huevos resonando en el corazón de Alma.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un templo volcánico ancestral en el corazón de la caldera. Columnas de obsidiana pura negra como el cristal que reflejan los ríos de lava incandescente que corren a ambos lados. Ceniza brillante dorada cayendo del techo.
- **Paleta de Color Principal:**
  - Obsidiana: Negro espejo (`#0B090A`) con reflejos de fuego.
  - Magma de Fondo: Rojo rubí brillante (`#BA181B`) y oro fundido (`#FFBA08`).
  - Huevo Rojo: Carmesí radiante (`#E63946`) con una corona de calor pulsante.
- **Iluminación 2D (URP):**
  - Las cuatro siluetas de los huevos en la espalda de Alma emiten sus respectivos colores: Verde, Azul, Morado y Rojo, creando un aura cromática única alrededor del personaje.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo + Rugido de Choque.
- **Objetivo de Diseño: El Examen Maestro Final**:
  1. *El Puzle de las Cuatro Puertas Elementales:*
     - Una serie de cuatro salas consecutivas que ponen a prueba cada una de las cuatro habilidades:
       - *Sala 1:* Vuelo y altura de Doble Salto.
       - *Sala 2:* Rompimiento de pilares con Pisotón Sísmico.
       - *Sala 3:* Cruce de corrientes de fuego con Dash Aéreo.
       - *Sala 4:* Puntería y sincronización sónica con Rugido de Choque para alinear los puentes de basalto.
  2. *El Pedestal de Obsidiana:*
     - Una isla solitaria en medio del foso de lava que exige un salto final perfecto combinando Doble Salto y Dash para alcanzar el Huevo Rojo.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 35.0`):* Mitad de las salas de prueba.
  - *Checkpoint 2 (`X = 75.0`):* Justo frente al pedestal del Huevo Rojo.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Pedestal de obsidiana tallada, Huevo Rojo carmesí con aura de calor, puente de basalto deslizante.
- [ ] **Sprites Alma:** Alma portando los cuatro huevos visibles acurrucados en su espalda con sus respectivos tonos de luz.
- [ ] **VFX:** Vetas de lava líquida iluminadas en URP; aura cuadrilateral de luz (verde, azul, morada, roja) emanando de los huevos.


## 6. Implementación jugable

Escena `Assets/Scenes/World_4_Volcano/Level_4_4.unity`, accesible desde `Alma/📂 Cargar Nivel 4-4`. El portal del 4-3 carga esta escena. Las cuatro habilidades están disponibles al entrar directamente; entrar no concede huevos ni completa el mundo.

1. **Alas — altura:** columnas X=7–12 con superficie Y=2.2 y X=14–17 con superficie Y=3.2. La primera exige Doble Salto real; la siguiente asciende solo 1m desde ella. Una salamandra patrulla la salida en X=20.
2. **Tierra — pilares:** Pisotón atraviesa dos suelos agrietados superpuestos en X=25–29, superficies Y=0 e Y=-1. Romper ambos abre la barrera X=30.5. El refugio inferior está en Y=-2; Doble Salto permite volver a suelo Y=0. Una salamandra debajo de los pilares recibe el impacto sísmico antes de que Alma llegue a su altura. Checkpoint X=35.
3. **Impulso — fuego:** una salamandra protege la aproximación. El foso X=44–52 contiene una corriente de fuego elevada con 2.2s seguros, 0.8s de aviso y 1.2s activos. Esperar PASA y combinar salto con Dash permite romper la reja X=50 y aterrizar en X=52–58. El fuego activo daña también durante Dash.
4. **Voz — puente:** desde X=53.5–53.8, Rugido frontal mueve el basalto X=56 cinco metros. Se solidifica en X=61 con apoyo plano de 4.2m y superficie Y=0.2 sobre lava X=58–74. Saltar al apoyo y usar Doble Salto + Dash desde X=62.5 permite llegar al checkpoint X=75. El cuerpo alto del basalto y el foso de 16m impiden cruzar caminando o ignorar la alineación.
5. **Pedestal:** último Doble Salto + Dash desde X=77.4 sobre lava X=78–86. La isla X=86–104 alberga el Huevo Rojo en X=90, Y=1.1. El portal al jefe está cerrado hasta tocar físicamente el huevo.

**Momento del rescate:** se guarda el Huevo Rojo, el combate y sus proyectiles se apagan, las corrientes de fuego quedan inactivas y el rescate ocurre en silencio. Alma pasa a tener checkpoint de rescate X=90. Las siluetas de huevos en su espalda muestran únicamente los huevos realmente recuperados, con luces verde, azul, morada y roja. En la partida normal, el texto es «Los cuatro están aquí. Mi nido vuelve a estar completo. (Cuatro de cuatro rescatados)». Entrar directamente para probar el nivel no concede los otros tres huevos.

Tras 2.5s de calma, una silueta del Rey Ladrón emerge junto al pedestal con temblor breve. Es la anticipación del combate: no inflige daño. El portal X=95 se dirige a `Boss_Final`, todavía pendiente de implementación. Rescatar el huevo no completa Mundo 4; eso corresponde a derrotar al jefe final. Al recargar, el rescate y el acceso al portal persisten sin duplicar huevos.

Morir restaura pilares y reja pendientes, mientras los mecanismos ya resueltos detrás del checkpoint y el puente completado permanecen. Los enemigos y la corriente reinician sus ciclos. El checkpoint del rescate permite recuperarse en el pedestal. Cámara size 6, seguimiento de Alma y anticipación horizontal 1.25m; muro detrás del inicio. Se retiraron las columnas decorativas y los bloques del fondo para despejar el santuario. El arte definitivo sigue pendiente.

### Validación de la implementación

- 13 pruebas PlayMode del Nivel 4-4 aprobadas, incluido el recorrido completo con controles reales y sin muertes.
- 16 pruebas PlayMode del Nivel 4-3 aprobadas como regresión, incluida la transición al 4-4.
- 79 pruebas EditMode aprobadas.
- Verificados alcance del doble salto, pisotón de ambos pilares, dash sobre corrientes, puente de rugido, checkpoints, rescate persistente y cámara de tamaño 6.
