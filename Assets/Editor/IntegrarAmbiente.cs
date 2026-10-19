using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public static class IntegrarAmbiente
{
    const string Carpeta = "Assets/Sprites/Ambiente/";
    static Material material;
    static T Componente<T>(GameObject go) where T : Component
    {
        var c=go.GetComponent<T>();return c!=null?c:go.AddComponent<T>();
    }
    static Sprite Importar(string nombre)
    {
        string ruta = Carpeta + nombre + ".png";
        var i = (TextureImporter)AssetImporter.GetAtPath(ruta);
        i.textureShape = TextureImporterShape.Texture2D; i.textureType = TextureImporterType.Sprite; i.spriteImportMode = SpriteImportMode.Single;
        i.isReadable = true; i.maxTextureSize = 2048; i.mipmapEnabled = false;
        i.textureCompression = TextureImporterCompression.Uncompressed; i.SaveAndReimport();
        var textura = AssetDatabase.LoadAssetAtPath<Sprite>(ruta).texture;
        var pixeles = textura.GetPixels32(); int x0=textura.width,y0=textura.height,x1=0,y1=0;
        for(int y=0;y<textura.height;y++) for(int x=0;x<textura.width;x++)
            if(pixeles[y*textura.width+x].a>15) { x0=Mathf.Min(x0,x);y0=Mathf.Min(y0,y);x1=Mathf.Max(x1,x);y1=Mathf.Max(y1,y); }
        return Recorte(nombre,textura,new Rect(x0,y0,x1-x0+1,y1-y0+1));
    }
    static Sprite Recorte(string nombre,Texture2D textura,Rect rect)
    {
        string ruta=Carpeta+nombre+".asset";
        var existente=AssetDatabase.LoadAssetAtPath<Sprite>(ruta);
        if(existente!=null) return existente;
        var sprite=Sprite.Create(textura,rect,new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect);
        sprite.name=nombre; AssetDatabase.CreateAsset(sprite,ruta); return sprite;
    }
    static Sprite ManijaCircular(Sprite baseSprite)
    {
        var sprite=Recorte("ManijaCircular",baseSprite.texture,baseSprite.rect);
        const int lados=32;var vertices=new Vector2[lados+1];var triangulos=new ushort[lados*3];
        float radio=Mathf.Min(sprite.bounds.size.x,sprite.bounds.size.y)*.5f;
        for(int i=0;i<lados;i++)
        {
            float a=i*Mathf.PI*2/lados;vertices[i+1]=new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radio;
            triangulos[i*3]=0;triangulos[i*3+1]=(ushort)(i+1);triangulos[i*3+2]=(ushort)((i+1)%lados+1);
        }
        sprite.OverrideGeometry(vertices,triangulos);EditorUtility.SetDirty(sprite);return sprite;
    }
    static void Vestir(SpriteRenderer sr,Sprite sprite,Vector2 dimensiones)
    {
        sr.sprite=sprite;sr.drawMode=SpriteDrawMode.Simple;sr.color=Color.white;sr.sharedMaterial=material;
        sr.transform.localScale=Vector3.one;
        // El collider sigue en unidades del mundo: ajustar solamente el tamaño del sprite mediante un hijo.
        var hijo=sr.transform.Find("DetalleAmbiental");
        if(hijo==null) { hijo=new GameObject("DetalleAmbiental").transform;hijo.SetParent(sr.transform,false); }
        var visual=Componente<SpriteRenderer>(hijo.gameObject);
        visual.sprite=sprite;visual.sharedMaterial=material;visual.sortingOrder=sr.sortingOrder;
        hijo.localScale=new Vector3(dimensiones.x/sprite.bounds.size.x,dimensiones.y/sprite.bounds.size.y,1);
        sr.enabled=false;
    }
    [MenuItem("GAMEJAM/Integrar objetos en el ambiente")]
    public static void Aplicar()
    {
        AssetDatabase.Refresh();
        material=AssetDatabase.LoadAssetAtPath<Material>("Assets/Sprites/Prototipos/IlustracionSinLuz.mat");
        var soporteArte=Importar("Portafusible");var notaArte=Importar("NotaMantenimiento");
        var fusible=Importar("Fusible");var llave=Importar("LlaveMaestra");var caja=Importar("CajaIndustrial");
        foreach(string nombre in new[]{"EscenaInicial","Nivel1","PasilloPrincipal","PuertaA","PuertaC"})
        {
            EditorSceneManager.OpenScene("Assets/Scenes/"+nombre+"/"+nombre+".unity");
            var objetos=SceneManager.GetActiveScene().GetRootGameObjects().SelectMany(r=>r.GetComponentsInChildren<Transform>(true)).ToArray();
            foreach(var t in objetos)
            {
                if(t.name=="Placa" || t.name=="CierreJaulaTemporal" || t.name.EndsWith("_Borde"))
                { var r=t.GetComponent<Renderer>();if(r!=null)r.enabled=false; }
                if(t.name.StartsWith("Rotulo_") && !t.name.Contains("SALIDA A") && t.GetComponent<RotuloPuertaC>()==null)
                    t.gameObject.SetActive(false);
            }
            if(nombre=="Nivel1")
            {
                var nota=Object.FindAnyObjectByType<NotaInteractuable>();
                nota.imagenNota=objetos.First(t=>t.name=="ImagenNota").gameObject;nota.textoNota="";
                var imagen=nota.imagenNota.GetComponent<Image>();imagen.preserveAspect=true;imagen.sprite=notaArte;
                var rect=imagen.rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
                rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(720,720);rect.localScale=Vector3.one;
                var canvas=imagen.GetComponentInParent<Canvas>(true);canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=200;
                var scaler=Componente<CanvasScaler>(canvas.gameObject);
                scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1600,900);
                nota.imagenNota.SetActive(false);
                var bg=GameObject.Find("BG_Nivel1").GetComponent<SpriteRenderer>();
                foreach(var m in Object.FindObjectsByType<MecanismoMantenimiento>())
                {
                    if (!m.esPortafusible && m.paso == 1) { m.gameObject.SetActive(false); continue; }
                    Vector2 pixel=m.esPortafusible?new Vector2(179,565):m.paso==0?new Vector2(486,493):m.paso==1?new Vector2(433,407):new Vector2(1046,516);
                    Vector3 p=new Vector3(bg.bounds.min.x+pixel.x/1672f*bg.bounds.size.x,bg.bounds.max.y-pixel.y/941f*bg.bounds.size.y,0);
                    m.transform.position=p;m.permiteClic=true;m.tamanoClic=new Vector2(.65f,.65f);
                    var collider=m.GetComponent<BoxCollider2D>();float bajo=-1.8f;
                    collider.offset=new Vector2(0,(bajo-p.y)/2);collider.size=new Vector2(.65f,p.y-bajo+.65f);
                    var sr=m.GetComponent<SpriteRenderer>();sr.enabled=false;
                    var detalle=m.transform.Find("DetalleMecanico");
                    if(detalle==null){detalle=new GameObject("DetalleMecanico").transform;detalle.SetParent(m.transform,false);}
                    var r=Componente<SpriteRenderer>(detalle.gameObject);
                    var tex=bg.sprite.texture;var original=bg.sprite.rect;
                    float ancho=m.esPortafusible?35:35,alto=m.esPortafusible?45:35;
                    r.sprite=Recorte(m.name+"Detalle",tex,new Rect(original.x+(pixel.x-ancho/2)/1672*original.width,original.y+(941-pixel.y-alto/2)/941*original.height,ancho/1672*original.width,alto/941*original.height));
                    r.sharedMaterial=material;r.sortingOrder=5;
                    detalle.localScale=new Vector3(bg.bounds.size.x/1672*ancho/r.sprite.bounds.size.x,bg.bounds.size.y/941*alto/r.sprite.bounds.size.y,1);
                    m.respuesta=Componente<RespuestaAmbiental>(detalle.gameObject);m.respuesta.detalle=r;
                    if(!m.esPortafusible && m.paso==0) { r.sprite=ManijaCircular(r.sprite);m.respuesta.giroActivo=80f; }
                    if(m.esPortafusible)
                    {
                        r.sprite=soporteArte;
                        detalle.localScale=new Vector3(1.1f/soporteArte.bounds.size.x,.62f/soporteArte.bounds.size.y,1);
                        var instalado=m.transform.Find("FusibleInstalado");
                        if(instalado==null){instalado=new GameObject("FusibleInstalado").transform;instalado.SetParent(m.transform,false);}
                        instalado.localPosition=new Vector3(0,-.02f,0);
                        instalado.localScale=new Vector3(.55f/fusible.bounds.size.x,.18f/fusible.bounds.size.y,1);
                        var visual=Componente<SpriteRenderer>(instalado.gameObject);visual.sprite=fusible;visual.sharedMaterial=material;visual.sortingOrder=7;
                        m.fusibleVisible=instalado.gameObject;m.fusibleVisible.SetActive(false);
                        m.tamanoClic=new Vector2(1.15f,.8f);
                    }
                }
                var o=Object.FindAnyObjectByType<ObjetoRecogible>();o.enPuerta=true;o.gameObject.name="PuertaEntregaLlave";o.transform.position=new Vector3(bg.bounds.min.x+1469f/1672*bg.bounds.size.x,bg.bounds.max.y-520f/941*bg.bounds.size.y,0);
                var srLlave=o.GetComponent<SpriteRenderer>();srLlave.sprite=llave;srLlave.drawMode=SpriteDrawMode.Simple;srLlave.color=Color.white;
                o.transform.localScale=Vector3.one;
                // sprite rect en unidades pequeñas sin escalar el área interactuable
                srLlave.sprite=Recorte("LlaveEnEstante",llave.texture,llave.rect);
                var dimensiones=srLlave.sprite.bounds.size;
                o.transform.localScale=new Vector3(.75f/dimensiones.x,.30f/dimensiones.y,1);
                var c=o.GetComponent<BoxCollider2D>();c.size=new Vector2(1.2f/o.transform.localScale.x,2.8f/o.transform.localScale.y);
                c.offset=new Vector2(0,-.65f/o.transform.localScale.y);
                o.permiteClic=true;o.tamanoClic=new Vector2(1.2f,2.8f);
            }
            if(nombre=="PuertaA")
            {
                foreach(var sr in Object.FindObjectsByType<SpriteRenderer>())
                {
                    if(new[]{"Escombro01","Escombro02","PasarelaRota","CajaIndustrial","RepisaFusible"}.Contains(sr.name))
                    {
                        var c=sr.GetComponent<BoxCollider2D>();
                        var arte=c.size.y<.4f?Recorte("BordeCaja",caja.texture,new Rect(caja.rect.x,caja.rect.y+caja.rect.height*.78f,caja.rect.width,caja.rect.height*.15f)):caja;
                        Vestir(sr,arte,c.size);
                    }
                    if(sr.name=="FusibleTemporal")
                    {
                        sr.transform.position=new Vector3(4.3f,-.55f,0);
                        sr.sprite=fusible;sr.drawMode=SpriteDrawMode.Simple;sr.color=Color.white;
                        sr.transform.localScale=new Vector3(.75f/fusible.bounds.size.x,.27f/fusible.bounds.size.y,1);
                        sr.GetComponent<BoxCollider2D>().size=new Vector2(1.1f/sr.transform.localScale.x,1.1f/sr.transform.localScale.y);
                    }
                }
            }
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        }
        AssetDatabase.SaveAssets();
    }
}








