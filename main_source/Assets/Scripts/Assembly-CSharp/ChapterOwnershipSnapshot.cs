internal readonly struct ChapterOwnershipSnapshot
{
	public bool OwnsChapter2 { get; }

	public bool OwnsChapter3 { get; }

	public ChapterOwnershipSnapshot(bool ownsChapter2, bool ownsChapter3)
	{
		OwnsChapter2 = false;
		OwnsChapter3 = false;
	}
}
