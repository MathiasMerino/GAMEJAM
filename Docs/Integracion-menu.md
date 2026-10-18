# Integración del menú de dev

Integrado origin/dev (d6f769f, PR #2) mediante avance directo de pruebaniveles, preservando los cambios locales de niveles. No se realizó push.

Se conserva la escena del menú de dev y su conexión serializada BotonJugar → ControladorMenu.Jugar. El método local Jugar inicia EstadoPartida.NuevaPartida, limpia inventario y progreso y abre EscenaInicial. El menú es la primera escena habilitada; se mantienen todas las escenas de progresión. El perfil Windows de dev utiliza esa lista global.

Validación en copia aislada de Unity: ValidarNiveles.EjecutarMenu. Comprueba orden de arranque, controlador, módulo de entrada, raycast al botón sin obstrucciones, evento de clic y partida limpia; luego recorre las pruebas de mantenimiento y corredor. Resultado: 54 comprobaciones correctas, registradas en Validacion-menu.txt.

Para probar en el editor, abrir Assets/Scenes/RabbitscapeMenu/Scene/Menuprincipal.unity y pulsar Play; el editor reproduce la escena abierta, mientras que la compilación comienza por el menú.
