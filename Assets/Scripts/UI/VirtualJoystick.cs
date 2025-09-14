using UnityEngine;
using UnityEngine.EventSystems;

public class VirtualJoystick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    [Header("Refs UI")]
    public RectTransform area;       // plein écran, capte les clics/touches (JoystickArea)
    public RectTransform background; // anneau/cadre (JoystickBG)
    public RectTransform handle;     // stick (Handle)

    [Header("Options")]
    public float maxRadius = 60f;
    public bool hideWhenReleased = true;

    public Vector2 Direction { get; private set; }

    Canvas _canvas;
    CanvasGroup _cg; // sur JoystickArea

    void Awake()
    {
        _canvas = GetComponentInParent<Canvas>();
        if (!area) area = (RectTransform)transform;

        _cg = area.GetComponent<CanvasGroup>();
        if (!_cg) _cg = area.gameObject.AddComponent<CanvasGroup>();
        _cg.blocksRaycasts = true;
        _cg.interactable   = true;

        if (hideWhenReleased && background)
            background.gameObject.SetActive(false);
    }

    public void SetRaycastsEnabled(bool enabled)
    {
        if (_cg)
        {
            _cg.blocksRaycasts = enabled;
            _cg.interactable   = enabled;
        }
    }

    public void OnPointerDown(PointerEventData e)
    {
        if (!_cg || !_cg.blocksRaycasts) return;

        var cam = (_canvas && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : (_canvas ? _canvas.worldCamera : null);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(area, e.position, cam, out var local))
        {
            background.anchoredPosition = local;
        }

        if (hideWhenReleased && !background.gameObject.activeSelf)
            background.gameObject.SetActive(true);

        handle.anchoredPosition = Vector2.zero;
        Direction = Vector2.zero;

        OnDrag(e);
    }

    public void OnDrag(PointerEventData e)
    {
        if (!_cg || !_cg.blocksRaycasts) return;

        var cam = (_canvas && _canvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : (_canvas ? _canvas.worldCamera : null);

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(background, e.position, cam, out var local))
        {
            local = Vector2.ClampMagnitude(local, maxRadius);
            handle.anchoredPosition = local;
            Direction = local / maxRadius;
        }
    }

    public void OnPointerUp(PointerEventData e)
    {
        handle.anchoredPosition = Vector2.zero;
        Direction = Vector2.zero;
        if (hideWhenReleased && background)
            background.gameObject.SetActive(false);
    }
}
