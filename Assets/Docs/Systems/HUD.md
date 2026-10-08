# HUD

**Estado (9 de octubre de 2026):** implementados el **indicador de hijos** (GDD 8.1) y los **botones de acción táctiles** con las runas de las habilidades. El prefab `HUD` está colocado en las **20 escenas jugables** (16 niveles y 4 jefes). Se construye paso a paso; los siguientes elementos están en [Pendiente](#pendiente).

Fichas relacionadas: [GDD 8](../GDD.md#8-interfaz-hud-y-feedback), [Guardado y progreso](SaveAndProgress.md), [Progresión](../LevelPieces/Progression.md) (huevos).

## Reglas

- **Solo en niveles y jefes.** El prefab está en cada escena jugable; los menús y el epílogo no lo tienen.
- **Se oculta en las cinemáticas** con un fundido de 0,35 s, mientras `CinematicState.IsPlaying` sea cierto. Una cinemática llama a `CinematicState.Begin()` al empezar y a `End()` al terminar (se pueden anidar). Al cargar una escena el estado se reinicia.
- **Minimalista y diegético:** sin barra de vida, sin contadores de muertes ni de tiempo y sin mapa. Nada necesario para jugar depende solo del texto o del color.

## Archivos

Todo está en `Assets/Systems/HUD/`:

| Archivo | Responsabilidad |
| --- | --- |
| `HUD.prefab` | Raíz del HUD (`Hud2D` + `HudEggIndicator` + `HudAbilityButtons`). Uno por escena jugable. |
| `Hud2D.cs` | Raíz: sigue la vista de la cámara **sin el temblor** (`AlmaCameraFollow.ViewPosition`), escala con su tamaño (diseñado para tamaño 8), coloca los elementos contra las esquinas sea cual sea la proporción de pantalla, dibuja por encima de todo (`Sorting Order` 1000) y se desvanece durante las cinemáticas (`Visibility`). |
| `CinematicState.cs` | Estado global «hay una cinemática en curso» (`Begin`, `End`, `IsPlaying`, evento `Changed`). |
| `HudEggIndicator.cs` | Indicador de hijos. |
| `HudAbilityButtons.cs` | Botones de acción (táctiles) y registro de habilidades. |
| `Sprites/HUD_Button_*.png`, `HUD_Shockwave.png` | Arte de los botones (generado por código, estilo cartoon: hueco de piedra con contorno oscuro, aro de color, flecha de salto) y onda de choque. |

El HUD se dibuja justo delante del plano cercano de la cámara: nada del nivel lo tapa, ni sprites ni mallas 3D (los bloques de práctica son cubos).

El proyecto no usa el paquete de UI de Unity: el HUD son sprites que acompañan a la cámara. Así el huevo puede volar del mundo al HUD sin conversiones de coordenadas.

## Indicador de hijos (`HudEggIndicator`)

Esquina superior izquierda: los cuatro huevos en orden (verde, azul, morado y rojo), con los sprites `Resource_RescueEgg_<Color>` del juego.

- **Huevo sin rescatar:** pequeño (75 %), apagado (gris claro al 50 %).
- **Huevo rescatado:** a tamaño completo, a todo color, con un brillo de su color que respira despacio.
- **Contraste:** detrás de cada hueco hay un halo oscuro suave (40 %), para que se lea sobre fondos claros y oscuros.
- **Al rescatar un huevo** (`RescueEgg2D` llama a `FlyIn`):
  1. El huevo del nivel estalla en chispas y, desde donde estaba, **vuela en arco hasta su hueco**: sube primero, crece un poco a mitad de camino, se balancea y deja una estela de chispas de su color. Dura 1,1 s, y el destino sigue a la cámara aunque se mueva.
  2. Al llegar, el hueco **aparece con un rebote** (sube a ~1,45× y se asienta en 0,45 s), un **destello blanco** que se abre y se apaga en 0,5 s, un **anillo de chispas** y **dos latidos** de corazón.
  - El rescate se guarda en el momento de tocar el huevo, no al terminar el vuelo. Sin HUD en la escena, el huevo vuela hacia Alma como antes.
- **Huevos ya rescatados en la partida:** salen encendidos desde el principio.

| Campo | Valor | Uso |
| --- | --- | --- |
| `Eggs` | Green, Blue, Purple, Red, con su sprite y color | Orden y aspecto de los huecos. |
| `Inset` | (0,95; 0,85) | Distancia del primer huevo a la esquina (unidades con cámara de tamaño 8). |
| `Spacing` | 0,95 | Separación entre huevos. |
| `Lit Height` | 0,9 | Alto de un huevo rescatado. |
| `Dim Scale` / `Dim Color` | 0,75 / (0,7; 0,7; 0,78; 0,5) | Aspecto de un huevo que falta. |
| `Backing Alpha` | 0,4 | Opacidad del halo oscuro. |
| `Flight Time` | 1,1 s | Duración del vuelo. |

**Comprobado** con una prueba PlayMode en `Level_1_1` (`HudEggTests`, en la copia de trabajo de pruebas): Alma coge un huevo verde, se captura la animación en varios momentos y el rescate queda guardado.

## Botones de acción (`HudAbilityButtons`)

El juego sale primero en **móvil**, así que son botones **táctiles** de verdad. También son el registro de los poderes de Alma. Van abajo a la derecha en rombo, como los botones de un mando:

| Botón | Posición | Radio | Runa / color | Comportamiento |
| --- | --- | --- | --- | --- |
| **Saltar** | abajo | 0,66 | Doble Salto, verde | Siempre funciona (flecha blanca). Al conseguir el Doble Salto se graba su runa encima. Mantenerlo pulsado = salto alto. |
| **Dash** | izquierda | 0,56 | Dash, morado | Se oscurece mientras el Dash aéreo está gastado o recargando. |
| **Pisotón** | derecha | 0,56 | Pisotón, azul | — |
| **Rugido** | arriba | 0,56 | Rugido, ámbar | — |

- **Habilidad bloqueada:** hueco de piedra vacío y apagado (60 %). Tocarlo no hace nada.
- **Habilidad conseguida:** su runa (el mismo sprite que levita sobre el altar) con un aro y un brillo de su color.
- **Al usarla,** con cualquier entrada (táctil, teclado o mando), el botón se hunde y destella. Mientras se mantiene pulsado queda hundido.
- **Táctil:** multitáctil (cada dedo, su botón) con 0,2 u de margen extra alrededor. Sin pantalla táctil, el clic del ratón hace de dedo, para probar en el editor. Durante una cinemática no responden.
- **Integración:** los botones escriben en `AlmaTouchControls` (módulo de Alma) y `AlmaInput` lo suma al teclado. `AlmaTouchControls.Move` queda preparado para el control de movimiento táctil, que se hará más adelante.

**Al coger un altar** (`AbilityAltar2D` llama a `FlyIn`):
1. **La runa se suelta del altar** y el juego se congela 0,12 s (*hit stop*). La runa sube, crece y gira como una moneda (0,3 s).
2. **Vuela en arco** hasta su botón con una estela de su color (0,7 s).
3. **Se graba en el botón:** el botón aparece con un rebote, un **destello blanco**, una **onda de choque** de su color, un anillo de chispas y **dos latidos**.

La habilidad se desbloquea y se guarda al tocar el altar; sin HUD, la runa vuela a Alma como antes.

| Campo | Valor | Uso |
| --- | --- | --- |
| `Inset` | (2,15; 2,35) | Centro del rombo, medido desde la esquina inferior derecha (cámara de tamaño 8). Los botones laterales y el de arriba están a 1,1–1,15 u del centro; el rombo entero ocupa ~3,5 u (≈22 % del alto de la pantalla). |
| `Touch Margin` | 0,2 | Área táctil extra. |
| `Locked Alpha` | 0,6 | Opacidad de un hueco bloqueado. |
| `Flight Time` / `Hit Stop` | 1,0 / 0,12 s | Vuelo de la runa y congelación al soltarse. |

**Comprobado** con una prueba PlayMode en `Level_1_1` (`HudAbilityTests`): tocar Saltar hace saltar a Alma; tocar Dash bloqueado no hace nada; Alma coge el altar del Dash, la runa vuela y se graba (capturas en varios momentos); después, tocar Dash en el aire hace el Dash.

## Pendiente

Por orden acordado:
1. **Capa de banners:** un solo estilo para los textos en pantalla (altares, rescates, jefes, prólogo). Ahora mismo cada pieza pinta el suyo; por ejemplo, la frase del rescate del huevo.
2. **Nombre del nivel** al entrar.
3. **Viñeta de tensión** (GDD 8.4).
4. ~~Iconos de habilidades~~ → hechos como botones de acción táctiles.
5. **Control de movimiento táctil** (izquierda/derecha); `AlmaTouchControls.Move` ya está preparado.
6. **Marcas de progreso del jefe** (3 impactos).
7. Ocultar el HUD con el menú de pausa cuando exista.
