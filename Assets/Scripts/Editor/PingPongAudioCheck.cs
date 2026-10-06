using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class PingPongAudioCheck
{
    const string Report = "C:/Users/HP/Documents/Codex/2026-10-05/tengo-actualmente-un-proyecto-unity-a/work/audio_runtime_check.txt";
    const string Key = "PingPongAudioCheck.";
    static PingPongAudioCheck() { EditorApplication.update += Tick; }
    [MenuItem("Tools/Ping Pong/Audio/3 Verificar reproduccion %#&t")]
    public static void Start()
    {
        if (EditorApplication.isPlaying) throw new Exception("Sal del modo Play antes de verificar.");
        if (SceneManager.GetActiveScene().isDirty) throw new Exception("Guarda la escena actual.");
        EditorSceneManager.OpenScene("Assets/Scenes/menu.unity");
        File.WriteAllText(Report,"Audio runtime check\n");
        SessionState.SetBool(Key+"active",true);
        SessionState.SetInt(Key+"stage",0);
        EditorApplication.isPlaying=true;
    }
    static void Next(int stage,float delay)
    { SessionState.SetInt(Key+"stage",stage); SessionState.SetFloat(Key+"next",(float)EditorApplication.timeSinceStartup+delay); }
    static void Check(bool ok,string message)
    { File.AppendAllText(Report,(ok?"PASS ":"FAIL ")+message+"\n"); }
    static void Tick()
    {
        if (!SessionState.GetBool(Key+"active",false) || !EditorApplication.isPlaying || EditorApplication.isCompiling) return;
        if (EditorApplication.timeSinceStartup<SessionState.GetFloat(Key+"next",0)) return;
        try
        {
            int stage=SessionState.GetInt(Key+"stage",0);
            if(stage==0) { Next(1,1.5f); return; }
            if(stage==1)
            {
                var music=GameObject.Find("MusicaMenu").GetComponent<AudioSource>();
                Check(music.isPlaying&&music.loop&&music.clip!=null,"Menu music plays and loops");
                Check(UnityEngine.Object.FindObjectsByType<AudioListener>(FindObjectsSortMode.None).Count(a=>a.enabled)==1,"Menu has one AudioListener");
                SceneManager.LoadScene("SampleScene"); Next(2,1f); return;
            }
            var ball=UnityEngine.Object.FindFirstObjectByType<BallMovement>();
            var manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
            var source=ball.GetComponent<AudioSource>();
            var rb=ball.GetComponent<Rigidbody2D>();
            if(stage==2)
            {
                Check(GameObject.Find("MusicaMenu")==null,"Menu music stops when entering game");
                Check(ball.hitSound!=null&&ball.wallSound!=null&&manager.scoreSound!=null&&manager.winSound!=null,"All four game cues assigned");
                var wall=GameObject.Find("MuroArriba").GetComponent<Collider2D>();
                float radius=ball.GetComponent<Collider2D>().bounds.extents.y;
                rb.position=new Vector2(wall.bounds.center.x,wall.bounds.min.y-radius-.08f);
                rb.linearVelocity=new Vector2(0,7); Next(3,.14f); return;
            }
            if(stage==3)
            {
                Check(source.isPlaying,"Wall collision plays audio");
                rb.position=Vector2.zero; rb.linearVelocity=Vector2.zero; Next(4,.7f); return;
            }
            if(stage==4)
            {
                var paddle=GameObject.Find("Player1").GetComponent<Collider2D>();
                float radius=ball.GetComponent<Collider2D>().bounds.extents.x;
                rb.position=new Vector2(paddle.bounds.max.x+radius+.08f,paddle.bounds.center.y);
                rb.linearVelocity=new Vector2(-7,0); Next(5,.12f); return;
            }
            if(stage==5)
            {
                Check(source.isPlaying&&rb.linearVelocity.x>0,"Paddle collision bounces and plays audio");
                rb.position=Vector2.zero; rb.linearVelocity=Vector2.zero;
                manager.PuntoJugador1(); Next(6,.15f); return;
            }
            if(stage==6)
            {
                Check(manager.audioSource.isPlaying&&manager.scoreP1==1,"Scoring plays audio and increments score");
                manager.audioSource.Stop();
                while(manager.scoreP1<manager.maxScore) manager.PuntoJugador1();
                Next(7,.15f); return;
            }
            if(stage==7)
            {
                Check(manager.audioSource.isPlaying&&Time.timeScale==0&&manager.winnerText.text.Contains("GANA"),"Victory audio plays while game is paused");
                int previous=manager.scoreP1; manager.PuntoJugador1();
                Check(manager.scoreP1==previous,"No repeated score or victory after game ends");
                SessionState.SetBool(Key+"active",false);
                Debug.Log("PINGPONG_AUDIO_CHECK: "+File.ReadAllText(Report));
                EditorApplication.isPlaying=false;
            }
        }
        catch(Exception error)
        {
            File.AppendAllText(Report,"ERROR "+error+"\n");
            SessionState.SetBool(Key+"active",false);
            EditorApplication.isPlaying=false;
            Debug.LogException(error);
        }
    }
}
