using EyesAnalytics;
using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class LimitedVisionEffect : PotionEffect
{


	private const float LimitedFallOffValue = 0.3f;

	private float AuthoredFallOff => EyesLightingComposer.AuthoredFalloffFactor;

	public override IEnumerator StartEffect()
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", AuthoredFallOff, "to", LimitedFallOffValue, "time", 0.3f, "onupdate", "ChangeValue"));
		MessageKit<bool>.post(11, true);
		Breadcrumbs.Log("potion_limited_vision");
		yield return Utils.GetDelay(0.3f);
	}

	public override IEnumerator EndEffect()
	{
		iTween.ValueTo(gameObject, iTween.Hash("from", LimitedFallOffValue, "to", AuthoredFallOff, "time", 0.3f, "onupdate", "ChangeValue"));
		MessageKit<bool>.post(11, false);
		yield return Utils.GetDelay(0.3f);
		EyesLightingComposer.SetFalloffFactorOverride(null);
	}

	private void ChangeValue(float f)
	{
		EyesLightingComposer.SetFalloffFactorOverride(f);
	}

	private void OnDestroy()
	{
		EyesLightingComposer.SetFalloffFactorOverride(null);
	}
}
