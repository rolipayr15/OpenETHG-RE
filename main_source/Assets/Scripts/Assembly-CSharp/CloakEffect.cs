using EyesAnalytics;
using Bibloteka;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class CloakEffect : PotionEffect
{


	[SerializeField]
	private Color cloakFalloffColor;

	private Camera cameraGirl;

	private void Start()
	{
		if ((int)UserGameSettings.currentEnemy == 4)
		{
			return;
		}
		if (UserGameSettings.IsMultiplayer() && !UserGameSettings.isMultiplayerHost)
		{
			cameraGirl = CustomSingleton<Characters.EnemyAvatar.EnemyAvatar>.Instance.GetCamera();
		}
		else
		{
			cameraGirl = CustomSingleton<EnemyManager>.Instance.CurrentEnemy.EnemyVision.EnemyCamera;
		}
	}

	public override IEnumerator StartEffect()
	{
		MessageKit<bool>.post(14, true);
		iTween.ValueTo(gameObject, iTween.Hash("from", 0f, "to", 1f, "time", 0.3f, "onupdate", "ChangeColor"));
		Breadcrumbs.Log("potion_cloak");
		yield return Utils.GetDelay(0.3f);
	}

	public override IEnumerator EndEffect()
	{
		MessageKit<bool>.post(14, false);
		iTween.ValueTo(gameObject, iTween.Hash("from", 1f, "to", 0f, "time", 0.3f, "onupdate", "ChangeColor"));
		yield return Utils.GetDelay(0.3f);
		EyesLightingComposer.SetFalloffColorOverride(null);
	}

	private void ChangeColor(float t)
	{
		Color color = Color.Lerp(EyesLightingComposer.AuthoredFalloffColor, cloakFalloffColor, t);
		EyesLightingComposer.SetFalloffColorOverride(color);
		if ((int)UserGameSettings.currentEnemy != 4 && cameraGirl != null)
		{
			cameraGirl.backgroundColor = color;
		}
	}

	private void OnDestroy()
	{
		EyesLightingComposer.SetFalloffColorOverride(null);
		if (cameraGirl != null)
		{
			cameraGirl.backgroundColor = EyesLightingComposer.AuthoredFalloffColor;
		}
	}
}
