public static class PresetPageTable
{
	public static readonly PresetPage[] Pages = new PresetPage[11] { PresetPage.KrasueMansion, PresetPage.CharlieHospital, PresetPage.GoodBoySchool, PresetPage.Witch, PresetPage.Goose, PresetPage.TrickOrTreat, PresetPage.DoubleTrouble, PresetPage.Pixel, PresetPage.Endless, PresetPage.Expert, PresetPage.Multiplayer };

	public static PresetPage At(int index)
	{
		return index >= 0 && index < Pages.Length ? Pages[index] : PresetPage.None;
	}

	public static int IndexOf(PresetPage page)
	{
		return System.Array.IndexOf(Pages, page);
	}
}
