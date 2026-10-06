using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class PingPongAudioBuilder
{
    private const string AudioFolder = "Assets/Sonidos/";
    private const string WorkFolder = "C:/Users/HP/Documents/Codex/2026-10-05/tengo-actualmente-un-proyecto-unity-a/work/";
    [MenuItem("Tools/Ping Pong/Audio/1 Analizar sonidos %#&a")]
    public static void Analyze()
    {
        Directory.CreateDirectory(WorkFolder);
        var report = new System.Text.StringBuilder();
        foreach (string name in new[] { "menu", "hit", "wall", "score", "win" })
        {
            AudioClip clip = Load(name + ".mp3");
            report.AppendLine(name + ": " + clip.length.ToString("F3") + " s, " + clip.frequency + " Hz, " + clip.channels + " canales");
            if (name != "menu")
            {
                clip.LoadAudioData();
                float[] data = new float[clip.samples * clip.channels];
                if (!clip.GetData(data, 0)) throw new Exception("No se pudo decodificar " + name);
                WriteWav(WorkFolder + name + "_original.wav", data, clip.channels, clip.frequency);
            }
        }
        File.WriteAllText(WorkFolder + "audio_analysis.txt", report.ToString());
        Debug.Log("PINGPONG_AUDIO_ANALYSIS: " + report);
    }
    [MenuItem("Tools/Ping Pong/Audio/2 Aplicar sonidos %#&s")]
    public static void Build()
    {
        var active = SceneManager.GetActiveScene();
        if (active.isDirty) throw new Exception("Guarda la escena actual antes de aplicar audio.");
        var menuClip = Load("menu.mp3");
        var hitClip = Load("hit_corto.wav");
        var wallClip = Load("wall_corto.wav");
        var scoreClip = Load("score.mp3");
        var winClip = Load("win.mp3");
        ConfigureImporter("menu.mp3", true);
        foreach (var file in new[] { "hit_corto.wav", "wall_corto.wav", "score.mp3", "win.mp3" }) ConfigureImporter(file, false);
        Scene game = SceneManager.GetSceneByPath("Assets/Scenes/SampleScene.unity");
        bool gameOpened = !game.isLoaded;
        if (gameOpened) game = EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity", OpenSceneMode.Additive);
        var ball = Components<BallMovement>(game).Single();
        var manager = Components<GameManager>(game).Single();
        Undo.RecordObject(ball, "Asignar sonidos de pelota");
        ball.hitSound = hitClip;
        ball.wallSound = wallClip;
        Source(ball.gameObject, .55f, false, null);
        Undo.RecordObject(manager, "Asignar sonidos de puntuacion");
        manager.audioSource = Source(manager.gameObject, .7f, false, null);
        manager.scoreSound = scoreClip;
        manager.winSound = winClip;
        EditorSceneManager.MarkSceneDirty(game);
        EditorSceneManager.SaveScene(game);
        Scene menu = SceneManager.GetSceneByPath("Assets/Scenes/menu.unity");
        bool menuOpened = !menu.isLoaded;
        if (menuOpened) menu = EditorSceneManager.OpenScene("Assets/Scenes/menu.unity", OpenSceneMode.Additive);
        var musicObject = menu.GetRootGameObjects().FirstOrDefault(g => g.name == "MusicaMenu");
        if (musicObject == null)
        {
            musicObject = new GameObject("MusicaMenu");
            SceneManager.MoveGameObjectToScene(musicObject, menu);
            Undo.RegisterCreatedObjectUndo(musicObject,"Crear musica del menu");
        }
        Source(musicObject, .3f, true, menuClip);
        EditorSceneManager.MarkSceneDirty(menu);
        EditorSceneManager.SaveScene(menu);
        if (gameOpened) EditorSceneManager.CloseScene(game, true);
        if (menuOpened) EditorSceneManager.CloseScene(menu, true);
        Debug.Log("PINGPONG_AUDIO_READY: musica de menu, golpe corto, pared, punto y victoria guardados.");
    }
    static T[] Components<T>(Scene scene) where T : Component
    { return scene.GetRootGameObjects().SelectMany(g => g.GetComponentsInChildren<T>(true)).ToArray(); }
    static AudioClip Load(string file)
    { var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(AudioFolder + file); if (clip == null) throw new Exception("Falta audio: " + file); return clip; }
    static AudioSource Source(GameObject owner,float volume,bool loop,AudioClip clip)
    {
        var source = owner.GetComponent<AudioSource>();
        if (source == null) source = Undo.AddComponent<AudioSource>(owner);
        Undo.RecordObject(source,"Configurar audio 2D");
        source.clip=clip; source.volume=volume; source.loop=loop; source.playOnAwake=loop;
        source.spatialBlend=0; source.pitch=1; source.mute=false;
        return source;
    }
    static void ConfigureImporter(string file,bool music)
    {
        var importer = (AudioImporter)AssetImporter.GetAtPath(AudioFolder + file);
        var settings=importer.defaultSampleSettings;
        settings.loadType=music ? AudioClipLoadType.Streaming : AudioClipLoadType.DecompressOnLoad;
        settings.preloadAudioData=!music; importer.defaultSampleSettings=settings; importer.SaveAndReimport();
    }
    static void WriteWav(string path,float[] data,int channels,int rate)
    {
        using(var writer=new BinaryWriter(File.Create(path)))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("RIFF")); writer.Write(36+data.Length*2);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt ")); writer.Write(16); writer.Write((short)1);
            writer.Write((short)channels); writer.Write(rate); writer.Write(rate*channels*2); writer.Write((short)(channels*2)); writer.Write((short)16);
            writer.Write(System.Text.Encoding.ASCII.GetBytes("data")); writer.Write(data.Length*2);
            foreach(float sample in data) writer.Write((short)Mathf.RoundToInt(Mathf.Clamp(sample,-1,1)*32767));
        }
    }
}

