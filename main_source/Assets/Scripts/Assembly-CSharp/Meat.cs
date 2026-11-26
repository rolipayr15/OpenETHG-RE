using Prime31.MessageKit;
using DarkTonic.MasterAudio;
using UnityEngine;

public class Meat : Interactable
{
	[SerializeField]
	private GameObject parent;

	public bool canBeCollected = true;

	public string meatId;

	public override void Start()
	{
		base.Start();
		if (UserGameSettings.IsMultiplayer())
		{
			MessageKit<string>.addObserver(115, DeactivateMeat);
		}
		if (meatId == "")
		{
			meatId = Bibloteka.Utils.Vector3ToString(parent.transform.position);
		}
	}

	public override void Clicked(bool isClickedByOtherPlayer)
	{
		if (canBeCollected)
		{
			base.Clicked(isClickedByOtherPlayer);
			if (!isClickedByOtherPlayer)
			{
				MessageKit.post(28);
				MessageKit<string>.post(28, meatId);
				MessageKit<Interactable>.post(58, this);
				MasterAudio.PlaySound3DAtTransform("DogFx", transform, 1f, null, 0f, "meatCollect");
				parent.SetActive(false);
			}
			else
			{
				Debug.Log("Drugi gracz zebrał mięsko");
			}
		}
		InteractionEnd();
	}

	private void DeactivateMeat(string id)
	{
		if (id == meatId)
		{
			parent.SetActive(false);
			Debug.Log("Deactivate meat " + id);
		}
	}

	protected override void OnDestroy()
	{
		if (UserGameSettings.IsMultiplayer())
		{
			MessageKit<string>.removeObserver(115, DeactivateMeat);
		}
		base.OnDestroy();
	}
}
