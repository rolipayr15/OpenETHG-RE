using System.Collections.Generic;
using Bibloteka;
using I2.Loc;
using Prime31.MessageKit;
using UnityEngine;

public class SafeManager : CustomSingleton<SafeManager>
{
	private string[] safeCodeFoundMessage;

	private int codesToActivate = 4;

	private int codesCounter;

	[SerializeField]
	private Mesh[] codeFragmentMesh;

	private void Start()
	{
		safeCodeFoundMessage = new string[4]
		{
			ScriptLocalization.safe_code_1,
			ScriptLocalization.safe_code_2,
			ScriptLocalization.safe_code_3,
			ScriptLocalization.safe_code_4
		};
		MessageKit<bool>.addObserver(18, SafeCodeFound);
		if (!UserGameSettings.IsMultiplayer() || UserGameSettings.isMultiplayerHost)
		{
			ActivateCodes();
		}
	}

	public List<string> ActivateCodes()
	{
		GameObject[] fragments = GameObject.FindGameObjectsWithTag("SafeFragment");
		Utils.Shuffle(fragments);
		foreach (GameObject fragment in fragments)
		{
			fragment.SetActive(false);
		}
		List<string> ids = new List<string>();
		for (int i = 0; i < codesToActivate; i++)
		{
			fragments[i].SetActive(true);
			fragments[i].GetComponent<MeshFilter>().mesh = codeFragmentMesh[i];
			ids.Add(Utils.GetId(fragments[i]));
		}
		return ids;
	}

	public void ActivateCodesFromHost(List<string> activeCodes)
	{
		GameObject[] fragments = GameObject.FindGameObjectsWithTag("SafeFragment");
		foreach (GameObject fragment in fragments)
		{
			fragment.SetActive(false);
		}
		int activated = 0;
		foreach (GameObject fragment in fragments)
		{
			if (activeCodes.Contains(Utils.GetId(fragment)))
			{
				fragment.GetComponent<MeshFilter>().mesh = codeFragmentMesh[activated];
				fragment.SetActive(true);
				activated++;
			}
		}
	}

	private void SafeCodeFound(bool isClickedByOtherPlayer)
	{
		if (UserGameSettings.IsMultiplayerCoop())
		{
			if (codesCounter >= codesToActivate)
			{
				return;
			}
			MessageKit<string, bool>.post(3, safeCodeFoundMessage[codesCounter], isClickedByOtherPlayer);
		}
		else
		{
			if (isClickedByOtherPlayer || codesCounter >= codesToActivate)
			{
				return;
			}
			MessageKit<string, bool>.post(3, safeCodeFoundMessage[codesCounter], false);
		}
		codesCounter++;
		if (codesCounter == codesToActivate)
		{
			MessageKit.post(91);
		}
	}

	protected override void OnDestroy()
	{
		MessageKit<bool>.removeObserver(18, SafeCodeFound);
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
