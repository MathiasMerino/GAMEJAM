# Mensajes de interacción y orientación de C

Las confirmaciones se muestran como subtítulos al pie de la pantalla, con letra mayor y una franja translúcida para contraste. Permanecen 6,5 segundos por defecto, se conservan al cambiar de sala si no hay un aviso nuevo y su tiempo se detiene mientras se lee una nota. Una partida nueva limpia los avisos anteriores.

Se confirma: fusible recogido, fusible instalado, flujo restablecido, sistema activado y puerta derecha desbloqueada, llave recogida, apertura de C sin consumir la llave y apertura de la salida final.

Corredor C: primera vuelta, reconocer la repetición; segunda vuelta, advertir que seguir la flecha no acerca a la salida; tercera y siguientes, indicar caminar hacia la izquierda hasta la puerta de entrada. La pista se repite al volver a entrar si aún falta resolverlo. El retroceso necesario es de 0,35 unidades para permitir girar inmediatamente tras la vuelta, sin el antiguo requisito de avanzar primero. Estar quieto no lo resuelve.

Al activarse la puerta de regreso se indica E. Al volver al pasillo se explica que C ahora dice «¿Otra vez?» y que usar E allí permite continuar hacia persecución y parkour.

Validación específica: -executeMethod ValidarNiveles.EjecutarMensajes. Verifica las confirmaciones, su conservación entre salas, las pistas, el retroceso inmediato y el reinicio. El recorrido posterior permanece cubierto por las pruebas completas de escape.
