using Rewired;
using RewiredConsts;
using UnityEngine;
using UnityEngine.UI;

public class ClickButtonOnRewiredAction : MonoBehaviour
{
	[ActionIdProperty(typeof(Action))]
	[SerializeField]
	private int rewiredAction;

	private Button button;

	private void Start()
	{
	}

	private void Update()
	{
	}
}
