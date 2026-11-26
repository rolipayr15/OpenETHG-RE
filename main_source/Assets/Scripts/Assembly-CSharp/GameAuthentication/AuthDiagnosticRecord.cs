using System.Collections.Generic;

namespace GameAuthentication
{
	internal sealed class AuthDiagnosticRecord
	{
		private readonly AuthOperationResult result;

		private readonly AuthOperation stateOperation;

		private readonly IReadOnlyList<GameAuthProvider> linkedProviders;

		private readonly string providerSessions;

		public AuthDiagnosticRecord(AuthOperationResult result, AuthOperation stateOperation, IEnumerable<GameAuthProvider> linkedProviders, string providerSessions)
		{
		}

		public string Format()
		{
			return null;
		}
	}
}
