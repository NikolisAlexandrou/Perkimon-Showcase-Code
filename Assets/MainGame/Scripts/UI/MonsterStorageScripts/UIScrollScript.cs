using UnityEngine;
using UnityEngine.EventSystems;

public class UIScrollScript : MonoBehaviour, IScrollHandler
{
    public RectTransform content;  // Content panel that moves
    public float scrollSpeed = 20f; // Adjust scroll speed
    public float minY;
    public float maxY;
    public float maxYMultiplier;


    public void OnScroll(PointerEventData eventData)
    {
        float scrollAmount = eventData.scrollDelta.y * scrollSpeed;
        Vector2 newPos = content.anchoredPosition + new Vector2(0, scrollAmount);
        content.anchoredPosition = Vector2.Lerp(content.anchoredPosition, ClampScroll(newPos), Time.deltaTime * 10f);
    }

    private Vector2 ClampScroll(Vector2 pos)
    {
        //minY = -content.rect.height + GetComponent<RectTransform>().rect.height; // Bottom limit
        //maxY = 0; // Top limit
        maxY = MonsterStorageManager.i.GetMonsterStorageList().Count * maxYMultiplier;

        pos.y = Mathf.Clamp(pos.y, minY, maxY);
        return pos;
    }
}