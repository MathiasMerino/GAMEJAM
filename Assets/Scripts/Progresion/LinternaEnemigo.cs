using UnityEngine;

// Haz translúcido visible también sobre los fondos ilustrados sin iluminación URP.
[DefaultExecutionOrder(100)]
public sealed class LinternaEnemigo : MonoBehaviour
{
    EnemigoTemporal enemigo;
    Mesh malla;
    Material material;
    GameObject haz;
    const int Rayos=40;
    readonly Vector3[] vertices=new Vector3[Rayos+2];
    readonly Color[] colores=new Color[Rayos+2];
    void Start()
    {
        enemigo=GetComponentInParent<EnemigoTemporal>();
        enemigo.emisor=transform;
        haz=new GameObject("HazLinterna");
        haz.transform.SetParent(enemigo.transform,false);
        malla=new Mesh { name="Cono de visión" };
        haz.AddComponent<MeshFilter>().sharedMesh=malla;
        material=new Material(Shader.Find("Sprites/Default"));
        var r=haz.AddComponent<MeshRenderer>(); r.sharedMaterial=material; r.sortingOrder=17;
        var indices=new int[Rayos*3];
        for(int i=0;i<Rayos;i++){indices[i*3]=0;indices[i*3+1]=i+1;indices[i*3+2]=i+2;}
        malla.vertices=vertices; malla.triangles=indices;
    }
    void LateUpdate()
    {
        if(enemigo==null)return;
        Vector2 origen=enemigo.Emisor;
        vertices[0]=haz.transform.InverseTransformPoint(origen);
        colores[0]=new Color(1,.88f,.55f,.32f);
        float angulo=Mathf.Atan2(enemigo.DireccionLuz.y,enemigo.DireccionLuz.x)*Mathf.Rad2Deg;
        for(int i=0;i<=Rayos;i++)
        {
            float a=(angulo-enemigo.semiAnguloLinterna+2*enemigo.semiAnguloLinterna*i/Rayos)*Mathf.Deg2Rad;
            var direccion=new Vector2(Mathf.Cos(a),Mathf.Sin(a));
            float distancia=enemigo.DistanciaLibre(direccion,enemigo.alcanceLinterna);
            vertices[i+1]=haz.transform.InverseTransformPoint(origen+direccion*distancia);
            colores[i+1]=new Color(1,.88f,.55f,.025f*(1-Mathf.Abs(2f*i/Rayos-1)));
        }
        malla.vertices=vertices;malla.colors=colores;malla.RecalculateBounds();
    }
    void OnDestroy(){if(malla!=null)Destroy(malla);if(material!=null)Destroy(material);if(haz!=null)Destroy(haz);}
}
