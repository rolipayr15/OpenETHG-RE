using UnityEngine.SceneManagement;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

namespace Bibloteka
{
    // Token: 0x0200063B RID: 1595
    public static class Utils
    {
        // Token: 0x060035DF RID: 13791 RVA: 0x0000CC76 File Offset: 0x0000AE76
        public static void Shuffle<T>(this IList<T> list)
        {
        	int n = list.Count;
        	while (n > 1)
        	{
        		int k = rng.Next(n);
        		n--;
        		T value = list[k];
        		list[k] = list[n];
        		list[n] = value;
        	}
        }

        // Token: 0x060035E0 RID: 13792 RVA: 0x0000CC78 File Offset: 0x0000AE78
        public static string GetId(GameObject gameObject)
        {
        	return gameObject.transform.position.sqrMagnitude.ToString(System.Globalization.CultureInfo.InvariantCulture);
        }

        // Token: 0x060035E1 RID: 13793 RVA: 0x0000CC7B File Offset: 0x0000AE7B
        public static bool Approximately(float a, float b, float tolerance = 0.0001f)
        {
        	return Mathf.Abs(a - b) < tolerance;
        }

        // Token: 0x060035E2 RID: 13794 RVA: 0x0000CC7E File Offset: 0x0000AE7E
        public static float Map(float value, float low1, float high1, float low2, float high2)
        {
        	if (value > high1)
        	{
        		return high2;
        	}
        	if (value < low1)
        	{
        		return low2;
        	}
        	return (value - low1) / (high1 - low1) * (high2 - low2) + low2;
        }

        // Token: 0x060035E3 RID: 13795 RVA: 0x0000CC85 File Offset: 0x0000AE85
        public static IEnumerator WaitForRealSeconds(float t)
        {
        	while (t > 0f)
        	{
        		t -= Time.unscaledDeltaTime;
        		yield return null;
        	}
        }

        // Token: 0x060035E4 RID: 13796 RVA: 0x0000CC88 File Offset: 0x0000AE88
        public static IEnumerator ChangeOverTime(float time, Action<float> f)
        {
        	float elapsedTime = 0f;
        	while (elapsedTime < time)
        	{
        		f(elapsedTime / time);
        		yield return null;
        		elapsedTime += Time.deltaTime;
        	}
        	f(1f);
        }

        // Token: 0x060035E5 RID: 13797 RVA: 0x0000CC8B File Offset: 0x0000AE8B
        public static IEnumerator ChangeWithSpeed(float speed, Action<float> f)
        {
        	float t = 0f;
        	while (t < 1f)
        	{
        		f(t);
        		yield return null;
        		t += speed * Time.deltaTime;
        	}
        	f(1f);
        }

        // Token: 0x060035E6 RID: 13798 RVA: 0x0000CC8E File Offset: 0x0000AE8E
        public static WaitForSeconds GetDelay(float time)
        {
        	if (!dict.TryGetValue(time, out WaitForSeconds delay))
        	{
        		delay = new WaitForSeconds(time);
        		dict.Add(time, delay);
        	}
        	return delay;
        }

        // Token: 0x060035E7 RID: 13799 RVA: 0x0000CC91 File Offset: 0x0000AE91
        public static bool Probability(int chance)
        {
        	return UnityEngine.Random.Range(0, 101) < chance;
        }

        // Token: 0x060035E8 RID: 13800 RVA: 0x0000CC94 File Offset: 0x0000AE94
        public static void ShowToast(string toastString)
        {
        	if (string.IsNullOrEmpty(toastString))
        	{
        		Debug.Log("toastString jest null lub pusty!");
        		return;
        	}
        	#if UNITY_ANDROID && !UNITY_EDITOR
        	using (AndroidJavaClass version = new AndroidJavaClass("android.os.Build$VERSION"))
        	{
        		int sdk = version.GetStatic<int>("SDK_INT");
        		if (sdk < 26)
        		{
        			Firebase.Crashlytics.Crashlytics.Log(string.Format("Skipping native Toast on Android SDK {0}: {1}", sdk, toastString));
        			return;
        		}
        	}
        	AndroidJavaClass player = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
        	AndroidJavaObject currentActivity = player.GetStatic<AndroidJavaObject>("currentActivity");
        	if (currentActivity == null)
        	{
        		Firebase.Crashlytics.Crashlytics.Log("currentActivity jest NULL! Spróbuj uruchomić kod później.");
        		return;
        	}
        	currentActivity.Call("runOnUiThread", new AndroidJavaRunnable(delegate
        	{
        		using (AndroidJavaObject context = currentActivity.Call<AndroidJavaObject>("getApplicationContext"))
        		using (AndroidJavaClass toastClass = new AndroidJavaClass("android.widget.Toast"))
        		using (AndroidJavaObject toast = toastClass.CallStatic<AndroidJavaObject>("makeText", context, toastString, toastClass.GetStatic<int>("LENGTH_SHORT")))
        		{
        			toast.Call("show");
        		}
        	}));
        	#endif
        }

