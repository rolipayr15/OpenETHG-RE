namespace UnityEngine.UI.Extensions
{
	[RequireComponent(typeof(RectTransform))]
	[ExecuteInEditMode]
	[AddComponentMenu("UI/Effects/Extensions/UIMultiplyEffect")]
	public class UIMultiplyEffect : MonoBehaviour
	{
		private MaskableGraphic mGraphic;

		private void Start()
		{
			SetMaterial();
		}

		public void SetMaterial()
		{
			mGraphic = GetComponent<MaskableGraphic>();
			if (mGraphic != null)
			{
				if (mGraphic.material == null || mGraphic.material.name == "Default UI Material")
				{
					mGraphic.material = new Material(Shader.Find("UI Extensions/UIMultiply"));
				}
			}
			else
			{
				Debug.LogError("Please attach component to a Graphical UI component");
			}
		}

		public void OnValidate()
		{
			SetMaterial();
		}
	}
}
