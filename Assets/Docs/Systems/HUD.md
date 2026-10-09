# HUD

**Estado (9 de octubre de 2026):** implementados el **indicador de hijos** (GDD 8.1), los **botones de acción táctiles** con las runas de las habilidades la **capa de banners** de texto, el **nombre del nivel al entrar** y la **presentación pausada de cada habilidad**, todo con TextMeshPro y **responsive** (zona segura y escala por dispositivo). El prefab `HUD` está colocado en las **20 escenas jugables** (16 niveles y 4 jefes). Se construye paso a paso; los siguientes elementos están en [Pendiente](#pendiente).

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
| `HUD.prefab` | Raíz del HUD (`Hud2D` + `HudEggIndicator` + `HudAbilityButtons` + `HudBanners` + `HudLevelTitle`). Uno por escena jugable. |
| `Hud2D.cs` | Raíz: sigue la vista de la cámara **sin el temblor** (`AlmaCameraFollow.ViewPosition`), escala con su tamaño (diseñado para tamaño 8), calcula la zona segura y la escala de la interfaz ([responsive](#responsive)), dibuja por encima de todo (`Sorting Order` 1000) y se desvanece durante las cinemáticas (`Visibility`). |
| `CinematicState.cs` | Estado global «hay una cinemática en curso» (`Begin`, `End`, `IsPlaying`, evento `Changed`). |
| `HudEggIndicator.cs` | Indicador de hijos. |
| `HudAbilityButtons.cs` | Botones de acción (táctiles), registro de habilidades y presentación pausada de una habilidad nueva. |
| `HudBanners.cs` | Capa única de banners de texto. |
| `HudLevelTitle.cs` | Tabla de títulos de las 20 escenas jugables; muestra el del nivel al empezar. |
| `HudText.cs` | Estilo de texto común (TextMeshPro): fuentes, contorno, sombra, degradado, ajuste al ancho. |
| `Fonts/LuckiestGuy-Regular.ttf` | Fuente de titulares (cartoon, mayúsculas). Licencia Apache 2.0 (`LuckiestGuy-LICENSE.txt`). |
| `Fonts/Fredoka-SemiBold.ttf` | Fuente de texto (redondeada). Instancia fija del Fredoka variable (peso 600). Licencia SIL OFL (`Fredoka-OFL.txt`). |
| `Fonts/LuckiestGuy SDF.asset`, `Fonts/Fredoka SDF.asset` | Fuentes de TextMeshPro (campo de distancia: nítidas a cualquier tamaño). Dinámicas: el atlas se rellena solo con los caracteres que se usan. |
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

## Pendiente

Por orden acordado:
1. ~~Capa de banners~~ → hecha (`HudBanners`, TextMeshPro, responsive); falta usarla en jefes, prólogo y epílogo cuando existan.
2. ~~Nombre del nivel~~ → hecho (`HudLevelTitle`, banner `Level`).
3. **Viñeta de tensión** (GDD 8.4).
4. ~~Iconos de habilidades~~ → hechos como botones de acción táctiles.
5. **Control de movimiento táctil** (izquierda/derecha); `AlmaTouchControls.Move` ya está preparado.
6. **Marcas de progreso del jefe** (3 impactos).
7. Ocultar el HUD con el menú de pausa cuando exista.
