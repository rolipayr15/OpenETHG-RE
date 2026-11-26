using System;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Mover
{
    // Token: 0x020003B7 RID: 951
    public class KrasueMover : EnemyMover
    {
        // Token: 0x06001F61 RID: 8033 RVA: 0x000084DB File Offset: 0x000066DB
        public override void SetReviveAnimation()
        {
        	anim.SetTrigger("idle");
        }

        // Token: 0x06001F62 RID: 8034 RVA: 0x000084DD File Offset: 0x000066DD
        protected override string AgentDestinationsTag()
        {
        	return "node";
        }

        // Token: 0x06001F63 RID: 8035 RVA: 0x000084E0 File Offset: 0x000066E0
        protected override void SampleNavMeshPosition(Vector3 farestPosition, out NavMeshHit navMeshHit)
        {
        	NavMesh.SamplePosition(farestPosition, out navMeshHit, 1f, -1);
        }
    }
}
