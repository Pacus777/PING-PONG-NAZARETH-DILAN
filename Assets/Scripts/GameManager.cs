using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{

    public AudioSource audioSource;
    public AudioClip scoreSound;
    public int scoreP1 = 0;
    public int scoreP2 = 0;

    public int maxScore = 5;
    public TMP_Text winnerText;

    public TMP_Text scoreText;

        private void Start()
    {
        Time.timeScale = 1f;
        ActualizarMarcador();
    }

    public void PuntoJugador1()
    {
        scoreP1++;
        audioSource.PlayOneShot(scoreSound);
        ActualizarMarcador();
    }

    public void PuntoJugador2()
    {
        scoreP2++;
        audioSource.PlayOneShot(scoreSound);
        ActualizarMarcador();
    }

    private void ActualizarMarcador()
    {
        scoreText.text = scoreP1 + "   -   " + scoreP2;

        if (scoreP1 >= maxScore)
        {
            winnerText.text = "¡JUGADOR 1 GANA!";
            Time.timeScale = 0f;
        }
        else if (scoreP2 >= maxScore)
        {
            winnerText.text = "¡JUGADOR 2 GANA!";
            Time.timeScale = 0f;
        }
    }


}