using System;
using ExternalApi.Models;

public interface IExperienceManager
{
	event Action<int, int> OnLevelUp;

	event Action<GrantExpData, bool> OnExpChanged;

	int GetTotalExp();

	int GetExpSinceLastLevelUp();

	int GetExpNeededForLevelUp();

	int GetCurrentLevel();

	void GrantExp(GrantExpData grantExpData);

	void OverwriteExp(int exp);

	int GetExpNeededBetweenLevels(int fromLevel, int toLevel);

	int GetLevelForExp(int exp);
}
