using System;
using System.Collections.Generic;

namespace Multiplayer
{
	[Serializable]
	public struct GameData
	{
		public List<string> bags;

		public string saw;

		public List<string> safeCodes;

		public List<int> portraits;

		public List<string> totems;

		public int potionGreenId;

		public List<string> potions;
	}
}
