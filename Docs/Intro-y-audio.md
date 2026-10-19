# Intro de Sidny y ambiente sonoro

Origen de la cinematografía: `origin/dev`, commit `00da109`, incorporado mediante PR #4 (`991d977`). La rama `origin/Sidny` todavía apunta al menú anterior. Se importó únicamente la carpeta IntroduccionNarrativa y se adaptaron las conexiones; no se reemplazaron los niveles ni la progresión local.

Jugar limpia la partida, abre IntroCinematica y luego EscenaInicial. Se conserva el MP4 y su pista original. Esc permite omitir la introducción. Si el vídeo no puede prepararse en 15 segundos o informa un error, continúa al juego. Durante la intro no se muestra el inventario ni se reproduce música de fondo adicional.

Música original sintetizada, sin reutilizar muestras ni melodías del MP4: Laboratorio (drones graves, resonancias metálicas y pulso discreto), Acecho (pulso más frecuente para persecución y parkour), FalsaCalma (notas más abiertas para el valle). Son tres bucles estéreo de 48 segundos, con fundidos de 2 segundos entre ambientes. M silencia/restaura la música; la preferencia se conserva. Leer notas reduce el volumen.

Los WAV están en Assets/Resources/Audio. Tools/crear_ambientes.py permite regenerarlos de forma determinista con numpy y scipy. Picos por debajo de -9 dBFS; no hay saturación digital. Es una propuesta de ambiente industrial relacionada con el escenario, no una copia de la banda sonora de la intro.

Validación: 121 comprobaciones correctas en Play Mode desde el clic real de Jugar hasta el final (`Validacion-Intro-Audio.txt`). Incluye reproducción real de fotogramas, pista de audio del vídeo, final natural de la intro, omisión con Esc, partida limpia, tres clips estéreo de más de 40 segundos, reproducción en bucle, M para silenciar/restaurar, transición a Acecho y FalsaCalma, y recorrido previo de puzzles, persecución y parkour. La inspección del sonido nuevo es técnica (niveles, ausencia de recorte y reproducción); el balance artístico final queda abierto a la escucha del equipo.
