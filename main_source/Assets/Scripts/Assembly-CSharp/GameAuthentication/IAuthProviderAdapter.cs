using System.Threading;
using System.Threading.Tasks;

namespace GameAuthentication
{
	public interface IAuthProviderAdapter
	{
		GameAuthProvider Provider { get; }

		bool HasActiveProviderSession { get; }

		Task<ProviderCredentialResult> AcquireCredentialAsync(CancellationToken cancellationToken);

		void SignOutProviderSession();

		void Tick();
	}
}
