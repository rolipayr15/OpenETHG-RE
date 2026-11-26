using System.Collections.Generic;

namespace Prime31.MessageKitLite
{
	public static class MessageKitLite
	{
		private static Dictionary<int, List<MessageReceiver>> _messageTable = new Dictionary<int, List<MessageReceiver>>();

		public static void addObserver(int messageType, MessageReceiver handler)
		{
			List<MessageReceiver> value = null;
			if (!_messageTable.TryGetValue(messageType, out value))
			{
				value = new List<MessageReceiver>();
				_messageTable.Add(messageType, value);
			}
			if (!value.Contains(handler))
			{
				_messageTable[messageType].Add(handler);
			}
		}

		public static void removeObserver(int messageType, MessageReceiver handler)
		{
			List<MessageReceiver> value = null;
			if (_messageTable.TryGetValue(messageType, out value) && value.Contains(handler))
			{
				value.Remove(handler);
			}
		}

		public static void post(int messageType)
		{
			List<MessageReceiver> value = null;
			if (_messageTable.TryGetValue(messageType, out value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					value[num].onMessageReceived(messageType);
				}
			}
		}

		public static void clearMessageTable(int messageType)
		{
			if (_messageTable.ContainsKey(messageType))
			{
				_messageTable.Remove(messageType);
			}
		}

		public static void clearMessageTable()
		{
			_messageTable.Clear();
		}
	}
	public static class MessageKitLite<U>
	{
		private static Dictionary<int, List<MessageReceiver<U>>> _messageTable = new Dictionary<int, List<MessageReceiver<U>>>();

		public static void addObserver(int messageType, MessageReceiver<U> handler)
		{
			List<MessageReceiver<U>> value = null;
			if (!_messageTable.TryGetValue(messageType, out value))
			{
				value = new List<MessageReceiver<U>>();
				_messageTable.Add(messageType, value);
			}
			if (!value.Contains(handler))
			{
				_messageTable[messageType].Add(handler);
			}
		}

		public static void removeObserver(int messageType, MessageReceiver<U> handler)
		{
			List<MessageReceiver<U>> value = null;
			if (_messageTable.TryGetValue(messageType, out value) && value.Contains(handler))
			{
				value.Remove(handler);
			}
		}

		public static void post(int messageType, U param)
		{
			List<MessageReceiver<U>> value = null;
			if (_messageTable.TryGetValue(messageType, out value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					value[num].onMessageReceived(messageType, param);
				}
			}
		}

		public static void clearMessageTable(int messageType)
		{
			if (_messageTable.ContainsKey(messageType))
			{
				_messageTable.Remove(messageType);
			}
		}

		public static void clearMessageTable()
		{
			_messageTable.Clear();
		}
	}
	public static class MessageKitLite<U, V>
	{
		private static Dictionary<int, List<MessageReceiver<U, V>>> _messageTable = new Dictionary<int, List<MessageReceiver<U, V>>>();

		public static void addObserver(int messageType, MessageReceiver<U, V> handler)
		{
			List<MessageReceiver<U, V>> value = null;
			if (!_messageTable.TryGetValue(messageType, out value))
			{
				value = new List<MessageReceiver<U, V>>();
				_messageTable.Add(messageType, value);
			}
			if (!value.Contains(handler))
			{
				_messageTable[messageType].Add(handler);
			}
		}

		public static void removeObserver(int messageType, MessageReceiver<U, V> handler)
		{
			List<MessageReceiver<U, V>> value = null;
			if (_messageTable.TryGetValue(messageType, out value) && value.Contains(handler))
			{
				value.Remove(handler);
			}
		}

		public static void post(int messageType, U firstParam, V secondParam)
		{
			List<MessageReceiver<U, V>> value = null;
			if (_messageTable.TryGetValue(messageType, out value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					value[num].onMessageReceived(messageType, firstParam, secondParam);
				}
			}
		}

		public static void clearMessageTable(int messageType)
		{
			if (_messageTable.ContainsKey(messageType))
			{
				_messageTable.Remove(messageType);
			}
		}

		public static void clearMessageTable()
		{
			_messageTable.Clear();
		}
	}
}
