using Prime31.MessageKit;
using UnityEngine;

public class MapButton : HideWhenGamePaused
{
	private Animator mapButtonAnimator;

	public override void Start()
	{
		mapButtonAnimator = GetComponent<Animator>();
		MessageKit<int>.addObserver(35, MapFragment);
		base.Start();
	}

	private void MapFragment(int i)
	{
		mapButtonAnimator.SetTrigger("highlight");
	}

	public void ShowMap()
	{
		MessageKit.post(135);
	}

	private void OnDestroy()
	{
		MessageKit<int>.removeObserver(35, MapFragment);
	}
}
