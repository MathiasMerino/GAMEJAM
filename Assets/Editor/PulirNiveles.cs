using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Ajustes repetibles sobre las escenas existentes; conserva sus GUID y conexiones.
public static class PulirNiveles
{
    const string Arte = "Assets/Sprites/Ambiente/";
    static Material material;
    static Sprite Importar(string ruta)
    {
        var i=(TextureImporter)AssetImporter.GetAtPath(ruta);
        i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;
        i.spritePixelsPerUnit=100;i.maxTextureSize=2048;i.mipmapEnabled=false;
        i.alphaIsTransparency=true;i.textureCompression=TextureImporterCompression.Uncompressed;i.SaveAndReimport();
        return AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
    }
    static Sprite Recorte(string nombre,Sprite original,Rect proporcion)
    {
        string ruta=Arte+nombre+".asset";
        var s=AssetDatabase.LoadAssetAtPath<Sprite>(ruta);if(s!=null)return s;
        var r=original.rect;
        s=Sprite.Create(original.texture,new Rect(r.x+r.width*proporcion.x,r.y+r.height*proporcion.y,r.width*proporcion.width,r.height*proporcion.height),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        s.name=nombre;AssetDatabase.CreateAsset(s,ruta);return s;
    }
    static Transform Hijo(Transform padre,string nombre)
    {
        var t=padre.Find(nombre);if(t!=null)return t;
        t=new GameObject(nombre).transform;t.SetParent(padre,false);return t;
    }
    static void Vestir(Transform t,Sprite s,float w,float h,int orden)
    {
        var r=t.GetComponent<SpriteRenderer>();if(r==null)r=t.gameObject.AddComponent<SpriteRenderer>();
        r.sprite=s;r.sharedMaterial=material;r.sortingOrder=orden;r.color=Color.white;
        t.localScale=new Vector3(w/s.bounds.size.x,h/s.bounds.size.y,1);
    }
    [MenuItem("GAMEJAM/Pulir carteles y jugabilidad")]
    public static void Aplicar()
    {
        AssetDatabase.Refresh();
        material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Sprites/Prototipos/IlustracionSinLuz.mat");
        var numeros=new System.Collections.Generic.Dictionary<int,Sprite>();
        foreach(int n in new[]{40,30,20,10}) numeros[n]=Recorte("NumeroPintado"+n,Importar(Arte+"CorredorCartel"+n+".png"),new Rect(508f/1672,497f/941,47f/1672,30f/941));
        var valle=Importar("Assets/Sprites/Backgrounds/Pruebas/ValleSinZanahoria.png");
        var zanahoria=Importar(Arte+"Zanahoria.png");
        const string rutaFisica="Assets/Sprites/Prototipos/SinFriccion.physicsMaterial2D";
        var fisica=AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(rutaFisica);
        if(fisica==null){fisica=new PhysicsMaterial2D("SinFriccion");AssetDatabase.CreateAsset(fisica,rutaFisica);}
        fisica.friction=0;fisica.bounciness=0;EditorUtility.SetDirty(fisica);
        foreach(string nombre in new[]{"EscenaInicial","Nivel1","PasilloPrincipal","PuertaA","PuertaC","Persecucion","EscapeVertical","Valle","HabitacionFinal"})
        {
            EditorSceneManager.OpenScene("Assets/Scenes/"+nombre+"/"+nombre+".unity");
            foreach(var c in Object.FindObjectsByType<Collider2D>(FindObjectsInactive.Include))
                if(!c.isTrigger)c.sharedMaterial=fisica;
            if(nombre=="PuertaC")
            {
                foreach(var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include))
                    if(r.name=="CartelFondo" || r.name.StartsWith("Rotulo_SALIDA A"))r.enabled=false;
                var fondos=Object.FindObjectsByType<SpriteRenderer>().Where(r=>r.sprite!=null && r.sprite.name=="Corredor").OrderBy(r=>r.transform.position.x).ToArray();
                for(int i=0;i<fondos.Length;i++)
                {
                    var bg=fondos[i];int distancia=i==0?10:i==6?50:60-i*10;
                    if(distancia==50)continue; // Conservar el cartel pintado original en las secciones de 50 m.
                    var anterior=bg.transform.parent.Find("CartelAmbiental_"+i);
                    if(anterior!=null)anterior.gameObject.SetActive(false);
                    anterior=bg.transform.parent.Find("DistanciaAmbiental_"+i);
                    if(anterior!=null)anterior.gameObject.SetActive(false);
                    // Conservar todos los píxeles originales de la placa, excepto el pequeño espacio del número.
                    var cifra=Hijo(bg.transform.parent,"NumeroPintado_"+i);
                    cifra.position=new Vector3(bg.bounds.min.x+531.5f/1672*bg.bounds.size.x,bg.bounds.max.y-429f/941*bg.bounds.size.y,0);
                    Vestir(cifra,numeros[distancia],47f/1672*bg.bounds.size.x,30f/941*bg.bounds.size.y,2);
                }
            }
            if(nombre=="EscapeVertical")
            {
                var jugador=Object.FindAnyObjectByType<MovimientoPlayer>();jugador.fuerzaSalto=7.7f;
                PrefabUtility.RecordPrefabInstancePropertyModifications(jugador);

            }
            if(nombre=="Persecucion")
                Object.FindObjectsByType<EnemigoTemporal>().First(e=>!e.patrulla).transform.position=new Vector3(-8.4f,-1.05f,0);
            foreach(var e in Object.FindObjectsByType<EnemigoTemporal>())e.demoraInicial=.65f;
            if(nombre=="Valle")
            {
                var bg=GameObject.Find("Fondo_LaboratorioValle").GetComponent<SpriteRenderer>();
                Vestir(bg.transform,valle,30,16.88f,-20);
                var trampa=Object.FindAnyObjectByType<TrampaZanahoria>();trampa.indicacion="Recoger zanahoria";trampa.permiteClic=true;trampa.tamanoClic=new Vector2(1.3f,1.4f);
                var objeto=Hijo(trampa.transform,"ZanahoriaRecogible");objeto.localPosition=new Vector3(.2f,-.2f,0);Vestir(objeto,zanahoria,.95f,.47f,20);trampa.zanahoria=objeto;
                // Solo piedra del fondo limpio: ninguna parte de la zanahoria está impresa en la losa.
                var losa=Recorte("LosaValleLimpia",valle,new Rect(25.2f/30, (8.44f-2.78f)/16.88f,1.6f/30,.4f/16.88f));
                Vestir(trampa.losa,losa,1.6f,.4f,3);trampa.losa.position=new Vector3(11,-2.58f,0);trampa.losa.rotation=Quaternion.identity;
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        // Mantener el material también en el prefab para futuros niveles.
        string prefab="Assets/Prefabs/Pruebas/JugadorTemporal.prefab";var raiz=PrefabUtility.LoadPrefabContents(prefab);
        foreach(var c in raiz.GetComponentsInChildren<Collider2D>())if(!c.isTrigger)c.sharedMaterial=fisica;
        PrefabUtility.SaveAsPrefabAsset(raiz,prefab);PrefabUtility.UnloadPrefabContents(raiz);
        AssetDatabase.SaveAssets();
    }
}

