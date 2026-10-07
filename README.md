# Alma: Mother's Roar

Juego de plataformas 2D en reconstrucción con **Unity 6000.6.0f1**. El diseño abarca cuatro mundos, 16 niveles y cuatro jefes. En esta rama solo existe una escena jugable de práctica, [`Level_1_1`](Assets/Scenes/World_01/Level_1_1.unity). Las veinte fichas de niveles y jefes describen el juego por construir; incluso `Level_1_1` aún no es un nivel completo. Sus notas de implementación corresponden al prototipo retirado.

## Estado actual

- **Alma:** locomoción, salto variable y doble salto, Pisotón, Dash aéreo, Rugido, muerte y reaparición. Tiene siete animaciones (Idle, Run, Jump, Fall, GroundPound, Dash y Roar), efectos visuales, cámara de seguimiento con **tamaño ortográfico fijo 8** y límites por escena. Su configuración está en [`AlmaMovement.asset`](Assets/Prefabs/Player/Configuration/AlmaMovement.asset).
- **Enemigos:** prefabs reutilizables de planta carnívora, escarabajo de cristal, murciélago de cueva, sapo venenoso y salamandra de magma, con sprites, animaciones y comportamiento propios.
- **Piezas de nivel:** doce trampas (incluida la zona de muerte invisible), diez piezas de plataformas y recursos, la roca de basalto y el conjunto del balancín. También existen un nido de checkpoint, cuatro altares, cuatro huevos y un portal de salida. Las variantes visibles tienen arte integrado; la corriente de viento usa partículas.
- **Progreso:** guardado local en JSON para habilidades, checkpoints, huevos y niveles completados. `Level_1_1` tiene además un fondo y primer plano con parallax de jungla.

Abre [`Level_1_1`](Assets/Scenes/World_01/Level_1_1.unity) y pulsa Play. Muévete con **A/D o flechas**, salta con **Espacio**, haz Pisotón en el aire con **S, ↓ o C**, Dash aéreo con **Shift** y Rugido con **E o F**. La escena sirve para probar a Alma, la cámara y el parallax; los enemigos y demás piezas están disponibles como prefabs para colocarlos en niveles futuros. El proyecto no usa música ni efectos de sonido.

## Documentación

| Documento | Contenido |
| --- | --- |
| [GDD](Assets/Docs/GDD.md) | Visión, historia, progresión y reglas generales. |
| [Inventario](Assets/Docs/INVENTARIO_GAMEPLAY_PREFABS.md) | Elementos previstos, descartados e implementados, con sus nombres y rutas. |
| [Alma](Assets/Docs/Player/Alma.md) | Controles, física, habilidades, animaciones y cámara actuales. |
| [Enemigos](Assets/Docs/Enemies/) | Una ficha por enemigo implementado. |
| [Piezas de nivel](Assets/Docs/LevelPieces/) | Trampas, plataformas, recursos, puzles y progresión. |
| [Guardado](Assets/Docs/Systems/SaveAndProgress.md) | Estado persistente y conexiones con el jugador. |
| [Niveles y jefes](Assets/Docs/Levels/) | Veinte fichas de diseño; sus menciones de escenas retiradas son históricas. |
| [Arte conceptual](Assets/Art/README.md) | Referencias visuales, distintas de los sprites integrados en `Assets/Prefabs/`. |

El GDD fija la dirección general; las fichas de implementación y los archivos de Unity describen el comportamiento actual. Las cifras del prototipo retirado sirven como referencia de diseño y requieren validación al reconstruir los niveles.
