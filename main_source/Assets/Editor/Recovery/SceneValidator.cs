using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Recovery tool: opens scenes in batchmode and reports missing scripts and broken object references.
// Run: Unity -batchmode -projectPath ... -executeMethod SceneValidator.ValidateFromCommandLine -scenes A.unity,B.unity
public static class SceneValidator
{
	public static void ValidateFromCommandLine()
	{
		string[] args = System.Environment.GetCommandLineArgs();
		int i = System.Array.IndexOf(args, "-scenes");
		string[] scenes = i >= 0 && i + 1 < args.Length ? args[i + 1].Split(',') : new string[0];
		foreach (string scene in scenes)
		{
			Validate(scene.Trim());
		}
	}

	public static void Validate(string scenePath)
	{
		var scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
		var all = new List<GameObject>();
		foreach (GameObject root in scene.GetRootGameObjects())
		{
			all.AddRange(root.GetComponentsInChildren<Transform>(true).Select(t => t.gameObject));
		}
		int missingScripts = 0;
		var missingByRoot = new Dictionary<string, int>();
		int brokenRefs = 0;
		var brokenSamples = new List<string>();
		foreach (GameObject go in all)
		{
			int m = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
			if (m > 0)
			{
				missingScripts += m;
				string root = go.transform.root.name;
				missingByRoot[root] = (missingByRoot.TryGetValue(root, out int c) ? c : 0) + m;
			}
			foreach (Component comp in go.GetComponents<Component>())
			{
				if (comp == null)
				{
					continue;
				}
				var so = new SerializedObject(comp);
				SerializedProperty p = so.GetIterator();
				while (p.NextVisible(true))
				{
					// A reference whose instance id is set but resolves to nothing points at a missing asset/object.
					if (p.propertyType == SerializedPropertyType.ObjectReference && p.objectReferenceValue == null && p.objectReferenceInstanceIDValue != 0)
					{
						brokenRefs++;
						if (brokenSamples.Count < 40)
						{
							brokenSamples.Add($"{Path(go)} [{comp.GetType().Name}.{p.propertyPath}]");
						}
					}
				}
			}
		}
		Debug.Log($"[SceneValidator] {scenePath}: objects={all.Count} missingScripts={missingScripts} brokenRefs={brokenRefs} lightmaps={LightmapSettings.lightmaps.Length} cameras={Object.FindObjectsByType<Camera>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length}");
		foreach (var kv in missingByRoot.OrderByDescending(kv => kv.Value))
		{
			Debug.Log($"[SceneValidator]   missing scripts under {kv.Key}: {kv.Value}");
		}
		foreach (string s in brokenSamples)
		{
			Debug.Log($"[SceneValidator]   broken ref: {s}");
		}
	}

	private static string Path(GameObject go)
	{
		string p = go.name;
		for (Transform t = go.transform.parent; t != null; t = t.parent)
		{
			p = t.name + "/" + p;
		}
		return p;
	}
}
