using UnityEngine;
using UnityEngine.EventSystems;

public class HoldButtonBridge : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public SeasonTransition seasonTransition;

    public void OnPointerDown(PointerEventData eventData)
    {
        if (seasonTransition != null)
        {
            seasonTransition.OnPointerDown(eventData);
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (seasonTransition != null)
        {
            seasonTransition.OnPointerUp(eventData);
        }
    }
}