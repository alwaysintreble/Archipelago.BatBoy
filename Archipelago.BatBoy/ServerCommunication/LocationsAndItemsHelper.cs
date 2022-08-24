using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Archipelago.BatBoy.ServerCommunication;

public static class LocationsAndItemsHelper
{
    public struct LevelLocation
    {
        [UsedImplicitly] private Level _level;
        [UsedImplicitly] private LocationType _locationType;

        public LevelLocation(Level level, LocationType locationType)
        {
            _level = level;
            _locationType = locationType;
        }
    }
    
    
    public static readonly Dictionary<long, Item> ItemsLookup = new();
    public static readonly Dictionary<long, Ability> AbilitiesLookup = new();
    public static readonly Dictionary<LevelLocation, long> LevelLocationsLookup = new();
    public static readonly Dictionary<ShopSlots, long> ShopLocationsLookup = new();
    
    public static void Init()
    {
        const long baseCodeOffset = 696969;
        const long abilityCodeOffset = 20;
        const long shopCodeOffset = 200;

        // add items and abilities to item lookup dictionary
        foreach (Item i in Enum.GetValues(typeof(Item)))
        {
            ItemsLookup[baseCodeOffset + (int)i] = i;
        }
        foreach (Ability i in Enum.GetValues(typeof(Ability)))
        {
            AbilitiesLookup[baseCodeOffset + abilityCodeOffset + (int)i] = i;
        }
        
        // add locations and shops to location lookup dictionary
        foreach (Level i in Enum.GetValues(typeof(Level)))
        {
            foreach (LocationType j in Enum.GetValues(typeof(LocationType)))
            {
                LevelLocationsLookup[new LevelLocation(i, j)] =
                    baseCodeOffset + (int)i * 5 + (int)j;
            }
        }
        // only one shop so pseudo hard coding this for now
        foreach (ShopSlots i in Enum.GetValues(typeof(ShopSlots)))
        {
            ShopLocationsLookup[i] = baseCodeOffset + shopCodeOffset + (int)i;
        }
        
#if DEBUG
        APLog.LogInfo("Items Lookup:");
        ItemsLookup.Select(i => $"{i.Key}: {i.Value}").ToList().ForEach(Console.WriteLine);
        APLog.LogInfo("Abilities Lookup:");
        AbilitiesLookup.Select(i => $"{i.Key}: {i.Value}").ToList().ForEach(Console.WriteLine);
        APLog.LogInfo("Levels Lookup:");
        LevelLocationsLookup.Select(i => $"{i.Key}: {i.Value}").ToList().ForEach(Console.WriteLine);
        APLog.LogInfo("Shops Lookup:");
        ShopLocationsLookup.Select(i => $"{i.Key}: {i.Value}").ToList().ForEach(Console.WriteLine);
#endif
    }
    
    public static void SendAsyncChecks()
    {
        BatBoySlot saveSlot = SaveManager.Savegame.GetCurrentSlot();
        foreach (int i in Enum.GetValues(typeof(Level)))
        {
            foreach (LocationType j in Enum.GetValues(typeof(LocationType)))
            {
                if (ArchipelagoClient.ServerData.Checked.Contains(LevelLocationsLookup[new LevelLocation((Level)i, j)])) 
                    continue;
                Level currentLevel = (Level)i + 1;
                switch (j)
                {
                    case LocationType.RedSeed:
                        if(saveSlot.RedSeedCollectedLevels.Contains(i))
                            CheckLocation(currentLevel, j);
                        break;
                    case LocationType.GreenSeed:
                        if (saveSlot.GreenSeedCollectedLevels.Contains(i))
                            CheckLocation(currentLevel, j);
                        break;
                    case LocationType.GoldenSeed:
                        if (saveSlot.GoldenSeedCollectedLevels.Contains(i))
                            CheckLocation(currentLevel, j);
                        break;
                    case LocationType.LevelClear:
                        if (saveSlot.LevelsClear.Contains(i))
                            CheckLocation(currentLevel, j);
                        break;
                }
            }
        }
        foreach (ShopSlots i in ArchipelagoClient.ServerData.ShopSlotsChecked)
        {
            if (!ArchipelagoClient.ServerData.Checked.Contains(ShopLocationsLookup[i]))
                CheckLocation(i);
        }
    }
    
    public static void Unlock(Item unlockItem)
    {
        BatBoySlot saveSlot = SaveManager.Savegame.GetCurrentSlot();
        
        switch (unlockItem)
        {
            case Item.RedSeed:
                ++saveSlot.RedSeeds;
                break;
            case Item.GreenSeed:
                ++saveSlot.GreenSeeds;
                break;
            case Item.GoldenSeed:
                ++saveSlot.GoldenSeeds;
                break;
            case Item.Casette:
                break;
            case Item.Health:
                ++saveSlot.Health;
                break;
            case Item.Stamina:
                ++saveSlot.Stamina;
                break;
        }
        APLog.LogInfo($"Received {unlockItem}!");
    }

    public static void Unlock(Ability unlockAbility)
    {
        BatBoySlot saveSlot = SaveManager.Savegame.GetCurrentSlot();
        AbilityMap.AbilityFields[unlockAbility].SetValue(saveSlot, true);
        APLog.LogInfo($"Received {unlockAbility}!");
        ArchipelagoClient.ServerData.AcquiredAbilities.Add(unlockAbility);
        SaveManager.Save();
    }

    public static void CheckLocation(Level level, LocationType locationType)
    {
        long checkID = LevelLocationsLookup[new LevelLocation(level, locationType)];
        ArchipelagoClient.Session.Locations.CompleteLocationChecks(checkID);
        ArchipelagoClient.ServerData.Checked.Add(checkID);
    }

    public static void CheckLocation(ShopSlots slot)
    {
        while (ArchipelagoClient.ServerData.ShopSlotsChecked.Contains(slot))
            ++slot;
        ArchipelagoClient.ServerData.ShopSlotsChecked.Add(slot);
        long checkID = ShopLocationsLookup[slot];
        ArchipelagoClient.Session.Locations.CompleteLocationChecks(checkID);
        ArchipelagoClient.ServerData.Checked.Add(checkID);
    }
}