using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

// Dos fuentes permiten fundir ambientes sin cortes al cambiar de sala.
public sealed class AmbienteSonoro : MonoBehaviour
{
    AudioSource actual, anterior;
    AudioClip elegido;
    float mezcla=1;
    bool silencio;
    public string AmbienteActual => elegido!=null?elegido.name:"";
    void Awake()
    {
        actual=CrearFuente();anterior=CrearFuente();
        silencio=PlayerPrefs.GetInt("GAMEJAM.MusicaSilenciada",0)!=0;
        SceneManager.sceneLoaded+=Cambiar;
    }
    AudioSource CrearFuente()
    {
        var fuente=gameObject.AddComponent<AudioSource>();fuente.playOnAwake=false;
        fuente.loop=true;fuente.spatialBlend=0;fuente.volume=0;return fuente;
    }
    void Cambiar(Scene escena,LoadSceneMode modo)
    {
        string nombre=escena.name;
        if(nombre=="IntroCinematica" || nombre=="Menuprincipal")
        {actual.Stop();anterior.Stop();elegido=null;actual.clip=null;anterior.clip=null;return;}
        string pista=nombre=="Persecucion" || nombre=="EscapeVertical" ? "Acecho" : nombre=="Valle" ? "FalsaCalma" : "Laboratorio";
        var clip=Resources.Load<AudioClip>("Audio/"+pista);
        if(clip==elegido)return;
        elegido=clip;var temporal=anterior;anterior=actual;actual=temporal;
        actual.Stop();actual.clip=clip;actual.volume=0;mezcla=0;
        if(clip!=null)actual.Play();
    }
    void Update()
    {
        if(elegido!=null && Keyboard.current!=null && Keyboard.current.mKey.wasPressedThisFrame)
        {silencio=!silencio;PlayerPrefs.SetInt("GAMEJAM.MusicaSilenciada",silencio?1:0);InterfazPartida.Mensaje(silencio?"Música silenciada. M para activar.":"Música activada. M para silenciar.",2);}
        mezcla=Mathf.MoveTowards(mezcla,1,Time.unscaledDeltaTime/2);
        float volumen=silencio?0:Time.timeScale==0?.12f:.55f;
        actual.volume=volumen*mezcla;anterior.volume=volumen*(1-mezcla);
        if(mezcla>=1 && anterior.isPlaying)anterior.Stop();
    }
    void OnDestroy(){SceneManager.sceneLoaded-=Cambiar;}
}
