# HUD

**Estado (9 de octubre de 2026):** implementado el **indicador de hijos** (GDD 8.1), primer elemento del HUD. El prefab `HUD` está colocado en las **20 escenas jugables** (16 niveles y 4 jefes). Se construye paso a paso; los siguientes elementos están en [Pendiente](#pendiente).

Fichas relacionadas: [GDD 8](../GDD.md#8-interfaz-hud-y-feedback), [Guardado y progreso](SaveAndProgress.md), [Progresión](../LevelPieces/Progression.md) (huevos).

## Reglas

- **Solo en niveles y jefes.** El prefab está en cada escena jugable; los menús y el epílogo no lo tienen.
- **Se oculta en las cinemáticas** con un fundido de 0,35 s, mientras `CinematicState.IsPlaying` sea cierto. Una cinemática llama a `CinematicState.Begin()` al empezar y a `End()` al terminar (se pueden anidar). Al cargar una escena el estado se reinicia.
- **Minimalista y diegético:** sin barra de vida, sin contadores de muertes ni de tiempo y sin mapa. Nada necesario para jugar depende solo del texto o del color.

## Archivos

Todo está en `Assets/Systems/HUD/`:

| Archivo | Responsabilidad |
| --- | --- |
| `HUD.prefab` | Raíz del HUD (`Hud2D` + `HudEggIndicator`). Uno por escena jugable. |
| `Hud2D.cs` | Raíz: sigue la vista de la cámara **sin el temblor** (`AlmaCameraFollow.ViewPosition`), escala con su tamaño (diseñado para tamaño 8), coloca los elementos contra las esquinas sea cual sea la proporción de pantalla, dibuja por encima de todo (`Sorting Order` 1000) y se desvanece durante las cinemáticas (`Visibility`). |
| `CinematicState.cs` | Estado global «hay una cinemática en curso» (`Begin`, `End`, `IsPlaying`, evento `Changed`). |
| `HudEggIndicator.cs` | Indicador de hijos. |

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

## Pendiente

Por orden acordado:
1. **Capa de banners:** un solo estilo para los textos en pantalla (altares, rescates, jefes, prólogo). Ahora mismo cada pieza pinta el suyo; por ejemplo, la frase del rescate del huevo.
2. **Nombre del nivel** al entrar.
3. **Viñeta de tensión** (GDD 8.4).
4. **Iconos de habilidades** (opcional).
5. **Marcas de progreso del jefe** (3 impactos).
6. Ocultar el HUD con el menú de pausa cuando exista.
