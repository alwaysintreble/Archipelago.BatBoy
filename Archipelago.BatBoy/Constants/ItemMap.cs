using HarmonyLib;

using System.Collections.Generic;
using System.Reflection;

namespace Archipelago.BatBoy.Constants;

public static class AbilityMap
{
    public static readonly Dictionary<Ability, FieldInfo> AbilityFields = new Dictionary<Ability, FieldInfo>()
    {
        { Ability.Batspin, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasBatspin)) },
        { Ability.SlashBash, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasSlashBash)) },
        { Ability.GrapplingRibbon, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasGrapplingRibbon)) },
        { Ability.BubbleShield, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasBubbleShield)) },
        { Ability.BullRush, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasBullRush)) },
        { Ability.WallJump, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasWallJump)) },
        { Ability.BouncingBasket, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasBouncingBasket)) },
        { Ability.MegaSmash, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasMegaSmash)) },
        { Ability.AceStomp, AccessTools.Field(typeof(BatBoySlot), nameof(BatBoySlot.HasAceStomp)) },
    };
}