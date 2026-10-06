using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public static class PingPongGameSkinBuilder
{
    const string Folder = "Assets/Sprites/juego funcional/";
    const string Work = "C:/Users/HP/Documents/Codex/2026-10-05/tengo-actualmente-un-proyecto-unity-a/work/";
    [MenuItem("Tools/Ping Pong/Diseno/Aplicar mockup a SampleScene %#&d")]
    public static void Build()
    {
        var scene=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/SampleScene.unity") throw new Exception("Abre SampleScene antes de aplicar el diseno.");
        if(GameObject.Find("DisenoJuego")!=null) throw new Exception("El diseno ya existe.");
        var camera=Camera.main;
        var manager=UnityEngine.Object.FindFirstObjectByType<GameManager>();
        var p1=GameObject.Find("Player1").GetComponent<Collider2D>();
        var p2=GameObject.Find("Player2").GetComponent<Collider2D>();
        var ball=UnityEngine.Object.FindFirstObjectByType<BallMovement>().GetComponent<Collider2D>();
        var background=Load("fondo/fondo_campo_de_juego.ase");
        var dragon=Load("jugadores/PingPong_Jugador1_Dragon.ase");
        var princess=Load("jugadores/PingPong_Jugador2_Princesa.ase");
        var ballSprite=Load("pelota del juego/PingPong_Pelota.ase");
        var green=Enumerable.Range(0,6).Select(i=>Load("numeros jugador 1/numero "+i+" jugador 1.ase")).ToArray();
        var pink=Enumerable.Range(0,6).Select(i=>Load("numeros jugador 2/numero "+i+" jugador 2.ase")).ToArray();
        var before=Capture();
        File.WriteAllLines(Work+"game_configuration_before.txt",before.Select(k=>k.Key+"="+k.Value));
        Undo.IncrementCurrentGroup(); int undoGroup=Undo.GetCurrentGroup(); Undo.SetCurrentGroupName("Aplicar diseno visual Ping Pong");
        var root=new GameObject("DisenoJuego",typeof(RectTransform),typeof(Canvas),typeof(CanvasScaler));
        var canvas=root.GetComponent<Canvas>(); canvas.renderMode=RenderMode.ScreenSpaceOverlay; canvas.sortingOrder=10;
        var scaler=root.GetComponent<CanvasScaler>(); scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize; scaler.referenceResolution=new Vector2(1672,941); scaler.matchWidthOrHeight=.5f;
        var skin=root.AddComponent<PingPongGameSkin>(); skin.gameCamera=camera; skin.gameManager=manager; skin.player1=p1; skin.player2=p2; skin.ball=ball;
        var field=Picture("FondoCampo",root.transform,background,Vector2.zero,Vector2.zero,false); Stretch(field.rectTransform);
        skin.player1Image=Picture("Dragon",root.transform,dragon,Vector2.zero,Vector2.zero,false);
        skin.player2Image=Picture("Princesa",root.transform,princess,Vector2.zero,Vector2.zero,false);
        skin.ballImage=Picture("Pelota",root.transform,ballSprite,Vector2.zero,Vector2.zero,false);
        skin.player1Digits=green; skin.player2Digits=pink;
        skin.player1Score=Picture("MarcadorJugador1",root.transform,green[manager.scoreP1],new Vector2(-88,296),new Vector2(94,100),true);
        skin.player2Score=Picture("MarcadorJugador2",root.transform,pink[manager.scoreP2],new Vector2(86,296),new Vector2(94,100),true);
        Label("Separador",root.transform,"-",new Vector2(0,296),new Vector2(50,90),78,new Color(1,.88f,.44f));
        skin.scoreFallback=Label("MarcadorAlternativo",root.transform,"",new Vector2(0,296),new Vector2(400,100),64,new Color(1,.9f,.5f));
        var panel=Solid("Victoria",root.transform,new Color(.035f,.05f,.15f,.94f),Vector2.zero,new Vector2(1050,210));
        var outline=panel.gameObject.AddComponent<Outline>(); outline.effectColor=new Color(1,.76f,.18f); outline.effectDistance=new Vector2(4,-4);
        skin.victoryPanel=panel.gameObject;
        skin.victoryLabel=Label("MensajeVictoria",panel.transform,"",Vector2.zero,new Vector2(980,165),64,new Color(1,.88f,.43f));
        skin.victoryLabel.fontStyle=FontStyles.Bold; skin.victoryPanel.SetActive(false);
        foreach(var owner in new[]{p1.gameObject,p2.gameObject,ball.gameObject,GameObject.Find("MuroArriba"),GameObject.Find("MuroAbajo")})
        { var renderer=owner.GetComponent<SpriteRenderer>(); if(renderer!=null) { Undo.RecordObject(renderer,"Ocultar dibujo anterior"); renderer.enabled=false; } }
        foreach(var label in new[]{manager.scoreText,manager.winnerText})
        { if(label!=null) { Undo.RecordObject(label,"Ocultar texto anterior"); var color=label.color; color.a=0; label.color=color; } }
        Undo.RegisterCreatedObjectUndo(root,"Crear diseno del juego");
        var after=Capture();
        var changed=before.Where(p=>!after.ContainsKey(p.Key)||after[p.Key]!=p.Value).Select(p=>p.Key).ToArray();
        if(changed.Length>0) { Undo.RevertAllDownToGroup(undoGroup); throw new Exception("Cambio inesperado de configuracion: "+string.Join(", ",changed)); }
        File.WriteAllLines(Work+"game_configuration_after.txt",after.Select(k=>k.Key+"="+k.Value));
        File.WriteAllText(Work+"game_design_validation.txt","PASS: "+before.Count+" componentes y transformaciones de juego sin cambios.\n");
        Undo.CollapseUndoOperations(undoGroup);
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        Selection.activeGameObject=root;
        Debug.Log("PINGPONG_GAME_SKIN_READY: diseno aplicado; "+before.Count+" configuraciones verificadas sin cambios.");
    }
    public static SortedDictionary<string,string> Capture()
    {
        var result=new SortedDictionary<string,string>();
        var roots=UnityEngine.SceneManagement.SceneManager.GetActiveScene().GetRootGameObjects();
        foreach(var root in roots)
        foreach(var component in root.GetComponentsInChildren<Component>(true))
        {
            if(component==null) continue;
            bool protectedType=component is Collider2D||component is Rigidbody2D||component is PaddleMovement||component is BallMovement||component is GameManager||component is Goal||component is Camera||component is AudioSource;
            bool protectedTransform=component is Transform && (component.GetComponent<Collider2D>()!=null||component.GetComponent<Rigidbody2D>()!=null||component.GetComponent<Camera>()!=null||component.GetComponent<GameManager>()!=null);
            if(!protectedType&&!protectedTransform) continue;
            string path=component.gameObject.name; var parent=component.transform.parent;
            while(parent!=null) { path=parent.name+"/"+path; parent=parent.parent; }
            string key=path+"/"+component.GetType().Name+"/"+component.GetInstanceID();
            result[key]=EditorJsonUtility.ToJson(component);
        }
        return result;
    }
    static Sprite Load(string name)
    { var sprites=AssetDatabase.LoadAllAssetsAtPath(Folder+name).OfType<Sprite>().ToArray(); if(sprites.Length==0) throw new Exception("Falta sprite "+name); return sprites.OrderByDescending(s=>s.rect.width*s.rect.height).First(); }
    static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
    { var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false); var r=(RectTransform)go.transform; r.anchorMin=r.anchorMax=new Vector2(.5f,.5f); r.anchoredPosition=position; r.sizeDelta=size; return r; }
    static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero; }
    static Image Solid(string name,Transform parent,Color color,Vector2 position,Vector2 size)
    { var r=Rect(name,parent,position,size); var image=r.gameObject.AddComponent<Image>(); image.color=color; image.raycastTarget=false; return image; }
    static Image Picture(string name,Transform parent,Sprite sprite,Vector2 position,Vector2 size,bool preserve)
    { var image=Solid(name,parent,Color.white,position,size); image.sprite=sprite; image.preserveAspect=preserve; return image; }
    static TMP_Text Label(string name,Transform parent,string text,Vector2 position,Vector2 size,int fontSize,Color color)
    { var r=Rect(name,parent,position,size); var label=r.gameObject.AddComponent<TextMeshProUGUI>(); label.text=text; label.fontSize=fontSize; label.color=color; label.alignment=TextAlignmentOptions.Center; label.raycastTarget=false; return label; }
}
