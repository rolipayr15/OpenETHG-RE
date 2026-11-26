namespace SkinSystem
{
	public interface ISkinVisualResolver
	{
		bool TryResolve(string skinId, out ISkinVisual visual);
	}
}
