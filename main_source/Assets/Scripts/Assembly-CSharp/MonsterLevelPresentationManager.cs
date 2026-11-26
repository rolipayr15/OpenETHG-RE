using UnityEngine;

public class MonsterLevelPresentationManager : CustomSingleton<MonsterLevelPresentationManager>
{
	[SerializeField]
	private Camera cameraMansion;

	[SerializeField]
	private Camera cameraMansionPixel;

	[SerializeField]
	private Camera cameraHospital;

	[SerializeField]
	private Camera cameraSchool;

	[SerializeField]
	private GameObject pumpkin;

	[SerializeField]
	private GameObject endlessBags;

	[SerializeField]
	private GameObject mansionKrasue;

	[SerializeField]
	private GameObject mansionCharlie;

	[SerializeField]
	private GameObject hospitalKrasue;

	[SerializeField]
	private GameObject hospitalCharlie;

	[SerializeField]
	private GameObject hospitalGoodBoy;

	[SerializeField]
	private GameObject schoolKrasue;

	[SerializeField]
	private GameObject schoolCharlie;

	[SerializeField]
	private GameObject schoolGoodBoy;

	[SerializeField]
	private GameObject witchMansion;

	[SerializeField]
	private GameObject mansionGoose;

	[SerializeField]
	private MonsterLevelPresentationRenderTexture StandardRenderTextureHolder;

	[SerializeField]
	private MonsterLevelPresentationRenderTexture SmallRenderTextureHolder;

	[SerializeField]
	private GameObject multiplayerShortcutButton;

	private void Start()
	{
		RenderTexture standard = Instance.GetStandardRenderTexture();
		cameraMansion.targetTexture = standard;
		cameraHospital.targetTexture = standard;
		cameraSchool.targetTexture = standard;
		cameraMansionPixel.targetTexture = Instance.GetSmallRenderTexture();
	}

	public RenderTexture GetStandardRenderTexture()
	{
		return StandardRenderTextureHolder.GetRenderTexture();
	}

	public RenderTexture GetSmallRenderTexture()
	{
		return SmallRenderTextureHolder.GetRenderTexture();
	}

	public void ChangeCamera(int storeObjectId)
	{
		cameraMansion.enabled = false;
		cameraMansionPixel.enabled = false;
		cameraHospital.enabled = false;
		cameraSchool.enabled = false;
		pumpkin.SetActive(false);
		mansionKrasue.SetActive(false);
		mansionCharlie.SetActive(false);
		hospitalCharlie.SetActive(false);
		hospitalKrasue.SetActive(false);
		hospitalGoodBoy.SetActive(false);
		schoolGoodBoy.SetActive(false);
		schoolKrasue.SetActive(false);
		schoolCharlie.SetActive(false);
		endlessBags.SetActive(false);
		witchMansion.SetActive(false);
		if (mansionGoose != null)
		{
			mansionGoose.SetActive(false);
		}
		multiplayerShortcutButton.SetActive(true);
		switch (PresetPageTable.At(storeObjectId))
		{
		case PresetPage.KrasueMansion:
			cameraMansion.enabled = true;
			mansionKrasue.SetActive(true);
			break;
		case PresetPage.CharlieHospital:
			cameraHospital.enabled = true;
			hospitalCharlie.SetActive(true);
			break;
		case PresetPage.GoodBoySchool:
			cameraSchool.enabled = true;
			schoolGoodBoy.SetActive(true);
			break;
		case PresetPage.Witch:
			cameraMansion.enabled = true;
			witchMansion.SetActive(true);
			break;
		case PresetPage.Goose:
			cameraMansion.enabled = true;
			if (mansionGoose != null)
			{
				mansionGoose.SetActive(true);
			}
			break;
		case PresetPage.TrickOrTreat:
			pumpkin.SetActive(true);
			hospitalKrasue.SetActive(true);
			cameraHospital.enabled = true;
			break;
		case PresetPage.DoubleTrouble:
			schoolKrasue.SetActive(true);
			schoolCharlie.SetActive(true);
			cameraSchool.enabled = true;
			break;
		case PresetPage.Pixel:
			cameraMansionPixel.enabled = true;
			mansionKrasue.SetActive(true);
			break;
		case PresetPage.Endless:
			cameraHospital.enabled = true;
			endlessBags.SetActive(true);
			hospitalGoodBoy.SetActive(true);
			break;
		case PresetPage.Multiplayer:
			multiplayerShortcutButton.SetActive(false);
			break;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
	}

	protected override void OnApplicationQuit()
	{
		base.OnApplicationQuit();
	}
}
