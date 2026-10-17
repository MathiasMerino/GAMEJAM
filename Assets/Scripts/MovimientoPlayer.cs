using UnityEngine;
using UnityEngine.InputSystem;

public class MovimientoPlayer : MonoBehaviour
{
    public float velocidad = 4f;
    private Rigidbody2D rb;
    private float movimiento;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        movimiento = 0f;

        if (Keyboard.current == null) return;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed) movimiento = -1f;
        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed) movimiento = 1f;
    }

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(movimiento * velocidad, rb.linearVelocity.y);
    }
}