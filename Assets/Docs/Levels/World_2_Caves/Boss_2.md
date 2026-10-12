> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_02/Boss_2.unity` como escena de trabajo con el fondo de la arena ([`Backdrop_Boss_2`](../../LevelPieces/Backgrounds.md#jefe-2--world_2backdrop_boss_2prefab): cámara de amatista que se agrieta y derrumba con los ciclos del combate hasta que entra la luz del pantano); la arena y el jefe aún no están construidos. El prototipo anterior (`ArmadilloBossConfig`) se retiró; el jefe se rehace sobre la base del Jefe 1 (`GiantMonkeyBoss2D`) para reutilizar su estructura. Las referencias de cámara a **8** son el criterio de la reconstrucción. No habrá audio.

# 👑 Jefe 2: "El Acorazado Subterráneo — Armadillo Prehistórico"
> **Mundo 2: Cuevas de Cristal** | **Arena de Combate y Cierre del Mundo 2**

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Con dos de sus cuatro huevos a salvo en su lomo, Alma intenta salir de las profundidades de la cueva, pero un coloso acorazado de placas óseas y cristales incrustados bloquea el túnel: el **Armadillo Prehistórico**. La criatura protege la entrada al túnel superior que conduce a los pantanos.
- **Estado Emocional de Alma:** Firmeza inquebrantable. Con dos vidas confiadas a su cuidado, Alma no retrocederá un solo milímetro ante la mole de piedra.
- **Textos en Pantalla / Banners:**
  - *Inicio del Combate:*
    > *"¡JEFE DE MUNDO: ARMADILLO PREHISTÓRICO!*  
    > *Esquiva sus embestidas giratorias con tu aleteo. Cuando quede mareado contra los cristales, ¡aplica tu Pisotón desde las alturas!"*
  - *Victoria:*
    > *"El gran acorazado cede el paso y se retira a las sombras.*  
    > *Una corriente de aire húmedo y frío anuncia la superficie: el Pantano de Viento te aguarda.*  
    > *(Mundo 2: Cuevas de Cristal Completado)"*

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **La Arena de Combate:**
  - Un túnel ovalado cerrado de dos niveles:
    - *Nivel Inferior:* Suelo llano de piedra dura flanqueado a izquierda y derecha por dos pilares de cristal gigante reforzado.
    - *Nivel Superior:* Dos plataformas elevadas de piedra a las que Alma puede subir con su Doble Salto.
- **Paleta de Color Principal:**
  - Caparazón del Jefe: Placas de hueso y granito gris pizarra (`#22223B`) con incrustaciones de cuarzo cian.
  - Punto Débil (Vértice de la Coronilla): Cristal agrietado brillante que se expone solo al quedar aturdido.
  - Pilares de Impacto: Geodas gigantes de amatista (`#9A031E` / `#5F0F40`).

---

## 3. 🧱 Diseño del combate (acordado el 11/10/2026)

Es un tanque: mientras rueda es invulnerable y letal. Solo se le daña cuando se estrella contra un pilar de cristal y queda **mareado**. Es el examen del **Pisotón**, la habilidad del Mundo 2: Alma sube a un refugio alto con el Doble Salto y cae con un Pisotón sobre la grieta brillante de su coronilla. Sigue el mismo esquema que el [Jefe 1](../World_1_Jungle/Boss_1.md) para reutilizar su código y sus efectos.

### Arena

- Suelo llano de piedra.
- Dos pilares de amatista a los lados que hacen de paredes; se agrietan un poco más con cada choque.
- Dos refugios altos de piedra en X = −5 y +5 (ancho 3), alcanzables con Doble Salto.
- Techo con estalactitas.

### Secuencia

1. **Intro:** Alma no puede moverse; la cámara lo enfoca. Se alza, le brillan los cristales, dice sus frases y se enrolla. Aparece la tarjeta de jefe.
2. **Aviso:** antes de cada ataque planta las patas y rasca el suelo con chispas; los cristales se iluminan hacia el lado al que va a rodar. Es la lectura del ataque sin sonido.
3. **Rodar:** enrollado en bola cruza el suelo rebotando de pilar a pilar. Tocar la bola mata. Se esquiva con un Doble Salto a tiempo o subiendo a un refugio.
4. **Choque:** tras los rebotes de la fase se estrella contra un pilar: esquirlas de cristal, temblor de cámara y grieta del pilar. Queda **mareado**, desenrollado, con estrellas de cristal girando y la grieta de la coronilla brillando. Mareado es inofensivo.
5. **Golpe:** solo el **Pisotón** sobre la grieta le hace daño; un salto normal encima rebota sin dañarlo. Al recibirlo parpadea en rojo y tiembla; después **se entierra** (polvo y piedras, el equivalente cueva del humo ninja del Mono) y sale del suelo en el centro: **empieza la fase siguiente**.
6. **Sin golpe a tiempo:** se sacude, se entierra y repite la misma fase.
7. **Derrota (3.er golpe):** como el Mono. Alma desaparece de la pantalla, la cámara lo enfoca, mira a Alma, dice su frase de derrota y se entierra. Aparece el **portal al Mundo 3** (`Level_3_1`) y Alma recupera el control.

