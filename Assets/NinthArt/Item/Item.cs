using UnityEngine;

[CreateAssetMenu(menuName = "ScriptableObjects/Item")]
[System.Serializable]
public class Item : ScriptableObject
{
    public string itemId;
    public int price;
    public Sprite icon;

    public virtual void UseItem()
    {
        ItemManager.RemoveItem(itemId);
    }
    public virtual void UseItem(int num)
    {
        ItemManager.RemoveItem(itemId, num);
    }
    public virtual void AddItem(int num)
    {

    }
}
