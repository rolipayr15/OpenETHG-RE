using EyesAnalytics;
using Prime31.MessageKit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

namespace Gameplay.Eggs
{
	[DisallowMultipleComponent]
	[RequireComponent(typeof(EggEffects))]
	public sealed class EggEffectsBinder : MonoBehaviour
	{

		private EggEffects effects;

		private EggCameraRollRenderer[] cameraRenderers;

		private Coroutine bindingRoutine;

		private bool roundEnded;

		private void Awake()
		{
			effects = GetComponent<EggEffects>();
			cameraRenderers = GetComponentsInChildren<EggCameraRollRenderer>(true);
			MessageKit.addObserver(26, OnRoundEnded);
			MessageKit.addObserver(90, OnRoundEnded);
			MessageKit.addObserver(71, OnRoundEnded);
			MessageKit.addObserver(105, OnRoundEnded);
			MessageKit<GameFinishedReason>.addObserver(105, OnMultiplayerFinished);
		}

		private void OnEnable()
		{
			MessageKit.addObserver(17, OnLocalDeath);
			MessageKit.addObserver(95, OnHostDeath);
			MessageKit.addObserver(92, OnGuestDeath);
			bindingRoutine = StartCoroutine(BindLocalPlayer());
		}

		private IEnumerator BindLocalPlayer()
		{
			yield return null;
			bindingRoutine = null;
			if (!isActiveAndEnabled || roundEnded)
			{
				yield break;
			}
			vp_FPPlayerEventHandler handler = GetComponent<vp_FPPlayerEventHandler>();
			if (handler != null && handler.IsLocal.Get() && CustomSingleton<PlayerManager>.Instance.Player == gameObject)
			{
				effects.GrantCoins = delegate(int amount)
				{
					CoinsManager.AddCoins(amount, (CurrencyItemType)2, (CurrencyItemId)40);
				};
				effects.GrantMultiplayerKeys = delegate(int amount)
				{
					if (amount >= 1)
					{
						Currency.SendCurrencyEvent((CurrencyFlowType)1, (CurrencyType)1, amount, (CurrencyItemType)2, (CurrencyItemId)41);
						KeysManager.AddMultiplayerKeys(amount);
					}
				};
				effects.SetLocalPlayer(true);
			}
			else
			{
				effects.SetLocalPlayer(false);
			}
		}

		private void OnLocalDeath()
		{
			effects.ClearPrank();
			RefreshCameras();
		}

		private void OnHostDeath()
		{
			if (UserGameSettings.isMultiplayerHost)
			{
				OnLocalDeath();
			}
		}

		private void OnGuestDeath()
		{
			if (!UserGameSettings.isMultiplayerHost)
			{
				OnLocalDeath();
			}
		}

		private void OnMultiplayerFinished(GameFinishedReason reason)
		{
			OnRoundEnded();
		}

		private void OnRoundEnded()
		{
			roundEnded = true;
			effects.ClearAllPranks();
			effects.SetLocalPlayer(false);
			RefreshCameras();
		}

		private void OnDisable()
		{
			if (bindingRoutine != null)
			{
				StopCoroutine(bindingRoutine);
				bindingRoutine = null;
			}
			MessageKit.removeObserver(17, OnLocalDeath);
			MessageKit.removeObserver(95, OnHostDeath);
			MessageKit.removeObserver(92, OnGuestDeath);
			if (effects != null)
			{
				effects.SetLocalPlayer(false);
			}
			RefreshCameras();
		}

		private void OnDestroy()
		{
			MessageKit.removeObserver(26, OnRoundEnded);
			MessageKit.removeObserver(90, OnRoundEnded);
			MessageKit.removeObserver(71, OnRoundEnded);
			MessageKit.removeObserver(105, OnRoundEnded);
			MessageKit<GameFinishedReason>.removeObserver(105, OnMultiplayerFinished);
		}

		private void RefreshCameras()
		{
			if (cameraRenderers == null)
			{
				return;
			}
			foreach (EggCameraRollRenderer renderer in cameraRenderers)
			{
				if (renderer != null)
				{
					renderer.RefreshView();
				}
			}
		}
	}
}
