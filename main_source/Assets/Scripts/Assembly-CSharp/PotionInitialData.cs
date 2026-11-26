using System.Globalization;
using System.Collections.Generic;
using UnityEngine;

public class PotionInitialData
{
	private int greenId;

	private List<string> potions;

	public List<string> Potions => potions;

	public int GreenId => greenId;

	public PotionInitialData(int greenId)
	{
		this.greenId = greenId;
		potions = new List<string>();
	}

	public void AddPotion(GameObject potion, int colorId)
	{
		potions.Add(colorId + ":" + potion.transform.position.sqrMagnitude.ToString(CultureInfo.InvariantCulture));
	}
}
