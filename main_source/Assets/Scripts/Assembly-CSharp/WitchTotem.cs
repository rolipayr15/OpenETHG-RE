using Characters;
using Debug = UnityEngine.Debug;
using Firebase.Crashlytics;
using Bibloteka;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using System.Globalization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class WitchTotem : MonoBehaviour
{





	[SerializeField]
	private Transform totemModelT;

	[SerializeField]
	private Material material;

	[SerializeField]
	private Material materialTransparent;

	[SerializeField]
	private Material totemMaterialNoShine;

	private ParticleSystem ps;

	private ParticleSystem.EmissionModule emission;

	private int totemType;

	[SerializeField]
	private TotemSound totemSound;

	private Collider coll;

	private string totemId;

	[SerializeField]
	private Renderer totemRenderer;

	private Dictionary<Difficulty, float> potionDurationMultipliers = new Dictionary<Difficulty, float> { { (Difficulty)0, 0.16f }, { (Difficulty)1, 0.2f }, { (Difficulty)2, 0.3f }, { (Difficulty)3, 0.4f }, { (Difficulty)4, 0.5f } };

	private void Start()
	{
		totemId = transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture);
		MessageKit<string, bool>.addObserver(112, RemoteMinigameResult);
		int difficulty = (int)UserGameSettings.currentDifficulty;
		if (difficulty == 3 || difficulty == 4)
		{
			material = totemMaterialNoShine;
			totemModelT.GetComponent<Renderer>().material = totemMaterialNoShine;
		}
		InitIfNeeded();
		ps = GetComponentInChildren<ParticleSystem>();
		Activate();
	}

	private void OnEnable()
	{
		InitIfNeeded();
		Activate();
	}

	private void RemoteMinigameResult(string id, bool result)
	{
		if (id != totemId)
		{
			return;
		}
		MasterAudio.StopAllSoundsOfTransform(transform);
		if (result)
		{
			Deactivate();
			return;
		}
		CustomSingleton<WitchTotemManager>.Instance.WitchTarget = (EnemyTarget)2;
		MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "totem_fail");
		if (gameObject.activeInHierarchy)
		{
			ps.Emit(7);
			StartCoroutine(ExplodeProcedure());
		}
	}

	public void StartMinigame()
	{
		if (FirebaseState.IsInitialized)
		{
			Crashlytics.Log("Witch totem minigame start");
		}
		CustomSingleton<TimingMinigame>.Instance.StartMinigame(OnMinigameFinished);
	}

	private void InitIfNeeded()
	{
		coll = GetComponent<Collider>();
		if (!ps)
		{
			ps = GetComponentInChildren<ParticleSystem>();
		}
	}

	public void Activate()
	{
		InitIfNeeded();
		totemType = 0;
		ParticleSystem.MainModule main = ps.main;
		main.startColor = new Color(0.5f, 0.5f, 0.5f, 0.53f);
		totemModelT.localScale = new Vector3(1f, 1f, 1f);
		totemModelT.Rotate(0f, UnityEngine.Random.Range(0, 360), 0f);
		coll.enabled = true;
	}

	private void SetMaterialRGB(float r, float g, float b, float a)
	{
		if (material != null)
		{
			material.GetColor("_Color");
			material.SetColor("_Color", new Color(r, g, b, a));
		}
	}

	private void SetMaterialAlpha(float a)
	{
		Color color = totemRenderer.material.GetColor("_Color");
		color.a = a;
		totemRenderer.material.SetColor("_Color", color);
	}

	private void OnTriggerEnter(Collider other)
	{
		if (other == null)
		{
			return;
		}
		Debug.Log("Totem trigger enter " + other.tag + " " + other.name);
		if (other.CompareTag("Player"))
		{
			coll.enabled = false;
			StartMinigame();
		}
	}

	private void Explode(bool inflictCurseOnThePlayer)
	{
		if (inflictCurseOnThePlayer)
		{
			MessageKit<int, float>.post(76, totemType, potionDurationMultipliers[UserGameSettings.currentDifficulty]);
			MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "totem_fail");
		}
		ps.Emit(7);
		if (gameObject.activeInHierarchy)
		{
			StartCoroutine(ExplodeProcedure());
		}
	}

	private IEnumerator ExplodeProcedure()
	{
		totemSound.StopSoundLoop();
		yield return ScaleAndAdjustAlphaOverTime(totemModelT.localScale * 1.4f, 0f, 2f);
		CustomSingleton<WitchTotemManager>.Instance.NotifyOfTotemDeactivation(gameObject);
		yield return Utils.GetDelay(2f);
		gameObject.SetActive(false);
		SetMaterialAlpha(1f);
		totemRenderer.material = material;
	}

	private void Deactivate()
	{
		if (gameObject.activeInHierarchy)
		{
			StartCoroutine(DeactivateProcedure());
		}
	}

	private IEnumerator DeactivateProcedure()
	{
		totemSound.StopSoundLoop();
		ps.Emit(7);
		yield return ScaleAndAdjustAlphaOverTime(totemModelT.localScale * 0.9f, 0f, 1f);
		if (!UserGameSettings.IsMultiplayer() || UserGameSettings.isMultiplayerHost)
		{
			CustomSingleton<WitchTotemManager>.Instance.NotifyOfTotemDeactivation(gameObject);
		}
		gameObject.SetActive(false);
		SetMaterialAlpha(1f);
		totemRenderer.material = material;
	}

	private IEnumerator ChangeScaleOverTime(Vector3 targetScale, float overTime)
	{
		Vector3 startScale = totemModelT.localScale;
		yield return Utils.ChangeOverTime(overTime, delegate(float t)
		{
			totemModelT.localScale = Vector3.Lerp(startScale, targetScale, t);
		});
	}

	private IEnumerator ScaleAndAdjustAlphaOverTime(Vector3 targetScale, float targetAlpha, float overTime)
	{
		Vector3 startScale = totemModelT.localScale;
		float startAlpha = totemRenderer.material.color.a;
		yield return Utils.ChangeOverTime(overTime, delegate(float t)
		{
			totemModelT.localScale = Vector3.Lerp(startScale, targetScale, t);
			SetMaterialAlpha(startAlpha + Mathf.Clamp01(t) * (targetAlpha - startAlpha));
		});
		yield return Utils.GetDelay(5f);
		totemModelT.localScale = startScale;
	}

	private void OnDestroy()
	{
		MessageKit<string, bool>.removeObserver(112, RemoteMinigameResult);
	}

	private void OnMinigameFinished(bool result)
	{
		totemRenderer.material = materialTransparent;
		if (!result)
		{
			CustomSingleton<WitchTotemManager>.Instance.WitchTarget = (EnemyTarget)1;
			Explode(true);
			if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log("Witch totem minigame failed");
			}
		}
		else
		{
			Deactivate();
			if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log("Witch totem minigame won");
			}
		}
		if (UserGameSettings.IsMultiplayer())
		{
			CustomSingleton<MultiplayerSettings>.Instance.SendTotemMinigameFinished(result, gameObject.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture));
			if (FirebaseState.IsInitialized)
			{
				Crashlytics.Log("Multiplayer witch totem minigame " + result);
			}
		}
	}
}
