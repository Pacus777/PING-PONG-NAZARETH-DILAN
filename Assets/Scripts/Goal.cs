using UnityEngine;

public class Goal : MonoBehaviour
{
    public bool isLeftGoal;
    public GameManager gameManager;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Ball"))
            return;

        if (isLeftGoal)
        {
            gameManager.PuntoJugador2();
        }
        else
        {
            gameManager.PuntoJugador1();
        }

        other.GetComponent<BallMovement>().ResetBall();
    }
}