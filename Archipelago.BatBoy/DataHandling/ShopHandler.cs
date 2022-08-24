using System;
using System.Collections.Generic;
using System.Reflection;
using Archipelago.BatBoy.ServerCommunication;
using HarmonyLib;

namespace Archipelago.BatBoy.DataHandling;

public class ShopHandler
{
    private readonly FieldInfo _shopItemsInfo;
    private readonly FieldInfo _selectedItemInfo;
    private readonly List<Shop> _consumablesBought = new();

    public ShopHandler()
    {
        _shopItemsInfo = AccessTools.Field(typeof(UIShop), "shopItems");
        _selectedItemInfo = AccessTools.Field(typeof(UIShop), "selectedItemIndex");
    }
    
    // TODO all of the shop code will probably need a rewrite when game releases as i had to do a lot of hardcoding
    public void OnTransaction(On.UIShop.orig_CommitTransaction orig, UIShop self)
    {
        var (purchaseItem, itemIndex) = this.GetCurrentShopItem(self);
        if (TransactionIsDone(purchaseItem, SaveManager.Savegame.GetCurrentSlot()) && 
            purchaseItem.ShopItemType != ShopItem.ShopItemTypes.GoldenSeed)
        {
            if ((ShopSlots)itemIndex != ShopSlots.Slot1)
            {
                // TODO somehow check which shop this is here
                if (!_consumablesBought.Contains(Shop.RedSeedShop))
                {
                    _consumablesBought.Add(Shop.RedSeedShop);
                    ArchipelagoItemsController.SendShopLocation(purchaseItem);
                    LocationsAndItemsHelper.CheckLocation((ShopSlots)itemIndex);
                }
            }
            else
            {
                ArchipelagoItemsController.SendShopLocation(purchaseItem);
                LocationsAndItemsHelper.CheckLocation((ShopSlots)itemIndex);
            }
        }

        orig(self);
    }

    private Tuple<ShopItem, int> GetCurrentShopItem(UIShop currentShop)
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

    private static bool TransactionIsDone(ShopItem currentItem, BatBoySlot saveSlot)
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