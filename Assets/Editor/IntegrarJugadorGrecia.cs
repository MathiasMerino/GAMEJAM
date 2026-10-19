using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class IntegrarJugadorGrecia
{
    const string Carpeta="Assets/Prefabs/Pruebas/";
    static GameObject visualPrefab;
    [MenuItem("GAMEJAM/Integrar conejo de Grecia")]
    public static void Aplicar()
    {
        string ruta=Carpeta+"ConejoIntegrado.controller";
        var controller=AssetDatabase.LoadAssetAtPath<AnimatorController>(ruta);
        if(controller==null)controller=AnimatorController.CreateAnimatorControllerAtPath(ruta);
        var estados=controller.layers[0].stateMachine;
        foreach(var e in estados.states)estados.RemoveState(e.state);
        string[] nombres={"Reposo","Caminar","Saltar","Caer"};
        string[] clips={"Conejo_Reposo","Caminar","Conejo_Saltar","Caer"};
        for(int i=0;i<nombres.Length;i++)estados.AddState(nombres[i]).motion=AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Player/Animaciones/Clips/"+clips[i]+".anim");
        EditorUtility.SetDirty(controller);
        var origen=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Player/Prefabs/Player.prefab");
        var contenedor=new GameObject("ConejoGrecia");contenedor.AddComponent<SortingGroup>().sortingOrder=20;
        var visual=Object.Instantiate(origen.transform.Find("VisualConejo").gameObject,contenedor.transform);
        visual.name="VisualConejo";visual.transform.localPosition=Vector3.zero;visual.transform.localScale=Vector3.one;
        var animador=visual.GetComponent<Animator>();animador.runtimeAnimatorController=controller;animador.applyRootMotion=false;animador.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Player/Animaciones/Clips/Conejo_Reposo.anim").SampleAnimation(visual,0);
        var sprites=visual.GetComponentsInChildren<SpriteRenderer>();
        var limites=sprites[0].bounds;foreach(var sprite in sprites){limites.Encapsulate(sprite.bounds);sprite.sharedMaterial=AssetDatabase.LoadAssetAtPath<Material>("Assets/Sprites/Prototipos/IlustracionSinLuz.mat");}
        float escala=1.15f/limites.size.y;
        visual.transform.localScale=Vector3.one*escala;
        visual.transform.localPosition=new Vector3(-limites.center.x*escala,-.475f-limites.min.y*escala,0);
        contenedor.AddComponent<VisualJugadorGrecia>().animador=animador;
        visualPrefab=PrefabUtility.SaveAsPrefabAsset(contenedor,Carpeta+"ConejoGrecia.prefab");Object.DestroyImmediate(contenedor);
        var prefab=PrefabUtility.LoadPrefabContents(Carpeta+"JugadorTemporal.prefab");
        Colocar(prefab.GetComponent<MovimientoPlayer>());
        PrefabUtility.SaveAsPrefabAsset(prefab,Carpeta+"JugadorTemporal.prefab");PrefabUtility.UnloadPrefabContents(prefab);
        foreach(string escena in new[]{"EscenaInicial","Nivel1","PasilloPrincipal","PuertaA","PuertaC","Persecucion","EscapeVertical","Valle","HabitacionFinal"})
        {
            EditorSceneManager.OpenScene("Assets/Scenes/"+escena+"/"+escena+".unity");
            foreach(var jugador in Object.FindObjectsByType<MovimientoPlayer>())Colocar(jugador);
            if(escena=="EscapeVertical")foreach(var enemigo in Object.FindObjectsByType<EnemigoTemporal>())Object.DestroyImmediate(enemigo.gameObject);
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();
    }
    static void Colocar(MovimientoPlayer jugador)
    {
        var existente=jugador.transform.Find("ConejoGrecia");
        foreach(var sprite in jugador.GetComponentsInChildren<SpriteRenderer>(true))
            if(existente==null || !sprite.transform.IsChildOf(existente))sprite.enabled=false;
        if(existente!=null)
        {
            existente.localScale=Vector3.one;
            return;
        }
        var visual=(GameObject)PrefabUtility.InstantiatePrefab(visualPrefab);
        visual.transform.SetParent(jugador.transform,false);visual.name="ConejoGrecia";visual.transform.localScale=Vector3.one;
    }
}
