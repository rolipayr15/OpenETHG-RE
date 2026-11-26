namespace ExternalApi.Models
{
	public struct StartGameData
	{
		public string gameId;

		public Difficulty diff;

		public Enemy enemy;

		public Level level;

		public Mode mapMode;
	}
}
