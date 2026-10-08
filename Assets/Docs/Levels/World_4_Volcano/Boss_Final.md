> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_04/Boss_Final.unity` como escena de trabajo con el fondo de la arena ([`Backdrop_Boss_Final`](../../LevelPieces/Backgrounds.md#jefe-final--world_4backdrop_boss_finalprefab): el Ojo del Volcán, que despierta hasta la erupción con las fases y muestra el cráter con el amanecer al trepar); la arena y el jefe aún no están construidos. Las referencias de cámara a **8** son el criterio de la reconstrucción. No habrá audio.

# 👑 Jefe Final & Epílogo: "El Rey Ladrón — Tirano Ancestral"
> **Mundo 4: Cima Volcánica** | **El Ojo del Volcán (El Caldero de Magma) & Final del Juego**

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En el corazón del cráter volcánico, sobre un altar de basalto flotando sobre un lago de magma burbujeante, aguarda el causante de toda la tragedia: el **Rey Ladrón**, un colosal T-Rex Anciano con placas tectónicas de obsidiana fundidas a su lomo y ojos carmesí como carbón encendido. Tras el rescate del **Huevo Rojo en 4-4**, Alma deja los cuatro huevos (Verde, Azul, Morado y Rojo) en un nido sobre un saliente elevado y protegido de la arena. Los cuatro brillan como faros de aliento mientras su madre abre la salida.
- **Estado Emocional de Alma:** Determinación suprema, furia maternal implacable y devoción absoluta. Toda la travesía, cada golpe, caída y aprendizaje convergen en este instante. No hay duda ni miedo: Alma regresará a casa con todos sus hijos.
- **Textos en Pantalla / Banners & Diálogos:**
  - *Inicio del Enfrentamiento:*
    > *"¡BATALLA FINAL: EL REY LADRÓN — TIRANO ANCESTRAL!*  
    > *Has cruzado la jungla, las cavernas y los pantanos. Has despertado tus cuatro poderes ancestrales.*  
    > *¡Demuéstrale al rey de las bestias lo que es el verdadero Rugido de una Madre!"*
  - *Transición a Fase 3:*
    > *"¡El volcán entra en erupción! ¡El caldero colapsa! ¡Llega hasta el techo de la caverna!"*
  - *Golpe Final:*
    > *"¡RUGE, ALMA! ¡HAZ TEMBLAR LA TIERRA!"*
  - *Cinemática de Epílogo (Texto de Cierre):*
    > *"El titán ruge por última vez mientras la estalactita de basalto lo hunde en las profundidades del abismo ardiente.*  
    > *Alma no mira hacia atrás. Con zancadas apresuradas corre hacia el refugio y vuelve a reunir a sus cuatro hijos.*
    > *Reuniendo a sus cuatro tesoros, desciende de la montaña humeante antes del gran colapso.*  
    > *Al atardecer, en un verde y sereno valle alejado del fuego, la madre contempla sus cuatro joyas.*  
    > *De pronto... un tenue chasquido rompe el silencio:*  
    > *¡Crack! ¡Crack! ¡Crack! ¡Crack!*  
    > *Cuatro pequeños pares de ojos curiosos se abren hacia el cielo, buscando la mirada de su protectora.*  
    > *'No hubo tormenta, abismo ni bestia que pudiera apagar este latido. Bienvenidos al mundo, pequeños.'*  
    > **— FIN —**"*

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **La Arena de Combate (El Ojo del Magma):**
  - Plataformas hexagonales de basalto negro que flotan sobre un mar de lava líquida hirviente.
  - El coloso T-Rex se mueve en el fondo y se apoya en el borde de la arena con sus garras gigantescas.
  - A medida que avanza la pelea, el magma sube de nivel y las plataformas del suelo se hunden, forzando a Alma a ascender verticalmente por las columnas de la caverna.
- **Paleta de Color Principal:**
  - Tirano: Escamas gris ceniza (`#2B2B2B`), coraza de obsidiana con venas de magma brillante anaranjado (`#FF4500`) y colmillos amarillentos manchados de hollín.
  - Lava y Magma: Gradiente radiante desde amarillo incandescente (`#FFD166`) hasta rojo magma (`#D62828`).
  - Huevo 4: Resplandor carmesí rubí pulsante (`#EF233C`).
  - Epílogo: Verde esmeralda suave, cielo azul dorado de atardecer (`#F4A261`), flores silvestres y luz cálida acogedora.

---

## 3. 🧱 Mecánicas de Combate & Fases del Jefe (Puzle de las 4 Habilidades)

Cada fase aporta un impacto y queda guardada como checkpoint. Al reintentar, el magma, las plataformas y los mecanismos vuelven al estado seguro de entrada de esa fase; los cuatro huevos siguen rescatados.

El combate final es un examen maestro donde Alma debe sincronizar las **4 habilidades aprendidas**:
- **Fase 1: La Carga del Coloso (Doble Salto + Dash Aéreo)**
  - El T-Rex barre horizontalmente la arena con su mandíbula y lanza zarpazos ardientes.
  - Alma debe ejecutar **Doble Salto** para esquivar el barrido bajo y rematar con un **Dash Aéreo** para atravesar las ondas de calor sin quemarse.
  - Tras fallar su embestida, la cabeza del jefe queda atascada contra un pilar durante 4 segundos. Alma salta sobre su lomo y ejecuta un **Pisotón Sísmico** sobre la placa dorsal agrietada. (Impacto 1/3).
