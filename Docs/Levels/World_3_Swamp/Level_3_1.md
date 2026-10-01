# 🗺️ Nivel 3-1: "Los Fangales Tóxicos"
> **Mundo 3: Pantano de Viento y Niebla** | **Función Pedagógica:** Introducir (Despertar del Dash Aéreo & Fosos Infranqueables)

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Tras emerger del inframundo de cristal, Alma llega a un lodazal infinito donde la niebla es tan espesa que apenas deja ver el siguiente paso. El aire huele a turba podrida y azufre vegetal. Ante ella se extienden pantanos burbujeantes de lodo ácido y fosos kilométricos que ni el salto más alto con aleteo materno puede cruzar.
- **Estado Emocional de Alma:** Impaciencia sofocante. La niebla y el lodo intentan ralentizarla, pero su determinación necesita ganar velocidad horizontal; necesita cortar el aire como una flecha.
- **El Despertar de la Habilidad (La Espora del Viento Ancestral):**
  - Alma encuentra una flor ancestral flotante en el centro de un islote marchito:
  - *Texto en Pantalla:*
    > *"¡HABILIDAD DESPERTADA: DASH AÉREO!*  
    > *Tus alas se afilan con la velocidad del vendaval.*  
    > *En el aire, pulsa DASH para impulsarte horizontalmente, congelar la gravedad y cruzar abismos más anchos."*
- **Pistas Narrativas en el Entorno:** Juncos doblados en la dirección del viento, huellas de simio que cruzan troncos flotantes y plumas moradas que flotan en el agua estancada.

---

## 2. 🎨 Dirección de Arte & Atmósfera Visual

- **Temática Visual:** Un manglar tenebroso y melancólico. Árboles con raíces zancudas sumergidas en aguas verdeazuladas estancadas, niebla volumétrica que fluye de izquierda a derecha y juncos que se doblan ante la brisa húmeda.
- **Paleta de Color Principal:**
  - Agua y Lodo Tóxico: Verde ciénaga oscuro (`#1E2D24`) y limo sulfuroso (`#2D4A3E`).
  - Niebla y Atmósfera: Gris perla azulado (`#8D99AE`) con veladuras semitransparentes.
  - Acento del Dash Aéreo: Blanco cian relampagueante (`#CAF0F8`) en las plumas de Alma.
- **Fondos Parallax (4 Capas):**
  - *Capa 0 (Fondo Lejano):* Siluetas fantasmales de sauces colosales perdidos en un horizonte blanco de niebla.
  - *Capa 1 (Fondo Medio):* Capas de niebla densa que se desplazan a diferentes velocidades.
  - *Capa 2 (Fondo Cercano / Gameplay):* Islotes de turba, troncos flotantes inestables, lodo burbujeante.
  - *Capa 3 (Primer Plano / Foreground):* Espigas de juncos y gotas de condensación desenfocadas que cruzan la vista.
- **Iluminación 2D (URP):**
  - Iluminación plana y difusa de día nublado (intensidad 0.70) con contrastes suaves de sombras.

---

## 3. 🧱 Mecánicas, Comportamiento de Bloques & Obstáculos

- **Habilidades Habilitadas:**
  - Movimiento base + Doble Salto + Pisotón Sísmico + **Dash Aéreo (`AirDash`)** desbloqueado en el altar del nivel.
- **Catálogo de Bloques y Mecánicas:**
  - *El Foso de Lodo Tóxico (Tutorial Orgánico de Distancia):*
    - Un abismo de 11 metros de longitud sobre un lago ácido.
    - A velocidad máxima y entre superficies a la misma altura, el doble salto cubre aproximadamente 7.6–7.9 m sin ayudas de borde. La distancia útil se verifica con colisionadores y desnivel.
    - La combinación obligatoria: **Salto ➔ Aleteo (Doble Salto) cerca del ápice ➔ Dash Aéreo cerca del final del segundo ascenso** añade 6 m al recorrido durante 0.2 s; el hueco de 11 m deja margen para aterrizar sin exigir el máximo teórico.
  - *Mecánica del Dash Aéreo:*
    - Desplaza a Alma 6 metros horizontales en `0.2 segundos`.
    - Congela la velocidad en el eje Y a cero durante esos 0.2s, permitiendo mantener la altura exacta en el aire.
    - Cooldown de 0.4s y una carga aérea, recuperada al aterrizar o tocar una espora. La dirección se fija al iniciar el dash; no se invierte durante el impulso.
- **Peligros:**
  - Aguas sulfurosas (daño por contacto y respawn instantáneo).

---

## 4. 🎵 Dirección de Sonido & Música (Audio Design)

- **Banda Sonora (BGM):**
  - *Título sugerido:* "Through the Sunken Mists" (A Través de las Nieblas Hundidas).
  - *Estilo:* Paisaje sonoro minimalista con flauta de bambú grave (shakuhachi), cuerdas tenues con sordina y grabaciones de campo de ranas y viento distante.
  - *Tempo:* 70 BPM. Sensación de avance cauteloso y misterio.
- **Efectos de Sonido (SFX):**
  - Dash Aéreo: Ráfaga sónica rápida y cortante (*¡FWOOOOSH!*) acompañada por el brillo de plumas.
  - Lodo Tóxico: Burbujeo viscoso constante en el fondo (*bloop-blop*).

---

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Tileset:** Terreno de turba con musgo pantanoso, troncos flotantes de manglar, juncos de agua.
- [ ] **Props:** Flor del Viento Ancestral, capas de niebla semitransparente animada para parallax.
- [ ] **Sprites Alma:** Animación de Dash Aéreo con estela de partículas y *smear frames* de velocidad.
- [ ] **VFX:** Ráfaga cian de viento detrás de Alma durante el Dash; burbujas de gas estallando en el foso tóxico.
- [ ] **Audio:** Pista "Through the Sunken Mists", SFX de dash aéreo, SFX ambiental de pantano.

---

## 6. Estado del prototipo y validación

- Escena implementada: `Assets/Scenes/World_3_Swamp/Level_3_1.unity`. Entrada desde `Boss_2`; salida preparada para `Level_3_2`, todavía pendiente de construcción.
- Abrir con **Alma → 📂 Cargar Nivel 3-1**. Regenerar con **Tools → Alma → Construir Nivel 3-1 - Los Fangales Tóxicos**.
- Pared sólida de raíces en X=-5.5 (8m de altura), cerrando el extremo detrás del punto de aparición para impedir caídas por el inicio.
- Isla inicial segura desde X=-6 hasta X=14; Espora del Viento en X=7, antes del primer abismo. Al comenzar una partida limpia, Alma conserva Doble Salto y Pisotón, pero debe recoger la espora para desbloquear Dash.
- Cuatro fosos de 11m: X=14–25, 33–44, 52–63 y 71–82. Las islas de práctica tienen 8m de anchura y la final 12m; sus superficies están en Y=0.
- Checkpoints estables en X=10, 29 y 67. Morir en el lodo devuelve al último nido sin perder el Dash desbloqueado. Aterrizar repone la carga aérea; sigue vigente el cooldown de 0.4s.
- Cámara ortográfica size 6, seguimiento de Alma y anticipo horizontal de 1.25 unidades. Cuatro capas de parallax, siluetas de sauces, bancos de niebla y juncos geométricos; arte y audio final pendientes.
- `Level3_1PlayTests`: entrada y destino correctos, imposibilidad de cruzar el foso tutorial con Doble Salto solo, conservación del Dash al morir y recorrido completo con entradas reales sin muertes. Las cuatro pruebas pasan, incluida la protección del extremo de entrada al caminar y usar Doble Salto.
