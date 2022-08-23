using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using Archipelago.BatBoy.ServerCommunication;
using BepInEx;
using HarmonyLib;
using UnityEngine;

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

        private ArchipelagoClient _AP;
        
        private readonly ArchipelagoItemsController _locationsHandler = new ArchipelagoItemsController();
        private readonly ShopHandler _shopHandler = new ShopHandler();
        private readonly FieldInfo _newGame = AccessTools.Field(typeof(TitleScreen), "NewGame");

        public List<Ability> acquiredAbilities = new List<Ability>();
        
        public BatBoySlot saveSlot;
        
        private void Awake()
        {
            APLog.Init(Logger);
            APLog.LogInfo("Hello World!");
            
            _AP = new ArchipelagoClient();
            //_AP.OnClientDisconnect += AP_OnClientDisconnect;
            
            // This only gets called on clearing levels but is the easiest way to handle collecting the seeds and
            // changing the ability states. Investigate other methods but unlikely.
            On.BatBoySaveGame.SaveLevelClear += _locationsHandler.SaveLevelClear;
            
            On.UIShop.CommitTransaction += OnTransaction;

            On.TitleScreen.StartGame += OnGameStart;
            On.UIShop.ShowPopup += OnShopPopup;
        }

        private void OnTransaction(On.UIShop.orig_CommitTransaction orig, UIShop self)
        {
            ShopItem purchaseItem = _shopHandler.GetCurrentShopItem(self);
            if (ShopHandler.TransactionIsDone(purchaseItem, SaveManager.Savegame.GetCurrentSlot()))
            {
                ArchipelagoItemsController.SendShopLocation(purchaseItem);
            }

            orig(self);
        }

        private void OnGameStart(On.TitleScreen.orig_StartGame orig, TitleScreen self, int number)
        {
            // on a new game or if connected before starting up the game, save the connection info
            if (SaveManager.Savegame.Slots[number].Health == 0 && !ArchipelagoClient.Authenticated)
            {
                
            }
            else // attempt to connect from saved information if it exists
            {
                
            }

            // don't load into the game unless we're connected first
            if (!ArchipelagoClient.Authenticated)
            {
                orig(self, number);
                saveSlot = SaveManager.Savegame.GetCurrentSlot();
                APLog.LogInfo("Save loaded successfully");
            }
        }

        private IEnumerator OnShopPopup(On.UIShop.orig_ShowPopup orig, UIShop self, string key)
        {
            yield return null;
        }
        
        private void OnGUI()
        {
            #if DEBUG
            // Debug buttons to grant items and abilities :)
            if (GUI.Button(new Rect(0, 10, 50, 50), "Red Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.RedSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Received {Item.RedSeed}");
                }
            }

            if (GUI.Button(new Rect(50, 10, 50, 50), "Green Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.GreenSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Received {Item.GreenSeed}");
                }
            }
            
            if (GUI.Button(new Rect(100, 10, 50, 50), "Golden Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.GoldenSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Received {Item.GoldenSeed}");
                }
            }

            if (GUI.Button(new Rect(150, 10, 50, 50), "100 Crystals"))
            {
                if (saveSlot != null)
                {
                    saveSlot.Crystals += 100;
                    SaveManager.Save();
                    APLog.LogInfo($"Received 100 Crystals");
                }
            }

            if (GUI.Button(new Rect(200, 10, 50, 50), "Bat Spin"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasBatspin = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Received Bat Spin");
                }
            }

            if (GUI.Button(new Rect(250, 10, 50, 50), "Slash Bash"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasSlashBash = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Received Slash Bash");
                }
            }

            if (GUI.Button(new Rect(300, 10, 50, 50), "Grappling Ribbon"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasGrapplingRibbon = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Received Grappling Ribbon");
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
            if ((ArchipelagoClient.Session == null || !ArchipelagoClient.Authenticated) && ArchipelagoClient.state == ArchipelagoClient.State.Menu)
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
