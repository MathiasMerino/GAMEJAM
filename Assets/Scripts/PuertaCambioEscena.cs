using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PuertaCambioEscena : InteractuableNivel
{
    public string escenaDestino = "Nivel1";

    public string entradaDestino;
    public bool requiereLlaveMaestra;
    public bool puertaC;
    public override string Indicacion => puertaC && EstadoPartida.Instancia.CorredorSuperado ? "¿Otra vez? · examinar puerta C" : indicacion;

    const float RadioBrilloCartel = 3.2f;
    static Sprite spritePlano;
    MovimientoPlayer jugadorCercano;
    Transform cartel;
    SpriteRenderer marcoCartel;
    SpriteRenderer biselCartel;
    SpriteRenderer panelCartel;
    SpriteRenderer haloCartel;
    SpriteRenderer[] remachesCartel;
    TextMesh textoCartel;

    void Start()
    {
        CrearCartelReflectivo();
    }

    void LateUpdate()
    {
        if (cartel == null) return;
        if (jugadorCercano == null) jugadorCercano = Object.FindAnyObjectByType<MovimientoPlayer>();

        float cercania = 0f;
        if (jugadorCercano != null)
        {
            float distancia = Vector2.Distance(jugadorCercano.transform.position, transform.position);
            cercania = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(1f - distancia / RadioBrilloCartel));
        }

        if (marcoCartel != null) marcoCartel.color = Color.Lerp(new Color(.13f,.14f,.13f,1f),new Color(.28f,.30f,.25f,1f),cercania);
        if (biselCartel != null) biselCartel.color = Color.Lerp(new Color(.25f,.21f,.15f,1f),new Color(.47f,.39f,.23f,1f),cercania);
        if (panelCartel != null) panelCartel.color = Color.Lerp(new Color(.025f,.045f,.038f,1f),new Color(.075f,.15f,.105f,1f),cercania);
        if (haloCartel != null) haloCartel.color = new Color(.40f,.57f,.38f,.025f+.105f*cercania);
        if (textoCartel != null) textoCartel.color = Color.Lerp(new Color(.42f,.48f,.39f,1f),new Color(.83f,.88f,.70f,1f),cercania);
        if (remachesCartel != null)
        {
            Color remache = Color.Lerp(new Color(.22f,.20f,.16f,1f),new Color(.48f,.40f,.27f,1f),cercania);
            foreach (SpriteRenderer remacheRenderer in remachesCartel)
                if (remacheRenderer != null) remacheRenderer.color = remache;
        }
    }

    void CrearCartelReflectivo()
    {
        if (cartel != null) return;

        Collider2D colision = GetComponent<Collider2D>();
        float parteSuperior = colision != null ? colision.bounds.max.y : transform.position.y + 1.1f;
        GameObject objetoCartel = new GameObject("Cartel reflectivo SALIDA");
        cartel = objetoCartel.transform;
        cartel.SetParent(transform, false);
        cartel.position = new Vector3(transform.position.x, parteSuperior + .76f, transform.position.z - .08f);
        Vector3 escalaPadre = transform.lossyScale;
        cartel.localScale = new Vector3(
            1f / Mathf.Max(.0001f, Mathf.Abs(escalaPadre.x)),
            1f / Mathf.Max(.0001f, Mathf.Abs(escalaPadre.y)),
            1f / Mathf.Max(.0001f, Mathf.Abs(escalaPadre.z)));

        haloCartel = CrearPlaca(cartel, "Resplandor tenue", new Vector3(0,0,.03f), new Vector3(.78f,.24f,1f), new Color(.40f,.57f,.38f,.025f), 38);
        marcoCartel = CrearPlaca(cartel, "Caja de metal envejecido", new Vector3(0,0,.02f), new Vector3(.75f,.22f,1f), new Color(.13f,.14f,.13f,1f), 39);
        biselCartel = CrearPlaca(cartel, "Bisel oxidado", new Vector3(0,0,.01f), new Vector3(.71f,.185f,1f), new Color(.25f,.21f,.15f,1f), 40);
        panelCartel = CrearPlaca(cartel, "Esmalte verde gastado", Vector3.zero, new Vector3(.675f,.15f,1f), new Color(.025f,.045f,.038f,1f), 41);
        CrearPlaca(cartel, "Filete superior", new Vector3(0,.118f,-.005f), new Vector3(.62f,.009f,1f), new Color(.27f,.30f,.24f,1f), 42);
        CrearPlaca(cartel, "Filete inferior", new Vector3(0,-.118f,-.005f), new Vector3(.62f,.009f,1f), new Color(.12f,.15f,.12f,1f), 42);

        remachesCartel = new SpriteRenderer[4];
        remachesCartel[0] = CrearPlaca(cartel, "Remache superior izquierdo", new Vector3(-.675f,.145f,-.01f), new Vector3(.012f,.012f,1f), new Color(.22f,.20f,.16f,1f), 43);
        remachesCartel[1] = CrearPlaca(cartel, "Remache superior derecho", new Vector3(.675f,.145f,-.01f), new Vector3(.012f,.012f,1f), new Color(.22f,.20f,.16f,1f), 43);
        remachesCartel[2] = CrearPlaca(cartel, "Remache inferior izquierdo", new Vector3(-.675f,-.145f,-.01f), new Vector3(.012f,.012f,1f), new Color(.22f,.20f,.16f,1f), 43);
        remachesCartel[3] = CrearPlaca(cartel, "Remache inferior derecho", new Vector3(.675f,-.145f,-.01f), new Vector3(.012f,.012f,1f), new Color(.22f,.20f,.16f,1f), 43);

        GameObject objetoTexto = new GameObject("Texto SALIDA");
        Transform textoTransform = objetoTexto.transform;
        textoTransform.SetParent(cartel, false);
        textoTransform.localPosition = new Vector3(0,0,-.02f);
        textoTransform.localScale = Vector3.one;
        textoCartel = objetoTexto.AddComponent<TextMesh>();
        textoCartel.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        textoCartel.text = "SALIDA";
        textoCartel.fontSize = 48;
        textoCartel.characterSize = .052f;
        textoCartel.fontStyle = FontStyle.Bold;
        textoCartel.anchor = TextAnchor.MiddleCenter;
        textoCartel.alignment = TextAlignment.Center;
        textoCartel.color = new Color(.42f,.48f,.39f,1f);
        MeshRenderer renderTexto = objetoTexto.GetComponent<MeshRenderer>();
        renderTexto.sortingOrder = 44;
    }

    static SpriteRenderer CrearPlaca(Transform padre, string nombre, Vector3 posicion, Vector3 escala, Color color, int orden)
    {
        if (spritePlano == null)
        {
            Texture2D textura = Texture2D.whiteTexture;
            spritePlano = Sprite.Create(textura, new Rect(0,0,textura.width,textura.height), new Vector2(.5f,.5f), 1f);
        }

        GameObject placa = new GameObject(nombre);
        Transform t = placa.transform;
        t.SetParent(padre, false);
        t.localPosition = posicion;
        SpriteRenderer render = placa.AddComponent<SpriteRenderer>();
        render.sprite = spritePlano;
        Vector3 tamanioSprite = spritePlano.bounds.size;
        t.localScale = new Vector3(
            escala.x * 2f / Mathf.Max(.0001f,tamanioSprite.x),
            escala.y * 2f / Mathf.Max(.0001f,tamanioSprite.y),
            escala.z);
        render.color = color;
        render.sortingOrder = orden;
        return render;
    }

    public override void Interactuar()
    {
        var estado = EstadoPartida.Instancia;
        if (requiereLlaveMaestra && !estado.Tiene(EstadoPartida.LlaveMaestra))
        {
            InterfazPartida.Mensaje("La puerta está cerrada. Necesitas la llave maestra.");
            return;
        }
        if (puertaC && estado.CorredorSuperado)
        {
            estado.Viajar("Persecucion", "Inicio", "Algo se mueve detrás de ti. Corre.");
            return;
        }
        bool aperturaNueva = puertaC && !estado.PuertaCDesbloqueada;
        if (puertaC) estado.DesbloquearC();
        string confirmacion=aperturaNueva ? "Puerta C desbloqueada. Usaste la llave maestra y la conservas en tu inventario." : null;
        if (escenaDestino == "PasilloPrincipal" && entradaDestino == "DesdeC" && estado.CorredorSuperado)
            confirmacion="Rompiste el bucle. La puerta C ha cambiado: «¿Otra vez?».\nVuelve a esa puerta y pulsa E para continuar.";
        estado.Viajar(escenaDestino, entradaDestino, confirmacion);
    }
}



