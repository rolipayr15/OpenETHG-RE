using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

namespace Characters.Mover
{
	public class GooseCompanion : MonoBehaviour
	{
		[Header("Leader")]
		[SerializeField]
		private Transform leader;

		[SerializeField]
		private NavMeshAgent leaderAgent;

		[SerializeField]
		private Animator leaderAnimator;

		[SerializeField]
		private Transform leaderBody;

		[Header("Self")]
		[SerializeField]
		private Transform body;

		[SerializeField]
		private Animator anim;

		[Tooltip("The companion directly in front of this one in the line, if any; keeps the pack spaced.")]
		[SerializeField]
		private GooseCompanion ahead;

		[Header("Formation")]
		[SerializeField]
		private float distanceBehind = 0.9f;

		[SerializeField]
		private float lateralOffset = 0.35f;

		[SerializeField]
		private float animatorSpeedScale = 1f;

		[Tooltip("Seconds this companion lags behind the leader's phase changes and charge lift, so the pack never moves as one.")]
		[SerializeField]
		private float reactionDelay = 0.15f;

		[Header("Tuning")]
		[SerializeField]
		private float speedFactor = 1.15f;

		[SerializeField]
		private float minSpeed = 1.5f;

		[SerializeField]
		private float navSampleRadius = 0.5f;

		[SerializeField]
		private float offsetTolerance = 0.3f;

		[SerializeField]
		private float offsetEaseSeconds = 0.5f;

		[SerializeField]
		private float minSpacing = 0.6f;

		[SerializeField]
		private float snapDistance = 4f;

		[SerializeField]
		private float turnDegreesPerSecond = 270f;

		[Tooltip("Turn rate while parked, easing toward the leader's facing.")]
		[SerializeField]
		private float parkedTurnDegreesPerSecond = 90f;

		[SerializeField]
		private float headingSmoothing = 6f;

		[SerializeField]
		private float crossFadeSeconds = 0.2f;

		[Tooltip("Walk clip speed never drops below this while walking, so a short stall does not freeze the stride.")]
		[SerializeField]
		private float walkSpeedFloor = 0.6f;

		[Tooltip("Below this measured speed a walking companion is parked and idles instead of treading in place.")]
		[SerializeField]
		private float parkedSpeed = 0.1f;

		[SerializeField]
		private float speedSmoothing = 8f;

		[Tooltip("Each phase change waits reactionDelay scaled by a random factor in this range, so takeoff and landing never ripple the same way twice.")]
		[SerializeField]
		private Vector2 reactionJitter = new Vector2(0.5f, 1.6f);

		[Tooltip("Flight clip speed is animatorSpeedScale times a random factor in this range, rolled at each takeoff.")]
		[SerializeField]
		private Vector2 flightSpeedSpread = new Vector2(0.7f, 1.4f);

		private readonly GooseTrail trail = new GooseTrail(40, 0.2f);

		private readonly Dictionary<int, (string phase, int variant)> phaseByStateHash = new Dictionary<int, (string phase, int variant)>();

		private readonly Dictionary<string, (string[] pool, int leaderCount, bool loops)> poolByPhase = new Dictionary<string, (string[] pool, int leaderCount, bool loops)>();

		private float offsetBlend;

		private string lastPhase;

		private string leaderPhase;

		private string pendingPhase;

		private float pendingAt;

		private float currentDelay;

		private float liftY;

		private Vector3 heading;

		private string preRolledPhase;

		private string oneShotBefore;

		private int preRolledVariant = -1;

		private Vector3 bodyRestLocalPosition;

		private float leaderBodyRestY;

		private float currentSpeed;

		private Vector3 lastWorldPosition;

		private Vector3 lastLeaderPosition;

		private bool leaderSeen;

		public int CurrentVariant { get; private set; } = -1;

		private void Awake()
		{
			foreach ((string phase, string[] variants, string[] extra, bool loops) in GooseClips.Phases)
			{
				string[] pool = new string[variants.Length + extra.Length];
				variants.CopyTo(pool, 0);
				extra.CopyTo(pool, variants.Length);
				poolByPhase[phase] = (pool, variants.Length, loops);
				for (int i = 0; i < pool.Length; i++)
				{
					phaseByStateHash[Animator.StringToHash(GooseClips.StateName(phase, i))] = (phase, i);
				}
			}
			if (body != null)
			{
				bodyRestLocalPosition = body.localPosition;
			}
			if (leaderBody != null)
			{
				leaderBodyRestY = leaderBody.localPosition.y;
			}
			if (anim != null)
			{
				anim.speed = animatorSpeedScale;
			}
		}

