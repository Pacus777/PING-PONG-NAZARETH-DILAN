using UnityEngine;
using UnityEngine.UI;
using TMPro;

// Presentation only: reads the existing game state and never writes to physics or scores.
[ExecuteAlways]
public class PingPongGameSkin : MonoBehaviour
{
    public Camera gameCamera;
    public GameManager gameManager;
    public Collider2D player1;
    public Collider2D player2;
    public Collider2D ball;
    public Image player1Image;
    public Image player2Image;
    public Image ballImage;
    public Image player1Score;
    public Image player2Score;
    public Sprite[] player1Digits;
    public Sprite[] player2Digits;
    public GameObject victoryPanel;
    public TMP_Text victoryLabel;
    public TMP_Text scoreFallback;
    private int lastP1 = int.MinValue;
    private int lastP2 = int.MinValue;

    private void LateUpdate()
    {
        if (gameCamera == null || gameManager == null) return;
        // Decorative horns, tail and ribbons surround the original collision rectangles.
        Follow(player1, player1Image, new Rect(265f/768f,64f/1681f,360f/768f,1380f/1681f));
        Follow(player2, player2Image, new Rect(245f/785f,155f/1626f,360f/785f,1175f/1626f));
        Follow(ball, ballImage, new Rect(.20f,.19f,.64f,.64f));
        SetScore(player1Score,player1Digits,gameManager.scoreP1,ref lastP1);
        SetScore(player2Score,player2Digits,gameManager.scoreP2,ref lastP2);
        bool supported = gameManager.scoreP1 >= 0 && gameManager.scoreP1 < player1Digits.Length && gameManager.scoreP2 >= 0 && gameManager.scoreP2 < player2Digits.Length;
        player1Score.gameObject.SetActive(supported);
        player2Score.gameObject.SetActive(supported);
        if (scoreFallback != null) { scoreFallback.gameObject.SetActive(!supported); scoreFallback.text = gameManager.scoreP1 + "  -  " + gameManager.scoreP2; }
        string winner = gameManager.winnerText != null ? gameManager.winnerText.text : "";
        bool showWinner = !string.IsNullOrEmpty(winner);
        if (victoryPanel.activeSelf != showWinner) victoryPanel.SetActive(showWinner);
        if (victoryLabel.text != winner) victoryLabel.text = winner;
    }
    private void Follow(Collider2D owner, Image image, Rect core)
    {
        if (owner == null || image == null) return;
        var parent = (RectTransform)image.transform.parent;
        Vector3 point = gameCamera.WorldToViewportPoint(owner.bounds.center);
        var rect = image.rectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(point.x,point.y);
        float viewHeight = gameCamera.orthographicSize * 2;
        float viewWidth = viewHeight * gameCamera.aspect;
        Vector2 size = new Vector2(owner.bounds.size.x/viewWidth*parent.rect.width/core.width,owner.bounds.size.y/viewHeight*parent.rect.height/core.height);
        rect.sizeDelta=size;
        rect.anchoredPosition=new Vector2((.5f-core.center.x)*size.x,(.5f-core.center.y)*size.y);
    }
    private void SetScore(Image image, Sprite[] digits, int score, ref int previous)
    {
        if (score < 0 || score >= digits.Length) return;
        if (previous != score || image.sprite != digits[score]) { image.sprite=digits[score]; previous=score; }
    }
}
