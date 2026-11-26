using System;
using System.Linq;
using System.Collections.Generic;

namespace GameAuthentication
{
	public sealed class GameAccountInfo
	{
		public string PlayerId { get; }

		public IReadOnlyList<GameAuthProvider> LinkedProviders { get; }

		public GameAccountType AccountType => LinkedProviders.Count != 0 ? (GameAccountType)2 : (GameAccountType)1;

		public GameAccountInfo(string playerId, IEnumerable<GameAuthProvider> linkedProviders)
		{
			PlayerId = playerId ?? string.Empty;
			LinkedProviders = Array.AsReadOnly((linkedProviders ?? Enumerable.Empty<GameAuthProvider>()).Distinct().ToArray());
		}
	}
}
