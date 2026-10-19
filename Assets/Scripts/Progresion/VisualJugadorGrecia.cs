using UnityEngine;

// El arte sigue la física del controlador existente, sin añadir otro movimiento.
public sealed class VisualJugadorGrecia : MonoBehaviour
{
    public Animator animador;
    public string EstadoAnimacion {get;private set;}="";
    MovimientoPlayer jugador;
    Rigidbody2D cuerpo;
    Vector3 escalaBase;
    Vector3 posicionBase;
    Quaternion rotacionBase;
    Transform[] piezasConejo;
    Vector3[] posicionesBasePiezas;
    Vector3[] eulerBasePiezas;
    Vector2 velocidadActual;
    float faseSaltito;
    float ultimoContactoSuelo;
    float tiempoAterrizaje;
    float direccionActual=1f;
    bool estabaEnAire;
    bool enAireActual;
    string clipActual="";

    const float DuracionAterrizaje=.12f;

    void Awake()
    {
        jugador=GetComponentInParent<MovimientoPlayer>();
        if(jugador!=null)cuerpo=jugador.GetComponent<Rigidbody2D>();
        escalaBase=new Vector3(
            Mathf.Abs(transform.localScale.x),
            Mathf.Abs(transform.localScale.y),
            Mathf.Abs(transform.localScale.z));
        posicionBase=transform.localPosition;
        rotacionBase=transform.localRotation;
        PrepararArticulacionesSalto();
    }

    void Update()
    {
        if(animador==null||jugador==null||cuerpo==null)return;
        if(Time.timeScale==0){animador.speed=0;return;}

        Vector2 velocidad=cuerpo.linearVelocity;
        velocidadActual=velocidad;
        bool detectaSuelo=jugador.groundCheck!=null && Physics2D.OverlapCircle(jugador.groundCheck.position,jugador.radioSuelo,jugador.capaSuelo)!=null;
        if(detectaSuelo&&velocidad.y<=.45f)ultimoContactoSuelo=Time.time;
        bool suelo=velocidad.y<=.45f&&(detectaSuelo||Time.time-ultimoContactoSuelo<=.1f);
        enAireActual=!suelo;
        if(Mathf.Abs(velocidad.x)>.05f)direccionActual=velocidad.x<0f?-1f:1f;

        if(estabaEnAire&&suelo)tiempoAterrizaje=DuracionAterrizaje;
        estabaEnAire=!suelo;
        tiempoAterrizaje=Mathf.Max(0f,tiempoAterrizaje-Time.deltaTime);

        Vector3 escalaPadreActual=transform.parent!=null?transform.parent.lossyScale:Vector3.one;
        float factorX=1f;
        float factorY=1f;
        bool caminando=suelo&&Mathf.Abs(velocidad.x)>.1f;
        EstadoAnimacion=!suelo?(velocidad.y>.15f?"Saltar":"Caer"):caminando?"Caminar":"Reposo";

        // En el aire el clip de reposo mantiene unida la silueta; las articulaciones
        // siguen una pose de impulso, recogida y aterrizaje aplicada en LateUpdate.
        string clip=!suelo?"Reposo":EstadoAnimacion;
        if(clip!=clipActual)
        {
            animador.CrossFadeInFixedTime(clip,.08f);
            clipActual=clip;
        }

        if(caminando)
        {
            float frecuencia=Mathf.Lerp(1.2f,2f,Mathf.InverseLerp(.5f,4f,Mathf.Abs(velocidad.x)));
            faseSaltito+=Time.deltaTime*frecuencia;
            float alturaPadre=transform.parent!=null?Mathf.Abs(transform.parent.lossyScale.y):1f;
            float rebote=Mathf.Max(0,Mathf.Sin(faseSaltito*Mathf.PI*2f))*.075f/Mathf.Max(.0001f,alturaPadre);
            transform.localPosition=posicionBase+Vector3.up*rebote;
            transform.localRotation=rotacionBase;
            animador.speed=frecuencia;
        }
        else if(!suelo)
        {
            faseSaltito=0f;
            transform.localPosition=posicionBase;

            // El conejo alarga el cuerpo al impulsarse y baja el hocico al descender.
            // El signo de la inclinacion hace que mire hacia delante en la caida.
            float ascenso=Mathf.InverseLerp(-8f,7f,Mathf.Clamp(velocidad.y,-8f,7f));
            float descenso=velocidad.y<0f?Mathf.InverseLerp(0f,-8f,velocidad.y):0f;
            factorX=1f+.12f*ascenso+.035f*descenso;
            factorY=1f-.13f*ascenso-.045f*descenso;
            float inclinacion=12f*ascenso-10f*descenso;
            transform.localRotation=rotacionBase*Quaternion.Euler(0f,0f,direccionActual*inclinacion);
            animador.speed=1f;
        }
        else
        {
            faseSaltito=0;
            transform.localPosition=posicionBase;
            transform.localRotation=rotacionBase;
            if(tiempoAterrizaje>0f)
            {
                float impacto=tiempoAterrizaje/DuracionAterrizaje;
                factorX=Mathf.Lerp(1f,1.07f,impacto);
                factorY=Mathf.Lerp(1f,.88f,impacto);
            }
            animador.speed=1;
        }

        transform.localScale=new Vector3(
            escalaBase.x/Mathf.Max(.0001f,Mathf.Abs(escalaPadreActual.x))*direccionActual*factorX,
            escalaBase.y/Mathf.Max(.0001f,Mathf.Abs(escalaPadreActual.y))*factorY,
            escalaBase.z/Mathf.Max(.0001f,Mathf.Abs(escalaPadreActual.z)));
    }

