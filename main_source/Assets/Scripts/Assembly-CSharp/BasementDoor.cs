using Prime31.MessageKit;
using UnityEngine;

public class BasementDoor : Interactable
{
	[SerializeField]
	private Collider doorCollider;

	public override void Start()
	{
		base.Start();
		MessageKit.addObserver(4, KeyFound);
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		MessageKit<string, bool>.post(3, I2.Loc.ScriptLocalization.YouNeedToFindAKey, isClickedByOtherPlayer);
		InteractionEnd();
	}

	public void KeyFound()
	{
		MessageKit.removeObserver(4, KeyFound);
		doorCollider.enabled = true;
		gameObject.SetActive(false);
	}

	protected override void OnDestroy()
	{
		MessageKit.removeObserver(4, KeyFound);
		base.OnDestroy();
	}
}
