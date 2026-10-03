# Alma — Player

Prefab: `Player_Alma.prefab`. Extraído del jugador configurado en Level_1_1, con origen local (0, 0, 0), referencias internas y assets compartidos conservados. Es la base reutilizable; las escenas existentes siguen usando sus jugadores actuales.

## Organización

- `Scripts/`: controlador, input, detector de suelo, animador, FSM y estados. Incluye el mismo assembly `AlmaDino.Features.Player` y sus GUIDs.
- `Sprites/`: los 58 PNG existentes (frames individuales, grids y strips de Idle, Run, Jump, Walk y Dead), con sus importadores conservados.
- `Configuration/AlmaPhysicsConfig.asset`: configuración vigente de física y habilidades.
- `Animations/README.md`: cobertura real de animaciones y secuencias pendientes.
- `Art/`: diseño, mecánicas y poses conceptuales creados con el generador integrado; no se aplican automáticamente a la animación.

## Mecánicas actuales

| Acción | Teclado | Comportamiento |
| --- | --- | --- |
| Movimiento | A/D o flechas | Velocidad máxima 7 m/s; aceleración y frenado existentes |
| Salto / Doble Salto | Espacio | Impulsos 8.2 / 7.6 m/s; mantener o soltar modifica altura |
| Dash aéreo | Shift izquierdo o derecho | Horizontal, 6 m en 0.2 s; cooldown 0.4 s; ignora viento, no veneno ni lava |
| Pisotón | S, flecha abajo o C | En el aire: anticipación 0.1 s y caída a 22 m/s |
| Rugido | E o F | Duración 0.25 s; radio base 3 m, medio ángulo 45°, resonancia hasta 8 m |

Mando y botones táctiles siguen usando el lector de entradas y VirtualInputBridge existentes. Habilidades acumulativas: Doble Salto en mundo 1, Pisotón en 2, Dash en 3 y Rugido en 4. El prefab parte con los desbloqueos apagados; la progresión y los altares los habilitan. Level_1_1 fuerza la introducción de habilidades.

Alma no tiene HP: el contacto con un peligro activo ejecuta muerte y reaparición en checkpoint. El juego no tiene audio.

## Colocación y edición

Arrastrar `Player_Alma.prefab` como raíz de la escena, colocar su posición de aparición y conectar la cámara y los mecanismos de esa escena a esta instancia. Mantener un solo jugador activo. Ajustar `_fallDeathY` al recorrido. El prefab no incluye cámara ni controles táctiles: estos son dependencias de la escena.

En `Visual`, SpriteRenderer expone el sprite. PlayerSpriteAnimator controla los arrays de frames y el renderer: no añadir PrefabSprite2D al renderer animado para evitar competir por su imagen. El collider, las referencias y la escala física no deben cambiar al sustituir arte. Los eventos, el input asset y el material físico compartidos permanecen en sus módulos.

Herramientas: `Alma > Prefabs > Create Alma prefab if missing` crea únicamente cuando falta; nunca sobrescribe el asset. `Validate Alma prefab` comprueba referencias, componentes y frames básicos.
