using UnityEngine;

public class Item : MonoBehaviour
{
    public enum ItemCategory
    {
        None,
        Metal,
        Gem,
        Equipable
    }

    public string Id { get { return "item." + _id; } }

    [field: SerializeField]
    public Sprite Icon { get; private set; }

    [field: SerializeField]
    public ItemCategory Category { get; private set; }

    [field: SerializeField]
    public int Value { get; private set; }

}
