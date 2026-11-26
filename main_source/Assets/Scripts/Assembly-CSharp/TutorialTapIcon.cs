using UnityEngine;

public class TutorialTapIcon : Interactable
{
	[SerializeField]
	private Transform iconTransformToPulse;

	[SerializeField]
	private bool lookAtPlayerCamera = true;

	[SerializeField]
	private Interactable relayTapTo;

	[SerializeField]
	private float pulseSpeed = 2f;

	private Transform playerCameraT;

	private Vector3 scaleStart;

	private Vector3 scaleEnd;

	[SerializeField]
	private TutorialObject tutorialObject;

	private new void Start()
	{
		string completedKey = null;
		switch ((int)tutorialObject)
		{
		case 0:
			completedKey = "EYE_TUTORIAL_COMPLETED";
			break;
		case 1:
			completedKey = "MONEY_BAG_TUTORIAL_COMPLETED";
			break;
		case 2:
			completedKey = "CUPBOARD_TUTORIAL_COMPLETED";
			break;
		}
		if (!TutorialManager.ShouldPlayTutorial() || (completedKey != null && PlayerPrefz.GetBool(completedKey, false)))
		{
			gameObject.SetActive(false);
			return;
		}
		playerCameraT = CustomSingleton<PlayerManager>.Instance.PlayerCamera.transform;
		scaleStart = iconTransformToPulse.transform.localScale;
		scaleEnd = scaleStart * 0.5f;
	}

	private void Update()
	{
		if (lookAtPlayerCamera)
		{
			iconTransformToPulse.LookAt(playerCameraT);
		}
		iconTransformToPulse.localScale = Vector3.Lerp(scaleStart, scaleEnd, Mathf.PingPong(pulseSpeed * Time.time, 1f));
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		InteractionEnd();
		if (relayTapTo != null)
		{
			relayTapTo.Clicked(false);
		}
		gameObject.SetActive(false);
	}
}
