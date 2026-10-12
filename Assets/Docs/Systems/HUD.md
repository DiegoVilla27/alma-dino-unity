# HUD

**Estado (9 de octubre de 2026):** implementados el **indicador de hijos** (GDD 8.1), los **botones de acción táctiles** con las runas de las habilidades la **capa de banners** de texto, el **nombre del nivel al entrar**, la **viñeta de tensión** y la **presentación pausada de cada habilidad**, todo con TextMeshPro y **responsive** (zona segura y escala por dispositivo). El prefab `HUD` está colocado en las **20 escenas jugables** (16 niveles y 4 jefes). Se construye paso a paso; los siguientes elementos están en [Pendiente](#pendiente).

Fichas relacionadas: [GDD 8](../GDD.md#8-interfaz-hud-y-feedback), [Guardado y progreso](SaveAndProgress.md), [Progresión](../LevelPieces/Progression.md) (huevos).

## Reglas

- **Solo en niveles y jefes.** El prefab está en cada escena jugable; los menús y el epílogo no lo tienen.
- **Se oculta en las cinemáticas** con un fundido de 0,35 s, mientras `CinematicState.IsPlaying` sea cierto. Una cinemática llama a `CinematicState.Begin()` al empezar y a `End()` al terminar (se pueden anidar). Al cargar una escena el estado se reinicia.
- **Responsive:** todo se ancla a la [zona segura](#responsive) del dispositivo y se escala con la pantalla; ningún texto ni botón puede salirse.
- **Minimalista y diegético:** sin barra de vida, sin contadores de muertes ni de tiempo y sin mapa. Nada necesario para jugar depende solo del texto o del color.

## Archivos

Todo está en `Assets/Systems/HUD/`:

| Archivo | Responsabilidad |
| --- | --- |
| `HUD.prefab` | Raíz del HUD (`Hud2D` + `HudEggIndicator` + `HudAbilityButtons` + `HudBanners` + `HudLevelTitle` + `HudTensionVignette`). Uno por escena jugable. |
| `Hud2D.cs` | Raíz: sigue la vista de la cámara **sin el temblor** (`AlmaCameraFollow.ViewPosition`), escala con su tamaño (diseñado para tamaño 8), calcula la zona segura y la escala de la interfaz ([responsive](#responsive)), dibuja por encima de todo (`Sorting Order` 1000) y se desvanece durante las cinemáticas (`Visibility`). |
| `CinematicState.cs` | Estado global «hay una cinemática en curso» (`Begin`, `End`, `IsPlaying`, evento `Changed`). |
| `HudEggIndicator.cs` | Indicador de hijos. |
| `HudAbilityButtons.cs` | Botones de acción (táctiles), registro de habilidades y presentación pausada de una habilidad nueva. |
| `HudBanners.cs` | Capa única de banners de texto. |
| `HudBossAlert.cs` | Aviso de «jefe vulnerable» (grito + insignia en el borde). Se añade solo al HUD la primera vez que un jefe lo usa. |
| `HudLevelTitle.cs` | Tabla de títulos de las 20 escenas jugables; muestra el del nivel al empezar. |
| `TensionState.cs` | Registro global de tensión: cada peligro pone su nivel (0–1) y su color; gana el más fuerte. |
| `HudTensionVignette.cs` | Viñeta de tensión (bordes que se cierran y laten). |
| `HudTutorial.cs` | Tutorial pausado: cámara lenta, plano, foco, trayectoria, botón iluminado y texto. Ver [Tutoriales](../LevelPieces/Tutorial.md). |
| `HudText.cs` | Estilo de texto común (TextMeshPro): fuentes, contorno, sombra, degradado, ajuste al ancho. |
| `Fonts/LuckiestGuy-Regular.ttf` | Fuente de titulares (cartoon, mayúsculas). Licencia Apache 2.0 (`LuckiestGuy-LICENSE.txt`). |
| `Fonts/Fredoka-SemiBold.ttf` | Fuente de texto (redondeada). Instancia fija del Fredoka variable (peso 600). Licencia SIL OFL (`Fredoka-OFL.txt`). |
| `Fonts/LuckiestGuy SDF.asset`, `Fonts/Fredoka SDF.asset` | Fuentes de TextMeshPro (campo de distancia: nítidas a cualquier tamaño). Dinámicas: el atlas se rellena solo con los caracteres que se usan. |
| `Sprites/HUD_Vignette.png`, `HUD_VignetteSmoke.png` | Viñeta suave (centro limpio, bordes opacos) y borde de humo con volutas (ruido orgánico). Generadas por código, blancas. |
| `Sprites/HUD_Banner_Veil.png`, `HUD_Divider.png`, `HUD_Rays.png` | Velo y bandas de cine (bloque blanco), separador ornamental (línea que se afina con un rombo central) y resplandor de rayos. Generados por código, blancos; se tiñen en el juego. |
| `Sprites/HUD_Button_*.png`, `HUD_Shockwave.png` | Arte de los botones (generado por código, estilo cartoon: hueco de piedra con contorno oscuro, aro de color, flecha de salto) y onda de choque. |

El HUD se dibuja justo delante del plano cercano de la cámara: nada del nivel lo tapa, ni sprites ni mallas 3D (los bloques de práctica son cubos).

El HUD no usa Canvas: son sprites y textos **TextMeshPro 3D** que acompañan a la cámara. Así huevos y runas vuelan del mundo al HUD sin conversiones de coordenadas. TextMeshPro viene del paquete **uGUI 2.6.0** (`com.unity.ugui`, añadido el 9/10/2026); sus recursos esenciales (shaders y ajustes) están en `Assets/TextMesh Pro/`.

## Responsive

El HUD está diseñado para una pantalla horizontal 16:9 de 16 unidades de alto (cámara de tamaño 8). En cualquier dispositivo:
- **Zona segura:** todo se coloca dentro de `Screen.safeArea` (notch, esquinas redondeadas, barra de gestos): huevos arriba a la izquierda, botones abajo a la derecha, banners centrados en ella. `Hud2D.Safe` da esa zona en unidades del HUD; `Hud2D.SafeAreaOverride` permite simular otra (pruebas).
- **Escala (`UiScale`):** 1 en pantallas 16:9 o más anchas; menor si la zona segura es más estrecha o más baja (mínimo 0,4).
- **Escala de texto (`TextScale`):** la de los banners y la presentación de habilidades. Parte de `UiScale` pero nunca deja las mayúsculas del texto normal por debajo de **2,4 mm** reales (`Min Text Millimetres` en `Hud2D`); tampoco crece tanto que un banner no quepa (máximo 1,8, el ancho de la zona segura entre 13 y su alto entre 9).
- **Botones:** escalan con `UiScale`, pero nunca bajan de **9 mm** reales de diámetro en dispositivos que informan de sus DPI. Tampoco ocupan más del 45 % del ancho ni del 50 % del alto de la zona segura.
- **Huevos:** usan la **misma escala que los botones**; un huevo rescatado mide lo mismo que un botón pequeño (1,12 u de diseño).
- **Textos:** los titulares de una línea se encogen para caber; los textos largos se parten en líneas según el ancho disponible.

**Comprobado** con `HudBannerTests` en cuatro pantallas, simulando además su densidad real (`Hud2D.DpiOverride`): móvil 16:9 (1920×1080 a 400 ppp), móvil 19,5:9 con notch (2532×1170 a 460 ppp, zona segura recortada a los lados), tablet 4:3 (2048×1536 a 264 ppp) y móvil en vertical 9:16 (1080×2400 a 400 ppp). Escalas resultantes: texto 1,12 / 1,19 / 0,75 / 0,5; botones y huevos 1,6 / 1,6 / 0,87 / 0,84. En todas se capturan la presentación de una habilidad, la frase de un huevo, un jefe, un grito, un título y la narración; nada se corta ni se solapa.


## Indicador de hijos (`HudEggIndicator`)

Esquina superior izquierda: los cuatro huevos en orden (verde, azul, morado y rojo), con los sprites `Resource_RescueEgg_<Color>` del juego.

- **Huevo sin rescatar:** algo más pequeño (85 %), apagado (gris claro al 50 %).
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
| `Inset` | (1; 1) | Distancia del primer huevo a la esquina superior izquierda de la **zona segura** (unidades de diseño; huevos, separación e inset escalan con la escala de los botones). |
| `Spacing` | 1,4 | Separación entre huevos. |
| `Lit Height` | 1,12 | Alto de un huevo rescatado (= diámetro de un botón pequeño). |
| `Dim Scale` / `Dim Color` | 0,85 / (0,7; 0,7; 0,78; 0,5) | Aspecto de un huevo que falta. |
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

**Al coger un altar** se pausa el juego para presentar la habilidad: ver [Habilidad nueva](#habilidad-nueva-presentación-pausada).

| Campo | Valor | Uso |
| --- | --- | --- |
| `Inset` | (2,15; 2,35) | Centro del rombo, medido desde la esquina inferior derecha de la zona segura (escala con los botones). Los botones laterales y el de arriba están a 1,1–1,15 u del centro; el rombo entero ocupa ~3,5 u (≈22 % del alto de la pantalla). |
| `Touch Margin` | 0,2 | Área táctil extra. |
| `Locked Alpha` | 0,6 | Opacidad de un hueco bloqueado. |
| `Min Button Millimetres` | 9 mm | Diámetro real mínimo de los botones pequeños. |
| `Flight Time` | 0,75 s | Vuelo de la runa del centro de la pantalla a su botón. |
| `Min Present Time` | 1,2 s | Tiempo mínimo antes de que un toque pueda continuar. |

**Comprobado** con una prueba PlayMode en `Level_1_1` (`HudAbilityTests`): tocar Saltar hace saltar a Alma; tocar Dash bloqueado no hace nada; Alma coge el altar del Dash, se presenta, se continúa y la runa se graba; después, tocar Dash en el aire hace el Dash.

## Habilidad nueva (presentación pausada)

Al tocar un altar con HUD en la escena, `AbilityAltar2D` desbloquea y guarda la habilidad y llama a `HudAbilityButtons.Present`:
1. **Pausa:** el juego se para (`Time.timeScale` = 0) y Alma ignora los controles (`AlmaTouchControls.InputLocked`). La pantalla se oscurece (velo al 80 %).
2. **La runa vuela al centro** desde el altar en arco, girando como una moneda y dejando estela (0,6 s). Al llegar: destello, chispas y un **resplandor de rayos** de su color que gira despacio detrás.
3. **Texto**, todo centrado bajo la runa:
   - «¡HABILIDAD DESPERTADA!» (línea pequeña espaciada);
   - el **nombre** en grande, con degradado de su color, contorno y sombra, entrando con un rebote;
   - el **separador ornamental**, que se abre desde el centro;
   - **una sola línea** de cómo se usa, con el **botón** donde vive (en miniatura) a su izquierda.
4. **«Toca para continuar»** late abajo pasados 1,2 s. Un toque (o clic, o cualquier tecla) continúa.
5. Los textos y el velo se apagan, **la runa vuela a su botón** y se graba (rebote, destello, onda de choque y dos latidos). Al llegar, el juego sigue.

Todo corre en tiempo real (no le afecta la pausa) y se adapta a la pantalla. Los banners que lleguen mientras tanto esperan su turno.

| Altar | Nombre | Línea |
| --- | --- | --- |
| Doble Salto | Aleteo Materno | Toca SALTO en el aire para dar un segundo salto. |
| Pisotón | Pisotón Sísmico | Toca PISOTÓN en el aire para caer con fuerza y romper suelos frágiles. |
| Dash | Dash Aéreo | Toca DASH en el aire para impulsarte y cruzar abismos. |
| Rugido | Rugido de Choque | Toca RUGIDO para lanzar una onda que empuja rocas y despierta campanas. |

Se editan en los campos `Banner Title` y `Banner Text` de cada altar. Las frases largas de las fichas quedan como texto narrativo; en el juego solo va una línea.

## Banners de texto (`HudBanners`)

Todos los textos en pantalla pasan por una sola capa con un estilo común (`HudText`): **aparecen, se leen y se van**.
- **Cola:** los banners salen de uno en uno. Uno idéntico al que se muestra o al que espera se descarta.
- **Tiempo real:** ni la pausa ni el *hit stop* los congelan.
- **Cinemáticas:** siguen visibles aunque el resto del HUD se oculte. Los que llevan **bandas de cine** (Jefe y Narración) son cinemáticas en sí: mientras se ven, ocultan huevos y botones (`CinematicState`).
- **Bandas y temblor:** el HUD no tiembla pero la cámara sí, así que en pantalla las bandas se mueven con cada temblor. Por eso cada banda (y el velo) sobresale **2 unidades** más allá del borde de la pantalla y por los lados (`BarBleed`): su borde exterior nunca deja ver la escena. El borde interior (donde va el texto) no cambia.
- **Estilo:** TextMeshPro con **Luckiest Guy** (titulares) y **Fredoka** (texto); contorno oscuro y sombra reales, nítidos a cualquier resolución; los titulares llevan degradado de su color. Sin cintas: un halo oscuro suave detrás cuando hace falta contraste. Los textos entran enteros con un fundido (no letra a letra).

| Tipo | Para | Cómo se ve |
| --- | --- | --- |
| `Title` | Mundo completado, victoria | Centrado algo arriba: línea pequeña espaciada, titular grande (entra con rebote), separador ornamental y una o dos líneas de texto, sobre un halo oscuro. |
| `Boss` | Presentación de jefe | Entran dos **bandas de cine** negras (14 % de la pantalla cada una). En la de abajo, «JEFE DEL MUNDO 1» y el **nombre** en grande; en la de arriba, el consejo. |
| `Line` | Rescate de huevo, burlas | Arriba centrado, con su borde superior siempre bajo los huevos (aunque ocupe varias líneas): el **icono** (p. ej. el huevo, con brillo de su color) y la frase, sobre un halo oscuro suave. Entra y sale con un fundido. |
| `Shout` | «¡RUGE, ALMA!» | Titular enorme en el centro: entra de golpe, tiembla, late, con un resplandor de su color. |
| `Level` | Nombre del nivel al entrar | Ver [Nombre del nivel](#nombre-del-nivel-hudleveltitle). |
| `Story` | Prólogo y epílogo | Bandas de cine y velo oscuro; los párrafos aparecen uno tras otro, centrados. Si no caben, se encogen. |

| Campo | Valor | Uso |
| --- | --- | --- |
| `Headline Font` / `Text Font` | `LuckiestGuy SDF` / `Fredoka SDF` | Fuentes. |
| `Veil Sprite` / `Divider Sprite` | `HUD_Banner_Veil` / `HUD_Divider` | Velo y bandas; separador. |
| `Read Time` | 0,045 s por letra | Tiempo en pantalla: 1,6 s + 0,045 s por letra (o `Hold`, si se da). |
| `Fade In` / `Fade Out` | 0,4 / 0,5 s | Entrada y salida. |
| `Bar Height` | 0,14 | Alto de cada banda de cine (fracción de la pantalla). |

Tamaños (altura de mayúsculas, en unidades de diseño, antes de `TextScale`): titular 1,25 (1,3 en la presentación de habilidad), grito 2,0, línea pequeña 0,44, texto 0,5 (`Hud2D.BodyCap`), «Toca para continuar» 0,38. En `Boss`, los textos se ajustan al alto de las bandas, que crecen con `TextScale` (hasta el 22 % de la pantalla).

**Uso desde código:**
```csharp
HudBanners.Show(BannerKind.Boss, "Mono Ladrón Gigante", "Esquiva sus proyectiles y golpéalo cuando quede exhausto.", color, "Jefe del Mundo 1");
HudBanners.Show(BannerKind.Line, null, frase, color, null, iconoSprite);
```
Devuelve `false` si no hay HUD; así cada pieza mantiene su texto antiguo sobre el mundo como respaldo.

**Quién lo usa ya:** los **huevos** (`Line`, con su icono y color). Los jefes, el prólogo y el epílogo usarán `Boss`, `Shout`, `Title` y `Story` cuando existan.

## Nombre del nivel (`HudLevelTitle`)

Al empezar cada escena jugable, a los 0,6 s, aparece su título (banner `Level`) y a los pocos segundos se va solo. Dura unos 6,5–7 s en total, con el título completo en pantalla unos 3,2 s. No pausa ni bloquea nada.

**Secuencia:**
1. Un halo oscuro suave aparece y el **separador ornamental** se abre desde el centro en el color del mundo.
2. **El número** («1-1», «JEFE», «JEFE FINAL») entra con un rebote, y encima aparece **«MUNDO 1 · JUNGLA ESMERALDA»**, cerrando su espaciado de letras.
3. **El título entra letra a letra:** cada letra cae a su sitio y aparece con un fundido (0,05 s entre letras, 0,4 s cada una). Va en blanco con un degradado al color del mundo, con contorno y sombra.
4. Un **destello de luz** recorre el título de izquierda a derecha.
5. **Salida:** las letras suben y se apagan de izquierda a derecha, la línea del mundo se abre y el separador se cierra.

Es responsive como el resto: centrado en la zona segura (a 2/3 de su alto), escalado con `TextScale`, y el título se encoge si no cabe.

| Escena | Número | Título | Mundo (color) |
| --- | --- | --- | --- |
| `Level_1_1` … `Level_1_4` | 1-1 … 1-4 | Despertar en el Nido · El Dosel Peligroso · Las Zarzas Profundas · La Copa del Gran Árbol | Mundo 1 · Jungla Esmeralda (verde 0,55; 1; 0,4) |
| `Boss_1` | Jefe | El Rey de la Copa | Mundo 1 |
| `Level_2_1` … `Level_2_4` | 2-1 … 2-4 | Descenso a la Penumbra · La Galería de Ecos · El Filo Resonante · El Laberinto de Geodas | Mundo 2 · Cuevas de Cristal (azul cristal 0,5; 0,85; 1) |
| `Boss_2` | Jefe | El Acorazado Subterráneo | Mundo 2 |
| `Level_3_1` … `Level_3_4` | 3-1 … 3-4 | Los Fangales Tóxicos · El Cañón de las Ráfagas · El Vuelo de las Esporas · El Sauce Ancestral | Mundo 3 · Pantano de Viento y Niebla (verde niebla 0,6; 0,95; 0,8) |
| `Boss_3` | Jefe | El Señor de las Ráfagas | Mundo 3 |
| `Level_4_1` … `Level_4_4` | 4-1 … 4-4 | Los Ríos de Ceniza · Las Campanas de Basalto · La Gran Fractura · La Antecámara del Fuego | Mundo 4 · Cima Volcánica (naranja 1; 0,6; 0,25) |
| `Boss_Final` | Jefe final | El Rey Ladrón | Mundo 4 |

Títulos tomados de las fichas de cada nivel. En los jefes va la primera parte del título de la ficha; el nombre de la criatura queda para el banner `Boss` de su presentación.

| Campo | Valor | Uso |
| --- | --- | --- |
| `Levels` | 20 entradas (`Scene`, `Code`, `Title`, `World`, `Accent`) | La tabla; una escena que no esté no muestra nada. |
| `Delay` | 0,6 s | Espera desde que empieza la escena. |
| `Show On Start` | sí | Desactívalo para que lo muestre otro (p. ej. tras un prólogo) con `HudLevelTitle.Instance.Show(...)`. |

**Comprobado** con `HudLevelTitleTests`: `Level_1_1` en móvil 16:9 (capturas de toda la secuencia), `Boss_Final` en móvil con notch y `Level_3_2` en vertical; en los tres el título aparece y se va solo.

## Viñeta de tensión (`HudTensionVignette`)

GDD 8.4: como no hay barra de vida, el peligro se **siente** en los bordes de la pantalla. Cuando un momento es tenso, los bordes se oscurecen y se cierran hacia el centro, **en el color del peligro**. Cuando pasa, se abren despacio.

- **Poca tensión:** solo se oscurecen un poco las esquinas.
- **Más tensión:** el borde oscuro avanza hacia el centro. Por encima lleva una capa de **humo con volutas** que se mece y respira.
- **Desde el 45 %:** **late como un corazón** (pum-pum): el borde se cierra un poco en cada latido y destella en el color del peligro. Late más rápido cuanto más tensión hay: de 62 a 120 latidos por minuto.
- **Transiciones:** entra en 0,7 s y se va en 1,6 s.
- **Siempre a pantalla completa** (sea cual sea la proporción), por debajo del resto del HUD. Se mantiene durante las cinemáticas, porque la tensión narrativa también vive en ellas.

**Fuentes de tensión** (`TensionState.Set(fuente, nivel, color)` / `Clear(fuente)`; gana la más fuerte):

| Fuente | Nivel | Color |
| --- | --- | --- |
| **Gas tóxico ascendente** (`RisingGas2D`) | Aviso: 0,3. Subiendo: 0,2 + 0,8 × cercanía², donde la cercanía es 1 cuando el gas toca los pies de Alma y 0 a 7 u (`Tension Range`). Lleno y lejos: 0. | Verde tóxico (`TensionState.Toxic`) |
| **Fondos de jefe** (`BossBackdropPhases`, `Tension` por fase) | Jefes 1 y 2: 0,15 / 0,35 / 0,6. Jefe 3: 0,15 / 0,4 / 0,65. Jefe final: 0,2 / 0,5 / 0,8 (late desde la fase 2). | Jefe 1, azul tormenta; jefe 2, amatista; jefe 3, verde pantano; jefe final, rojo brasa (`TensionState.Fire`). |
| Cinemáticas y otros peligros (cuando existan) | Lo que pidan. | `TensionState.Narrative` (tinta), `Fire` o el suyo. |

| Campo | Valor | Uso |
| --- | --- | --- |
| `Rise Time` / `Fall Time` | 0,7 / 1,6 s | Entrada y salida. |
| `Heartbeat From` | 0,45 | Nivel desde el que late. |
| `Beats Per Minute` | 62 → 120 | Ritmo en el umbral y a tensión máxima. |
| `Preview` / `Preview Tint` | 0 / rojo brasa | En Play, fuerza un nivel para verlo desde el Inspector. |

**Comprobado** con `HudTensionTests`:
- Niveles crecientes en `Level_1_1`, con capturas.
- El latido medido: el borde se aviva de 62 a 73 en el rojo en cada pulso.
- Se apaga sola al quitar el peligro.
- El gas real en `Level_3_1`: la tensión pasa de 0,3 en el aviso a 0,73 cuando alcanza a Alma.
- `Boss_Final`: 0,2 en la fase 1 y 0,8 en la fase 3.
- También en pantalla vertical.

## Pendiente

Por orden acordado:
1. ~~Capa de banners~~ → hecha (`HudBanners`, TextMeshPro, responsive); falta usarla en jefes, prólogo y epílogo cuando existan.
2. ~~Nombre del nivel~~ → hecho (`HudLevelTitle`, banner `Level`).
3. ~~Viñeta de tensión~~ → hecha (`HudTensionVignette` + `TensionState`).
4. ~~Iconos de habilidades~~ → hechos como botones de acción táctiles.
5. **Control de movimiento táctil** (izquierda/derecha); `AlmaTouchControls.Move` ya está preparado.
6. **Marcas de progreso del jefe** (3 impactos).
7. Ocultar el HUD con el menú de pausa cuando exista.

## El Reencuentro (`HudEggReunion`)

La sala donde Alma recupera cada huevo al derrotar a un jefe. **La sala es un prefab hecho a mano, uno por jefe**; `HudEggReunion` (en el prefab del HUD, presente en todas las escenas) solo lo muestra. El contenido de cada sala lo hace el usuario.

**Flujo:** el jefe es derrotado, dice su discurso y desaparece o muere → **destello blanco** (0,3 s) → aparece **la sala del prefab** a pantalla completa, por encima de todo → pasados los segundos de la sala, otro destello devuelve a la arena → el jefe sigue (portal). No aparece ningún huevo en la arena antes. El huevo se **guarda** al abrirse la sala (`GameProgress.RescueEgg`) y, cuando acaba la cinemática del jefe y vuelve el HUD, **vuela a su hueco** (`HudEggIndicator.FlyIn`). El reloj de la derrota del jefe se detiene mientras la sala se ve.

**Los prefabs** (`Assets/Prefabs/Level/Progression/EggRooms/`):

| Prefab | Jefe (campo `Reunion Room`) | `Egg Id` |
| --- | --- | --- |
| `EggRoom_World1_Green` | Mono (`Boss_GiantMonkey_Jungle`) | Green |
| `EggRoom_World2_Blue` | Armadillo (`Boss_Armadillo_Caves`) | Blue |
| `EggRoom_World3_Purple` | Pterodáctilo (`Boss_Pterodactyl_Swamp`) | Purple |
| `EggRoom_World4_Red` | Rey Ladrón (`Boss_TRex_Volcano`) | Red |

**Contenido de las salas (12/10/2026).** Cada prefab tiene tres hijos (en ejecución pasan a colgar de un hijo `Stage` que se crea solo, para el acercamiento):

| Hijo | Qué es | Valores |
| --- | --- | --- |
| `Backdrop` | Fondo del mundo, escalado para cubrir 21:9 (37,4 × 16). | `EggRooms/Sprites/EggRoom_WorldN_BG.png` (Higgsfield, 2688×1152, 72 PPU, orden 0). **Los cuatro tienen la misma composición** (12/10/2026: los de los Mundos 2–4 se generaron con el del Mundo 1 como plantilla): suelo transitable abajo a la misma altura, círculo de piedra en el centro con un nido detrás, dos pilares con glifos de huella y huevo, haz de luz que baja al centro y una cascada a la derecha. Mundo 1: santuario en la selva. Mundo 2: cueva de cristales azules con cascada subterránea. Mundo 3: pantano con manglares, musgo colgante, nenúfares y niebla lila. Mundo 4: santuario volcánico de basalto y obsidiana con grietas de magma y cascada de lava (sin vegetación). |
| `Alma` | `SpriteRenderer` + `Animator` con `EggRooms/Alma_EggRoom.controller` (estados `Run` = `run.anim` y `Take` = `take.anim` a velocidad **0,55**, sin bucle: ≈1,2 s y se queda en la última pose). | Escala 2,2, en (−1,2; −3,26): pies sobre la franja de hierba delantera (y −5,9), a la izquierda del centro para que el huevo quede centrado. Orden 10. |
| `Egg` | `Resource_RescueEgg_<Color>` del mundo. | Escala 2,0, en (1,74; −1,71): sobre las manos de la pose Take. Orden 30. |

**La escena (la reproduce `EggReunionRoom`):**

1. Entran **bandas de cine** negras arriba y abajo (1,3 cada una, 0,6 s) y empieza un **acercamiento lento** hacia el sitio del huevo (hasta ×1,1 al final; ese punto no se mueve en pantalla). En el aire flotan **motas de luz** todo el rato.
2. Alma entra **corriendo desde la izquierda** (x −21) a 10 u/s, **levantando polvo**, con su **sombra** en el suelo, y **frena derrapando** en los últimos 1,4 (nube de polvo) hasta su sitio (≈2,1 s en total).
3. Al pararse hace **Take una vez** (manos arriba, mirando hacia arriba) y se queda así.
4. Se abre una **columna de luz** sobre su sitio y el **fondo se oscurece** (×0,68, efecto foco) mientras Alma coge un tono cálido de la luz. El huevo **baja despacio** dentro de la columna (de 0,5 s a 3 s tras pararse Alma, frenando al final, balanceándose y girando un poco), con un **halo** y **8 rayos cortos girando** detrás, **destellos** cayendo y su **sombra creciendo** en el suelo.
5. Al posarse en sus manos: **estallido de 40 destellos**, **destello blanco** y **dos latidos**; la columna se cierra en 1 s (el fondo vuelve a medias) y el halo y los rayos siguen brillando hasta el final (8,5 s), cuando salen las bandas.

Todo lo de los pasos 1–5 (bandas, sombras, polvo, motas, luz, rayos, destellos) se crea en tiempo de ejecución con texturas de `HazardFx` (sin imágenes nuevas); la luz usa el `Light Color` de cada sala: dorado (Mundo 1), celeste (2), lila (3) y naranja (4). La columna, el halo y los rayos van detrás de Alma (`Light Order` 5); los destellos y el flash, delante de todo (+30); las bandas, encima de todo (200).

| Campo de `EggReunionRoom` | Base | Uso |
| --- | --- | --- |
| `Duration` | 8,5 s | Tiempo que se ve la sala antes de volver a la arena. |
| `Alma` | — | Opcional: su `Animator` (estados `Run` y `Take`), colocado donde se para. |
| `Alma Start X` | −21 | Desde dónde empieza a correr. |
| `Run Speed` | 10 | Velocidad de la carrera (u/s). |
| `Brake Distance` | 1,4 | Tramo final en el que frena (derrape). |
| `Egg` | — | Opcional: el huevo, colocado donde acaba (en sus manos). |
| `Egg Drop Height` | 9 | Desde cuánto más arriba baja. |
| `Egg Drop Time` | (0,5; 3) | Cuándo empieza a bajar y cuándo llega (s desde que Alma se para; desde que aparece la sala si no hay Alma). |
| `Heartbeat` | sí | Los dos latidos al llegar. |
| `Light Color` | por mundo | Color de la columna, el halo, los rayos, los destellos y las motas. |
| `Sparkle Material` | `FX_Fire_Additive` | Material aditivo de destellos y motas (sin él no hay). |
| `Light Order` | 5 | Orden de la columna/halo/rayos; destellos y flash +30. |
| `Backdrop` | el hijo `Backdrop` | Se oscurece con la luz. |
| `Ground Y` | −5,9 | Altura del suelo: sombras y polvo. |
| `Dust Material` / `Dust Color` | `FX_Fire_Smoke` / tierra (0,62; 0,5; 0,36; 0,6) | Polvo de la carrera y del derrape (sin material no hay). |
| `Push In` | 1,1 | Zoom final del acercamiento (1 = sin acercamiento). |
| `Letterbox` | 1,3 | Alto de cada banda de cine (0 = sin bandas). |
| `Spotlight Dim` | 0,68 | Brillo del fondo con la luz abierta. |

**Vuelta a la arena (12/10/2026).** `onDone` se llama en cuanto se retira la sala, aún **detrás del destello blanco**: el jefe quita su plano (`ClearShot`), muestra a Alma y la cámara **salta a ella** (`SnapToTarget`); al aclararse el destello ya se ve a Alma, no el sitio vacío del jefe. En el pterodáctilo, en ese momento aparecen también el portal y Alma junto a él, en la rama. Comprobado en los combates completos de los cuatro jefes: el primer fotograma tras la sala muestra a Alma en pantalla, visible y con el zoom normal.

Sin prefab asignado (o sin HUD) no se muestra nada: el huevo se guarda y el jefe sigue al momento.

**Pruebas:** `EggRoomAllTests` (las cuatro salas seguidas: cada una se reproduce entera y se cierra), `EggRoomSceneTests` (12/10/2026: la sala del Mundo 1 en Boss_1: Alma corre, frena en −1,2, pasa a Take, el huevo baja y se posa en sus manos y la sala se cierra), `EggReunionTests` (la sala aparece centrada en pantalla, con su grupo de orden por encima de todo, se guarda el huevo, se cierra y el huevo vuela al HUD) y los combates completos de los cuatro jefes (`MonkeyCycleTests`, `ArmadilloTests.FullFight`, `PterodactylTests.FullFight`, `TRexTests.FullFight`) llegan al final con la sala incluida.

## Aviso de jefe vulnerable (`HudBossAlert`)

Común a todos los jefes (11/10/2026). Cuando un jefe abre su ventana para golpearlo, llama a:

```csharp
HudBossAlert.Show(() => puntoDelJefe, segundosDeVentana, "¡Mareado!", colorDelJefe);
// ... y al cerrarse la ventana (golpe, se recupera, Alma muere):
HudBossAlert.Hide();
```

- **Grito** (`BannerKind.Shout`) con el texto, en el color del jefe, para que el jugador se entere esté donde esté.
- **Insignia en el borde de la pantalla** mientras el jefe está **fuera de cámara**: disco oscuro con tres estrellas de mareo girando, **flecha** hacia el jefe y un **anillo de 20 puntos que se vacía** (en el sentido de las agujas del reloj) con el tiempo que queda; los últimos instantes late más rápido. Si el jefe está en pantalla se oculta (ya se ven sus estrellas).
- Responsive: dentro de la zona segura, escalada con `UiScale`; se desvanece con el HUD en las cinemáticas.
- No hay que colocar nada: el componente se añade al HUD la primera vez que se usa.

| Jefe | Cuándo | Grito | Punto |
| --- | --- | --- | --- |
| Mono (Jefe 1) | Agotado en el suelo | «¡Agotado!» (`Tired Shout`) | su cabeza |
| Armadillo (Jefe 2) | Mareado contra la pared | «¡Mareado!» (`Dizzy Shout`) | la grieta del caparazón |

**Localizador del jefe (12/10/2026).** Como el juego es en vertical y se ve poca arena, los cuatro jefes llevan además un **localizador**: la **misma insignia sin texto**, sin estrellas y sin anillo (solo disco oscuro, borde y flecha en el color del jefe), al **75 %** del tamaño y con un latido suave, que aparece en el borde de la pantalla **siempre que el jefe esté fuera de cámara** y apunta hacia él. Se oculta en cuanto el jefe entra en pantalla.

```csharp
HudBossAlert.Track(this, () => centroDelJefe, colorDelJefe);   // cada fotograma durante el combate
HudBossAlert.Track(this, null, default);                        // fuera del combate: se apaga
```

- Cada jefe lo llama en su `Update` (`UpdateTracker`) con el centro de su sprite: activo durante el combate, apagado en la intro, en las cinemáticas, en la derrota y mientras su sprite está oculto (p. ej. el mono desaparecido). Al destruirse el jefe se apaga solo.
- Si se abre un **aviso** (`Show`), la insignia pasa a ser la del aviso (estrellas, anillo, su objetivo) mientras dure; al cerrarse vuelve el localizador.
- Se desvanece con el HUD en las cinemáticas, como el aviso.

Pruebas (PlayMode): con un objetivo lejos fuera de cámara la insignia aparece en el borde derecho con la flecha a 18° (la dirección real); con el objetivo en pantalla se oculta; en el combate del Armadillo el aviso se abre al marearse y se cierra con el Pisotón. `BossTrackerTests`: en el Jefe Final, con el T-Rex en pantalla el localizador está oculto; con Alma en el otro extremo (T-Rex fuera de pantalla a la derecha) aparece en el borde derecho, opaco y sin estrellas.
