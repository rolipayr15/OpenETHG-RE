using System.Collections.Generic;

public static class WinSummaryTableFormatter
{
	public readonly struct Row
	{
		public readonly string left;

		public readonly string right;

		public Row(string left, string right)
		{
			this.left = null;
			this.right = null;
		}
	}

	public static string BuildTwoPanels(string leftHeader, List<Row> leftRows, string rightHeader, List<Row> rightRows, int leftColWidth = 14, int rightColWidth = 6, int gap = 6)
	{
		return null;
	}

	private static string FormatRow(Row row, int leftColWidth, int rightColWidth, int panelWidth)
	{
		return null;
	}

	private static string CenterVisible(string text, int width)
	{
		return null;
	}

	private static string PadToPanelWidth(string s, int panelWidth)
	{
		return null;
	}
}
