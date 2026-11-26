using UnityEngine;

public class ControlConfig : MonoBehaviour
{
	public enum ControlConfigPlatform
	{
		Standalone = 0,
		Mobile = 1
	}

	public ControlConfigPlatform platform;
}
