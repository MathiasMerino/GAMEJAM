using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PuertaCambioEscena : InteractuableNivel
{
    public string escenaDestino = "Nivel1";

    public string entradaDestino;
    public bool requiereLlaveMaestra;
    public bool puertaC;
    public override string Indicacion => puertaC && EstadoPartida.Instancia.CorredorSuperado ? "¿Otra vez? · examinar puerta C" : indicacion;

    public override void Interactuar()
    {
        var estado = EstadoPartida.Instancia;
        if (requiereLlaveMaestra && !estado.Tiene(EstadoPartida.LlaveMaestra))
        {
            InterfazPartida.Mensaje("La puerta está cerrada. Necesitas la llave maestra.");
            return;
        }
        if (puertaC && estado.CorredorSuperado)
        {
            estado.Viajar("Persecucion", "Inicio", "Algo se mueve detrás de ti. Corre.");
            return;
        }
        bool aperturaNueva = puertaC && !estado.PuertaCDesbloqueada;
        if (puertaC) estado.DesbloquearC();
        string confirmacion=aperturaNueva ? "Puerta C desbloqueada. Usaste la llave maestra y la conservas en tu inventario." : null;
        if (escenaDestino == "PasilloPrincipal" && entradaDestino == "DesdeC" && estado.CorredorSuperado)
            confirmacion="Rompiste el bucle. La puerta C ha cambiado: «¿Otra vez?».\nVuelve a esa puerta y pulsa E para continuar.";
        estado.Viajar(escenaDestino, entradaDestino, confirmacion);
    }
}



