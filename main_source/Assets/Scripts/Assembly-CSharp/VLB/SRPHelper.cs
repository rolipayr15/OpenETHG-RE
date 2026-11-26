using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace VLB
{
	public static class SRPHelper
	{
		private static bool m_IsRenderPipelineCached;

		private static RenderPipeline m_RenderPipelineCached;

		public static string renderPipelineScriptingDefineSymbolAsString => null;

		public static RenderPipeline projectRenderPipeline => RenderPipeline.BuiltIn;

		private static RenderPipeline ComputeRenderPipeline()
		{
			return RenderPipeline.BuiltIn;
		}

		public static bool IsUsingCustomRenderPipeline()
		{
			return false;
		}

		public static void RegisterCameraRenderingCallbacks(Action<ScriptableRenderContext, Camera> cbOnBegin, Action<ScriptableRenderContext, Camera> cbOnEnd)
		{
		}

		public static void UnregisterCameraRenderingCallbacks(Action<ScriptableRenderContext, Camera> cbOnBegin, Action<ScriptableRenderContext, Camera> cbOnEnd)
		{
		}
	}
}
