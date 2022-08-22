using System.Reflection;
using HarmonyLib;

namespace Archipelago.BatBoy;

public class ShopHandler
{
    private readonly FieldInfo _shopItemsInfo;
    private readonly FieldInfo _selectedItemInfo;

    public ShopHandler()
    {
        _shopItemsInfo = AccessTools.Field(typeof(UIShop), "shopItems");
        _selectedItemInfo = AccessTools.Field(typeof(UIShop), "selectedItemIndex");
    }

    public ShopItem GetCurrentShopItem(UIShop currentShop)
    {
        ShopItem[] shopItems = _shopItemsInfo.GetValue(currentShop) as ShopItem[];
        int selectedItemIndex = (int)_selectedItemInfo.GetValue(currentShop);
        ShopItem shopItem = shopItems[selectedItemIndex];

        return shopItem;
    }

    public bool TransactionIsDone(ShopItem currentItem, BatBoySlot saveSlot)
    {
        switch (currentItem.ShopCurrency)
        {
            case ShopItem.ShopCurrencies.Crystals:
                if (currentItem.Cost <= saveSlot.Crystals + StageManager.Instance.CollectedCrystals)
                {
                    return true;
                    
                }

                return false;
            case ShopItem.ShopCurrencies.StageCrystals:
                if (currentItem.Cost <= StageManager.Instance.CollectedCrystals)
                {
                    return true;
                    
                }

                return false;
            case ShopItem.ShopCurrencies.RedGoldenSeed:
                if (currentItem.Cost <= saveSlot.RedSeeds + saveSlot.GoldenSeeds)
                {
                    return true;
                }

                return false;
            case ShopItem.ShopCurrencies.GreenGoldenSeed:
                if (currentItem.Cost <= saveSlot.GreenSeeds + saveSlot.GoldenSeeds)
                {
                    return true;
                }

                return false;
        }

        return false;
    }
}