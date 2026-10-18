#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public sealed class PruebaNiveles : MonoBehaviour
{
    readonly List<string> resultados = new List<string>();
    readonly List<string> errores = new List<string>();
    Keyboard teclado;
    float inicio;
    bool terminado;
    void Start()
    {
        DontDestroyOnLoad(gameObject);
        inicio = Time.realtimeSinceStartup;
        Application.runInBackground = true;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.updateMode = InputSettings.UpdateMode.ProcessEventsInDynamicUpdate;
        Application.logMessageReceived += RegistrarError;
        teclado = InputSystem.AddDevice<Keyboard>(); teclado.MakeCurrent();
        StartCoroutine(Protegida(Prueba()));
    }
    void RegistrarError(string mensaje,string stack,LogType tipo)
    {
        if (tipo == LogType.Exception || tipo == LogType.Error || tipo == LogType.Assert) errores.Add(mensaje);
    }
    void Update() { if (!terminado && Time.realtimeSinceStartup - inicio > 300) Terminar("Tiempo de prueba excedido"); }
    IEnumerator Protegida(IEnumerator rutina)
    {
        var pila = new Stack<IEnumerator>(); pila.Push(rutina);
        while (pila.Count > 0)
        {
            object valor = null; bool avanzar = false;
            try { avanzar = pila.Peek().MoveNext(); if (avanzar) valor = pila.Peek().Current; }
            catch (Exception ex) { Terminar(ex.ToString()); yield break; }
            if (!avanzar) { pila.Pop(); continue; }
            if (valor is IEnumerator siguiente) pila.Push(siguiente);
            else yield return valor;
        }
        Terminar(errores.Count == 0 ? null : string.Join("\n",errores));
    }
    void Comprobar(bool condicion,string mensaje)
    {
        if (!condicion) throw new Exception(mensaje);
        resultados.Add("OK: " + mensaje); Debug.Log("PRUEBA OK: " + mensaje);
    }
    MovimientoPlayer Jugador => Object.FindAnyObjectByType<MovimientoPlayer>();
    EstadoPartida Estado => EstadoPartida.Instancia;
    IEnumerator Esperar() { yield return new WaitForSecondsRealtime(0.25f); }
    IEnumerator Tecla(Key key,float segundos = 0.08f)
    {
        InputSystem.QueueStateEvent(teclado,new KeyboardState(key));
        yield return new WaitForSecondsRealtime(segundos);
        Debug.Log("DIAGNOSTICO INPUT " + key + " pressed=" + teclado[key].isPressed + " current=" + (Keyboard.current == teclado) + " enabled=" + teclado.enabled + " scale=" + Time.timeScale + " delta=" + Time.deltaTime);
        InputSystem.QueueStateEvent(teclado,new KeyboardState());
        yield return null;
    }
    IEnumerator Clic(InteractuableNivel objeto)
    {
        var raton=Mouse.current;
        Vector2 posicion=Camera.main.WorldToScreenPoint(objeto.transform.position);
        InputSystem.QueueStateEvent(raton,new MouseState { position=posicion }.WithButton(MouseButton.Left));
        yield return new WaitForSecondsRealtime(.08f);
        InputSystem.QueueStateEvent(raton,new MouseState { position=posicion });
        yield return null;
    }
    void Colocar(Vector2 punto)
    {
        Jugador.transform.position = punto;
        var rb = Jugador.GetComponent<Rigidbody2D>(); rb.position = punto; rb.linearVelocity = Vector2.zero;
        Physics2D.SyncTransforms();
    }
    IEnumerator Viajar(string destino,string entrada)
    {
        Estado.Viajar(destino,entrada);
        float limite = Time.realtimeSinceStartup + 15f;
        while (SceneManager.GetActiveScene().name != destino || Estado.Cargando)
        { if (Time.realtimeSinceStartup > limite) throw new Exception("No cargó " + destino); yield return null; }
        yield return Esperar();
        Comprobar(Object.FindObjectsByType<EstadoPartida>().Length == 1,"Un único estado persistente en " + destino);
    }

    IEnumerator Prueba()
    {
        yield return Esperar();
        if (SessionState.GetBool("GAMEJAM.ValidarMenu",false))
        {
            SessionState.SetBool("GAMEJAM.ValidarMenu",false);
            Comprobar(SceneManager.GetActiveScene().name == "Menuprincipal", "La prueba arranca en el menú integrado de dev");
            Comprobar(EditorBuildSettings.scenes.First(s => s.enabled).path.EndsWith("Menuprincipal.unity"), "El menú es la primera escena habilitada de la compilación");
            var boton = Object.FindObjectsByType<UnityEngine.UI.Button>().Single(b => Enumerable.Range(0,b.onClick.GetPersistentEventCount()).Any(i => b.onClick.GetPersistentMethodName(i) == "Jugar"));
            Comprobar(boton.IsInteractable() && boton.onClick.GetPersistentTarget(0) is MenuPrincipal, "Jugar tiene un controlador válido y está habilitado");
            var eventos = UnityEngine.EventSystems.EventSystem.current;
            Comprobar(eventos != null && eventos.currentInputModule != null, "El menú tiene un módulo de entrada activo");
            Canvas.ForceUpdateCanvases();
            var canvas = boton.GetComponentInParent<Canvas>();
            var rect = (RectTransform)boton.transform;
            var punto = RectTransformUtility.WorldToScreenPoint(canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera, rect.TransformPoint(rect.rect.center));
            var clic = new UnityEngine.EventSystems.PointerEventData(eventos) { position = punto, button = UnityEngine.EventSystems.PointerEventData.InputButton.Left };
            var resultadosClic = new List<UnityEngine.EventSystems.RaycastResult>();
            eventos.RaycastAll(clic,resultadosClic);
            Comprobar(resultadosClic.Count > 0 && resultadosClic[0].gameObject.GetComponentInParent<UnityEngine.UI.Button>() == boton, "El botón Jugar recibe el clic sin elementos que lo tapen");
            Estado.Recoger(EstadoPartida.Fusible); Estado.Recoger(EstadoPartida.LlaveMaestra);
            Estado.JaulaAbierta = true; Estado.RomperBucle();
            UnityEngine.EventSystems.ExecuteEvents.ExecuteHierarchy(resultadosClic[0].gameObject,clic,UnityEngine.EventSystems.ExecuteEvents.pointerClickHandler);
            float limiteMenu = Time.realtimeSinceStartup + 15f;
            while(SceneManager.GetActiveScene().name != "EscenaInicial" || Estado.Cargando)
            { if(Time.realtimeSinceStartup > limiteMenu) throw new Exception("Jugar no abrió EscenaInicial"); yield return null; }
            yield return Esperar();
            Comprobar(!Estado.Objetos.Any() && !Estado.JaulaAbierta && !Estado.CorredorSuperado, "Jugar inicia una partida limpia en la sala de jaulas");
            Comprobar(Object.FindObjectsByType<EstadoPartida>().Length == 1, "Jugar conserva un único estado de partida");
        }
        Comprobar(Jugador != null && Jugador.groundCheck != null && Jugador.capaSuelo.value != 0,"Jugador inicial y detección de suelo configurados");
        yield return new WaitForSecondsRealtime(0.5f);
        float suelo = Jugador.transform.position.y;
        Captura("00-inicial");
        Debug.Log("DIAGNOSTICO SALTO: y=" + suelo + " ground=" + Physics2D.OverlapCircle(Jugador.groundCheck.position,Jugador.radioSuelo,Jugador.capaSuelo) + " teclado=" + Keyboard.current);
        Debug.Log("DIAGNOSTICO CUERPO " + Jugador.GetComponent<Collider2D>().bounds + " piso=" + GameObject.Find("Piso").GetComponent<Collider2D>().bounds + " gc=" + Jugador.groundCheck.position);
        yield return Tecla(Key.Space);
        Debug.Log("DIAGNOSTICO SALTO DESPUES: y=" + Jugador.transform.position.y + " velocidad=" + Jugador.GetComponent<Rigidbody2D>().linearVelocity);
        Comprobar(Jugador.transform.position.y > suelo + 0.1f,"Salto real con Input System en escena inicial");
        var botonJaula = Object.FindAnyObjectByType<BotonJaula>();
        var raton = InputSystem.AddDevice<Mouse>(); raton.MakeCurrent();
        Vector2 posicionRaton = Camera.main.WorldToScreenPoint(botonJaula.transform.position);
        InputSystem.QueueStateEvent(raton,new MouseState { position = posicionRaton }.WithButton(MouseButton.Left));
        yield return new WaitForSecondsRealtime(0.08f);
        InputSystem.QueueStateEvent(raton,new MouseState { position = posicionRaton });
        yield return null;
        Comprobar(Estado.JaulaAbierta && !Object.FindAnyObjectByType<BotonJaula>().jaulaInicial.activeSelf,"Mecanismo abre la jaula");
        yield return Viajar("PasilloPrincipal","DesdeInicial");
        var puertaC = Object.FindObjectsByType<PuertaCambioEscena>().First(p => p.puertaC);
        puertaC.Interactuar(); yield return Esperar();
        Comprobar(SceneManager.GetActiveScene().name == "PasilloPrincipal" && !Estado.PuertaCDesbloqueada,"C permanece bloqueada sin llave");
        Captura("01-pasillo");
        yield return Viajar("Nivel1","DesdePasillo");
        var soporte = Object.FindObjectsByType<MecanismoMantenimiento>().First(m => m.esPortafusible);
        soporte.Interactuar();
        Comprobar(!Estado.FusibleInstalado,"Visitar B primero no permite instalar un fusible inexistente");
        var nota = Object.FindAnyObjectByType<NotaInteractuable>();
        Colocar(new Vector2(nota.transform.position.x,0)); yield return new WaitForSecondsRealtime(0.6f);
        yield return Tecla(Key.E);
        Comprobar(Time.timeScale == 0f,"E abre la nota y pausa la partida");
        Comprobar(nota.imagenNota != null && nota.imagenNota.activeInHierarchy,"La nota muestra la ilustración de mantenimiento");
        Captura("nota-original");
        yield return Tecla(Key.E);
        Comprobar(Time.timeScale == 1f,"E cierra la nota sin reabrirla en el mismo frame");
        yield return Viajar("PasilloPrincipal","DesdeB");
        yield return Viajar("PuertaA","DesdePasillo");
        Captura("02-derrumbe");
        var fusible = Object.FindAnyObjectByType<ObjetoRecogible>();
        Colocar(new Vector2(fusible.transform.position.x,0)); yield return Esperar();
        yield return Tecla(Key.E);
        Comprobar(Estado.Tiene(EstadoPartida.Fusible),"E recoge el fusible físico en A");
        Comprobar(InterfazPartida.TextoActual.Contains("Recogiste un fusible"),"La recogida confirma el fusible en pantalla");
        yield return Viajar("PasilloPrincipal","DesdeA");
        Comprobar(Estado.Tiene(EstadoPartida.Fusible),"Fusible conservado al volver al pasillo");
        Comprobar(InterfazPartida.TextoActual.Contains("Recogiste un fusible"),"La confirmación sobrevive al cambio inmediato de sala");
        yield return Viajar("Nivel1","DesdePasillo");
        var mecanismos = Object.FindObjectsByType<MecanismoMantenimiento>();
        soporte = mecanismos.First(m => m.esPortafusible); yield return Clic(soporte);
        Comprobar(Estado.FusibleInstalado && !Estado.Tiene(EstadoPartida.Fusible),"Instalación consume solo el fusible");
        yield return Esperar();
        Comprobar(soporte.fusibleVisible != null && soporte.fusibleVisible.activeInHierarchy,"El soporte muestra el fusible instalado");
        Comprobar(!mecanismos.Any(m=>!m.esPortafusible && m.paso==1),"El manómetro ya no exige una interacción adicional");
        Comprobar(InterfazPartida.TextoActual.Contains("Instalaste el fusible"),"La instalación confirma el fusible colocado");
        Captura("soporte-instalado");
        yield return Clic(mecanismos.First(m => !m.esPortafusible && m.paso == 2));
        Comprobar(Estado.PasoMantenimiento == 0,"El botón no resuelve el puzzle antes de restablecer el flujo");
        yield return Clic(mecanismos.First(m => !m.esPortafusible && m.paso == 0));
        yield return Viajar("PasilloPrincipal","DesdeB");
        yield return Viajar("Nivel1","DesdePasillo");
        Comprobar(Estado.PasoMantenimiento == 1,"Secuencia parcial persiste entre escenas");
        var manija=Object.FindObjectsByType<MecanismoMantenimiento>().First(m=>!m.esPortafusible && m.paso==0);
        yield return Clic(manija);
        Comprobar(Estado.PasoMantenimiento==1,"Volver a girar la manija no borra el flujo restablecido");
        mecanismos = Object.FindObjectsByType<MecanismoMantenimiento>();
        yield return Clic(mecanismos.First(m => !m.esPortafusible && m.paso == 2));
        Comprobar(Estado.MantenimientoResuelto,"Fusible, manija y botón resuelven mantenimiento sin paso de presión");
        Comprobar(!Estado.Tiene(EstadoPartida.LlaveMaestra),"El botón desbloquea la puerta pero no entrega automáticamente la llave");
        Comprobar(InterfazPartida.TextoActual.Contains("puerta de la derecha"),"La activación confirma qué puerta se desbloqueó");
        yield return Esperar(); Captura("llave-desbloqueada");
        var recompensa=Object.FindAnyObjectByType<ObjetoRecogible>();
        Comprobar(recompensa.enPuerta && recompensa.transform.position.x > 5f,"La llave se recoge en la puerta derecha");
        Colocar(new Vector2(recompensa.transform.position.x,-1.3f));yield return Esperar();yield return Tecla(Key.E);
        Comprobar(Estado.Tiene(EstadoPartida.LlaveMaestra),"E en la puerta entrega la llave maestra");
        Captura("03-mantenimiento");
        yield return Viajar("PuertaA","DesdePasillo");
        Comprobar(Object.FindAnyObjectByType<ObjetoRecogible>() == null,"El fusible instalado no reaparece");
        yield return Viajar("PasilloPrincipal","DesdeB");
        Colocar(new Vector2(7,-1.5f)); yield return Esperar();
        yield return Tecla(Key.E);
        yield return new WaitForSecondsRealtime(0.5f);
        Comprobar(SceneManager.GetActiveScene().name == "PuertaC" && Estado.PuertaCDesbloqueada,"E abre C con la llave");
        Comprobar(Estado.Tiene(EstadoPartida.LlaveMaestra),"La llave no se consume al abrir C");
        var bucle = Object.FindAnyObjectByType<CorredorInfinito>();
        Colocar(new Vector2(-7,-1.3f)); yield return Esperar();
        Comprobar(!Estado.CorredorSuperado,"No se resuelve el bucle al entrar");
        Captura("04-corredor");
        for (int i=0;i<3;i++) { Colocar(new Vector2(bucle.finalX+0.1f,-1.3f)); yield return Esperar(); }
        Comprobar(Estado.VueltasCorredor == 3,"Tres vueltas completas vuelven a 50 metros");
        Comprobar(InterfazPartida.TextoActual.Contains("hacia la izquierda"),"La tercera vuelta da una pista clara de retroceso");
        Captura("pista-corredor");
        yield return Esperar();
        Comprobar(!Estado.CorredorSuperado,"El bucle no se resuelve quedándose quieto tras la tercera vuelta");
        yield return Tecla(Key.A,.4f);
        Comprobar(Estado.CorredorSuperado && bucle.puertaRegreso.activeSelf,"Retroceder físicamente rompe el bucle y revela la puerta");
        Colocar(new Vector2(-7.6f,-1.3f)); yield return Esperar(); yield return Tecla(Key.E);
        yield return new WaitForSecondsRealtime(0.5f);
        Comprobar(SceneManager.GetActiveScene().name == "PasilloPrincipal","Puerta revelada permite volver al pasillo");
        Comprobar(InterfazPartida.TextoActual.Contains("pulsa E para continuar"),"Al regresar se explica cómo continuar por C");
        Captura("regreso-corredor");
        Comprobar(Object.FindAnyObjectByType<RotuloPuertaC>().GetComponent<TextMesh>().text == "¿Otra vez?","Rótulo de C actualizado tras romper el bucle");
        Comprobar(Estado.Tiene(EstadoPartida.LlaveMaestra) && Estado.CorredorSuperado,"Llave y solución conservadas al terminar");
        if(SessionState.GetBool("GAMEJAM.SoloMensajes",false))
        {
            SessionState.SetBool("GAMEJAM.SoloMensajes",false);
            Estado.NuevaPartida();yield return new WaitForSecondsRealtime(.5f);
            Comprobar(string.IsNullOrEmpty(InterfazPartida.TextoActual),"Nueva partida limpia mensajes de la partida anterior");
            yield break;
        }
        // Continuación completa, usando los estados obtenidos en el recorrido anterior.
        Colocar(new Vector2(7,-1.5f));yield return Esperar();yield return Tecla(Key.E);
        yield return new WaitForSecondsRealtime(.7f);
        Comprobar(SceneManager.GetActiveScene().name=="Persecucion","La puerta C conecta con persecución tras romper el bucle");
        Captura("05-persecucion");
        var recuperacion=Jugador.GetComponent<RecuperarJugador>();
        Colocar(new Vector2(11,-1.05f));yield return Esperar();
        Comprobar(recuperacion.PuntoSeguro.x>10,"La persecución guarda un punto de reaparición");
        var enemigo=Object.FindObjectsByType<EnemigoTemporal>().First(e=>!e.patrulla);
        enemigo.transform.position=Jugador.transform.position;Physics2D.SyncTransforms();yield return Esperar();
        Comprobar(recuperacion.Reintentos>0 && Estado.Tiene(EstadoPartida.LlaveMaestra),"El enemigo provoca reaparición sin perder la llave");
        yield return RecorrerPersecucion();
        Comprobar(SceneManager.GetActiveScene().name=="EscapeVertical" && Estado.PersecucionSuperada,"La persecución termina en el conducto vertical");
        Captura("06-parkour-inicio");
        // Comprobar el recorrido físico por separado del temporizador de persecución.
        foreach(var e in Object.FindObjectsByType<EnemigoTemporal>())e.gameObject.SetActive(false);
        var plataformas=Object.FindObjectsByType<BoxCollider2D>().Where(c=>c.name.StartsWith("Plataforma_")).OrderBy(c=>c.name).ToArray();
        Comprobar(plataformas.Length>=20,"Parkour largo con al menos veinte plataformas");
        for(int i=1;i<plataformas.Length;i++)
        {
            float pies=Jugador.GetComponent<Collider2D>().bounds.min.y;
            if(pies>=plataformas[i].bounds.max.y-.08f)continue;
            var origen=plataformas.Where(c=>Mathf.Abs(c.bounds.max.y-pies)<.15f && Mathf.Abs(c.bounds.center.x-Jugador.transform.position.x)<c.bounds.extents.x+.2f).OrderByDescending(c=>c.bounds.max.y).First();
            yield return SaltarHasta(origen,plataformas[i]);
        }
        Comprobar(Jugador.transform.position.y>27,"Todos los saltos del ascenso son alcanzables con el controlador real");
        Comprobar(Camera.main.transform.position.y>20,"La cámara sigue al jugador hacia arriba");
        Captura("07-parkour-salida");
        var escalador=Object.FindObjectsByType<EnemigoTemporal>(FindObjectsInactive.Include).First(e=>e.escalador);
        var recuperacionVertical=Jugador.GetComponent<RecuperarJugador>();int antes=recuperacionVertical.Reintentos;
        escalador.gameObject.SetActive(true);escalador.transform.position=Jugador.transform.position;Physics2D.SyncTransforms();yield return Esperar();
        Comprobar(recuperacionVertical.Reintentos>antes && Jugador.transform.position.y>20,"El escalador devuelve al último descanso del ascenso");
        Colocar(new Vector2(-.4f,28));yield return Esperar();yield return Tecla(Key.E);yield return new WaitForSecondsRealtime(.7f);
        Comprobar(SceneManager.GetActiveScene().name=="Valle" && Estado.ParkourSuperado && Estado.Tiene(EstadoPartida.LlaveMaestra),"La misma llave abre la salida final sin consumirse");
        Captura("08-valle");
        Colocar(new Vector2(10.7f,-1.8f));yield return Esperar();
        var trampa=Object.FindAnyObjectByType<TrampaZanahoria>();
        Comprobar(trampa.Activada && !trampa.sueloTrampa.enabled,"Acercarse a la zanahoria hace ceder el suelo");
        yield return new WaitForSecondsRealtime(2.5f);
        Comprobar(SceneManager.GetActiveScene().name=="HabitacionFinal","La caída devuelve al conejo al laboratorio");
        yield return new WaitForSecondsRealtime(3f);
        Comprobar(Estado.ExperimentoCompletado,"La última habitación revela el final del experimento");
        Captura("09-final");
        yield return Tecla(Key.R);yield return new WaitForSecondsRealtime(.7f);
        Comprobar(!Estado.Tiene(EstadoPartida.LlaveMaestra) && !Estado.JaulaAbierta && !Estado.CorredorSuperado && !Estado.PersecucionSuperada && !Estado.ParkourSuperado && !Estado.ExperimentoCompletado,"Nueva partida reinicia la progresión");
    }

    IEnumerator RecorrerPersecucion()
    {
        float limite=Time.realtimeSinceStartup+35;
        while(SceneManager.GetActiveScene().name=="Persecucion" && Time.realtimeSinceStartup<limite)
        {
            float x=Jugador.transform.position.x;
            bool cerca=Object.FindObjectsByType<BoxCollider2D>().Any(c=>c.name.StartsWith("Obstaculo_") && c.bounds.min.x-x<1.6f && c.bounds.max.x>x+.1f);
            cerca|=Object.FindObjectsByType<EnemigoTemporal>().Any(e=>e.transform.position.x-x<2.2f && e.transform.position.x>x-.2f);
            bool suelo=Physics2D.OverlapCircle(Jugador.groundCheck.position,Jugador.radioSuelo,Jugador.capaSuelo)!=null;
            InputSystem.QueueStateEvent(teclado,cerca && suelo?new KeyboardState(Key.D,Key.Space):new KeyboardState(Key.D));yield return null;
        }
        InputSystem.QueueStateEvent(teclado,new KeyboardState());yield return new WaitForSecondsRealtime(.4f);
        Comprobar(SceneManager.GetActiveScene().name=="EscapeVertical","Recorrido físico de persecución con saltos y enemigos activos");
    }
    IEnumerator SaltarHasta(BoxCollider2D origen,BoxCollider2D destino)
    {
        float inicioSalto=Mathf.Clamp(destino.bounds.center.x,origen.bounds.min.x+.38f,origen.bounds.max.x-.38f);
        float limite=Time.realtimeSinceStartup+4f;
        while(Mathf.Abs(Jugador.transform.position.x-inicioSalto)>.12f && Time.realtimeSinceStartup<limite)
        {
            InputSystem.QueueStateEvent(teclado,new KeyboardState(Jugador.transform.position.x<inicioSalto?Key.D:Key.A));yield return null;
        }
        InputSystem.QueueStateEvent(teclado,new KeyboardState());yield return new WaitForSecondsRealtime(.12f);
        InputSystem.QueueStateEvent(teclado,new KeyboardState(Key.Space));yield return new WaitForSecondsRealtime(.04f);
        limite=Time.realtimeSinceStartup+3f;bool aterrizo=false;
        while(Time.realtimeSinceStartup<limite)
        {
            float dx=destino.bounds.center.x-Jugador.transform.position.x;
            InputSystem.QueueStateEvent(teclado,Mathf.Abs(dx)>.12f?new KeyboardState(dx>0?Key.D:Key.A):new KeyboardState());
            yield return null;
            float pies=Jugador.GetComponent<Collider2D>().bounds.min.y;
            if(pies>=destino.bounds.max.y-.08f && Jugador.GetComponent<Rigidbody2D>().linearVelocity.y<=.1f && Physics2D.OverlapCircle(Jugador.groundCheck.position,Jugador.radioSuelo,Jugador.capaSuelo)!=null) {aterrizo=true;break;}
        }
        InputSystem.QueueStateEvent(teclado,new KeyboardState());
        if(!aterrizo)Captura("fallo-salto");
        Comprobar(aterrizo,"Salto físico hasta "+destino.name+" o plataforma superior; posición="+Jugador.transform.position);
    }
    void Captura(string nombre)
    {
        if (SystemInfo.graphicsDeviceType == UnityEngine.Rendering.GraphicsDeviceType.Null) return;
        var cam = Camera.main; var rt = new RenderTexture(1600,900,24);
        var canvases = Object.FindObjectsByType<Canvas>().Where(c => c.renderMode == RenderMode.ScreenSpaceOverlay).ToArray();
        float aspecto=cam.aspect, altura=cam.orthographicSize;
        cam.aspect=1600f/900f;
        var ajuste=cam.GetComponent<CamaraNivel>();cam.orthographicSize=Mathf.Max(ajuste.altura,ajuste.semiancho/cam.aspect);
        foreach(var c in canvases) { c.renderMode=RenderMode.ScreenSpaceCamera;c.worldCamera=cam;c.planeDistance=1f; }
        Canvas.ForceUpdateCanvases();
        var anterior = RenderTexture.active; cam.targetTexture = rt; cam.Render(); RenderTexture.active = rt;
        var imagen = new Texture2D(1600,900,TextureFormat.RGB24,false);
        imagen.ReadPixels(new Rect(0,0,1600,900),0,0); imagen.Apply();
        Directory.CreateDirectory("ValidationResults/Capturas"); File.WriteAllBytes("ValidationResults/Capturas/"+nombre+".png",imagen.EncodeToPNG());
        foreach(var c in canvases) c.renderMode=RenderMode.ScreenSpaceOverlay;
        cam.aspect=aspecto;cam.orthographicSize=altura;cam.targetTexture = null; RenderTexture.active = anterior; rt.Release(); Destroy(rt); Destroy(imagen);
    }
    void Terminar(string error)
    {
        if (terminado) return; terminado = true;
        Application.logMessageReceived -= RegistrarError;
        if (error != null) resultados.Add("FALLO: " + error);
        Directory.CreateDirectory("ValidationResults"); File.WriteAllLines("ValidationResults/resultado-niveles.txt",resultados);
        Debug.Log(error == null ? "VALIDACION_NIVELES_OK" : "VALIDACION_NIVELES_FALLO: " + error);
        EditorApplication.Exit(error == null ? 0 : 1);
    }
}

#endif












