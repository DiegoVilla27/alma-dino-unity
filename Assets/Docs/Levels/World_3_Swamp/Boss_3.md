> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_03/Boss_3.unity` como escena de trabajo con el fondo de la arena ([`Backdrop_Boss_3`](../../LevelPieces/Backgrounds.md#jefe-3--world_3backdrop_boss_3prefab): la copa del Sauce Ancestral sobre la niebla, con un vendaval que crece hasta huracán y el horizonte volcánico encendiéndose con las fases); la arena y el jefe aún no están construidos. El prototipo anterior se retiró; el jefe se rehace sobre la base de los Jefes 1 y 2 para reutilizar su estructura. Las referencias de cámara a **8** son el criterio de la reconstrucción. No habrá audio.

# 👑 Jefe 3: "El Señor de las Ráfagas — Pterodáctilo Alfa"
> **Mundo 3: Pantano de Viento y Niebla** | **Arena de Combate y Cierre del Mundo 3**

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En la copa más alta del Sauce Ancestral, por encima de la niebla del pantano, el cielo ruge. El gobernante de los aires pantanosos desciende para recuperar a las crías: el **Pterodáctilo Alfa**, un reptil volador colosal con una envergadura descomunal, cresta afilada como un sable y alas capaces de generar vendavales con un solo batido.
- **Estado Emocional de Alma:** Coraje aéreo. Alma no tiene alas para volar, pero sus saltos y su Dash cortarán el cielo para defender a sus tres pequeños.
- **Textos en Pantalla / Banners:**
  - *Inicio del Combate:*
    > *"¡JEFE DE MUNDO: PTERODÁCTILO ALFA!*  
    > *Resiste sus vendavales en las ramas inestables. Cuando se lance en picado, ¡ejecuta tu Dash Aéreo directo a su cabeza!"*
  - *Victoria:*
    > *"El rey de los cielos grazna derrotado y huye hacia el horizonte humeante.*  
    > *Las cenizas en el aire indican el final del camino: la Cima Volcánica.*  
    > *Allí aguarda el Rey Ladrón... y tu último hijo.*  
    > *(Mundo 3: Pantano de Viento y Niebla Completado)"*

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **La Arena de Combate:**
  - Cuatro ramas inestables dispuestas en semicírculo sobre el abismo del pantano. El viento azota continuamente la arena.
  - Al fondo, las nubes del pantano se tiñen de un rojo fuego tenue proveniente del horizonte volcánico lejano.
- **Paleta de Color Principal:**
  - Pterodáctilo: Membranas de ala púrpura oscuro (`#3C096C`), cuerpo escamoso gris verdoso y cresta cian fosforescente.
  - Viento y Vendaval: Ráfagas blancas y grises que cruzan la pantalla de lado a lado.
  - Ramas de la Arena: Madera musgosa suspendida sobre la bruma.

---

## 3. 🧱 Diseño del combate (acordado el 11/10/2026)

Es el dueño del cielo y Alma no vuela. Ataca con **viento** (para tirarla de las ramas al abismo) y con **picados** que cruzan la pantalla. Es el examen del **Dash Aéreo**: en el vendaval sirve para aguantar (ignora el empuje) y en el picado Alma sube con el Doble Salto y le hace un **Dash de frente a la cresta cian**, la única forma de dañarlo. Diseño: alas de membrana púrpura oscuro, cuerpo verde grisáceo escamoso y cresta cian con manchas (su punto débil).

### Arena

- Cuatro ramas de sauce sobre un abismo de niebla: una central ancha y tres laterales a distintas alturas, alcanzables con Doble Salto.
- Zona de muerte en el abismo.
- Fondo `Backdrop_Boss_3` (ya existe) con su viento `GustWind2D`, que se refuerza con las fases.

### Secuencia

