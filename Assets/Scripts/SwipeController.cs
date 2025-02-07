using UnityEngine;
using UnityEngine.EventSystems;

public class SwipeController : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public RectTransform content;     // Contenitore con le card
    private Vector2 startPosition;
    private float swipeThreshold = 0.2f;   // Percentuale necessaria per effettuare uno swipe

    private void Start()
    {
        startPosition = content.anchoredPosition;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Trascina il contenuto orizzontalmente
        content.anchoredPosition += new Vector2(eventData.delta.x, 0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Controlla se è stato effettuato uno swipe sufficiente
        float swipeDistance = Mathf.Abs(eventData.pressPosition.x - eventData.position.x);
        if (swipeDistance / Screen.width > swipeThreshold)
        {
            // Muove il contenuto alla posizione successiva o precedente in base alla direzione dello swipe
            float direction = Mathf.Sign(eventData.pressPosition.x - eventData.position.x);
            Vector2 newPosition = startPosition + new Vector2(direction * -Screen.width, 0);
            content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, newPosition, 0.5f);
        }
        else
        {
            // Ritorna alla posizione iniziale se lo swipe non è sufficiente
            content.anchoredPosition = startPosition;
        }
    }
}
