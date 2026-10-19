using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPlayer : MonoBehaviour
{
    public float velocidad = 4f;
    public float fuerzaSalto = 7f;
    public Transform groundCheck;
    public LayerMask capaSuelo;
    public float radioSuelo = 0.15f;

    private Rigidbody2D rb;
    private float movimiento;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movimiento = 0f;

        if (Keyboard.current == null || Time.timeScale == 0f) return;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            movimiento = -1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            movimiento = 1f;

        bool tocandoSuelo = groundCheck != null && Physics2D.OverlapCircle(
            groundCheck.position,
            radioSuelo,
            capaSuelo
        );

        if (Keyboard.current.spaceKey.wasPressedThisFrame && tocandoSuelo)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, fuerzaSalto);
        }
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(
            movimiento * velocidad,
            rb.linearVelocity.y
        );
    }
}
