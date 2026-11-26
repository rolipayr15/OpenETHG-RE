using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Batch-mode close-up of one scene object, for checking materials without playing the level.
// RECOVERY_SHOT_SCENE=Assets/scenes/Hospital.unity RECOVERY_SHOT_OBJECTS=first,second
// writes E:/eyes/logs/shots/obj_<name>.jpg and logs the renderer's material setup.
public static class RecoveryObjectShot
{
	public static void Run()
	{
		string scene = System.Environment.GetEnvironmentVariable("RECOVERY_SHOT_SCENE");
		string[] names = (System.Environment.GetEnvironmentVariable("RECOVERY_SHOT_OBJECTS") ?? "").Split(',');
		EditorSceneManager.OpenScene(scene);
		Shader.SetGlobalFloat("_FalloffScale", 0.1f);
		Shader.SetGlobalFloat("_FalloffOffset", -0.76f);
		Shader.SetGlobalColor("_FalloffColor", Color.black);
		Shader.SetGlobalFloat("_DesaturationLevel", 1f);
		Directory.CreateDirectory("E:/eyes/logs/shots");
		foreach (string name in names)
		{
			foreach (Renderer r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
			{
				if (r.name != name)
				{
					continue;
				}
				Material m = r.sharedMaterial;
				Texture tex = m != null && m.HasProperty("_MainTex") ? m.GetTexture("_MainTex") : null;
				Debug.Log("[Shot] " + name + " active=" + r.gameObject.activeInHierarchy + " enabled=" + r.enabled + " material=" + (m != null ? m.name + "/" + m.shader.name : "-") + " tex=" + (tex != null ? tex.name + " " + tex.width + "x" + tex.height + " " + (tex is Texture2D t2 ? t2.format.ToString() : "") : "-") + " lightmap=" + r.lightmapIndex + " bounds=" + r.bounds);
				GameObject go = new GameObject("ShotCamera");
				Camera cam = go.AddComponent<Camera>();
				Transform t = r.transform;
				Vector3 normal = -t.forward;
				cam.transform.position = r.bounds.center + normal * (r.bounds.extents.magnitude * 2.5f + 0.3f);
				cam.transform.LookAt(r.bounds.center);
				cam.nearClipPlane = 0.01f;
				cam.clearFlags = CameraClearFlags.SolidColor;
				cam.backgroundColor = new Color(0.1f, 0.1f, 0.3f);
				RenderTexture rt = RenderTexture.GetTemporary(512, 512, 24);
				cam.targetTexture = rt;
				cam.Render();
				RenderTexture.active = rt;
				Texture2D img = new Texture2D(512, 512, TextureFormat.RGB24, false);
				img.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
				img.Apply();
				RenderTexture.active = null;
				File.WriteAllBytes("E:/eyes/logs/shots/obj_" + name + ".jpg", img.EncodeToJPG(85));
				cam.targetTexture = null;
				RenderTexture.ReleaseTemporary(rt);
				Object.DestroyImmediate(go);
				break;
			}
		}
		EditorApplication.Exit(0);
	}
}
