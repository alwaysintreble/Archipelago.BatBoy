﻿using System;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;

namespace Archipelago.BatBoy.ServerCommunication;

public static class ArchipelagoClient
{
    private struct LevelLocation
    {
        private Level _level;
        private LocationType _locationType;

        public LevelLocation(Level level, LocationType locationType)
        {
            _level = level;
            _locationType = locationType;
        }
    }

    public static readonly int[] APVersion = new int[] { 0, 3, 4 };
    public static APData ServerData = new APData();
    private static readonly Dictionary<long, Item> ItemsLookup = new Dictionary<long, Item>();
    private static readonly Dictionary<long, Ability> AbilitiesLookup = new Dictionary<long, Ability>();

    private static readonly Dictionary<LevelLocation, long> LevelLocationsLookup = new Dictionary<LevelLocation, long>();
    private static readonly Dictionary<ShopSlots, long> ShopLocationsLookup = new Dictionary<ShopSlots, long>();

    private static float _unlockDequeueTimeout = 0.0f;
    private static readonly List<string> MessageQueue = new List<string>();
    private static float _messageDequeueTimeout = 0.0f;
    private static readonly bool Silent = false;
    public static bool Authenticated;

    public static ArchipelagoSession Session;

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

    public static bool Connect()
    {
        if (Authenticated)
            return true;
        
        // Start the archipelago session
        var uri = ServerData.HostName;
        int port = 38281;
        if (uri.Contains(":"))
        {
            var splits = uri.Split(new char[] { ':' });
            uri = splits[0];
            if (!int.TryParse(splits[1], out port)) port = 38281;
        }
        
        Session = ArchipelagoSessionFactory.CreateSession(uri, port);
        Session.Socket.PacketReceived += Session_PacketReceived;
        Session.Socket.ErrorReceived += Session_ErrorReceived;
        Session.Socket.SocketClosed += Session_SocketClosed;

        LoginResult loginResult = Session.TryConnectAndLogin(
            "BatBoy",
            ServerData.SlotName,
            ItemsHandlingFlags.AllItems,
            new Version(APVersion[0], APVersion[1], APVersion[2]),
            null,
            "",
            ServerData.Password == "" ? null : ServerData.Password);

        if (loginResult is LoginSuccessful loginSuccess)
        {
            Authenticated = true;
            // if (loginSuccess.SlotData.ContainsKey()){}
        }
        else if (loginResult is LoginFailure loginFailure)
        {
            APLog.LogError("Connection Error: " + String.Join("\n", loginFailure.Errors));
            Disconnect();
        }
        return loginResult.Successful;
    }

    public static void Session_SocketClosed(string reason)
    {
        MessageQueue.Add("Connection to Archipelago lost: " + reason);
        APLog.LogError("Connection to Archipelago lost: " + reason);
        Disconnect();
    }

    public static void Session_ErrorReceived(Exception e, string message)
    {
        APLog.LogError(message);
        if (e != null) APLog.LogError(e.ToString());
        Disconnect();
    }

    public static void Disconnect()
    {
        if (Session != null && Session.Socket != null)
            Session.Socket.Disconnect();
        Session = null;
        Authenticated = false;
    }

    public static void Session_PacketReceived(ArchipelagoPacketBase packet)
    {
        APLog.LogDebug("Incoming Packet: " + packet.PacketType);
        switch (packet.PacketType)
        {
            case ArchipelagoPacketType.Print:
                if (!Silent)
                {
                    var p = packet as PrintPacket;
                    MessageQueue.Add(p.Text);
                    APLog.LogInfo(p.Text);
                }

                break;
            case ArchipelagoPacketType.PrintJSON:
                if (!Silent)
                {
                    var p = packet as PrintJsonPacket;
                    string text = "";
                    foreach (var messagePart in p.Data)
                    {
                        switch (messagePart.Type)
                        {
                            case JsonMessagePartType.PlayerId:
                                text += int.TryParse(messagePart.Text, out var PlayerSlot)
                                    ? Session.Players.GetPlayerAlias(PlayerSlot) ?? $"Slot: {PlayerSlot}"
                                    : messagePart.Text;
                                break;
                            case JsonMessagePartType.ItemId:
                                text += int.TryParse(messagePart.Text, out var itemID)
                                    ? Session.Items.GetItemName(itemID) ?? $"Item: {itemID}" : messagePart.Text;
                                break;
                            case JsonMessagePartType.LocationId:
                                text += int.TryParse(messagePart.Text, out var locationID)
                                    ? Session.Locations.GetLocationNameFromId(locationID) ?? $"Location: {locationID}"
                                    : messagePart.Text;
                                break;
                            default:
                                text += messagePart.Text;
                                break;
                        }
                    }

                    MessageQueue.Add(text);
                    APLog.LogInfo(text);
                }

                break;
        }
    }

    public static void DequeueUnlocks()
    {
        const int DequeueCount = 2;
        const float DequeueTime = 3.0f;

        if (_unlockDequeueTimeout > 0.0f) _unlockDequeueTimeout -= Time.deltaTime;
        if (_messageDequeueTimeout > 0.0f) _messageDequeueTimeout -= Time.deltaTime;

        if (_messageDequeueTimeout <= 0.0f)
        {
            // queue these up so the screen doesn't get crowded
            List<string> ToProcess = new List<string>();
            while (ToProcess.Count < DequeueCount && MessageQueue.Count > 0)
            {
                ToProcess.Add(MessageQueue[0]);
                MessageQueue.RemoveAt(0);
            }

            foreach (var message in ToProcess)
            {
                APLog.LogInfo(message);
            }

            _messageDequeueTimeout = DequeueTime;
        }
        
        // unlock items
        if (ServerData.Index < Session.Items.AllItemsReceived.Count)
        {
            long currentItemID = Session.Items.AllItemsReceived[Convert.ToInt32(ServerData.Index)].Item;
            if (ItemsLookup.ContainsKey(currentItemID))
            {
                Unlock(ItemsLookup[currentItemID]);
            }
            else
            {
                Unlock(AbilitiesLookup[currentItemID]);
            }
            ++ServerData.Index;
            _unlockDequeueTimeout = DequeueTime;
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
        ServerData.AcquiredAbilities.Add(unlockAbility);
        SaveManager.Save();
    }

    public static void SendAsyncChecks()
    {
        BatBoySlot saveSlot = SaveManager.Savegame.GetCurrentSlot();
        foreach (int i in Enum.GetValues(typeof(Level)))
        {
            foreach (LocationType j in Enum.GetValues(typeof(LocationType)))
            {
                if (ServerData.Checked.Contains(LevelLocationsLookup[new LevelLocation((Level)i, j)])) 
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
        foreach (ShopSlots i in ServerData.ShopSlotsChecked)
        {
            if (!ServerData.Checked.Contains(ShopLocationsLookup[i]))
                CheckLocation(i);
        }
    }

    public static void CheckLocation(Level level, LocationType locationType)
    {
        long checkID = LevelLocationsLookup[new LevelLocation(level, locationType)];
        Session.Locations.CompleteLocationChecks(checkID);
        ServerData.Checked.Add(checkID);
    }

    public static void CheckLocation(ShopSlots slot)
    {
        while (ServerData.ShopSlotsChecked.Contains(slot))
            ++slot;
        ServerData.ShopSlotsChecked.Add(slot);
        long checkID = ShopLocationsLookup[slot];
        Session.Locations.CompleteLocationChecks(checkID);
        ServerData.Checked.Add(checkID);
    }
    
}