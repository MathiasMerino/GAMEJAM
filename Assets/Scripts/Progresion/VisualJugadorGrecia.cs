using UnityEngine;

// El arte sigue la física del controlador existente, sin añadir otro movimiento.
public sealed class VisualJugadorGrecia : MonoBehaviour
{
    public Animator animador;
    public string EstadoAnimacion {get;private set;}="";
    MovimientoPlayer jugador;
    Rigidbody2D cuerpo;
    float escala;
    void Awake()
    {
        jugador=GetComponentInParent<MovimientoPlayer>();cuerpo=jugador.GetComponent<Rigidbody2D>();
        escala=Mathf.Abs(transform.localScale.x);
    }
    void Update()
    {
        if(Time.timeScale==0){animador.speed=0;return;}
        Vector2 velocidad=cuerpo.linearVelocity;
        bool suelo=jugador.groundCheck!=null && velocidad.y<.2f && Physics2D.OverlapCircle(jugador.groundCheck.position,jugador.radioSuelo,jugador.capaSuelo)!=null;
        if(Mathf.Abs(velocidad.x)>.05f)transform.localScale=new Vector3(Mathf.Sign(velocidad.x)*escala,escala,escala);
        string estado=!suelo?(velocidad.y>.15f?"Saltar":"Caer"):Mathf.Abs(velocidad.x)>.1f?"Caminar":"Reposo";
        if(estado!=EstadoAnimacion){animador.CrossFadeInFixedTime(estado,.08f);EstadoAnimacion=estado;}
        animador.speed=estado=="Caminar"?Mathf.Clamp(Mathf.Abs(velocidad.x)/4f,.4f,1.1f):1;
    }
}
