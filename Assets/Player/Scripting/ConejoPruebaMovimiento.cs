using UnityEngine;
using UnityEngine.InputSystem;

public class ConejoPruebaMovimiento : MonoBehaviour
{
    [Header("Movimiento del conejo")]
    public float Velocidad = 5f;
    public float FuerzaSalto = 8f;

    [Header("Deteccion del suelo")]
    public Transform PuntoSuelo;
    public float RadioSuelo = 0.15f;
    public LayerMask CapaSuelo;

    [Header("Animaciones")]
    public Animator animator;
    public Transform VisualConejo;

    private Rigidbody2D rb;
    private float movimiento;
    private bool enSuelo;
    private bool saltoPendiente;
    private bool saltando;
    private bool estabaEnSuelo;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (VisualConejo == null)
            Debug.LogWarning("Asigna VisualConejo.");

        if (PuntoSuelo == null)
            Debug.LogWarning("Asigna PuntoSuelo.");
    }

    void Update()
    {
        if (rb == null)
            return;

        // Detectar suelo
        if (PuntoSuelo != null)
        {
            enSuelo = Physics2D.OverlapCircle(
                PuntoSuelo.position,
                RadioSuelo,
                CapaSuelo
            ) != null;
        }

        // Detectar aterrizaje
        if (enSuelo && !estabaEnSuelo)
        {
            saltando = false;
        }

        estabaEnSuelo = enSuelo;

        if (Keyboard.current != null)
        {
            movimiento = 0f;

            // Caminar izquierda
            if (Keyboard.current.aKey.isPressed ||
                Keyboard.current.leftArrowKey.isPressed)
            {
                movimiento = -1f;
            }

            // Caminar derecha
            if (Keyboard.current.dKey.isPressed ||
                Keyboard.current.rightArrowKey.isPressed)
            {
                movimiento = 1f;
            }

            // Girar todo el conejo
            if (VisualConejo != null)
            {
                Vector3 escala = VisualConejo.localScale;

                if (movimiento > 0)
                    escala.x = Mathf.Abs(escala.x);

                if (movimiento < 0)
                    escala.x = -Mathf.Abs(escala.x);

                VisualConejo.localScale = escala;
            }

            // Saltar
            bool presionoSalto =
                Keyboard.current.spaceKey.wasPressedThisFrame ||
                Keyboard.current.wKey.wasPressedThisFrame ||
                Keyboard.current.upArrowKey.wasPressedThisFrame;

            if (presionoSalto && enSuelo && !saltoPendiente)
            {
                saltoPendiente = true;
                saltando = true;
            }
        }

        // Actualizar animaciones
        if (animator != null)
        {
            animator.SetFloat(
                "Velocidad",
                Mathf.Abs(movimiento)
            );

            animator.SetBool("EnSuelo", enSuelo);

            animator.SetFloat(
                "VelocidadY",
                rb.linearVelocity.y
            );

            animator.SetBool("Saltando", saltando);
        }
    }

    void FixedUpdate()
    {
        if (rb == null)
            return;

        // Movimiento horizontal
        rb.linearVelocity = new Vector2(
            movimiento * Velocidad,
            rb.linearVelocity.y
        );

        // Aplicar salto
        if (saltoPendiente)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                0f
            );

            rb.AddForce(
                Vector2.up * FuerzaSalto,
                ForceMode2D.Impulse
            );

            saltoPendiente = false;
        }
    }

    void OnDrawGizmosSelected()
    {
        if (PuntoSuelo != null)
        {
            Gizmos.color = Color.green;

            Gizmos.DrawWireSphere(
                PuntoSuelo.position,
                RadioSuelo
            );
        }
    }
}