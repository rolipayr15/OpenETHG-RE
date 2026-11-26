using UnityEngine;

public class LightConfig : MonoBehaviour
{
	public enum Type
	{
		OnlyNormal = 0,
		OnlySpecial = 1
	}

	public Type type;
}
