# Carteles, zanahoria y ajustes de dificultad

- Todos los tramos conservan el cartel pintado original. En 40, 30, 20 y 10 m solo se sustituye el pequeño recorte del número con letras pintadas del mismo estilo, sin texto generado por Unity encima.
- La zanahoria se recoge con E (o clic cercano), queda en el inventario y acompaña al conejo al caer. Acercarse no activa la trampa. Tras recogerla hay una pausa de 0,45 s antes de ceder el suelo.
- Fondo y losa usan una imagen sin zanahoria impresa. La zanahoria es un sprite independiente.
- Material físico sin fricción ni rebote en jugador, cajas y superficies para evitar quedar pegado a los laterales mientras se mantiene una dirección.
- Salto del parkour: 8 → 7,7. Los otros niveles conservan su salto.
- Enemigos: demora inicial 2,5 → 0,65 s, con perseguidor y escalador más cerca. Se conserva el margen de 2,5 s al reaparecer.
- Fusible, soporte y llave aumentados aproximadamente un 30 %. Se mantienen sus interacciones.

Aplicación repetible: `PulirNiveles.Aplicar`, también invocada al preparar el escape. Validación completa: `ValidarNiveles.Ejecutar` en una copia aislada.

Resultado: 85 comprobaciones correctas (ver `Validacion-pulido.txt`). Incluyen caída y salto contra el lateral de una caja, persecución con enemigos activos, recorrido físico de las plataformas con salto 7,7 y recogida de la zanahoria antes de caer. El recorrido de plataformas se comprueba con el enemigo desactivado para aislar su geometría; la reaparición por contacto con el escalador se comprueba por separado.

## Arte

Actualización de carteles: cuatro variantes creadas con imagegen integrada, guardadas en `Assets/Sprites/Ambiente/CorredorCartel40.png`, `CorredorCartel30.png`, `CorredorCartel20.png` y `CorredorCartel10.png`. Los sprites `NumeroPintado40/30/20/10.asset` usan únicamente el recorte del número (47 × 30 píxeles de referencia); el resto de la placa conserva exactamente el fondo original. Las placas y cifras superpuestas anteriores quedan desactivadas.

Prompt final de las cuatro variantes (N = 40, 30, 20, 10): “Precise localized edit of this exact game background. On the small metal sign at x35%, y46%, change ONLY the two digits '50' to 'N'. Exact text on sign must read 'SALIDA A' above 'N METROS', with original right arrow below. Match the original numerals' worn painted stencil-like typeface, size, baseline, spacing, muted brown-gray color, and distressing exactly; not bright white, not a modern font. Keep the new two digits in the very same 50 slot. Preserve all other pixels and entire composition, including every other letter, sign frame, arrow, bolts, lighting, background architecture. Output full image at the same wide aspect ratio and framing as input, not a closeup.”

Herramienta: imagegen integrada. Se conservan los fondos originales. Nuevos archivos:

- `Assets/Sprites/Backgrounds/Pruebas/ValleSinZanahoria.png`
- `Assets/Sprites/Ambiente/Zanahoria.png`
- `Assets/Sprites/Ambiente/CartelSinNumero.png`

Prompts finales utilizados:

Valle: “Edit target: this exact 2D game background. Remove ONLY the small orange carrot and its green leaves lying on the pavement at the far right near x87%, y63%. Reconstruct continuous empty stone pavement there. Preserve exact composition, dimensions/aspect ratio, all architecture, foliage, lighting, ground height, hand painted outlines. No new objects. Output complete background.”

Zanahoria (transparencia): “Supporting style reference: attached hand painted 2D game background and its small carrot at far right. Create one isolated collectible carrot sprite matching it: orange root pointing to lower left, short green leaves pointing upper right, side view lying horizontally at slight angle, dark illustrated outlines, textured orange skin, warm natural light. Entire carrot centered large with generous transparent margins. No ground, no floor, no shadow surface, no other objects. Actual transparent background.”

Cartel: “Edit target: this closeup industrial sign. Completely remove ONLY the two digits 50. The entire area to the left of METROS must be plain worn blue charcoal metal, without ANY ghost numerals or traces. Keep SALIDA A, METROS, arrow, metal border, bolts, surrounding wall and lighting identical. Exact original composition. No additional text. This must be an empty number slot to later render 40, 30, 20 or 10.”
