using NinthArt;
using System.Collections.Generic;
using UnityEngine;

public class ItemManager : Singleton<ItemManager>
{
    public GameItems items;
    [ContextMenu("AddSort")]
    public void AddSort()
    {
        AddItem("Sort");
    }
    [ContextMenu("AddShuffle")]
    public void AddShuffle()
    {
        AddItem("Shuffle");
    }
    [ContextMenu("AddVipSlot")]
    public void AddVipSlot()
    {
        AddItem("VipSlot");
    }
    public static void AddItem(Item newItem, int num = 1)
    {
        if (newItem is BaseItem baseItem)
        {
            switch (baseItem.itemType)
            {
                case BaseItem.BaseItemType.COINT:
                    NinthArt.Profile.CoinAmount += num;
                    NinthArt.Profile.CoinCollected += num;
                    break;
                case BaseItem.BaseItemType.STAR:
                    NinthArt.Profile.StarAmount += num;
                    NinthArt.Profile.StarCollected += num;
                    break;
            }
            return;
        }
        foreach (Items items in NinthArt.Profile.Items)
        {
            if (items.item == null || items.item.itemId == null || string.IsNullOrEmpty(items.item.itemId))
            {
                continue;
            }

            if (items.item.itemId.Contains(newItem.itemId))
            {
                items.num += num;
                EventManager.Annouce(NinthArt.EventType.NumItemChange);
                return;
            }
        }

        Items newItems = new Items();
        newItems.item = newItem;
        newItems.num = num;

        NinthArt.Profile.AddItem(newItems);
        EventManager.Annouce(NinthArt.EventType.NumItemChange);
    }
    public static void AddItem(string itemId, int num = 1)
    {
        foreach (Items items in NinthArt.Profile.Items)
        {
            if (items.item == null || items.item.itemId == null || string.IsNullOrEmpty(items.item.itemId))
                continue;
            if (items.item.itemId.Contains(itemId))
            {
                items.num += num;
                EventManager.Annouce(NinthArt.EventType.NumItemChange);
                return;
            }
        }

        Item getItem = GetItemById(itemId);
        Item newItem = new Item();

        if (getItem != null)
        {
            newItem = getItem;
        }
        else
        {
            newItem.itemId = itemId;
        }

        Items newItems = new Items();
        newItems.item = newItem;
        newItems.num = num;

        NinthArt.Profile.AddItem(newItems);
        EventManager.Annouce(NinthArt.EventType.NumItemChange);
    }
    [ContextMenu("AddAllItem+1")]
    public void AddAllItem()
    {
        foreach(Item item in items.items)
        {
            AddItem(item);
        }    
    }    
    public static void RemoveItem(string itemId, int num = 1)
    {
        Items removeItems = null;
        foreach (Items items in NinthArt.Profile.Items)
        {
            if ((items.item != null && items.item.itemId != null && !string.IsNullOrEmpty(items.item.itemId)) && items.item.itemId.Contains(itemId))
            {
                if (items.num > num)
                {
                    items.num -= num;
                    EventManager.Annouce(NinthArt.EventType.NumItemChange);
                }    
                else
                    removeItems = items;

                break;
            }
        }
        if (removeItems != null && NinthArt.Profile.Items.Contains(removeItems))
        {
            NinthArt.Profile.RemoveItem(removeItems);
            EventManager.Annouce(NinthArt.EventType.NumItemChange);
        }    
    }
    public static void RemoveItems(List<Item> items)
    {
        // remove use list item input
    }
    public static bool UseItem(Item newItem)
    {
        foreach (Items items in NinthArt.Profile.Items)
        {
            if (items.item == null || items.item.itemId == null || string.IsNullOrEmpty(items.item.itemId))
                continue;

            if (items.item.itemId.Contains(newItem.itemId))
            {
                items.item.UseItem();
                return true;
            }
        }

        return false;
    }
    public static bool UseItem(string itemId)
    {
        foreach (Items items in NinthArt.Profile.Items)
        {
            if (items.item == null || items.item.itemId == null || string.IsNullOrEmpty(items.item.itemId))
                continue;

            if (items.item.itemId.Contains(itemId))
            {
                items.item.UseItem();
                return true;
            }
        }

        return false;
    } 
    public static int CountNumItem(string itemId)
    {
        foreach (Items items in NinthArt.Profile.Items)
        {
            if (items.item == null || items.item.itemId == null || string.IsNullOrEmpty(items.item.itemId))
                continue;

            if (items.item.itemId.Contains(itemId))
            {
                return items.num;
            }
        }

        return 0;
    }
    public static bool BuyAndUseItem(Item item)
    {
        if (NinthArt.Profile.CoinAmount < item.price)
        {
            //TODO: notify something -> hien popup xem quang cao de mua
            /*
            if (Gameplay.Instance != null)
            {
                Gameplay.Instance.curAdsItem = item;
                SceneManager.OpenPopup(SceneID.AdsSupportItemUi);
            }*/
            return false;
        }

        NinthArt.Profile.CoinAmount -= item.price;
        AddItem(item);
        item.UseItem();

        return true;
    }
    public static bool BuyItem(Item item, int num = 1)
    {
        int totalPrice = item.price * num;
        if (NinthArt.Profile.CoinAmount < totalPrice)
        {
            //TODO: notify something
            return false;
        }

        NinthArt.Profile.CoinAmount -= totalPrice;
        AddItem(item, num);

        return true;
    }    
    public static bool IsHaveItem(Item newItem)
    {
        foreach (Items items in NinthArt.Profile.Items)
        {
            if (items.item == null || items.item.itemId == null || string.IsNullOrEmpty(items.item.itemId))
                continue;

            if (items.item.itemId.Contains(newItem.itemId))
            {
                return true;
            }
        }

        return false;
    }
    public static int GetNumItem(Item newItem)
    {
        foreach (Items items in NinthArt.Profile.Items)
        {
            if (items.item == null || items.item.itemId == null || string.IsNullOrEmpty(items.item.itemId))
                continue;

            if (items.item.itemId.Contains(newItem.itemId))
            {
                return items.num;
            }
        }

        return 0;
    }
    public static Item GetItemById(string id)
    {
        foreach(Item item in Instance.items.items)
        {
            if (!item.itemId.Contains(id))
                continue;

            return item;
        }

        return null;
    }    
}
[System.Serializable]
public enum GameTool
{
    Sort = 0,
    Shuffle = 1,
    VipSlot = 2,
}
