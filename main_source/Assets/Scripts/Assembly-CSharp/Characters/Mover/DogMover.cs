using Bibloteka;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Mover
{
    // Token: 0x020003AB RID: 939
    public class DogMover : EnemyMover
    {
        // Token: 0x06001EE5 RID: 7909 RVA: 0x00008336 File Offset: 0x00006536
        public override void SetReviveAnimation()
        {
        }

        // Token: 0x06001EE6 RID: 7910 RVA: 0x00008338 File Offset: 0x00006538
        protected override string AgentDestinationsTag()
        {
        	return "node";
        }

        // Token: 0x06001EE7 RID: 7911 RVA: 0x0000833B File Offset: 0x0000653B
        public void Run()
        {
        	anim.SetFloat("agent_speed", chaseSpeed);
        	agent.speed = chaseSpeed;
        	anim.SetTrigger("run");
        }

        // Token: 0x06001EE8 RID: 7912 RVA: 0x0000833D File Offset: 0x0000653D
        public void Walk()
        {
        	anim.SetFloat("agent_speed", patrolSpeed);
        	agent.speed = patrolSpeed;
        	anim.SetTrigger("walk");
        }

        // Token: 0x06001EE9 RID: 7913 RVA: 0x0000833F File Offset: 0x0000653F
        public override IEnumerator GoToDestination(Vector3 newDestination)
        {
        	yield return base.GoToDestination(newDestination);
        }

        // Token: 0x06001EEA RID: 7914 RVA: 0x00008342 File Offset: 0x00006542
        protected override void SampleNavMeshPosition(Vector3 farestPosition, out NavMeshHit navMeshHit)
        {
        	NavMesh.SamplePosition(farestPosition, out navMeshHit, 1f, -1);
        }

        // Token: 0x06001EEB RID: 7915 RVA: 0x0000834B File Offset: 0x0000654B
        public IEnumerator EatMeat(Action playEatMeatSound, Action onGrab = null)
        {
        	yield return Utils.GetDelay(0.25f);
        	onGrab?.Invoke();
        	anim.SetTrigger("eat");
        	agent.speed = 0f;
        	yield return Utils.GetDelay(1f);
        	if (UserGameSettings.isEnemyOnTheSameFloor)
        	{
        		playEatMeatSound?.Invoke();
        	}
        	yield return Utils.GetDelay(4f);
        	agent.speed = patrolSpeed;
        }
    }
}
