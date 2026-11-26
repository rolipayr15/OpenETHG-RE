namespace GameAuthentication
{
	public sealed class ProviderCredentialResult
	{
		public ProviderCredentialStatus Status { get; }

		public ProviderCredential Credential { get; }

		public string Error { get; }

		private ProviderCredentialResult(ProviderCredentialStatus status, ProviderCredential credential, string error)
		{
			Status = status;
			Credential = credential;
			Error = error;
		}

		public static ProviderCredentialResult Succeeded(ProviderCredential credential)
		{
			return new ProviderCredentialResult((ProviderCredentialStatus)0, credential, null);
		}

		public static ProviderCredentialResult Cancelled()
		{
			return new ProviderCredentialResult((ProviderCredentialStatus)1, null, null);
		}

		public static ProviderCredentialResult Failed(string error)
		{
			return new ProviderCredentialResult((ProviderCredentialStatus)2, null, error);
		}
	}
}
