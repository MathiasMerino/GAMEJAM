using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Linq;
using UnityEngine.InputSystem;

public sealed class InterfazPartida : MonoBehaviour
{
    static InterfazPartida actual;
    Text inventario, indicacion, mensaje, lectura;
    GameObject raiz, panelLectura;
    float segundosMensaje;
    public static string TextoActual => actual != null && actual.segundosMensaje > 0 ? actual.mensaje.text : "";
    static readonly Color Tinta = new Color(0.055f, 0.07f, 0.08f, 0.94f);

    void Awake()
    {
        actual = this;
        raiz = new GameObject("HUD", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler));
        raiz.transform.SetParent(transform, false);
        var canvas = raiz.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        var scaler = raiz.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1600, 900);
        scaler.matchWidthOrHeight = 0.5f;
        inventario = Texto("Inventario", new Vector2(0,1), new Vector2(0,1), new Vector2(24,-18), new Vector2(730,42), 22, TextAnchor.MiddleLeft);
        indicacion = Texto("Interaccion", new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0,30), new Vector2(1000,48), 24, TextAnchor.MiddleCenter);
        mensaje = Texto("Mensaje", new Vector2(0.5f,0), new Vector2(0.5f,0), new Vector2(0,34), new Vector2(1160,100), 28, TextAnchor.MiddleCenter);
        lectura = Texto("Lectura", new Vector2(0.5f,0.5f), new Vector2(0.5f,0.5f), Vector2.zero, new Vector2(1000,440), 28, TextAnchor.MiddleCenter);
        panelLectura = lectura.transform.parent.gameObject;
        panelLectura.SetActive(false);
    }

    Text Texto(string nombre, Vector2 ancla, Vector2 pivote, Vector2 posicion, Vector2 tamano, int fuente, TextAnchor alineacion)
    {
        var panel = new GameObject(nombre, typeof(RectTransform), typeof(Image));
        panel.transform.SetParent(raiz.transform, false);
        var rect = panel.GetComponent<RectTransform>();
        rect.anchorMin = rect.anchorMax = ancla;
        rect.pivot = pivote; rect.anchoredPosition = posicion; rect.sizeDelta = tamano;
        panel.GetComponent<Image>().color = nombre == "Lectura" ? Tinta : nombre == "Mensaje" ? new Color(.025f,.035f,.04f,.78f) : Color.clear;
        panel.GetComponent<Image>().raycastTarget = false;
        var go = new GameObject("Texto", typeof(RectTransform), typeof(Text));
        go.transform.SetParent(panel.transform, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = Vector2.zero; r.anchorMax = Vector2.one;
        r.offsetMin = new Vector2(14,6); r.offsetMax = new Vector2(-14,-6);
        var t = go.GetComponent<Text>();
        t.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        t.fontSize = fuente; t.alignment = alineacion; t.raycastTarget = false;
        t.color = new Color(0.96f, 0.87f, 0.64f);
        var sombra = go.AddComponent<Shadow>(); sombra.effectColor = new Color(0,0,0,0.9f); sombra.effectDistance = new Vector2(1,-2);
        return t;
    }

    void Update()
    {
        var e = EstadoPartida.Instancia;
        bool jugando = SceneManager.GetActiveScene().name != "Menuprincipal";
        raiz.SetActive(jugando);
        if (!jugando || e == null) return;
        inventario.text = "INVENTARIO   " + (e.Objetos.Any() ? string.Join("  ", e.Objetos.Select(id => "[ " + (id == EstadoPartida.Fusible ? "Fusible" : id == EstadoPartida.LlaveMaestra ? "Llave maestra" : id) + " ]")) : "Vacío");
        inventario.transform.parent.gameObject.SetActive(Keyboard.current != null && Keyboard.current.tabKey.isPressed && Time.timeScale > 0f);
        if (Time.timeScale > 0f && !panelLectura.activeSelf) segundosMensaje = Mathf.Max(0, segundosMensaje - Time.unscaledDeltaTime);
        mensaje.transform.parent.gameObject.SetActive(segundosMensaje > 0 && Time.timeScale > 0f && !panelLectura.activeSelf);
        indicacion.transform.parent.gameObject.SetActive(false);
    }

    public static void Indicar(string texto) { if (actual != null) actual.indicacion.text = texto; }
    public static void Mensaje(string texto, float segundos = 6.5f)
    {
        if (actual == null) return;
        actual.mensaje.text = texto;
        actual.segundosMensaje = segundos;
        actual.mensaje.transform.parent.gameObject.SetActive(Time.timeScale > 0f);
    }
    public static void Leer(string texto, bool indicarCierre = true)
    {
        if (actual == null) return;
        actual.lectura.text = texto + (indicarCierre ? "\n\n[E o clic para cerrar]" : "");
        actual.panelLectura.SetActive(true);
    }
    public static void CerrarLectura() { if (actual != null) actual.panelLectura.SetActive(false); }
    public static void AlCambiarEscena()
    {
        Indicar(""); CerrarLectura();
        // Las confirmaciones de objetos no desaparecen al cruzar inmediatamente una puerta.
    }
    public static void LimpiarMensajes() { if (actual != null) actual.segundosMensaje = 0; }
    void OnDestroy() { if (actual == this) actual = null; }
}



