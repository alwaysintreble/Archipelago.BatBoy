using HarmonyLib;

using System.Collections.Generic;
using System.Reflection;

namespace Archipelago.BatBoy;

public static class AbilityMap
{
    public static readonly Dictionary<Abilities, FieldInfo> AbilityFields = new Dictionary<Abilities, FieldInfo>()
    {
        { Abilities.Batspin, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasBatspin)) },
        { Abilities.SlashBash, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasSlashBash)) },
        { Abilities.GrapplingRibbon, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasGrapplingRibbon)) },
        { Abilities.BubbleShield, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasBubbleShield)) },
        { Abilities.BullRush, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasBullRush)) },
        { Abilities.WallJump, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasWallJump)) },
        { Abilities.BouncingBasket, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasBouncingBasket)) },
        { Abilities.MegaSmash, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasMegaSmash)) },
        { Abilities.AceStomp, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasAceStomp)) },
    };
}