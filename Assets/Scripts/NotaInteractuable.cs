using UnityEngine;
using UnityEngine.InputSystem;

public class NotaInteractuable : MonoBehaviour, IInteractuable
{
    public GameObject imagenNota;

    private bool abierta = false;
    private int frameApertura;

    public void Interactuar()
    {
        if (abierta) return;

        abierta = true;
        frameApertura = Time.frameCount;

        imagenNota.SetActive(true);
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
        abierta = false;
        imagenNota.SetActive(false);
        Time.timeScale = 1f;
    }
}