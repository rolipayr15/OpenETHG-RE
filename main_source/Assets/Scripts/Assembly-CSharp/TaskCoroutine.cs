using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public static class TaskCoroutine
{
	public static IEnumerator ToCoroutine<T>(this Task<T> task, MonoBehaviour owner, Action<T> onDone)
	{
		yield return new WaitUntil(() => !owner || task.IsCompleted);
		if (!owner || task.IsCanceled)
		{
			yield break;
		}
		if (task.IsFaulted)
		{
			Debug.Log(task.Exception.ToString());
		}
		else
		{
			onDone?.Invoke(task.Result);
		}
	}
}
