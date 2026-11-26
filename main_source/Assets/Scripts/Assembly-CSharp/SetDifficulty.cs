using UnityEngine;
using UnityEngine.UI;

public class SetDifficulty : MonoBehaviour
{
	[SerializeField]
	private Dropdown dropdown;

	[SerializeField]
	private Text labelText;

	private void SetOptions()
	{
	}

	private void OnEnable()
	{
	}

	public void ChangeDifficulty(int choice)
	{
	}

	private int EnumToInt(Difficulty difficultyName)
	{
		return 0;
	}

	private Difficulty IntToEnum(int difficultyInt)
	{
		return Difficulty.Casual;
	}
}
