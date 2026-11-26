public class CoroutineManager : CustomSingleton<CoroutineManager>
{
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
