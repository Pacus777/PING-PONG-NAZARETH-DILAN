using UnityEngine;



public class BallMovement : MonoBehaviour
{

    public AudioClip hitSound;
    public AudioClip wallSound;

    private AudioSource audioSource;
    public float speed = 7f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        LaunchBall();

        audioSource = GetComponent<AudioSource>();
    }

    void LaunchBall()
    {
        float xDirection = Random.value < 0.5f ? -1f : 1f;
        float yDirection = Random.Range(-0.6f, 0.6f);

        Vector2 direction = new Vector2(xDirection, yDirection).normalized;

        rb.linearVelocity = direction * speed;
    }

    public void ResetBall()
    {
        rb.linearVelocity = Vector2.zero;
        transform.position = Vector2.zero;

        Invoke(nameof(LaunchBall), 1f);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Paddle"))
        {
            audioSource.PlayOneShot(hitSound);
        }
        else
        {
            audioSource.PlayOneShot(wallSound);
        }
    }
}