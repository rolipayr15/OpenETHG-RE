using System;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using UnityEngine.Networking;

namespace BestHTTP
{
	// backend.ethg-api.online presents a Cloudflare Origin certificate, which public CA stores do not trust.
	// That one host is pinned to the exact certificate it serves (valid until 2038); every other host keeps
	// the normal chain validation.
	public static class OfficialServerPin
	{
		public const string PinnedHost = "backend.ethg-api.online";

		private const string CertificateSha256 = "7c600a9dffcf2ee8dc7a2d2c87e523df2bc8809fa4f4aadd0c1a17261ea165d6";

		public static bool Applies(Uri uri)
		{
			return uri != null && string.Equals(uri.Host, PinnedHost, StringComparison.OrdinalIgnoreCase);
		}

		public static bool Matches(byte[] certificate)
		{
			if (certificate == null)
			{
				return false;
			}
			using (SHA256 sha = SHA256.Create())
			{
				string hash = string.Concat(sha.ComputeHash(certificate).Select((byte b) => b.ToString("x2")));
				return hash == CertificateSha256;
			}
		}

		public static bool ValidateSocketCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
		{
			if (errors == SslPolicyErrors.None)
			{
				return true;
			}
			// Only the exact pinned certificate is accepted when the store cannot validate the chain.
			bool pinned = Matches(certificate?.GetRawCertData());
			if (!pinned)
			{
				UnityEngine.Debug.LogWarning("[Online] certificate rejected: " + errors + ", subject " + certificate?.Subject);
			}
			return pinned;
		}
	}

	public sealed class OfficialServerPinHandler : CertificateHandler
	{
		protected override bool ValidateCertificate(byte[] certificateData)
		{
			return OfficialServerPin.Matches(certificateData);
		}
	}
}
