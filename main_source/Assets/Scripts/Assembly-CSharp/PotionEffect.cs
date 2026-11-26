using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class PotionEffect : MonoBehaviour
{


	[SerializeField]
	protected int duration;

	public virtual IEnumerator StartEffect()
	{
		yield return null;
	}

	public virtual IEnumerator EndEffect()
	{
		yield return null;
	}

	public virtual int GetDuration()
	{
		return duration;
	}
}
