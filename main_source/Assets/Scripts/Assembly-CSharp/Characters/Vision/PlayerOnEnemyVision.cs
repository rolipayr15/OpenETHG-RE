using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Characters.Vision
{
	public abstract class PlayerOnEnemyVision : MonoBehaviour
	{

		[SerializeField]
		public Camera monsterOverlayCamera;

		[SerializeField]
		protected Transform monsterFaceZoomPoint;

		private Coroutine followMonsterFaceCoroutine;

		protected GameObject playerCamera;

		protected Camera playerCameraComponent;

		protected Transform playerCameraT;

		protected float initialValuePlayerCameraFoV;

		[SerializeField]
		private Transform killCamPoint;

		public Transform cam;

		public Transform camPoint;

		private Transform lookPoint;

		public float rotationSpeed = 8f;

		public float posSpeed = 20f;

		private bool playerCameraFollowsMonsterFace;

		[SerializeField]
		private Camera targetCamera;

		[SerializeField]
		private float fromFOV = 50f;

		[SerializeField]
		private float toFOV = 25f;

		[SerializeField]
		private float duration = 1f;

		private Coroutine fovCoroutine;

		public Vector3 lookAtPoint => GetGirlLookAtPoint();

		private void Start()
		{
			if (monsterOverlayCamera.GetComponent<PixelModeCamera>() == null)
			{
				monsterOverlayCamera.gameObject.AddComponent<PixelModeCamera>();
			}
			playerCamera = CustomSingleton<PlayerManager>.Instance.PlayerCamera;
			playerCameraComponent = playerCamera.GetComponent<Camera>();
			playerCameraT = playerCamera.transform;
			initialValuePlayerCameraFoV = playerCameraComponent.fieldOfView;
			cam = monsterOverlayCamera.transform;
			camPoint = cam;
			lookPoint = monsterFaceZoomPoint.transform;
		}

		public abstract Vector3 GetGirlLookAtPoint();

		public abstract void UpdateMonsterOverlayFieldOfView();

		private void LateUpdate()
		{
			if (!lookPoint)
			{
				return;
			}
			cam.position = Vector3.Lerp(cam.position, camPoint.position, posSpeed * Time.deltaTime);
			Quaternion look = Quaternion.LookRotation((lookPoint.position - cam.position).normalized, Vector3.up);
			cam.rotation = Quaternion.Slerp(cam.rotation, look, rotationSpeed * Time.deltaTime);
			if (playerCameraFollowsMonsterFace)
			{
				Quaternion playerLook = Quaternion.LookRotation((lookPoint.position - playerCameraT.position).normalized, Vector3.up);
				playerCameraT.rotation = Quaternion.Slerp(playerCameraT.rotation, playerLook, rotationSpeed * Time.deltaTime);
			}
			if ((lookPoint.position - cam.position).sqrMagnitude >= 0.0001f)
			{
				cam.rotation = Quaternion.Slerp(cam.rotation, look, rotationSpeed * Time.deltaTime);
			}
		}

		public void StartFollowMonsterFace()
		{
			playerCameraFollowsMonsterFace = true;
			monsterOverlayCamera.enabled = true;
			playerCameraComponent.enabled = false;
			PlayFovAnimation();
		}

		public void StopFollowMonsterFace()
		{
			if (followMonsterFaceCoroutine != null)
			{
				StopCoroutine(followMonsterFaceCoroutine);
			}
			playerCameraComponent.enabled = true;
			monsterOverlayCamera.enabled = false;
			playerCameraFollowsMonsterFace = false;
		}

		public void DisablePlayerFPCamera()
		{
			playerCamera.GetComponent<vp_FPCamera>().enabled = false;
		}

		public void EnablePlayerFPCamera()
		{
			playerCamera.GetComponent<vp_FPCamera>().enabled = true;
		}

		public void PlayFovAnimation()
		{
			if (fovCoroutine != null)
			{
				StopCoroutine(fovCoroutine);
			}
			fovCoroutine = StartCoroutine(Animate());
		}

		private IEnumerator Animate()
		{
			float elapsed = 0f;
			monsterOverlayCamera.fieldOfView = fromFOV;
			while (elapsed < duration)
			{
				elapsed += Time.deltaTime;
				monsterOverlayCamera.fieldOfView = Mathf.Lerp(fromFOV, toFOV, Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(elapsed / duration)));
				yield return null;
			}
			monsterOverlayCamera.fieldOfView = toFOV;
			fovCoroutine = null;
		}
	}
}
