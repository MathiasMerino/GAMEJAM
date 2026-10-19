# Integración ambiental

Se retiraron los rótulos de acciones y las placas negras de las escenas. Los objetos del mantenimiento usan zonas de interacción sobre el dibujo original (contactos, válvula, manómetro, botón y cajón), con recortes del mismo fondo para animar su respuesta. No se muestran objetivos ni instrucciones al acercarse. Tab permite consultar el inventario. Los mensajes aparecen después de interactuar y describen lo ocurrido, sin enumerar la secuencia.

La nota vuelve a utilizar su imagen original `Assets/UI/Notas/nota pista.png`. Se abre con E y se cierra con E o clic. Su contenido ilustrado original se conservó.

Se crearon tres sprites transparentes mediante imagegen, usando el fondo de Nivel1 como referencia: un fusible cerámico envejecido, una llave maestra de latón y una caja industrial azul grisácea. Están en `Assets/Sprites/Ambiente`. Los prompts solicitaron un objeto aislado, sin texto ni interfaz, con pintura detallada, sombras oscuras y luz ámbar del laboratorio. Los recortes de sprites son recursos de Unity; los fondos originales no se modificaron.

`IntegrarAmbiente.Aplicar` conserva los objetos y referencias originales y aplica la presentación ambiental de forma repetible. `PrepararNiveles.Construir` llama a ese paso al terminar. No hace falta ejecutarlos para jugar las escenas guardadas.

Actualización posterior: consulta Mantenimiento-didactico.md. La nota ahora muestra las instrucciones de mantenimiento ilustradas; la secuencia utiliza solo manija y botón después del fusible. La llave se recoge en la puerta derecha.