        // Token: 0x060035E9 RID: 13801 RVA: 0x0000CC96 File Offset: 0x0000AE96
        public static float RoundToNearest(float val, float roundTo)
        {
        	return Mathf.Round(val / roundTo) * roundTo;
        }

        // Token: 0x060035EA RID: 13802 RVA: 0x0000CC9D File Offset: 0x0000AE9D
        public static void LoadNextSceneInBuildOrder()
        {
        	SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

        // Token: 0x060035EB RID: 13803 RVA: 0x0000CC9F File Offset: 0x0000AE9F
        public static UnityEngine.AsyncOperation LoadNextSceneInBuildOrderAsync()
        {
        	return SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex + 1);
        }

        // Token: 0x060035EC RID: 13804 RVA: 0x0000CCA2 File Offset: 0x0000AEA2
        public static string FormatTime(float t)
        {
        	TimeSpan span = TimeSpan.FromSeconds(t);
        	return string.Format("{0:D2}:{1:D2}", span.Minutes, span.Seconds);
        }

        // Token: 0x060035ED RID: 13805 RVA: 0x0000CCA5 File Offset: 0x0000AEA5
        public static void CopyToClipboard(this string str)
        {
        	GUIUtility.systemCopyBuffer = str;
        }

        // Token: 0x060035EE RID: 13806 RVA: 0x0000CCA7 File Offset: 0x0000AEA7
        public static string FloatToString(float f)
        {
        	return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F2}", f);
        }

        // Token: 0x060035EF RID: 13807 RVA: 0x0000CCAA File Offset: 0x0000AEAA
        public static float ParseStringToFloat(string f)
        {
        	return float.Parse(f, System.Globalization.CultureInfo.InvariantCulture);
        }

        // Token: 0x060035F0 RID: 13808 RVA: 0x0000CCB1 File Offset: 0x0000AEB1
        public static string Vector3ToString(Vector3 vector)
        {
        	return string.Format(System.Globalization.CultureInfo.InvariantCulture, "{0:F2},{1:F2},{2:F2}", vector.x, vector.y, vector.z);
        }

        // Token: 0x060035F1 RID: 13809 RVA: 0x00011D90 File Offset: 0x0000FF90
        public static Vector3 ParseStringToVector3(string vectorString)
        {
        	if (vectorString.StartsWith('(') && vectorString.EndsWith(')'))
        	{
        		vectorString = vectorString.Substring(1, vectorString.Length - 2);
        	}
        	string[] parts = vectorString.Split(',');
        	System.Globalization.CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
        	return new Vector3(float.Parse(parts[0], culture), float.Parse(parts[1], culture), float.Parse(parts[2], culture));
        }

        // Token: 0x060035F2 RID: 13810 RVA: 0x0000CCB4 File Offset: 0x0000AEB4
        public static bool IsSceneLoaded(string sceneName)
        {
        	return SceneManager.GetSceneByName(sceneName).isLoaded;
        }

        // Token: 0x04002B40 RID: 11072
        private static System.Random rng = new System.Random();

        // Token: 0x04002B41 RID: 11073
        private static Dictionary<float, WaitForSeconds> dict = new Dictionary<float, WaitForSeconds>();

        // Token: 0x04002B42 RID: 11074
        public static WaitForEndOfFrame WaitForEndOfFrame = new WaitForEndOfFrame();
    }
}
