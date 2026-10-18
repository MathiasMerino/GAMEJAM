using UnityEngine;

public class BotonJaula : InteractuableNivel
{
    public GameObject jaulaInicial;
    private bool activado = false;

    void Start()
    {
        permiteClic = true;
        indicacion = "Abrir jaula (también puedes hacer clic en el botón rojo)";
        if (EstadoPartida.Instancia.JaulaAbierta && jaulaInicial != null) jaulaInicial.SetActive(false);
    }

    public override void Interactuar()
    {
        if (activado) return;

        activado = true;
        EstadoPartida.Instancia.JaulaAbierta = true;

        if (jaulaInicial != null)
        {
            jaulaInicial.SetActive(false);
            Debug.Log("Jaula desactivada");
            InterfazPartida.Mensaje("Activaste el mecanismo. La jaula está abierta.", 2.5f);
        }
    }
}

