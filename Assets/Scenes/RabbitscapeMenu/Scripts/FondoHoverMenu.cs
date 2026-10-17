using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class FondoHoverMenu : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    public Image fondoHover;

    public void OnPointerEnter(PointerEventData eventData)
    {
        CambiarFondo(0.14f);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CambiarFondo(0f);
    }

    void CambiarFondo(float alpha)
    {
        if (fondoHover == null) return;

        Color color = fondoHover.color;
        color.a = alpha;
        fondoHover.color = color;
    }
}