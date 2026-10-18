using UnityEngine;

public sealed class RecuperarJugador : MonoBehaviour
{
    public float limiteCaida=-8f;
    public bool seguirAlturaCheckpoint;
    public Vector3 PuntoSeguro { get; private set; }
    public int Reintentos { get; private set; }
    float inmuneHasta;
    void Start() { PuntoSeguro=transform.position; }
    void Update() { if(transform.position.y < limiteCaida) Reaparecer(); }
    public void GuardarPunto(Vector3 punto)
    {
        if(Vector3.Distance(PuntoSeguro,punto)<.1f)return;
        PuntoSeguro=punto;
        if(seguirAlturaCheckpoint)limiteCaida=punto.y-6;
        InterfazPartida.Mensaje("Punto de reaparición guardado.",2.5f);
    }
    public void Reaparecer()
    {
        if(Time.time<inmuneHasta || EstadoPartida.Instancia.Cargando)return;
        Reintentos++;inmuneHasta=Time.time+1.5f;
        var rb=GetComponent<Rigidbody2D>();rb.linearVelocity=Vector2.zero;rb.position=PuntoSeguro;
        transform.position=PuntoSeguro;
        foreach(var enemigo in FindObjectsByType<EnemigoTemporal>())enemigo.Reiniciar(PuntoSeguro);
        InterfazPartida.Mensaje("Vuelve a intentarlo. Conservas tus objetos.");
    }
}
