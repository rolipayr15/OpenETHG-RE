namespace Characters.Strategies.Goose
{
	public readonly struct GooseChaseSignal
	{
		public readonly bool PostLocal;

		public readonly bool LocalValue;

		public readonly bool PostGuest;

		public readonly bool GuestValue;

		public readonly bool CountsLocalPursuit;

		public static readonly GooseChaseSignal None;

		private GooseChaseSignal(bool postLocal, bool localValue, bool postGuest, bool guestValue, bool countsLocalPursuit)
		{
			PostLocal = postLocal;
			LocalValue = localValue;
			PostGuest = postGuest;
			GuestValue = guestValue;
			CountsLocalPursuit = countsLocalPursuit;
		}

		public static GooseChaseSignal OnWindup(EnemyTarget target)
		{
			return (int)target == 2 ? new GooseChaseSignal(false, false, true, true, false) : new GooseChaseSignal(true, true, false, false, true);
		}

		public static GooseChaseSignal OnWalkingStart(bool isPatrol, bool localRaised, bool guestRaised)
		{
			return isPatrol ? new GooseChaseSignal(localRaised, false, guestRaised, false, false) : None;
		}
	}
}
