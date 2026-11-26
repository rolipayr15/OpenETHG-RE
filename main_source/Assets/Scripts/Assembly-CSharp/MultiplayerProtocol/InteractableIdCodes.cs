using System.Collections.Generic;

namespace MultiplayerProtocol
{
	public static class InteractableIdCodes
	{
		public static readonly Dictionary<string, byte> InteractionsMap;

		static InteractableIdCodes()
		{
			InteractionsMap = new Dictionary<string, byte>
			{
				{ "BedsideCupboard", 1 },
				{ "RunningPumpkinPickup", 2 },
				{ "JournalEntry", 3 },
				{ "JournalInteraction", 4 },
				{ "JournalMultipleObjectsInteraction", 4 },
				{ "RadioPlayChildSong", 6 },
				{ "BasementDoor", 7 },
				{ "BasementKey", 8 },
				{ "Bathroom", 9 },
				{ "Call", 10 },
				{ "Candle", 11 },
				{ "Cupboard", 12 },
				{ "Drawer", 13 },
				{ "ExitDoor", 14 },
				{ "Eye", 15 },
				{ "Freezer", 16 },
				{ "Globe", 17 },
				{ "Gramofon", 18 },
				{ "JackO", 19 },
				{ "Meat", 20 },
				{ "MoneyBag", 21 },
				{ "MusicBox", 22 },
				{ "Piano", 23 },
				{ "Portrait", 24 },
				{ "Potion", 25 },
				{ "Printer", 26 },
				{ "Safe", 27 },
				{ "SafeCode", 28 },
				{ "Saw", 29 },
				{ "SecretRoomDoor", 30 },
				{ "Suitcase", 31 },
				{ "Toilet", 32 },
				{ "Wardrobe", 33 },
				{ "TutorialTapIcon", 34 }
			};
		}

		public static byte GetInteractableId(string interactableClassName)
		{
			byte id;
			return InteractionsMap.TryGetValue(interactableClassName, out id) ? id : (byte)0;
		}
	}
}
