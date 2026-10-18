# Dr. Cortez de Grecia

Origen: rama remota `origin/Grecia`, commit `d5c70bd` (agregar Dr Cortez con sprites, animación y movimiento 2D).

Se importan `Assets/Enemigos`, `Assets/Scripting/DrCortezAI.cs` y sus metadatos originales. No se fusionan cambios de otras ramas ni se publica nada.

El prefab original contiene un sprite provisional, la etiqueta Player y una linterna. La animación referencia una jerarquía por piezas que no está dentro de ese prefab. Se conserva el original y se construye `Assets/Prefabs/Enemigos/CortezVisual.prefab` con los sprites de cabeza, torso, dos piernas, brazo y linterna de Grecia.

La animación integrada deriva de sus curvas de caminar: conserva el movimiento de las piezas, normaliza el ciclo, limita los giros y omite las posiciones absolutas del montaje original. Hay una variante de subida con el brazo elevado. El personaje se orienta según el desplazamiento y deja de caminar cuando está quieto o en pausa.

Se sustituye el aspecto de los tres enemigos de Persecucion y el escalador de EscapeVertical. El movimiento, retrasos, contacto y reaparición conservan `EnemigoTemporal` y `RecuperarJugador`, para mantener la progresión existente. `DrCortezAI` se conserva como referencia y para futuras salas; no se activa simultáneamente con otro controlador de movimiento. El collider de los enemigos se adapta al cuerpo del personaje (0,65 × 1,5), con margen alrededor de cabeza y pies.

Integración repetible: `IntegrarCortez.Aplicar`, llamada al preparar las escenas de escape. Los originales de Grecia permanecen separados de los prefabs y clips adaptados.

Validación: 90 comprobaciones correctas en Unity, registradas en `Validacion-Cortez.txt`, y capturas revisadas en `Capturas/Cortez-persecucion.png` y `Capturas/Cortez-parkour.png`. Incluye recorrer la persecución con enemigos activos y reaparición por contacto con el escalador. La geometría de los saltos se comprueba por separado con el escalador desactivado. Se verificó por hash que los 38 archivos importados bajo Enemigos y Scripting coinciden con Grecia.

## Linternas y búsqueda

Los guardias rastrean a 0,65 unidades/s y corren a 3,15 al ver al jugador en un cono de 6 unidades y 64 grados. La luz cálida translúcida nace en la linterna y se recorta contra los colliders sólidos; el mismo cálculo comprueba la visión. Los fondos usan material sin luz, por lo que se dibuja un haz sobre la ilustración para que sea visible.

Al perder contacto visual persisten 1,2 segundos hacia la última posición conocida y después buscan allí hasta completar 4 segundos desde el último avistamiento. No siguen la posición oculta del jugador. Los patrulleros recorren su sector; el escalador rastrea despacio hacia arriba y acelera cuando ve al conejo. Tras reaparecer se borra la persecución y hay 2,5 segundos de margen sin captura.

La animación usa velocidad 0,38 durante rastreo y 0,8 en persecución, frente al máximo anterior de 1,6. Se detiene cuando el enemigo no avanza. Los originales de Grecia se conservan.

Validación de esta revisión: 98 comprobaciones correctas en Play Mode (`Validacion-Linternas.txt`), incluyendo visión frontal, exclusión detrás/fuera del haz, oclusión por pared, adquisición y pérdida del objetivo, reinicio, recorrido de persecución con enemigos activos, saltos del parkour y final. El conductor de la prueba aterriza en las cajas para encadenar saltos cuando hay un guardia al otro lado. Capturas revisadas: `Capturas/Cortez-linterna.png` y `Capturas/Cortez-linterna-parkour.png`. La copia temporal requirió restaurar su caché de paquetes; hubo un error del índice de búsqueda del editor antes de Play Mode, sin errores registrados durante la prueba del juego.
