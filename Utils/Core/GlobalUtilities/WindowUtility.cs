namespace MonoTools.Core.GlobalUtilities {
	using MonoTools.Core;
	public static class WindowUtility {
		private static GameWindow gameWindow { get; set; } = null;
		private static GraphicsDevice graphicsDevice { get; set; } = null;
		private static GraphicsDeviceManager graphicsDeviceManager { get; set; } = null;

		private static Vector2 screenSize = new();

		public static void initialize(Game game, GraphicsDeviceManager graphicsDeviceManagerRef) {
			gameWindow = game.Window;
			graphicsDevice = game.GraphicsDevice;
			graphicsDeviceManager = graphicsDeviceManagerRef;
			graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
			;
			screenSize = new(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
			gameWindow.ClientSizeChanged += windowSizeChageCallback;
		}

		public static Vector2 getMoniterScreenSize() {
			return screenSize;
		}
		public static Point getwindowScreenSize() {
			return graphicsDevice.PresentationParameters.Bounds.Size;
		}

		public static void switchToFullScreen(bool isborderless, bool shouldHardwareSwitch) {
			screenSize = new(GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width, GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height);
			gameWindow.Position = new(0, 0);
			gameWindow.IsBorderless = isborderless;
			graphicsDeviceManager.HardwareModeSwitch = shouldHardwareSwitch;
			graphicsDeviceManager.PreferredBackBufferWidth = (int)screenSize.X;
			graphicsDeviceManager.PreferredBackBufferHeight = (int)screenSize.Y;
			graphicsDeviceManager.ApplyChanges();
		}

		private static void windowSizeChageCallback(object sender, EventArgs e) {

			gameWindow.ClientSizeChanged -= windowSizeChageCallback;
			/*
			// Update the preferred back buffer size
			graphicsDeviceManager.PreferredBackBufferWidth = gameWindow.ClientBounds.Width;
			graphicsDeviceManager.PreferredBackBufferHeight = gameWindow.ClientBounds.Height;

			screenSize = new(gameWindow.ClientBounds.Width, gameWindow.ClientBounds.Height);

			// Apply the changes
			graphicsDeviceManager.ApplyChanges();
			*/
			// Re-subscribe to the event
			gameWindow.ClientSizeChanged += windowSizeChageCallback;
		}


	}


}
