namespace Utils.Core.GlobalUtilities {
	using Utils.Core;

	public static class WindowUtility {
		private static GameWindow gameWindow { get; set; } = null;
		private static GraphicsDevice graphicsDevice { get; set; } = null;
		private static GraphicsDeviceManager graphicsDeviceManager { get; set; } = null;

		public static void initialize(Game game, GraphicsDeviceManager graphicsDeviceManager) {
			gameWindow = game.Window;
			graphicsDevice = game.GraphicsDevice;
			WindowUtility.graphicsDeviceManager = graphicsDeviceManager;
			WindowUtility.graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
		}

		public static Vector2 getMoniterScreenSize() {
			return new(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
		}
		public static Point getwindowScreenSize() {
			return new(graphicsDevice.PresentationParameters.Bounds.Width, graphicsDevice.PresentationParameters.Bounds.Height);
		}

		public static void switchToFullScreen(bool isborderless, bool shouldHardwareSwitch) {
			gameWindow.Position = new(0, 0);
			gameWindow.IsBorderless = isborderless;
			graphicsDeviceManager.HardwareModeSwitch = shouldHardwareSwitch;
			graphicsDeviceManager.PreferredBackBufferWidth = (int)getMoniterScreenSize().X;
			graphicsDeviceManager.PreferredBackBufferHeight = (int)getMoniterScreenSize().Y;
			graphicsDeviceManager.ApplyChanges();
		}

	}


}
