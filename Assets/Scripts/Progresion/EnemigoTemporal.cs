using UnityEngine;
// Silueta provisional: puede sustituirse conservando este componente y su trigger.
public sealed class EnemigoTemporal : MonoBehaviour
{
    public bool escalador;
    public bool patrulla;
    public float velocidad=3f;
    public float recorrido=1.2f;
    Vector3 origen;
    Transform objetivo;
    float esperaHasta;
    void Start()
    {
        origen=transform.position;
        var p=FindAnyObjectByType<MovimientoPlayer>();if(p!=null)objetivo=p.transform;
        esperaHasta=Time.time+2.5f;
    }
    public void Reiniciar(Vector3 punto)
    {
        transform.position=patrulla?origen:escalador?new Vector3(punto.x,punto.y-6,0):new Vector3(punto.x-8,origen.y,0);
        esperaHasta=Time.time+2.5f;
    }
    void Update()
    {
        if(objetivo==null || Time.timeScale==0 || Time.time<esperaHasta)return;
        if(patrulla)transform.position=origen+Vector3.right*Mathf.Sin(Time.time*velocidad)*recorrido;
        else if(escalador)
        {
            var p=transform.position;p.y+=velocidad*Time.deltaTime;
            p.x=Mathf.MoveTowards(p.x,objetivo.position.x,Time.deltaTime*1.1f);transform.position=p;
        }
        else transform.position+=Vector3.right*velocidad*Time.deltaTime;
    }
    void OnTriggerEnter2D(Collider2D other) { Tocar(other); }
    void OnTriggerStay2D(Collider2D other) { Tocar(other); }
    void Tocar(Collider2D other)
    {
        var jugador=other.GetComponent<RecuperarJugador>();
        if(jugador!=null)jugador.Reaparecer();
    }
}
