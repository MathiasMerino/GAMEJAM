using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// Estado de la sesión: sobrevive a cambios de escena, no a cerrar el juego.
public sealed class EstadoPartida : MonoBehaviour
{
    public const string Fusible = "fusible";
    public const string Zanahoria = "zanahoria";
    public const string LlaveMaestra = "llave_maestra";
    public static EstadoPartida Instancia { get; private set; }
    readonly HashSet<string> objetos = new HashSet<string>();
    public IEnumerable<string> Objetos => objetos;
    public bool JaulaAbierta { get; set; }
    public bool FusibleInstalado { get; private set; }
    public bool MantenimientoResuelto { get; private set; }
    public bool PuertaCDesbloqueada { get; private set; }
    public bool CorredorSuperado { get; private set; }
    public int PasoMantenimiento { get; private set; }
    public int VueltasCorredor { get; private set; }
    public bool Cargando { get; private set; }
    public bool PersecucionSuperada, ParkourSuperado, ExperimentoCompletado;
    string siguienteEntrada;
    string mensajeLlegada;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    static void LimpiarEstaticos() { Instancia = null; }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void Crear()
    {
        if (Instancia == null) new GameObject("EstadoPartida").AddComponent<EstadoPartida>();
    }

    void Awake()
    {
        if (Instancia != null && Instancia != this) { Destroy(gameObject); return; }
        Instancia = this;
        DontDestroyOnLoad(gameObject);
        gameObject.AddComponent<InterfazPartida>();
        gameObject.AddComponent<AmbienteSonoro>();
        SceneManager.sceneLoaded += EscenaCargada;
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= EscenaCargada;
        if (Instancia == this) Instancia = null;
    }

    public bool Tiene(string id) { return objetos.Contains(id); }
    public bool Recoger(string id) { return !string.IsNullOrWhiteSpace(id) && objetos.Add(id); }

    public bool InstalarFusible()
    {
        if (FusibleInstalado) return true;
        if (!objetos.Remove(Fusible)) return false;
        FusibleInstalado = true;
        return true;
    }

    public bool ActivarMecanismo(int paso)
    {
        if (!FusibleInstalado) return false;
        if (MantenimientoResuelto) return true;
        int esperado = PasoMantenimiento == 0 ? 0 : 2;
        if (paso != esperado) return false;
        PasoMantenimiento++;
        if (PasoMantenimiento == 2) MantenimientoResuelto = true;
        return true;
    }

    public bool DesbloquearC()
    {
        if (!Tiene(LlaveMaestra)) return false;
        PuertaCDesbloqueada = true;
        return true;
    }

    public void CompletarVuelta() { VueltasCorredor++; }
    public void RomperBucle() { CorredorSuperado = true; }

    public void Viajar(string escena, string entrada, string confirmacion = null)
    {
        if (Cargando) return;
        if (!Application.CanStreamedLevelBeLoaded(escena))
        {
            InterfazPartida.Mensaje("No se puede cargar " + escena + ". Revisa Build Profiles.");
            Debug.LogError("Escena no incluida en Build Profiles: " + escena);
            return;
        }
        Cargando = true;
        siguienteEntrada = entrada;
        mensajeLlegada = confirmacion;
        Time.timeScale = 1f;
        SceneManager.LoadSceneAsync(escena);
    }

    void EscenaCargada(Scene escena, LoadSceneMode modo)
    {
        Cargando = false;
        var jugador = FindAnyObjectByType<MovimientoPlayer>();
        if (jugador != null && !string.IsNullOrEmpty(siguienteEntrada))
        {
            foreach (var punto in FindObjectsByType<PuntoEntrada>())
                if (punto.id == siguienteEntrada)
                {
                    jugador.transform.position = punto.transform.position;
                    var cuerpo = jugador.GetComponent<Rigidbody2D>();
                    cuerpo.position = punto.transform.position;
                    cuerpo.linearVelocity = Vector2.zero;
                    break;
                }
        }
        siguienteEntrada = null;
        InterfazPartida.AlCambiarEscena();
        if (!string.IsNullOrEmpty(mensajeLlegada)) InterfazPartida.Mensaje(mensajeLlegada);
        mensajeLlegada = null;
    }

    public void NuevaPartida(bool mostrarIntro = false)
    {
        objetos.Clear();
        InterfazPartida.LimpiarMensajes();
        JaulaAbierta = FusibleInstalado = MantenimientoResuelto = false;
        PuertaCDesbloqueada = CorredorSuperado = false;
        PasoMantenimiento = VueltasCorredor = 0;
        PersecucionSuperada=ParkourSuperado=ExperimentoCompletado=false;
        Viajar(mostrarIntro ? "IntroCinematica" : "EscenaInicial", "");
    }
}





