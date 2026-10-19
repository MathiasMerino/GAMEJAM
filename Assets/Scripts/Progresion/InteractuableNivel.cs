using UnityEngine;

public abstract class InteractuableNivel : MonoBehaviour, IInteractuable
{
    public string indicacion = "Interactuar";
    public bool permiteClic;
    public Vector2 tamanoClic = new Vector2(0.65f,0.65f);
    public bool AceptaClic(Vector2 punto)
    {
        Vector2 distancia = punto - (Vector2)transform.position;
        return Mathf.Abs(distancia.x) <= tamanoClic.x * 0.5f && Mathf.Abs(distancia.y) <= tamanoClic.y * 0.5f;
    }
    public virtual string Indicacion => indicacion;
    public abstract void Interactuar();
}
