using Rewired.Dev;

namespace RewiredConsts
{
	public static class Action
	{
		[ActionIdFieldInfo(categoryName = "MainMenu", friendlyName = "DifficultyUp")]
		public const int DifficultyUp = 41;

		[ActionIdFieldInfo(categoryName = "MainMenu", friendlyName = "DifficultyDown")]
		public const int DifficultyDown = 42;

		[ActionIdFieldInfo(categoryName = "MainMenu", friendlyName = "ChapterNext")]
		public const int ChapterNext = 43;

		[ActionIdFieldInfo(categoryName = "MainMenu", friendlyName = "ChapterPrevious")]
		public const int ChapterPrevious = 44;

		[ActionIdFieldInfo(categoryName = "MainMenu", friendlyName = "Play")]
		public const int Play = 45;

		[ActionIdFieldInfo(categoryName = "MainMenu", friendlyName = "BuyWithCoins")]
		public const int BuyWithCoins = 46;

		[ActionIdFieldInfo(categoryName = "MainMenu", friendlyName = "Accept in UI")]
		public const int Accept = 33;

		[ActionIdFieldInfo(categoryName = "MainMenu", friendlyName = "Decline in UI")]
		public const int Decline = 34;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Horizontal")]
		public const int Horizontal = 17;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Vertical")]
		public const int Vertical = 16;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Mouse X")]
		public const int Mouse_X = 18;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Mouse Y")]
		public const int Mouse_Y = 19;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Run")]
		public const int Run = 8;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Interact")]
		public const int Interact = 9;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Map")]
		public const int Map = 31;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Action0")]
		public const int EnemyVision = 32;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Menu")]
		public const int Menu = 11;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "Journal")]
		public const int Journal = 50;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "ThrowMeat")]
		public const int ThrowMeat = 51;

		[ActionIdFieldInfo(categoryName = "Gameplay", friendlyName = "OpenChatSettings")]
		public const int OpenChatSettings = 52;

		[ActionIdFieldInfo(categoryName = "PauseMenu", friendlyName = "UIHorizontal")]
		public const int UIHorizontal = 37;

		[ActionIdFieldInfo(categoryName = "PauseMenu", friendlyName = "UIVertical")]
		public const int UIVertical = 36;

		[ActionIdFieldInfo(categoryName = "PauseMenu", friendlyName = "UISubmit")]
		public const int UISubmit = 38;

		[ActionIdFieldInfo(categoryName = "PauseMenu", friendlyName = "UICancel")]
		public const int UICancel = 39;

		[ActionIdFieldInfo(categoryName = "Modal", friendlyName = "ModalYes")]
		public const int ModalYes = 47;

		[ActionIdFieldInfo(categoryName = "Modal", friendlyName = "ModalNo")]
		public const int ModalNo = 48;
	}
}
