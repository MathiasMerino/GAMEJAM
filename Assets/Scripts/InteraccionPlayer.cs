using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class InteraccionPlayer : MonoBehaviour
{
    readonly HashSet<Collider2D> cercanos = new HashSet<Collider2D>();

    void Update()
    {
        if (Time.timeScale == 0f || EstadoPartida.Instancia.Cargando || NotaInteractuable.UltimoFrameCierre == Time.frameCount) return;
        cercanos.RemoveWhere(c => c == null || !c.enabled || !c.gameObject.activeInHierarchy);
        IInteractuable actual = null;
        float distancia = float.PositiveInfinity;
        foreach (var c in cercanos)
        {
            var candidato = c.GetComponent<IInteractuable>();
            float d = ((Vector2)c.bounds.center - (Vector2)transform.position).sqrMagnitude;
            if (candidato != null && d < distancia) { actual = candidato; distancia = d; }
        }
        if (actual != null && Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        { actual.Interactuar(); return; }
        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame && Camera.main != null)
        {
            var punto = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
            foreach (var c in Physics2D.OverlapPointAll(punto))
            {
                var boton = c.GetComponent<InteractuableNivel>();
                if (boton != null && boton.permiteClic && boton.AceptaClic(punto)) { boton.Interactuar(); break; }
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<IInteractuable>() != null) cercanos.Add(other);
    }

    private void OnTriggerStay2D(Collider2D other) { OnTriggerEnter2D(other); }

    private void OnTriggerExit2D(Collider2D other)
    {
        cercanos.Remove(other);
    }
    void OnDisable() { cercanos.Clear(); InterfazPartida.Indicar(""); }
}
