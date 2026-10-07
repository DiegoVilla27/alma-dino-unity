> **Ficha de diseño para reconstrucción:** esta escena aún no existe en el proyecto actual. Las notas sobre escenas, pruebas o assets existentes describen el prototipo retirado. Las referencias de cámara a **8** son el criterio de la reconstrucción, no resultados de aquellas pruebas; sus valores de seguimiento están en [Alma](../../Player/Alma.md#cámara-daño-y-feedback). No habrá audio.

# 👑 Jefe 2: "El Acorazado Subterráneo — Armadillo Prehistórico"
> **Mundo 2: Cuevas de Cristal** | **Arena de Combate y Cierre del Mundo 2**

---

## 1. 📖 Sinopsis Narrativa & Contexto Emocional

- **Momento Narrativo:** Con dos de sus cuatro huevos a salvo en su lomo, Alma intenta salir de las profundidades de la cueva, pero un coloso acorazado de placas óseas y cristales incrustados bloquea el túnel: el **Armadillo Prehistórico**. La criatura protege la entrada al túnel superior que conduce a los pantanos.
- **Estado Emocional de Alma:** Firmeza inquebrantable. Con dos vidas confiadas a su cuidado, Alma no retrocederá un solo milímetro ante la mole de piedra.
- **Textos en Pantalla / Banners:**
  - *Inicio del Combate:*
    > *"¡JEFE DE MUNDO: ARMADILLO PREHISTÓRICO!*  
    > *Esquiva sus embestidas giratorias con tu aleteo. Cuando quede aturdido contra los cristales, ¡aplica tu Pisotón desde las alturas!"*
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

## 3. 🧱 Mecánicas de Combate & Fases del Jefe (Puzle de Habilidad)

- **Estructura: 3 Impactos para Vencer**:
  - **Fase 1 (La Bola Rodante de Demolición):**
    - El armadillo se enrolla en una bola impenetrable y rueda a toda velocidad por el suelo inferior, rebotando de pared a pared.
    - Alma debe saltar sobre él usando el **Doble Salto** en el momento justo o refugiarse en las plataformas del nivel superior.
  - **Fase 2 (El Choque y Aturdimiento):**
    - En el tercer rebote, el armadillo impacta con fuerza contra uno de los pilares de cristal gigantes. El impacto lo desorienta y queda mareado en el suelo durante **`4.5 segundos`**, exponiendo la fisura superior de su caparazón.
  - **Fase 3 (El Contraataque Sísmico de Alma):**
    - Alma debe saltar desde la plataforma del nivel superior y ejecutar un **Pisotón Sísmico** directo sobre su fisura para infligir daño.
  - **Evolución por Ciclos:**
    - *Impacto 1:* Rueda a velocidad normal.
    - *Impacto 2:* Rueda más rápido y provoca la caída de estalactitas del techo que Alma debe esquivar en el aire.
    - *Impacto 3:* Realiza un salto en forma de bola que roza las plataformas superiores antes de estrellarse contra el pilar.
- **Condición de Derrota de Alma:**
  - Ser atropellada por la bola de demolición o golpeada por una estalactita reinicia la fase actual.

---

## 4. Comunicación visual

El nivel no tendrá música ni efectos de sonido. Señalizar amenazas, habilidades y cambios de estado mediante animación, formas, luces y texto breve cuando haga falta.

## 5. 📋 Checklist de Assets para Producción a Futuro

- [ ] **Sprites Jefe:** Armadillo Prehistórico (pose de reto, animación de enrollarse en bola giratoria, animación de aturdimiento con pajaritos/estrellas de cristal, animación de derrota).
- [ ] **Sprites Arena:** Pilares de cristal reforzados (con estados de agrietamiento tras los impactos).
- [ ] **VFX:** Chispas saliendo del caparazón al rozar el suelo; estalactitas cayendo con estela de polvo; destello de rotura de cristal al acertar el Pisotón.

## Estado de implementación

Arena jugable en `Assets/Scenes/World_2_Caves/Boss_2.unity`, enlazada desde 2-4 e incluida en Build Settings. Configuración editable en `ArmadilloBossConfig.asset`. Tres impactos contra pilares exponen la coronilla durante 4.5 segundos; solo un Pisotón directo inflige daño. Los saltos normales y ondas cercanas no dañan al jefe. Morir reinicia el ciclo actual y elimina las estalactitas, conservando los impactos acertados. El segundo ciclo incorpora cristales con aviso amarillo en el suelo y el tercero un salto rodante. La victoria registra el Mundo 2 y habilita la salida prevista hacia `Level_3_1`, aún pendiente de construcción. Visuales geométricos provisionales; sprites finales pendientes.

Validación: 42 pruebas EditMode y 2 pruebas PlayMode del Boss 2 superadas. Las pruebas físicas comprueban el daño por Pisotón directo, rechazo de caída normal y reinicio por muerte.

Para reconstruir la arena: cámara ortográfica fija de tamaño 8, seguimiento de Alma y anticipo horizontal de 4,3 unidades. La cámara no debe encuadrar automáticamente toda la arena; sus límites pertenecerán al jefe y no se heredarán de 2-4.

Validación histórica del prototipo retirado: once escenas tenían seguimiento y se probaron distintas relaciones de aspecto. Estos resultados no verifican la cámara de tamaño 8 de la reconstrucción.

Acceso y contraataque corregidos: los refugios se centran en X = -5 y +5, con ancho 3 y superficie a Y = 2.15 (antes 2.65). El boss se aturde en X = -8 o +8, fuera de las plataformas; queda libre toda la vertical de la coronilla. Sube al refugio con doble salto, salta hacia el pilar y pulsa POUND al estar encima del jefe. Cuatro pruebas PlayMode superadas, incluidas subida desde suelo y Pisotón desde ambos refugios con controles reales.
