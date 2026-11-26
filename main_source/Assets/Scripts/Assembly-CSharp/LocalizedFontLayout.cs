using System;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

// The "Bokor" display font has tall line metrics; labels using it get 0.8x line spacing. The authored spacing is
// remembered per label (and on a hidden LocalizedFontLayoutBaseline component, so clones keep it) so switching
// language/font back and forth never compounds the multiplier.
public static class LocalizedFontLayout
{
	[Serializable]
	internal sealed class Spacing
	{
		public float Original;

		public float LastApplied;
	}

	public const float BokorLineSpacingMultiplier = 0.8f;

	private static ConditionalWeakTable<Text, Spacing> spacing = new ConditionalWeakTable<Text, Spacing>();

	public static void SetFont(Text label, Font font)
	{
		if (label == null)
		{
			return;
		}
		if (font != null)
		{
			label.font = font;
		}
		Apply(label);
	}

	public static void Apply(Text label)
	{
		if (!Application.isPlaying || label == null)
		{
			return;
		}
		bool notBokor = label.font == null || !string.Equals(label.font.name, "Bokor", StringComparison.Ordinal);
		if (!TryGetSpacing(label, out Spacing value))
		{
			if (notBokor)
			{
				return;
			}
			value = Remember(label, label.lineSpacing);
		}
		if (value != null)
		{
			value.LastApplied = (notBokor ? 1f : BokorLineSpacingMultiplier) * value.Original;
			label.lineSpacing = value.LastApplied;
		}
	}

	// Instantiated copies must start from the source's authored spacing, not its already-scaled one.
	public static void CopyBaselines(GameObject source, GameObject clone)
	{
		if (!Application.isPlaying || source == null || clone == null)
		{
			return;
		}
		Text[] sourceLabels = source.GetComponentsInChildren<Text>(true);
		Text[] cloneLabels = clone.GetComponentsInChildren<Text>(true);
		int count = Mathf.Min(sourceLabels.Length, cloneLabels.Length);
		for (int i = 0; i < count; i++)
		{
			float original = TryGetSpacing(sourceLabels[i], out Spacing sourceSpacing) ? sourceSpacing.Original : sourceLabels[i].lineSpacing;
			if (TryGetSpacing(cloneLabels[i], out Spacing cloneSpacing))
			{
				cloneSpacing.Original = original;
			}
			else
			{
				Remember(cloneLabels[i], original);
			}
			Apply(cloneLabels[i]);
		}
	}

	private static bool TryGetSpacing(Text label, out Spacing value)
	{
		if (spacing.TryGetValue(label, out value))
		{
			return true;
		}
		if (label.TryGetComponent(out LocalizedFontLayoutBaseline baseline) && baseline.Target == label && baseline.Value != null)
		{
			value = baseline.Value;
			Track(label, value);
			return true;
		}
		value = null;
		return false;
	}

	private static Spacing Remember(Text label, float original)
	{
		Spacing value = new Spacing
		{
			Original = original,
			LastApplied = original
		};
		if (!label.TryGetComponent(out LocalizedFontLayoutBaseline baseline))
		{
			baseline = label.gameObject.AddComponent<LocalizedFontLayoutBaseline>();
			baseline.hideFlags = HideFlags.HideInInspector | HideFlags.DontSaveInEditor;
		}
		baseline.Target = label;
		baseline.Value = value;
		Track(label, value);
		return value;
	}

	private static void Track(Text label, Spacing value)
	{
		spacing.Add(label, value);
	}
}
