using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;

public class SprintEffect : PotionEffect
{


	private vp_FPController controller;

	private void Start()
	{
		controller = CustomSingleton<PlayerManager>.Instance.Player.GetComponent<vp_FPController>();
	}

	public override IEnumerator StartEffect()
	{
		MessageKit<bool>.post(13, true);
		EyesAnalytics.Breadcrumbs.Log("potion_sprint");
		yield return null;
	}

	public override IEnumerator EndEffect()
	{
		MessageKit<bool>.post(13, false);
		yield return null;
	}
}
