# Guardado y progreso

**Estado (6 de octubre de 2026):** implementados en Unity 6000.6.0f1 el guardado local en JSON (GDD 9.3) y el gestor de progreso por nivel. Comprobados con una prueba PlayMode en `Level_1_1`: sin partida, las habilidades empiezan bloqueadas; un altar las desbloquea y se guarda; un nido guarda el checkpoint y Alma reaparece en él.

Piezas relacionadas: [nido y altares](../LevelPieces/Progression.md). Jugador: [Alma](../Player/Alma.md).

## Archivos

| Archivo | Responsabilidad |
| --- | --- |
| `Assets/Systems/Save/SaveData.cs` | Datos guardados (clase serializable). |
| `Assets/Systems/Save/SaveSystem.cs` | Leer, escribir y borrar el archivo JSON. |
| `Assets/Systems/Progress/GameProgress.cs` | Gestor por nivel: carga la partida, aplica habilidades y checkpoint a Alma, recibe avisos de nidos y altares y autoguarda. También define `ICheckpoint`. |
| `Assets/Systems/Progress/System_GameProgress.prefab` | Objeto con `GameProgress`. **Uno en cada nivel.** |

Namespace `AlmaGame.Systems`, ensamblado `Assembly-CSharp`.

## Archivo de guardado

- **Formato:** JSON legible, con `JsonUtility`.
- **Ubicación:** `Application.persistentDataPath/alma_save.json`. En Mac, en el editor: `~/Library/Application Support/DefaultCompany/AlmaDino/alma_save.json`.
- **Escritura segura:** se escribe primero en `alma_save.json.tmp` y después sustituye al archivo, para que un cierre a mitad no deje un guardado roto.
- **Lectura:** si no existe, no se puede leer o es de una versión más nueva, se empieza partida nueva (con un aviso en consola si estaba dañado).

