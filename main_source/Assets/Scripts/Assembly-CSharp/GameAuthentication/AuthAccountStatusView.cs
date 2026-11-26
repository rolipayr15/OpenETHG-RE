namespace GameAuthentication
{
	public sealed class AuthAccountStatusView
	{
		public AuthAccountStatusKind Kind { get; }

		public bool IsSignedInAccount { get; }

		public bool ShowProviderUpgradeActions { get; }

		public bool ShowLinkedAccountActions { get; }

		public bool ShowSignedOutLoginActions { get; }

		public string Title { get; }

		public string Detail { get; }

		public GameAuthProvider? DisplayProvider { get; }

		public string CombinedStatusText => string.IsNullOrEmpty(Detail) ? Title : Title + System.Environment.NewLine + Detail;

		public AuthAccountStatusView(AuthAccountStatusKind kind, bool isSignedInAccount, bool showProviderUpgradeActions, bool showLinkedAccountActions, bool showSignedOutLoginActions, string title, string detail, GameAuthProvider? displayProvider = null)
		{
			Kind = kind;
			IsSignedInAccount = isSignedInAccount;
			ShowProviderUpgradeActions = showProviderUpgradeActions;
			ShowLinkedAccountActions = showLinkedAccountActions;
			ShowSignedOutLoginActions = showSignedOutLoginActions;
			Title = title;
			Detail = detail;
			DisplayProvider = displayProvider;
		}
	}
}
