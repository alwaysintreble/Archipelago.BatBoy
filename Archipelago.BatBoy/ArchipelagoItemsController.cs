using System;
using System.Reflection;
namespace Archipelago.BatBoy;

public class ArchipelagoItemsController
{
    public void SaveLevelClear(On.BatBoySaveGame.orig_SaveLevelClear orig, BatBoySaveGame saveGame, int levelIndex)
    {
        BatBoySlot saveSlot = saveGame.Slots[saveGame.CurrentSlot];
        
        if (!saveSlot.LevelsClear.Contains(levelIndex))
        {
            saveGame.Slots[saveGame.CurrentSlot].LevelsClear.Add(levelIndex);
            SendLocationCheck(LocationType.LevelClear, (Levels)levelIndex);
        }
        else
        {
            APLog.LogInfo($"{(Levels)levelIndex} {LocationType.LevelClear}");
        }

        if (StageManager.Instance.CollectedRedSeed &&
            !saveSlot.RedSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.RedSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck(LocationType.RedSeed, (Levels)levelIndex);
        }

        if (StageManager.Instance.CollectedGreenSeed &&
            !saveSlot.GreenSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.GreenSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck(LocationType.GreenSeed, (Levels)levelIndex);
            
        }

        if (StageManager.Instance.CollectedGoldenSeed &&
            !saveSlot.GoldenSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.GoldenSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck(LocationType.GoldenSeed, (Levels)levelIndex);
        }

        if (StageManager.Instance.CollectedCasette &&
            !saveSlot.CasetteCollectedLevels.Contains(levelIndex))
        {
            // TODO There isn't a counter for cassettes they're tied to levels. Investigate
            saveSlot.CasetteCollectedLevels.Add(levelIndex);
        }

        // GetCorrectAbilities(saveGame.GetCurrentSlot());
        saveSlot.Crystals += StageManager.Instance.CollectedCrystals;
        if (saveSlot.Crystals > 999)
            saveSlot.Crystals = 999;
        SaveManager.Save();
    }
    
    // TODO
    private void SendLocationCheck(LocationType locationType, Levels currentLevel)
    {
        APLog.LogInfo($"{locationType} for {currentLevel} found!");
    }

    // TODO
    public void SendShopLocation(ShopItem shopItem)
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
        }
        APLog.LogInfo($"{shopItem.ShopItemType} in shop purchased");
    }

    // TODO this gets run before the ability tutorial
    // either find a way to skip these tutorials or to do this loop after it
    private void GetCorrectAbilities(BatBoySlot saveSlot)
    {
        foreach (Abilities ability in Enum.GetValues(typeof(Abilities)))
        {
            FieldInfo abilityField = AbilityMap.AbilityFields[ability];
            abilityField.SetValue(saveSlot, false);
            APLog.LogInfo("Goodbye Jojo");
        }
    }
}