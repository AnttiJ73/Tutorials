using UnityEngine;
using UnityEngine.Events;

public class InventoryDisplay : MonoBehaviour
{
    [SerializeField] private Transform _content;
    [SerializeField] private ViewElement _viewElementPrefab;
    [SerializeField] private Inventory<Item> _inventory;

    public UnityEvent<ItemUIEventData> OnInteract = new UnityEvent<ItemUIEventData>();

    private bool _initialized;

    private void OnEnable()
    {
        if (_inventory != null && !_initialized)
        {
            _inventory.OnChanged.AddListener(Refresh);
            _initialized = true;
        }
        Refresh();
    }

    private void OnDisable()
    {
        if (_inventory != null && _initialized)
        {
            _inventory.OnChanged.RemoveListener(Refresh);
            _initialized = false;
        }
    }

    private void Start()
    {
        if (_inventory != null && !_initialized)
        {
            _inventory.OnChanged.AddListener(Refresh);
            _initialized = true;
        }
        Refresh();
    }

    public void Refresh()
    {
        foreach (Transform t in _content)
        {
            Destroy(t.gameObject);
        }

        foreach (ItemContainer<Item> item in _inventory.Items)
        {
            var displayData = DisplayItem(item);
            if (displayData == null)
            {
                continue;
            }

            var instance = Instantiate(_viewElementPrefab, _content);
            instance.Display(displayData);
            instance.OnEvent += HandleUIEvent;
        }
    }

    private ViewElementData DisplayItem(ItemContainer<Item> item)
    {
        return new ViewElementData
        {
            HeaderText = item.Item.name,
            MainIcon = item.Item.Icon,
            ValueText = item.Item.Value.ToString(),
            AmountText = item.Amount.ToString(),
            Payload = item
        };
    }

    private void HandleUIEvent(ItemUIEventData eventData)
    {
        OnInteract.Invoke(eventData);
    }
}