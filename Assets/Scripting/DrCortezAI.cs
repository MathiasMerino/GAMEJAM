using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class DrCortezAI : MonoBehaviour
{
    [Header("Patrullaje")]
    public float Velocidad = 1.5f;
    public float DistanciaPatrulla = 3f;

    [Header("Deteccion del conejo")]
    public float RadioDeteccion = 4f;
    public float VelocidadPersecucion = 2.5f;
    public float DistanciaCaptura = 0.6f;

    [Header("Personaje visual")]
    public Transform VisualCortez;

    [Header("Linterna")]
    public Transform Linterna;

    [Header("Busqueda")]
    public float TiempoBusqueda = 3f;

    [Header("Deteccion de paredes y tuberias")]
    public LayerMask CapaObstaculos;
    public LayerMask CapaBloqueoVision;
    public float DistanciaDeteccionPared = 0.7f;
    public Vector2 OffsetDetector = Vector2.zero;
    public float PausaAlGirar = 0.4f;

    private Rigidbody2D rb;
    private Transform conejo;

    private Vector2 posicionInicial;
    private Vector3 posicionLinterna;
    private Vector3 escalaOriginalVisual;

    private int direccion = 1;
    private int direccionBusquedaInicial = 1;

    private bool persiguiendo = false;
    private bool buscando = false;
    private bool capturado = false;

    private float tiempoRestanteBusqueda = 0f;
    private float tiempoPausa = 0f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        posicionInicial = rb.position;

        GameObject jugador =
            GameObject.FindGameObjectWithTag("Player");

        if (jugador != null)
        {
            conejo = jugador.transform;
        }
        else
        {
            Debug.LogWarning(
                "Dr. Cortez: No se encontro un objeto con Tag Player."
            );
        }

        if (Linterna != null)
        {
            posicionLinterna = Linterna.localPosition;
        }

        if (VisualCortez != null)
        {
            escalaOriginalVisual = VisualCortez.localScale;
        }
        else
        {
            Debug.LogWarning(
                "Dr. Cortez: Asigna VisualCortez en el Inspector."
            );
        }

        ActualizarOrientacion();
    }

    void FixedUpdate()
    {
        if (capturado)
            return;

        if (tiempoPausa > 0f)
        {
            tiempoPausa -= Time.fixedDeltaTime;
            ActualizarOrientacion();
            return;
        }

        bool detectado = PuedeVerConejo();

        if (conejo != null)
        {
            float distancia = Vector2.Distance(
                rb.position,
                conejo.position
            );

            if (persiguiendo &&
                distancia <= DistanciaCaptura &&
                !HayObstaculoEntreConejo())
            {
                CapturarConejo();
                return;
            }
        }

        if (detectado)
        {
            persiguiendo = true;
            buscando = false;
            tiempoRestanteBusqueda = TiempoBusqueda;
        }
        else if (persiguiendo)
        {
            persiguiendo = false;
            buscando = true;

            tiempoRestanteBusqueda = TiempoBusqueda;
            direccionBusquedaInicial = direccion;

            Debug.Log(
                "Dr. Cortez perdio de vista al conejo"
            );
        }

        if (persiguiendo)
        {
            Perseguir();
        }
        else if (buscando)
        {
            BuscarConejo();
        }
        else
        {
            Patrullar();
        }

        ActualizarOrientacion();
    }

    void Patrullar()
    {
        float distancia =
            rb.position.x - posicionInicial.x;

        if (distancia >= DistanciaPatrulla)
        {
            direccion = -1;
        }
        else if (distancia <= -DistanciaPatrulla)
        {
            direccion = 1;
        }

        if (HayParedDelante())
        {
            direccion *= -1;
            tiempoPausa = PausaAlGirar;

            Debug.Log(
                "Dr. Cortez detecto una pared y cambio de direccion"
            );

            return;
        }

        MoverHorizontal(Velocidad);
    }

    void Perseguir()
    {
        if (conejo == null)
            return;

        float diferenciaX =
            conejo.position.x - rb.position.x;

        if (Mathf.Abs(diferenciaX) < 0.05f)
            return;

        direccion = diferenciaX > 0f ? 1 : -1;

        if (HayParedDelante())
        {
            persiguiendo = false;
            buscando = true;

            tiempoRestanteBusqueda = TiempoBusqueda;
            direccionBusquedaInicial = direccion;

            Debug.Log(
                "Dr. Cortez encontro un obstaculo durante la persecucion"
            );

            return;
        }

        MoverHorizontal(VelocidadPersecucion);
    }

    void BuscarConejo()
    {
        tiempoRestanteBusqueda -= Time.fixedDeltaTime;

        if (tiempoRestanteBusqueda <= 0f)
        {
            buscando = false;
            direccion = direccionBusquedaInicial;

            Debug.Log(
                "Dr. Cortez termina la busqueda y vuelve a patrullar"
            );

            return;
        }

        if (tiempoRestanteBusqueda > TiempoBusqueda / 2f)
        {
            direccion = direccionBusquedaInicial;
        }
        else
        {
            direccion = -direccionBusquedaInicial;
        }
    }

    bool PuedeVerConejo()
    {
        if (conejo == null)
            return false;

        Vector2 origen =
            rb.position + OffsetDetector;

        Vector2 destino = conejo.position;

        Vector2 haciaConejo = destino - origen;
        float distancia = haciaConejo.magnitude;

        if (distancia > RadioDeteccion ||
            distancia < 0.001f)
        {
            return false;
        }

        // Solo detecta al conejo que esta delante.
        if (haciaConejo.x * direccion <= 0f)
            return false;

        // Si hay una pared entre ambos, no lo ve.
        return !HayObstaculoEntreConejo();
    }

    bool HayObstaculoEntreConejo()
    {
        if (conejo == null)
            return false;

        Vector2 origen =
            rb.position + OffsetDetector;

        Vector2 haciaConejo =
            (Vector2)conejo.position - origen;

        float distancia = haciaConejo.magnitude;

        if (distancia < 0.001f)
            return false;

        RaycastHit2D impacto = Physics2D.Raycast(
            origen,
            haciaConejo.normalized,
            distancia,
            CapaBloqueoVision
        );

        return impacto.collider != null;
    }

    bool HayParedDelante()
    {
        Vector2 origen =
            rb.position + OffsetDetector;

        Vector2 direccionRayo =
            Vector2.right * direccion;

        RaycastHit2D impacto = Physics2D.Raycast(
            origen,
            direccionRayo,
            DistanciaDeteccionPared,
            CapaObstaculos
        );

        return impacto.collider != null;
    }

    void MoverHorizontal(float velocidad)
    {
        Vector2 nuevaPosicion =
            rb.position +
            Vector2.right *
            direccion *
            velocidad *
            Time.fixedDeltaTime;

        rb.MovePosition(nuevaPosicion);
    }

    void CapturarConejo()
    {
        capturado = true;
        persiguiendo = false;
        buscando = false;

        Debug.Log("Dr. Cortez capturo al conejo");

        // Mas adelante conectaremos la captura
        // con el sistema de jaulas.
    }

    // NUEVO: actualiza el cuerpo y la linterna.
    void ActualizarOrientacion()
    {
        GirarPersonaje();
        GirarLinterna();
    }

    // NUEVO: gira todo el cuerpo hacia la direccion
    // de patrulla o persecucion.
    void GirarPersonaje()
    {
        if (VisualCortez == null)
            return;

        Vector3 escala = escalaOriginalVisual;

        // Derecha = X positivo.
        // Izquierda = X negativo.
        escala.x =
            Mathf.Abs(escalaOriginalVisual.x) * direccion;

        VisualCortez.localScale = escala;
    }

    void GirarLinterna()
    {
        if (Linterna == null)
            return;

        Linterna.localPosition = new Vector3(
            Mathf.Abs(posicionLinterna.x) * direccion,
            posicionLinterna.y,
            posicionLinterna.z
        );

        Linterna.localRotation = Quaternion.Euler(
            0f,
            direccion == 1 ? 0f : 180f,
            0f
        );
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(
            transform.position,
            RadioDeteccion
        );

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(
            transform.position,
            DistanciaCaptura
        );

        Gizmos.color = Color.green;

        Vector3 origen =
            transform.position +
            new Vector3(
                OffsetDetector.x,
                OffsetDetector.y,
                0f
            );

        Gizmos.DrawLine(
            origen,
            origen +
            Vector3.right *
            direccion *
            DistanciaDeteccionPared
        );

        if (conejo != null)
        {
            Gizmos.color = HayObstaculoEntreConejo()
                ? Color.red
                : Color.cyan;

            Gizmos.DrawLine(
                origen,
                conejo.position
            );
        }
    }
}