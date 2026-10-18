# Mantenimiento didáctico — recursos y validación

Rama: pruebaniveles. La sala B reutiliza Nivel1. Se conserva la nota anterior como recurso; el Canvas ahora muestra NotaMantenimiento. El soporte se integra en la máquina izquierda y muestra el fusible instalado. La llave se recoge del cierre de la puerta derecha, una vez activados el flujo y el botón rojo. No se otorga al pulsar el botón.

Recursos nuevos: Assets/Sprites/Ambiente/Portafusible.png y NotaMantenimiento.png, con sprites recortados por Unity. Generados con la herramienta integrada imagegen, sin CLI. Los fondos no se editaron.

Prompt del portafusible: Create a single isolated game sprite on transparent background: EMPTY industrial fuse holder mounted on a small dark blue-gray weathered metal plate, front orthographic view. Match reference background painted 2D detailed abandoned industrial laboratory style, amber overhead light, rust and worn edges. Two clearly visible brass clips separated by an EMPTY horizontal ceramic-fuse-shaped recess. Four mounting screws, small faded lightning symbol. No fuse installed, no lettering, no labels, no background scene. Compact landscape shape. Sprite intended to mount on left machinery and show clearly where player can insert a fuse. Only this object, centered, transparent outside.

Prompt de la nota: Edit this illustrated parchment note for a Unity game. Preserve its worn tan parchment, irregular torn edges, folded top right corner, handwritten dark brown ink and small rabbit sketch at bottom right. Transparent outside parchment. Replace all existing text with exactly this Spanish text, large and clearly legible with generous line spacing: MANTENIMIENTO / 1. Coloca el fusible en el soporte de la máquina de la izquierda. / 2. Gira la manija circular del tanque para restablecer el flujo. / 3. Pulsa el botón rojo. / 4. Revisa la puerta de la derecha. No other text, no interface, preserve illustrated art style. Use actual line breaks.

Las otras salas conservan su diseño. Se confirma la apertura de jaula, la recogida del fusible, el uso de la llave en C sin consumirla y la respuesta de la puerta después de romper el bucle. No se añaden placas flotantes con instrucciones ni pistas anticipadas para el corredor.

El recorrido se verifica con PruebaNiveles en una copia aislada de Unity; los resultados se guardan en Validacion-didactica.txt.
