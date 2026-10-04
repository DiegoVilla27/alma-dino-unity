# Alma: Mother's Roar — documentación de reconstrucción

Proyecto Unity 6000.6.0f1 en reconstrucción. Incluye la documentación, el arte conceptual y la primera base jugable de Alma: movimiento, salto y animaciones Idle/Run/Jump/Fall.

Para probarla, abre `Assets/Scenes/World_01/Level_1_1.unity` y pulsa Play. Muévete con **A/D o flechas** y salta con **Espacio**; mantenerlo produce un salto más alto. La escena contiene suelo y plataformas de práctica. La configuración se ajusta en `Assets/Prefabs/Player/Configuration/AlmaMovement.asset`.

## Documentos que usamos

| Documento | Para qué sirve |
| --- | --- |
| [GDD](Assets/Docs/GDD.md) | Visión del juego, historia, mundos y reglas generales. |
| [Inventario](Assets/Docs/INVENTARIO_GAMEPLAY_PREFABS.md) | 82 elementos previstos: función, comportamiento y nombre futuro de prefab. |
| [Alma](Assets/Docs/Player/Alma.md) | Implementación actual y especificación de controles, mecánicas, física, animaciones, cámara y feedback. |
| [Niveles](Assets/Docs/Levels/) | Una ficha por nivel o jefe; 20 fichas agrupadas por mundo. |
| [Arte conceptual](Assets/Art/README.md) | Índice de las 12 láminas del inventario. |

El GDD fija la dirección general. El inventario define qué objetos habrá. La ficha de Alma define cómo juega el personaje. Las fichas de niveles describen dónde y por qué se combinan esos elementos. Las imágenes ayudan a visualizar el aspecto, pero no son sprites de producción.

Los números y notas de implementación anteriores son referencias del prototipo retirado; deben validarse al reconstruir el juego. **No habrá música ni efectos de sonido.** Los avisos y acciones se comunicarán visualmente.
