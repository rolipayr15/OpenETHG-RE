using System;
namespace GameAuthentication
{
	public sealed class AuthOperationResult
	{
		public AuthResultCode Code { get; }

		public AuthOperation Operation { get; }

		public GameAuthProvider? Provider { get; }

		public string PreviousPlayerId { get; }

		public string PlayerId { get; }

		public string Error { get; }

		public string CorrelationId { get; }

		public string ProviderDiagnosticId { get; }

		public bool IsSuccess => (int)Code < 7 && ((0x6F >> (int)Code) & 1) != 0;

		public AuthOperationResult(AuthResultCode code, GameAuthProvider? provider = null, string previousPlayerId = null, string playerId = null, string error = null, AuthOperation operation = AuthOperation.Idle, string correlationId = null, string providerDiagnosticId = null)
		{
			if (providerDiagnosticId != null && (!provider.HasValue || !ProviderIdentityDiagnostics.IsValid(provider.Value, providerDiagnosticId)))
			{
				throw new ArgumentException("The provider diagnostic identity has an invalid format.", "providerDiagnosticId");
			}
			Code = code;
			Operation = operation;
			Provider = provider;
			PreviousPlayerId = previousPlayerId ?? string.Empty;
			PlayerId = playerId ?? string.Empty;
			Error = error;
			CorrelationId = correlationId ?? Guid.NewGuid().ToString("N");
			ProviderDiagnosticId = providerDiagnosticId;
		}
	}
}
