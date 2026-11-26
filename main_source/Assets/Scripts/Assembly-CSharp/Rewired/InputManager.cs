using System.ComponentModel;
using UnityEngine;

namespace Rewired
{
	[AddComponentMenu("Rewired/Input Manager")]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DefaultExecutionOrder(-32000)]
	public sealed class InputManager : InputManager_Base
	{
		protected override void OnInitialized()
		{
		}

		protected override void OnDeinitialized()
		{
		}

		protected override void DetectPlatform()
		{
		}

		protected override void CheckRecompile()
		{
		}
	}
}
