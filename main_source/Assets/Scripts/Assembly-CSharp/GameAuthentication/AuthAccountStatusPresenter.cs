namespace GameAuthentication
{
	public static class AuthAccountStatusPresenter
	{
		public const string LinkedUnknownDetail = "";

		public const string SessionExpiredDetail = "Sign out before choosing another sign-in method so this account is not replaced without your choice.";

		public static string GuestTitle => I2.Loc.ScriptLocalization.AuthStatusGuestTitle;

		public static string GuestDetail => I2.Loc.ScriptLocalization.AuthStatusGuestDetail;

		public static string SignedInWithExternal => I2.Loc.ScriptLocalization.AuthStatusSignedInWithExternal;

		public static string NotSignedIn => I2.Loc.ScriptLocalization.AuthStatusNotSignedIn;

		public static string SignInWithExternalDetail => I2.Loc.ScriptLocalization.AuthStatusSignInWithExternalDetail;

		public static string SessionExpiredTitle => I2.Loc.ScriptLocalization.AuthStatusSessionExpiredTitle;

		public static string GuestUpgradeActionLabel => I2.Loc.ScriptLocalization.AuthStatusGuestUpgradeActionLabel;

		public static string SignedOutLoginActionLabel => I2.Loc.ScriptLocalization.AuthStatusSignedOutLoginActionLabel;

		public static string MultiplayerGuestGateTitle => I2.Loc.ScriptLocalization.AuthStatusMultiplayerGuestGateTitle;

		public static string MultiplayerGuestGateDetail => I2.Loc.ScriptLocalization.AuthStatusMultiplayerGuestGateDetail;

		public static string MultiplayerGuestGateConfirmLabel => I2.Loc.ScriptLocalization.AuthStatusMultiplayerGuestGateConfirmLabel;

		public static string MultiplayerGuestNotNowLabel => I2.Loc.ScriptLocalization.AuthStatusMultiplayerGuestNotNowLabel;

		public static string MultiplayerMyAccountLabel => I2.Loc.ScriptLocalization.AuthStatusMultiplayerMyAccountLabel;

		public static AuthAccountStatusView Build(AuthState state)
		{
			if (state == null)
			{
				return new AuthAccountStatusView((AuthAccountStatusKind)0, false, false, false, true, NotSignedIn, SignInWithExternalDetail, null);
			}
			if ((int)state.SessionStatus != 2)
			{
				return new AuthAccountStatusView((AuthAccountStatusKind)4, true, false, false, false, SessionExpiredTitle, SessionExpiredDetail, null);
			}
			if ((int)state.AccountType == 1)
			{
				return new AuthAccountStatusView((AuthAccountStatusKind)1, true, true, false, false, GuestTitle, GuestDetail, null);
			}
			GameAuthProvider? provider = state.LastExplicitProvider;
			if (!provider.HasValue && state.LinkedProviders != null && state.LinkedProviders.Count == 1)
			{
				provider = state.LinkedProviders[0];
			}
			if (provider.HasValue)
			{
				return new AuthAccountStatusView((AuthAccountStatusKind)2, true, false, true, false, "Signed in with " + GetProviderDisplayName(provider.Value), "", provider);
			}
			return new AuthAccountStatusView((AuthAccountStatusKind)3, true, false, true, false, SignedInWithExternal, LinkedUnknownDetail, null);
		}

		public static string GetProviderDisplayName(GameAuthProvider provider)
		{
			switch ((int)provider)
			{
			case 0:
				return "Unity Player Account";
			case 1:
				return "Facebook";
			case 2:
				return "Apple";
			case 3:
				return "Google Play Games";
			default:
				return provider.ToString();
			}
		}

		public static bool IsSignedInGameAccount(AuthState state)
		{
			return state != null && (int)state.AccountType - 1U < 2U;
		}

		public static bool IsGuestAccount(AuthState state)
		{
			return state != null && (int)state.SessionStatus == 2 && (int)state.AccountType == 1;
		}

		public static bool IsExpiredGameAccount(AuthState state)
		{
			return state != null && (int)state.SessionStatus != 2 && (int)state.AccountType != 0;
		}

		public static bool IsLinkedGameAccount(AuthState state)
		{
			return state != null && (int)state.SessionStatus == 2 && (int)state.AccountType == 2;
		}

		public static bool IsMultiplayerReady(AuthState state, bool isSignedInWithBasicAuth)
		{
			return isSignedInWithBasicAuth || IsLinkedGameAccount(state);
		}

		public static string BuildMultiplayerGuestGateMessage()
		{
			return "<b>" + MultiplayerGuestGateTitle + "</b>\n\n" + MultiplayerGuestGateDetail;
		}

		public static string BuildMultiplayerExpiredGateMessage()
		{
			return "<b>" + SessionExpiredTitle + "</b>\n\n" + SessionExpiredDetail;
		}

		public static bool ShouldShowMyAccountLabel(AuthState state)
		{
			return IsSignedInGameAccount(state);
		}
	}
}
