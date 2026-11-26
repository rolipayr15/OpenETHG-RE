using Prime31.MessageKit;
using Characters.Agents;
using UnityEngine;

public class MeatDogTrigger : MonoBehaviour
{
	private const float DetectionRetryIntervalSeconds = 0.2f;

	private const float DetectionRayDistance = 4f;

	[SerializeField]
	private GameObject parent;

	[SerializeField]
	private LayerMask layerMask;

	[SerializeField]
	private Meat meatInteraction;

	private DogAgent dogAgent;

	private float nextDetectionTime;

	private int detectionAttemptCount;

	private string lastRejectionReason = "none";

	private bool claimedLocally;

	private void Start()
	{
		MessageKit<string>.addObserver(27, MeatConsumedFromHost);
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(27, MeatConsumedFromHost);
	}

	private void OnDisable()
	{
		dogAgent = null;
	}

	private void OnTriggerEnter(Collider other)
	{
		if (TryGetDog(other, out DogAgent foundDog) && dogAgent == null)
		{
			dogAgent = foundDog;
			nextDetectionTime = Time.time;
			detectionAttemptCount = 0;
			lastRejectionReason = "none";
			TryDetectionIfDue();
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (TryGetDog(other, out DogAgent foundDog) && foundDog == dogAgent)
		{
			TryDetectionIfDue();
		}
	}

	private void OnTriggerExit(Collider other)
	{
		if (TryGetDog(other, out DogAgent foundDog) && foundDog == dogAgent)
		{
			dogAgent = null;
		}
	}

	private void TryDetectionIfDue()
	{
		if (dogAgent != null && meatInteraction != null && meatInteraction.canBeCollected && !claimedLocally && !dogAgent.IsMeatFound && !dogAgent.IsDogEating && nextDetectionTime <= Time.time)
		{
			CheckIfCanSeeMeat();
		}
	}

	private void CheckIfCanSeeMeat()
	{
		detectionAttemptCount++;
		Transform origin = dogAgent != null && dogAgent.EnemyVision != null ? dogAgent.EnemyVision.RaycastOrigin : null;
		if (origin == null)
		{
			RejectDetection("missing_dog_raycast_origin", default(RaycastHit), false, 0f, 0);
			return;
		}
		Vector3 toDog = origin.position - transform.position;
		RaycastHit[] hits = Physics.RaycastAll(transform.position, toDog, DetectionRayDistance, layerMask, QueryTriggerInteraction.Collide);
		bool found = TryFindNearestRelevantHit(hits, out RaycastHit nearest, out int ignored);
		if (found && IsTrackedDog(nearest.collider))
		{
			AcceptMeat(nearest, toDog.magnitude, ignored);
			return;
		}
		string reason = hits.Length == 0 ? "no_hit" : (found ? "first_hit_not_enemy" : "no_relevant_hit");
		RejectDetection(reason, nearest, found, toDog.magnitude, ignored);
	}

	private bool TryFindNearestRelevantHit(RaycastHit[] hits, out RaycastHit nearestRelevantHit, out int ignoredOpenDoorTriggers)
	{
		nearestRelevantHit = default(RaycastHit);
		ignoredOpenDoorTriggers = 0;
		bool found = false;
		foreach (RaycastHit hit in hits)
		{
			if (IsOpenDoorTrigger(hit.collider))
			{
				ignoredOpenDoorTriggers++;
			}
			else
			{
				if (!found || hit.distance < nearestRelevantHit.distance)
				{
					nearestRelevantHit = hit;
				}
				found = true;
			}
		}
		return found;
	}

	private bool IsTrackedDog(Collider hitCollider)
	{
		if (hitCollider == null || dogAgent == null)
		{
			return false;
		}
		return hitCollider.gameObject.layer == LayerMask.NameToLayer("Enemy") && hitCollider.GetComponentInParent<DogAgent>() == dogAgent;
	}

	private static bool IsOpenDoorTrigger(Collider hitCollider)
	{
		if (hitCollider == null || !hitCollider.isTrigger)
		{
			return false;
		}
		Door door = hitCollider.GetComponentInParent<Door>();
		return door != null && door.isOpen;
	}

	private void AcceptMeat(RaycastHit hit, float dogDistance, int ignoredOpenDoorTriggers)
	{
		claimedLocally = true;
		meatInteraction.canBeCollected = false;
		dogAgent.GoForTheMeatPiece(transform, parent);
		MessageKit.post(27);
		MessageKit<string>.post(27, meatInteraction.meatId);
	}

	private void RejectDetection(string rejectionReason, RaycastHit hit, bool hasHit, float dogDistance, int ignoredOpenDoorTriggers)
	{
		lastRejectionReason = rejectionReason;
		nextDetectionTime = Time.time + DetectionRetryIntervalSeconds;
	}

	private void MeatConsumedFromHost(string id)
	{
		if (meatInteraction != null && id == meatInteraction.meatId)
		{
			meatInteraction.canBeCollected = false;
			if (!claimedLocally)
			{
				HideParent("consumed by host");
			}
		}
	}

	private void HideParent(string reason)
	{
		if (parent != null && parent.activeSelf)
		{
			parent.SetActive(false);
		}
	}

	private static bool TryGetDog(Collider other, out DogAgent foundDog)
	{
		foundDog = null;
		if (other == null || !other.CompareTag("Enemy"))
		{
			return false;
		}
		foundDog = other.GetComponentInParent<DogAgent>();
		return foundDog != null;
	}

	private string MeatId()
	{
		return meatInteraction != null ? meatInteraction.meatId : "<none>";
	}

	private int RetryCount()
	{
		return detectionAttemptCount < 1 ? 0 : detectionAttemptCount - 1;
	}
}
