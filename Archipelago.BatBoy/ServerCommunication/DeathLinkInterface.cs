using Archipelago.MultiClient.Net.BounceFeatures.DeathLink;
using UnityEngine;

namespace Archipelago.BatBoy.ServerCommunication;

public class DeathLinkInterface
{
    public static DeathLinkService DeathLinkService;
    private static bool _deathLinkKilling;

    public DeathLinkInterface()
    {
        APLog.LogInfo("Initializing death link service...");
        DeathLinkService = ArchipelagoClient.Session.CreateDeathLinkService();
        DeathLinkService.OnDeathLinkReceived += DeathLinkReceived;
        APLog.LogInfo("Death Link status:");
        APLog.LogInfo(ArchipelagoClient.ServerData.DeathLink);
        if (ArchipelagoClient.ServerData.DeathLink)
            DeathLinkService.EnableDeathLink();
        else
            DeathLinkService.DisableDeathLink();
    }

    public void DeathLinkReceived(DeathLink deathLink)
    {
        if (!(bool)(Object)PlayerController.Instance)
            return;
        _deathLinkKilling = true;
        APLog.LogInfo("Received Death Link");
        APLog.LogInfo(deathLink.Cause);
        PlayerController.Die();
    }

    public static void SendDeathLink(On.PlayerController.orig_Die orig)
    {
        if (!_deathLinkKilling)
        {
            if (ArchipelagoClient.ServerData.DeathLink)
            {
                APLog.LogInfo("Killing your friends...");
                DeathLinkService.SendDeathLink(new DeathLink(ArchipelagoClient.ServerData.SlotName));
            }
        }

        _deathLinkKilling = false;
        orig();
    }
}