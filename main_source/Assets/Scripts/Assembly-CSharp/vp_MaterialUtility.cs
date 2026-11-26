using UnityEngine;

public static class vp_MaterialUtility
{
	public static void MakeMaterialTransparent(Material material)
	{
		material.SetFloat("_Mode", 2f);
		material.SetInt("_SrcBlend", 5);
		material.SetInt("_DstBlend", 10);
		material.SetInt("_ZWrite", 0);
		material.DisableKeyword("_ALPHATEST_ON");
		material.EnableKeyword("_ALPHABLEND_ON");
		material.DisableKeyword("_ALPHAPREMULTIPLY_ON");
		material.renderQueue = 3000;
	}

	public static string GetColorPropertyName(Material material)
	{
		if (material.HasProperty("_TintColor"))
		{
			return "_TintColor";
		}
		return "_MainColor";
	}
}
