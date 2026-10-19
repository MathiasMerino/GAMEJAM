using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class IntegrarCortez
{
    const string Origen="Assets/Enemigos/AnimacionesCortez/";
    const string Destino="Assets/Prefabs/Enemigos/";
    static Material material;
    static Transform Nodo(Transform padre,string nombre,Vector2 posicion)
    {
        var t=new GameObject(nombre).transform;t.SetParent(padre,false);t.localPosition=posicion;return t;
    }
    static Transform Pieza(Transform padre,string nombre,string archivo,Vector2 centro,int orden)
    {
        var sprite=AssetDatabase.LoadAllAssetsAtPath(Origen+"Sprites/"+archivo+".png").OfType<Sprite>().First();
        var t=Nodo(padre,nombre,centro-(Vector2)sprite.bounds.center);
        var r=t.gameObject.AddComponent<SpriteRenderer>();r.sprite=sprite;r.sharedMaterial=material;r.sortingOrder=orden;return t;
    }
    static AnimationClip Clip(bool escalar)
    {
        string ruta=Destino+(escalar?"CortezEscalar":"CortezCaminarIntegrado")+".anim";
        var clip=AssetDatabase.LoadAssetAtPath<AnimationClip>(ruta);
        if(clip==null){clip=new AnimationClip();AssetDatabase.CreateAsset(clip,ruta);}
        clip.ClearCurves();clip.frameRate=30;
        var original=AssetDatabase.LoadAssetAtPath<AnimationClip>(Origen+"Clips/Cortez_Caminar.anim");
        foreach(var binding in AnimationUtility.GetCurveBindings(original))
        {
            // La escena de montaje original no se subió: evitar sus posiciones absolutas y rotaciones 3D.
            if(binding.type!=typeof(Transform) || !binding.propertyName.EndsWith(".z") || !binding.propertyName.Contains("Euler"))continue;
            var claves=AnimationUtility.GetEditorCurve(original,binding).keys;
            if(claves.Length<2)continue;
            var nuevas=new Keyframe[claves.Length+1];
            for(int i=0;i<claves.Length;i++)
                nuevas[i]=new Keyframe(i*.8f/claves.Length,Mathf.Clamp(claves[i].value,-24,24));
            nuevas[nuevas.Length-1]=new Keyframe(.8f,nuevas[0].value);
            AnimationUtility.SetEditorCurve(clip,binding,new AnimationCurve(nuevas));
        }
        if(escalar)
        {
            AnimationUtility.SetEditorCurve(clip,EditorCurveBinding.FloatCurve("BrazoDerecho",typeof(Transform),"localEulerAnglesRaw.z"),new AnimationCurve(new Keyframe(0,-100),new Keyframe(.4f,-45),new Keyframe(.8f,-100)));
        }
        var ajustes=AnimationUtility.GetAnimationClipSettings(clip);ajustes.loopTime=true;AnimationUtility.SetAnimationClipSettings(clip,ajustes);
        EditorUtility.SetDirty(clip);return clip;
    }
    [MenuItem("GAMEJAM/Integrar Dr Cortez de Grecia")]
    public static void Aplicar()
    {
        Directory.CreateDirectory(Destino);AssetDatabase.Refresh();
        material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Sprites/Prototipos/IlustracionSinLuz.mat");
        var caminar=Clip(false);var escalar=Clip(true);
        string rutaControl=Destino+"CortezIntegrado.controller";
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(rutaControl);
        if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(rutaControl);
        var machine=controller.layers[0].stateMachine;
        foreach(var estado in machine.states)machine.RemoveState(estado.state);
        machine.AddState("Caminar").motion=caminar;machine.AddState("Escalar").motion=escalar;
        EditorUtility.SetDirty(controller);
        var raiz=new GameObject("CortezVisual");raiz.AddComponent<SortingGroup>().sortingOrder=18;
        var torso=Nodo(raiz.transform,"Torso",Vector2.zero);
        Pieza(torso,"Torso_Derecha_01_0","Torso_Derecha_01",new Vector2(0,4.5f),3);
        var cabeza=Nodo(torso,"Cabeza",Vector2.zero);
        Pieza(cabeza,"Cabeza_Derecha_01_0","Cabeza_Derecha_01",new Vector2(.15f,7),4);
        var pierna=Nodo(raiz.transform,"PiernaDerecha",new Vector2(-.35f,3.1f));
        Pieza(pierna,"Pierna_Derecha_04_0","Pierna_Derecha_04",new Vector2(0,-1.5f),1);
        pierna=Nodo(raiz.transform,"PiernaIzquierda",new Vector2(.35f,3.1f));
        Pieza(pierna,"Pierna_Derecha_01_0","Pierna_Derecha_01",new Vector2(0,-1.5f),2);
        var brazo=Nodo(raiz.transform,"BrazoDerecho",new Vector2(-.55f,6));
        Pieza(brazo,"Brazo_Derecha_02_0","Brazo_Derecha_02",new Vector2(-.2f,-1.5f),5);
        var linterna=Pieza(brazo,"Linterna","Linterna_Derecha_01",new Vector2(.4f,-2.65f),6);
        Nodo(linterna,"Emisor",(Vector2)linterna.GetComponent<SpriteRenderer>().sprite.bounds.center+Vector2.right*.5f).gameObject.AddComponent<LinternaEnemigo>();
        var animador=raiz.AddComponent<Animator>();animador.runtimeAnimatorController=controller;animador.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        raiz.AddComponent<VisualEnemigoCortez>().animador=animador;
        var prefab=PrefabUtility.SaveAsPrefabAsset(raiz,Destino+"CortezVisual.prefab");Object.DestroyImmediate(raiz);
        foreach(var escena in new[]{"Persecucion"})
        {
            EditorSceneManager.OpenScene("Assets/Scenes/"+escena+"/"+escena+".unity");
            foreach(var enemigo in Object.FindObjectsByType<EnemigoTemporal>())
            {
                foreach(Transform hijo in enemigo.transform.Cast<Transform>().ToArray())
                    if(hijo.name=="CortezVisual")Object.DestroyImmediate(hijo.gameObject);
                    else hijo.gameObject.SetActive(false);
                var visual=(GameObject)PrefabUtility.InstantiatePrefab(prefab);visual.name="CortezVisual";
                visual.transform.SetParent(enemigo.transform,false);visual.transform.localPosition=new Vector3(0,-.55f,0);visual.transform.localScale=Vector3.one*.235f;
                var c=enemigo.GetComponent<BoxCollider2D>();c.size=new Vector2(.65f,1.5f);c.offset=new Vector2(0,.25f);
                enemigo.gameObject.tag="Untagged";
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();
    }
}


