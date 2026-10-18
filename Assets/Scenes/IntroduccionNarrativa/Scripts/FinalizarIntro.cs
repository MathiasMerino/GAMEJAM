using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Video;

[RequireComponent(typeof(VideoPlayer))]
public class FinalizarIntro : MonoBehaviour
{
    VideoPlayer video;
    bool terminada;
    void Awake()
    {
        video=GetComponent<VideoPlayer>();video.playOnAwake=false;video.isLooping=false;
        video.audioOutputMode=VideoAudioOutputMode.Direct;
        video.loopPointReached+=TerminarVideo;video.errorReceived+=ErrorVideo;
    }
    IEnumerator Start()
    {
        video.Prepare();float limite=Time.realtimeSinceStartup+15;
        while(!video.isPrepared && !terminada && Time.realtimeSinceStartup<limite)yield return null;
        if(terminada)yield break;
        if(!video.isPrepared){Continuar();yield break;}
        video.Play();
    }
    void Update(){if(Keyboard.current!=null && Keyboard.current.escapeKey.wasPressedThisFrame)Continuar();}
    void OnGUI()
    {
        var estilo=new GUIStyle(GUI.skin.label){alignment=TextAnchor.LowerRight,fontSize=Mathf.RoundToInt(Screen.height*.022f)};
        GUI.Label(new Rect(0,Screen.height-55,Screen.width-25,35),"Esc · Omitir introducción",estilo);
    }
    public void Continuar()
    {
        if(terminada)return;terminada=true;video.Stop();
        EstadoPartida.Instancia.Viajar("EscenaInicial","");
    }
    void TerminarVideo(VideoPlayer vp){Continuar();}
    void ErrorVideo(VideoPlayer vp,string mensaje){Debug.LogWarning("No se pudo reproducir la intro: "+mensaje);Continuar();}
    void OnDestroy(){if(video!=null){video.loopPointReached-=TerminarVideo;video.errorReceived-=ErrorVideo;}}
}
