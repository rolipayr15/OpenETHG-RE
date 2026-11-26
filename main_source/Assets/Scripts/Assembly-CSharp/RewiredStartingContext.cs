using UnityEngine;

// Sets which Rewired map category is active when this scene starts.
public class RewiredStartingContext : MonoBehaviour
{
	[SerializeField]
	private RewiredCategory startingCategory;

	private void Start()
	{
		ControlModeManager.Instance.SetStartingCategory((int)startingCategory);
	}
}
