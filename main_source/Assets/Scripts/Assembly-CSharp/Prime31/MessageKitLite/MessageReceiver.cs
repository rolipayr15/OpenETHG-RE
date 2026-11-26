namespace Prime31.MessageKitLite
{
	public interface MessageReceiver
	{
		void onMessageReceived(int messageType);
	}
	public interface MessageReceiver<T>
	{
		void onMessageReceived(int messageType, T message);
	}
	public interface MessageReceiver<T, U>
	{
		void onMessageReceived(int messageType, T message, U other);
	}
}
