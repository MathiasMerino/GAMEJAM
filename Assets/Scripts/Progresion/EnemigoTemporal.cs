using UnityEngine;

// Conserva el trigger de captura y los puntos seguros de los niveles.
public sealed class EnemigoTemporal : MonoBehaviour
{
    public bool escalador, patrulla;
    public float velocidad=3f, recorrido=1.2f, demoraInicial=.65f;
    public float alcanceLinterna=6f, semiAnguloLinterna=32f;
    public bool Persiguiendo { get; private set; }
    public float Mirada { get; private set; }=1;
    public Vector2 DireccionLuz => escalador ? new Vector2(Mirada*.25f,1).normalized : new Vector2(Mirada,0);
    public Vector2 Emisor => emisor!=null ? (Vector2)emisor.position : (Vector2)transform.position+Vector2.up*.35f;
    public Transform emisor;
    Vector3 origen, ultimoAvistamiento;
    Transform objetivo;
    float esperaHasta, ultimaVista, buscarHasta;
    readonly RaycastHit2D[] impactos=new RaycastHit2D[32];
    void Start()
    {
        origen=transform.position;
        var p=FindAnyObjectByType<MovimientoPlayer>(); if(p!=null)objetivo=p.transform;
        esperaHasta=Time.time+demoraInicial;
    }
    public void Reiniciar(Vector3 punto)
    {
        transform.position=patrulla?origen:escalador?new Vector3(punto.x,punto.y-6,0):new Vector3(punto.x-8,origen.y,0);
        esperaHasta=Time.time+2.5f; Persiguiendo=false; buscarHasta=0; Mirada=1;
    }
    public float DistanciaLibre(Vector2 direccion,float distancia)
    {
        var filtro=ContactFilter2D.noFilter; filtro.useTriggers=false;
        int cantidad=Physics2D.Raycast(Emisor,direccion,filtro,impactos,distancia);
        float libre=distancia;
        for(int i=0;i<cantidad;i++)
        {
            var c=impactos[i].collider;
            if(c.transform.IsChildOf(transform) || c.GetComponentInParent<MovimientoPlayer>()!=null || c.GetComponentInParent<EnemigoTemporal>()!=null)continue;
            libre=Mathf.Min(libre,impactos[i].distance);
        }
        return libre;
    }
    public bool PuedeVer(Vector2 punto)
    {
        Vector2 delta=punto-Emisor;
        return delta.magnitude<=alcanceLinterna && Vector2.Angle(DireccionLuz,delta)<=semiAnguloLinterna && DistanciaLibre(delta.normalized,delta.magnitude)>=delta.magnitude-.02f;
    }
    void Update()
    {
        if(objetivo==null || Time.timeScale==0 || Time.time<esperaHasta)return;
        if(PuedeVer(objetivo.position))
        { Persiguiendo=true; ultimoAvistamiento=objetivo.position; ultimaVista=Time.time; buscarHasta=Time.time+4; }
        else if(Persiguiendo && Time.time-ultimaVista>1.2f)Persiguiendo=false;
        float paso=Persiguiendo ? (escalador?velocidad:Mathf.Max(3.15f,velocidad)) : (escalador?velocidad*.4f:.65f);
        var p=transform.position;
        if(Persiguiendo || Time.time<buscarHasta)
        {
            float diferencia=ultimoAvistamiento.x-p.x;
            if(Mathf.Abs(diferencia)>.15f)Mirada=Mathf.Sign(diferencia);
            else if(!Persiguiendo)Mirada=Mathf.Sin(Time.time*2)>0?1:-1;
            p.x=Mathf.MoveTowards(p.x,ultimoAvistamiento.x,paso*Time.deltaTime);
            if(escalador)p.y=Mathf.MoveTowards(p.y,ultimoAvistamiento.y,paso*Time.deltaTime);
        }
        else if(escalador)p.y+=paso*Time.deltaTime;
        else
        {
            float radio=patrulla?Mathf.Max(recorrido,2):4;
            if(p.x>origen.x+radio)Mirada=-1;
            if(p.x<origen.x-radio)Mirada=1;
            p.x+=Mirada*paso*Time.deltaTime;
        }
        transform.position=p;
    }
    void OnTriggerEnter2D(Collider2D other){Tocar(other);}
    void OnTriggerStay2D(Collider2D other){Tocar(other);}
    void Tocar(Collider2D other)
    {
        if(Time.time<esperaHasta)return;
        var jugador=other.GetComponent<RecuperarJugador>(); if(jugador!=null)jugador.Reaparecer();
    }
}

