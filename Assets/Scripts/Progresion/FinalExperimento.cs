using UnityEngine;
using UnityEngine.InputSystem;
public sealed class FinalExperimento : MonoBehaviour
{
    float tiempo;
    public bool Mostrado {get;private set;}
    void Update()
    {
        tiempo+=Time.deltaTime;
        if(!Mostrado && tiempo>2.5f)
        {
            Mostrado=true;EstadoPartida.Instancia.ExperimentoCompletado=true;
            InterfazPartida.Mensaje("La salida también formaba parte del experimento.\n[R] Volver a empezar",3600f);
        }
        if(Mostrado && Keyboard.current!=null && Keyboard.current.rKey.wasPressedThisFrame)
            EstadoPartida.Instancia.NuevaPartida();
    }
}