| Campo | Tipo | Contenido |
| --- | --- | --- |
| `Version` | int | Versión del formato (hoy 1). |
| `Level` | string | Nombre de la escena del nivel actual. |
| `HasCheckpoint`, `CheckpointX`, `CheckpointY` | bool, float | Último checkpoint de ese nivel (punto de reaparición). |
| `RescuedEggs` | lista de string | Huevos rescatados (`Green`, `Blue`, `Purple`, `Red`), añadidos por los [huevos](../LevelPieces/Progression.md#huevos-a-rescatar-resource_rescueegg_). |
| `CompletedLevels` | lista de string | Niveles terminados por el [portal de salida](../LevelPieces/Progression.md#portal-de-salida-resource_levelexitportal_universal). |
| `DoubleJump`, `GroundPound`, `Dash`, `Roar` | bool | Registro global de habilidades conseguidas alguna vez (no decide qué tiene Alma al empezar un nivel). |
| `LevelUnlocks` | lista de texto | Habilidades desbloqueadas en `Level` desde que se empezó desde el principio; se devuelven al reaparecer en un checkpoint de ese nivel. |
| `PlayTimeSeconds` | float | Tiempo jugado (tiempo real). |
| `SavedAtUtc` | string | Fecha y hora del último guardado (ISO 8601, UTC). |

**Cuándo se guarda:** al pasar por un nido, al desbloquear una habilidad, al rescatar un huevo, al terminar un nivel por el portal y al cerrar el juego (para el tiempo jugado).

## Gestor de progreso (`GameProgress`)

Al empezar el nivel:

1. Carga la partida (si `Load Save` está activo); si no hay, crea una nueva.
2. Si la partida es de **otro nivel**, olvida su checkpoint (no aplica aquí) y pasa a este nivel.
3. Calcula las **habilidades del nivel**: las iniciales (`Start With …`, lo que dan los niveles anteriores) más, solo si reanuda en un checkpoint de este nivel, las de `LevelUnlocks`. Si empieza el nivel desde el principio, vacía `LevelUnlocks`. Así un nivel siempre se juega como está diseñado: en el 1-1 no hay doble salto antes del altar, aunque la partida lo tenga registrado.
4. Aplica a Alma qué habilidades tiene (las demás quedan **bloqueadas**).
5. Si la partida tiene checkpoint de este nivel y `Resume At Saved Checkpoint` está activo, Alma aparece en él y ese nido se enciende.

Después recibe los avisos de los nidos (`CheckpointReached`), los altares (`UnlockAbility`), los huevos (`RescueEgg`, con `IsEggRescued` para consultarlo) y el portal (`CompleteLevel(siguiente)`: añade el nivel actual a `CompletedLevels`, pone `Level` en el siguiente y borra el checkpoint; `IsLevelCompleted` para consultarlo) y guarda.

| Campo | Valor | Uso |
| --- | ---: | --- |
| `Start With Double Jump` / `Ground Pound` / `Dash` / `Roar` | no | Habilidades disponibles desde el inicio de este nivel aunque no haya partida (por ejemplo, en el Mundo 2 se empezaría con el Doble Salto). |
| `Load Save` | sí | Leer la partida guardada al empezar. |
| `Resume At Saved Checkpoint` | sí | Empezar en el checkpoint guardado si es de este nivel. |

**Para probar desde cero** hay tres formas:
- Menú superior de Unity **AlmaDino → Borrar partida guardada** (`Assets/Systems/Editor/SaveMenu.cs`). Funciona en cualquier momento; si estás en Play, el cambio se nota al volver a darle a Play.
- En el Inspector de `System_GameProgress`, los tres puntos del componente → **Borrar partida guardada**.
- Desactivar `Load Save` en la escena: cada Play empieza sin partida, aunque se siga guardando.

### Importante al probar en el editor

- **Sin `System_GameProgress` en la escena**, Alma tiene **todas las habilidades desbloqueadas** y no se guarda nada (como hasta ahora). Con él, solo tiene las guardadas y las iniciales del nivel.
- La partida **se mantiene entre sesiones de Play**: si activaste un nido, la siguiente vez empiezas en él; si además desbloqueaste una habilidad en ese nivel, la conservas y su altar sale gastado. Si empiezas el nivel desde el principio (sin checkpoint guardado), vuelves a tener solo sus habilidades iniciales. Para empezar de cero usa **Borrar partida guardada** o desactiva `Load Save`.
- Debe haber un solo `GameProgress` por escena; si hay dos, el segundo se desactiva con un aviso.

## Reinicio al morir

Al reaparecer Alma (evento `AlmaMotor2D.Respawned`) cada elemento vuelve a su estado inicial, en su propio script:

| Elemento | Qué se restaura |
| --- | --- |
| [Planta carnívora](../Enemies/Plant_Carnivorous_Jungle.md) | Reposo (con su tiempo de descanso), sin onda ni aviso. |
| [Escarabajo de cristal](../Enemies/CrystalBeetle_Caves.md) | Posición inicial, de pie y caminando; se borran los cristales en vuelo. |
| [Murciélago de cueva](../Enemies/CaveBat_Caves.md) | Posición inicial, patrullando. |
| Gas tóxico ascendente, géiser, techo aplastante ([trampas](../LevelPieces/Hazards.md)) | Gas a su altura inicial; géiser sin bola y con el ciclo reiniciado; techo arriba, entero y listo. |
| Plataformas que se desmoronan, piso rompible, espora ([piezas](../LevelPieces/Pieces.md)) | Vuelven a estar enteras y disponibles. |

Los nidos, los altares y los huevos **no** se reinician: un checkpoint encendido, una habilidad desbloqueada y un huevo rescatado se conservan.

## Pendiente

- ~~Indicador de huevos en pantalla.~~ Hecho: ver [HUD](HUD.md).
- Menú de inicio que cargue el nivel guardado en `Level` (el portal ya pasa de un nivel al siguiente).
- Varias ranuras de partida, si se quieren.
