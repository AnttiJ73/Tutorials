using UnityEngine;

public class EquipmentInventoryController : MonoBehaviour
{
    private ItemContainer<Item> _draggedItem;
    private ItemSlot _draggedFromSlot;
    private ItemSlot _currentHoveredEquipmentSlot;
    [SerializeField] private RectTransform _storagePanelRect;

    public void HandleUIEventFromStorage(ItemUIEventData eventData)
    {
        switch (eventData.Type)
        {
            case ItemUIEventData.EventType.HoverEnter:
                break;
            case ItemUIEventData.EventType.HoverExit:
                break;
            case ItemUIEventData.EventType.DragStart:
                if (eventData.TryGetPayload<ItemContainer<Item>>(out var container))
                {
                    _draggedItem = container.CreateCopy();
                    _draggedFromSlot = null;
                }

                break;
            case ItemUIEventData.EventType.DragEnd:
                if (_draggedItem != null)
                {
                    if (_currentHoveredEquipmentSlot != null)
                    {
                        TryEquipDraggedItemToSlot(_currentHoveredEquipmentSlot);
                    }
                }
                ClearDragState();
                break;
            default:
                break;
        }
    }

    public void HandleUIEventFromEquipment(ItemUIEventData eventData)
    {
        switch (eventData.Type)
        {
            case ItemUIEventData.EventType.HoverEnter:
                if (eventData.TryGetPayload<ItemSlot>(out var slot))
                {
                    _currentHoveredEquipmentSlot = slot;
                }
                break;
            case ItemUIEventData.EventType.HoverExit:
                if (eventData.TryGetPayload<ItemSlot>(out var exitSlot))
                {
                    if (_currentHoveredEquipmentSlot == exitSlot)
                    {
                        _currentHoveredEquipmentSlot = null;
                    }
                }
                break;
            case ItemUIEventData.EventType.DragStart:
                if (eventData.TryGetPayload<ItemSlot>(out var fromSlot))
                {
                    if (fromSlot.IsEmpty())
                    {
                        return;
                    }

                    if (fromSlot.ItemContainer != null)
                    {
                        _draggedItem = fromSlot.ItemContainer.CreateCopy();
                        _draggedFromSlot = fromSlot;
                    }
                }
                break;
            case ItemUIEventData.EventType.DragEnd:
                if (_draggedItem != null)
                {
                    bool hoveredStorage = IsStoragePanelHovered(eventData.Position);

                    if (hoveredStorage)
                    {
                        TryUnequipDraggedItemToStorage();
                    }
                    else if (_currentHoveredEquipmentSlot != null)
                    {
                        TryMoveBetweenEquipmentSlots(_draggedFromSlot, _currentHoveredEquipmentSlot);
                    }
                }
                else
                {
                }
                ClearDragState();
                break;
            default:
                break;
        }
    }

    private bool SlotTypeMatches(ItemSlot slot, Item item)
    {
        return item != null && slot != null && item.SlotType == slot.Type;
    }

    private bool IsStoragePanelHovered(Vector2 screenPosition)
    {
        Camera uiCam = Camera.main;
        Canvas canvas = _storagePanelRect.GetComponentInParent<Canvas>();

        return RectTransformUtility.RectangleContainsScreenPoint(_storagePanelRect, uiCam.WorldToScreenPoint(screenPosition), uiCam);
    }

    private void TryEquipDraggedItemToSlot(ItemSlot targetSlot)
    {
        if (ServiceLocator.Instance == null)
        {
            return;
        }

        if (_draggedItem == null || _draggedItem.Item == null)
        {
            return;
        }

        if (_draggedItem.Item.Category != Item.ItemCategory.Equippable)
        {
            return;
        }

        if (!SlotTypeMatches(targetSlot, _draggedItem.Item))
        {
            return;
        }

        var loadout = ServiceLocator.Instance.PlayerLoadout;
        if (loadout == null)
        {
            return;
        }

        int targetIndex = loadout.Slots.IndexOf(targetSlot);
        if (targetIndex < 0)
        {
            return;
        }

        var previous = targetSlot.ItemContainer;
        if (previous != null && previous.Item != null && previous.Amount > 0)
        {
            ServiceLocator.Instance.PlayerInventory.AddItem(previous.CreateCopy());
        }

        bool removed = ServiceLocator.Instance.PlayerInventory.RemoveItem(_draggedItem);
        if (!removed)
        {
            return;
        }

        //Equip
        loadout.SetItemAt(targetIndex, _draggedItem);
    }

    private void TryUnequipDraggedItemToStorage()
    {
        if (ServiceLocator.Instance == null)
        {
            return;
        }

        if (_draggedFromSlot == null)
        {
            return;
        }

        if (_draggedItem == null || _draggedItem.Item == null)
        {
            return;
        }

        var loadout = ServiceLocator.Instance.PlayerLoadout;
        if (loadout == null)
        {
            return;
        }

        int fromIndex = loadout.Slots.IndexOf(_draggedFromSlot);
        if (fromIndex < 0)
        {
            return;
        }

        //Unequip
        ServiceLocator.Instance.PlayerInventory.AddItem(_draggedItem.CreateCopy());
        loadout.ClearItemAt(fromIndex);
    }

    private void TryMoveBetweenEquipmentSlots(ItemSlot fromSlot, ItemSlot toSlot)
    {
        if (ServiceLocator.Instance == null)
        {
            return;
        }

        if (fromSlot == null || _draggedItem == null) return;

        var loadout = ServiceLocator.Instance.PlayerLoadout;
        if (loadout == null)
        {
            return;
        }

        int fromIndex = loadout.Slots.IndexOf(fromSlot);
        int toIndex = loadout.Slots.IndexOf(toSlot);
        if (fromIndex < 0 || toIndex < 0)
        {
            return;
        }

        if (_draggedItem.Item.Category != Item.ItemCategory.Equippable)
        {
            return;
        }

        if (!SlotTypeMatches(toSlot, _draggedItem.Item))
        {
            return;
        }

        var targetPrev = toSlot.ItemContainer;
        var sourcePrev = fromSlot.ItemContainer;

        if (targetPrev != null && targetPrev.Item != null && targetPrev.Amount > 0)
        {
            if (!SlotTypeMatches(fromSlot, targetPrev.Item))
            {
                return;
            }
            // Swap
            loadout.SetItemAt(toIndex, _draggedItem);
            loadout.SetItemAt(fromIndex, targetPrev.CreateCopy());
        }
        else
        {
            // Move
            loadout.ClearItemAt(fromIndex);
            loadout.SetItemAt(toIndex, _draggedItem);
        }
    }

    private void ClearDragState()
    {
        _draggedItem = null;
        _draggedFromSlot = null;
    }
}
