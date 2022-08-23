using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using BepInEx;
using UnityEngine;

using XPLUSGames.Base.SaveSystem;
using Archipelago.BatBoy.ServerCommunication;

namespace Archipelago.BatBoy
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.alwaysintreble.Archipelago.BatBoy";
        public const string PluginAuthor = "alwaysintreble";
        public const string PluginName = "Archipelago";
        public const string PluginVersion = "0.1.0";
        public const string Game = "BatBoy";
        
        private readonly ArchipelagoItemsController _locationsHandler = new ArchipelagoItemsController();
        private readonly ShopHandler _shopHandler = new ShopHandler();


        private readonly string _filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory,
            @"..\Bat Boy Demo\BepInEx\plugins\Archipelago\saves\"); // TODO this will need to change when game releases
        
        public BatBoySlot saveSlot;
        
        private void Awake()
        {
            APLog.Init(Logger);
            APLog.LogInfo("Hello World!");
            
            ArchipelagoClient.Init();
            
            // This only gets called on clearing levels but is the easiest way to handle collecting the seeds and
            // changing the ability states. Investigate other methods but unlikely.
            On.BatBoySaveGame.SaveLevelClear += _locationsHandler.SaveLevelClear;
            
            On.UIShop.CommitTransaction += OnTransaction;

            On.TitleScreen.StartGame += OnGameStart;
            On.UIShop.ShowPopup += OnShopPopup;
            On.SaveManager.Save += SaveAPInfo;

        }

        private void Update()
        {
            if (ArchipelagoClient.Authenticated && saveSlot != null)
            {
                ArchipelagoClient.DequeueUnlocks();
            }
        }

        private void LoadAPInfo(int slot)
        {
            var path = _filePath + $"connectInfo{slot}";
            if (File.Exists(path))
            {
                using (StreamReader reader = new StreamReader(path))
                {
                    APData tempData = JsonConvert.DeserializeObject<APData>(reader.ReadToEnd());
                    if (ArchipelagoClient.Authenticated && SaveManager.Savegame.GetCurrentSlot().Health != 0)
                    {
                        ArchipelagoClient.ServerData.Checked = tempData.Checked;
                        ArchipelagoClient.ServerData.AcquiredAbilities = tempData.AcquiredAbilities;
                    }
                    else
                        ArchipelagoClient.ServerData = tempData;

                    if (ArchipelagoClient.Connect() && ArchipelagoClient.ServerData.Checked != null)
                        ArchipelagoClient.Session.Locations.
                            CompleteLocationChecks(ArchipelagoClient.ServerData.Checked.ToArray());
                }
            }
        }

        private void SaveAPInfo(On.SaveManager.orig_Save orig)
        {
            orig();
            
            string json = JsonConvert.SerializeObject(ArchipelagoClient.ServerData);
            int slot = SaveManager.Savegame.CurrentSlot;
            if (!Directory.Exists(_filePath))
                Directory.CreateDirectory(_filePath);
            File.WriteAllText(_filePath + $"connectInfo{slot}", json);
        }

        private void OnTransaction(On.UIShop.orig_CommitTransaction orig, UIShop self)
        {
            var (purchaseItem, itemIndex) = _shopHandler.GetCurrentShopItem(self);
            if (ShopHandler.TransactionIsDone(purchaseItem, SaveManager.Savegame.GetCurrentSlot()))
            {
                if (itemIndex == (int)ShopSlots.Consumable)
                {
                    // somehow check which shop this is here
                    if (!_shopHandler.consumablesBought.Contains(Shop.RedSeedShop))
                    {
                        _shopHandler.consumablesBought.Add(Shop.RedSeedShop);
                        ArchipelagoItemsController.SendShopLocation(purchaseItem);
                        ArchipelagoClient.CheckLocation((ShopSlots)itemIndex);
                    }
                }
                else
                {
                    ArchipelagoClient.CheckLocation((ShopSlots)itemIndex);
                    ArchipelagoItemsController.SendShopLocation(purchaseItem);
                }
            }

            orig(self);
        }

        private void OnGameStart(On.TitleScreen.orig_StartGame orig, TitleScreen self, int number)
        {
            if (SaveManager.Savegame.Slots[number].Health != 0)
            {
                LoadAPInfo(number);
                ArchipelagoClient.SendAsyncChecks();
            }

            if (ArchipelagoClient.Authenticated)
            {
                orig(self, number);
                saveSlot = SaveManager.Savegame.GetCurrentSlot();
                APLog.LogInfo("Save loaded successfully");
            }
            else
                APLog.LogInfo("Not Connected to a server");
        }

        private IEnumerator OnShopPopup(On.UIShop.orig_ShowPopup orig, UIShop self, string key)
        {
            yield return null;
        }
        
        private void OnGUI()
        {
            #if DEBUG
            // Debug buttons to grant items and abilities :)
            if (GUI.Button(new Rect(0, 100, 50, 50), "Red Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.RedSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated {Item.RedSeed}");
                }
            }

            if (GUI.Button(new Rect(50, 100, 50, 50), "Green Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.GreenSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated {Item.GreenSeed}");
                }
            }
            
            if (GUI.Button(new Rect(100, 100, 50, 50), "Golden Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.GoldenSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated {Item.GoldenSeed}");
                }
            }

            if (GUI.Button(new Rect(150, 100, 50, 50), "100 Crystals"))
            {
                if (saveSlot != null)
                {
                    saveSlot.Crystals += 100;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated 100 Crystals");
                }
            }

            if (GUI.Button(new Rect(200, 100, 50, 50), "Bat Spin"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasBatspin = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Bat Spin");
                }
            }

            if (GUI.Button(new Rect(250, 100, 50, 50), "Slash Bash"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasSlashBash = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Slash Bash");
                }
            }

            if (GUI.Button(new Rect(300, 100, 50, 50), "Grappling Ribbon"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasGrapplingRibbon = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Grappling Ribbon");
                }
            }
            #endif
            
            // Shows whether we're currently connected to the ap server
            string apVersion = "Archipelago v" + "." + ArchipelagoClient.APVersion[0] + "." +
                               ArchipelagoClient.APVersion[1] + "." + ArchipelagoClient.APVersion[2];
            if (ArchipelagoClient.Session != null)
            {
                GUI.Label(new Rect(16, 16, 300, 20), apVersion + " Status: Connected");
            }
            else
            {
                GUI.Label(new Rect(16, 16, 300, 20), apVersion + " Status: Not Connected");
            }
            
            // If we aren't connected yet draws a text box allowing for the information to be entered
            if ((ArchipelagoClient.Session == null || !ArchipelagoClient.Authenticated) 
                && ArchipelagoClient.state == ArchipelagoClient.State.Menu)
            {
                GUI.Label(new Rect(16, 36, 150, 20), "Host: ");
                GUI.Label(new Rect(16, 56, 150, 20), "PlayerName: ");
                GUI.Label(new Rect(16, 76, 150, 20), "Password: ");

                ArchipelagoClient.ServerData.HostName = GUI.TextField(new Rect(150 + 16 + 8, 36, 150, 20),
                    ArchipelagoClient.ServerData.HostName);
                ArchipelagoClient.ServerData.SlotName = GUI.TextField(new Rect(150 + 16 + 8, 56, 150, 20),
                    ArchipelagoClient.ServerData.SlotName);
                ArchipelagoClient.ServerData.Password = GUI.TextField(new Rect(150 + 16 + 8, 76, 150, 20),
                    ArchipelagoClient.ServerData.Password);
                if (GUI.Button(new Rect(16, 96, 100, 20), "Connect"))
                {
                    ArchipelagoClient.Connect();
                }
            }
        }
    }
}
