using UnityEngine;
namespace GameAuthentication
{
	public sealed class AuthBootstrapStore : IAuthBootstrapStore
	{
		private readonly bool hadLegacyInstallationEvidenceAtStartup = PlayerPrefs.HasKey("AUTOLOGIN") || PlayerPrefs.HasKey("GAME_SESSION_COUNTER") || PlayerPrefs.HasKey("PLAY_COUNTER");

		public bool IsFirstInstallGuestBootstrapCompleted { get { if (PlayerPrefs.HasKey("AUTH_GUEST_BOOTSTRAP_COMPLETED")) { return PlayerPrefz.GetBool("AUTH_GUEST_BOOTSTRAP_COMPLETED", false); } if (hadLegacyInstallationEvidenceAtStartup) { MarkFirstInstallGuestBootstrapCompleted(); } return hadLegacyInstallationEvidenceAtStartup; } }

		public void MarkFirstInstallGuestBootstrapCompleted()
		{
			PlayerPrefz.SetBool("AUTH_GUEST_BOOTSTRAP_COMPLETED", true);
			PlayerPrefs.Save();
		}
	}
}
