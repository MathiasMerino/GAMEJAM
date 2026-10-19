# Escape completo — rama pruebaniveles

## Recorrido

EscenaInicial → PasilloPrincipal → A (fusible) y B (mantenimiento) → C (bucle) → regreso al pasillo → volver a interactuar con C → Persecucion → EscapeVertical → Valle → HabitacionFinal.

El mantenimiento conserva el orden confirmado: soporte de fusible, manija circular, botón rojo y recogida de la llave en la puerta derecha. La llave se conserva para C y para la puerta al exterior.

## Nuevas escenas

- Persecucion: tres módulos industriales, siete cajas que saltar, un perseguidor y dos vigilantes provisionales. Dos puntos de reaparición. El extremo derecho lleva al conducto.
- EscapeVertical: una única escena de 30 unidades de alto, 21 plataformas y tres puntos de reaparición. Colliders de plataforma de un solo sentido, cámara vertical y enemigo que asciende desde abajo. Se añadieron cinco descansos visuales donde el dibujo dejaba huecos demasiado grandes. La puerta superior se abre con E y requiere la llave maestra.
- Valle: recorrido desde el interior industrial hacia vegetación y luz natural. Acercarse a la zanahoria hace ceder la losa y caer al personaje; no se concede la zanahoria antes de la caída.
- HabitacionFinal: llegada por el techo, aterrizaje sobre la zona acolchada y cierre narrativo del experimento como mensaje discreto sobre el escenario. R inicia una partida nueva.

## Controles y recuperación

A/D o flechas para caminar, Espacio para saltar, E para interactuar; los mecanismos de mantenimiento admiten clic. Mantener Tab muestra el inventario. Una captura o una caída en persecución/parkour devuelve al último punto seguro conservando los objetos. El perseguidor se recoloca detrás o debajo del punto seguro y espera antes de volver a avanzar.

El estado dura durante la sesión; no hay guardado en disco. El jugador y los enemigos son provisionales, según lo acordado. El prefab JugadorTemporal conserva el controlador existente; los cuerpos y ojos de enemigos son hijos sustituibles. Los fondos de Pruebas se utilizan sin reemplazar los originales.

## Ajustes y construcción

EnemigoTemporal expone velocidad, modo escalador y patrulla. RecuperarJugador controla el límite de caída y PuntoControl fija la posición segura. CamaraNivel añade seguimiento vertical sin cambiar el horizontal existente. PrepararEscape crea únicamente escenas ausentes y registra las cuatro en Build Profiles. No es necesario ejecutarlo para jugar la entrega guardada.

ValidarNiveles ejecuta el recorrido en una copia aislada. La persecución se recorre con Input System y enemigos activos. Las comprobaciones de plataforma utilizan Input System y física real; durante esa parte se desactivan los enemigos para separar la geometría de los saltos del ritmo de persecución. El contacto y la reaparición se prueban por separado. No equivale a una evaluación humana de dificultad o diversión.

