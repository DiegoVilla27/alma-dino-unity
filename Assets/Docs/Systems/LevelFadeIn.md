# Entrada de los niveles (iris)

**Estado (11 de octubre de 2026):** activo en los 20 escenarios jugables (16 niveles y 4 jefes).

Cada nivel (`Level_*`) y jefe (`Boss_*`) empieza con un **cierre de iris** de dibujos animados, por encima de todo (HUD incluido):

1. La pantalla está en negro.
2. Se abre un **círculo pequeño sobre Alma**, con un pequeño rebote.
3. Se queda un instante enfocándola, "respirando".
4. El círculo **crece** hasta mostrar toda la pantalla.

Es automático: no hay que colocar nada en las escenas, y cualquier escena nueva con esos nombres lo tiene.

- **Script:** `Assets/Prefabs/Level/Progression/Scripts/LevelFadeIn.cs`. Se instala solo al arrancar el juego y actúa en cada carga de escena, también al pulsar Play con un nivel abierto.
- **Arranque:** se queda en negro hasta que la escena corre con fluidez (8 frames seguidos rápidos, como mucho 3 s), porque los primeros frames tras cargar o tras pulsar Play van a tirones.
- **Tiempos** (constantes): `Black` 0,3 s en negro, `Pop` 0,45 s abriendo, `HoldSmall` 0,45 s enfocando y `Expand` 0,8 s creciendo. Tamaño del círculo pequeño: `SmallRadius` 0,17 de la altura de pantalla.
- **Centro:** sigue a Alma (a la altura de su pecho) mientras la cámara se coloca. Otra escena puede elegir el punto con `LevelFadeIn.Focus`: el jefe 1 lo abre sobre el mono.
- **Dibujo:** un cuadrado negro con un agujero redondo suavizado y cuatro barras negras alrededor, en un Canvas por encima de todo.
- **Con el portal de salida:** el portal funde a negro y el nivel siguiente se abre con el iris, así que el cambio de nivel nunca es de golpe.
