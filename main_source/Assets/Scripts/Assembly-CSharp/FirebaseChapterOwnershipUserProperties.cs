using System;

public static class FirebaseChapterOwnershipUserProperties
{
	internal const string Chapter2PropertyName = "owns_chapter_2";

	internal const string Chapter3PropertyName = "owns_chapter_3";

	private const string OwnedValue = "true";

	private const string NotOwnedValue = "false";

	private static bool entitlementsHydrated;

	public static void MarkEntitlementsHydrated()
	{
	}

	public static void Sync()
	{
	}

	internal static ChapterOwnershipSnapshot GetCurrentSnapshot()
	{
		return default;
	}

	internal static ChapterOwnershipSnapshot Resolve(bool ownsChapter2Bundle, bool ownsCharlie, bool ownsHospital, bool ownsChapter3Bundle, bool ownsGoodBoy, bool ownsSchool)
	{
		return default;
	}

	internal static void Publish(ChapterOwnershipSnapshot snapshot, Action<string, string> setUserProperty)
	{
	}
}
