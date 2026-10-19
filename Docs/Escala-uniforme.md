# Tamaño uniforme del conejo

La cámara del parkour usaba altura 4 y las demás altura 5: el cambio de zoom hacía que el personaje pareciera menor al salir. Se unifican las nueve escenas jugables en altura 5 y semiancho 8,89, con el mismo ajuste de relación de aspecto. En parkour el seguimiento vertical se limita a 5–25 para cubrir el fondo de 30 unidades.

El prefab visual comparte una altura de referencia de 1,4 unidades (antes 1,15), aplicada uniformemente en ambos ejes. La corrección de escala heredada mantiene su tamaño mundial entre escenas. Se conserva el rebote, la inclinación y las articulaciones de Leoxix, eliminando el estiramiento desigual de ancho/alto durante salto y aterrizaje. Los colliders, velocidad y fuerza de salto permanecen iguales; el arte se apoya en la misma base de los pies.

La configuración se aplica también al reconstruir las escenas mediante IntegrarJugadorGrecia. Las pruebas recorren el juego y verifican la escala en suelo y aire, además del encuadre en proporciones 16:9, 4:3 y 21:9.

Validación: 175 comprobaciones correctas en Unity Play Mode, incluyendo el recorrido completo hasta el final, saltos, cajas, interacciones y comprobaciones de proporciones y cámara. Capturas inicial, parkour y final revisadas en Docs/Capturas/Escala-*.png.
