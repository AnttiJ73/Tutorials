using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ViewElementData
{
    public string HeaderText;
    public Sprite MainIcon;
    public string AmountText;
    public string AvailabilityText;
    public string ValueText;
    public bool Selected;
    public bool Available = true;
    public object Payload;
}

[System.Serializable]
public class ItemUIEventData
{
    public enum EventType
    {
        HoverEnter,
        HoverExit,
        Click,
        DragStart,
        DragEnd,
        Drop,
        Disable,
        Custom
    }

    public GameObject InventoryUI;
    public ItemUI Source { get; private set; }
    public EventType Type { get; private set; }
    public PointerEventData.InputButton Button { get; private set; }
    public Vector2 Position { get; private set; }
    private readonly object _payload;

    public T GetPayload<T>()
    {
        if (_payload != null && _payload is T t)
        {
            return t;
        }
        return default;
    }

    public bool TryGetPayload<T>(out T result)
    {
        if (_payload != null && _payload is T t)
        {
            result = t;
            return true;
        }
        result = default;
        return false;
    }

    public ItemUIEventData(
        EventType type,
        ViewElement source,
        object payload,
        PointerEventData.InputButton button = PointerEventData.InputButton.Left,
        Vector2 position = default
        )
    {
        Type = type;
        Source = source;
        Button = button;
        Position = position;
        _payload = payload;
    }
}

public class ViewElement : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler,
    IPointerClickHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    public System.Action<ItemUIEventData> OnEvent;

    [SerializeField] private TMP_Text _headerText;
    [SerializeField] private Image _mainIcon;
    [SerializeField] private TMP_Text _amountText;
    [SerializeField] private TMP_Text _availabilityText;
    [SerializeField] private TMP_Text _valueText;

    [SerializeField] private bool _disableImageWhenUnavailable = false;

    private object _payload;
    private bool _deregistered = false;
    private bool _selected = false;

    public void Display(ViewElementData item)
    {
        SetText(_headerText, item.HeaderText);
        SetText(_amountText, item.AmountText);

        SetText(_valueText, item.ValueText);
        SetText(_availabilityText, item.AvailabilityText);

        SetImage(_mainIcon, item.MainIcon);

        _selected = item.Selected;
        _payload = item.Payload;
    }

    private void SetText(TMP_Text text, string value)
    {
        if (text)
        {
            text.text = value;
        }
    }

    private void SetImage(Image image, Sprite sprite)
    {
        if (image && sprite)
        {
            image.sprite = sprite;
        }
        if (image && _disableImageWhenUnavailable)
        {
            image.enabled = (sprite != null);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OnEvent?.Invoke(new ItemUIEventData(ItemUIEventData.EventType.HoverEnter, this, _payload, eventData.button, eventData.position));
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        OnEvent?.Invoke(new ItemUIEventData(ItemUIEventData.EventType.HoverExit, this, _payload, eventData.button, eventData.position));
    }
    public void OnPointerClick(PointerEventData eventData)
    {
        OnEvent?.Invoke(new ItemUIEventData(ItemUIEventData.EventType.Click, this, _payload, eventData.button, eventData.position));
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        OnEvent?.Invoke(new ItemUIEventData(ItemUIEventData.EventType.DragStart, this, _payload, eventData.button, eventData.position));
    }
    public void OnEndDrag(PointerEventData eventData)
    {
        OnEvent?.Invoke(new ItemUIEventData(ItemUIEventData.EventType.DragEnd, this, _payload, eventData.button, eventData.position));
    }
    public void OnDrag(PointerEventData eventData) { }

    void OnDisable()
    {
        if (!_deregistered)
        {
            _deregistered = true;
            OnEvent?.Invoke(new ItemUIEventData(ItemUIEventData.EventType.Disable, this, _payload));
        }
    }
    private void OnDestroy()
    {
        if (!_deregistered)
        {
            _deregistered = true;
            OnEvent?.Invoke(new ItemUIEventData(ItemUIEventData.EventType.Disable, this, _payload));
        }
    }
    private void OnEnable()
    {
        _deregistered = false;
    }
}
