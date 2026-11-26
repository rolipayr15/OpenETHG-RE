using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LightingProbeViews", menuName = "Eyes/Lighting Probe Views")]
public sealed class LightingProbeViews : ScriptableObject
{
	[Serializable]
	public sealed class View
	{
		public string name;

		public Vector3 position;

		public Vector3 eulerAngles;

		[TextArea]
		public string readsAs;
	}

	public const string ResourcePrefix = "LightingProbeViews_";

	public const string MansionResourceName = "LightingProbeViews_Mansion";

	public List<View> views;

	public static string ResourceNameFor(string scene)
	{
		return null;
	}

	public static LightingProbeViews Mansion()
	{
		return null;
	}
}
