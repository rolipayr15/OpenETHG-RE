using System;
namespace GameAuthentication
{
	public sealed class ProviderCredential
	{
		public GameAuthProvider Provider { get; }

		public string Token { get; }

		public string DiagnosticIdentity { get; }

		public ProviderCredential(GameAuthProvider provider, string token)
			: this(provider, token, null)
		{
		}

		private ProviderCredential(GameAuthProvider provider, string token, string diagnosticIdentity)
		{
			if (string.IsNullOrEmpty(token))
			{
				throw new ArgumentException("A provider credential token is required.", "token");
			}
			if (diagnosticIdentity != null && !ProviderIdentityDiagnostics.IsValid(provider, diagnosticIdentity))
			{
				throw new ArgumentException("The provider diagnostic identity has an invalid format.", "diagnosticIdentity");
			}
			Provider = provider;
			Token = token;
			DiagnosticIdentity = diagnosticIdentity;
		}

		internal static ProviderCredential Create(GameAuthProvider provider, string token, string identitySubject)
		{
			return new ProviderCredential(provider, token, ProviderIdentityDiagnostics.Compute(provider, identitySubject));
		}
	}
}
