public sealed class IntCounter : IEventCounter
{
	public int Count { get; private set; }

	public void Register(int count)
	{
		Count += count;
	}

	public void Reset()
	{
		Count = 0;
	}
}
