using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object=UnityEngine.Object;

public static class PrepararEscape
{
    static Material material;
    static Sprite blanco, caja, repisa;
    static GameObject jugadorPrefab;
    static Transform raiz;
    static string Ruta(string n)=>"Assets/Scenes/"+n+"/"+n+".unity";
    [MenuItem("GAMEJAM/Preparar escape y final")]
    public static void Construir()
    {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.dataPath));
        material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Sprites/Prototipos/IlustracionSinLuz.mat");
        blanco=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Prototipos/Blanco.png");
        caja=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ambiente/CajaIndustrial.asset");
        repisa=AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Ambiente/BordeCaja.asset");
        Directory.CreateDirectory("Assets/Prefabs/Pruebas");
        const string prefab="Assets/Prefabs/Pruebas/JugadorTemporal.prefab";
        jugadorPrefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefab);
        if(jugadorPrefab==null)
        {
            EditorSceneManager.OpenScene(Ruta("EscenaInicial"));
            jugadorPrefab=PrefabUtility.SaveAsPrefabAsset(Object.FindAnyObjectByType<MovimientoPlayer>().gameObject,prefab);
        }
        foreach(var n in new[]{"Persecucion","EscapeVerticalLargo","LaboratorioValle","HabitacionFinal"})
        {
            var i=(TextureImporter)AssetImporter.GetAtPath("Assets/Sprites/Backgrounds/Pruebas/"+n+".png");
            i.textureShape=TextureImporterShape.Texture2D;i.textureType=TextureImporterType.Sprite;i.spriteImportMode=SpriteImportMode.Single;
            i.spritePixelsPerUnit=100;i.maxTextureSize=4096;i.mipmapEnabled=false;i.textureCompression=TextureImporterCompression.Uncompressed;i.SaveAndReimport();
        }
        Persecucion();Parkour();AjustarParkour();Valle();Final();
        var escenas=EditorBuildSettings.scenes.ToList();
        foreach(var n in new[]{"Persecucion","EscapeVertical","Valle","HabitacionFinal"})
            if(!escenas.Any(e=>e.path==Ruta(n)))escenas.Add(new EditorBuildSettingsScene(Ruta(n),true));
        EditorBuildSettings.scenes=escenas.ToArray();AssetDatabase.SaveAssets();
        PulirNiveles.Aplicar();
        IntegrarCortez.Aplicar();
        IntegrarJugadorGrecia.Aplicar();
    }
    static bool Nueva(string n)
    {
        if(File.Exists(Ruta(n)))return false;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        raiz=new GameObject("EscapeLaboratorio").transform;
        var cam=Go("Main Camera",0,0);cam.tag="MainCamera";cam.transform.position=new Vector3(0,0,-10);
        var c=cam.AddComponent<Camera>();c.orthographic=true;c.orthographicSize=5;c.backgroundColor=Color.black;c.clearFlags=CameraClearFlags.SolidColor;
        cam.AddComponent<AudioListener>();cam.AddComponent<CamaraNivel>();return true;
    }
    static void Guardar(string n){Directory.CreateDirectory(Path.GetDirectoryName(Ruta(n)));EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),Ruta(n));}
    static GameObject Go(string nombre,float x,float y)
    {var g=new GameObject(nombre);g.transform.SetParent(raiz,false);g.transform.position=new Vector3(x,y,0);return g;}
    static SpriteRenderer Arte(string nombre,Sprite sprite,float x,float y,float w,float h,int orden=5)
    {
        var g=Go(nombre,x,y);var sr=g.AddComponent<SpriteRenderer>();sr.sprite=sprite;sr.sharedMaterial=material;sr.sortingOrder=orden;
        g.transform.localScale=new Vector3(w/sprite.bounds.size.x,h/sprite.bounds.size.y,1);return sr;
    }
    static SpriteRenderer Fondo(string nombre,float x,float y,float w,float h)
    {return Arte("Fondo_"+nombre,AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Backgrounds/Pruebas/"+nombre+".png"),x,y,w,h,-20);}
    static BoxCollider2D Caja(string nombre,float x,float y,float w,float h,bool trigger=false)
    {var g=Go(nombre,x,y);var c=g.AddComponent<BoxCollider2D>();c.size=new Vector2(w,h);c.isTrigger=trigger;if(!trigger)g.layer=LayerMask.NameToLayer("Suelo");return c;}
    static void Plataforma(string nombre,float x,float top,float w,bool ilustrada=false)
    {
        var c=Caja(nombre,x,top-.1f,w,.2f);c.usedByEffector=true;
        var e=c.gameObject.AddComponent<PlatformEffector2D>();e.useOneWay=true;e.surfaceArc=170;
        if(ilustrada)Arte(nombre+"_Arte",repisa,x,top-.1f,w,.2f);
    }
    static MovimientoPlayer Jugador(float x,float y)
    {
        var go=(GameObject)PrefabUtility.InstantiatePrefab(jugadorPrefab);go.transform.SetParent(raiz,true);go.transform.position=new Vector3(x,y,0);go.name="PlayerTemporal";
        var entrada=Go("Entrada_Inicio",x,y).AddComponent<PuntoEntrada>();entrada.id="Inicio";
        return go.GetComponent<MovimientoPlayer>();
    }
    static void Checkpoint(float x,float y,float ancho=1.2f)
    {
        var c=Caja("PuntoControl",x,y,ancho,1.3f,true);c.gameObject.AddComponent<PuntoControl>().posicionSegura=new Vector3(x,y,0);
    }
    static EnemigoTemporal Enemigo(float x,float y,bool vertical=false,bool patrulla=false)
    {
        var g=Go(vertical?"EnemigoEscaladorTemporal":patrulla?"EnemigoVigilanteTemporal":"EnemigoPerseguidorTemporal",x,y);
        var c=g.AddComponent<BoxCollider2D>();c.isTrigger=true;c.size=new Vector2(1.05f,.8f);
        var e=g.AddComponent<EnemigoTemporal>();e.escalador=vertical;e.patrulla=patrulla;e.velocidad=vertical?.65f:patrulla?1.3f:3.15f;
        var cuerpo=Arte("CuerpoTemporal",blanco,x,y,1.05f,.65f,18);cuerpo.color=new Color(.16f,.045f,.045f);cuerpo.transform.SetParent(g.transform,true);
        foreach(float dx in new[]{-.32f,.32f})
        {
            var ojo=Arte("OjoTemporal",blanco,x+dx,y+.17f,.12f,.09f,19);ojo.color=new Color(1,.15f,.03f);ojo.transform.SetParent(g.transform,true);
            var pata=Arte("GarraTemporal",blanco,x+dx,y-.4f,.15f,.36f,18);pata.color=cuerpo.color;pata.transform.SetParent(g.transform,true);
        }
        return e;
    }
    static void Persecucion()
    {
        if(!Nueva("Persecucion"))return;
        for(int i=0;i<3;i++)Fondo("Persecucion",17.78f*i,0,17.78f,10);
        Caja("Suelo",17.78f,-1.75f,53.34f,.3f);Caja("LimiteIzquierdo",-8.85f,0,.2f,10);
        Jugador(-6.5f,-1.05f);
        for(int i=0;i<7;i++)
        {
            float x=-1+i*5.7f,h=i%2==0?.8f:1.15f;
            Caja("Obstaculo_"+i,x,-1.6f+h/2,.9f,h);Arte("Caja_"+i,caja,x,-1.6f+h/2,.9f,h);
        }
        Checkpoint(11,-1.05f);Checkpoint(28,-1.05f);
        Enemigo(-12,-1.05f);
        var salida=Caja("EntradaConducto",43,-.5f,1.4f,4,true).gameObject.AddComponent<SalidaEtapa>();salida.destino="EscapeVertical";salida.terminaPersecucion=true;
        var cam=Camera.main.GetComponent<CamaraNivel>();cam.seguirHorizontal=true;cam.minimoX=0;cam.maximoX=35.56f;
        Guardar("Persecucion");
    }
    static void Parkour()
    {
        if(!Nueva("EscapeVertical"))return;
        Fondo("EscapeVerticalLargo",0,15,10,30);
        // Las cotas siguen las plataformas de la ilustración. Los descansos añadidos salvan los huecos demasiado altos.
        float[,] puntos={ {-2,2},{0,3.35f},{2.15f,4.66f},{0,6.3f},{-2,8.03f},{0,9.65f},{2.4f,11.32f},{-2.3f,12.13f},{2.1f,13.89f},{-2.5f,15.4f},{2.55f,16.6f},{-1.8f,18.28f},{2.5f,19.75f},{-1.4f,21.06f},{2.1f,22.24f},{3,23.85f},{0,24.05f},{-2.6f,24.5f},{0,25.3f},{2.2f,25.97f},{0,27.44f} };
        for(int i=0;i<puntos.GetLength(0);i++)
        {
            bool extra=new[]{1,3,5,16,18}.Contains(i);
            float ancho=extra?1.3f:i==20?5.6f:i==0?3.8f:2.5f;
            Plataforma("Plataforma_"+i.ToString("D2"),puntos[i,0],puntos[i,1],ancho,extra);
            if(i==4 || i==9 || i==13)Checkpoint(puntos[i,0],puntos[i,1]+.55f);
        }
        Caja("ParedIzquierda",-4.7f,15,.2f,32);Caja("ParedDerecha",4.7f,15,.2f,32);
        var p=Jugador(-2,2.55f);p.fuerzaSalto=8;
        var recuperacion=p.GetComponent<RecuperarJugador>();recuperacion.seguirAlturaCheckpoint=true;recuperacion.limiteCaida=-3;
        // El parkour se recorre sin enemigo.
        var puerta=Caja("PuertaFinal",-.4f,28.15f,1.8f,2.4f,true).gameObject.AddComponent<PuertaFinal>();puerta.indicacion="Abrir salida";
        var cam=Camera.main.GetComponent<CamaraNivel>();cam.seguirVertical=true;cam.altura=5;cam.semiancho=8.89f;cam.minimoY=5;cam.maximoY=25;
        Guardar("EscapeVertical");
    }
    static void AjustarParkour()
    {
        EditorSceneManager.OpenScene(Ruta("EscapeVertical"));
        // Extremos medidos en la ilustración de referencia (683 px de ancho en la vista).
        float[,] bordes={{0,70,335},{2,365,630},{4,55,325},{6,445,590},{7,62,285},{8,415,620},{9,50,270},{10,445,590},{11,130,325},{12,425,620},{13,150,330},{14,405,570},{15,485,590},{17,60,270},{19,395,560},{20,125,495}};
        for(int i=0;i<bordes.GetLength(0);i++)
        {
            var c=GameObject.Find("Plataforma_"+((int)bordes[i,0]).ToString("D2")).GetComponent<BoxCollider2D>();
            var p=c.transform.position;p.x=-5+(bordes[i,1]+bordes[i,2])*.5f/683*10;c.transform.position=p;
            c.size=new Vector2((bordes[i,2]-bordes[i,1])/683*10,.2f);
        }
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }    static void Valle()
    {
        if(!Nueva("Valle"))return;
        var bg=Fondo("LaboratorioValle",0,0,30,16.88f);
        Caja("SueloIzquierdo",-2.4f,-2.58f,25.2f,.4f);
        var trampa=Caja("SueloQueCede",11,-2.58f,1.6f,.4f);
        Caja("SueloDerecho",13.4f,-2.58f,3.2f,.4f);
        Caja("LimiteIzquierdo",-14.9f,0,.2f,20);Caja("LimiteDerecho",14.9f,0,.2f,20);
        Jugador(-11,-1.83f);
        var activador=Caja("ZanahoriaTrampa",10.8f,-1.85f,1.2f,1.6f,true).gameObject.AddComponent<TrampaZanahoria>();activador.sueloTrampa=trampa;
        var hueco=Arte("HuecoDelTunel",blanco,11,-6.4f,1.65f,8,2);hueco.color=new Color(.018f,.025f,.026f);hueco.gameObject.SetActive(false);activador.hueco=hueco.gameObject;
        // Recorte de suelo de la misma imagen: cae con el jugador, dejando a la vista el hueco.
        string ruta="Assets/Sprites/Ambiente/LosaValle.asset";var sprite=AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if(sprite==null)
        {
            var rect=bg.sprite.rect; sprite=Sprite.Create(bg.sprite.texture,new Rect(rect.x+rect.width*.84f,rect.y+rect.height*.345f,rect.width*.06f,rect.height*.035f),new Vector2(.5f,.5f),100);
            AssetDatabase.CreateAsset(sprite,ruta);
        }
        activador.losa=Arte("LosaTrampa",sprite,11,-2.55f,1.65f,.4f,3).transform;
        var cam=Camera.main.GetComponent<CamaraNivel>();cam.seguirHorizontal=true;cam.minimoX=-6.1f;cam.maximoX=6.1f;
        Guardar("Valle");
    }
    static void Final()
    {
        if(!Nueva("HabitacionFinal"))return;
        Fondo("HabitacionFinal",0,0,17.78f,10);Caja("Suelo",0,-2.35f,17.78f,.4f);
        Caja("Colchon",-3,-1.6f,5.4f,.7f);Caja("ParedIzquierda",-8.8f,0,.2f,10);Caja("ParedDerecha",8.8f,0,.2f,10);
        Jugador(-3,3.7f);var entrada=Go("Entrada_Caida",-3,3.7f).AddComponent<PuntoEntrada>();entrada.id="Caida";
        Go("RegistroFinal",0,0).AddComponent<FinalExperimento>();Guardar("HabitacionFinal");
    }
}




