internal readonly struct WinbackRewardAvailability
{
	public bool HasChapter2Access { get; }

	public bool HasChapter3Access { get; }

	public WinbackRewardAvailability(bool hasChapter2Access, bool hasChapter3Access)
	{
		HasChapter2Access = false;
		HasChapter3Access = false;
	}
}
