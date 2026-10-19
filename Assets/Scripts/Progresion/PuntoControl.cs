using UnityEngine;
public sealed class PuntoControl : MonoBehaviour
{
    public Vector3 posicionSegura;
    void OnTriggerEnter2D(Collider2D other)
    {
        var jugador=other.GetComponent<RecuperarJugador>();
        if(jugador!=null)jugador.GuardarPunto(posicionSegura);
    }
}
