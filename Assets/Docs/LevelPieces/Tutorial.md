# Tutoriales y momentos de historia

**Estado (10 de octubre de 2026):** implementados `Resource_TutorialPrompt_Universal` y `Resource_StoryMoment_Universal` en `Assets/Prefabs/Level/Tutorial/`. Probados jugando en `Level_1_1` (`TutorialTests`): el tutorial pausa y, al tocar SALTAR, Alma salta; el plano viaja, muestra el texto y devuelve los controles.

## Tutorial (`TutorialPrompt2D`)

Zona invisible (verde en el editor). Cuando Alma la pisa en el estado adecuado:
1. **Cámara lenta:** el tiempo se frena hasta pararse (0,35 s).
2. **Plano:** la cámara se acerca (`Zoom`) al punto medio entre Alma y el aterrizaje.
3. **Foco:** la pantalla se oscurece salvo un círculo suave alrededor del obstáculo.
4. **Trayectoria:** se dibuja con puntos (con un cometa del color de acento que la recorre) hasta un aro que marca el aterrizaje.
5. **Botón:** el que hay que pulsar se ilumina por encima del oscurecido, con un aro que late, ondas y una flecha que lo señala.
6. **Texto:** el título entra con rebote («¡SALTA!») y debajo aparece la explicación. Los dos se ajustan al ancho de la pantalla: la explicación se parte en líneas y el título se encoge si no cabe.
7. **Al tocar ese botón,** el juego sigue al instante y **el movimiento ocurre en ese momento**. Todo se desvanece y la cámara vuelve.

Solo sale una vez por carga de escena (no al reaparecer tras morir). Si la habilidad no está desbloqueada, no sale.

| Campo | Por defecto | Uso |
| --- | --- | --- |
| `Action` | Jump | Jump (en el suelo), DoubleJump (en el aire, cerca de la cima del salto), Dash (en el aire), GroundPound (en el aire), Roar. |
| `Title` / `Text` | «¡Salta!» / «Toca el botón SALTAR para cruzar al otro lado.» | Textos. |
| `Accent` | verde | Color del título, la trayectoria y el aro. |
| `Landing` | (4,5; 0) | Dónde aterriza el movimiento, relativo a la zona. En el editor se ve como un arco blanco con un círculo al final. |
| `Arc Height` / `Show Arc` | 1,5 / sí | Altura de la trayectoria. |
| `Zoom` | 6 | Tamaño de cámara del plano (el juego usa 8; menos es más cerca). |
| `Focus Offset` | (0; 1,2) | Ajuste del encuadre. |
| `Assist Move` | sí | Tras pulsar, Alma sigue avanzando hacia el aterrizaje hasta tocar suelo. Necesario en móvil mientras no haya control táctil de movimiento. |
| `Only Once` | sí | Una vez por carga de escena. |

**Colocación:**
- Pon la zona justo **antes del borde** del hueco o desnivel; la caja es de 1,2 × 3.
- Arrastra el `Landing` al otro lado.
- Para un **doble salto**, pon la zona en el aire, donde Alma está cerca de la cima de su primer salto.

## Momento de historia (`StoryMoment2D`)

Zona invisible (naranja en el editor). Al entrar Alma:
1. La cámara **viaja** a un punto (`Shot Target`, o `Shot Offset` si no hay objetivo) con zoom.
2. Se queda unos segundos.
3. **Aparece un banner** con el texto (Frase, Título, Grito o Jefe) y la cámara vuelve.

Los controles se bloquean durante el plano, pero el juego no se pausa. Sirve para enseñar al mono, el altar, el camino que viene o un detalle de la historia. Una vez por carga de escena. Se puede usar solo el texto (`Use Shot` desactivado) o solo el plano (texto vacío).

| Campo | Por defecto | Uso |
| --- | --- | --- |
| `Shot Target` / `Shot Offset` | — / (8; 2) | Qué enseña la cámara; el rectángulo naranja del editor es el encuadre. |
| `Zoom` | 6,5 | Tamaño de cámara del plano. |
| `Travel Time` / `Hold Time` | 1,1 / 2,8 s | Viaje de ida y vuelta, y tiempo quieta. |
| `Lock Controls` | sí | Alma no se mueve durante el plano. |
| `Lines` | 3 frases del mono | Lo que dice el objetivo, en un **bocadillo de cómic** sobre su cabeza (`SpeechBubble2D`). Una frase tras otra, escritas letra a letra; las que acaban en «!» tiemblan. El plano se alarga solo para que dé tiempo a leerlas. |
| `Speaker` / `Speaker Color` | «Mono ladrón» / naranja | Nombre sobre el bocadillo. |
| `Target Exit` | LeapAway | **LeapAway:** al terminar de hablar, el objetivo huye de un salto girando fuera del plano y luego se desactiva (sin verse desaparecer). **HideAfterShot:** se desactiva cuando la cámara ya volvió a Alma. **Stay:** se queda. |
| `Exit Direction` | +1 | Hacia dónde huye (+1 derecha, −1 izquierda). |
| `Kind`, `Kicker`, `Headline`, `Text`, `Accent`, `Icon` | Frase | El banner (ver [HUD](../Systems/HUD.md#banners-de-texto-hudbanners)). |
| `Text Delay` | 0,6 s | Cuándo aparece el texto. |

## Piezas del sistema

- **`HudTutorial`** (en el prefab `HUD`): dibuja el tutorial. Usa `HUD_Spotlight` (oscuro con un círculo claro), `HUD_Button_Ring`, `HUD_Shockwave` y `HUD_Arrow`.
- **`AlmaCameraFollow.SetShot(punto, tamaño, peso)` / `ClearShot()`:** planos de cámara mezclados con el seguimiento normal, siempre dentro de los límites del nivel. El HUD sigue a la vista del plano.
- **`HudAbilityButtons.Spotlight(habilidad)` y `TryGetButton(…)`:** iluminar un botón y saber dónde está.
