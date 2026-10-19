using UnityEngine;
public sealed class TrampaZanahoria : InteractuableNivel
{
    public Collider2D sueloTrampa;
    public GameObject hueco;
    public Transform losa;
    public Transform zanahoria;
    public bool Activada {get;private set;}
    Transform jugador;
    float instanteCaida;
    public override void Interactuar()
    {
        if(Activada)return;
        var conejo=FindAnyObjectByType<MovimientoPlayer>();
        if(conejo==null || Vector2.Distance(conejo.transform.position,transform.position)>1.6f)return;
        Activada=true;jugador=conejo.transform;
        EstadoPartida.Instancia.Recoger(EstadoPartida.Zanahoria);
        if(zanahoria!=null){zanahoria.SetParent(jugador,true);zanahoria.localPosition=new Vector3(.28f,-.05f,-.1f);}
        conejo.enabled=false;
        jugador.GetComponent<Rigidbody2D>().linearVelocity=Vector2.zero;
        var recuperacion=jugador.GetComponent<RecuperarJugador>();if(recuperacion!=null)recuperacion.enabled=false;
        instanteCaida=Time.time+.45f;
        InterfazPartida.Mensaje("Recogiste la zanahoria. La guardaste en tu inventario.\nEl suelo está cediendo...",6.5f);
    }
    void Update()
    {
        if(!Activada)return;
        if(Time.time<instanteCaida)
        {
            var rb=jugador.GetComponent<Rigidbody2D>();
            rb.position=new Vector2(Mathf.MoveTowards(rb.position.x,sueloTrampa.bounds.center.x,4f*Time.deltaTime),rb.position.y);
            return;
        }
        sueloTrampa.enabled=false;hueco.SetActive(true);
        if(losa!=null){losa.position+=Vector3.down*Time.deltaTime*5;losa.Rotate(0,0,Time.deltaTime*18);}
        if(jugador!=null && jugador.position.y < -8)
            EstadoPartida.Instancia.Viajar("HabitacionFinal","Caida");
    }
}
