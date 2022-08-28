using System;
using System.Collections.Generic;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Packets;
using UnityEngine;

namespace Archipelago.BatBoy.ServerCommunication;

public static class ArchipelagoClient
{

    public static readonly int[] APVersion = { 0, 3, 4 };
    public static APData ServerData = new();

    private static float _unlockDequeueTimeout;
    private static readonly List<string> MessageQueue = new();
    private static float _messageDequeueTimeout;
    private static readonly bool Silent = false;
    public static bool Authenticated;

    public static ArchipelagoSession Session;
    public static DeathLinkInterface DeathLinkService;

    public static bool Connect()
    {
        if (Authenticated)
            return true;
        
        // Start the archipelago session
        var uri = ServerData.HostName;
        int port = 38281;
        if (uri.Contains(":"))
        {
            var splits = uri.Split(':');
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
        
        APLog.LogInfo(loginResult);
        if (loginResult is LoginSuccessful loginSuccess)
        {
            APLog.LogInfo("Authenticating...");
            Authenticated = true;
            if (loginSuccess.SlotData.TryGetValue("deathlink", out var deathLink))
            {
                ServerData.DeathLink = (bool)deathLink;
                APLog.LogInfo("Death Link status from server:");
                APLog.LogInfo(ServerData.DeathLink);
            }
            APLog.LogInfo("Setting up death link...");
            DeathLinkService = new DeathLinkInterface();
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
                }

                break;
        }
    }

    public static void DequeueUnlocks()
    {
        const int dequeueCount = 2;
        const float dequeueTime = 3.0f;

        if (_unlockDequeueTimeout > 0.0f) _unlockDequeueTimeout -= Time.deltaTime;
        if (_messageDequeueTimeout > 0.0f) _messageDequeueTimeout -= Time.deltaTime;

        if (_messageDequeueTimeout <= 0.0f)
        {
            // queue these up so the screen doesn't get crowded
            List<string> toProcess = new List<string>();
            while (toProcess.Count < dequeueCount && MessageQueue.Count > 0)
            {
                toProcess.Add(MessageQueue[0]);
                MessageQueue.RemoveAt(0);
            }

            foreach (var message in toProcess)
            {
                APLog.LogInfo(message);
            }

            _messageDequeueTimeout = dequeueTime;
        }
        
        // unlock items
        if (ServerData.Index < Session.Items.AllItemsReceived.Count)
        {
            long currentItemID = Session.Items.AllItemsReceived[Convert.ToInt32(ServerData.Index)].Item;
            if (LocationsAndItemsHelper.ItemsLookup.ContainsKey(currentItemID))
            {
                LocationsAndItemsHelper.Unlock(LocationsAndItemsHelper.ItemsLookup[currentItemID]);
            }
            else
            {
                LocationsAndItemsHelper.Unlock(LocationsAndItemsHelper.AbilitiesLookup[currentItemID]);
            }
            ++ServerData.Index;
            _unlockDequeueTimeout = dequeueTime;
        }
        
    }
}