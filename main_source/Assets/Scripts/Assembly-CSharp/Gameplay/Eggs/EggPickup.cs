using System.Collections.Generic;
using DarkTonic.MasterAudio;
using Prime31.MessageKit;
using UnityEngine;

namespace Gameplay.Eggs
{
	[DisallowMultipleComponent]
	public sealed class EggPickup : Interactable
	{
		private const string RewardSoundVariation = "pumpkin_reward";

		private const string PrankSoundVariation = "pumpkin_prank";

		[Tooltip("Decorative candle flame; extinguished only after a successful outcome.")]
		public ParticleSystem candleFire;

		[Tooltip("Decorative candle light; extinguished only after a successful outcome.")]
		public Light candleLight;

		[Tooltip("One-shot copy of the totem smoke, spawned independently so it survives collection.")]
		public ParticleSystem prankSmokePrefab;

		[Tooltip("Sound played after receiving coins or multiplayer keys.")]
		public AudioClip rewardSound;

		[Tooltip("Sound played after receiving a negative prank.")]
		public AudioClip prankSound;

		[Tooltip("Restricts this pickup to rewards or pranks; Any preserves the unrestricted random draw.")]
		public EggEffectCategory OutcomeCategory;

		[HideInInspector]
		public bool PrankLastsUntilRoundEnd;

		public bool IsCollected { get; private set; }

		public override void Start()
		{
		}

		public override void Clicked(bool isClickedByOtherPlayer)
		{
			if (!isClickedByOtherPlayer && !IsCollected && isActiveAndEnabled)
			{
				PlayerManager manager = CustomSingleton<PlayerManager>.Instance;
				GameObject player = manager != null ? manager.Player : null;
				if (player != null)
				{
					TryCollect(player.GetComponent<EggEffects>());
				}
			}
			InteractionEnd();
		}

		public override void OtherPlayerInteracts(string remoteItemId)
		{
		}

		public bool TryCollect(EggEffects player)
		{
			if (IsCollected || !isActiveAndEnabled || player == null || !player.IsLocalPlayer || !player.isActiveAndEnabled)
			{
				return false;
			}
			IsCollected = true;
			if (!player.TryApplyRandomEffect(OutcomeCategory, out EggEffectId applied))
			{
				IsCollected = false;
				return false;
			}
			ExtinguishCandle();
			int value = (int)applied;
			if (value >= 0 && value < 12)
			{
				int bit = 1 << value;
				if ((bit & 0xE60) != 0)
				{
					PlayOutcomeSound(prankSound, PrankSoundVariation);
				}
				else if ((bit & 0xC) != 0)
				{
					PlayOutcomeSound(rewardSound, RewardSoundVariation);
				}
			}
			PlaySmokeFeedback();
			PumpkinAnalytics.ReportLantern(applied, player.AmountOf(applied), transform.position);
			MessageKit<string>.post(3, EggPickupFeedback.GetMessage(player, applied));
			return true;
		}

		private void ExtinguishCandle()
		{
			if (candleFire != null)
			{
				candleFire.Stop(true, ParticleSystemStopBehavior.StopEmitting);
			}
			if (candleLight != null)
			{
				candleLight.enabled = false;
			}
		}

		private void PlayOutcomeSound(AudioClip clip, string variationName)
		{
			if (clip == null || MasterAudio.SafeInstance == null || !MasterAudio.SoundsReady)
			{
				return;
			}
			List<MasterAudio.AudioInfo> variations = MasterAudio.GetAllVariationsOfGroup("fx", false);
			if (variations == null)
			{
				return;
			}
			if (!variations.Exists(info => info.Variation != null && info.Variation.name == variationName))
			{
				MasterAudio.CreateGroupVariationFromClip("fx", clip, variationName, 1f, 1f);
			}
			MasterAudio.PlaySound3DAtVector3("fx", transform.position, 1f, null, 0f, variationName);
		}

		private void PlaySmokeFeedback()
		{
			if (prankSmokePrefab == null)
			{
				return;
			}
			ParticleSystem smoke = Instantiate(prankSmokePrefab, transform.TransformPoint(prankSmokePrefab.transform.localPosition), transform.rotation * prankSmokePrefab.transform.localRotation);
			smoke.Play(true);
			smoke.Emit(7);
		}
	}
}
