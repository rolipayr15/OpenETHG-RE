using System;
using System.Globalization;
using System.Text.RegularExpressions;
using Prime31.MessageKit;
using UnityEngine;
using UnityEngine.UI;

public class InGameObjectives : MonoBehaviour
{
	[SerializeField]
	private GameObject moneyBagImage;

	[SerializeField]
	private Text label;

	[SerializeField]
	private GameObject panel;

	private bool isPlayerAlive = true;

	private bool isGuestAlive = true;

	private void Awake()
	{
		MessageKit<string>.addObserver(22, UpdateLabel);
		MessageKit.addObserver(17, Death);
		MessageKit.addObserver(92, DeathGuest);
		MessageKit.addObserver(53, Revive);
		label.text = "";
		MessageKit<bool>.addObserver(7, Vision);
	}

	private void UpdateLabel(string txt)
	{
		label.text = FormatGooseBagCount(txt);
		MoneyBagsManager manager = CustomSingleton<MoneyBagsManager>.Instance;
		if (manager != null && manager.CanEscape() && moneyBagImage != null)
		{
			moneyBagImage.SetActive(false);
		}
	}

	private string FormatGooseBagCount(string text)
	{
		if ((int)UserGameSettings.currentEnemy != 7 || string.IsNullOrEmpty(text))
		{
			return text;
		}
		int slash = text.LastIndexOf('/');
		if (slash < 1)
		{
			return text;
		}
		string collected = text.Substring(0, slash);
		string total = text.Substring(slash + 1);
		bool valid;
		if (collected.StartsWith("(", StringComparison.Ordinal) && collected.EndsWith(")", StringComparison.Ordinal))
		{
			collected = collected.Substring(1, collected.Length - 2);
			valid = Regex.IsMatch(collected, "^[0-9]+(?: [+−] [0-9]+){1,2}$");
		}
		else
		{
			valid = int.TryParse(collected, NumberStyles.None, CultureInfo.InvariantCulture, out int _);
		}
		if (!valid || !int.TryParse(total, NumberStyles.None, CultureInfo.InvariantCulture, out int _))
		{
			return text;
		}
		int bigSize = (int)Math.Round((double)((float)label.fontSize * 0.88f));
		int smallSize = (int)Math.Round((double)((float)label.fontSize * 0.6f));
		return "<size=" + bigSize + ">" + collected + "</size>  <size=" + smallSize + "><color=#B8B8B8>/" + total + "</color></size>";
	}

	private void Death()
	{
		isPlayerAlive = false;
		panel.SetActive(false);
	}

	private void DeathGuest()
	{
		isGuestAlive = false;
		panel.SetActive(false);
	}

	private void Revive()
	{
		isPlayerAlive = true;
		panel.SetActive(true);
	}

	private void Vision(bool isVisionActive)
	{
		if (isPlayerAlive && isGuestAlive)
		{
			panel.SetActive(!isVisionActive);
		}
	}

	private void OnDestroy()
	{
		MessageKit<string>.removeObserver(22, UpdateLabel);
		MessageKit.removeObserver(17, Death);
		MessageKit.removeObserver(92, DeathGuest);
		MessageKit.removeObserver(53, Revive);
		MessageKit<bool>.removeObserver(7, Vision);
	}
}
