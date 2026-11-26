using Characters.Strategies.Goose;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Mover
{
    // Token: 0x020003B3 RID: 947
    public class GooseMover : EnemyMover
    {
        // Token: 0x06001F39 RID: 7993 RVA: 0x0000843B File Offset: 0x0000663B
        protected override void Start()
        {
        	base.Start();
        	patrolAcceleration = agent.acceleration;
        	patrolAngularSpeed = agent.angularSpeed;
        	chaseSpeedAtStart = chaseSpeed;
        }

        // Token: 0x06001F3A RID: 7994 RVA: 0x0000843D File Offset: 0x0000663D
        public override void SetReviveAnimation()
        {
        	foreach (string trigger in GooseAnimatorDriver.Triggers)
        	{
        		anim.ResetTrigger(trigger);
        	}
        	anim.Play(GooseClips.StateName("Walk", 0), 0, 0f);
        }

        // Token: 0x06001F3B RID: 7995 RVA: 0x0000843F File Offset: 0x0000663F
        protected override string AgentDestinationsTag()
        {
        	return "node";
        }

        // Token: 0x06001F3C RID: 7996 RVA: 0x00008442 File Offset: 0x00006642
        protected override void SampleNavMeshPosition(Vector3 farestPosition, out NavMeshHit navMeshHit)
        {
        	NavMesh.SamplePosition(farestPosition, out navMeshHit, 1f, -1);
        }

        // Token: 0x06001F3D RID: 7997 RVA: 0x0000844B File Offset: 0x0000664B
        public float ChargeSpeedWithRamp(float profileChargeSpeed)
        {
        	return Mathf.Max(0f, chaseSpeed - chaseSpeedAtStart) + profileChargeSpeed;
        }

        // Token: 0x06001F3E RID: 7998 RVA: 0x00008452 File Offset: 0x00006652
        public void Halt()
        {
        	agent.velocity = Vector3.zero;
        	agent.speed = 0f;
        	if (agent.enabled && agent.isOnNavMesh)
        	{
        		agent.ResetPath();
        	}
        }

        // Token: 0x1700034A RID: 842
        // (get) Token: 0x06001F3F RID: 7999 RVA: 0x00008454 File Offset: 0x00006654
        public bool HasMovementRequest
        {
            get
            {
                return agent.enabled && agent.isOnNavMesh && (agent.hasPath || agent.pathPending || agent.speed > 0f);
            }
        }

        // Token: 0x06001F40 RID: 8000 RVA: 0x00008457 File Offset: 0x00006657
        public void FaceTowards(Vector3 worldPosition)
        {
        	Vector3 direction = worldPosition - transform.position;
        	direction.y = 0f;
        	if (direction.x * direction.x + direction.z * direction.z > 0.0001f)
        	{
        		transform.rotation = Quaternion.LookRotation(direction, Vector3.up);
        	}
        }

        // Token: 0x06001F41 RID: 8001 RVA: 0x00008459 File Offset: 0x00006659
        public IEnumerator BeginCharge(Vector3 destination, float chargeSpeed)
        {
        	yield return GoToDestination(destination);
        	agent.speed = chargeSpeed;
        	agent.acceleration = chargeAcceleration;
        	agent.angularSpeed = chargeAngularSpeed;
        	agent.autoBraking = false;
        }

        // Token: 0x06001F42 RID: 8002 RVA: 0x0000845C File Offset: 0x0000665C
        public void EndCharge()
        {
        	agent.speed = patrolSpeed;
        	agent.acceleration = patrolAcceleration;
        	agent.angularSpeed = patrolAngularSpeed;
        	agent.autoBraking = true;
        	if (agent.enabled && agent.isOnNavMesh)
        	{
        		agent.ResetPath();
        	}
        }

        // Token: 0x1700034B RID: 843
        // (get) Token: 0x06001F43 RID: 8003 RVA: 0x0000845E File Offset: 0x0000665E
        public bool PathPending
        {
            get
            {
                return agent.enabled && agent.isOnNavMesh && agent.pathPending;
            }
        }

        // Token: 0x1700034C RID: 844
        // (get) Token: 0x06001F44 RID: 8004 RVA: 0x00008461 File Offset: 0x00006661
        public bool HasCompletePath
        {
            get
            {
                return agent.enabled && agent.isOnNavMesh && agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete;
            }
        }

        // Token: 0x1700034D RID: 845
        // (get) Token: 0x06001F45 RID: 8005 RVA: 0x00008464 File Offset: 0x00006664
        public float RemainingDistance
        {
            get
            {
                return HasCompletePath ? agent.remainingDistance : float.PositiveInfinity;
            }
        }

        // Token: 0x1700034E RID: 846
        // (get) Token: 0x06001F46 RID: 8006 RVA: 0x0000846B File Offset: 0x0000666B
        public float StopDistance
        {
            get
            {
                return agent.stoppingDistance;
            }
        }

        // Token: 0x06001F47 RID: 8007 RVA: 0x00008472 File Offset: 0x00006672
        public float CurrentPathLength()
        {
        	if (agent.enabled && agent.isOnNavMesh && agent.hasPath)
        	{
        		return GooseChargeRules.PathLength(agent.path.corners);
        	}
        	return 0f;
        }

        // Token: 0x06001F48 RID: 8008 RVA: 0x00008479 File Offset: 0x00006679
        public void TriggerWalk()
        {
        	GooseAnimatorDriver.Walk(anim);
        }

        // Token: 0x06001F49 RID: 8009 RVA: 0x0000847B File Offset: 0x0000667B
        public void TriggerWindup()
        {
        	GooseAnimatorDriver.Windup(anim);
        }

        // Token: 0x06001F4A RID: 8010 RVA: 0x0000847D File Offset: 0x0000667D
        public void TriggerCharge()
        {
        	GooseAnimatorDriver.Charge(anim);
        }

        // Token: 0x06001F4B RID: 8011 RVA: 0x0000847F File Offset: 0x0000667F
        public void TriggerLand()
        {
        	GooseAnimatorDriver.Land(anim);
        }

        // Token: 0x04001AC0 RID: 6848
        [Header("Charge profile")]
        [SerializeField]
        private float chargeAcceleration = 60f;

        // Token: 0x04001AC1 RID: 6849
        [SerializeField]
        private float chargeAngularSpeed = 8000f;

        // Token: 0x04001AC2 RID: 6850
        private float patrolAcceleration;

        // Token: 0x04001AC3 RID: 6851
        private float patrolAngularSpeed;

        // Token: 0x04001AC4 RID: 6852
        private float chaseSpeedAtStart;
    }
}
