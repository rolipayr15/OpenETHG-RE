using System;

namespace Multiplayer
{
	[Serializable]
	public struct GamePayload
	{
		public string type;

		public string difficulty;

		public string mode;

		public string enemy;

		public string level;

		public GamePayload(string _type, string _difficulty, string _mode, string _enemy, string _level)
		{
			type = _type;
			difficulty = _difficulty;
			mode = _mode;
			enemy = _enemy;
			level = _level;
		}
	}
}
