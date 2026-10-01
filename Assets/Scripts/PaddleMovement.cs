using UnityEngine;

public class PaddleMovement : MonoBehaviour
{
    public float speed = 8f;
    public float limitY = 3.7f;

    public KeyCode upKey;
    public KeyCode downKey;

    private Rigidbody2D rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void FixedUpdate()
    {
        float movement = 0f;

        if (Input.GetKey(upKey))
            movement = 1f;

        if (Input.GetKey(downKey))
            movement = -1f;

        rb.linearVelocity = new Vector2(0f, movement * speed);

        Vector2 position = rb.position;

        position.y = Mathf.Clamp(
            position.y,
            -limitY,
            limitY
        );

        rb.position = position;
    }
}