Si Alma muere (bola o estalactita), reaparece y la fase sigue: se conservan los golpes y se borran las estalactitas.

### Fases (3 vidas, una por fase; cada fase un 15 % más rápida)

| Fase | Ataque | Rebotes antes del choque | Mareado |
| --- | --- | --- | --- |
| 1 | Solo rueda por el suelo | 2 | 4,5 s |
| 2 | Rueda más rápido; cada rebote contra un pilar hace caer 2–3 estalactitas, avisadas con una sombra en el suelo | 3 | 3,8 s |
| 3 | Además, a veces se agacha (aviso) y da un **salto en bola** en arco que roza los refugios (dejan de ser del todo seguros); siguen las estalactitas | 3–4 | 3,2 s |

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 🎞️ Animaciones

Frames de 256×256 en hojas de 1024×512 (8 frames), estilo cartoon plano fiel a `Assets/Art/Bosses/Bosses_ConceptSheet_v1.png` (celda 02). Mismo criterio que el Mono: Higgsfield solo para lo que necesita dibujo nuevo; el resto por código.

| Animación | Origen | Uso | Estado |
| --- | --- | --- | --- |
| **Taunt** (reto) | Higgsfield | Intro: se alza sobre las patas traseras y golpea el suelo. Su frame 1 se usa en la derrota. | Hecha (11/10/2026) |
| **Curl** (enrollarse) | Higgsfield | De pie a bola; al revés, desenrollarse. El último frame es la bola. | Hecha (11/10/2026) |
| **Stunned** (mareado) | Higgsfield | Desenrollado y aturdido, con la grieta de la coronilla brillando. | Hecha (11/10/2026) |
| Rodar | Código | Frame de la bola girando, con chispas y polvo. | Pendiente |
| Aviso, choque, golpe, enterrarse | Código | Rascar el suelo, aplastarse al chocar, destello rojo, temblor, polvo y piedras. | Pendiente |
| Estalactitas, esquirlas, estrellas | Código | Efectos. | Pendiente |

Opcional más adelante: caminar o excavar.

### Taunt (hecha)

`Assets/Prefabs/Bosses/Armadillo/`:

| Archivo | Qué es |
| --- | --- |
| `Animations/Taunt/Boss_Armadillo_Taunt_Sheet.png` | Hoja de 1024×512, 8 frames de 256×256, 45 px por unidad, apoyo en los pies (0,5; 4/256). Una generación de Higgsfield (`gpt_image_2_5`) con la celda 02 de la hoja de conceptos como referencia, recortada por código: frames separados por su forma, misma escala y pata trasera anclada en el mismo punto. |
| `Animations/Taunt/Boss_Armadillo_Taunt.anim` | Clip a 12 fps en bucle. |
| `Animations/Taunt/Boss_Armadillo.controller` | Animator del jefe; por ahora solo el estado `Boss_Armadillo_Taunt`. Las siguientes animaciones se añadirán aquí, como en el Mono. |
| `Boss_Armadillo_Taunt_Preview.prefab` | Para verla en una escena. |

Frames: 1 a cuatro patas (como el concepto) · 2–4 levanta el hocico, carga el peso atrás y se alza rugiendo · 5 en lo alto, de frente a la cámara · 6–7 baja y golpea con las garras · 8 cierra el bucle. El golpe del frame 7 se reforzará en el juego con temblor y polvo.

### Curl (hecha)

