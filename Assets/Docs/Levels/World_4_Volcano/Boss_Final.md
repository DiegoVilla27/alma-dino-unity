> **Estado (11/10/2026):** diseño acordado (sección 3), animaciones hechas y combate implementado y probado (sección 6). Escena de trabajo `Assets/Scenes/World_04/Boss_Final.unity` con el fondo de la arena ([`Backdrop_Boss_Final`](../../LevelPieces/Backgrounds.md#jefe-final--world_4backdrop_boss_finalprefab): el Ojo del Volcán, que despierta hasta la erupción con las fases). El prototipo anterior (subida vertical con magma ascendente) se retiró: el magma ascendente se descartó como trampa el 5/10/2026. El jefe se construye sobre la base de los Jefes 1–3 para reutilizar su estructura. No habrá audio.

# 👑 Jefe Final & Epílogo: "El Rey Ladrón — Tirano Ancestral"
> **Mundo 4: Cima Volcánica** | **El Ojo del Volcán (El Caldero de Magma) & Final del Juego**

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En el corazón del cráter volcánico, sobre un suelo de obsidiana sobre un lago de magma burbujeante, aguarda el causante de toda la tragedia: el **Rey Ladrón**, un colosal T-Rex Anciano con placas tectónicas de obsidiana fundidas a su lomo y ojos carmesí como carbón encendido.
- **Estado Emocional de Alma:** Determinación suprema, furia maternal implacable y devoción absoluta. Toda la travesía, cada golpe, caída y aprendizaje convergen en este instante. No hay duda ni miedo: Alma regresará a casa con todos sus hijos.
- **Textos en Pantalla / Banners & Diálogos:**
  - *Inicio del Enfrentamiento:*
    > *"¡BATALLA FINAL: EL REY LADRÓN — TIRANO ANCESTRAL!*  
    > *Has cruzado la jungla, las cavernas y los pantanos. Has despertado tus cuatro poderes ancestrales.*  
    > *¡Demuéstrale al rey de las bestias lo que es el verdadero Rugido de una Madre!"*
  - *Transición a Fase 3:*
    > *"¡El volcán entra en erupción! ¡El caldero colapsa!"*
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

- **La Arena de Combate (El Ojo del Magma):** suelo de **obsidiana** (kit del Mundo 4: `Platform_Ground_Obsidian_Volcano` y `Platform_Floating_Obsidian_Volcano`) sobre el lago de magma del fondo. Sin vegetación.
- **Paleta de Color Principal:**
  - Tirano: Escamas gris ceniza (`#2B2B2B`), coraza de obsidiana con venas de magma brillante anaranjado (`#FF4500`) y colmillos amarillentos manchados de hollín. Ojos rojos.
  - Lava y Magma: Gradiente radiante desde amarillo incandescente (`#FFD166`) hasta rojo magma (`#D62828`).
  - Acento del jefe (tarjeta, alertas, nombre del globo): **el color del Rugido** (1; 0,75; 0,25), la habilidad que lo derrota, igual que el Jefe 3 usa el del Dash.
  - Humo: paleta del Mundo 4 de `NinjaSmoke2D` (carbón y brasas naranjas).
  - Epílogo: Verde esmeralda suave, cielo azul dorado de atardecer (`#F4A261`), flores silvestres y luz cálida acogedora.

---

## 3. 🧱 Diseño del combate (acordado el 11/10/2026)

Examen final: una arena de una pantalla, **tres fases y un golpe por fase** (como los Jefes 1–3); la fase 3 pide las cuatro habilidades. Solo el **Pisotón** (o la estalactita al final) le hace daño; aterrizar encima normal solo rebota a Alma; tocar su cuerpo, la cortina de fuego, los meteoros o la roca mata.

### Arena

- Suelo ancho de **obsidiana** (≈ 28 u) entre dos **topes laterales** contra los que se estampa al embestir: por ahora un **sprite cuadrado simple** (el arte se cambia más adelante).
- Dos **plataformas flotantes de obsidiana** a media altura para esquivar y una **cornisa alta** central.
- La **estalactita colosal** cuelga del techo sobre el centro, con su **anclaje agrietado** junto a la cornisa alta (provisional: cuadrados).
- Fondo `Backdrop_Boss_Final` (ya existe) con sus tres fases.
- **Sin nido de huevos:** el huevo se obtiene al derrotar a cada jefe. Esa recompensa falta también en los Jefes 1–3 y se omite por ahora.

### Ataques (se suman por fase)

| Ataque | Aviso | Cómo se esquiva |
| --- | --- | --- |
| **Embestida** | Baja la cabeza, rasca el suelo, ojos parpadeando en rojo (~0,8 s) | Cruza la arena con la mandíbula baja: **Doble Salto** o subir a una plataforma. |
| **Pisotón de fuego** | Levanta la pata, la coraza brilla | Golpe al suelo que lanza una **cortina de fuego** alta que recorre el suelo: solo se atraviesa con **Dash** (durante el Dash no quema). |
| **Lluvia de meteoros** (fase 2+) | Ruge al techo; cada meteoro marca en **amarillo** dónde caerá | Salir de las marcas; no persiguen a Alma. Bolas de fuego de `ProjectileTrap2D` en modo manual. |
| **Roca rodante** (fase 2+) | Patea un peñasco incandescente hacia Alma | **Rugido** de frente en el momento justo: vuelve rodando hacia él (`IRoarTarget`). Si no, aplasta. |

### Fases

1. **La Carga del Coloso** (Doble Salto + Dash + Pisotón): alterna embestida y pisotón de fuego. Si la embestida falla, **se estampa contra el tope lateral y queda atascado 4 s** (`HudBossAlert` «¡ATASCADO!»): Alma sube a su lomo y hace **Pisotón sobre la placa dorsal agrietada** → golpe 1.
2. **La Lluvia de Meteoros** (Rugido): más rápido; suma meteoros y, al cerrar cada ciclo, la roca rodante. Devuelta con el Rugido, le golpea las fauces y queda **aturdido 4 s** con la cabeza en el suelo («¡ATURDIDO!»): **Pisotón en la cabeza** → golpe 2.
3. **El Gran Colapso** (las cuatro): los extremos del suelo se hunden en lava (`Trap_LavaPool_Volcano`) y se encienden géiseres (`Trap_FireGeyser_Volcano`); combina todos los ataques, más rápido. Cuando queda atascado o aturdido, **el anclaje de la estalactita brilla**: Doble Salto por las plataformas → Dash a través de una cortina de fuego → en la cornisa, **Rugido** al anclaje (se agrieta) → **Pisotón** sobre él: la estalactita cae sobre el tirano → golpe 3.

**Intro:** Alma bloqueada, cámara sobre él, frases, Taunt (rugido) y tarjeta «JEFE FINAL · El Rey Ladrón · Tirano Ancestral».
**Si Alma muere:** se reinicia el ciclo de la fase (roca, cortinas, anclaje); los golpes se conservan.
**Derrota:** la estalactita lo hunde en el magma (destello rojo, hundimiento y humo por código), últimas frases y final del juego. El **epílogo** (valle al atardecer y nacimiento de las cuatro crías) es un paso aparte, después del jefe.

### Animaciones (8 frames de 256 × 256, hoja 1024 × 512, fondo transparente)

| Animación | Uso |
| --- | --- |
| **Taunt** | Rugido: intro, derrota y llamada de meteoros. El frame 1 es la pose quieta. |
| **Charge** | Embestida con la cabeza baja y la mandíbula abierta (bucle). |
| **Stomp** | Levanta la pata y golpea el suelo (la cortina sale por código); también para patear la roca. |
| **Stunned** | Atascado / aturdido: cabeza baja, tambaleándose; estrellas por código. |

Caminar, la caída y el hundimiento en el magma se hacen por código.

#### Taunt — `Prefabs/Bosses/TRex/Animations/Taunt/` (hecho el 11/10/2026)

- `Boss_TRex_Taunt_Sheet.png` (1024 × 512, 8 frames de 256 × 256, fondo transparente; 1 generación de Higgsfield con el T-Rex del concept sheet y la hoja del armadillo como referencia de formato; halo semitransparente eliminado, misma escala y pies alineados en todos los frames).
- Sprites `Boss_TRex_Taunt_0` … `_7`: 45 PPU (5,69 u por frame), pivote en los pies (0,5; 4/256).
- `Boss_TRex_Taunt.anim` (12 fps, bucle) y `Boss_TRex.controller` (controlador principal del jefe; se le irán sumando los estados). Vista previa: `Prefabs/Bosses/TRex/Boss_TRex_Taunt_Preview.prefab`.
- Frames: 1 de pie, inclinado, boca cerrada (pose quieta) · 2 baja la cabeza y toma aire · 3–4 se yergue, abre los brazos y alza la cabeza con la boca abierta · 5–6 rugido a máxima apertura (en el 6 se inclina hacia delante) · 7–8 cierra la boca y vuelve a la pose inicial.

### Se reutiliza

`CinematicState`, `SpeechBubble2D`, tarjeta `HudBanners` BossIntro, `HudBossAlert`, `NinjaSmoke2D` (paleta del Mundo 4), `ProjectileTrap2D` (meteoros), `IRoarTarget` (roca), las fases de `Backdrop_Boss_Final`.

#### Charge — `Prefabs/Bosses/TRex/Animations/Charge/` (hecho el 11/10/2026)

- `Boss_TRex_Charge_Sheet.png` (1024 × 512, 8 frames de 256 × 256; 1 generación de Higgsfield con el concept y la hoja del Taunt como referencia). Cada frame conserva su posición en la celda generada (escala uniforme, suelo de cada fila en y 252), así que se mantienen el rebote y la zancada.
- Sprites `Boss_TRex_Charge_0` … `_7`: 45 PPU, pivote en los pies (0,5; 4/256).
- `Boss_TRex_Charge.anim` (12 fps, bucle), añadido a `Boss_TRex.controller`. Vista previa `Boss_TRex_Charge_Preview.prefab` (con su controller propio).
- Carrera con la boca abierta y la cola recta: la fila de abajo repite casi igual la de arriba (ciclo de 4 frames ×2) y en los frames 4 y 8 va en el aire. Aceptada así por el usuario.

#### Stomp — `Prefabs/Bosses/TRex/Animations/Stomp/` (hecho el 11/10/2026)

- `Boss_TRex_Stomp_Sheet.png` (1024 × 512, 8 frames de 256 × 256; 1 generación de Higgsfield). Cada frame se recorta solo con sus propios píxeles (el hocico del frame 5 invadía la celda del 6) y conserva su posición en la celda generada. Para que quepa la pata levantada, el T-Rex sale ~5 % más pequeño que en el Taunt.
- Sprites `Boss_TRex_Stomp_0` … `_7`: 45 PPU, pivote en los pies (0,5; 4/256).
- `Boss_TRex_Stomp.anim` (12 fps, **sin bucle**: se reproduce una vez por pisotón), añadido a `Boss_TRex.controller`. Vista previa `Boss_TRex_Stomp_Preview.prefab`.
- Frames: 1 reposo · 2–4 pasa el peso atrás y levanta la pata derecha (se echa atrás, boca abierta, magma encendiéndose) · **5 impacto** (pata plantada, cuerpo adelante y abajo, rugido, magma al máximo: aquí sale la cortina de fuego) · 6 aguanta · 7–8 se endereza y vuelve al reposo.

#### Stunned — `Prefabs/Bosses/TRex/Animations/Stunned/` (hecho el 11/10/2026)

- `Boss_TRex_Stunned_Sheet.png` (1024 × 512, 8 frames de 256 × 256; 1 generación de Higgsfield). Recorte por píxeles propios, posición de la celda conservada, tamaño casi igual al del Taunt.
- Sprites `Boss_TRex_Stunned_0` … `_7`: 45 PPU, pivote en los pies (0,5; 4/256).
- `Boss_TRex_Stunned.anim` (12 fps, bucle), añadido a `Boss_TRex.controller`. Vista previa `Boss_TRex_Stunned_Preview.prefab`. Se usa para «atascado» (tope lateral) y «aturdido» (roca); las estrellas van por código.
- Aturdido: magma **apagado** (grietas oscuras), ojos medio cerrados, lengua fuera, brazos caídos; las placas del lomo se ven arriba (donde va el Pisotón). Frames: 1 cabeza baja · 2–3 cabecea adelante y abajo · 4–5 recupera un poco la cabeza · 6 ojos en blanco · 7–8 vuelve a caer (bucle). La cabeza baja hasta las rodillas, no hasta el suelo.

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets

- [x] Taunt.
- [x] Charge.
- [x] Stomp.
- [x] Stunned.
- [x] Estalactita colosal (`stalactite_colossal`, colgando del racimo `stalactites_obsidian`).
- [ ] Arte de los topes laterales (ahora cuadrados provisionales).
- [ ] Epílogo: cuatro crías saliendo del cascarón; Alma recostada en la hierba.

## 6. Implementación (11/10/2026)

**Archivos:** `Prefabs/Bosses/TRex/Scripts/TRexBoss2D.cs` (el jefe), `TRexRoarTarget2D.cs` (pasa el Rugido de Alma a la roca y a la estalactita) y `Prefabs/Bosses/TRex/Boss_TRex_Volcano.prefab` (en `Boss_Final` en (0, 0); crea su sprite, globo, estrellas, cortinas, meteoros y roca por código).

### Arena (`Scenes/World_04/Boss_Final.unity`, objeto `Level`)

Montada por Claude y **ajustada a mano por el usuario** (11/10/2026): esas posiciones y tamaños mandan y no se tocan.

| Objeto | Pieza | Medidas actuales (colisionador) |
| --- | --- | --- |
| `Floor_Center` | `Platform_Ground_Obsidian_Volcano` | x −13,39…13,39; arriba −5,41 |
| `Floor_Left` / `Floor_Right` | `Platform_Ground_Obsidian_Volcano` | x −21,13…−12,87 y 12,87…20,71; arriba −5,39 (se hunden en la fase 3) |
| `Platform_Left` / `Platform_Right` | `Platform_Floating_Obsidian_Volcano` | x −8,15…−4,65 (arriba −2,62) y 4,75…8,25 (arriba −2,41) |
| `Ledge_Anchor` (cornisa) | `Platform_Floating_Obsidian_Volcano` | x −3,32…−0,32; arriba 0,09 |
| `Wall_Left` / `Wall_Right` | cuadrado provisional (`Level_Placeholder`, sólido) | caras interiores en x ∓19, hasta y 11 |
| `Stalactite_Ceiling` | `stalactites_obsidian` (prop del Mundo 4), colocado por el usuario | racimo del techo, x −2,55…4,15, de y 2,28 a 10,07; orden 2; fijo |
| `Stalactite_Colossal` | `stalactite_colossal` (prop del Mundo 4, generado el 12/10/2026) + trigger + `TRexRoarTarget2D` | la punta grande central del racimo, delante de él (orden 3): x 0,01…1,59, de y 1,58 a 8,90; es la que se agrieta y cae |
| `Lava_Left` / `Lava_Right` | `Trap_LavaPool_Volcano` (inactivas hasta la fase 3) | centro x ∓16, 6 × 1,2, bajo los suelos laterales; **Order in Layer −4** |
| `Abyss_DeathZone` | `Trap_DeathZone_Universal` | y −12 |
| `CameraBounds` | `CameraBounds2D` | 44 × 16 centrado en (0; 1,5), altura fija 1,5 |

- El jefe usa la arena del usuario: `Wall Left X` / `Wall Right X` ∓19 y `Floor Y` −5,41 (en el prefab).
- Alma empieza sobre `Platform_Left` con las cuatro habilidades. La plataforma flotante de prueba oculta que había al nivel del suelo se **borró**.
- **Cámara:** la del prefab común `Camera_Alma`, igual que en el resto del juego (12/10/2026): la vista va por delante de hacia donde mira Alma, también durante el combate (el jefe ya no encuadra la cámara entre él y Alma). La altura no se mueve nunca (`CameraBounds2D`, `Fixed Camera Y` 1,5) y el área de 44 de ancho limita hasta dónde se desplaza.
- Con la arena más ancha, atascado contra la pared (x ∓16,3) su lomo queda a ~8,5 del borde de la plataforma más cercana: se llega con Doble Salto + Dash.

### Combate

- **Intro** (~15 s): de pie en x 7 mirando a Alma, frases («¿La madre ha llegado hasta mi volcán?» y «¡Tus crías ahora son mías!»; globo a `Bubble Height` 4,1), rugido (Taunt) y tarjeta «JEFE FINAL · El Rey Ladrón · Tirano Ancestral».
- **Ciclo por fase** (`Phases[].Cycle`, en orden, con `Rest` entre ataques; nunca empieza otro ataque mientras caen meteoros):

| Fase | Ciclo | Embestida | Cortina | Meteoros | Roca | Choque contra la pared | Ventana |
| --- | --- | --- | --- | --- | --- | --- | --- |
| 1 | Pisotón → Embestida | 11 u/s | 7 u/s, hacia Alma | — | — | **atascado** (ventana) | 4 s |
| 2 | Pisotón → Embestida → Meteoros → Patada | 12,5 u/s | 8 u/s | 4 | 6,5 u/s | se sacude (0,7 s) | 4 s |
| 3 | Meteoros → Patada (sin pisotón, decisión del usuario) | — | — | 6 | 7 u/s | — | 8 s |

- **Pisotón:** en el frame 5 sale una **cortina de fuego** (3,4 de alto: más que un Doble Salto) que recorre el suelo hasta la pared. Mata salvo si Alma está en pleno Dash (el Dash es aéreo: salto + Dash). Es un sistema de **partículas** con materiales propios (`Prefabs/Bosses/TRex/Materials/FX_Fire_Additive.mat`, *Legacy Shaders/Particles/Additive*, y `FX_Fire_Smoke.mat`, *Alpha Blended*): llamas aditivas (160 por segundo) que hierven hacia arriba con ruido y se enfrían de blanco-amarillo a naranja y rojo oscuro, un núcleo blanco en la base, chispas estiradas, humo negro que sale por arriba, un resplandor que titila detrás y marcas incandescentes en el suelo que se enfrían a negro. Al llegar a la pared deja de matar, suelta humo y se apaga en 0,5 s.
- **Embestida:** aviso de 0,8 s (tiembla con destellos rojos), corre hasta que el hocico toca la pared. Caja letal baja (`Charge Box`, 2,3 de alto): se salta con Doble Salto o se espera en una plataforma.
- **Meteoros:** ruge; cada meteoro marca en amarillo su punto de caída 1 s antes (el primero, donde está Alma) y cae desde y 11. Provisional: un **cuadrado blanco** que gira (hasta que haya arte de meteorito). **No atraviesan plataformas:** al aparecer, cada meteoro busca lo primero sólido bajo él (plataforma, cornisa o suelo); la marca se pinta ahí y el meteoro se rompe ahí, así que debajo de una plataforma Alma está a salvo.
- **Patada:** con el Stomp lanza una **roca ardiente** rodando hacia Alma: la roca del Rugido teñida (gira) envuelta en llamas aditivas que se quedan atrás al rodar, resplandor que titila, chispas y humo (mismos materiales que la cortina). Un **Rugido de frente** la devuelve al doble de velocidad; al llegar a su hocico queda **aturdido**. Si no se devuelve, mata a Alma y se rompe contra la pared.
- **Ventanas** (`HudBossAlert`: «¡ATASCADO!», «¡ATURDIDO!»; en la fase 3, las pistas de abajo; estrellas sobre su cabeza): un **Pisotón** sobre su lomo (`Back Box`) le quita una vida en las fases 1 y 2; aterrizar sin Pisotón solo rebota. Su lomo (3,4 sobre el suelo) se alcanza desde las plataformas.
- **Fase 3 (El Gran Colapso):** ruge, `Floor_Left` y `Floor_Right` pierden el colisionador, se hunden y desaparecen, y aparecen las lavas; camina hasta x 1, bajo la estalactita, y ya no se mueve de ahí. Pisarlo no le hace nada. Aturdido: la cornisa brilla; **Rugido** a la estalactita desde la cornisa (se agrieta y se tiñe) y **Pisotón** en la cornisa: la estalactita cae sobre su lomo → golpe 3.
- **Pistas de la fase 3** (mientras está aturdido, paso a paso): cada paso grita una vez (`HudBanners` Shout) y una **flecha** que rebota marca dónde ir; la insignia del borde de `HudBossAlert` apunta al mismo sitio si está fuera de pantalla.
  1. Alma no está en la cornisa: «¡SUBE A LA CORNISA!» y flecha hacia abajo sobre `Ledge_Anchor`.
  2. En la cornisa, estalactita sin grieta: «¡RUGE A LA ESTALACTITA!» y flecha hacia la derecha junto a la estalactita, por encima de la cabeza de Alma.
  3. Estalactita agrietada: «¡PISOTÓN EN LA CORNISA!» y flecha sobre la cornisa.
  Los textos son `Hint Climb`, `Hint Roar` y `Hint Pound` en el Inspector.
- **Golpe:** destello rojo, temblor y humo; se borran cortinas, meteoros y roca; fase siguiente (el fondo `Backdrop_Boss_Final` cambia de fase).
- **Derrota:** al caerle la estalactita (destello rojo) se endereza en el **frame base del Taunt** (frame 1) y la estalactita queda apoyada sobre él (`Stalactite Rest Height` 3,8). Alma se oculta y queda congelada. Dice sus frases («¡Imposible...! Tu valor arde más que mi fuego...» y «El volcán... me reclama...»; textos aptos para todos los públicos); al terminar se carga 1,1 s (temblor creciente, parpadeo rojo-blanco, se hincha un 8 % y saltan tres explosiones pequeñas de su cuerpo) y **estalla**: fogonazo blanco, bola de fuego de llamas aditivas, restos de obsidiana con gravedad, chispas estiradas y nube de humo negro (todo partículas, `Explosion`), temblor fuerte y humo. Desaparece; la estalactita **cae de golpe** (sale ya a 12 u/s con gravedad de 120 u/s², ≈0,2 s, soltando una estela de polvo), **se estrella** contra el suelo (`StalactiteImpact`: destello, nube de polvo que rueda a los lados, restos de obsidiana con gravedad, chispas estiradas, temblor fuerte) y **se clava**: entra con el 60 % de su velocidad (8–25 u/s) y frena de forma uniforme hasta quedarse justo en Y = −8,69 (`Stalactite Sink Y`), en ≈0,7 s. Alma vuelve. El **epílogo** está pendiente (paso aparte); por ahora el combate termina ahí (evento `Defeated`).
- **Si Alma muere:** se borran los ataques y empieza el ciclo de la fase desde el principio; se conservan los golpes, el colapso y la grieta de la estalactita. Él vuelve a su sitio (fases 1–2: al lado contrario a Alma; fase 3: el centro).

### Valores en el Inspector

`Phases` (tabla de arriba), `Ground Sink` 0,18 (los pies se hunden en el suelo para que no parezca flotar), `Curtain Height` 3,4, `Boulder Radius` 0,9, `Meteor Warning` 1 s, `Stomp Bounce` 13,5, `Soft Bounce` 9, `Scale` 1, cajas `Stand Box` (−1,2…2,2; alto 3), `Charge Box` (−1,8…2,7; alto 2,3), `Back Box` (−0,8…1,6; lomo a 3,4) y `Snout X` 2,7 (para escala 1, mirando a la derecha, desde los pies).

> La hoja `Boss_TRex_Stunned_Sheet.png` está importada a **50 PPU** (las demás a 45): aturdido se ve un 10 % más pequeño. Las cajas están medidas a 45 PPU; si se mantiene 50, bajar el lomo de `Back Box` (z) a ~3,1.

### Pruebas (PlayMode, copia del proyecto)

- `FullFight`: intro; la cortina mata a Alma de pie y un salto + Dash la atraviesa; atascado tras la embestida con alerta; aterrizar no hace daño y el Pisotón sí (1); en la fase 2 la roca no devuelta mata, devuelta lo aturde y el Pisotón da el 2; en la fase 3 los lados se hunden, aparece la lava, se coloca en x 1, pisarlo no hace nada, Rugido desde la cornisa agrieta la estalactita, Pisotón en la cornisa la suelta y lo derrota (3); termina con Alma visible y con física.
- `ChargeKills`: la embestida no alcanza a Alma sobre una plataforma y la mata en el suelo.
- En `FullFight`, fase 3: nunca hace el pisotón; con él aturdido la pista va por los pasos 1 → 2 (en la cornisa) → 3 (tras agrietarla) y la flecha se ve.
- En `FullFight`, derrota: mientras habla se ve `Boss_TRex_Taunt_0`; después hay explosión, su sprite desaparece y la estalactita queda activa con la base en el suelo (y ≈ −5,56).
- `MeteorsStopOnPlatforms`: un meteoro sobre Alma debajo de `Platform_Left` cae sobre la plataforma (y −2,62) y no la mata; en campo abierto sí.
- `CameraFixedHeightAndLookAhead`: la altura de la cámara no cambia (1,5) al subir a la cornisa y volver; con el jefe en x 15 y Alma en −17, −8, 0, 8 y 16, Alma queda siempre a ≥ 3,8 del borde y el jefe a la vista cuando cabe.


## Huevo: «El Reencuentro» (11-12/10/2026)

Cuando termina su discurso y desaparece o muere (en el jefe final, cuando la estalactita queda en el suelo), un destello lleva a la sala del **Huevo Rojo**: el prefab `Prefabs/Level/Progression/EggRooms/EggRoom_World4_Red` (campo `Reunion Room` del jefe; contenido hecho a mano). Al volver se abre el portal y el huevo vuela a su hueco del HUD. Campos del jefe: `Egg Id`, `Reunion Room`. Ver [HUD](../../Systems/HUD.md#el-reencuentro-hudeggreunion).
