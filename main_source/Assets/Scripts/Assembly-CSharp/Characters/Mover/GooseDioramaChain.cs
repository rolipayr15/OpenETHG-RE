using System.Linq;
using System;
using System.Collections.Generic;

namespace Characters.Mover
{
    // Token: 0x020003B1 RID: 945
    public static class GooseDioramaChain
    {
        // Token: 0x06001F30 RID: 7984 RVA: 0x00008414 File Offset: 0x00006614
        public static string Next(string last, IReadOnlyCollection<string> takenByOthers, double rareRoll, double pick)
        {
        	if (rareRoll < RareChance && last != Rare && !takenByOthers.Contains(Rare))
        	{
        		return Rare;
        	}
        	List<string> candidates = new List<string>(Common.Length);
        	foreach (string name in Common)
        	{
        		if (name != last && !takenByOthers.Contains(name))
        		{
        			candidates.Add(name);
        		}
        	}
        	if (candidates.Count == 0)
        	{
        		foreach (string name in Common)
        		{
        			if (!takenByOthers.Contains(name))
        			{
        				candidates.Add(name);
        			}
        		}
        		if (candidates.Count == 0)
        		{
        			candidates.AddRange(Common);
        		}
        	}
        	int index = (int)(candidates.Count * pick);
        	return candidates[index >= candidates.Count ? candidates.Count - 1 : index];
        }

        // Token: 0x04001AB6 RID: 6838
        public static readonly string[] Common = new string[] { "Idle_0", "Idle_1", "Idle_2", "Idle_3", "Dazed_1" };

        // Token: 0x04001AB7 RID: 6839
        public const string Rare = "Windup_2";

        // Token: 0x04001AB8 RID: 6840
        public const double RareChance = 0.08;
    }
}
