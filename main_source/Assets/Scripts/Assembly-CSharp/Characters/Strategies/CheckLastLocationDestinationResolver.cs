using System;
using UnityEngine;

namespace Characters.Strategies
{
    // Token: 0x02000386 RID: 902
    public static class CheckLastLocationDestinationResolver
    {
        // Token: 0x06001E38 RID: 7736 RVA: 0x00010ECC File Offset: 0x0000F0CC
        public static Vector3 Resolve(bool isForcedCheck, Vector3 currentPosition, Vector3 lastKnownPosition, Vector3? forcedSnapshot = null)
        {
        	if (!isForcedCheck)
        	{
        		return lastKnownPosition;
        	}
        	return forcedSnapshot.HasValue ? forcedSnapshot.Value : currentPosition;
        }

        // Token: 0x06001E39 RID: 7737 RVA: 0x00008097 File Offset: 0x00006297
        public static string DescribeBasis(bool isForcedCheck, bool hasForcedSnapshot)
        {
        	if (!isForcedCheck)
        	{
        		return "last_known";
        	}
        	return hasForcedSnapshot ? "forced_snapshot" : "current";
        }
    }
}