		public static float LeaderSpeed(float? agentSpeed, Vector3 leaderDelta, float deltaTime)
		{
			if (agentSpeed.HasValue)
			{
				return agentSpeed.Value;
			}
			return deltaTime > 0f ? Mathf.Sqrt(leaderDelta.x * leaderDelta.x + leaderDelta.z * leaderDelta.z) / deltaTime : 0f;
		}

		private void OnEnable()
		{
			leaderSeen = false;
			lastPhase = null;
			trail.Clear();
			leaderPhase = null;
			pendingPhase = null;
			liftY = 0f;
			currentDelay = reactionDelay;
			heading = (leader != null ? leader : transform).forward;
			if (anim != null)
			{
				anim.speed = animatorSpeedScale;
			}
			offsetBlend = 0f;
			currentSpeed = 0f;
			SnapToLeader();
		}

		private void LateUpdate()
		{
			if (leader == null)
			{
				return;
			}
			Vector3 leaderPosition = leader.position;
			trail.Record(leaderPosition);
			if (snapDistance < Vector3.Distance(lastWorldPosition, leaderPosition))
			{
				SnapToLeader();
			}
			Vector3 last = lastWorldPosition;
			Vector3 target = FormationTarget(leaderPosition);
			Vector3 leaderDelta = leaderSeen ? leaderPosition - lastLeaderPosition : Vector3.zero;
			lastLeaderPosition = leaderPosition;
			leaderSeen = true;
			float? agentSpeed = null;
			if (leaderAgent != null)
			{
				agentSpeed = leaderAgent.velocity.magnitude;
			}
			float speed = Mathf.Max(LeaderSpeed(agentSpeed, leaderDelta, Time.deltaTime) * speedFactor, minSpeed);
			Vector3 next = Vector3.MoveTowards(last, target, Time.deltaTime * speed);
			next = KeepSpacing(next, leaderPosition);
			if (ahead != null)
			{
				next = KeepSpacing(next, ahead.transform.position);
			}
			transform.position = next;
			Vector3 delta = next - last;
			delta.y = 0f;
			lastWorldPosition = next;
			float measured = Time.deltaTime > 0f ? delta.magnitude / Time.deltaTime : 0f;
			currentSpeed = Mathf.Lerp(currentSpeed, measured, speedSmoothing * Time.deltaTime);
			TurnTowardsMotion(delta);
			MirrorLift();
			MirrorAnimation();
		}

		private void TurnTowardsMotion(Vector3 delta)
		{
			bool parked = currentSpeed < parkedSpeed || delta.sqrMagnitude <= 1E-06f;
			Vector3 direction = parked ? leader.forward : delta.normalized;
			direction.y = 0f;
			if (direction.sqrMagnitude < 1E-06f)
			{
				return;
			}
			heading = Vector3.Slerp(heading, direction.normalized, Mathf.Clamp01(headingSmoothing * Time.deltaTime));
			float rate = parked ? parkedTurnDegreesPerSecond : turnDegreesPerSecond;
			transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.LookRotation(heading, Vector3.up), rate * Time.deltaTime);
		}

		private Vector3 FormationTarget(Vector3 leaderPosition)
		{
			trail.PointBehind(distanceBehind, leaderPosition, out Vector3 point, out Vector3 direction);
			if ((direction - Vector3.zero).sqrMagnitude < 9.9999994E-11f)
			{
				direction = leader.forward;
			}
			Vector3 offsetTarget = point + Vector3.Cross(Vector3.up, direction) * (lateralOffset * offsetBlend);
			NavMeshHit hit;
			bool blocked = !NavMesh.SamplePosition(offsetTarget, out hit, navSampleRadius, -1) || offsetTolerance < Vector3.Distance(hit.position, offsetTarget);
			float step = offsetEaseSeconds > 0f ? Time.deltaTime / offsetEaseSeconds : 1f;
			if (blocked)
			{
				offsetBlend = Mathf.MoveTowards(offsetBlend, 0f, step);
				if (NavMesh.SamplePosition(point, out hit, navSampleRadius, -1))
				{
					return hit.position;
				}
				return point;
			}
			offsetBlend = Mathf.MoveTowards(offsetBlend, 1f, step);
			return hit.position;
		}

		private Vector3 KeepSpacing(Vector3 position, Vector3 other)
		{
			Vector3 away = new Vector3(position.x - other.x, 0f, position.z - other.z);
			if (away.sqrMagnitude >= minSpacing * minSpacing)
			{
				return position;
			}
			if (away.sqrMagnitude < 1E-06f)
			{
				away = -leader.forward;
			}
			return other + away.normalized * minSpacing;
		}

