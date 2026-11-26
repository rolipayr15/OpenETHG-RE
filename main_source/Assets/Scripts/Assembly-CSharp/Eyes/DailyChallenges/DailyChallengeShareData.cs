using UnityEngine;
using I2.Loc;
using System.Globalization;
using System;

namespace Eyes.DailyChallenges
{
	public sealed class DailyChallengeShareData
	{
		public string Heading { get; }

		public string Title { get; }

		public string Objective { get; }

		public bool ShowNumericProgress { get; }

		public string ProgressText { get; }

		public string DateText { get; }

		public string PlayerName { get; }

		public string Invitation { get; }

		public string ShareText { get; }

		public string FileName { get; }

		public DailyChallengeShareData(DailyChallengeCompletion completion, string playerName = null, Func<string, string> translate = null)
		{
			if (completion == null)
			{
				throw new ArgumentNullException("completion");
			}
			DailyChallengeView view = completion.Challenge;
			if (view == null || view.Definition == null || !view.IsCompleted)
			{
				throw new ArgumentException("Sharing requires a completed daily challenge.", "completion");
			}
			DailyChallengeDefinition definition = view.Definition;
			Func<string, string> lookup = translate ?? GetTranslation;
			CultureInfo culture = CultureInfo.InvariantCulture;
			string parameter = definition.Parameter.ToString("N0", culture);
			Heading = Localize(lookup, "DailyChallenges/ShareHeading", "DAILY CHALLENGE\nCOMPLETED!");
			Title = Localize(lookup, definition.TitleTerm, definition.Title).Replace("{0}", parameter);
			Objective = Localize(lookup, definition.ShortDescriptionTerm, definition.ShortDescription).Replace("{0}", parameter);
			ShowNumericProgress = definition.Goal > 1;
			ProgressText = ShowNumericProgress ? view.Progress.ToString(culture) + " / " + definition.Goal.ToString(culture) : string.Empty;
			DateText = completion.CompletedAtUtc.ToString("dd MMM yyyy", culture).ToUpperInvariant() + " · UTC";
			FileName = "Eyes-daily-challenge-" + completion.CompletedAtUtc.ToString("yyyy-MM-dd", culture) + ".png";
			PlayerName = string.IsNullOrWhiteSpace(playerName) ? string.Empty : string.Join(" ", playerName.Split((char[])null, StringSplitOptions.RemoveEmptyEntries));
			Invitation = Localize(lookup, "DailyChallenges/ShareInvitation", "Can you complete it too?");
			ShareText = string.Concat(new string[]
			{
				Heading.Replace('\n', ' '),
				"\n",
				Title,
				" — ",
				Objective,
				"\n",
				ShowNumericProgress ? ProgressText + " · " : string.Empty,
				DateText,
				"\n\n",
				Invitation,
				"\n",
				UrlStrings.GetGameStoreUrl(Application.platform)
			});
		}

		private static string GetTranslation(string term)
		{
			return LocalizationManager.GetTranslation(term, false);
		}

		private static string Localize(Func<string, string> lookup, string term, string fallback)
		{
			string value = lookup(term);
			if (!string.IsNullOrWhiteSpace(value) && value != term)
			{
				return value.Trim();
			}
			return fallback;
		}
	}
}
