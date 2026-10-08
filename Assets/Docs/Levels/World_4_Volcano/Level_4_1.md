> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_04/Level_4_1.unity` como escena de trabajo (copia de la de práctica) con su fondo [`Parallax_Level_4_1`](../../LevelPieces/Backgrounds.md) (dos capas: el volcán lejano bajo nubes de tormenta y acantilados de basalto con cascadas de lava); el nivel completo aún no está construido. Las notas sobre escenas, pruebas o assets existentes describen el prototipo retirado. Las referencias de cámara a **8** son el criterio de la reconstrucción, no resultados de aquellas pruebas; sus valores de seguimiento están en [Alma](../../Player/Alma.md#cámara-daño-y-feedback). No habrá audio.

# 🗺️ Nivel 4-1: "Los Ríos de Ceniza"
> **Mundo 4: Cima Volcánica** | **Función Pedagógica:** Introducir (Despertar del Rugido de Choque & Rocas Ígneas)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Siguiendo la columna de humo negro y fuego, Alma asciende por las faldas de la Cima Volcánica. El aire abrasa la garganta y una densa lluvia de ceniza gris cubre el suelo de basalto. Frente a ella, ríos de lava líquida cortan el paso y enormes rocas ígneas pesadas bloquean las gargantas de piedra. No hay forma física de empujarlas con el cuerpo sin arder.
- **Estado Emocional de Alma:** Furia contenida y amor materno llevado al punto de ebullición. El dolor del calor no es nada comparado con el dolor de no tener a sus cuatro crías a salvo. De lo más hondo de su pecho nace un rugido ancestral.
- **El Despertar de la Habilidad (El Fuego Primordial):**
  - Alma interactúa con una fumarola volcánica sagrada:
  - *Texto en Pantalla:*
    > *"¡HABILIDAD DESPERTADA: RUGIDO DE CHOQUE!*  
    > *La voz de la madre dinosaurio sacude la roca y el fuego.*  
    > *Pulsa ROAR para desatar una onda sonora cónica que empuja rocas gigantes, activa campanas lejanas y repele amenazas."*
- **Pistas Narrativas en el Entorno:** Huellas de garras gigantescas quemadas en la piedra, fragmentos de roca fundida y el eco lejano de un rugido tiránico desde el cráter superior.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un páramo volcánico abrasador. Piedra de basalto negro con grietas que desprenden un resplandor naranja y ríos de magma amarillo incandescente. Lluvia constante de partículas de ceniza y ascuas ardientes en el aire.
- **Paleta de Color Principal:**
  - Magma y Lava: Amarillo fuego (`#FFD166`), naranja brillante (`#F77F00`) y rojo carmesí (`#D62828`).
  - Basalto y Roca: Negro carbón (`#1B1B1E`) y gris ceniza (`#3A3A3A`).
  - Onda del Rugido: Anillos concéntricos de distorsión sónica con tinte doradonaranja (`#FFB703`).
- **Fondos Parallax:** implementados con **2 capas**, como en los mundos 1 a 3: la lejana corresponde a la capa 0 y la media a la capa 1. La capa 2 la forman las piezas jugables y no se usa capa de primer plano. Ficha original:
  - *Capa 0 (Fondo Lejano):* El cráter gigantesco del volcán activo bajo un cielo cubierto de nubes negras de tormenta ígnea.
  - *Capa 1 (Fondo Medio):* Cascadas de lava fluida cayendo de cornisas de basalto lejanas.
  - *Capa 2 (Fondo Cercano / Gameplay):* Canales de lava, puentes de roca natural, rocas ígneas empujables.
  - *Capa 3 (Primer Plano / Foreground):* Chispas y brasas ardientes que flotan cerca de la cámara.
- **Iluminación 2D (URP):**
  - Iluminación ambiental cálida e intensa proveniente del magma inferior (luces 2D en tono naranja brillante a nivel del suelo).

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo + **Rugido de Choque (`RoarAction`)** desbloqueado en el altar del nivel.
- **Catálogo de Bloques y Mecánicas:**
  - *Rocas Ígneas Empujables (`PushableBoulder2D`):*
    - Bloques esféricos pesados de basalto. Alma no puede empujarlas caminando contra ellas.
    - Al emitir un **Rugido de Choque** frente a una roca a menos de 3 metros, la onda sónica transmite una fuerza de impacto que desplaza la roca 4-6 metros en la dirección de la onda.
    - *Mecánica de Puente:* Empujar una roca hacia un río de lava hace que caiga en el magma, solidificando un punto de apoyo seguro para cruzar.
  - *Ríos de Lava Líquida:* Peligro mortal de un solo impacto.
