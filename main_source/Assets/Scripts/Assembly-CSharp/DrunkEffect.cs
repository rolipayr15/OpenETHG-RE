using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class DrunkEffect : PotionEffect
{


	public override IEnumerator StartEffect()
	{
		CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Drunk", true, true, false);
		MessageKit<bool>.post(12, true);
		EyesAnalytics.Breadcrumbs.Log("potion_drunk");
		yield return null;
	}

	public override IEnumerator EndEffect()
	{
		CustomSingleton<PlayerManager>.Instance.playerHandler.SetState("Drunk", false, true, false);
		MessageKit<bool>.post(12, false);
		yield return null;
	}
}
