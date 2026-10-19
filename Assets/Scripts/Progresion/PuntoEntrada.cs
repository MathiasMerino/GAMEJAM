using UnityEngine;

public sealed class PuntoEntrada : MonoBehaviour
{
    public string id;
    void OnDrawGizmos() { Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, 0.25f); }
}
