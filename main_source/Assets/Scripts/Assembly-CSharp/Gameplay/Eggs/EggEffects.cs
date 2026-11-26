using System.Collections.Generic;
using System;
using UnityEngine;

namespace Gameplay.Eggs
{
	[DisallowMultipleComponent]
	public sealed class EggEffects : MonoBehaviour
	{
		public const float DefaultPrankDuration = 60f;

		public EggEffectEntry[] Effects = new EggEffectEntry[] { new EggEffectEntry { id = (EggEffectId)2, weight = 1, amount = 10 }, new EggEffectEntry { id = (EggEffectId)3, weight = 1, amount = 10 }, new EggEffectEntry { id = (EggEffectId)5, weight = 1, duration = 60f }, new EggEffectEntry { id = (EggEffectId)6, weight = 1, duration = 60f }, new EggEffectEntry { id = (EggEffectId)9, weight = 1, duration = 60f }, new EggEffectEntry { id = (EggEffectId)10, weight = 1 }, new EggEffectEntry { id = (EggEffectId)11, weight = 1, duration = 60f } };

		[Tooltip("Dedicated identity child of FPSCamera. EggCameraRollRenderer reads its roll for each local render camera. No other rotation writer or replication.")]
		public Transform CameraRollPivot;

		[HideInInspector]
		public RectTransform HudRollPivot;

		[NonSerialized]
		public Action<int> GrantCoins;

		[NonSerialized]
		public Action<int> GrantMultiplayerKeys;

		private double expiresAt;

		private Transform rotatedCamera;

		private Quaternion cameraRestRotation;

		public bool IsLocalPlayer { get; private set; }

		public bool CanReceiveEgg => IsLocalPlayer && isActiveAndEnabled;

		public EggEffectId ActivePrank { get; private set; }

		public bool HasArithmeticPrank { get; private set; }

		public bool PrankLastsUntilRoundEnd => false;

		public void SetLocalPlayer(bool isLocalPlayer)
		{
			bool refresh = HasArithmeticPrank && IsLocalPlayer != isLocalPlayer;
			if (!isLocalPlayer)
			{
				ClearPrank();
			}
			IsLocalPlayer = isLocalPlayer;
			if (refresh)
			{
				RefreshBagCounter();
			}
		}

		private void Update()
		{
			if ((int)ActivePrank != 0 && expiresAt <= Time.timeAsDouble)
			{
				ClearPrank();
			}
		}

		private void OnDisable()
		{
			ClearPrank();
		}

		public void OnPlayerDied()
		{
			ClearPrank();
		}

		public void OnRoundEnded()
		{
			ClearAllPranks();
		}

		private void OnDestroy()
		{
			if (HasArithmeticPrank && IsLocalPlayer)
			{
				HasArithmeticPrank = false;
				RefreshBagCounter();
			}
		}

		public bool TryApplyRandomEffect(out EggEffectId appliedEffect)
		{
			return TryApplyRandomEffect((EggEffectCategory)0, out appliedEffect);
		}

		public bool TryApplyRandomEffect(EggEffectCategory category, bool prankLastsUntilRoundEnd, out EggEffectId appliedEffect)
		{
			return TryApplyRandomEffect(category, out appliedEffect);
		}

		public bool TryApplyRandomEffect(EggEffectCategory category, out EggEffectId appliedEffect)
		{
			appliedEffect = (EggEffectId)0;
			if (!IsLocalPlayer || !isActiveAndEnabled || Effects == null)
			{
				return false;
			}
			List<EggEffectEntry> candidates = new List<EggEffectEntry>(Effects.Length);
			HashSet<EggEffectId> seen = new HashSet<EggEffectId>();
			long totalWeight = 0L;
			foreach (EggEffectEntry entry in Effects)
			{
				if (seen.Add(entry.id) && MatchesCategory(entry.id, category) && IsAvailable(entry))
				{
					candidates.Add(entry);
					totalWeight += entry.weight;
				}
			}
			if (candidates.Count == 0)
			{
				return false;
			}
			float roll = totalWeight * UnityEngine.Random.value;
			EggEffectId chosen = candidates[candidates.Count - 1].id;
			foreach (EggEffectEntry entry in candidates)
			{
				if (roll < entry.weight)
				{
					chosen = entry.id;
					break;
				}
				roll -= entry.weight;
			}
			if (!ApplyEffect(chosen))
			{
				return false;
			}
			appliedEffect = chosen;
			return true;
		}