1. **Intro:** Alma quieta; la cámara lo enfoca posado en una rama. Abre las alas, grazna, dice sus frases y alza el vuelo. Tarjeta de jefe.
2. **Vendaval:** al fondo de la pantalla, bate las alas con fuerza; cada aletazo lanza una ráfaga que empuja a Alma hacia el borde, anunciada por líneas de viento. Alma aguanta tras una rama alta, saltando entre ramas o con Dash contra el viento.
3. **Aviso de picado:** grazna con la cresta brillando y sale de la pantalla. Una **línea violeta (color del Dash)** cruza la pantalla a la altura por la que pasará (sobre la rama más cercana a Alma), y `HudBossAlert` grita «¡PICADO!» y señala con la flecha del borde **por qué lado** entra.
4. **Picado:** cruza la pantalla en horizontal a toda velocidad por esa línea. Su cuerpo mata. **Contraataque:** salto → Doble Salto → **Dash de frente** (contra su dirección de vuelo) a la cabeza. Salto normal, Pisotón o Dash desde el suelo no le hacen nada. Si Alma solo lo esquiva, vuelve al vendaval.
5. **Golpe:** destello rojo, temblor, explosión de plumas y rebote de Alma (recarga sus habilidades del aire). Cae dando vueltas sobre una rama lateral, **la rompe con las garras** (cae al abismo) y vuelve a subir: **fase siguiente**. Rompe siempre la rama lateral más lejana de Alma.
6. **Derrota (3.er golpe):** se posa en la rama central; Alma desaparece; la mira, dice sus frases de derrota y se va volando en una nube de plumas. Aparece el **portal al Mundo 4** (`Level_4_1`).

Si Alma muere (abismo o cuerpo del jefe), reaparece en la rama central y sigue la misma fase; se conservan los golpes y las ramas rotas.

### Fases (3 vidas, una por fase; cada fase un 15 % más rápida)

| Fase | Ramas | Vendaval | Picado |
| --- | --- | --- | --- |
| 1 | 4 | 4 s, ráfagas de un solo lado | 1 picado, velocidad 9 |
| 2 | 3 | Más fuerte; el viento **cambia de lado** a mitad | 1 picado, velocidad 10,35 |
| 3 | 2 (central y una lateral) | Más fuerte todavía | **2 picados seguidos**, uno desde cada lado (cualquiera vale para golpearlo); velocidad 11,9 |

### Reutilizado

Estructura de jefe (intro, una vida por fase, destello rojo, derrota con portal y Alma oculta), `HudBossAlert` (aviso de picado), `NinjaSmoke2D` con colores del pantano, `GustWind2D` (viento del fondo), zona de muerte.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 🎞️ Animaciones

Frames de 256×256 en hojas de 1024×512 (8 frames), fieles a la celda 03 de `Assets/Art/Bosses/Bosses_ConceptSheet_v1.png`.

| Animación | Origen | Uso | Estado |
| --- | --- | --- | --- |
| **Taunt** | Higgsfield | Posado en la rama, abre las alas y grazna. Intro; su frame 1 para la derrota. | Hecha (11/10/2026) |
| **Fly** (aleteo) | Higgsfield | Bucle de aleteo de lado: volar y vendaval (más rápido). | Hecha (11/10/2026) |
| **Dive** (picado) | Higgsfield | Alas plegadas atrás, pico y cresta por delante. Se refleja según el lado. | Hecha (11/10/2026) |
| Ráfagas, línea de aviso, plumas, rama rompiéndose, caída en espiral | Código | Efectos. | Pendiente |

No necesita animación de aturdido: el golpe es en el aire y la caída en espiral se hace girando el sprite.

### Taunt (hecha)

`Assets/Prefabs/Bosses/Pterodactyl/`:

| Archivo | Qué es |
| --- | --- |
| `Animations/Taunt/Boss_Pterodactyl_Taunt_Sheet.png` | Hoja de 1024×512, 8 frames de 256×256, 45 px por unidad, apoyo en las patas (0,5; 4/256). Una generación de Higgsfield con la celda 03 de la hoja de conceptos; recortada por código con la misma escala en todos los frames y las patas en el mismo punto. |
| `Animations/Taunt/Boss_Pterodactyl_Taunt.anim` | Clip a 12 fps en bucle. |
| `Animations/Taunt/Boss_Pterodactyl.controller` | Animator del jefe; por ahora solo `Boss_Pterodactyl_Taunt`. |
| `Boss_Pterodactyl_Taunt_Preview.prefab` | Para verla en una escena. |

Frames: 1 posado de perfil con las alas plegadas · 2–4 levanta la cabeza y abre las alas (se gira a tres cuartos) · 5 alas abiertas al máximo, grazna con la cresta brillando · 6 aletazo hacia delante · 7–8 pliega las alas y vuelve a la pose inicial. Con las alas plegadas ocupa ~140 px de ancho (la escala la marca el frame 5); en el juego se puede ampliar el sprite.

### Fly (hecha)

