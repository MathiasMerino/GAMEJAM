# Prueba jugable del laboratorio

Trabajo en la rama `pruebaniveles`. No requiere paquetes adicionales.

## Abrir y jugar

1. Esperar a que Unity termine de importar y compilar.
2. Abrir `Assets/Scenes/EscenaInicial/EscenaInicial.unity` desde Project. Si ya estaba abierta antes de recibir los cambios, volver a abrirla para cargar la versión guardada en disco.
3. Pulsar Play. El jugador temporal tiene cuerpo oscuro y orejas geométricas.
4. A/D o flechas: caminar. Espacio: saltar. E: interactuar con el objeto más cercano. Los mecanismos de mantenimiento y el botón de la jaula también admiten clic a distancia. Mantener Tab muestra el inventario.

Las referencias, colliders, entradas y fondos quedan configurados en las escenas. No hace falta arrastrar componentes en el Inspector.

## Etapa 2: recorrido entre salas

- Se conserva `EscenaInicial`; el mecanismo rojo abre la jaula y su puerta lleva a `PasilloPrincipal`.
- Se conserva `Nivel1` como mantenimiento, accesible desde la puerta B.
- A y B están disponibles desde el principio. Ambas tienen un regreso al pasillo.
- La puerta C requiere la llave maestra. Los mensajes se muestran en pantalla.
- Se crean `PasilloPrincipal`, `PuertaA` y `PuertaC` y se incluyen en la lista de escenas de Build Settings / Build Profiles.
- El menú existente se conserva. Su botón Jugar comienza una nueva partida; no se cambia la escena de arranque del equipo.

Prueba: visitar B antes que A, intentar instalar el fusible y regresar. Intentar abrir C sin llave. Deben aparecer mensajes y mantenerse el control del personaje.

## Etapa 3: fusible e inventario

En A hay escombros y plataformas. El fusible está sobre la repisa de la derecha. Acercarse y pulsar E para recogerlo. Regresar al pasillo conserva el objeto.

`EstadoPartida` mantiene una sola instancia entre escenas. El inventario usa identificadores de texto: `Recoger(id)` y `Tiene(id)` permiten añadir objetos. Instalar el fusible lo retira del inventario y conserva el estado de instalación. No vuelve a aparecer al regresar a A.

## Etapa 4: mantenimiento

La nota junto a la caja abre una nueva ilustración de mantenimiento, con el mismo papel y dibujo de conejo que la nota original. La imagen anterior permanece intacta. Se cierra con E o clic.

Solución:

1. Instalar el fusible en el soporte visible de la máquina izquierda. El fusible aparece colocado.
2. Girar la manija circular del tanque para restablecer el flujo.
3. Pulsar el botón rojo para desbloquear la puerta derecha.
4. Acercarse a esa puerta y pulsar E para recoger la llave maestra de su cierre.

El manómetro es decorativo: no exige un paso adicional. Pulsar el botón antes de tiempo muestra qué falta; repetir la manija no borra el progreso. Instalar, activar y recoger muestran confirmaciones. El progreso parcial se conserva al salir de B. La llave sirve para C y se conserva después de abrirla, con confirmación al entrar al corredor.

## Etapas 5 y 6: corredor infinito

Hay cinco tramos con señales de 50, 40, 30, 20 y 10 metros, más módulos de continuidad visual. Una sola escena evita recargas, duplicación de scripts y pérdida de estado entre tramos. Al completar una vuelta se desplaza al jugador una longitud de ciclo, manteniendo altura y velocidad.

Después de tres vueltas, caminar hacia la izquierda hasta la entrada. Basta retroceder 0,35 unidades para reconocer la intención: no hace falta avanzar primero, y quedarse quieto no resuelve el puzzle. Aparece una puerta de regreso. Pulsar E devuelve al pasillo; el rótulo C cambia a «¿Otra vez?».

El corredor muestra pistas progresivas al terminar las vueltas. La tercera indica volver hacia la izquierda y, al activar la puerta, indica pulsar E. Al completar el bucle, volver al pasillo y usar C inicia la persecución. El recorrido posterior se describe en Escape-completo.md.

## Inspector y ajustes

- `PuertaCambioEscena`: destino, entrada de llegada, requisito de llave y marca de puerta C.
- `PuntoEntrada`: identificador que coincide con la entrada de la puerta de origen.
- `MecanismoMantenimiento`: soporte de fusible o paso 0/1/2; permiso de clic.
- `ObjetoRecogible`: identificador, nombre y requisito de mantenimiento.
- `CorredorInfinito`: vueltas necesarias, límites del recorrido, puerta revelada y lámpara.
- `MovimientoPlayer`: velocidad 4, salto 7, referencia a GroundCheck y capa Suelo.
- `CamaraNivel`: encuadre y límites horizontales del corredor.

El jugador es temporal. Los objetos y plataformas utilizan sprites ambientales; los mecanismos coinciden con los elementos del fondo. Consulta `Integracion-ambiental.md` para los cambios visuales. Los nuevos fondos proceden de las imágenes suministradas. Los fondos de persecución, valle, habitación final y parkour permanecen en `Assets/Sprites/Backgrounds/Pruebas` y ahora se utilizan en las escenas del escape.

## Persistencia y límites

La progresión dura durante la sesión de juego y los cambios de escena. No se guarda en disco al cerrar el juego o detener Play. El botón Jugar del menú limpia la sesión.

La ampliación posterior incorpora persecución, parkour vertical, puerta final, valle y trampa de zanahoria. Consulta Escape-completo.md para el recorrido y los límites de los personajes temporales.

## Verificación

`Assets/Editor/ValidarNiveles.cs` contiene una prueba de integración para ejecutarse en una copia con Unity en batch mode mediante `-executeMethod ValidarNiveles.Ejecutar` (sin `-quit`, el propio test termina el proceso). Comprueba salto y tecla E usando el Input System, bloqueo de C, nota, objetos, secuencia correcta/incorrecta, persistencia, vueltas, retroceso y nueva partida. Escribe `ValidationResults/resultado-niveles.txt`.

El constructor `PrepararNiveles` se conserva para reproducir el montaje en una copia del proyecto original. Omite las escenas nuevas que ya existen y las escenas existentes que ya contienen `ProgresionLaboratorio`; no es necesario ejecutarlo al abrir esta entrega.

Vigilar en Console referencias faltantes, errores de carga de escena o errores de compilación. Si el equipo añade nuevos objetos interactuables, sus colliders deben ser trigger y estar en el mismo GameObject que el componente `IInteractuable`; el jugador debe conservar Rigidbody2D, collider e InteraccionPlayer.





