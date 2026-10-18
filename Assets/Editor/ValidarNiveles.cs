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

// Prueba de integración en Play Mode. Ejecutar en una copia del proyecto, nunca sobre una partida del equipo.
[InitializeOnLoad]
public static class ValidarNiveles
{
    const string Clave = "GAMEJAM.Validacion";
    static ValidarNiveles() { EditorApplication.playModeStateChanged += CambiarModo; }
    public static void EjecutarMensajes()
    { SessionState.SetBool("GAMEJAM.SoloMensajes",true); Ejecutar(); }
    public static void EjecutarMenu()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Validar en una copia con -batchmode.");
        SessionState.SetBool("GAMEJAM.SoloMensajes",true);
        SessionState.SetBool("GAMEJAM.ValidarMenu",true);
        EditorSceneManager.OpenScene("Assets/Scenes/RabbitscapeMenu/Scene/Menuprincipal.unity");
        SessionState.SetBool(Clave,true);
        EditorApplication.EnterPlaymode();
    }
    public static void Ejecutar()
    {
        if (!Application.isBatchMode) throw new InvalidOperationException("Ejecutar esta validación en una copia con -batchmode.");
        PrepararNiveles.Construir();
        EditorSceneManager.OpenScene("Assets/Scenes/EscenaInicial/EscenaInicial.unity");
        SessionState.SetBool(Clave,true);
        EditorApplication.EnterPlaymode();
    }
    static void CambiarModo(PlayModeStateChange modo)
    {
        if (modo != PlayModeStateChange.EnteredPlayMode || !SessionState.GetBool(Clave,false)) return;
        SessionState.SetBool(Clave,false);
        new GameObject("PruebaIntegracion").AddComponent<PruebaNiveles>();
    }
}


