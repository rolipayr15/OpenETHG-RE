internal struct Zone
{
	public float start { get; private set; }

	public float end { get; private set; }

	public Zone(float start, float end)
	{
		this.start = start;
		this.end = end;
	}

	public float GetWidth()
	{
		return end - start;
	}

	public int GetFillPercentage()
	{
		return (int)(GetWidth() / 360f * 100f);
	}

	public void Shift(float amount)
	{
		start += amount;
		end += amount;
	}
}
