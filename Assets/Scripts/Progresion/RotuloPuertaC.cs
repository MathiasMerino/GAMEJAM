using UnityEngine;

public sealed class RotuloPuertaC : MonoBehaviour
{
    void Start()
    {
        GetComponent<TextMesh>().text = EstadoPartida.Instancia.CorredorSuperado ? "¿Otra vez?" : "";
    }
}

