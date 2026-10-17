using UnityEngine;
using UnityEngine.InputSystem;

public class InteraccionPlayer : MonoBehaviour
{
    private IInteractuable interactuableActual;

    void Update()
    {
        if (interactuableActual != null &&
            Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            interactuableActual.Interactuar();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        IInteractuable interactuable = other.GetComponent<IInteractuable>();

        if (interactuable != null)
        {
            interactuableActual = interactuable;
            Debug.Log("Presiona E para interactuar");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        IInteractuable interactuable = other.GetComponent<IInteractuable>();

        if (interactuable != null && interactuable == interactuableActual)
        {
            interactuableActual = null;
        }
    }
}