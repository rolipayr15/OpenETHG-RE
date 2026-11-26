using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB
{
	public abstract class BeamGeometryAbstractBase : MonoBehaviour
	{
		protected Matrix4x4 m_ColorGradientMatrix;

		protected Material m_CustomMaterial;

		private Stack<Camera> m_CurrentCameraRenderingSRP;

		public MeshRenderer meshRenderer { get; protected set; }

		public MeshFilter meshFilter { get; protected set; }

		public Mesh coneMesh { get; protected set; }

		protected abstract VolumetricLightBeamAbstractBase GetMaster();

		private void OnDisable()
		{
		}

		protected virtual void OnEnable()
		{
		}

		private void Start()
		{
		}

		private void OnDestroy()
		{
		}

		private void DestroyOrphanBeamGeom()
		{
		}

		public static void DestroyBeamGeometryGameObject(BeamGeometryAbstractBase beamGeom)
		{
		}

		protected abstract void OnWillCameraRenderThisBeam(Camera cam);

		private void OnBeginCameraRenderingSRP(ScriptableRenderContext context, Camera cam)
		{
		}

		private void OnEndCameraRenderingSRP(ScriptableRenderContext context, Camera cam)
		{
		}

		private void OnWillRenderObject()
		{
		}
	}
}
