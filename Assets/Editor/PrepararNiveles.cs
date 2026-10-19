using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

// Constructor reproducible. Solo añade contenido cuando no existe su raíz de progresión.
public static class PrepararNiveles
{
    const string Inicial = "Assets/Scenes/EscenaInicial/EscenaInicial.unity";
    const string Mantenimiento = "Assets/Scenes/Nivel1/Nivel1.unity";
    const string Raiz = "ProgresionLaboratorio";
    static Sprite blanco;
    static Material material;
    static Transform grupo;
    static readonly Color Ambar = new Color(0.9f,0.65f,0.27f);

    [MenuItem("GAMEJAM/Preparar niveles faltantes")]
    public static void Construir()
    {
        Directory.SetCurrentDirectory(Path.GetDirectoryName(Application.dataPath));
        if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
        PrepararRecursos();
        PrepararInicial();
        PrepararMantenimiento();
        CrearPasillo();
        CrearDerrumbe();
        CrearCorredor();
        var escenas = EditorBuildSettings.scenes.ToList();
        foreach (string ruta in new[] { Inicial, Mantenimiento, Ruta("PasilloPrincipal"), Ruta("PuertaA"), Ruta("PuertaC") })
        {
            var existente = escenas.FirstOrDefault(s => s.path == ruta);
            if (existente != null) existente.enabled = true;
            else escenas.Add(new EditorBuildSettingsScene(ruta, true));
        }
        EditorBuildSettings.scenes = escenas.ToArray();
        IntegrarAmbiente.Aplicar();
        PrepararEscape.Construir();
        AssetDatabase.SaveAssets();
        EditorSceneManager.OpenScene(Inicial);
        Debug.Log("GAMEJAM: etapas 2–6 construidas. Escenas listas para Play.");
    }

