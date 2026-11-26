using System;

namespace I2.Loc
{
	[Serializable]
	public class LanguageData
	{
		public string Name;

		public string Code;

		public byte Flags;

		[NonSerialized]
		public bool Compressed;

		public bool IsEnabled()
		{
			return (Flags & 1) == 0;
		}

		public void SetEnabled(bool bEnabled)
		{
			Flags = (byte)(bEnabled ? Flags & ~1 : Flags | 1);
		}

		public bool IsLoaded()
		{
			return (Flags & 4) == 0;
		}

		public bool CanBeUnloaded()
		{
			return (Flags & 2) == 0;
		}

		public void SetLoaded(bool loaded)
		{
			Flags = (byte)(loaded ? Flags & ~4 : Flags | 4);
		}

		public void SetCanBeUnLoaded(bool allowUnloading)
		{
			Flags = (byte)(allowUnloading ? Flags & ~2 : Flags | 2);
		}
	}
}
