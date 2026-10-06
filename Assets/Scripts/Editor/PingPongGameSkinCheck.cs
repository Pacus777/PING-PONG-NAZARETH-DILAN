using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PingPongGameSkinCheck
{
    const string Key="PingPongSkinCheck.";
    const string Report="C:/Users/HP/Documents/Codex/2026-10-05/tengo-actualmente-un-proyecto-unity-a/work/game_skin_runtime_check.txt";
    static PingPongGameSkinCheck() { EditorApplication.update+=Tick; }
    [MenuItem("Tools/Ping Pong/Diseno/Verificar juego con diseno %#&v")]
    public static void Start()
    {
        if(EditorApplication.isPlaying||SceneManager.GetActiveScene().isDirty||SceneManager.GetActiveScene().name!="SampleScene") throw new Exception("Abre y guarda SampleScene antes de verificar.");
        File.WriteAllText(Report,"Game visual integration\n");
        SessionState.SetBool(Key+"active",true); SessionState.SetInt(Key+"stage",0); SessionState.SetInt(Key+"digit",0); SessionState.SetInt(Key+"player",1);
        EditorApplication.isPlaying=true;
    }
    static void Next(int stage,float delay) { SessionState.SetInt(Key+"stage",stage); SessionState.SetFloat(Key+"next",(float)EditorApplication.timeSinceStartup+delay); }
    static void Check(bool pass,string text) { File.AppendAllText(Report,(pass?"PASS ":"FAIL ")+text+"\n"); }
    static void Tick()
    {
        if(!SessionState.GetBool(Key+"active",false)||!EditorApplication.isPlaying||EditorApplication.isCompiling||EditorApplication.timeSinceStartup<SessionState.GetFloat(Key+"next",0)) return;
        try
        {
            int stage=SessionState.GetInt(Key+"stage",0);
            if(stage==0) { Next(1,1f); return; }
            var skin=UnityEngine.Object.FindFirstObjectByType<PingPongGameSkin>();
            var manager=skin.gameManager;
            var ball=UnityEngine.Object.FindFirstObjectByType<BallMovement>();
            var rb=ball.GetComponent<Rigidbody2D>();
            if(stage==1)
            {
                Check(skin.player1Image.sprite!=null&&skin.player2Image.sprite!=null&&skin.ballImage.sprite!=null,"Player and ball artwork loaded");
                Check(skin.player1Image.GetComponent<Collider2D>()==null&&skin.player2Image.GetComponent<Collider2D>()==null&&skin.ballImage.GetComponent<Collider2D>()==null,"Artwork has no extra colliders");
                rb.position=Vector2.zero; rb.linearVelocity=Vector2.zero; Next(2,.2f); return;
            }
            if(stage==2)
            {
                int digit=SessionState.GetInt(Key+"digit",0), player=SessionState.GetInt(Key+"player",1);
                var shown=player==1?skin.player1Score.sprite:skin.player2Score.sprite;
                var expected=player==1?skin.player1Digits[digit]:skin.player2Digits[digit];
                Check(shown==expected,"Player "+player+" sprite displays score "+digit);
                if(digit<manager.maxScore)
                {
                    if(player==1) manager.PuntoJugador1(); else manager.PuntoJugador2();
                    SessionState.SetInt(Key+"digit",digit+1); Next(2,.2f); return;
                }
                Check(skin.victoryPanel.activeSelf&&skin.victoryLabel.text==manager.winnerText.text&&Time.timeScale==0,"Winner presentation matches existing game state for player "+player);
                if(player==1)
                {
                    SceneManager.LoadScene("SampleScene"); SessionState.SetInt(Key+"player",2); SessionState.SetInt(Key+"digit",0); Next(1,.5f); return;
                }
                SessionState.SetBool(Key+"active",false);
                EditorApplication.isPlaying=false;
                Debug.Log("PINGPONG_SKIN_CHECK: "+File.ReadAllText(Report));
            }
        }
        catch(Exception error)
        { File.AppendAllText(Report,"ERROR "+error+"\n"); SessionState.SetBool(Key+"active",false); EditorApplication.isPlaying=false; Debug.LogException(error); }
    }
}
