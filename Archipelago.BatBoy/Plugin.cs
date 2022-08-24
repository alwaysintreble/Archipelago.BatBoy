using System.Collections;
using Archipelago.BatBoy.DataHandling;

using BepInEx;
using UnityEngine;

using Archipelago.BatBoy.ServerCommunication;

namespace Archipelago.BatBoy
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.alwaysintreble.Archipelago.BatBoy";
        public const string PluginAuthor = "alwaysintreble";
        public const string PluginName = "Archipelago";
        public const string PluginVersion = "0.1.1";
        
        
        private readonly ArchipelagoItemsController _locationsHandler = new();
        private readonly ShopHandler _shopHandler = new();

        public BatBoySlot saveSlot;

        private void Awake()
        {
            APLog.Init(Logger);
            APLog.LogInfo("Hello World!");
            
            LocationsAndItemsHelper.Init();
            
            // This only gets called on clearing levels but is the easiest way to handle collecting the seeds and
            // changing the ability states. Investigate other methods but unlikely.
            On.BatBoySaveGame.SaveLevelClear += _locationsHandler.SaveLevelClear;
            
            On.UIShop.CommitTransaction += _shopHandler.OnTransaction;

            On.TitleScreen.StartGame += OnGameStart;
            On.UIShop.ShowPopup += OnShopPopup;
            On.SaveManager.Save += ArchipelagoClient.ServerData.SaveAPInfo;
            On.TitleScreen.Exit += OnGameClose;
            // On.PlayerController.PlayerDie // lmao they made this so easy

        }

        private void Update()
        {
            if (saveSlot != null && ArchipelagoClient.Authenticated)
                ArchipelagoClient.DequeueUnlocks();
        }


        private void OnGameStart(On.TitleScreen.orig_StartGame orig, TitleScreen self, int number)
        {
            if (SaveManager.Savegame.Slots[number].Health != 0)
            {
                ArchipelagoClient.ServerData.LoadAPInfo(number);
                LocationsAndItemsHelper.SendAsyncChecks();
            }

            if (ArchipelagoClient.Authenticated)
            {
                orig(self, number);
                saveSlot = SaveManager.Savegame.GetCurrentSlot();
                APLog.LogInfo("Save loaded successfully");
            }
            else
            {
                ArchipelagoClient.Connect();
            }
        }

        private void OnGameClose(On.TitleScreen.orig_Exit orig, TitleScreen title)
        {
            APLog.LogInfo("Disconnecting...");
            ArchipelagoClient.Disconnect();
            orig(title);
        }

        private IEnumerator OnShopPopup(On.UIShop.orig_ShowPopup orig, UIShop self, string key)
        {
            yield return null;
        }
        
        private void OnGUI()
        {
            // Shows whether we're currently connected to the ap server
            string apVersion = "Archipelago v" + "." + ArchipelagoClient.APVersion[0] + "." +
                               ArchipelagoClient.APVersion[1] + "." + ArchipelagoClient.APVersion[2];
            if (ArchipelagoClient.Authenticated)
            {
                GUI.Label(new Rect(16, 16, 300, 20), apVersion + " Status: Connected");
            }
            else // show we aren't connected and a text box allowing the info to be entered
            {
                GUI.Label(new Rect(16, 16, 300, 20), apVersion + " Status: Not Connected");
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

            if (GUI.Button(new Rect(350, 110, 50, 50), "He Dead"))
            {
                if (saveSlot != null)
                {
                    //send deathlink function
                }
            }
#endif
        }
    }
}
