using System;

internal sealed class ReviewerBasicAuthSession
{
	private string credential;

	public bool IsAuthorized => !string.IsNullOrEmpty(credential);

	public string Credential => credential;

	public void Activate(string validatedCredential)
	{
		if (string.IsNullOrEmpty(validatedCredential))
		{
			throw new ArgumentException("A validated reviewer Basic credential is required.", "validatedCredential");
		}
		credential = validatedCredential;
	}

	public bool Clear()
	{
		bool had = !string.IsNullOrEmpty(credential);
		credential = null;
		return had;
	}
}
