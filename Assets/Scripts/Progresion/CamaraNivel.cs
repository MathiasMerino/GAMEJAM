using UnityEngine;

public sealed class CamaraNivel : MonoBehaviour
{
    public bool seguirHorizontal;
    public bool seguirVertical;
    public float minimoY,maximoY,semiancho=8.89f;
    public float minimoX, maximoX;
    public float altura = 5f;
    Transform jugador;
    void Start()
    {
        GetComponent<Camera>().backgroundColor = new Color(0.015f,0.02f,0.025f);
        var p = FindAnyObjectByType<MovimientoPlayer>(); if (p != null) jugador = p.transform;
    }
    void LateUpdate()
    {
        var camara = GetComponent<Camera>();
        camara.orthographicSize = Mathf.Max(altura, semiancho / camara.aspect);
        if (seguirVertical && jugador != null)
        {
            var posicion=transform.position;posicion.y=Mathf.Clamp(jugador.position.y+1f,minimoY,maximoY);transform.position=posicion;
        }
        if (seguirHorizontal && jugador != null)
        {
            var p = transform.position;
            p.x = Mathf.Clamp(jugador.position.x, minimoX, maximoX);
            transform.position = p;
        }
    }
}