    static string Ruta(string nombre) { return "Assets/Scenes/" + nombre + "/" + nombre + ".unity"; }
    static void PrepararRecursos()
    {
        Directory.CreateDirectory("Assets/Sprites/Prototipos");
        string pixel = "Assets/Sprites/Prototipos/Blanco.png";
        if (!File.Exists(pixel))
        {
            var t = new Texture2D(4,4);
            t.SetPixels(Enumerable.Repeat(Color.white,16).ToArray()); t.Apply();
            File.WriteAllBytes(pixel,t.EncodeToPNG()); Object.DestroyImmediate(t);
        }
        AssetDatabase.Refresh();
        foreach (string p in new[] { pixel, "Assets/Sprites/Backgrounds/Niveles/PasilloPrincipal.png", "Assets/Sprites/Backgrounds/Niveles/Derrumbe.png", "Assets/Sprites/Backgrounds/Niveles/Corredor.png" })
        {
            var importer = (TextureImporter)AssetImporter.GetAtPath(p);
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.spritePixelsPerUnit = p == pixel ? 4 : 100;
            importer.maxTextureSize = 4096;
            importer.textureCompression = TextureImporterCompression.Uncompressed;
            importer.mipmapEnabled = false;
            var ajustes = new TextureImporterSettings();
            importer.ReadTextureSettings(ajustes); ajustes.spriteMeshType = SpriteMeshType.FullRect;
            importer.SetTextureSettings(ajustes);
            importer.SaveAndReimport();
        }
        blanco = AssetDatabase.LoadAssetAtPath<Sprite>(pixel);
        string matPath = "Assets/Sprites/Prototipos/IlustracionSinLuz.mat";
        material = AssetDatabase.LoadAssetAtPath<Material>(matPath);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default"));
            AssetDatabase.CreateAsset(material,matPath);
        }
    }

    static bool AbrirExistente(string ruta)
    {
        EditorSceneManager.OpenScene(ruta);
        if (GameObject.Find(Raiz) != null) return false;
        grupo = new GameObject(Raiz).transform;
        return true;
    }
    static bool Nueva(string nombre)
    {
        if (File.Exists(Ruta(nombre))) return false;
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        grupo = new GameObject(Raiz).transform;
        var cam = new GameObject("Main Camera",typeof(Camera),typeof(AudioListener),typeof(CamaraNivel));
        cam.tag = "MainCamera"; cam.transform.position = new Vector3(0,0,-10);
        var c = cam.GetComponent<Camera>(); c.orthographic = true; c.orthographicSize = 5;
        c.clearFlags = CameraClearFlags.SolidColor; c.backgroundColor = new Color(0.015f,0.02f,0.025f);
        return true;
    }
    static void Guardar(string nombre)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Ruta(nombre)));
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),Ruta(nombre));
    }
    static GameObject Go(string nombre, float x, float y)
    {
        var go = new GameObject(nombre); go.transform.SetParent(grupo,false);
        go.transform.position = new Vector3(x,y,0); return go;
    }
    static SpriteRenderer Forma(string nombre, float x, float y, float w, float h, Color color, int orden = 5)
    {
        var go = Go(nombre,x,y); var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = blanco; sr.sharedMaterial = material; sr.drawMode = SpriteDrawMode.Sliced;
        sr.size = new Vector2(w,h); sr.color = color; sr.sortingOrder = orden; return sr;
    }
    static void Fondo(string nombre, float centro = 0)
    {
        var sr = Go("Fondo_" + nombre,centro,0).AddComponent<SpriteRenderer>();
        sr.sprite = AssetDatabase.LoadAssetAtPath<Sprite>("Assets/Sprites/Backgrounds/Niveles/" + nombre + ".png");
        sr.sharedMaterial = material; sr.sortingOrder = -20;
        sr.transform.localScale = new Vector3(17.78f / sr.sprite.bounds.size.x,10f / sr.sprite.bounds.size.y,1);
    }
    static BoxCollider2D Colision(GameObject go, float w, float h, bool trigger = false)
    {
        var c = go.AddComponent<BoxCollider2D>(); c.size = new Vector2(w,h); c.isTrigger = trigger;
        if (!trigger) go.layer = LayerMask.NameToLayer("Suelo"); return c;
    }
    static void Suelo(float y, float centro = 0, float ancho = 17.78f)
    { Colision(Go("Suelo",centro,y - 0.2f),ancho,0.4f); }
    static void Limites(float izquierda = -8.85f, float derecha = 8.85f)
    {
        Colision(Go("LimiteIzquierdo",izquierda,0),0.2f,12);
        Colision(Go("LimiteDerecho",derecha,0),0.2f,12);
    }
    static TextMesh Rotulo(string texto, float x, float y, float ancho = 2.4f)
    {
        Forma("Placa",x,y,ancho,0.38f,new Color(0.04f,0.065f,0.07f),10);
        var go = Go("Rotulo_" + texto,x,y);
        var t = go.AddComponent<TextMesh>(); t.text = texto; t.fontSize = 48; t.characterSize = 0.065f;
        t.anchor = TextAnchor.MiddleCenter; t.alignment = TextAlignment.Center; t.color = Ambar;
        go.GetComponent<MeshRenderer>().sortingOrder = 11; return t;
    }
    static void Entrada(string id, float x, float y) { Go("Entrada_" + id,x,y).AddComponent<PuntoEntrada>().id = id; }
    static PuertaCambioEscena Puerta(string nombre, float x, float y, string destino, string entrada, string texto)
    {
        var go = Go(nombre,x,y); Colision(go,1.15f,2.1f,true);
        var d = go.AddComponent<PuertaCambioEscena>(); d.escenaDestino = destino;
        d.entradaDestino = entrada; d.indicacion = texto; return d;
    }
    static void AjustarJugador(MovimientoPlayer p, Vector2 posicion)
    {
        p.transform.position = posicion; p.transform.localScale = Vector3.one;
        var capsule = p.GetComponent<CapsuleCollider2D>();
        if (capsule != null) { capsule.size = new Vector2(0.6f,0.95f); capsule.offset = Vector2.zero; }
        var box = p.GetComponent<BoxCollider2D>();
        if (box != null) { box.size = new Vector2(0.6f,0.95f); box.offset = Vector2.zero; }
        p.groundCheck.localPosition = new Vector3(0,-0.51f,0);
        p.capaSuelo = LayerMask.GetMask("Suelo"); p.radioSuelo = 0.12f;
        var rb = p.GetComponent<Rigidbody2D>(); rb.constraints = RigidbodyConstraints2D.FreezeRotation;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous; rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        if (p.GetComponent<InteraccionPlayer>() == null) p.gameObject.AddComponent<InteraccionPlayer>();
        if (p.GetComponent<RecuperarJugador>() == null) p.gameObject.AddComponent<RecuperarJugador>();
        var sr = p.GetComponent<SpriteRenderer>(); sr.sprite = blanco; sr.sharedMaterial = material;
        sr.drawMode = SpriteDrawMode.Sliced; sr.size = new Vector2(0.6f,0.95f);
        sr.color = new Color(0.08f,0.085f,0.11f); sr.sortingOrder = 20;
        foreach (float x in new[] { -0.18f,0.18f })
        {
            var oreja = Forma("OrejaTemporal",0,0,0.15f,0.42f,sr.color,20);
            oreja.transform.SetParent(p.transform,false); oreja.transform.localPosition = new Vector3(x,0.63f,0);
            var ojo = Forma("OjoTemporal",0,0,0.07f,0.08f,Color.white,21);
            ojo.transform.SetParent(p.transform,false); ojo.transform.localPosition = new Vector3(x,0.22f,0);
        }
    }
    static void Jugador(float x, float y)
    {
        var go = Go("PlayerTemp",x,y); go.tag = "Player";
        go.AddComponent<SpriteRenderer>(); go.AddComponent<CapsuleCollider2D>(); go.AddComponent<Rigidbody2D>();
        var p = go.AddComponent<MovimientoPlayer>();
        p.groundCheck = new GameObject("GroundCheck").transform; p.groundCheck.SetParent(go.transform);
        AjustarJugador(p,new Vector2(x,y));
    }
    static void ColliderExistente(string nombre, Vector2 centro, Vector2 tamano)
    {
        var go = GameObject.Find(nombre); var c = go.GetComponent<BoxCollider2D>();
        go.transform.position = centro; c.offset = Vector2.zero; c.size = tamano;
        go.layer = LayerMask.NameToLayer("Suelo");
    }

    static void PrepararInicial()
    {
        if (!AbrirExistente(Inicial)) return;
        var p = Object.FindAnyObjectByType<MovimientoPlayer>();
        AjustarJugador(p,new Vector2(-7.7f,-1.25f));
        ColliderExistente("Ground",new Vector2(0,-2.5f),new Vector2(17.6f,0.3f));
        ColliderExistente("LeftWall",new Vector2(-8.85f,0),new Vector2(0.2f,10));
        ColliderExistente("RightWall",new Vector2(8.85f,0),new Vector2(0.2f,10));
        ColliderExistente("Piso",new Vector2(-7.7f,-1.95f),new Vector2(1.8f,0.15f));
        ColliderExistente("Techo",new Vector2(-7.7f,0.3f),new Vector2(1.8f,0.15f));
        ColliderExistente("ParedIzquierda",new Vector2(-8.6f,-0.8f),new Vector2(0.15f,2.3f));
        ColliderExistente("ParedDerecha",new Vector2(-6.8f,-0.8f),new Vector2(0.15f,2.3f));
        ColliderExistente("PuertaJaula",new Vector2(-6.8f,-0.8f),new Vector2(0.15f,2.3f));
        // Barrera temporal visible para que la apertura no dependa de la jaula dibujada en el fondo.
        var barra = Forma("CierreJaulaTemporal",-6.8f,-0.8f,0.12f,2.3f,Ambar);
        barra.transform.SetParent(GameObject.Find("JaulaInicial").transform,true);
        var d = Object.FindAnyObjectByType<PuertaCambioEscena>();
        d.escenaDestino = "PasilloPrincipal"; d.entradaDestino = "DesdeInicial"; d.indicacion = "Entrar al pasillo principal";
        d.transform.position = new Vector3(4.75f,-1.2f,0);
        var dc = d.GetComponent<BoxCollider2D>(); dc.offset = Vector2.zero; dc.size = new Vector2(1.2f,2.2f);
        var boton = Object.FindAnyObjectByType<BotonJaula>();
        boton.transform.position = new Vector3(-1.16f,-0.47f,0);
        var bc = boton.GetComponent<BoxCollider2D>(); bc.offset = Vector2.zero; bc.size = new Vector2(0.65f,0.7f);
        Forma("CartelJaulaLED",-1.16f,2.1f,4.55f,0.78f,new Color(0.12f,0.008f,0.012f),10);
        var textoCartel=Go("Rotulo_PRESIONA EL BOTON ROJO",-1.16f,2.1f).AddComponent<TextMesh>();
        textoCartel.text="PRESIONA EL BOTON ROJO\nPARA ABRIR LA JAULA";
        textoCartel.fontSize=48;textoCartel.characterSize=0.065f;textoCartel.fontStyle=FontStyle.Bold;
        textoCartel.anchor=TextAnchor.MiddleCenter;textoCartel.alignment=TextAlignment.Center;
        textoCartel.color=new Color(1f,0.035f,0.025f);
        textoCartel.GetComponent<MeshRenderer>().sortingOrder=11;
        var cam = Camera.main.gameObject.AddComponent<CamaraNivel>(); cam.altura = 5;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }

    static void PrepararMantenimiento()
    {
        if (!AbrirExistente(Mantenimiento)) return;
        AjustarJugador(Object.FindAnyObjectByType<MovimientoPlayer>(),new Vector2(-7.25f,-1.3f));
        ColliderExistente("Ground",new Vector2(0,-2.16f),new Vector2(17.6f,0.4f));
        ColliderExistente("LeftWall",new Vector2(-8.8f,0),new Vector2(0.2f,10));
        ColliderExistente("RightWall",new Vector2(8.8f,0),new Vector2(0.2f,10));
        var nota = Object.FindAnyObjectByType<NotaInteractuable>();
        nota.textoNota = "PROTOCOLO DE MANTENIMIENTO\n\nInstala el fusible en su soporte.\nDespués: abre el FLUJO con la válvula,\najusta la PRESIÓN con el regulador\ny activa la ENERGÍA con el botón rojo.\n\nUn orden incorrecto reinicia la secuencia.\nEl compartimento contiene una llave maestra.";
        // La imagen original se conserva en el Canvas; esta nota usa el texto actualizado.
        nota.imagenNota = null;
        nota.transform.position = new Vector3(-1.8f,-0.5f,0);
        var nc = nota.GetComponent<BoxCollider2D>(); nc.size = new Vector2(1.3f/nota.transform.lossyScale.x,1.5f/nota.transform.lossyScale.y); nc.offset = Vector2.zero;
        Rotulo("NOTA · E",-1.8f,0.55f,1.8f);
        Puerta("RegresoPasillo",-7.6f,-1,"PasilloPrincipal","DesdeB","Volver al pasillo");
        Rotulo("VOLVER · E",-7.4f,0.4f,2.1f); Entrada("DesdePasillo",-6.7f,-1.25f);
        Mecanismo("Portafusible",-4.7f,-0.85f,true,0,false,"Instalar fusible");
        Mecanismo("ValvulaFlujo",-3.25f,-0.85f,false,0,true,"Abrir flujo");
        Mecanismo("ReguladorPresion",1f,-0.85f,false,1,true,"Ajustar presión");
        Mecanismo("BotonRojo",2.25f,-0.3f,false,2,true,"Activar energía");
        var llave = Forma("CompartimentoLlave",5.4f,-0.95f,0.65f,0.35f,Ambar).gameObject;
        Colision(llave,1.25f,1.8f,true);
        var o = llave.AddComponent<ObjetoRecogible>(); o.objetoId = EstadoPartida.LlaveMaestra;
        o.nombreVisible = "llave maestra"; o.indicacion = "Examinar compartimento / recoger llave"; o.requiereMantenimiento = true;
        Rotulo("COMPARTIMENTO",5.4f,0.25f,2.8f);
        Camera.main.gameObject.AddComponent<CamaraNivel>();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }
    static void Mecanismo(string nombre,float x,float y,bool fusible,int paso,bool clic,string indicacion)
    {
        var go = Forma(nombre,x,y,0.35f,0.35f,Ambar).gameObject;
        Colision(go,0.95f,1.8f,true);
        var m = go.AddComponent<MecanismoMantenimiento>(); m.esPortafusible = fusible; m.paso = paso; m.permiteClic = clic; m.indicacion = indicacion;
        Rotulo(fusible ? "FUSIBLE" : paso == 0 ? "FLUJO" : paso == 1 ? "PRESIÓN" : "ENERGÍA",x,y+0.6f,1.35f);
    }

    static void CrearPasillo()
    {
        if (!Nueva("PasilloPrincipal")) return;
        Fondo("PasilloPrincipal"); Suelo(-2.12f); Limites(); Jugador(-7.6f,-1.5f);
        Puerta("PuertaA",-6.8f,-1,"PuertaA","DesdePasillo","Entrar a A · derrumbe");
        Puerta("PuertaB",0.9f,-1,"Nivel1","DesdePasillo","Entrar a B · mantenimiento");
        var c = Puerta("PuertaC",7f,-1,"PuertaC","DesdePasillo","Entrar a C · evacuación");
        c.requiereLlaveMaestra = true; c.puertaC = true;
        Rotulo("A · DERRUMBE",-6.8f,1.1f,2.6f);
        Rotulo("B · MANTENIMIENTO",0.9f,1.1f,3.2f);
        Rotulo("C · EVACUACIÓN",7f,1.1f,2.7f).gameObject.AddComponent<RotuloPuertaC>();
        Entrada("DesdeInicial",-7.9f,-1.45f); Entrada("DesdeA",-5.6f,-1.45f);
        Entrada("DesdeB",2.15f,-1.45f); Entrada("DesdeC",5.8f,-1.45f);
        Guardar("PasilloPrincipal");
    }

    static void CrearDerrumbe()
    {
        if (!Nueva("PuertaA")) return;
        Fondo("Derrumbe"); Suelo(-2.65f); Limites(); Jugador(-7.5f,-2);
        Entrada("DesdePasillo",-7.5f,-2);
        Puerta("RegresoPasillo",-7.7f,-1.5f,"PasilloPrincipal","DesdeA","Volver al pasillo");
        Rotulo("VOLVER · E",-7.3f,-0.3f,2.2f);
        Plataforma("Escombro01",-5.1f,-2.25f,1.1f,0.8f);
        Plataforma("Escombro02",-3.3f,-1.85f,1.1f,1.6f);
        Plataforma("PasarelaRota",-0.9f,-0.9f,1.8f,0.25f);
        Plataforma("CajaIndustrial",1.7f,-2.1f,1.3f,1.1f);
        Plataforma("RepisaFusible",4.3f,-0.8f,1.65f,0.25f);
        var go = Forma("FusibleTemporal",4.3f,-0.25f,0.45f,0.18f,new Color(0.5f,0.95f,0.85f)).gameObject;
        Colision(go,1.1f,1.1f,true);
        var f = go.AddComponent<ObjetoRecogible>(); f.objetoId = EstadoPartida.Fusible; f.nombreVisible = "fusible"; f.indicacion = "Recoger fusible";
        Rotulo("FUSIBLE · E",4.3f,0.5f,2.1f);
        Guardar("PuertaA");
    }
    static void Plataforma(string nombre,float x,float y,float w,float h)
    {
        var go = Forma(nombre,x,y,w,h,new Color(0.19f,0.23f,0.25f)).gameObject;
        Colision(go,w,h);
        Forma(nombre+"_Borde",x,y+h/2, w,0.07f,Ambar);
    }

    static void CrearCorredor()
    {
        if (!Nueva("PuertaC")) return;
        var luces = new System.Collections.Generic.List<SpriteRenderer>();
        for (int i = -1; i <= 5; i++)
        {
            float centro = i * 17.78f;
            Fondo("Corredor",centro);
            Forma("CartelFondo",centro-2.7f,0.48f,2.3f,1.35f,new Color(0.07f,0.09f,0.1f),1);
            var texto = Rotulo("SALIDA A\n" + (i < 0 ? 10 : i == 5 ? 50 : 50-i*10) + " METROS  →",centro-2.7f,0.48f,2.35f);
            texto.characterSize = 0.085f;
            luces.Add(Forma("LamparaParpadeante",centro+0.9f,1.15f,0.8f,0.08f,Ambar));
        }
        Suelo(-1.9f,40,118); Limites(-8.8f,99);
        Jugador(-7,-1.2f); Entrada("DesdePasillo",-7,-1.2f);
        var regreso = Puerta("PuertaRetornoRevelada",-7.6f,-0.85f,"PasilloPrincipal","DesdeC","Salir del bucle · volver al pasillo");
        var cartel = Rotulo("REGRESAR · E",-7.4f,0.7f,2.5f);
        cartel.transform.SetParent(regreso.transform,true);
        // También ocultar la placa del rótulo hasta que se revele la puerta.
        var fondoCartel = grupo.GetComponentsInChildren<SpriteRenderer>().Last(r => r.name == "Placa");
        fondoCartel.transform.SetParent(regreso.transform,true);
        var loop = Go("ControlBucle",0,0).AddComponent<CorredorInfinito>();
        loop.puertaRegreso = regreso.gameObject; loop.finalX = 81.9f;
        loop.lamparas = luces.ToArray();
        regreso.gameObject.SetActive(false);
        var cam = Camera.main.GetComponent<CamaraNivel>(); cam.seguirHorizontal = true; cam.minimoX = -7; cam.maximoX = 82;
        Guardar("PuertaC");
    }
}