| Archivo | Qué es |
| --- | --- |
| `Animations/Curl/Boss_Armadillo_Curl_Sheet.png` | Hoja de 1024×512, 8 frames de 256×256, 45 px por unidad, apoyo abajo (0,5; 4/256). Una generación de Higgsfield con el concepto y la hoja del Taunt de referencia; escalada para que el frame 1 mida lo mismo que en el Taunt. Los frames 4 y 5 de la generación se intercambiaron para que el cierre sea continuo. |
| `Animations/Curl/Boss_Armadillo_Curl.anim` | Clip a 12 fps, **sin bucle**: hacia delante se enrolla; al revés (velocidad −1) se desenrolla. |
| `Boss_Armadillo.controller` | Estado `Boss_Armadillo_Curl`, junto al Taunt. |
| `Animations/Curl/Boss_Armadillo_Curl_Preview.controller` + `Boss_Armadillo_Curl_Preview.prefab` | Vista previa: se enrolla y se desenrolla en bucle. |

Frames: 1 a cuatro patas · 2–3 baja la cabeza, arquea el lomo y mete las patas · 4–5 asoman garras y oreja, la cola le envuelve · 6–8 se cierra. El **frame 8 es una bola redonda**: es el sprite que se gira por código al rodar.

### Stunned (hecha)

| Archivo | Qué es |
| --- | --- |
| `Animations/Stunned/Boss_Armadillo_Stunned_Sheet.png` | Hoja de 1024×512, 8 frames de 256×256, 45 px por unidad, apoyo abajo (0,5; 4/256). Una generación de Higgsfield con el concepto y la hoja del Taunt de referencia; misma escala que el Curl y la punta de la cola anclada en el mismo sitio. |
| `Animations/Stunned/Boss_Armadillo_Stunned.anim` | Clip a 12 fps en bucle. |
| `Boss_Armadillo.controller` | Estado `Boss_Armadillo_Stunned`. |
| `Animations/Stunned/Boss_Armadillo_Stunned_Preview.controller` + `Boss_Armadillo_Stunned_Preview.prefab` | Vista previa. |

Tirado boca abajo con las patas abiertas, lengua fuera y ojos en espiral. Arriba del caparazón, **un cristal grande partido con una grieta brillante**: el punto débil del Pisotón. El caparazón sube y baja al respirar (el frame 4 es el más alto, así que la altura de la grieta cambia por frame, como la cabeza del Mono agotado) y la grieta parpadea.

Con esto están las tres animaciones de Higgsfield; el resto se hace por código.

## 6. 📋 Checklist

- [x] Animación Taunt.
- [x] Animación Curl.
- [x] Animación Stunned.
- [x] Arena (montada por el usuario): suelo, bloques laterales, bloques flotantes, estalactitas.
- [x] Controlador del jefe `ArmadilloBoss2D` (intro, aviso, rodar, saltos a los bloques, choque, mareado, Pisotón, enterrarse, derrota).
- [x] Estalactitas al chocar con las paredes y contra el campeo en los bloques flotantes.
- [x] Portal al Mundo 3 (`Level_3_1`).
- [ ] Marcas de progreso en el HUD (pendiente también en el Jefe 1).

## Implementación (11/10/2026)

`Assets/Prefabs/Bosses/Armadillo/Boss_Armadillo_Caves.prefab` (`Scripts/ArmadilloBoss2D.cs`), colocado en `Boss_2` en (0, 0). Misma estructura que el Mono: intro con frases y tarjeta, una vida por fase, destello rojo al golpe, derrota con frases y portal.

### Arena (valores del componente, medidos en la escena)

| Elemento | Valor |
| --- | --- |
| Suelo (`Floor Y`) | −1,52 |
| Paredes (`Wall Left X` / `Wall Right X`) | −13,73 / 13,73: cara interior de `BlockLeft` y `BlockRight`. **Se les añadió un `BoxCollider2D`** (no tenían y Alma los atravesaba). |
| Bloques flotantes (`Blocks`: borde izq., borde der., arriba) | (−9, −5, 1,33) y (5, 9, 1,33) |
| Techo de las estalactitas de campeo (`Ceiling Y`) | 13,6 |
| Estalactitas de la escena | Las `Trap_Stalactite_Caves` colocadas en el nivel; el jefe las pone en manual (solo caen al chocar él contra una pared). |

**Tamaño (`Scale` 0,62):** la bola mide 2,33 de diámetro, así que pasa rodando por debajo de los bloques flotantes (hueco de 2,56). Más grande no cabría.

### Combate

