namespace SkinSystem
{
	public sealed class SkinDefinition
	{
		public string Id { get; }

		public string TargetId { get; }

		public string SlotId { get; }

		public string DisplayNameKey { get; }

		public SkinDefinition(string id, string targetId, string slotId, string displayNameKey)
		{
		}
	}
}
