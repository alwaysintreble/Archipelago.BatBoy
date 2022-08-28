using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;

namespace Archipelago.BatBoy.ServerCommunication;

public class APData
{
    public long Index;
    public string HostName;
    public string SlotName;
    public string Password;
    public HashSet<long> @Checked = new();
    public List<Ability> AcquiredAbilities = new();
    public List<ShopSlots> ShopSlotsChecked = new();
    public bool DeathLink = false;
    
    private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
        @"..\Bat Boy Demo\BepInEx\plugins\Archipelago\saves\"); // TODO this will need to change when game releases
    
    public void LoadAPInfo(int slot)
    {
        var path = _filePath + $"connectInfo{slot}.apbb";
        if (File.Exists(path))
        {
            using (StreamReader reader = new StreamReader(path))
            {
                APData tempData = JsonConvert.DeserializeObject<APData>(reader.ReadToEnd());
                if (ArchipelagoClient.Authenticated && SaveManager.Savegame.GetCurrentSlot().Health != 0)
                {
                    Checked = tempData.Checked;
                    AcquiredAbilities = tempData.AcquiredAbilities;
                }
                else
                {
                    Index = tempData.Index;
                    HostName = tempData.HostName;
                    SlotName = tempData.SlotName;
                    Password = tempData.Password;
                    Checked = tempData.Checked;
                    AcquiredAbilities = tempData.AcquiredAbilities;
                    ShopSlotsChecked = tempData.ShopSlotsChecked;
                    DeathLink = tempData.DeathLink;
                    
                    if (ArchipelagoClient.Connect() && ArchipelagoClient.ServerData.Checked != null)
                        ArchipelagoClient.Session.Locations.CompleteLocationChecks(ArchipelagoClient.ServerData.Checked
                            .ToArray());
                }
            }
        }
    }
    
    public void SaveAPInfo(On.SaveManager.orig_Save orig)
    {
        orig();
            
        string json = JsonConvert.SerializeObject(ArchipelagoClient.ServerData);
        int slot = SaveManager.Savegame.CurrentSlot;
        if (!Directory.Exists(_filePath))
            Directory.CreateDirectory(_filePath);
        File.WriteAllText(_filePath + $"connectInfo{slot}.apbb", json);
    }
}