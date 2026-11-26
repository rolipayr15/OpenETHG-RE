using Prime31.MessageKit;
using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using Eyes.DailyChallenges;
using UnityEngine;

public class PotionEffectManager : CustomSingleton<PotionEffectManager>
{

	public PotionEffect[] basicEffects;

	public PotionEffect[] compoundEffects;

	[SerializeField]
	private Material redBottle;

	[SerializeField]
	private Material greenBottle;

	[SerializeField]
	private Material redBottleNoShine;

	[SerializeField]
	private Material greenBottleNoShine;

	[SerializeField]
	private Material redIcon;

	[SerializeField]
	private Material greenIcon;

	[SerializeField]
	private Texture2D greenBottleTexture;

	[SerializeField]
	private Texture2D redBottleTexture;

	private PotionEffect currentEffect;

	private int basicEffectsCount;

	private void InitPotionsImages(int greenId)
	{
		GameObject[] first = GameObject.FindGameObjectsWithTag("Potion0");
		GameObject[] second = GameObject.FindGameObjectsWithTag("Potion1");
		for (int i = 0; i < first.Length; i++)
		{
			first[i].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", greenId == 0 ? greenBottleTexture : redBottleTexture);
			second[i].GetComponent<MeshRenderer>().material.SetTexture("_MainTex", greenId == 0 ? redBottleTexture : greenBottleTexture);
		}
	}

	public PotionInitialData ActivateInitialPotions()
	{
		int greenId = UnityEngine.Random.Range(0, 2);
		PotionInitialData data = new PotionInitialData(greenId);
		InitPotionsImages(greenId);
		GameObject[] potions = GameObject.FindGameObjectsWithTag("Potion");
		int removed = 0;
		while (removed < potions.Length - 20)
		{
			GameObject candidate = potions[UnityEngine.Random.Range(0, potions.Length)];
			if (candidate.activeInHierarchy)
			{
				candidate.SetActive(false);
				removed++;
			}
		}
		foreach (GameObject potion in potions)
		{
			if (potion.activeInHierarchy)
			{
				int colorId = UnityEngine.Random.Range(0, 2);
				potion.GetComponent<Potion>().SetId(colorId);
				potion.GetComponent<MeshRenderer>().material = colorId == greenId ? greenBottle : redBottle;
				data.AddPotion(potion, colorId);
			}
		}
		return data;
	}

	private void Start()
	{
		basicEffectsCount = basicEffects.Length;
		int difficulty = (int)UserGameSettings.currentDifficulty;
		if (difficulty == 3 || difficulty == 4)
		{
			redBottle = redBottleNoShine;
			greenBottle = greenBottleNoShine;
		}
		MessageKit.addObserver(9, msg_PotionExpired);
		MessageKit<int>.addObserver(15, msg_ActivatePotion);
		MessageKit<int, float>.addObserver(76, msg_ActivateWitchTotemVersionOfTheEffect);
		if (!UserGameSettings.IsMultiplayer())
		{
			ActivateInitialPotions();
		}
	}

	public void ActivatePotionsFromHost(int greenId, List<string> potionsToActivate)
	{
		InitPotionsImages(greenId);
		List<string> ids = new List<string>();
		Dictionary<string, int> colors = new Dictionary<string, int>();
		if (potionsToActivate != null)
		{
			foreach (string entry in potionsToActivate)
			{
				string[] parts = entry.Split(new char[] { ':' }, 2);
				if (parts.Length == 2)
				{
					int colorId = int.Parse(parts[0]);
					ids.Add(parts[1]);
					colors.Add(parts[1], colorId);
				}
			}
		}
		foreach (GameObject potion in GameObject.FindGameObjectsWithTag("Potion"))
		{
			potion.SetActive(false);
			string id = potion.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture);
			if (ids.Contains(id))
			{
				potion.SetActive(true);
				int colorId = colors[id];
				potion.GetComponent<Potion>().SetId(colorId);
				potion.GetComponent<MeshRenderer>().material = colorId == greenId ? greenBottle : redBottle;
			}
		}
	}

	private void msg_ActivateWitchTotemVersionOfTheEffect(int potionId, float durationMultiplier)
	{
		StartCoroutine(ActivatePotionEffect(potionId, true, durationMultiplier));
	}

	private void msg_ActivatePotion(int potionId)
	{
		StartCoroutine(ActivatePotionEffect(potionId));
	}

	private void msg_PotionExpired()
	{
		if (currentEffect != null)
		{
			StartCoroutine(currentEffect.EndEffect());
			currentEffect = null;
		}
	}

	private IEnumerator ActivatePotionEffect(int potionId, bool ignoreEffectStacking = false, float durationMultiplier = 1f)
	{
		if (currentEffect == null)
		{
			currentEffect = basicEffects[potionId];
		}
		else
		{
			yield return StartCoroutine(currentEffect.EndEffect());
			if (ignoreEffectStacking)
			{
				currentEffect = basicEffects[potionId];
			}
			else
			{
				int index = Array.IndexOf(basicEffects, currentEffect);
				currentEffect = index == -1 ? basicEffects[potionId] : compoundEffects[potionId + basicEffectsCount * index];
			}
		}
		PotionEffect effect = currentEffect;
		MessageKit<int>.post(8, (int)(durationMultiplier * effect.GetDuration()));
		Coroutine running = StartCoroutine(effect.StartEffect());
		if (!ignoreEffectStacking)
		{
			PositivePotionEffect positive = GetPositivePotionEffect(effect);
			if (positive != PositivePotionEffect.None)
			{
				DailyChallengeRuntime.PositivePotionEffectApplied(this, positive);
			}
		}
		yield return running;
		if (this != null && currentEffect.GetDuration() == 0)
		{
			currentEffect = null;
		}
	}

	private static PositivePotionEffect GetPositivePotionEffect(PotionEffect effect)
	{
		if (effect is ExtraEyesEffect)
		{
			return (PositivePotionEffect)1;
		}
		if (effect is CloakEffect)
		{
			return (PositivePotionEffect)2;
		}
		if (effect is SprintEffect)
		{
			return (PositivePotionEffect)4;
		}
		return effect is MoneyBagsVisionEffect ? (PositivePotionEffect)8 : PositivePotionEffect.None;
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(9, msg_PotionExpired);
		MessageKit<int>.removeObserver(15, msg_ActivatePotion);
		MessageKit<int, float>.removeObserver(76, msg_ActivateWitchTotemVersionOfTheEffect);
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
