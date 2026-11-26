using System;

namespace Multiplayer
{
	[Serializable]
	public struct MultiGame
	{
		public string gameId;

		public int playerId;

		public string type;

		public string difficulty;

		public string mode;

		public string enemy;

		public string level;

		public MultiGame(string _gameId, int _playerId, string _type, string _difficulty, string _mode, string _enemy, string _level)
		{
			gameId = _gameId;
			playerId = _playerId;
			type = _type;
			difficulty = _difficulty;
			mode = _mode;
			enemy = _enemy;
			level = _level;
		}
	}
}
