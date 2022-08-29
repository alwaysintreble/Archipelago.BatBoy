using System;
using System.Collections.Generic;
using System.Reflection;
using Archipelago.BatBoy.Constants;
using Archipelago.BatBoy.ServerCommunication;

namespace Archipelago.BatBoy.DataHandling;

public class ArchipelagoItemsController
{
    public static void OnCollect(On.Collectable.orig_Collect orig, Collectable self)
    {
        orig(self);

        BatBoySlot saveSlot = SaveManager.Savegame.GetCurrentSlot();
        int levelIndex = StageManager.Instance.LevelIndex;

        List<Collectable.CollectableType> sendableItems = new()
        {
            Collectable.CollectableType.RedSeed,
            Collectable.CollectableType.GreenSeed,
            Collectable.CollectableType.GoldenSeed,
            Collectable.CollectableType.Casette,
        };

        if (sendableItems.Contains(self.type))
        {
            switch (self.type)
            {
                case Collectable.CollectableType.RedSeed:
                    saveSlot.RedSeedCollectedLevels.Add(levelIndex);
                    SendLocationCheck((Level)levelIndex, LocationType.RedSeed);
                    break;
                case Collectable.CollectableType.GreenSeed:
                    saveSlot.GreenSeedCollectedLevels.Add(levelIndex);
                    SendLocationCheck((Level)levelIndex, LocationType.GreenSeed);
                    break;
                case Collectable.CollectableType.GoldenSeed:
                    saveSlot.GoldenSeedCollectedLevels.Add(levelIndex);
                    SendLocationCheck((Level)levelIndex, LocationType.GoldenSeed);
                    break;
                case Collectable.CollectableType.Casette:
                    SendLocationCheck((Level)levelIndex, LocationType.Casette);
                    break;
            }
        }
    }
    
    public void SaveLevelClear(On.BatBoySaveGame.orig_SaveLevelClear orig, BatBoySaveGame saveGame, int levelIndex)
    {
        BatBoySlot saveSlot = saveGame.Slots[saveGame.CurrentSlot];
        StageManager.Instance.LevelAlreadyClearedBefore = true;
        
        if (!saveSlot.LevelsClear.Contains(levelIndex))
        {
            saveGame.Slots[saveGame.CurrentSlot].LevelsClear.Add(levelIndex);
            SendLocationCheck((Level)levelIndex, LocationType.LevelClear);
        }

        if (StageManager.Instance.CollectedRedSeed &&
            !saveSlot.RedSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.RedSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck((Level)levelIndex, LocationType.RedSeed);
        }

        if (StageManager.Instance.CollectedGreenSeed &&
            !saveSlot.GreenSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.GreenSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck((Level)levelIndex, LocationType.GreenSeed);
            
        }
        
        if (StageManager.Instance.CollectedGoldenSeed &&
            !saveSlot.GoldenSeedCollectedLevels.Contains(levelIndex))
        {
            saveSlot.GoldenSeedCollectedLevels.Add(levelIndex);
            SendLocationCheck((Level)levelIndex, LocationType.GoldenSeed);
        }

        if (StageManager.Instance.CollectedCasette &&
            !saveSlot.CasetteCollectedLevels.Contains(levelIndex))
        {
            // TODO There isn't a counter for cassettes they're tied to levels. Investigate
            // perhaps since the cassettes don't actually unlock anything currently i just
            // add them to the list in numerical order when received?
            saveSlot.CasetteCollectedLevels.Add(levelIndex);
        }
        
        GetCorrectAbilities(saveGame.GetCurrentSlot());
        
        saveSlot.Crystals += StageManager.Instance.CollectedCrystals;
        if (saveSlot.Crystals > 999)
            saveSlot.Crystals = 999;
        SaveManager.Save();
    }
    
    public static void SendLocationCheck(Level currentLevel, LocationType locationType)
    {
        --currentLevel;
        LocationsAndItemsHelper.CheckLocation(currentLevel, locationType);
        APLog.LogInfo($"{locationType} for {currentLevel} found!");
    }

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