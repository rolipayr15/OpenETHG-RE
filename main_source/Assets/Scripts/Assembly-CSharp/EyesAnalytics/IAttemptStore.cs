namespace EyesAnalytics
{
	internal interface IAttemptStore
	{
		string Load();

		void Save(string json);

		void Clear();
	}
}