| Archivo | Qué es |
| --- | --- |
| `Animations/Fly/Boss_Pterodactyl_Fly_Sheet.png` | Hoja de 1024×512, 8 frames de 256×256, 45 px por unidad, **pivote en el centro** (vuela). Segunda generación de Higgsfield: la primera dibujaba una tercera pata saliendo del ala, y esta se pidió con exactamente dos patas. Recortada por código con la misma escala y la **cresta fija** en el mismo punto de todos los frames (en la celda, en (158, 118) px), para que el cuerpo no salte. |
| `Animations/Fly/Boss_Pterodactyl_Fly.anim` | Clip a 12 fps en bucle. |
| `Boss_Pterodactyl.controller` | Estado `Boss_Pterodactyl_Fly`, junto al Taunt. |
| `Animations/Fly/Boss_Pterodactyl_Fly_Preview.controller` + `Boss_Pterodactyl_Fly_Preview.prefab` | Vista previa. |

Ciclo: 1–3 alas arriba bajando poco a poco · 4 rectas · 5 abajo (las patas quedan tras el ala) · 6–7 las sube · 8 vuelve arriba. Solo dos patas, juntas bajo el vientre.

### Dive (hecha)

| Archivo | Qué es |
| --- | --- |
| `Animations/Dive/Boss_Pterodactyl_Dive_Sheet.png` | Hoja de 1024×512, 8 frames de 256×256, 45 px por unidad, pivote en el centro. Una generación de Higgsfield con el concepto y el Fly de referencia; cresta fija en el mismo punto (en la celda, en (169, 107) px) y escalada para que la cresta mida lo mismo que en el Fly (no cambia de tamaño al lanzarse). |
| `Animations/Dive/Boss_Pterodactyl_Dive.anim` | Clip a 12 fps en bucle. |
| `Boss_Pterodactyl.controller` | Estado `Boss_Pterodactyl_Dive`. |
| `Animations/Dive/Boss_Pterodactyl_Dive_Preview.controller` + `Boss_Pterodactyl_Dive_Preview.prefab` | Vista previa. |

Picado horizontal recto, de perfil hacia la derecha: cuello estirado, alas plegadas hacia atrás como un dardo, dos patas recogidas. En bucle tiemblan las puntas de las alas y el pico se abre y cierra; la silueta casi no cambia.

Con esto están las tres animaciones de Higgsfield; el resto se hace por código.

## 6. 📋 Checklist

- [x] Animación Taunt.
- [x] Animación Fly.
- [x] Animación Dive.
- [x] Arena: cuatro ramas, abismo, viento.
- [x] Controlador del jefe `PterodactylBoss2D`.
- [x] Portal al Mundo 4 (`Level_4_1`).
- [ ] Arte de rama rota (ahora la rama cae y se desvanece) y marcas de progreso en el HUD.

## Implementación (11/10/2026)

`Assets/Prefabs/Bosses/Pterodactyl/Boss_Pterodactyl_Swamp.prefab` (`Scripts/PterodactylBoss2D.cs`), colocado en `Boss_3` en (0, 0). Misma estructura que los Jefes 1 y 2.

### Arena (`Boss_3`, dentro de `Level`)

| Objeto | Pieza | Posición (centro x; parte de arriba) | Ancho |
| --- | --- | --- | --- |
| `Branch_Center` | `Platform_Floating_Mangrove_Swamp` | 0; −5,37 (donde aparece Alma) | 7,7 |
| `Branch_Left` | `Platform_Floating_Mangrove_Swamp` | −8,4; −3,6 | 3,8 |
| `Branch_Left (1)` (rama derecha) | `Platform_Floating_Mangrove_Swamp` | 8; −3,6 | 3,8 |
| `Branch_Left (2)` (rama alta derecha) | `Platform_Floating_Mangrove_Swamp` | 13,6; −1,72 | 3,8 |
| `Abyss_DeathZone` | `Trap_DeathZone_Universal` | 0; −13 | 80 × 3 |

- El suelo que había (`Platform_Ground_Peat_Swamp`) está **desactivado**, no borrado: la arena es un abismo.
- `Side Branches` del jefe = `Branch_Left`, `Branch_Left (1)` y `Branch_Left (2)`; las casillas vacías se ignoran y se posa en la que esté **más a la derecha**.
- Las ramas usan las plataformas flotantes de manglar del kit del Mundo 3 (arte real). Su colisionador se ajustó a mano al ancho, porque `ApplySize` solo estira el dibujo.
- `ExitPortal_World4` (`Next Scene` = `Level_4_1`), inactivo hasta la derrota (**debe quedar inactivo en la escena**: si está activo, Alma lo toca al empezar y pasa al Mundo 4); aparece en (0; −3,41).
- `System_GameProgress`: Alma empieza con **Doble Salto, Pisotón y Dash**.
- **Arreglo en `Backdrop_Boss_3`:** la zona de viento `Gale` había perdido su tamaño y medía 6 × 3 alrededor del centro, así que nunca empujaba a Alma. Ahora mide **46 × 26** (toda la arena), sin trazos propios (`Streaks Per Unit` 0), y su colisionador también.

