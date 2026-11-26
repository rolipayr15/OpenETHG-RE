using System;

namespace Multiplayer
{
	[Serializable]
	public struct GameWithToken
	{
		public string gameId;

		public int playerId;

		public string token;

		public string type;

		public string difficulty;

		public string mode;

		public string enemy;

		public string level;
	}
}
