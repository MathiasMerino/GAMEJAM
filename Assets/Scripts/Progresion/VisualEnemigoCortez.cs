using UnityEngine;

// El movimiento y la captura siguen perteneciendo al controlador de cada nivel.
public sealed class VisualEnemigoCortez : MonoBehaviour
{
    public Animator animador;
    EnemigoTemporal enemigo;
    Vector3 anterior;
    float escala;
    void Start()
    {
        enemigo=GetComponentInParent<EnemigoTemporal>();
        anterior=transform.parent.position;escala=Mathf.Abs(transform.localScale.x);
        animador.Play(enemigo!=null && enemigo.escalador ? "Escalar" : "Caminar");
    }
    void LateUpdate()
    {
        if(enemigo==null)return;
        Vector3 delta=transform.parent.position-anterior;anterior=transform.parent.position;
        if(!enemigo.escalador)
            transform.localScale=new Vector3(enemigo.Mirada*escala,escala,escala);
        float rapidez=Time.deltaTime>0?delta.magnitude/Time.deltaTime:0;
        animador.speed=Time.timeScale==0 || !enemigo.enabled || rapidez<.01f ? 0 : (enemigo.Persiguiendo ? .8f : .38f);
    }
}

