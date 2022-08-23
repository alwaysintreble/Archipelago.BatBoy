using System.Collections.Generic;

namespace Archipelago.BatBoy.ServerCommunication;

public class APData
{
    public long Index;
    public string HostName;
    public string SlotName;
    public string Password;
    public HashSet<long> @Checked = new HashSet<long>();
    public List<Ability> AcquiredAbilities = new List<Ability>();
    public int SaveSlot;
    public List<ShopSlots> ShopSlotsChecked = new List<ShopSlots>();
}