    void PrepararArticulacionesSalto()
    {
        if(animador==null)return;
        string[] nombres={"torax","cabeza","Pata_Derecha","Pata_Izq","T_Derech","T_Izq"};
        piezasConejo=new Transform[nombres.Length];
        posicionesBasePiezas=new Vector3[nombres.Length];
        eulerBasePiezas=new Vector3[nombres.Length];
        for(int i=0;i<nombres.Length;i++)
        {
            piezasConejo[i]=animador.transform.Find(nombres[i]);
            if(piezasConejo[i]!=null)
            {
                posicionesBasePiezas[i]=piezasConejo[i].localPosition;
                eulerBasePiezas[i]=piezasConejo[i].localEulerAngles;
            }
        }
    }

    void LateUpdate()
    {
        if(!enAireActual||Time.timeScale==0f||piezasConejo==null)return;

        float fuerza=Mathf.Max(1f,jugador!=null?jugador.fuerzaSalto:7f);
        float ascenso=velocidadActual.y>0f?Mathf.InverseLerp(0f,fuerza,velocidadActual.y):0f;
        float descenso=velocidadActual.y<0f?Mathf.InverseLerp(0f,-fuerza,velocidadActual.y):0f;
        ascenso=Suavizar(ascenso);
        descenso=Suavizar(descenso);
        float impulso=Suavizar(Mathf.InverseLerp(.35f,1f,ascenso));
        float preparacionAterrizaje=Suavizar(Mathf.InverseLerp(.25f,1f,descenso));
        // Secuencia de vuelo: manos arriba en el impulso, silueta horizontal en
        // el punto alto y manos abajo al descender. Las piezas giran sobre sus
        // pivotes originales: no se desplazan y permanecen unidas al torso.
        float anguloDelanteras=90f+40f*impulso-80f*preparacionAterrizaje;
        float anguloTraseras=-90f-10f*impulso+65f*preparacionAterrizaje;
        AplicarPose(0,Vector3.zero,10f*impulso-12f*preparacionAterrizaje);
        AplicarPose(1,Vector3.zero,-5f*impulso+8f*preparacionAterrizaje);
        AplicarPose(2,Vector3.zero,anguloDelanteras-3f);
        AplicarPose(3,Vector3.zero,anguloDelanteras+3f);
        AplicarPose(4,Vector3.zero,anguloTraseras-3f);
        AplicarPose(5,Vector3.zero,anguloTraseras+3f);
    }

    static float Suavizar(float valor)
    {
        valor=Mathf.Clamp01(valor);
        return valor*valor*(3f-2f*valor);
    }

    void AplicarPose(int indice, Vector3 desplazamiento, float giroZ)
    {
        if(indice>=piezasConejo.Length||piezasConejo[indice]==null)return;
        Transform pieza=piezasConejo[indice];
        pieza.localPosition=posicionesBasePiezas[indice]+desplazamiento;
        Vector3 euler=eulerBasePiezas[indice];
        euler.z+=giroZ;
        pieza.localEulerAngles=euler;
    }
}
