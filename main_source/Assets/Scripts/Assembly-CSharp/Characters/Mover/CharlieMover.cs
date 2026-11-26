using Prime31.MessageKit;
using System;
using System.Collections;
using Characters.Agents;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Mover
{
    // Token: 0x020003AA RID: 938
    public class CharlieMover : EnemyMover
    {
        // Token: 0x06001EDE RID: 7902 RVA: 0x00008318 File Offset: 0x00006518
        protected override void Start()
        {
        	base.Start();
        	charlieAgent = GetComponent<CharlieAgent>();
        }

        // Token: 0x06001EDF RID: 7903 RVA: 0x0000831A File Offset: 0x0000651A
        public override void SetReviveAnimation()
        {
        	anim.SetTrigger("idle");
        }

        // Token: 0x06001EE0 RID: 7904 RVA: 0x0000831C File Offset: 0x0000651C
        protected override string AgentDestinationsTag()
        {
        	return "portal";
        }

        // Token: 0x06001EE1 RID: 7905 RVA: 0x0000831F File Offset: 0x0000651F
        protected override void SampleNavMeshPosition(Vector3 farestPosition, out NavMeshHit navMeshHit)
        {
        	NavMesh.SamplePosition(farestPosition, out navMeshHit, 4f, -1);
        }

        // Token: 0x06001EE2 RID: 7906 RVA: 0x00008328 File Offset: 0x00006528
        private IEnumerator CharliePortalTravel(Transform charlieT, Transform portalT, bool intoThePortal, float duration)
        {
        	Vector3 start;
        	Vector3 end;
        	if (intoThePortal)
        	{
        		start = charlieT.position;
        		end = new Vector3(portalT.position.x, charlieT.position.y, portalT.position.z);
        	}
        	else
        	{
        		Vector3 entry = portalT.gameObject.GetComponent<CharliePortal>().GetEntryPoint().position;
        		NavMesh.SamplePosition(entry, out navmeshHit, 2f, -1);
        		start = new Vector3(charlieT.position.x, navmeshHit.position.y, charlieT.position.z);
        		end = new Vector3(entry.x, navmeshHit.position.y, entry.z);
        		charlieT.position = start;
        	}
        	charlieT.LookAt(end);
        	float t = 0f;
        	while (t <= duration)
        	{
        		charlieT.position = Vector3.Lerp(start, end, t / duration);
        		float fraction = t / duration;
        		charlieAgent.SetCharlieTransparency(intoThePortal ? 1f - fraction : fraction);
        		t += Time.deltaTime;
        		yield return null;
        	}
        	charlieAgent.SetCharlieTransparency(intoThePortal ? 0f : 1f);
        }

        // Token: 0x06001EE3 RID: 7907 RVA: 0x0000832B File Offset: 0x0000652B
        public IEnumerator TravelBetweenPortals(GameObject targetWalkingPortal, GameObject targetTeleportingPortal)
        {
        	agent.enabled = false;
        	MessageKit<bool, float>.post(117, true, 3f);
        	yield return StartCoroutine(CharliePortalTravel(transform, targetWalkingPortal.transform, true, 3f));
        	transform.position = targetTeleportingPortal.transform.position;
        	MessageKit<bool, float>.post(117, false, 3f);
        	yield return StartCoroutine(CharliePortalTravel(transform, targetTeleportingPortal.transform, false, 3f));
        	agent.enabled = true;
        }

        // Token: 0x04001A63 RID: 6755
        private CharlieAgent charlieAgent;

        // Token: 0x04001A64 RID: 6756
        private NavMeshHit navmeshHit;
    }
}