		private void SnapToLeader()
		{
			if (leader == null)
			{
				return;
			}
			trail.PointBehind(0f, leader.position, out Vector3 _, out Vector3 direction);
			if ((direction - Vector3.zero).sqrMagnitude < 9.9999994E-11f)
			{
				direction = leader.forward;
			}
			Vector3 target = leader.position - direction * distanceBehind + Vector3.Cross(Vector3.up, direction) * lateralOffset;
			transform.position = NavMesh.SamplePosition(target, out NavMeshHit hit, navSampleRadius + navSampleRadius, -1) ? hit.position : target;
			transform.rotation = leader.rotation;
			lastWorldPosition = transform.position;
		}

		private void MirrorLift()
		{
			if (body == null || leaderBody == null)
			{
				return;
			}
			float target = leaderBody.localPosition.y - leaderBodyRestY;
			if (currentDelay > 0f)
			{
				target = liftY + (target - liftY) * Mathf.Clamp01(1f - Mathf.Exp(-Time.deltaTime / currentDelay));
			}
			liftY = target;
			body.localPosition = bodyRestLocalPosition + Vector3.up * liftY;
		}

		private void MirrorAnimation()
		{
			if (anim == null || leaderAnimator == null)
			{
				return;
			}
			AnimatorStateInfo info = leaderAnimator.GetCurrentAnimatorStateInfo(0);
			if (phaseByStateHash.TryGetValue(info.shortNameHash, out (string phase, int variant) state) && state.phase != leaderPhase)
			{
				leaderPhase = state.phase;
				pendingPhase = state.phase;
				currentDelay = reactionDelay * UnityEngine.Random.Range(reactionJitter.x, reactionJitter.y);
				pendingAt = Time.time + currentDelay;
			}
			if (pendingPhase == null || Time.time < pendingAt)
			{
				if (lastPhase != "Walk" && lastPhase != "Idle")
				{
					return;
				}
			}
			else
			{
				leaderPhase = pendingPhase;
				pendingPhase = null;
			}
			ApplyPhase(leaderPhase);
		}

		private void ApplyPhase(string phase)
		{
			if (phase == null)
			{
				return;
			}
			if (phase == "Walk")
			{
				if (currentSpeed >= parkedSpeed || (lastPhase == "Walk" && currentSpeed >= parkedSpeed * 0.5f))
				{
					anim.SetFloat("agent_speed", Mathf.Max(currentSpeed, walkSpeedFloor));
				}
				else
				{
					phase = "Idle";
				}
			}
			if (phase == lastPhase)
			{
				return;
			}
			lastPhase = phase;
			if (phase == "Charge")
			{
				anim.speed = animatorSpeedScale * UnityEngine.Random.Range(flightSpeedSpread.x, flightSpeedSpread.y);
			}
			else if (phase != "Land")
			{
				anim.speed = animatorSpeedScale;
			}
			if (phase == preRolledPhase && phaseByStateHash.TryGetValue(anim.GetCurrentAnimatorStateInfo(0).shortNameHash, out (string phase, int variant) current))
			{
				if ((current.phase == phase && current.variant == preRolledVariant) || current.phase == oneShotBefore)
				{
					CurrentVariant = preRolledVariant;
					preRolledPhase = null;
					return;
				}
			}
			preRolledPhase = null;
			(string[] pool, int leaderCount, bool loops) entry = poolByPhase[phase];
			int variant = PickVariant(entry.pool.Length, entry.leaderCount, phase);
			CurrentVariant = variant;
			int parameter = variant;
			string next = phase == "Land" ? "Dazed" : (phase == "Attack" ? "Idle" : null);
			if (next != null)
			{
				oneShotBefore = phase;
				preRolledPhase = next;
				(string[] pool, int leaderCount, bool loops) nextEntry = poolByPhase[next];
				parameter = PickVariant(nextEntry.pool.Length, nextEntry.leaderCount, next);
				preRolledVariant = parameter;
			}
			anim.SetInteger("variant", parameter);
			anim.CrossFade(GooseClips.StateName(phase, variant), crossFadeSeconds, 0, entry.loops ? UnityEngine.Random.Range(0f, 0.8f) : 0f);
		}

		private int PickVariant(int poolSize, int leaderCount, string phase)
		{
			if (poolSize < 2)
			{
				return 0;
			}
			List<int> taken = new List<int>(2);
			if (phase == leaderPhase)
			{
				int leaderVariant = leaderAnimator.GetInteger("variant");
				if (leaderVariant > -1 && leaderVariant < leaderCount)
				{
					taken.Add(leaderVariant);
				}
			}
			if (ahead != null && ahead.lastPhase == phase)
			{
				taken.Add(ahead.CurrentVariant);
			}
			List<int> free = new List<int>(poolSize);
			for (int i = 0; i < poolSize; i++)
			{
				if (!taken.Contains(i))
				{
					free.Add(i);
				}
			}
			if (free.Count < 1)
			{
				return UnityEngine.Random.Range(0, poolSize);
			}
			return free[UnityEngine.Random.Range(0, free.Count)];
		}
	}
}
