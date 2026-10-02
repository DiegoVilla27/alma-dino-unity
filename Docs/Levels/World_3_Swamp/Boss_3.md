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

## 3. 🧱 Mecánicas de Combate & Fases del Jefe (Puzle de Habilidad)

- **Estructura: 3 Impactos para Vencer**:
  - **Fase 1 (El Vendaval Huracanado):**
    - El pterodáctilo se suspende en el fondo de la pantalla y bate sus alas rítmicamente, generando ráfagas continuas que intentan empujar a Alma fuera de las plataformas hacia el abismo.
    - Alma debe usar el **Dash Aéreo** para contrarrestar la fuerza del viento y saltar entre las ramas evitando caer.
  - **Fase 2 (El Ataque en Picado / Dive Bomb):**
    - El pterodáctilo emite un graznido agudo, se eleva fuera de la pantalla y desciende en picado a toda velocidad en trayectoria diagonal o horizontal hacia una de las plataformas.
    - **El Contragolpe:** Alma debe saltar con el Doble Salto en el momento justo, calcular la altura de vuelo de la cabeza del jefe y ejecutar un **Dash Aéreo frontal directo hacia su cabeza**.
    - La inercia del Dash impacta su cresta ósea, interrumpe el ataque y lo derriba contra la rama durante unos segundos.
  - **Fase 3 (Plataformas Destruidas):**
    - Con cada impacto, el jefe destruye una de las ramas con sus garras, reduciendo el espacio de maniobra para el siguiente ciclo y aumentando la velocidad de su picado.
- **Condición de Derrota de Alma:**
  - Caer al abismo o ser arrollada por el cuerpo del pterodáctilo reinicia el ciclo actual.

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Wings over the Abyss" (Alas sobre el Abismo).
  - *Estilo:* Orquestación épica y vertiginosa: cuerdas en staccato furioso, metales triunfales y percusión militarizada rápida que imita el aleteo descomunal de una bestia voladora.
  - *Tempo:* 150 BPM. Sensación de combate aéreo al borde del abismo.
- **Efectos de Sonido (SFX):**
  - Batido de Alas: Ondas de viento de baja frecuencia (*WHUMP... WHUMP... WHUMP*).
  - Picado del Jefe: Silbido agudo sónico de avión en picada antes del impacto.
  - Impacto del Dash: Colisión sónica crujiente con vibración de pantalla (*¡WHACK!*).

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Jefe:** Pterodáctilo Alfa (pose de aleteo estático, animación de ráfaga, pose de picado aéreo, animación de aturdimiento y derrota).
- [ ] **Sprites Arena:** Ramas de sauce quebradizas que se rompen tras cada ciclo.
- [ ] **VFX:** Ráfagas de viento huracanadas; estela de viento sónico durante el picado del jefe; plumas y chispas al golpear la cabeza con el Dash.
- [ ] **Audio:** Pista "Wings over the Abyss", SFX de aleteo huracanado, graznido de picado.


## 6. Implementación jugable

- Escena `Assets/Scenes/World_3_Swamp/Boss_3.unity`, accesible desde el portal del 3-4 tras rescatar el Huevo Morado.
- Cuatro ramas: X=0 (ancho 6m, suelo Y=0), X=-6 y X=6 (ancho 4m, suelo Y=0.6), X=12 (ancho 4m, suelo Y=1.2). Todas se alcanzan con Doble Salto. La rama central permanece hasta la victoria.
- Cámara size **6**, seguimiento de Alma y anticipación horizontal **1.25m**. El abismo causa daño y devuelve a la rama central.
- **Vendaval:** 3s de viento horizontal a 12m/s², con 2s adicionales de preparación inicial. La dirección alterna tras cada impacto. Dash ignora el viento y conserva el peligro por contacto con el cuerpo.
- **Aviso:** 1.4s. Fija la altura del ataque a 3.5m sobre la rama más próxima a Alma. Una línea cian señala esa altura y el texto indica la dirección del Dash de contraataque. La trayectoria queda fijada durante el aviso.
- **Picado horizontal:** velocidad 7, 8 y 9m/s según los impactos. Salto → Doble Salto → Dash frontal contra la cabeza cian. Un salto corriente, Pisotón, Dash en el suelo o ataque en la dirección del vuelo no dañan al jefe.
- **Impacto:** cancela el Dash, rebota a Alma, recarga habilidades aéreas, muestra el contador y activa sacudida, pausa de 0.05s, partículas y sonido provisional. El jefe cae aturdido durante una recuperación de 2.5s.
- Tras cada impacto se destruye la rama lateral activa más alejada de Alma, evitando retirar el apoyo desde el que acaba de contraatacar. Después del tercer golpe queda la rama central.
- Caer o tocar al jefe reinicia el ciclo actual; conserva los golpes acertados y las ramas ya destruidas. Un picado esquivado vuelve al vendaval sin sumar impactos.
- Tres impactos completan y guardan Mundo 3, desactivan el jefe y abren el portal hacia `Level_4_1`. Esa escena todavía no existe; el portal muestra el cierre del mundo mientras se implementa Mundo 4.
- Constructor: `Tools → Alma → Construir Jefe 3 - Pterodáctilo Alfa`. Acceso: `Alma → 📂 Cargar Arena Jefe 3`.

### Validación

59 pruebas EditMode y 8 pruebas Boss3 PlayMode: fases y rechazos de ataques, habilidades iniciales, cuatro ramas alcanzables con entradas reales, daño por cabeza/cuerpo, rechazo de Dash terrestre y de salto simple con Dash, carga desde el portal del 3-4 y tres contraataques reales desde ambos lados con persistencia tras una muerte y activación del portal final.

El arte geométrico, las partículas y el sonido son provisionales; los assets finales de producción siguen pendientes.
