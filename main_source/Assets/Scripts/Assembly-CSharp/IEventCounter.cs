public interface IEventCounter
{
	int Count { get; }

	void Register(int count);

	void Reset();
}
