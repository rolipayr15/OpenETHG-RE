using System.Collections;
using System.Collections.Generic;

namespace Prime31.MessageKit
{
	public static class MessageKitManager
	{
		private static List<IDictionary> _messageKitMessageTables = new List<IDictionary>();

		public static void registerMessageKitInstance(IDictionary messageKitMessageTable)
		{
			_messageKitMessageTables.Add(messageKitMessageTable);
		}

		public static void clearAllMessageTables()
		{
			for (int i = 0; i < _messageKitMessageTables.Count; i++)
			{
				_messageKitMessageTables[i].Clear();
			}
		}
	}
}