### Combate

1. **Intro** (~16 s): posado en `Branch_HighRight` mirando a Alma; frases (globo anclado a `Bubble Height` 3,8 sobre sus pies, a la altura de la cabeza y no del borde del fotograma), Taunt, tarjeta de jefe; despega.
2. **Vendaval** (`Gale Time` 4 s): vuela sobre la arena (`Hover Point` (0; 2,5), vaivén ±3) batiendo las alas rápido. El jefe controla el `GustWind2D` del fondo: **solo sopla durante el vendaval**, a rachas (calma 0,6 s). En las fases 2 y 3 el viento cambia de lado a mitad; y cambia de lado tras cada golpe. Alma quieta acaba en el abismo; caminando contra el viento aguanta.
3. **Aviso:** grazna con la cresta parpadeando en violeta y sale por arriba. Una **línea violeta** parpadeante (el color del Dash, `Accent` (0,8; 0,5; 1), también usado en el nombre del globo, la tarjeta y la alerta) cruza la pantalla a `Dive Height` (2,4) sobre la rama más cercana a Alma; `HudBossAlert` grita «¡PICADO!» y su flecha señala por dónde entra.
4. **Picado:** cruza la pantalla en horizontal por la línea. **Golpe:** Dash de Alma **en sentido contrario a su vuelo** tocando la cresta (círculo de radio 0,85 en `Head Offset` (1,6; 0,2)). Cualquier otro contacto con la cabeza o el cuerpo (caja 3 × 0,9) mata. Alma de pie en la rama bajo la línea es alcanzada.
5. **Tras el golpe:** destello rojo, temblor, humo del pantano, rebote de Alma (11) y recarga de sus habilidades del aire. Cae en espiral sobre la rama lateral activa **más lejana de Alma**; la rama pierde el colisionador, cae y se desvanece. Vuelve a volar: fase siguiente.
6. **Derrota (3.er golpe):** cae sobre `Branch_Center`. Alma desaparece y queda **congelada** (sin física: no puede caer al abismo ni reaparecer durante el discurso). Frases de derrota, se va en una nube de plumas y aparece el portal; **en ese momento** Alma reaparece con humo **a la izquierda del portal** (`Alma Portal Gap` 0,35 entre ella y el portal, medido con los bordes de su colisionador) sobre la rama, y detrás de ella se crea una **pared invisible** (`ExitWall`, 0,4 × 10): solo puede caminar a la derecha, hacia el portal.

Posado (intro, caída tras un golpe y derrota), sus pies se hunden `Perch Sink` (0,12) en la rama para que no parezca flotar.

**Si Alma muere** (abismo o jefe): vuelve volando al vendaval de la misma fase; se conservan golpes y ramas rotas.

### Fases (`Phases`)

| Fase | Vendaval | Viento | Cambia de lado a mitad | Picado | Picados seguidos | Línea antes de entrar |
| --- | --- | --- | --- | --- | --- | --- |
| 1 | 4 s | 10 | no | 9 | 1 | 1,3 s |
| 2 | 4 s | 13 | sí | 10,35 | 1 | 1,15 s |
| 3 | 4 s | 16 | sí | 11,9 | 2 (uno desde cada lado) | 1 s |

### Pruebas (PlayMode, `Boss_3`)

- Combate completo: la intro termina; el vendaval empuja a Alma (quieta cae al abismo); el aviso muestra la línea y la alerta; de pie bajo la línea el picado la mata; tres Dash contra la cresta lo derrotan (en la fase 3 con dos picados); cada golpe rompe una rama (quedan 2); aparece el portal en (0; −3,41); Alma no se ve hasta que aparece el portal, sigue viva, aparece a la izquierda sin tocarlo, la pared la frena hacia la izquierda y caminando a la derecha entra en el portal.
- Contra el viento: en las tres fases, caminando contra el viento cuando empuja, Alma se mantiene en la rama.


## Huevo: «El Reencuentro» (11-12/10/2026)

Cuando termina su discurso y desaparece o muere (en el jefe final, cuando la estalactita queda en el suelo), un destello lleva a la sala del **Huevo Morado**: el prefab `Prefabs/Level/Progression/EggRooms/EggRoom_World3_Purple` (campo `Reunion Room` del jefe; contenido hecho a mano). Al volver se abre el portal y el huevo vuela a su hueco del HUD. Campos del jefe: `Egg Id`, `Reunion Room`. Ver [HUD](../../Systems/HUD.md#el-reencuentro-hudeggreunion).
