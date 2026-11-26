using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu(null)]
[DisallowMultipleComponent]
public sealed class LocalizedFontLayoutBaseline : MonoBehaviour
{
	[SerializeField]
	internal Text Target;

	[SerializeField]
	internal LocalizedFontLayout.Spacing Value;
}
