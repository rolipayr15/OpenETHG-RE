public static class MultiplayerWinRules
{
	public static MultiplayerWinKind Evaluate(bool wasWon, GameMode gameMode, bool playedWithRandom)
	{
		if (!wasWon)
		{
			return MultiplayerWinKind.None;
		}
		int kind;
		if ((int)gameMode == 2)
		{
			kind = 3;
		}
		else
		{
			if ((int)gameMode != 1)
			{
				return MultiplayerWinKind.None;
			}
			kind = 1;
		}
		if (!playedWithRandom)
		{
			kind++;
		}
		return (MultiplayerWinKind)kind;
	}
}