- **Fase 2: La Lluvia de Meteoros (Rugido de Onda Expansiva)**
  - El jefe ruge enfurecido haciendo llover peñascos incandescentes desde la bóveda de la cueva.
  - Un meteorito gigante rueda directo hacia Alma. Alma debe plantarse frente a él y activar el **Rugido de Onda Expansiva** en el momento exacto: la onda de choque desvía el meteorito a contra-trayectoria directo a las fauces abiertas del T-Rex.
  - El impacto aturde al coloso, dejándolo vulnerable para un segundo **Pisotón Sísmico** en el pecho. (Impacto 2/3).
- **Fase 3: El Gran Colapso (Síntesis Total de las 4 Habilidades)**
  - El T-Rex entra en frenesí volcánico: el magma sube rápidamente destruyendo el suelo.
  - Alma debe iniciar una carrera vertical hacia la cima de la cueva:
    1. Impulsarse con **Doble Salto** sobre plataformas de basalto en caída libre.
    2. Usar **Dash Aéreo** para cruzar cortinas de fuego descendentes.
    3. Romper un bloque de obsidiana con **Pisotón Sísmico** para activar un geiser de vapor ascendente.
    4. Llegar a la base de una colosal estalactita suspendida en el techo sobre el tirano.
    5. Utilizar el **Rugido de Onda Expansiva** para agrietar el anclaje de la estalactita y asestar un **Pisotón Sísmico en caída libre** para desprenderla directamente sobre el jefe. (Impacto Definitivo 3/3).

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Jefe Final:** Tirano Ancestral T-Rex (animaciones de acecho en fondo, mordisco frontal, barrido de cola, rugido sísmico, aturdimiento y caída al magma).
- [ ] **Sprites Entorno:** Plataformas de basalto flotante, columnas colapsables, estalactita colosal rompible, nido con Huevo Rojo.
- [ ] **Sprites Epílogo:** Cuatro crías de dinosaurio bebé (Verde, Azul, Púrpura, Rojo) animadas saliendo de sus respectivos cascarones; Alma en pose pacífica recostada en la hierba.
- [ ] **VFX:** Chispas volcánicas, lava ascendente procedural, distorsión de calor en pantalla, onda de choque sónica para el rugido devuelto, pantalla en blanco suave para la transición al epílogo.

## 6. Implementación jugable

En la reconstrucción, `Boss_Final` continuará el portal del 4-4 y usará cámara ortográfica de tamaño 8, seguimiento vertical y anticipación horizontal de 4,3 m. La escena `Assets/Scenes/World_4_Volcano/Boss_Final.unity` y el menú **Alma → 📂 Cargar Jefe Final** pertenecían al prototipo retirado. El encuentro se comunicará visualmente.

- **Carga:** mandíbula baja precedida por aviso. Doble salto y Dash aéreo contra el sello de calor X=8 exponen la placa dorsal X=12 durante 4 segundos. Solo un Pisotón desde arriba confirma el impacto; caminar, aterrizar normalmente o rugir no dañan la coraza.
- **Meteoros:** un peñasco se acerca desde la derecha. El Rugido frontal lo devuelve a las fauces y abre otra ventana de 4 segundos para el Pisotón. La lluvia secundaria marca primero su punto de caída en amarillo y no persigue después a Alma.
- **Colapso:** checkpoint X=20. El altar desaparece; tras 7 segundos, el magma sube a 0.38m/s. Cornisas a 2.2m y 4.4m avisan durante 1.5s antes de colapsar. Un salto con Dash cruza el paso bajo la cortina de fuego. Pisotón en el sello X=38 libera el vapor; el impulso y la cornisa superior permiten alcanzar el anclaje. Rugido frontal para agrietarlo, salto y Pisotón para soltar la estalactita. El tercer impacto guarda Mundo 4 completado.
- **Reintentos:** cada impacto conserva la fase durante el encuentro. Morir reinicia la ventana, las posiciones de meteoritos, las cornisas, el sello, el anclaje y el magma a la entrada segura de esa fase. Los huevos guardados no se modifican; probar el jefe directamente no concede rescates.
- **Final:** caída visual de la estalactita y del tirano, transición al valle de atardecer, nacimiento secuencial de cuatro crías y mensaje de cierre. El arte del tirano, las crías y el escenario es provisional.

### Validación

- 7 pruebas PlayMode del jefe final aprobadas: recorrido completo con controles reales y sin muertes, daño de la mandíbula, requisitos de habilidades, ventanas, checkpoints, ausencia de audio y portal del 4-4.
- 13 pruebas PlayMode del Nivel 4-4 aprobadas como regresión.
- 83 pruebas EditMode aprobadas, incluidas las reglas de exposición, impactos, vapor, anclaje y reinicio de fase.
- Revisión visual histórica de la arena, el ascenso y el epílogo; repetirla con la cámara de tamaño 8 al reconstruir la escena.
