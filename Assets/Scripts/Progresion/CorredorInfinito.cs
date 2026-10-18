using UnityEngine;

public sealed class CorredorInfinito : MonoBehaviour
{
    [Min(1)] public int vueltasNecesarias = 3;
    public float inicioX = -7f;
    public float finalX = 81.9f;
    public float longitudVuelta = 88.9f;
    public GameObject puertaRegreso;
    public SpriteRenderer lampara;
    public SpriteRenderer[] lamparas;
    Transform jugador;
    float maximoAlcanzado;
    bool retrocediendo;
    const string PistaRegreso = "Otra vez 50 metros... Date la vuelta.\nCamina hacia la izquierda hasta la puerta por la que entraste.";
    const string PuertaRevelada = "Puerta de regreso activada.\nAcércate a la puerta de la izquierda y pulsa E para salir.";
    void Start()
    {
        jugador = FindAnyObjectByType<MovimientoPlayer>().transform;
        maximoAlcanzado = jugador.position.x;
        puertaRegreso.SetActive(EstadoPartida.Instancia.CorredorSuperado);
        if (EstadoPartida.Instancia.CorredorSuperado) InterfazPartida.Mensaje(PuertaRevelada,10f);
        else if (EstadoPartida.Instancia.VueltasCorredor >= vueltasNecesarias) InterfazPartida.Mensaje(PistaRegreso,12f);

    }

    void Update()
    {
        if (lampara != null) lampara.color = new Color(1f,0.65f,0.25f, Mathf.Sin(Time.time * 17f) > -0.3f ? 0.85f : 0.12f);
        if (lamparas != null)
            foreach (var luz in lamparas)
                if (luz != null) luz.color = new Color(1f,0.65f,0.25f, Mathf.Sin(Time.time * 17f) > -0.3f ? 0.85f : 0.12f);
        if (jugador == null || Time.timeScale == 0f) return;
        EvaluarPosicion();
    }

    public void EvaluarPosicion()
    {
        var estado = EstadoPartida.Instancia;
        if (estado.CorredorSuperado) return;
        float x = jugador.position.x;
        maximoAlcanzado = Mathf.Max(maximoAlcanzado, x);
        if (x >= finalX)
        {
            estado.CompletarVuelta();
            Vector3 p = jugador.position;
            // Mismo punto relativo del siguiente módulo: salto invisible de una vuelta completa.
            p.x -= longitudVuelta;
            jugador.position = p;
            jugador.GetComponent<Rigidbody2D>().position = p;
            maximoAlcanzado = p.x;
            retrocediendo = false;
            string pista=estado.VueltasCorredor >= vueltasNecesarias ? PistaRegreso : estado.VueltasCorredor == 1 ? "¿Otra vez 50 metros?\nEste lugar se parece demasiado al anterior..." : "El pasillo te está devolviendo al mismo sitio.\nSeguir la flecha no parece acercarte a la salida.";
            InterfazPartida.Mensaje(pista,12f);

            return;
        }
        if (estado.VueltasCorredor < vueltasNecesarias) return;
        if (maximoAlcanzado - x >= .35f) retrocediendo = true;
        if (retrocediendo && x <= inicioX)
        {
            estado.RomperBucle();
            InterfazPartida.Mensaje(PuertaRevelada,10f);
            puertaRegreso.SetActive(true);

        }
    }
}




