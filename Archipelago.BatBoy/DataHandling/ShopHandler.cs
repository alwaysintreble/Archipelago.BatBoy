using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace Archipelago.BatBoy.DataHandling;

public class ShopHandler
{
    private readonly FieldInfo _shopItemsInfo;
    private readonly FieldInfo _selectedItemInfo;
    public readonly List<Shop> ConsumablesBought = new List<Shop>();

    public ShopHandler()
    {
        _shopItemsInfo = AccessTools.Field(typeof(UIShop), "shopItems");
        _selectedItemInfo = AccessTools.Field(typeof(UIShop), "selectedItemIndex");
    }

    public Tuple<ShopItem, int> GetCurrentShopItem(UIShop currentShop)
    {
        ShopItem[] shopItems = _shopItemsInfo.GetValue(currentShop) as ShopItem[];
        int selectedItemIndex = (int)_selectedItemInfo.GetValue(currentShop);
        if (shopItems == null)
        {
            throw new ArgumentOutOfRangeException();
        }
        ShopItem shopItem = shopItems[selectedItemIndex];

        return new Tuple<ShopItem, int>(shopItem, selectedItemIndex);
    }

    public static bool TransactionIsDone(ShopItem currentItem, BatBoySlot saveSlot)
    {
        return currentItem.ShopCurrency switch
        {
            ShopItem.ShopCurrencies.Crystals => currentItem.Cost <=
                                                saveSlot.Crystals + StageManager.Instance.CollectedCrystals,
            ShopItem.ShopCurrencies.StageCrystals => currentItem.Cost <= StageManager.Instance.CollectedCrystals,
            ShopItem.ShopCurrencies.RedGoldenSeed => currentItem.Cost <= saveSlot.RedSeeds + saveSlot.GoldenSeeds,
            ShopItem.ShopCurrencies.GreenGoldenSeed => currentItem.Cost <= saveSlot.GreenSeeds + saveSlot.GoldenSeeds,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}