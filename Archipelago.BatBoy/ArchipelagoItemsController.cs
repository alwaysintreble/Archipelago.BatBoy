using System;
using System.Collections.Generic;
using System.Reflection;
using Archipelago.BatBoy.ServerCommunication;
using UnityEngine;

namespace Archipelago.BatBoy;

public class ArchipelagoItemsController
{
    public void SaveLevelClear(On.BatBoySaveGame.orig_SaveLevelClear orig, BatBoySaveGame saveGame, int levelIndex)
    {
        BatBoySlot saveSlot = saveGame.Slots[saveGame.CurrentSlot];
        
        if (!saveSlot.LevelsClear.Contains(levelIndex))
        {
            saveGame.Slots[saveGame.CurrentSlot].LevelsClear.Add(levelIndex);
            SendLocationCheck(LocationType.LevelClear, (Level)levelIndex);
        }
        else
        {
            APLog.LogInfo($"{(Level)levelIndex} {LocationType.LevelClear}");
        }

        if (StageManager.Instance.CollectedRedSeed &&
            !saveSlot.RedSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.RedSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck(LocationType.RedSeed, (Level)levelIndex);
        }

        if (StageManager.Instance.CollectedGreenSeed &&
            !saveSlot.GreenSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.GreenSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck(LocationType.GreenSeed, (Level)levelIndex);
            
        }

        if (StageManager.Instance.CollectedGoldenSeed &&
            !saveSlot.GoldenSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.GoldenSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck(LocationType.GoldenSeed, (Level)levelIndex);
        }

        if (StageManager.Instance.CollectedCasette &&
            !saveSlot.CasetteCollectedLevels.Contains(levelIndex))
        {
            // TODO There isn't a counter for cassettes they're tied to levels. Investigate
            saveSlot.CasetteCollectedLevels.Add(levelIndex);
        }
        
        GetCorrectAbilities(saveGame.GetCurrentSlot());
        
        saveSlot.Crystals += StageManager.Instance.CollectedCrystals;
        if (saveSlot.Crystals > 999)
            saveSlot.Crystals = 999;
        SaveManager.Save();
    }
    
    private void SendLocationCheck(LocationType locationType, Level currentLevel)
    {
        --currentLevel;
        ArchipelagoClient.CheckLocation(currentLevel, locationType);
        APLog.LogInfo($"{locationType} for {currentLevel} found!");
    }

    public static void SendShopLocation(ShopItem shopItem)
    {
        BatBoySlot saveSlot = SaveManager.Savegame.GetCurrentSlot();
        switch (shopItem.ShopItemType)
        {
            case ShopItem.ShopItemTypes.RedSeed:
                --saveSlot.RedSeeds;
                break;
            case ShopItem.ShopItemTypes.GreenSeed:
                --saveSlot.GreenSeeds;
                break;
            case ShopItem.ShopItemTypes.GoldenSeed:
                --saveSlot.GoldenSeeds;
                break;
            case ShopItem.ShopItemTypes.IncreaseHP:
                --saveSlot.Health;
                break;
            case ShopItem.ShopItemTypes.IncreaseStamina:
                --saveSlot.Stamina;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        APLog.LogInfo($"{shopItem.ShopItemType} in shop purchased");
    }


    // TODO this gets run before the ability tutorial
    // either find a way to skip these tutorials or to do this loop after it
    private void GetCorrectAbilities(BatBoySlot saveSlot)
    {
        #if DEBUG
        APLog.LogInfo($"Current abilities: ");
        foreach (Ability ability in ArchipelagoClient.ServerData.AcquiredAbilities)
        {
            APLog.LogInfo(ability);
        }
        #endif
        foreach (Ability ability in Enum.GetValues(typeof(Ability)))
        {
            if (!ArchipelagoClient.ServerData.AcquiredAbilities.Contains(ability))
            {
                FieldInfo abilityField = AbilityMap.AbilityFields[ability];
                abilityField.SetValue(saveSlot, false);
                #if DEBUG
                APLog.LogInfo($"Goodbye Jojo! ({ability})");
                #endif
            }
        }
    }
}