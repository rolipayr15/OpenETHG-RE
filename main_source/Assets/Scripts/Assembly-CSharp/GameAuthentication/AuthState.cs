using System;
using System.Linq;
using System.Collections.Generic;

namespace GameAuthentication
{
	public sealed class AuthState
	{
		public GameAuthSessionStatus SessionStatus { get; }

		public AuthOperation Operation { get; }

		public string PlayerId { get; }

		public GameAccountType AccountType { get; }

		public IReadOnlyList<GameAuthProvider> LinkedProviders { get; }

		public GameAuthProvider? PendingProvider { get; }

		public string LastError { get; }

		public GameAuthProvider? LastExplicitProvider { get; }

		public bool IsAuthorized => SessionStatus == (GameAuthSessionStatus)2;

		public bool HasLinkedSignInMethod => AccountType == (GameAccountType)2;

		public static AuthState Initial => new AuthState((GameAuthSessionStatus)0, (AuthOperation)0, string.Empty, (GameAccountType)0, Array.Empty<GameAuthProvider>());

		public AuthState(GameAuthSessionStatus sessionStatus, AuthOperation operation, string playerId, GameAccountType accountType, IEnumerable<GameAuthProvider> linkedProviders, GameAuthProvider? pendingProvider = null, string lastError = null, GameAuthProvider? lastExplicitProvider = null)
		{
			SessionStatus = sessionStatus;
			Operation = operation;
			PlayerId = playerId ?? string.Empty;
			AccountType = accountType;
			LinkedProviders = Array.AsReadOnly((linkedProviders ?? Enumerable.Empty<GameAuthProvider>()).Distinct().ToArray());
			PendingProvider = pendingProvider;
			LastError = lastError;
			LastExplicitProvider = lastExplicitProvider;
		}
	}
}
