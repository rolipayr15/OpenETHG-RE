using I2.Loc;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class ExpTable : MonoBehaviour
{
	[SerializeField]
	private Text tableText;

	[SerializeField]
	private Text totalExp;

	[SerializeField]
	private GameObject levelUpLabel;

	[SerializeField]
	private Text levelUpReward;

	private const string YELLOW = "#F2D36B";

	private const string RED = "#E15B5B";

	private CalcResult calcRes;

	public bool IsResultReady { get; private set; }

	private static string C(string hex, string s)
	{
		return "<color=" + hex + ">" + s + "</color>";
	}

	private static string B(string s)
	{
		return "<b>" + s + "</b>";
	}

	public void Start()
	{
		string scene = SceneManager.GetActiveScene().name;
		if (scene != "Win" && scene != "Over")
		{
			return;
		}
		calcRes = UserGameSettings.calcResult;
		if (calcRes != null)
		{
			PrintTable(calcRes);
		}
		if (UserGameSettings.hasPlayerLeveledUp)
		{
			levelUpReward.text = "x " + UserGameSettings.levelUpReward;
			levelUpLabel.SetActive(true);
			UserGameSettings.hasPlayerLeveledUp = false;
		}
		else
		{
			levelUpReward.text = string.Empty;
			levelUpLabel.SetActive(false);
		}
		IsResultReady = true;
	}

	public void PrintTable(CalcResult _calcResult)
	{
		if (_calcResult == null)
		{
			return;
		}
		PhaseResult basePhase = _calcResult.GetPhaseResult(Phase.Base);
		List<WinSummaryTableFormatter.Row> baseRows = new List<WinSummaryTableFormatter.Row>();
		if (basePhase != null && basePhase.Steps != null)
		{
			foreach (Step step in basePhase.Steps)
			{
				if (step != null && step.Value != 0)
				{
					baseRows.Add(new WinSummaryTableFormatter.Row((step.Label ?? string.Empty).ToUpper() + " (x" + step.Occurrences + ")", C(YELLOW, "+" + step.Occurrences * step.Value)));
				}
			}
			baseRows.Add(new WinSummaryTableFormatter.Row(ScriptLocalization.baseExp, "= " + basePhase.SubtotalAfterPhase));
		}
		PhaseResult bonusPhase = _calcResult.GetPhaseResult(Phase.Modifiers);
		List<WinSummaryTableFormatter.Row> bonusRows = new List<WinSummaryTableFormatter.Row>();
		if (bonusPhase != null && bonusPhase.Steps != null)
		{
			foreach (Step step in bonusPhase.Steps)
			{
				if (step.Value != 100)
				{
					bonusRows.Add(new WinSummaryTableFormatter.Row((step.Label ?? string.Empty).ToUpper(), C(YELLOW, "+" + (step.Value - 100) + "%")));
				}
			}
			bonusRows.Add(new WinSummaryTableFormatter.Row(ScriptLocalization.bonuses, "= " + bonusPhase.PhaseDelta));
		}
		tableText.supportRichText = true;
		totalExp.text = _calcResult.FinalTotal.ToString();
		tableText.text = WinSummaryTableFormatter.BuildTwoPanels(ScriptLocalization.baseExp, baseRows, ScriptLocalization.bonuses, bonusRows, 16, 6, 6);
	}
}
