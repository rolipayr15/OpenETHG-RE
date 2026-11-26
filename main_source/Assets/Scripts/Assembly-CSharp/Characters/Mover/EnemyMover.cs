using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Mover
{
    // Token: 0x020003AC RID: 940
    public abstract class EnemyMover : MonoBehaviour
    {
        // Token: 0x1700033D RID: 829
        // (get) Token: 0x06001EEE RID: 7918 RVA: 0x00008359 File Offset: 0x00006559
        public bool IsDoorDetected
        {
            get
            {
                return isDoorDetected;
            }
        }

        // Token: 0x06001EEF RID: 7919 RVA: 0x0000835C File Offset: 0x0000655C
        public void ClearDoorDetected()
        {
        	isDoorDetected = false;
        }

        // Token: 0x1700033E RID: 830
        // (get) Token: 0x06001EF0 RID: 7920 RVA: 0x0000835E File Offset: 0x0000655E
        internal float DiagnosticCurrentSpeed
        {
            get
            {
                return agent != null ? agent.speed : 0f;
            }
        }

        // Token: 0x1700033F RID: 831
        // (get) Token: 0x06001EF1 RID: 7921 RVA: 0x00008365 File Offset: 0x00006565
        internal float DiagnosticPatrolSpeed
        {
            get
            {
                return patrolSpeed;
            }
        }

        // Token: 0x17000340 RID: 832
        // (get) Token: 0x06001EF2 RID: 7922 RVA: 0x0000836C File Offset: 0x0000656C
        internal float DiagnosticChaseSpeed
        {
            get
            {
                return chaseSpeed;
            }
        }

        // Token: 0x17000341 RID: 833
        // (get) Token: 0x06001EF3 RID: 7923 RVA: 0x00008373 File Offset: 0x00006573
        internal bool DiagnosticHasPath
        {
            get
            {
                return agent != null && agent.enabled && agent.isOnNavMesh && agent.hasPath;
            }
        }

        // Token: 0x17000342 RID: 834
        // (get) Token: 0x06001EF4 RID: 7924 RVA: 0x00008376 File Offset: 0x00006576
        internal bool DiagnosticPathPending
        {
            get
            {
                return agent != null && agent.enabled && agent.isOnNavMesh && agent.pathPending;
            }
        }

        // Token: 0x17000343 RID: 835
        // (get) Token: 0x06001EF5 RID: 7925 RVA: 0x00008379 File Offset: 0x00006579
        internal string DiagnosticPathStatus
        {
            get
            {
                return agent != null && agent.enabled && agent.isOnNavMesh ? agent.pathStatus.ToString() : "<unavailable>";
            }
        }

        // Token: 0x17000344 RID: 836
        // (get) Token: 0x06001EF6 RID: 7926 RVA: 0x0000837C File Offset: 0x0000657C
        internal float DiagnosticRemainingDistance
        {
            get
            {
                return DiagnosticHasPath ? agent.remainingDistance : -1f;
            }
        }

        // Token: 0x17000345 RID: 837
        // (get) Token: 0x06001EF7 RID: 7927 RVA: 0x00010F74 File Offset: 0x0000F174
        internal Vector3 DiagnosticDestination
        {
            get
            {
                return agent != null && agent.enabled && agent.isOnNavMesh ? agent.destination : transform.position;
            }
        }

        // Token: 0x17000346 RID: 838
        // (get) Token: 0x06001EF8 RID: 7928 RVA: 0x00008383 File Offset: 0x00006583
        public GameObject[] AgentDestinations
        {
            get
            {
                return agentDestinations;
            }
        }

        // Token: 0x06001EF9 RID: 7929 RVA: 0x00008386 File Offset: 0x00006586
        protected virtual void Awake()
        {
        	GameObject[] starts = GameObject.FindGameObjectsWithTag("StartingPosition");
        	agent.Warp(starts[UnityEngine.Random.Range(0, starts.Length)].transform.position);
        	MessageKit<string>.addObserver(30, DoorDetected);
        	SetParametersDependentOnDifficultyLevel();
        	FindAndSetAgentDestinations();
        	MessageKit<bool>.addObserver(107, FreezeEnemyWhenWaitingForSecondPlayer);
        }

        // Token: 0x06001EFA RID: 7930 RVA: 0x00008388 File Offset: 0x00006588
        protected virtual void Start()
        {
        	agent.stoppingDistance = targetReachedDistance;
        	MessageKit.addObserver(25, StartSpeedIncreasing);
        }

        // Token: 0x06001EFB RID: 7931 RVA: 0x0000838A File Offset: 0x0000658A
        protected virtual void OnDestroy()
        {
        	MessageKit<string>.removeObserver(30, DoorDetected);
        	MessageKit<bool>.removeObserver(107, FreezeEnemyWhenWaitingForSecondPlayer);
        	MessageKit.removeObserver(25, StartSpeedIncreasing);
        }

        // Token: 0x06001EFC RID: 7932
        protected abstract string AgentDestinationsTag();

        // Token: 0x06001EFD RID: 7933 RVA: 0x0000838C File Offset: 0x0000658C
        protected void FindAndSetAgentDestinations()
        {
        	agentDestinations = GameObject.FindGameObjectsWithTag(AgentDestinationsTag());
        }

        // Token: 0x06001EFE RID: 7934 RVA: 0x00010F8C File Offset: 0x0000F18C
        protected Vector3 GetNewRandomDestination()
        {
        	return agentDestinations[UnityEngine.Random.Range(0, agentDestinations.Length)].transform.position;
        }

        // Token: 0x06001EFF RID: 7935 RVA: 0x0000838E File Offset: 0x0000658E
        public IEnumerator GoToNewRandomDestination()
        {
        	yield return GoToDestination(GetNewRandomDestination());
        }

        // Token: 0x06001F00 RID: 7936 RVA: 0x00008391 File Offset: 0x00006591
        public bool IsTargetReached()
        {
        	return !agent.pathPending && agent.remainingDistance < targetReachedDistance;
        }

        // Token: 0x06001F01 RID: 7937 RVA: 0x00008394 File Offset: 0x00006594
        public virtual IEnumerator GoToDestination(Vector3 newDestination)
        {
        	isDoorDetected = false;
        	float agentSpeed = agent.speed == 0f ? patrolSpeed : agent.speed;
        	agent.speed = 0f;
        	Vector3 destination = newDestination;
        	if ((int)UserGameSettings.currentLevel == 3)
        	{
        		if (NavMesh.SamplePosition(newDestination, out NavMeshHit hit, 3.5f, -1))
        		{
        			destination = hit.position;
        		}
        		else
        		{
        			Debug.LogError("Custom sampling SamplePosition failed; radius too small?");
        		}
        	}
        	agent.SetDestination(destination);
        	yield return WaitForPathComputedCoroutine();
        	agent.speed = agentSpeed;
        }

        // Token: 0x06001F02 RID: 7938 RVA: 0x00008397 File Offset: 0x00006597
        public void AttackMove(Vector3 targetTransform)
        {
        	agent.updateRotation = true;
        	agent.transform.LookAt(CustomSingleton<PlayerManager>.Instance.Player.transform.position);
        	if (agent.isOnNavMesh)
        	{
        		agent.isStopped = true;
        	}
        	agent.enabled = false;
        	if (anim != null)
        	{
        		anim.SetTrigger("attack");
        	}
        }

        // Token: 0x06001F03 RID: 7939
        protected abstract void SampleNavMeshPosition(Vector3 farestPosition, out NavMeshHit navMeshHit);

        // Token: 0x06001F04 RID: 7940 RVA: 0x00008399 File Offset: 0x00006599
        public void MoveAgentAwayFromPlayer()
        {
        	Vector3 playerPosition = CustomSingleton<PlayerManager>.Instance.Player.transform.position;
        	Vector3 farthest = Vector3.zero;
        	float farthestDistance = 0f;
        	foreach (GameObject destination in agentDestinations)
        	{
        		Vector3 position = destination.transform.position;
        		if (CustomSingleton<FloorManager>.Instance.GetFloor(position) != CustomSingleton<FloorManager>.Instance.GetFloor(playerPosition))
        		{
        			float distance = (playerPosition - position).sqrMagnitude;
        			if (farthestDistance < distance)
        			{
        				farthest = position;
        				farthestDistance = distance;
        			}
        		}
        	}
        	SampleNavMeshPosition(farthest, out NavMeshHit navMeshHit);
        	transform.position = navMeshHit.position;
        	agent.enabled = true;
        }

        // Token: 0x06001F05 RID: 7941 RVA: 0x0000839B File Offset: 0x0000659B
        private void StartSpeedIncreasing()
        {
        	MessageKit.removeObserver(25, StartSpeedIncreasing);
        	float increase;
        	if ((int)UserGameSettings.currentMode == 1)
        	{
        		increase = 0.3f;
        	}
        	else
        	{
        		if ((int)UserGameSettings.currentDifficulty == 0)
        		{
        			return;
        		}
        		increase = 0.1f;
        	}
        	StartCoroutine(SpeedIncreasing(increase));
        }

        // Token: 0x06001F06 RID: 7942 RVA: 0x0000839D File Offset: 0x0000659D
        public void ToggleStopAgent()
        {
        	if (agent.speed == 0f)
        	{
        		agent.speed = normalAgentSpeed;
        	}
        	else
        	{
        		normalAgentSpeed = agent.speed;
        		agent.speed = 0f;
        	}
        }

        // Token: 0x06001F07 RID: 7943 RVA: 0x0000839F File Offset: 0x0000659F
        public void StopAgent()
        {
        	if (agent.isOnNavMesh)
        	{
        		agent.isStopped = true;
        	}
        	else
        	{
        		previousSpeed = agent.speed;
        		agent.speed = 0f;
        	}
        }

        // Token: 0x06001F08 RID: 7944 RVA: 0x000083A1 File Offset: 0x000065A1
        public void RestartAgent()
        {
        	agent.isStopped = false;
        	if (agent.speed == 0f)
        	{
        		agent.speed = previousSpeed;
        	}
        }

        // Token: 0x06001F09 RID: 7945 RVA: 0x000083A3 File Offset: 0x000065A3
        private void FreezeEnemyWhenWaitingForSecondPlayer(bool isWaiting)
        {
        	if (isWaiting)
        	{
        		StopAgent();
        	}
        	else
        	{
        		RestartAgent();
        	}
        }

        // Token: 0x06001F0A RID: 7946 RVA: 0x000083A5 File Offset: 0x000065A5
        protected IEnumerator SpeedIncreasing(float speed)
        {
        	while (true)
        	{
        		patrolSpeed += speed;
        		chaseSpeed += speed;
        		yield return Utils.GetDelay(60f);
        	}
        }

        // Token: 0x06001F0B RID: 7947 RVA: 0x000083A8 File Offset: 0x000065A8
        private void DoorDetected(string _enemyName)
        {
        	if (_enemyName == name)
        	{
        		isDoorDetected = true;
        		agent.speed = 0f;
        	}
        }

        // Token: 0x06001F0C RID: 7948 RVA: 0x000083AA File Offset: 0x000065AA
        private void SetParametersDependentOnDifficultyLevel()
        {
        	float[] speed = CharactersConfig.GetEnemySpeed(UserGameSettings.currentDifficulty, UserGameSettings.currentLevel, UserGameSettings.platform, UserGameSettings.currentEnemy);
        	patrolSpeed = speed[0];
        	chaseSpeed = speed[1];
        	agent.speed = patrolSpeed;
        }

        // Token: 0x06001F0D RID: 7949 RVA: 0x000083AC File Offset: 0x000065AC
        public IEnumerator WaitForPathComputedCoroutine()
        {
        	yield return null;
        	while (agent != null && agent.pathPending)
        	{
        		yield return Utils.GetDelay(0.2f);
        	}
        }

        // Token: 0x06001F0E RID: 7950
        public abstract void SetReviveAnimation();

        // Token: 0x04001A65 RID: 6757
        [SerializeField]
        protected NavMeshAgent agent;

        // Token: 0x04001A66 RID: 6758
        [SerializeField]
        protected Animator anim;

        // Token: 0x04001A67 RID: 6759
        [SerializeField]
        private float targetReachedDistance = 0.5f;

        // Token: 0x04001A68 RID: 6760
        private float normalAgentSpeed;

        // Token: 0x04001A69 RID: 6761
        private float previousSpeed;

        // Token: 0x04001A6A RID: 6762
        protected float patrolSpeed;

        // Token: 0x04001A6B RID: 6763
        protected float chaseSpeed;

        // Token: 0x04001A6C RID: 6764
        private bool isDoorDetected;

        // Token: 0x04001A6D RID: 6765
        private GameObject[] agentDestinations;
    }
}
