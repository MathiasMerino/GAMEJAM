using UnityEngine;

public sealed class MecanismoMantenimiento : InteractuableNivel
{
    public bool esPortafusible;
    [Range(0,2)] public int paso;
    public RespuestaAmbiental respuesta;
    public GameObject fusibleVisible;

    void Update()
    {
        var e = EstadoPartida.Instancia;
        bool activo = esPortafusible ? e.FusibleInstalado : paso == 0 ? e.PasoMantenimiento > 0 : e.MantenimientoResuelto;
        if (respuesta != null) respuesta.activo = activo;
        if (fusibleVisible != null) fusibleVisible.SetActive(e.FusibleInstalado);
    }
    public override void Interactuar()
    {
        var estado = EstadoPartida.Instancia;
        if (esPortafusible)
        {
            if (estado.FusibleInstalado) { InterfazPartida.Mensaje("El fusible ya está instalado. La máquina tiene corriente."); return; }
            bool instalado = estado.InstalarFusible();
            if (respuesta != null) respuesta.Reaccionar(instalado);
            InterfazPartida.Mensaje(instalado ? "Instalaste el fusible. La máquina recuperó la corriente." : "Falta un fusible en este soporte. Necesitas encontrar uno.");
            return;
        }
        if (!estado.FusibleInstalado) { if (respuesta != null) respuesta.Reaccionar(false); InterfazPartida.Mensaje("La máquina no tiene corriente. Revisa el soporte del fusible."); return; }
        if (estado.MantenimientoResuelto) { InterfazPartida.Mensaje("El sistema está encendido. La puerta de la derecha está desbloqueada."); return; }
        if (paso == 0 && estado.PasoMantenimiento > 0) { InterfazPartida.Mensaje("El flujo ya está restablecido. Puedes activar el botón rojo."); return; }
        bool correcto = estado.ActivarMecanismo(paso);
        if (respuesta != null) respuesta.Reaccionar(correcto);
        if (!correcto) { InterfazPartida.Mensaje("El botón no responde: primero restablece el flujo con la manija circular del tanque."); return; }
        InterfazPartida.Mensaje(estado.MantenimientoResuelto ? "Activaste el sistema. Se desbloqueó la puerta de la derecha." : "Giraste la manija. Flujo restablecido; ahora puedes pulsar el botón rojo.");
    }
}
