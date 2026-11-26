public sealed class BoolCounter : IEventCounter
{
	public int Count { get; private set; }

	public void Register(int count)
	{
		Count = 1;
	}

	public void Reset()
	{
		Count = 0;
	}
}
