# Conejo de Grecia y parkour sin enemigo

Fuente: `origin/Grecia`, commit `6a62c18`. Se conserva completa y sin modificar la carpeta `Assets/Player`. El prefab de Grecia utiliza la variante de silueta con ojos claros; esa es la apariencia integrada, con sus seis piezas y clips originales de reposo, caminar, salto y caída.

Se crea `Assets/Prefabs/Pruebas/ConejoGrecia.prefab` a partir de VisualConejo, con un Animator adaptado a las cuatro fases de movimiento. VisualJugadorGrecia observa la velocidad y el suelo del Rigidbody2D que ya tenía el juego. No se añade ConejoPruebaMovimiento ni otro cuerpo físico. Las colisiones, salto, interacciones, inventario y puntos seguros siguen usando sus componentes existentes. La animación se pausa al leer notas; girar solo afecta al arte y no al collider. La caída de la zanahoria continúa animándose aunque se deshabilite el movimiento del jugador.

Se sustituye el dibujo provisional en las nueve escenas y en JugadorTemporal.prefab. El constructor `IntegrarJugadorGrecia.Aplicar` permite repetir la integración. El enemigo de EscapeVertical se elimina de la escena y de su constructor; IntegrarCortez solo modifica Persecucion. Se mantienen plataformas, saltos, cámaras y puntos seguros del parkour.

Validación: 109 comprobaciones correctas en Unity Play Mode (`Validacion-Jugador-Grecia.txt`), con dos reconstrucciones de escenas. Incluye seis piezas visibles, un solo cuerpo físico, reposo/caminar/giro/salto/caída/aterrizaje, pausa de animación, puzzles e inventario, cajas, persecución, ausencia total de enemigos en parkour, todos sus saltos y reaparición por caída, además de recoger y conservar la zanahoria hasta el final. Capturas revisadas en Docs/Capturas/Grecia-*.png.
