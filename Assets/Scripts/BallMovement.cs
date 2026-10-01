using UnityEngine;

public class BallMovement : MonoBehaviour
{
    public float speed = 7f;

    public AudioClip hitSound;
    public AudioClip wallSound;

    private Rigidbody2D rb;
    private AudioSource audioSource;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        audioSource = GetComponent<AudioSource>();

        LaunchBall();
    }

    void LaunchBall()
    {
        float xDirection = Random.value < 0.5f ? -1f : 1f;

        float yDirection = Random.Range(0.35f, 0.6f);

        if (Random.value < 0.5f)
            yDirection *= -1f;

        Vector2 direction = new Vector2(xDirection, yDirection).normalized;

        rb.linearVelocity = direction * speed;
    }

    public void ResetBall()
    {
        CancelInvoke();

        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        transform.position = Vector2.zero;

        Invoke(nameof(LaunchBall), 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            float hitPoint =
                transform.position.y - collision.transform.position.y;

            float paddleHeight =
                collision.collider.bounds.size.y;

            float yDirection =
                (hitPoint / paddleHeight) * 1.2f;

            // Evita que salga demasiado vertical
            yDirection = Mathf.Clamp(yDirection, -0.75f, 0.75f);

            // Evita que salga totalmente horizontal
            if (Mathf.Abs(yDirection) < 0.2f)
            {
                yDirection = yDirection >= 0 ? 0.2f : -0.2f;
            }

            float xDirection =
                collision.transform.position.x < 0 ? 1f : -1f;

            Vector2 direction =
                new Vector2(xDirection, yDirection).normalized;

            rb.linearVelocity = direction * speed;

            if (hitSound != null && audioSource != null)
                audioSource.PlayOneShot(hitSound);
        }
        else
        {
            // Seguridad extra:
            // después de tocar pared, mantenemos movimiento horizontal suficiente
            Vector2 velocity = rb.linearVelocity;

            if (Mathf.Abs(velocity.x) < speed * 0.45f)
            {
                velocity.x =
                    Mathf.Sign(velocity.x == 0 ? Random.Range(-1f, 1f) : velocity.x)
                    * speed * 0.6f;

                velocity = velocity.normalized * speed;

                rb.linearVelocity = velocity;
            }

            if (wallSound != null && audioSource != null)
                audioSource.PlayOneShot(wallSound);
        }
    }
}