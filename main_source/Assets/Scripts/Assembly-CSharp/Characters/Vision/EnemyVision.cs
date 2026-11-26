using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Characters.Vision
{
	[RequireComponent(typeof(EnemyCheats))]
	public class EnemyVision : MonoBehaviour
	{

		internal bool cheatMonsterBlindness;

		private bool raycastDisabled;

		private bool isPlayerCloaked;

		private bool isGuestCloaked;

		private bool canSeeHost;

		private bool canSeeGuest;

		private Vector3 hostLastKnownLocation;

		private Vector3 guestLastKnownLocation;

		private int castsSinceHostSeen;

		private int castsSinceGuestSeen = UserGameSettings.IsMultiplayer() ? 0 : int.MaxValue;

		private int hostLayer;

		private int guestLayer;

		private GameObject host;

		private Transform hostT;

		private Transform hostHeartT;

		private GameObject guest;

		private Transform guestT;

		private Transform guestHeartT;

		[SerializeField]
		private Transform raycastOrigin;

		[SerializeField]
		private Camera enemyCamera;

		[Tooltip("Layers to test when raycasting by monster")]
		[SerializeField]
		protected LayerMask playerRaycastLayers;

		private readonly RaycastHit[] visionHits = new RaycastHit[16];

		private bool hasEverSeenTarget;

		public bool CanSeeHost => canSeeHost;

		public bool CanSeeGuest => canSeeGuest;

		public Vector3 HostLastKnownLocation => hostLastKnownLocation;

		public Vector3 GuestLastKnownLocation => guestLastKnownLocation;

		public Transform HostT => hostT;

		public Transform HostHeartT => hostHeartT;

		public Transform GuestT => guestT;

		public Transform GuestHeartT => guestHeartT;

		public Camera EnemyCamera => enemyCamera;

		public Transform RaycastOrigin => raycastOrigin;

		public void ResetSight()
		{
			canSeeHost = false;
			canSeeGuest = false;
		}

		public void SetRaycastDisabled(bool value)
		{
			raycastDisabled = value;
		}

		private void Awake()
		{
			hostLayer = LayerMask.NameToLayer("LocalPlayer");
			guestLayer = LayerMask.NameToLayer("Avatar");
			Physics.SyncTransforms();
		}

		private void Start()
		{
			host = CustomSingleton<PlayerManager>.Instance.Player;
			hostT = host.transform;
			hostHeartT = CustomSingleton<PlayerManager>.Instance.Heart.transform;
			if (UserGameSettings.IsMultiplayer())
			{
				guest = CustomSingleton<GuestManager>.Instance.Avatar;
				guestHeartT = CustomSingleton<GuestManager>.Instance.Heart.transform;
				guestT = guest.transform;
			}
			MessageKit<bool>.addObserver(14, Cloak);
			MessageKit<bool>.addObserver(119, CloakGuest);
			StartCoroutine(StartRaycast());
		}

		private void Cloak(bool isCloaked)
		{
			isPlayerCloaked = isCloaked;
		}

		private void CloakGuest(bool isCloaked)
		{
			isGuestCloaked = isCloaked;
		}

		private void OnDestroy()
		{
			MessageKit<bool>.removeObserver(14, Cloak);
			MessageKit<bool>.removeObserver(119, CloakGuest);
		}

		public EnemyTarget GetLastSeenEnemyTarget()
		{
			if (!hasEverSeenTarget)
			{
				return EnemyTarget.None;
			}
			return castsSinceGuestSeen <= castsSinceHostSeen ? (EnemyTarget)2 : (EnemyTarget)1;
		}

		public bool CanSeePlayer()
		{
			return canSeeHost || canSeeGuest;
		}

		private IEnumerator StartRaycast()
		{
			while (true)
			{
				canSeeHost = !isPlayerCloaked && RaycastHostOrGuest(hostHeartT.position, hostLayer);
				if (UserGameSettings.IsMultiplayer())
				{
					canSeeGuest = !isGuestCloaked && RaycastHostOrGuest(guestHeartT.position, guestLayer);
				}
				if (canSeeHost)
				{
					castsSinceHostSeen = 0;
					hasEverSeenTarget = true;
					hostLastKnownLocation = hostHeartT.position;
				}
				else
				{
					if (canSeeGuest)
					{
						hasEverSeenTarget = true;
					}
					castsSinceHostSeen++;
				}
				if (UserGameSettings.IsMultiplayer())
				{
					if (canSeeGuest)
					{
						castsSinceGuestSeen = 0;
						guestLastKnownLocation = guestHeartT.position;
					}
					else
					{
						castsSinceGuestSeen++;
					}
				}
				yield return Utils.GetDelay(0.1f);
			}
		}

		private bool RaycastHostOrGuest(Vector3 heartPosition, int layer)
		{
			if (raycastDisabled)
			{
				return false;
			}
			Vector3 origin = raycastOrigin.position;
			Vector3 toHeart = heartPosition - origin;
			if (toHeart.sqrMagnitude < 0.0001f)
			{
				return true;
			}
			int count = Physics.SphereCastNonAlloc(origin, 0.1f, toHeart.normalized, visionHits, 18f, playerRaycastLayers, QueryTriggerInteraction.Collide);
			bool found = false;
			RaycastHit nearest = default(RaycastHit);
			for (int i = 0; i < count; i++)
			{
				RaycastHit hit = visionHits[i];
				if (!IsOpenDoorTrigger(hit.collider) && (!found || hit.distance < nearest.distance))
				{
					nearest = hit;
					found = true;
				}
			}
			if (found && nearest.collider != null)
			{
				return nearest.collider.gameObject.layer == layer;
			}
			return false;
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
	}
}
