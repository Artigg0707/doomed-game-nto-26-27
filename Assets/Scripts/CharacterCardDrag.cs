using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class CharacterCardDrag : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField, Min(0f)]
    private float edgePadding = 0f;

    private RectTransform cardRect;
    private RectTransform dragArea;
    private Vector2 pointerOffset;
    private bool isDragging;

    private void Awake()
    {
        cardRect = GetComponent<RectTransform>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Карточка должна быть непосредственным ребёнком CardContainer.
        dragArea = cardRect.parent as RectTransform;

        if (dragArea == null)
        {
            Debug.LogError("Родитель карточки должен быть UI-объектом с RectTransform.", this);
            return;
        }

        if (!TryGetPointerPosition(eventData, out Vector2 pointerPosition))
            return;

        Vector3 cardPosition = cardRect.localPosition;
        pointerOffset = new Vector2(cardPosition.x, cardPosition.y) - pointerPosition;

        isDragging = true;

        // Во время перетаскивания карточка будет поверх остальных карточек.
        cardRect.SetAsLastSibling();

        MoveCard(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
            MoveCard(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDragging)
            return;

        MoveCard(eventData);
        isDragging = false;
    }

    private void MoveCard(PointerEventData eventData)
    {
        if (!TryGetPointerPosition(eventData, out Vector2 pointerPosition))
            return;

        Vector2 targetPosition = pointerPosition + pointerOffset;
        targetPosition = ClampToBoard(targetPosition);

        // Z = 0 оставляет карточку в плоскости CardContainer.
        cardRect.localPosition = new Vector3(targetPosition.x, targetPosition.y, 0f);
    }

    private bool TryGetPointerPosition(
        PointerEventData eventData,
        out Vector2 localPosition)
    {
        Camera eventCamera = eventData.pressEventCamera;

        // Запасной вариант для World Space Canvas.
        if (eventCamera == null)
        {
            Canvas canvas = GetComponentInParent<Canvas>();

            if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                eventCamera = canvas.worldCamera != null
                    ? canvas.worldCamera
                    : Camera.main;
            }
        }

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            dragArea,
            eventData.position,
            eventCamera,
            out localPosition
        );
    }

    private Vector2 ClampToBoard(Vector2 position)
    {
        Rect boardRect = dragArea.rect;

        float halfWidth = cardRect.rect.width * Mathf.Abs(cardRect.localScale.x) * 0.5f;
        float halfHeight = cardRect.rect.height * Mathf.Abs(cardRect.localScale.y) * 0.5f;

        float minX = boardRect.xMin + halfWidth + edgePadding;
        float maxX = boardRect.xMax - halfWidth - edgePadding;
        float minY = boardRect.yMin + halfHeight + edgePadding;
        float maxY = boardRect.yMax - halfHeight - edgePadding;

        // Если карточка больше области, ставим её по центру по этой оси.
        float x = minX > maxX
            ? boardRect.center.x
            : Mathf.Clamp(position.x, minX, maxX);

        float y = minY > maxY
            ? boardRect.center.y
            : Mathf.Clamp(position.y, minY, maxY);

        return new Vector2(x, y);
    }
}