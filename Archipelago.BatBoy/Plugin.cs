using System.Collections;
using System.Reflection;
using Archipelago.BatBoy.DataHandling;

using BepInEx;
using UnityEngine;

using Archipelago.BatBoy.ServerCommunication;
using Archipelago.BatBoy.Constants;
using HarmonyLib;

namespace Archipelago.BatBoy
{
    [BepInPlugin(PluginGUID, PluginName, PluginVersion)]
    public class Plugin : BaseUnityPlugin
    {
        public const string PluginGUID = "com.alwaysintreble.Archipelago.BatBoy";
        public const string PluginAuthor = "alwaysintreble";
        public const string PluginName = "Archipelago";
        public const string PluginVersion = "0.1.6";
        
        
        private readonly ArchipelagoItemsController _locationsHandler = new();
        private readonly ShopHandler _shopHandler = new();

        public BatBoySlot saveSlot;

        private FieldInfo _highlightedSlot;

        private void Awake()
        {
            APLog.Init(Logger);
            APLog.LogInfo("Hello World!");
            
            LocationsAndItemsHelper.Init();
            
            // This only gets called on clearing levels but is the easiest way to handle changing the ability states.
            // Investigate other methods but unlikely.
            On.BatBoySaveGame.SaveLevelClear += _locationsHandler.SaveLevelClear;
            
            On.UIShop.CommitTransaction += _shopHandler.OnTransaction;

            On.TitleScreen.StartGame += OnGameStart;
            On.UIShop.ShowPopup += OnShopPopup;
            On.SaveManager.Save += ArchipelagoClient.ServerData.SaveAPInfo;
            On.TitleScreen.Exit += OnGameClose;
            On.PlayerController.Die += DeathLinkInterface.SendDeathLink;
            On.Collectable.Collect += ArchipelagoItemsController.OnCollect;

        }

        private void Update()
        {
            if (saveSlot != null && ArchipelagoClient.Authenticated)
                ArchipelagoClient.DequeueUnlocks();
        }


        private void OnGameStart(On.TitleScreen.orig_StartGame orig, TitleScreen self)
        {
            _highlightedSlot = AccessTools.Field(typeof(TitleScreen), "hightlightedSlot");
            var slotNumber = (int)_highlightedSlot.GetValue(self);
            saveSlot = SaveManager.Savegame.Slots[slotNumber];
            // game only starts if we're connected to a server first
            if (ArchipelagoClient.Authenticated)
            {
                orig(self);
                APLog.LogInfo("Save loaded successfully");
            }
            else if (SaveManager.Savegame.Slots[slotNumber].Health != 0)
            {
                ArchipelagoClient.ServerData.LoadAPInfo(slotNumber);
                APLog.LogInfo("Loading save...");
                ArchipelagoClient.Connect();
                if (ArchipelagoClient.Authenticated)
                {
                    orig(self);
                    // APLog.LogInfo("Sending checks...");
                    // LocationsAndItemsHelper.SendAsyncChecks(); // can't figure out why this doesn't work
                }
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
                Cursor.visible = false;
                GUI.Label(new Rect(16, 16, 300, 20), apVersion + " Status: Connected");
            }
            else // show we aren't connected and a text box allowing the info to be entered
            {
                Cursor.visible = true;
                
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
            if (GUI.Button(new Rect(0, 120, 75, 50), "Red Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.RedSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated {Item.RedSeed}");
                }
            }

            if (GUI.Button(new Rect(75, 120, 75, 50), "Green Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.GreenSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated {Item.GreenSeed}");
                }
            }
            
            if (GUI.Button(new Rect(150, 120, 75, 50), "Golden Seeds"))
            {
                if (saveSlot != null)
                {
                    ++saveSlot.GoldenSeeds;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated {Item.GoldenSeed}");
                }
            }

            if (GUI.Button(new Rect(225, 120, 75, 50), "100 Crystals"))
            {
                if (saveSlot != null)
                {
                    saveSlot.Crystals += 100;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated 100 Crystals");
                }
            }

            if (GUI.Button(new Rect(0, 170, 75, 50), "Bat Spin"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasBatspin = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Bat Spin");
                }
            }

            if (GUI.Button(new Rect(75, 170, 75, 50), "Slash Bash"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasSlashBash = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Slash Bash");
                }
            }

            if (GUI.Button(new Rect(150, 170, 75, 50), "Grappling Ribbon"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasGrapplingRibbon = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Grappling Ribbon");
                }
            }

            if (GUI.Button(new Rect(225, 170, 75, 50), "He Dead"))
            {
                if (saveSlot != null)
                {
                    PlayerController.Die();
                }
            }
            
            if (GUI.Button(new Rect(0, 220, 75, 50), "Bubble Shield"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasBubbleShield = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Bubble Shield");
                }
            }
            
            if (GUI.Button(new Rect(75, 220, 75, 50), "Bull Rush"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasBullRush = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Bull Rush");
                }
            }
            
            if (GUI.Button(new Rect(150,220, 75, 50), "Wall Jump"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasWallJump = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Wall Jump");
                }
            }
            
            if (GUI.Button(new Rect(225, 220, 75, 50), "Bouncing Basket"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasBouncingBasket = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Bouncing Basket");
                }
            }
            
            if (GUI.Button(new Rect(0, 270, 75, 50), "Mega Smash"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasMegaSmash = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Mega Smash");
                }
            }
            
            if (GUI.Button(new Rect(75, 270, 75, 50), "Ace Stomp"))
            {
                if (saveSlot != null)
                {
                    saveSlot.HasAceStomp = true;
                    SaveManager.Save();
                    APLog.LogInfo($"Cheated Ace Stomp");
                }
            }
#endif
        }
    }
}
