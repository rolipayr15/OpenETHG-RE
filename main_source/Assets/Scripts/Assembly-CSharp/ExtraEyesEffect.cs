using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class ExtraEyesEffect : PotionEffect
{

	public override int GetDuration()
	{
		return duration;
	}

	public override IEnumerator StartEffect()
	{
		MessageKit.post(16);
		EyesAnalytics.Breadcrumbs.Log("potion_extra_eyes");
		yield return null;
	}
}
