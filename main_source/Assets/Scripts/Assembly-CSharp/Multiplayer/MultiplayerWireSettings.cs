using System;

namespace Multiplayer
{
	public static class MultiplayerWireSettings
	{
		private const string GooseWireEnemy = "krasue";

		private const string GooseWireMode = "halloween";

		public static (string, string) ToWire(Enemy enemy, Mode mode)
		{
			if ((int)enemy == 7)
			{
				return ("krasue", "halloween");
			}
			return (enemy.ToString().ToLower(), mode.ToString().ToLower());
		}

		public static (Enemy, Mode) FromWire(string enemy, string mode)
		{
			if (string.Equals(enemy, "krasue", StringComparison.OrdinalIgnoreCase) && string.Equals(mode, "halloween", StringComparison.OrdinalIgnoreCase))
			{
				return ((Enemy)7, (Mode)2);
			}
			Enemy parsedEnemy = default(Enemy);
			if (!string.IsNullOrEmpty(enemy) && !char.IsDigit(enemy[0]))
			{
				Enum.TryParse(enemy, true, out parsedEnemy);
			}
			Mode parsedMode = default(Mode);
			if (!string.IsNullOrEmpty(mode) && !char.IsDigit(mode[0]))
			{
				Enum.TryParse(mode, true, out parsedMode);
			}
			return (parsedEnemy, parsedMode);
		}

		public static string ToWire(GameMode gameMode)
		{
			return gameMode.ToString().ToLower();
		}

		public static string ToWire(Difficulty difficulty)
		{
			return difficulty.ToString().ToLower();
		}

		public static string ToWire(Level level)
		{
			return level.ToString().ToLower();
		}
	}
}
