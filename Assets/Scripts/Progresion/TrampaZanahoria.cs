using UnityEngine;
public sealed class TrampaZanahoria : MonoBehaviour
{
    public Collider2D sueloTrampa;
    public GameObject hueco;
    public Transform losa;
    public bool Activada {get;private set;}
    Transform jugador;
    void OnTriggerEnter2D(Collider2D other)
    {
        if(Activada || other.GetComponent<MovimientoPlayer>()==null)return;
        Activada=true;jugador=other.transform;sueloTrampa.enabled=false;hueco.SetActive(true);
        other.GetComponent<RecuperarJugador>().enabled=false;
        InterfazPartida.Mensaje("El suelo cruje bajo tus patas...",2f);
    }
    void Update()
    {
        if(!Activada)return;
        if(losa!=null){losa.position+=Vector3.down*Time.deltaTime*5;losa.Rotate(0,0,Time.deltaTime*18);}
        if(jugador!=null && jugador.position.y < -8)
            EstadoPartida.Instancia.Viajar("HabitacionFinal","Caida");
    }
}
