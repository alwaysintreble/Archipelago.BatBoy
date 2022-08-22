using System.Collections.Generic;

namespace Archipelago.BatBoy;

public class APData
{
    public long Index;
    public string HostName;
    public string SlotName;
    public string Password;
    public HashSet<long> @Checked = new HashSet<long>();
}