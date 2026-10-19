using UnityEngine;
public sealed class PuertaFinal : InteractuableNivel
{
    public override void Interactuar()
    {
        var e=EstadoPartida.Instancia;
        if(!e.Tiene(EstadoPartida.LlaveMaestra)){InterfazPartida.Mensaje("Necesitas la llave maestra para abrir esta puerta.");return;}
        e.ParkourSuperado=true;
        e.Viajar("Valle","Inicio","La llave gira en el cierre. Por fin entra aire del exterior.");
    }
}
