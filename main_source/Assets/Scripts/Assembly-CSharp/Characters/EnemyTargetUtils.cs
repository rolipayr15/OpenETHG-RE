using System;
using UnityEngine;

namespace Characters
{
    // Token: 0x0200037C RID: 892
    public static class EnemyTargetUtils
    {
        // Token: 0x06001DFD RID: 7677 RVA: 0x00007FE4 File Offset: 0x000061E4
        public static EnemyTarget FindNearestTarget(Vector3 krasueP, Vector3 hostP, Vector3 guestP)
        {
        	return (krasueP - hostP).sqrMagnitude < (krasueP - guestP).sqrMagnitude ? (EnemyTarget)1 : (EnemyTarget)2;
        }
    }
}
