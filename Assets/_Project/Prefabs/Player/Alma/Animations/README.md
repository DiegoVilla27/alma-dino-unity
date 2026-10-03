# Animaciones de Alma

El reproductor vigente es `../Scripts/Components/PlayerSpriteAnimator.cs`. Utiliza arrays de sprites, no un Animator Controller ni clips .anim. Los frames reales están en `../Sprites`; las láminas de `../Art` son conceptos y no spritesheets listos para cortar.

| Estado | Assets existentes | Reproducción actual del prefab |
| --- | --- | --- |
| Idle | 10 frames | Conectados, 8 fps |
| Run | 8 frames | Conectados, 10 fps |
| Jump | 12 frames | Conectados, 12 fps |
| Fall | Jump 08 y 09 | Conectados, 10 fps |
| DoubleJump | Comparte Jump | Conectado; ciclo propio pendiente |
| Walk | 10 frames | Disponibles; no hay estado Walk en la FSM |
| Dead | 8 frames | Disponibles; no se reproducen en la muerte/reaparición inmediata |
| GroundPound | Guía de pose | Secuencia propia pendiente; usa fallback Idle |
| Dash | Guía de pose | Secuencia propia pendiente; usa fallback Idle |
| Roar | Guía de pose | Secuencia propia pendiente; usa fallback Idle |
| Hurt / Respawn | Guía de pose | Secuencia e integración pendientes |
| Rescue | Guía de pose | Secuencia e integración pendientes |

Los PNG Grid y Strip son composiciones de referencia existentes; los arrays usan los frames individuales. Para crear ciclos nuevos: conservar tamaño de lienzo, pivote, escala y silueta; mantener los pies en una línea de apoyo común; separar anticipación, acción y recuperación. La animación debe seguir la FSM y sus tiempos, sin introducir retrasos en las entradas.

La guía visual sugiere poses, no garantiza ciclos animados. Integrar nuevos estados visuales requiere trabajo posterior en PlayerSpriteAnimator; las mecánicas actuales se mantienen.
