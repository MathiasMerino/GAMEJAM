using UnityEngine;
public sealed class SalidaEtapa : MonoBehaviour
{
    public string destino;
    public string entrada="Inicio";
    public bool terminaPersecucion;
    void OnTriggerEnter2D(Collider2D other)
    {
        if(other.GetComponent<MovimientoPlayer>()==null)return;
        if(terminaPersecucion)EstadoPartida.Instancia.PersecucionSuperada=true;
        EstadoPartida.Instancia.Viajar(destino,entrada);
    }
}
