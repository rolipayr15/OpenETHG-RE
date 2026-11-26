using I2.Loc;
using System.Globalization;
namespace Gameplay.Eggs
{
	public static class EggPickupFeedback
	{
		public static string GetMessage(EggEffects player, EggEffectId id)
		{
			string template;
			switch ((int)id)
			{
			case 2:
				template = "Pumpkin: +{amount} coins!";
				break;
			case 3:
				template = "Pumpkin: +{amount} multiplayer keys!";
				break;
			case 5:
				template = "Rotated view for {duration} seconds!";
				break;
			case 6:
				template = "Reversed horizontal look for {duration} seconds!";
				break;
			case 9:
				template = "Left/right movement controls are swapped for {duration} seconds!";
				break;
			case 10:
				template = "Bag counter arithmetic until the end of this game!";
				break;
			case 11:
				template = "Forward/backward movement controls are swapped for {duration} seconds!";
				break;
			default:
				return string.Empty;
			}
			EggEffectEntry found = default(EggEffectEntry);
			if (player != null && player.Effects != null)
			{
				foreach (EggEffectEntry entry in player.Effects)
				{
					if (entry.id == id)
					{
						found = entry;
						break;
					}
				}
			}
			string translated = LocalizationManager.GetTranslation("EggOutcome/" + id, false);
			bool rightToLeft = !string.IsNullOrEmpty(translated) && LocalizationManager.IsRight2Left;
			if (!string.IsNullOrEmpty(translated))
			{
				template = translated;
			}
			string message = template.Replace("{amount}", found.amount.ToString(CultureInfo.CurrentCulture)).Replace("{duration}", found.duration.ToString("0.##", CultureInfo.CurrentCulture));
			return rightToLeft ? LocalizationManager.ApplyRTLfix(message) : message;
		}
	}
}
