using UnityEngine;

namespace Gameplay.Eggs
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(Camera))]
	[DefaultExecutionOrder(10000)]
	public sealed class EggCameraRollRenderer : MonoBehaviour
	{
		public EggEffects Effects;

		private Camera renderCamera;

		private bool overridden;

		private void LateUpdate()
		{
			RefreshView();
		}

		private void OnPreCull()
		{
			RefreshView();
		}

		public void RefreshView()
		{
			if (renderCamera == null)
			{
				renderCamera = GetComponent<Camera>();
			}
			// The camera roll prank rotates the rendered view without moving the camera transform.
			if (Effects != null && Effects.IsLocalPlayer && Effects.isActiveAndEnabled && (int)Effects.ActivePrank == 5 && Effects.CameraRollPivot != null)
			{
				Matrix4x4 view = Matrix4x4.Scale(new Vector3(1f, 1f, -1f)) * Matrix4x4.Rotate(Quaternion.Inverse(Effects.CameraRollPivot.localRotation));
				renderCamera.worldToCameraMatrix = view * renderCamera.transform.worldToLocalMatrix;
				overridden = true;
				return;
			}
			if (overridden)
			{
				renderCamera.ResetWorldToCameraMatrix();
				overridden = false;
			}
		}

		private void OnDisable()
		{
			if (renderCamera != null && overridden)
			{
				renderCamera.ResetWorldToCameraMatrix();
			}
			overridden = false;
		}
	}
}
