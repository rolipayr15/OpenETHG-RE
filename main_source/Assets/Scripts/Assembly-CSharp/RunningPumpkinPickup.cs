using UnityEngine;
using Prime31.MessageKit;
using System.Text;

public class RunningPumpkinPickup : Interactable
{
	private int multiplier = 1;

	private StringBuilder sb = new StringBuilder();

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		multiplier++;
		if (!isClickedByOtherPlayer)
		{
			Eyes.DailyChallenges.DailyChallengeRuntime.PumpkinCaught(this);
			EyesAnalytics.Game.PumpkinCollected(EyesAnalytics.PumpkinKind.Running, EyesAnalytics.PumpkinOutcome.CoinMultiplier, (int)Time.timeSinceLevelLoad, (double)multiplier, Gameplay.Eggs.PumpkinAnalytics.FloorOf(transform.position));
		}
		sb.Length = 0;
		sb.Append(I2.Loc.ScriptLocalization.WinCoinMultiplier);
		sb.Append(": x");
		sb.Append(multiplier);
		MessageKit<string>.post(3, sb.ToString());
		MessageKit.post(21);
		UserGameSettings.halloweenMultiplier = multiplier;
		InteractionEnd();
	}
}