- **Peligros:**
  - Chorros de vapor ardiente y ríos de magma.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Tileset:** Basalto volcánico agrietado con vetas emisoras de lava, ríos de magma líquido animado.
- [ ] **Props:** Rocas esféricas de basalto empujables, fumarolas volcánicas humeantes.
- [ ] **Sprites Alma:** Animación de rugido con apertura mandibular, pecho hinchado y emisión de ondas sónicas.
- [ ] **VFX:** Ondas de choque sónicas translúcidas en arco; lluvia de partículas de ceniza y ascuas flotantes.


## 6. Implementación jugable

- Escena `Assets/Scenes/World_4_Volcano/Level_4_1.unity`, registrada después de `Boss_3`. La salida del Pterodáctilo ya lleva al nivel.
- Alma entra con Doble Salto, Pisotón y Dash. El Rugido se desbloquea al tocar la fumarola de X=6; se guarda y permanece después de morir. Teclas **E/F**, botón de mando **Y/△** o botón móvil **ROAR**.
- La onda es un cono frontal de **3m** y **45° de semiancho**, dura **0.25s** y muestra un arco dorado con sacudida visual. No afecta objetivos detrás de Alma ni fuera del alcance.
- Las tres rocas son cinemáticas: caminar, Dash y Pisotón no las desplazan. Las gargantas de basalto impiden trepar sobre ellas para omitir el Rugido.
- Rugir frente a la roca la desplaza **5m en 0.8s**. Al caer en lava forma una superficie plana de **4.2m** de ancho, con suelo a **Y=0.2**, indicada por el cambio de color.

### Recorrido

1. **Introducción:** isla inicial X=-6–16, pared posterior X=-5.5, fumarola X=6 y checkpoint X=10. Acercarse a X=11.5 y rugir hacia la roca de X=14. Cae en X=19, dentro del río X=16–30 (14m). Saltar al apoyo, colocarse en X=20.5 y cruzar con Salto → Doble Salto → Dash. Sin el apoyo, la combinación no alcanza la otra orilla.
2. **Práctica:** isla X=30–48 y checkpoint X=34. Roca X=46 → apoyo X=51 sobre río X=48–64 (16m). Rugir desde X=43.5, saltar al apoyo, avanzar a X=52.5 y encadenar Doble Salto y Dash hacia la orilla.
3. **Vapor ardiente:** isla X=64–90, checkpoint X=68 y dos chorros en X=74 y X=82. Cada uno permanece seguro 2.2s, avisa durante 0.8s y erupciona 1.2s. Las etiquetas **PASA / ¡VAPOR! / ¡ESPERA!** y el cambio de color permiten elegir el momento de cruzar. No hay inmunidad al calor durante Dash.
4. **Aplicación final:** roca X=88 → apoyo X=93 sobre río X=90–106 (16m). Rugir desde X=85.5 y repetir el cruce desde X=94.5. Isla final X=106–124, checkpoint X=110 y portal X=120 hacia `Level_4_2`.

### Reintentos y estado

- La lava y el vapor activo causan daño por contacto; reaparece Alma en el último checkpoint.
- Morir restaura las rocas pendientes o movidas del tramo actual. Los puentes completados detrás del checkpoint se conservan, y los chorros vuelven a un período seguro completo.
- El nivel no rescata todavía el Huevo Rojo ni completa Mundo 4; corresponden al 4-4 y al jefe final. Su portal carga `Level_4_2`, Las Campanas de Basalto.
- Cámara **size 8**, seguimiento de Alma y anticipación horizontal **4.3 m**. Cuatro capas de parallax volcánico con basalto, lava lejana, ceniza y brasas; luz cálida naranja.
- Constructor: `Tools → Alma → Construir Nivel 4-1 - Los Ríos de Ceniza`. Acceso: `Alma → 📂 Cargar Nivel 4-1`.
- Reutiliza `PushableBoulder2D`, `LavaGeyser2D`, el altar de habilidad y el portal. Datos de roca y vapor en `BoulderRoarConfig.asset` y `GeyserConfig.asset`; el Rugido se ajusta en `AlmaPhysicsConfig.asset`.

### Validación

Pruebas EditMode cubren el cono del Rugido, movimiento y reinicio de rocas, ciclo del vapor y baseline de entrada. **10 pruebas PlayMode** validan entrada segura, bloqueo de las rocas ante caminar/Dash/saltos, alcance y dirección del Rugido, apoyo seguro sobre lava, imposibilidad de cruzar sin puente, reinicio por checkpoint y persistencia del Rugido, daño real de vapor/lava y recorrido completo con entradas reales sin muertes.

Arte y partículas finales pendientes.
