using System.Linq;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public static class PingPongMenuBuilder
{
    private const string Folder = "Assets/Sprites/sprites/";
    [MenuItem("Tools/Ping Pong/Montar menu con sprites")]
    public static void Build()
    {
        var scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/menu.unity") throw new System.Exception("Abre la escena menu primero.");
        var canvas = Object.FindFirstObjectByType<Canvas>();
        var manager = Object.FindFirstObjectByType<MenuManager>();
        if (canvas == null || manager == null) throw new System.Exception("Falta Canvas o MenuManager.");
        if (canvas.transform.Find("MenuSprites") != null) { Debug.Log("MenuSprites ya existe."); return; }
        Undo.IncrementCurrentGroup();
        int group = Undo.GetCurrentGroup();
        Undo.SetCurrentGroupName("Montar menu Ping Pong");
        // Resolve all assets before changing the scene.
        Sprite fondo = Load("PingPong_Fondo.ase");
        Sprite logo = Load("PingPong_Logo.ase");
        Sprite play = Load("boton de play.ase");
        Sprite options = Load("boton de options.ase");
        Sprite how = Load("btn how to play.ase");
        Sprite credits = Load("btn credits.ase");
        Sprite exit = Load("btn exit.ase");
        Undo.RecordObject(canvas, "Configurar Canvas");
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        var scaler = canvas.GetComponent<CanvasScaler>();
        if (scaler == null) scaler = Undo.AddComponent<CanvasScaler>(canvas.gameObject);
        Undo.RecordObject(scaler, "Escala adaptable");
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(960,540);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;
        foreach (Transform child in canvas.transform) { Undo.RecordObject(child.gameObject, "Ocultar interfaz anterior"); child.gameObject.SetActive(false); }
        var oldBackground = scene.GetRootGameObjects().FirstOrDefault(g => g.name == "PingPong_Fondo");
        if (oldBackground != null) { Undo.RecordObject(oldBackground,"Ocultar fondo anterior"); oldBackground.SetActive(false); }
        RectTransform root = Rect("MenuSprites", canvas.transform, Vector2.zero, Vector2.zero); Stretch(root);
        Image background = Picture("Fondo", root, fondo, Vector2.zero, Vector2.zero); Stretch(background.rectTransform);
        // Aspect-preserving full-screen cover, including tall editor views.
        background.preserveAspect = false;
        var fit = background.gameObject.AddComponent<AspectRatioFitter>();
        fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
        fit.aspectRatio = fondo.rect.width / fondo.rect.height;
        Image shade = Solid("Sombra",root,new Color(0.035f,0.022f,0.08f,0.14f),Vector2.zero,Vector2.zero); Stretch(shade.rectTransform);
        Picture("Logo",root,logo,new Vector2(0,155),new Vector2(390,145));
        var ui = root.gameObject.AddComponent<PingPongMenuUI>();
        MakeButton("Play",root,play,new Vector2(0,42),new Vector2(260,80),manager.Jugar);
        MakeButton("Options",root,options,new Vector2(-140,-44),new Vector2(228,62),ui.Options);
        MakeButton("HowToPlay",root,how,new Vector2(140,-44),new Vector2(228,62),ui.HowToPlay);
        MakeButton("Credits",root,credits,new Vector2(-140,-118),new Vector2(228,62),ui.Credits);
        MakeButton("Exit",root,exit,new Vector2(140,-118),new Vector2(228,62),manager.Salir);
        Label("Firma",root,"NAZARETH & DILAN",new Vector2(0,-222),new Vector2(600,32),18,new Color(1,0.94f,0.78f));
        var dialog = Solid("Dialogo",root,new Color(0.02f,0.02f,0.06f,0.85f),Vector2.zero,Vector2.zero); Stretch(dialog.rectTransform); dialog.raycastTarget=true;
        var panel = Solid("Panel",dialog.transform,new Color(0.14f,0.09f,0.2f,1),Vector2.zero,new Vector2(620,420));
        ui.dialog=dialog.gameObject;
        ui.heading=Label("Titulo",panel.transform,"",new Vector2(0,146),new Vector2(550,54),36,new Color(1,0.78f,0.26f));
        ui.body=Label("Contenido",panel.transform,"",new Vector2(0,25),new Vector2(530,170),23,Color.white);
        ui.volumeControls=Rect("Volumen",panel.transform,new Vector2(0,-58),new Vector2(400,40)).gameObject;
        ui.volumeSlider=Volume(ui.volumeControls.transform,ui);
        var close = Solid("Volver",panel.transform,new Color(1,0.7f,0.18f),new Vector2(0,-155),new Vector2(200,52)); close.raycastTarget=true;
        var closeButton=close.gameObject.AddComponent<Button>(); closeButton.targetGraphic=close;
        UnityEventTools.AddPersistentListener(closeButton.onClick,ui.Close);
        Label("Texto",close.transform,"VOLVER",Vector2.zero,new Vector2(180,45),22,new Color(0.18f,0.08f,0.12f));
        ui.dialog.SetActive(false);
        Undo.RegisterCreatedObjectUndo(root.gameObject,"Crear menu de sprites");
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        Undo.CollapseUndoOperations(group);
        Selection.activeGameObject=root.gameObject;
        Debug.Log("PINGPONG_MENU_READY: fondo, logo y 5 botones creados y guardados.");
    }
    static Sprite Load(string file)
    {
        var sprites=AssetDatabase.LoadAllAssetsAtPath(Folder+file).OfType<Sprite>().ToArray();
        if(sprites.Length==0) throw new System.Exception("No hay sprite en "+file);
        return sprites.OrderByDescending(s=>s.rect.width*s.rect.height).First();
    }
    static RectTransform Rect(string name,Transform parent,Vector2 position,Vector2 size)
    {
        var go=new GameObject(name,typeof(RectTransform)); go.transform.SetParent(parent,false);
        var r=(RectTransform)go.transform; r.anchorMin=r.anchorMax=new Vector2(.5f,.5f); r.anchoredPosition=position; r.sizeDelta=size; return r;
    }
    static void Stretch(RectTransform r) { r.anchorMin=Vector2.zero; r.anchorMax=Vector2.one; r.offsetMin=r.offsetMax=Vector2.zero; }
    static Image Solid(string name,Transform parent,Color color,Vector2 position,Vector2 size)
    { var r=Rect(name,parent,position,size); var i=r.gameObject.AddComponent<Image>(); i.color=color; i.raycastTarget=false; return i; }
    static Image Picture(string name,Transform parent,Sprite sprite,Vector2 position,Vector2 size)
    { var i=Solid(name,parent,Color.white,position,size); i.sprite=sprite; i.preserveAspect=true; return i; }
    static void MakeButton(string name,Transform parent,Sprite sprite,Vector2 position,Vector2 size,UnityAction action)
    { var image=Picture(name,parent,sprite,position,size); image.raycastTarget=true; var b=image.gameObject.AddComponent<Button>(); b.targetGraphic=image; var c=b.colors; c.highlightedColor=new Color(1,1,.8f); c.pressedColor=new Color(.8f,.7f,.5f); b.colors=c; UnityEventTools.AddPersistentListener(b.onClick,action); }
    static TMP_Text Label(string name,Transform parent,string text,Vector2 position,Vector2 size,int fontSize,Color color)
    { var r=Rect(name,parent,position,size); var label=r.gameObject.AddComponent<TextMeshProUGUI>(); label.text=text; label.fontSize=fontSize; label.color=color; label.alignment=TextAlignmentOptions.Center; label.raycastTarget=false; return label; }
    static Slider Volume(Transform parent,PingPongMenuUI ui)
    {
        var bg=Solid("Slider",parent,new Color(.3f,.24f,.38f),Vector2.zero,new Vector2(400,20));
        var fillArea=Rect("FillArea",bg.transform,Vector2.zero,new Vector2(380,12));
        var fill=Solid("Fill",fillArea,new Color(1,.72f,.22f),Vector2.zero,Vector2.zero); Stretch(fill.rectTransform);
        var handleArea=Rect("HandleArea",bg.transform,Vector2.zero,new Vector2(380,30));
        var handle=Solid("Handle",handleArea,Color.white,Vector2.zero,new Vector2(24,30)); handle.raycastTarget=true;
        bg.raycastTarget=true; var slider=bg.gameObject.AddComponent<Slider>(); slider.fillRect=fill.rectTransform; slider.handleRect=handle.rectTransform; slider.targetGraphic=handle; slider.minValue=0; slider.maxValue=1; slider.value=1;
        UnityEventTools.AddPersistentListener(slider.onValueChanged,ui.SetVolume); return slider;
    }
}

