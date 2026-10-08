> **Ficha de diseño para reconstrucción:** existe `Assets/Scenes/World_03/Level_3_4.unity` como escena de trabajo (copia de la de práctica) con su fondo [`Parallax_Level_3_4`](../../LevelPieces/Backgrounds.md) (dos capas: el pantano visto desde lo alto con los volcanes rojos que se revelan al subir, y la copa del sauce con una abertura central); el nivel completo aún no está construido. Las notas sobre escenas, pruebas o assets existentes describen el prototipo retirado. Las referencias de cámara a **8** son el criterio de la reconstrucción, no resultados de aquellas pruebas; sus valores de seguimiento están en [Alma](../../Player/Alma.md#cámara-daño-y-feedback). No habrá audio.

# 🗺️ Nivel 3-4: "El Sauce Ancestral"
> **Mundo 3: Pantano de Viento y Niebla** | **Función Pedagógica:** Evaluar + Tercer Rescate (La Subida del Gas & Huevo Morado)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** En el centro del pantano se alza el Sauce Ancestral, un coloso milenario cuyas raíces descienden a las aguas más profundas y cuyas ramas altas tocan las corrientes de aire libre. Sin embargo, el pantano comienza a hervir: una marea tóxica de gas verde pantanoso asciende inexorablemente desde las aguas. En lo más alto de la copa, protegido en un nido de musgo, descansa el tercer huevo robado: el **Huevo Morado**, cuyo cascarón ya comienza a vibrar tenuemente.
- **Estado Emocional de Alma:** Desesperación contra el reloj. El gas presiona al avanzar, pero los checkpoints permiten detenerse y preparar el siguiente tramo.
- **El Momento del Rescate (Cinemática Diegética en Gameplay):**
  - Alma alcanza la rama cumbre del Sauce, dejando atrás el gas tóxico.
  - Al posar su hocico sobre el **Huevo Morado**, la tensión se evapora; el piano de la "Nana Maternal" regresa con una calidez conmovedora:
  - *Texto del Rescate (Huevo Morado):*
    > *"El cascarón tiembla...*  
    > *Falta muy poco para que rompas a cantar.*  
    > *Solo nos falta uno.*  
    > *(Tres de cuatro rescatados)"*
  - Un chillido ensordecedor rompe la niebla sobre el sauce: una silueta alada masiva desciende batiendo alas gigantescas: el **Pterodáctilo Alfa** ataca la copa del árbol.
- **Pistas Narrativas en el Entorno:** Hojas de sauce arrancadas, humo tóxico verde que asciende desde la parte inferior de la pantalla, y el canto apagado de la cría desde el interior del huevo morado.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un ascenso entre ramas entre ramas de sauce llorón y lianas movedizas mientras una densa nube de gas verde radiactivo asciende desde el fondo de la pantalla. Al superar las ramas altas, el cielo gris se abre mostrando las cimas de las montañas volcánicas rojas a lo lejos.
- **Paleta de Color Principal:**
  - Gas Venenoso Ascendente: Verde ácido fosforescente (`#38B000`) con volutas de humo amarillento.
  - Hojas de Sauce: Verde grisáceo melancólico (`#6B705C`) y madera pálida (`#B7B7A4`).
  - Huevo Morado: Amatista cósmica brillante (`#9D4EDD`) con vetas luminiscentes.
- **Iluminación 2D (URP):**
  - Contraste entre la luz verde tóxica que sube desde abajo y la luz dorada pura que espera en la cima del árbol.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + Dash Aéreo.
- **Objetivo de Diseño: El Examen Maestro del Mundo 3**:
  1. *El Muro de Gas Tóxico Ascendente (`RisingHazardFloor2D`):*
     - El prototipo usa `0.9 m/s`, calibrados para los saltos reales, con `3s` de aviso antes de subir. Se activa por secciones al salir de los refugios.
     - Si Alma es alcanzada por el gas, sufre daño y reaparece en el último checkpoint del tramo.
     - Impide quedarse inmóvil en el recorrido. Los checkpoints detienen el gas y morir restaura un margen de 4m por debajo del suelo del tramo.
  2. *Esporas Horizontales entre Ramas:*
     - Dos esporas alineadas a Y=10.2 sobre un lago de 16m. Solo hay que avanzar y encadenar Dash; la altura se gana saltando entre ramas sólidas.
  3. *Balancines de Raíces Activados con Pisotón:*
     - Se reutiliza el balancín: Pisotón en el extremo izquierdo y correr al derecho genera un impulso. El primero es un atajo opcional; el segundo es necesario para llegar a la copa. Mantener Salto aumenta el impulso; Doble Salto ayuda a corregir la subida.
- **Puntos de Control:**
  - *Checkpoint 1 (`X = 40.0, Y = 7.5`):* Primer tercio del ascenso.
  - *Checkpoint 2 (`X = 84.0, Y = 12.0`):* Antes de la carrera final hacia la copa del Sauce.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Props:** Ramas colgantes de sauce llorón, nido de musgo morado, Huevo Morado bioluminiscente.
- [ ] **VFX:** Capa de gas tóxico verde animada con partículas de humo y burbujas ascendentes.


---

## 6. Recorrido del prototipo

- Escena `Assets/Scenes/World_3_Swamp/Level_3_4.unity`, conectada desde 3-3 y registrada en Build Settings.
- Menús **Alma → 📂 Cargar Nivel 3-4** y **Tools → Alma → Construir Nivel 3-4 - El Sauce Ancestral**.
- Cámara size 8, seguimiento vertical y anticipo horizontal; pared detrás del inicio. Doble Salto, Pisotón y Dash disponibles; Rugido bloqueado.

### Secciones

1. **Raíces y ramas:** entrada segura hasta X=12, balancín opcional en X=8. Ramas de 4m centradas en X=16/22/28/34, con incrementos de altura de 1.5m, hasta el checkpoint X=40, suelo Y=7.5.
2. **Lago y cañas:** lago X=44–60, esporas (48, 10.2) y (54, 10.2), llegada a la misma altura. Barrera de Dash X=64 sobre suelo firme. Ramas X=72/Y=9 y X=78/Y=10.5; checkpoint X=84/Y=12.
3. **Impulso a la copa:** balancín X=92, sobre suelo Y=12. Pisotón en X=89.7 y correr hacia X=94.3. La rama alta empieza en X=98, suelo Y=19.5; el doble salto corriente no alcanza esa altura. Nido y Huevo Morado en X=118; portal a `Boss_3` en X=123.

### Enemigos y cobertura

- Tres sapos reutilizan `PoisonToad2D`: X=18/Y=2.15, X=65.5/Y=8.15 y X=114.5/Y=20.15. Cada disparo apunta al lado donde esté Alma en ese momento, incluso si cambia de lado durante el aviso.
- Cada disparo avisa con `!` durante 0.6s, con cadencia de 2s. Burbujas a 8m/s y daño por contacto; Dash no concede inmunidad al veneno.
- Se pueden saltar sus cuerpos al cambiar de rama. Una raíz en X=14.4 protege el balancín inicial de los disparos del primer sapo; se supera con Doble Salto o con el lanzamiento del balancín. Antes del nido, una raíz en X=110.5 intercepta burbujas mientras preparas el salto final. Doble Salto + Dash permiten pasar sobre raíz y sapo hacia el huevo. Los checkpoints conservan su seguridad.
- Morir limpia los proyectiles y reinicia los ciclos de aviso.

> **Implementado** ([ficha](../../Enemies/PoisonToad_Swamp.md)): a diferencia de este plan, el sapo **fija** el lado al empezar el aviso (no cambia si Alma cruza durante él), el aviso es un parpadeo rojo con la papada hinchada (no un `!`) y la detección es de 8 m, no 18. Gravedad del glob: 6 u/s².

### Gas y rescate

- `RisingHazardFloor2D` mueve un volumen de gas con física cinemática; `RisingGasCycle` controla aviso, subida y reinicio. Datos en `RisingGasConfig.asset`.
- Secciones activadas en X=12, 44 y 88. Aviso de 3s, subida de 0.9m/s y margen inicial de 4m bajo el suelo. Llegar al siguiente refugio deja el gas inactivo; el nido queda a salvo.
- El gas vuelve a activarse en cada intento, incluso si el Huevo Morado ya figura rescatado en el guardado.
- El gas hace daño por contacto, incluido durante Dash. Morir restablece la sección del checkpoint, las esporas y ambos balancines.
- Rescate mediante contacto real con el Huevo Morado: persiste, abre el portal, detiene el gas y revela la silueta del Pterodáctilo Alfa. No completa Mundo 3 hasta derrotar al jefe.
- El tercer huevo usa `EggType.PurpleEgg = 3`; se conserva lectura de partidas antiguas que lo llamaban `YellowEgg`.
- `Boss_3` ya es jugable: el portal carga la arena del Pterodáctilo Alfa. Arte y VFX siguen pendientes.


### Validación

**13 pruebas PlayMode pasan**: dirección de disparo al cambiar Alma de lado durante el aviso, ascenso físico y visual del gas con el huevo ya guardado, disparo real y cobertura ante tiros repetidos, entrada segura y pared posterior, aviso y daño real del gas, pausa/reinicio en checkpoint, fallo del salto corriente hacia la copa, lanzamiento final con Pisotón, atajo opcional, persistencia del rescate y recorrido completo con entradas reales sin muertes.

La suite completa EditMode incluye el ciclo del gas, el baseline de habilidades del 3-4 y compatibilidad de las partidas del tercer huevo.
