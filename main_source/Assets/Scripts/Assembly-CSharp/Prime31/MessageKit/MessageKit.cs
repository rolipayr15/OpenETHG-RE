using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Prime31.MessageKit
{
	public static class MessageKit
	{
		private static Dictionary<int, List<Action>> _messageTable;

		static MessageKit()
		{
			_messageTable = new Dictionary<int, List<Action>>();
			MessageKitManager.registerMessageKitInstance(_messageTable);
		}

		public static void addObserver(int messageType, Action handler)
		{
			List<Action> value = null;
			if (!_messageTable.TryGetValue(messageType, out value))
			{
				value = new List<Action>();
				_messageTable.Add(messageType, value);
			}
			if (!value.Contains(handler))
			{
				_messageTable[messageType].Add(handler);
			}
		}

		public static void removeObserver(int messageType, Action handler)
		{
			List<Action> value = null;
			if (_messageTable.TryGetValue(messageType, out value) && value.Contains(handler))
			{
				value.Remove(handler);
			}
		}

		public static void post(int messageType)
		{
			List<Action> value = null;
			if (_messageTable.TryGetValue(messageType, out value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					value[num]();
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

		public static void LogObservers(int messageType)
		{
			if (!_messageTable.ContainsKey(messageType) || _messageTable[messageType].Count == 0)
			{
				Debug.Log("MessageType: " + messageType + " has no observers");
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MessageType: " + messageType + " has " + _messageTable[messageType].Count + " Observers");
			for (int i = 0; i < _messageTable[messageType].Count; i++)
			{
				Action action = _messageTable[messageType][i];
				stringBuilder.AppendLine("Target: " + action.Target?.ToString() + "\t Method: " + action.Method);
			}
			Debug.Log(stringBuilder.ToString());
		}
	}
	public static class MessageKit<U>
	{
		private static Dictionary<int, List<Action<U>>> _messageTable;

		static MessageKit()
		{
			_messageTable = new Dictionary<int, List<Action<U>>>();
			MessageKitManager.registerMessageKitInstance(_messageTable);
		}

		public static void addObserver(int messageType, Action<U> handler)
		{
			List<Action<U>> value = null;
			if (!_messageTable.TryGetValue(messageType, out value))
			{
				value = new List<Action<U>>();
				_messageTable.Add(messageType, value);
			}
			if (!value.Contains(handler))
			{
				_messageTable[messageType].Add(handler);
			}
		}

		public static void removeObserver(int messageType, Action<U> handler)
		{
			List<Action<U>> value = null;
			if (_messageTable.TryGetValue(messageType, out value) && value.Contains(handler))
			{
				value.Remove(handler);
			}
		}

		public static void post(int messageType, U param)
		{
			List<Action<U>> value = null;
			if (_messageTable.TryGetValue(messageType, out value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					value[num](param);
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

		public static void logObservers(int messageType)
		{
			if (!_messageTable.ContainsKey(messageType) || _messageTable[messageType].Count == 0)
			{
				Debug.Log("MessageType: " + messageType + " has no observers");
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MessageType: " + messageType + " has " + _messageTable[messageType].Count + " Observers");
			for (int i = 0; i < _messageTable[messageType].Count; i++)
			{
				Action<U> action = _messageTable[messageType][i];
				stringBuilder.AppendLine("Target: " + action.Target?.ToString() + "\t Method: " + action.Method);
			}
			Debug.Log(stringBuilder.ToString());
		}
	}
	public static class MessageKit<U, V>
	{
		private static Dictionary<int, List<Action<U, V>>> _messageTable;

		static MessageKit()
		{
			_messageTable = new Dictionary<int, List<Action<U, V>>>();
			MessageKitManager.registerMessageKitInstance(_messageTable);
		}

		public static void addObserver(int messageType, Action<U, V> handler)
		{
			List<Action<U, V>> value = null;
			if (!_messageTable.TryGetValue(messageType, out value))
			{
				value = new List<Action<U, V>>();
				_messageTable.Add(messageType, value);
			}
			if (!value.Contains(handler))
			{
				_messageTable[messageType].Add(handler);
			}
		}

		public static void removeObserver(int messageType, Action<U, V> handler)
		{
			List<Action<U, V>> value = null;
			if (_messageTable.TryGetValue(messageType, out value) && value.Contains(handler))
			{
				value.Remove(handler);
			}
		}

		public static void post(int messageType, U firstParam, V secondParam)
		{
			List<Action<U, V>> value = null;
			if (_messageTable.TryGetValue(messageType, out value))
			{
				for (int num = value.Count - 1; num >= 0; num--)
				{
					value[num](firstParam, secondParam);
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

		public static void logObservers(int messageType)
		{
			if (!_messageTable.ContainsKey(messageType) || _messageTable[messageType].Count == 0)
			{
				Debug.Log("MessageType: " + messageType + " has no observers");
				return;
			}
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.AppendLine("MessageType: " + messageType + " has " + _messageTable[messageType].Count + " Observers");
			for (int i = 0; i < _messageTable[messageType].Count; i++)
			{
				Action<U, V> action = _messageTable[messageType][i];
				stringBuilder.AppendLine("Target: " + action.Target?.ToString() + "\t Method: " + action.Method);
			}
			Debug.Log(stringBuilder.ToString());
		}
	}
}