		public bool ApplyEffect(EggEffectId id, bool prankLastsUntilRoundEnd)
		{
			return ApplyEffect(id);
		}

		public int AmountOf(EggEffectId id)
		{
			if (Effects != null)
			{
				foreach (EggEffectEntry entry in Effects)
				{
					if (entry.id == id)
					{
						return entry.amount;
					}
				}
			}
			return 0;
		}

		public bool ApplyEffect(EggEffectId id)
		{
			if (!IsLocalPlayer || !isActiveAndEnabled || Effects == null)
			{
				return false;
			}
			EggEffectEntry found = default(EggEffectEntry);
			foreach (EggEffectEntry entry in Effects)
			{
				if (entry.id == id)
				{
					found = entry;
					break;
				}
			}
			if (!IsAvailable(found))
			{
				return false;
			}
			int value = (int)id;
			if (value == 5 || value == 6 || value == 9 || value == 11)
			{
				ClearPrank();
				ActivePrank = id;
				expiresAt = Time.timeAsDouble + found.duration;
				if (value == 5)
				{
					rotatedCamera = CameraRollPivot;
					cameraRestRotation = rotatedCamera.localRotation;
					rotatedCamera.localRotation = cameraRestRotation * Quaternion.Euler(0f, 0f, 90f);
				}
				return true;
			}
			switch (value)
			{
			case 2:
				GrantCoins(found.amount);
				return true;
			case 3:
				GrantMultiplayerKeys(found.amount);
				return true;
			case 10:
				HasArithmeticPrank = true;
				RefreshBagCounter();
				return true;
			default:
				return false;
			}
		}

		public void ClearPrank()
		{
			ActivePrank = (EggEffectId)0;
			expiresAt = 0.0;
			if (rotatedCamera != null)
			{
				rotatedCamera.localRotation = cameraRestRotation;
			}
			rotatedCamera = null;
		}

		public void ClearAllPranks()
		{
			ClearPrank();
			bool had = HasArithmeticPrank;
			HasArithmeticPrank = false;
			if (had && IsLocalPlayer)
			{
				RefreshBagCounter();
			}
		}

		private static void RefreshBagCounter()
		{
			MoneyBagsManager manager = CustomSingleton<MoneyBagsManager>.HasInstance ? CustomSingleton<MoneyBagsManager>.Instance : FindFirstObjectByType<MoneyBagsManager>();
			if (manager != null)
			{
				manager.RefreshBagCounter();
			}
		}

		public Vector2 ModifyLookInput(Vector2 input)
		{
			if (IsLocalPlayer && isActiveAndEnabled && (int)ActivePrank == 6)
			{
				input.x = -input.x;
			}
			return input;
		}

		public float ModifyStrafeInput(float strafeInput)
		{
			return IsLocalPlayer && isActiveAndEnabled && (int)ActivePrank == 9 ? -strafeInput : strafeInput;
		}

		public float ModifyLongitudinalInput(float longitudinalInput)
		{
			return IsLocalPlayer && isActiveAndEnabled && (int)ActivePrank == 11 ? -longitudinalInput : longitudinalInput;
		}

		private static bool MatchesCategory(EggEffectId id, EggEffectCategory category)
		{
			int value = (int)id;
			if ((int)category == 0)
			{
				return true;
			}
			if (value > 11 || value < 0)
			{
				return false;
			}
			int bit = 1 << value;
			return (bit & 0xE60) != 0 ? (int)category == 2 : (bit & 0xC) != 0 && (int)category == 1;
		}

		private bool IsAvailable(EggEffectEntry entry)
		{
			if (entry.weight < 1)
			{
				return false;
			}
			switch ((int)entry.id)
			{
			case 2:
				return entry.amount >= 1 && GrantCoins != null;
			case 3:
				return entry.amount >= 1 && GrantMultiplayerKeys != null;
			case 5:
				return ValidDuration(entry.duration) && CameraRollPivot != null;
			case 6:
			case 9:
			case 11:
				return ValidDuration(entry.duration);
			case 10:
				return !HasArithmeticPrank;
			default:
				return false;
			}
		}

		private static bool ValidDuration(float duration)
		{
			return duration > 0f && !float.IsInfinity(duration) && !float.IsNaN(duration);
		}
	}
}
