using UnityEngine;

public class BotonJaula : MonoBehaviour
{
    public GameObject jaulaInicial;
    private bool activado = false;

    private void OnMouseDown()
    {
        if (activado) return;

        activado = true;

        if (jaulaInicial != null)
        {
            jaulaInicial.SetActive(false);
            Debug.Log("Jaula desactivada");
        }
    }
}