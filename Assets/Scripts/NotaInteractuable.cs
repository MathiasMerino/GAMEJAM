using UnityEngine;
using UnityEngine.InputSystem;

public class NotaInteractuable : MonoBehaviour, IInteractuable
{
    public static int UltimoFrameCierre { get; private set; } = -1;
    public GameObject imagenNota;
    [TextArea(3, 10)] public string textoNota;

    private bool abierta = false;
    private int frameApertura;
    private float tiempoAnterior = 1f;

    public void Interactuar()
    {
        if (abierta) return;

        abierta = true;
        frameApertura = Time.frameCount;

        if (imagenNota != null) imagenNota.SetActive(true);
        if (imagenNota == null && !string.IsNullOrEmpty(textoNota)) InterfazPartida.Leer(textoNota);
        tiempoAnterior = Time.timeScale;
        Time.timeScale = 0f;
    }

    void Update()
    {
        if (!abierta) return;

        if (Time.frameCount <= frameApertura) return;

        bool presionoE =
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame;

        bool hizoClick =
            Mouse.current != null &&
            Mouse.current.leftButton.wasPressedThisFrame;

        if (presionoE || hizoClick)
        {
            CerrarNota();
        }
    }

    void CerrarNota()
    {
        UltimoFrameCierre = Time.frameCount;
        abierta = false;
        if (imagenNota != null) imagenNota.SetActive(false);
        InterfazPartida.CerrarLectura();
        Time.timeScale = tiempoAnterior;
    }
    void OnDisable() { if (abierta) CerrarNota(); }
}
