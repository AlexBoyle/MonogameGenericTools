using System.Collections.Generic;

namespace Utils.Core.GlobalUtilities {

	public static class InputUtility {

		public static Dictionary<string, string> inputMap { get; set; }
		public static MouseState currentMouseState { get; set; }
		public static KeyboardState currentKeyboardState { get; set; }
		public static KeyboardState lastKeyboardState { get; set; }
		public static MouseState lastMouseState { get; set; }

		static InputUtility() {
			// Setup
			inputMap = new Dictionary<string, string>();
			currentMouseState = Mouse.GetState();
			lastMouseState = currentMouseState;
			currentKeyboardState = Keyboard.GetState();
			lastKeyboardState = currentKeyboardState;
		}
		public static void update(GameTime gt) {
			lastMouseState = currentMouseState;
			lastKeyboardState = currentKeyboardState;
			currentMouseState = Mouse.GetState();
			currentKeyboardState = Keyboard.GetState();
		}

		public static bool isKeyDown(Keys key) { return currentKeyboardState.IsKeyDown(key); }

		public static float getScrollDiff() { return currentMouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue; }
		public static bool isMouse1Down() { return currentMouseState.LeftButton == ButtonState.Pressed; }
		public static bool isMouse2Down() { return currentMouseState.RightButton == ButtonState.Pressed; }
		public static bool isMouse3Down() { return currentMouseState.MiddleButton == ButtonState.Pressed; }
		public static Point getMousePositionDiffrence() { return currentMouseState.Position - lastMouseState.Position; }
		public static Point getMousePosition() { return currentMouseState.Position; }

		public static bool wasKeyPressed(Keys key) {
			return currentKeyboardState.IsKeyUp(key) && lastKeyboardState.IsKeyDown(key);
		}
	}
}
