namespace GameAuthentication
{
	public interface IAuthBootstrapStore
	{
		bool IsFirstInstallGuestBootstrapCompleted { get; }

		void MarkFirstInstallGuestBootstrapCompleted();
	}
}
