namespace Eyes.DailyChallenges
{
	public sealed class DailyChallengeDefinition
	{
		public string Id { get; }

		public DailyChallengeKind Kind { get; }

		public int Parameter { get; }

		public int Goal { get; }

		public string Title { get; }

		public string Description { get; }

		public string TitleTerm { get; }

		public string DescriptionTerm { get; }

		public string ShortDescription { get; }

		public string ShortDescriptionTerm { get; }

		internal DailyChallengeDefinition(string id, DailyChallengeKind kind, int parameter, int goal, string title, string description, string termKey, string shortDescription)
		{
			Id = id;
			Kind = kind;
			Parameter = parameter;
			Goal = goal;
			Title = title;
			Description = description;
			TitleTerm = "DailyChallenges/" + termKey + "/Title";
			DescriptionTerm = "DailyChallenges/" + termKey + "/Description";
			ShortDescription = shortDescription;
			ShortDescriptionTerm = "DailyChallenges/" + termKey + "/ShortDescription";
		}
	}
}
