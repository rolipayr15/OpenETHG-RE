using System;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Mover
{
    // Token: 0x020003B8 RID: 952
    public class WitchMover : EnemyMover
    {
        // Token: 0x06001F65 RID: 8037 RVA: 0x000084F1 File Offset: 0x000066F1
        public override void SetReviveAnimation()
        {
        	anim.SetTrigger("revive");
        }

        // Token: 0x06001F66 RID: 8038 RVA: 0x000084F3 File Offset: 0x000066F3
        protected override string AgentDestinationsTag()
        {
        	return "node";
        }

        // Token: 0x06001F67 RID: 8039 RVA: 0x000084F6 File Offset: 0x000066F6
        protected override void SampleNavMeshPosition(Vector3 farestPosition, out NavMeshHit navMeshHit)
        {
        	NavMesh.SamplePosition(farestPosition, out navMeshHit, 1f, -1);
        }
    }
}
