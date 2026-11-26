using Prime31.MessageKit;
using DarkTonic.MasterAudio;
using UnityEngine;

public class SecretRoomDoor : Interactable
{
	private bool hasPlayerASaw;

	[SerializeField]
	private Collider doorCollider;

	[SerializeField]
	private GameObject planks;

	public override void Start()
	{
		base.Start();
		MessageKit.addObserver(20, SawFound);
	}

	private void SawFound()
	{
		hasPlayerASaw = true;
		MessageKit.removeObserver(20, SawFound);
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		InteractionEnd();
		if (!isClickedByOtherPlayer)
		{
			if (!hasPlayerASaw)
			{
				MessageKit<string, bool>.post(3, I2.Loc.ScriptLocalization.YouNeedASaw, false);
				return;
			}
			base.Clicked(false);
		}
		RemovePlanks();
	}

	private void RemovePlanks()
	{
		MasterAudio.PlaySound3DAtTransform("fx", gameObject.transform, 1f, null, 0f, "saw");
		doorCollider.isTrigger = true;
		planks.SetActive(false);
		gameObject.SetActive(false);
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(20, SawFound);
		base.OnDestroy();
	}
}
