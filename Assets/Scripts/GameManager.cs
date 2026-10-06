using UnityEngine;
using TMPro;

public class GameManager : MonoBehaviour
{
    public AudioSource audioSource;
    public AudioClip scoreSound;
    public AudioClip winSound;
    public int scoreP1 = 0;
    public int scoreP2 = 0;
    public int maxScore = 5;
    public TMP_Text winnerText;
    public TMP_Text scoreText;
    private bool gameOver;

    private void Start()
    {
        Time.timeScale = 1f;
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        if (winnerText != null) winnerText.text = "";
        ActualizarMarcador();
    }
    public void PuntoJugador1() { Punto(true); }
    public void PuntoJugador2() { Punto(false); }
    private void Punto(bool playerOne)
    {
        if (gameOver) return;
        if (playerOne) scoreP1++; else scoreP2++;
        bool won = scoreP1 >= maxScore || scoreP2 >= maxScore;
        // The final point uses the victory cue instead of overlapping two effects.
        AudioClip cue = won && winSound != null ? winSound : scoreSound;
        if (audioSource != null && cue != null) audioSource.PlayOneShot(cue);
        ActualizarMarcador();
    }
    private void ActualizarMarcador()
    {
        if (scoreText != null) scoreText.text = scoreP1 + "   -   " + scoreP2;
        if (scoreP1 < maxScore && scoreP2 < maxScore) return;
        gameOver = true;
        if (winnerText != null) winnerText.text = scoreP1 >= maxScore ? "¡JUGADOR 1 GANA!" : "¡JUGADOR 2 GANA!";
        Time.timeScale = 0f;
    }
}
