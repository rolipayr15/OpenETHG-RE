using System;
using System.Collections;
using Bibloteka;
using Prime31.MessageKit;
using UnityEngine;

public class CollectibleAnimationHandler : MonoBehaviour
{
	private struct AnimationConfig
	{
		public GameObject prefab;

		public RectTransform animationTarget;

		public bool lookAtCamera;

		public Quaternion grantSpawnRotation;
	}

	[SerializeField]
	private AnimationCurve growShrinkCurve;

	[SerializeField]
	private GameObject bagLitePrefab;

	[SerializeField]
	private GameObject eyeLitePrefab;

	[SerializeField]
	private GameObject meatLitePrefab;

	[SerializeField]
	private GameObject sweetsLitePrefab;

	[SerializeField]
	private GameObject safeCodeLitePrefab;

	[SerializeField]
	private GameObject sawLitePrefab;

	[SerializeField]
	private GameObject keyLitePrefab;

	[SerializeField]
	private RectTransform bagAnimationTarget;

	[SerializeField]
	private RectTransform eyeAnimationTarget;

	[SerializeField]
	private RectTransform meatAnimationTarget;

	private Camera mainCamera;

	[SerializeField]
	private Camera collectibleCamera;

	[SerializeField]
	private float grantSpawnDistance = 2f;

	[SerializeField]
	private float grantAnimationStagger = 0.3f;

	private void Start()
	{
		MessageKit<Interactable>.addObserver(58, StartPickupAnimation);
		MessageKit<Type, int>.addObserver(137, StartGrantAnimation);
		mainCamera = Camera.main;
	}

	private void OnDestroy()
	{
		MessageKit<Interactable>.removeObserver(58, StartPickupAnimation);
		MessageKit<Type, int>.removeObserver(137, StartGrantAnimation);
	}

	private AnimationConfig GetAnimationConfig(Type collectibleType)
	{
		if (collectibleType == typeof(MoneyBag))
		{
			return new AnimationConfig
			{
				prefab = (((int)UserGameSettings.currentMode == 2) ? sweetsLitePrefab : bagLitePrefab),
				animationTarget = bagAnimationTarget,
				lookAtCamera = false,
				grantSpawnRotation = Quaternion.Euler(-90f, 0f, 0f)
			};
		}
		GameObject prefab;
		RectTransform target = null;
		if (collectibleType == typeof(Eye))
		{
			prefab = eyeLitePrefab;
			target = eyeAnimationTarget;
		}
		else if (collectibleType == typeof(SafeCode))
		{
			prefab = safeCodeLitePrefab;
		}
		else if (collectibleType == typeof(Meat))
		{
			prefab = meatLitePrefab;
			target = meatAnimationTarget;
		}
		else if (collectibleType == typeof(Saw))
		{
			prefab = sawLitePrefab;
		}
		else
		{
			if (!(collectibleType == typeof(BasementKey)))
			{
				return default(AnimationConfig);
			}
			prefab = keyLitePrefab;
		}
		return new AnimationConfig
		{
			prefab = prefab,
			animationTarget = target,
			lookAtCamera = true,
			grantSpawnRotation = Quaternion.identity
		};
	}

	private Vector3 ProjectToCollectibleCamera(Vector3 worldPosition)
	{
		Vector3 viewport = mainCamera.WorldToViewportPoint(worldPosition);
		float distance = Vector3.Distance(worldPosition, mainCamera.transform.position);
		return collectibleCamera.ScreenToWorldPoint(new Vector3(viewport.x * (float)Screen.width, viewport.y * (float)Screen.height, distance));
	}

	private GameObject SpawnCollectible(GameObject prefab, Vector3 worldPosition, Quaternion rotation)
	{
		GameObject spawned = vp_PoolManager.Spawn(prefab, Vector3.zero, Quaternion.identity);
		if (spawned == null)
		{
			return spawned;
		}
		spawned.transform.position = worldPosition;
		spawned.transform.rotation = rotation;
		spawned.transform.localScale = Vector3.one;
		spawned.layer = LayerMask.NameToLayer("CollectibleAnimation");
		return spawned;
	}

	private void StartPickupAnimation(Interactable interactable)
	{
		if (interactable != null)
		{
			AnimationConfig config = GetAnimationConfig(interactable.GetType());
			Transform source = interactable.gameObject.transform;
			GameObject spawned = SpawnCollectible(config.prefab, ProjectToCollectibleCamera(source.position), source.rotation);
			if (spawned != null)
			{
				StartCoroutine(AnimateCollectible(spawned, config, false));
			}
		}
	}

	private void StartGrantAnimation(Type collectibleType, int count)
	{
		StartCoroutine(AnimateGrantedCollectibles(GetAnimationConfig(collectibleType), count));
	}

	private IEnumerator AnimateGrantedCollectibles(AnimationConfig config, int count)
	{
		for (int i = 0; i < count; i++)
		{
			Vector3 position = collectibleCamera.ScreenToWorldPoint(new Vector3((float)Screen.width / 2f, (float)Screen.height / 2f, grantSpawnDistance));
			GameObject spawned = SpawnCollectible(config.prefab, position, config.grantSpawnRotation);
			if (spawned != null)
			{
				StartCoroutine(AnimateCollectible(spawned, config, true));
			}
			if (i < count - 1)
			{
				yield return Utils.GetDelay(grantAnimationStagger);
			}
		}
	}

	private IEnumerator AnimateCollectible(GameObject collectible, AnimationConfig config, bool skipMoveToCenter)
	{
		Transform collectibleT = collectible.transform;
		if (config.lookAtCamera)
		{
			collectibleT.LookAt(collectibleCamera.transform);
		}
		float distanceToCamera = Vector3.Distance(collectibleT.position, collectibleCamera.transform.position);
		if (!skipMoveToCenter)
		{
			Vector2 screenFrom = collectibleCamera.WorldToScreenPoint(collectibleT.position);
			Vector2 screenCenter = new Vector2((float)Screen.width / 2f, (float)Screen.height / 2f);
			yield return Utils.ChangeWithSpeed(6f, delegate(float t)
			{
				Vector2 point = Vector2.Lerp(screenFrom, screenCenter, t);
				collectibleT.position = collectibleCamera.ScreenToWorldPoint(new Vector3(point.x, point.y, distanceToCamera));
			});
		}
		Vector3 startScale = collectibleT.localScale;
		Vector3 pulseScale = startScale * 2.5f;
		yield return Utils.ChangeWithSpeed(2f, delegate(float t)
		{
			collectibleT.localScale = Vector3.Lerp(startScale, pulseScale, growShrinkCurve.Evaluate(Mathf.PingPong(t * 2f, 1f)));
		});
		if (config.animationTarget != null)
		{
			Vector2 screenFrom2 = collectibleCamera.WorldToScreenPoint(collectibleT.position);
			Vector2 screenTarget = RectTransformUtility.WorldToScreenPoint(null, config.animationTarget.position);
			yield return Utils.ChangeWithSpeed(6f, delegate(float t)
			{
				Vector2 point = Vector2.Lerp(screenFrom2, screenTarget, t);
				collectibleT.position = collectibleCamera.ScreenToWorldPoint(new Vector3(point.x, point.y, distanceToCamera));
				collectibleT.localScale = Vector3.Lerp(startScale, startScale * 0.5f, t);
			});
		}
		startScale = collectibleT.localScale;
		yield return Utils.ChangeWithSpeed(3f, delegate(float t)
		{
			collectibleT.localScale = Vector3.Lerp(startScale, Vector3.zero, t);
		});
		vp_PoolManager.Despawn(collectible);
	}
}
