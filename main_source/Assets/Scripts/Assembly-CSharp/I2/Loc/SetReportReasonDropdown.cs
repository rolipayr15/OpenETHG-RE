using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
	public class SetReportReasonDropdown : MonoBehaviour
	{
		[SerializeField]
		private Text reportLabelInfo;

		[SerializeField]
		private ChatSettings chatSettings;

		[SerializeField]
		private GameObject submitButton;

		private string reason;

		private void OnEnable()
		{
		}

		private void OnValueChanged(int index)
		{
		}

		public void Submit()
		{
		}
	}
}
