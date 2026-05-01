namespace MonoTools.Core.UIElements {
	public static class UIManager {
		private static UIElement rootElement;
		private static Vector2 screenSize = new(1920, 1080);
		public static UIElement getRootElement() { return rootElement; }

		static UIManager() {
			rootElement = new UIElement();
			rootElement.setDimensions(screenSize);
		}

		public static void update(GameTime gt) {
			Vector2 currentScreenSize = WindowUtility.getMoniterScreenSize();
			if (screenSize != currentScreenSize) {
				screenSize = currentScreenSize;
				rootElement.setDimensions(screenSize);
			}


			rootElement.update(gt);
		}
		public static void draw(GameTime gt) {
			rootElement.draw(gt);
		}

		public static void customDraw(GameTime gt) {
			rootElement.customDraw(gt);
		}

	}
}
