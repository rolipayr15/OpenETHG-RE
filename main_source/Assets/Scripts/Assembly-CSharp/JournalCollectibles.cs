using Prime31.MessageKit;
using UnityEngine;

public class JournalCollectibles : MonoBehaviour
{
	[SerializeField]
	private JournalData cloak;

	[SerializeField]
	private JournalData speed;

	[SerializeField]
	private JournalData drunk;

	[SerializeField]
	private JournalData moreEyes;

	[SerializeField]
	private JournalData blind;

	[SerializeField]
	private JournalData transparency;

	[SerializeField]
	private JournalData basementKey;

	private void Start()
	{
		if ((int)UserGameSettings.currentLevel == 0 && !basementKey.IsUnlocked())
		{
			MessageKit.addObserver(4, BasementKeyFound);
		}
		if (!cloak.IsUnlocked())
		{
			MessageKit<bool>.addObserver(14, UnlockCloak);
		}
		if (!speed.IsUnlocked())
		{
			MessageKit<bool>.addObserver(13, UnlockSpeed);
		}
		if (!moreEyes.IsUnlocked())
		{
			MessageKit.addObserver(16, UnlockMoreEyes);
		}
		if (!transparency.IsUnlocked())
		{
			MessageKit<bool>.addObserver(10, UnlockTransparency);
		}
		if (!drunk.IsUnlocked())
		{
			MessageKit<bool>.addObserver(12, UnlockDrunk);
		}
		if (!blind.IsUnlocked())
		{
			MessageKit<bool>.addObserver(11, UnlockBlind);
		}
	}

	private void UnlockCloak(bool isUnlocked)
	{
		cloak.Unlock();
		MessageKit<bool>.removeObserver(14, UnlockCloak);
	}

	private void UnlockSpeed(bool isUnlocked)
	{
		speed.Unlock();
		MessageKit<bool>.removeObserver(13, UnlockSpeed);
	}

	private void UnlockTransparency(bool isUnlocked)
	{
		transparency.Unlock();
		MessageKit<bool>.removeObserver(10, UnlockTransparency);
	}

	private void UnlockDrunk(bool isUnlocked)
	{
		drunk.Unlock();
		MessageKit<bool>.removeObserver(12, UnlockDrunk);
	}

	private void UnlockBlind(bool isBlind)
	{
		blind.Unlock();
		MessageKit<bool>.removeObserver(11, UnlockBlind);
	}

	private void UnlockMoreEyes()
	{
		moreEyes.Unlock();
		MessageKit.removeObserver(16, UnlockMoreEyes);
	}

	private void BasementKeyFound()
	{
		basementKey.Unlock();
		MessageKit.removeObserver(4, BasementKeyFound);
	}
}
