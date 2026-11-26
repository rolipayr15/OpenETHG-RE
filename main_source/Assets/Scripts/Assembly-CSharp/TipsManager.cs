using I2.Loc;
using UnityEngine;
using UnityEngine.UI;

public class TipsManager : MonoBehaviour
{
	[SerializeField]
	private Text tipLabel;

	private void Start()
	{
		string[] tips = new string[]
		{
			ScriptLocalization.TIP_1_Use_map,
			ScriptLocalization.TIP_2_Difficulty,
			ScriptLocalization.TIP_3_Store,
			ScriptLocalization.SandboxTip
		};
		string tip;
		if (UserGameSettings.bagsCollected == 0)
		{
			tip = ScriptLocalization.TIP_4_MoneyBags;
		}
		else if (!UserGameSettings.eyesUsed)
		{
			tip = ScriptLocalization.TIP_5_EyeRunes;
		}
		else if (UserGameSettings.bagsCollected < 4)
		{
			tip = ScriptLocalization.SandboxTip;
		}
		else
		{
			tip = tips[Random.Range(0, tips.Length)];
		}
		tipLabel.text = ScriptLocalization.Tip + " " + tip;
	}
}
