using UnityEngine;

public sealed class ObjetoRecogible : InteractuableNivel
{
    public string objetoId;
    public string nombreVisible;
    public bool requiereMantenimiento;
    public bool enPuerta;
    void Update()
    {
        if (requiereMantenimiento && TryGetComponent<SpriteRenderer>(out var imagen))
            imagen.enabled = EstadoPartida.Instancia.MantenimientoResuelto;
    }

    void Start()
    {
        if (EstadoPartida.Instancia.Tiene(objetoId) ||
            (objetoId == EstadoPartida.Fusible && EstadoPartida.Instancia.FusibleInstalado)) gameObject.SetActive(false);
    }
    public override void Interactuar()
    {
        var estado = EstadoPartida.Instancia;
        if (requiereMantenimiento && !estado.MantenimientoResuelto)
        { InterfazPartida.Mensaje(enPuerta ? "La puerta está bloqueada. Restablece el sistema de mantenimiento." : "El cajón no cede."); return; }
        if (estado.Recoger(objetoId)) InterfazPartida.Mensaje(enPuerta ? "Recogiste la llave maestra del cierre de la puerta. La guardaste en tu inventario." : objetoId == EstadoPartida.Fusible ? "Recogiste un fusible. Lo guardaste en tu inventario. [Tab] Ver objetos" : "Recogiste: " + nombreVisible + ".");
        gameObject.SetActive(false);
    }
}


