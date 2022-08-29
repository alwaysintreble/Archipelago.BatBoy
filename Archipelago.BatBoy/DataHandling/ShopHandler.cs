using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Archipelago.BatBoy.Constants;
using Archipelago.BatBoy.ServerCommunication;
using HarmonyLib;

namespace Archipelago.BatBoy.DataHandling;

public class ShopHandler
{
    private readonly FieldInfo _shopItemsInfo;
    private readonly FieldInfo _selectedItemInfo;
    private List<Shop> _consumablesBought = new();

    private Dictionary<Shop, List<ShopSlots>> _slotsBoughtPerShop = new()
    {
        { Shop.RedSeedShop, new List<ShopSlots>() },
    };

    public ShopHandler()
    {
        _shopItemsInfo = AccessTools.Field(typeof(UIShop), "shopItems");
        _selectedItemInfo = AccessTools.Field(typeof(UIShop), "selectedItemIndex");
    }
    
    // TODO all of the shop code will probably need a rewrite when game releases as i had to do a lot of hardcoding
    public void OnTransaction(On.UIShop.orig_CommitTransaction orig, UIShop self)
    {
        var purchaseItem = GetCurrentShopItem(self);
        if (TransactionIsDone(purchaseItem, SaveManager.Savegame.GetCurrentSlot()))
        {
            if (StageManager.Instance.IsPlatformingStage)
            {
                Level level = (Level)StageManager.Instance.LevelIndex;
                ArchipelagoItemsController.SendLocationCheck(level, LocationType.GoldenSeed);
            }
            else
            {
                BatBoySlot saveSlot = SaveManager.Savegame.GetCurrentSlot();
                ShopSlots itemIndex = ShopSlots.Slot1;
                switch (purchaseItem.ShopItemType)
                {
                    case ShopItem.ShopItemTypes.RedSeed:
                        --saveSlot.RedSeeds;
                        foreach (ShopSlots slot in Enum.GetValues(typeof(ShopSlots)))
                        {
                            if (!ArchipelagoClient.ServerData.ShopSlotsChecked.Contains(slot))
                            {
                                itemIndex = slot;
                                break;
                            }
                        }
                        LocationsAndItemsHelper.CheckLocation(itemIndex);
                        break;
                    case ShopItem.ShopItemTypes.GreenSeed:
                        --saveSlot.GreenSeeds;
                        foreach (ShopSlots slot in Enum.GetValues(typeof(ShopSlots)))
                        {
                            if (!ArchipelagoClient.ServerData.ShopSlotsChecked.Contains(slot))
                            {
                                itemIndex = slot;
                                break;
                            }
                        }
                        _slotsBoughtPerShop[Shop.RedSeedShop].Add(itemIndex);
                        LocationsAndItemsHelper.CheckLocation(itemIndex);
                        break;
                    case ShopItem.ShopItemTypes.GoldenSeed:
                        --saveSlot.GoldenSeeds;
                        foreach (ShopSlots slot in Enum.GetValues(typeof(ShopSlots)))
                        {
                            if (!ArchipelagoClient.ServerData.ShopSlotsChecked.Contains(slot))
                            {
                                itemIndex = slot;
                                break;
                            }
                        }
                        _slotsBoughtPerShop[Shop.RedSeedShop].Add(itemIndex);
                        LocationsAndItemsHelper.CheckLocation(itemIndex);
                        break;
                    case ShopItem.ShopItemTypes.IncreaseHP:
                        if (!ArchipelagoClient.ServerData.ShopSlotsChecked.Contains(ShopSlots.Consumable))
                        {
                            --saveSlot.Health;
                            LocationsAndItemsHelper.CheckLocation(ShopSlots.Consumable);
                        }
                        break;
                    case ShopItem.ShopItemTypes.IncreaseStamina:
                        --saveSlot.Stamina;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                APLog.LogInfo($"{purchaseItem.ShopItemType} in shop purchased");
            }
        }

        orig(self);
    }

    private ShopItem GetCurrentShopItem(UIShop currentShop)
    {
        ShopItem[] shopItems = _shopItemsInfo.GetValue(currentShop) as ShopItem[];
        int selectedItemIndex = (int)_selectedItemInfo.GetValue(currentShop);
        if (shopItems == null)
        {
            throw new ArgumentOutOfRangeException();
        }
        ShopItem shopItem = shopItems[selectedItemIndex];

        return shopItem;
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