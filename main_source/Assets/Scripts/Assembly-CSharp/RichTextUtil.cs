using System.Text.RegularExpressions;

public static class RichTextUtil
{
	private static readonly Regex Tags = new Regex("<.*?>", RegexOptions.Singleline);

	public static int VisibleLength(string s)
	{
		if (string.IsNullOrEmpty(s))
		{
			return 0;
		}
		return Tags.Replace(s, "").Length;
	}

	public static string PadRightVisible(string s, int totalVisibleWidth)
	{
		s = s ?? "";
		int visible = VisibleLength(s);
		if (totalVisibleWidth <= visible)
		{
			return s;
		}
		return s + new string(' ', totalVisibleWidth - visible);
	}

	public static string PadLeftVisible(string s, int totalVisibleWidth)
	{
		s = s ?? "";
		int visible = VisibleLength(s);
		if (totalVisibleWidth <= visible)
		{
			return s;
		}
		return new string(' ', totalVisibleWidth - visible) + s;
	}
}
