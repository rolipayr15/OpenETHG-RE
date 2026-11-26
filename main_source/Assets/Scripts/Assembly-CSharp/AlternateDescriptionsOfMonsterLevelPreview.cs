using UnityEngine;

public class AlternateDescriptionsOfMonsterLevelPreview : MonoBehaviour
{
	[SerializeField]
	private AnimationCurve animationCurve;

	[SerializeField]
	private CanvasGroup group1;

	[SerializeField]
	private CanvasGroup group2;

	private float val;

	private Color col;

	private void Start()
	{
		animationCurve.preWrapMode = WrapMode.PingPong;
		animationCurve.postWrapMode = WrapMode.PingPong;
	}

	private void Update()
	{
		val = animationCurve.Evaluate(Time.time);
		group1.alpha = 1f - val;
		group2.alpha = val;
	}
}
