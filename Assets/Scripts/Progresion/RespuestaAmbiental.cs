using UnityEngine;

// Anima un detalle del propio fondo; no añade señalización ni resalta objetos antes de probarlos.
public sealed class RespuestaAmbiental : MonoBehaviour
{
    public bool activo;
    public SpriteRenderer detalle;
    public float giroActivo;
    Vector3 origen;
    float impulso;
    AudioSource sonido;
    AudioClip clic, fallo;
    void Awake()
    {
        origen = transform.localPosition;
        sonido = gameObject.AddComponent<AudioSource>(); sonido.playOnAwake = false; sonido.volume = 0.16f;
        clic = CrearSonido(180f); fallo = CrearSonido(65f);
    }
    static AudioClip CrearSonido(float tono)
    {
        const int muestras = 4410;
        var datos = new float[muestras];
        for (int i=0;i<muestras;i++) datos[i] = Mathf.Sin(i * tono * 2f * Mathf.PI / 22050f) * Mathf.Exp(-i / 650f);
        var clip = AudioClip.Create("RespuestaMecanica",muestras,1,22050,false); clip.SetData(datos,0); return clip;
    }
    public void Reaccionar(bool correcto)
    {
        impulso = 1f;
        if (sonido != null) sonido.PlayOneShot(correcto ? clic : fallo);
    }
    void Update()
    {
        impulso = Mathf.MoveTowards(impulso,0,Time.deltaTime * 3f);
        transform.localPosition = origen + Vector3.right * (Mathf.Sin(Time.time * 45f) * impulso * 0.015f);
        transform.localRotation = Quaternion.Slerp(transform.localRotation,Quaternion.Euler(0,0,activo ? giroActivo : 0),Time.deltaTime * 7f);
        if (detalle != null) detalle.color = Color.Lerp(Color.white,new Color(1f,0.87f,0.7f),impulso * 0.25f);
    }
    void OnDestroy() { if (clic != null) Destroy(clic); if (fallo != null) Destroy(fallo); }
}
