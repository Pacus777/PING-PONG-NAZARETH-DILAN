using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PingPongMenuUI : MonoBehaviour
{
    public GameObject dialog;
    public TMP_Text heading;
    public TMP_Text body;
    public GameObject volumeControls;
    public Slider volumeSlider;
    private void Start()
    {
        if (volumeSlider != null) { volumeSlider.GetComponent<Image>().raycastTarget = true; volumeSlider.SetValueWithoutNotify(AudioListener.volume); }
    }
    public void Options() { Show("OPTIONS", "Volumen general", true); }
    public void HowToPlay() { Show("HOW TO PLAY", "Dos jugadores, una pelota.\n\nMueve tu paleta para devolver la pelota y evita que cruce tu lado de la cancha.\n\nJugador 1: W / S\nJugador 2: flechas arriba / abajo", false); }
    public void Credits() { Show("CREDITS", "PING PONG\n\nNazareth & Dilan", false); }
    public void Close() { dialog.SetActive(false); }
    public void SetVolume(float value) { AudioListener.volume = value; }
    private void Show(string title, string text, bool options)
    {
        heading.text = title;
        body.text = text;
        volumeControls.SetActive(options);
        dialog.SetActive(true);
    }
}

