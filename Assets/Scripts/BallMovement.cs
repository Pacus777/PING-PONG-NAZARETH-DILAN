using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float speed = 7f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        LaunchBall();
    }

    void LaunchBall()
    {
        float xDirection = Random.value < 0.5f ? -1f : 1f;
        float yDirection = Random.Range(-0.6f, 0.6f);

        Vector2 direction = new Vector2(xDirection, yDirection).normalized;

        rb.linearVelocity = direction * speed;
    }
}