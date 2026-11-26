using UnityEngine;
using UnityEngine.UI;

namespace I2.Loc
{
	[AddComponentMenu("I2/Localization/SetLanguage Dropdown")]
	public class SetLanguageDropdown : MonoBehaviour
	{
		[SerializeField]
		private Dropdown dropdown;

		private void OnEnable()
		{
		}

		public void OnValueChanged(int index)
		{
		}
	}
}
