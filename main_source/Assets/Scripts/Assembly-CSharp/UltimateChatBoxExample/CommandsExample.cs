using System;
using System.Collections.Generic;
using TankAndHealerStudioAssets;
using UnityEngine;
using UnityEngine.Events;

namespace UltimateChatBoxExample
{
	public class CommandsExample : MonoBehaviour
	{
		[Serializable]
		public class Command
		{
			public string commandValue;

			public string messageToSend;

			public bool requireMessageValue;

			public UnityEvent unityEvent;
		}

		public UltimateChatBox chatBox;

		public string myUsername;

		public List<Command> Commands;

		private void Start()
		{
		}

		private void Update()
		{
		}
	}
}