1. **Intro** (unos 15 s): Alma quieta, cámara sobre él, dos frases, Taunt con golpe al suelo (polvo y temblor), tarjeta de jefe.
2. **Aviso:** mirando a Alma hace el Taunt (se alza y golpea el suelo), luego se enrolla (Curl).
3. **Rodar** hacia Alma, de pared a pared. En cada cruce puede **saltar a uno de los bloques flotantes**, rodar por encima y caer por el otro extremo (`Hop Chance`). La bola mata al tocarla: zona letal de radio 0,91 (`Ball Hit Scale` 0,78 de la bola dibujada; las puntas de cristal del borde no matan), comparada con la cápsula real de Alma y no con su rectángulo. Mientras avisa y se enrolla, su cuerpo mata en una caja de 2 × 1,35 desde los pies (`Body Hit Size`). Ajustado el 11/10/2026: antes era 1,07 contra el rectángulo de Alma y mataba "medio acercándose".
4. **Choque con una pared:** temblor de cámara, polvo y caen estalactitas de la escena (`Stalactites` por fase). Al llegar a los choques de la fase, se queda contra esa pared, se desenrolla y queda **mareado** con estrellas de cristal.
5. **Pisotón** sobre la grieta: rebote de Alma (13,5), destello rojo y temblor; **se entierra** y sale del suelo lejos de Alma: **fase siguiente**. Un salto normal encima solo hace rebotar a Alma (9) sin dañarlo.
6. Si se acaba el mareo sin golpe, se entierra y repite la fase.
7. **Derrota (3.er golpe):** Alma desaparece, frases de derrota, se entierra y aparece `ExitPortal_World3` (`Next Scene` = `Level_3_1`) en (`Portal X`, `Portal Y`) = (0; 0,5).

**Contra el campeo:** si Alma está de pie en un bloque flotante durante el combate, cada `Camp Interval` (2 s) cae una estalactita sobre ella (instancia de `Trap_Stalactite_Caves` que se borra al romperse). Al bajarse, la cuenta vuelve a empezar.

**Efectos:** el polvo al enterrarse, salir del suelo, golpear el suelo y chocar con las paredes es el humo ninja con los colores de las cuevas: humo negro y astillas violetas de amatista (ver [Humo ninja](../../LevelPieces/Tutorial.md#humo-ninja-ninjasmoke2d)).

**Si Alma muere:** se borran las estalactitas de campeo; el jefe se entierra y vuelve para la **misma fase**. Se conservan los golpes **y la cuenta de choques contra la pared** (corregido el 11/10/2026: antes volvía a cero, y en la fase 3, que pide 4 choques con la bola más rápida, un jugador que moría antes del 4.º no lo veía marearse nunca). Si Alma muere mientras él ya está mareado, su siguiente intento cuenta desde cero. Si se le acaba el mareo sin golpe, también.

**Aviso de mareo:** al marearse aparece el grito «¡MAREADO!» y, si está fuera de cámara, una insignia en el borde que apunta hacia él con el tiempo que queda ([`HudBossAlert`](../../Systems/HUD.md#aviso-de-jefe-vulnerable-hudbossalert)).

Alma empieza con Doble Salto y Pisotón: `System_GameProgress` de la escena tiene `Start With Double Jump` y **`Start With Ground Pound`** activados (el jefe también los activa, pero `GameProgress` es quien manda al empezar).

### Fases (`Phases`; solo el rodar es más rápido)

| Fase | Velocidad | Choques hasta marearse | Mareado | Salto a bloque | Estalactitas por choque |
| --- | --- | --- | --- | --- | --- |
| 1 | 9 | 2 | 4,5 s | 35 % | 1 |
| 2 | 10,35 (+15 %) | 3 | 3,8 s | 45 % | 2 |
| 3 | 11,9 (+15 %) | 4 | 3,2 s | 55 % | 3 |

### Pruebas (PlayMode, `Boss_2`)

Fase 3 con Alma muriendo tras cada choque: se marea al 4.º choque acumulado (3 muertes). Combate completo, repetido: la intro termina y devuelve el control; se marea tras 2, 3 y 4 choques; salta a los bloques; quedarse en un bloque trae una estalactita que mata a Alma, y el jefe vuelve en la misma fase; un salto normal no le hace daño; tres Pisotones lo derrotan y aparece el portal.


## Huevo: «El Reencuentro» (11-12/10/2026)

Cuando termina su discurso y desaparece o muere (en el jefe final, cuando la estalactita queda en el suelo), un destello lleva a la sala del **Huevo Azul**: el prefab `Prefabs/Level/Progression/EggRooms/EggRoom_World2_Blue` (campo `Reunion Room` del jefe; contenido hecho a mano). Al volver se abre el portal y el huevo vuela a su hueco del HUD. Campos del jefe: `Egg Id`, `Reunion Room`. Ver [HUD](../../Systems/HUD.md#el-reencuentro-hudeggreunion).
