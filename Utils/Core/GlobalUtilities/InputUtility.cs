namespace MonoTools.Core.GlobalUtilities {

	public static class InputUtility {

		private static bool _disableScroll = false;
		private static bool _disableM1 = false;
		private static bool _disableM2 = false;
		private static bool _disableM3 = false;
		public static List<Keys> keysToDisable;
		public static Dictionary<Keys, Keys> keyboardInputMap { get; set; }

		private static MouseState currentMouseState;
		private static MouseState lastMouseState;

		private static KeyboardState currentKeyboardState;
		private static KeyboardState lastKeyboardState;


		static InputUtility() {

			keysToDisable = new List<Keys>();
			keyboardInputMap = new Dictionary<Keys, Keys>();

			currentMouseState = Mouse.GetState();
			lastMouseState = currentMouseState;

			currentKeyboardState = Keyboard.GetState();
			lastKeyboardState = currentKeyboardState;
		}
		public static void update(GameTime gt) {
			keysToDisable.Clear();
			_disableScroll = false;
			_disableM1 = false;
			_disableM2 = false;
			_disableM3 = false;

			lastMouseState = currentMouseState;
			lastKeyboardState = currentKeyboardState;
			currentMouseState = Mouse.GetState();
			currentKeyboardState = Keyboard.GetState();
		}


		/**
		 *  The following block is keyboard inputs
		 *  by default it will check if a key is mapped to another key
		 *  by default, if a key is in keysToDisable, a function call to check status of that key will be false
		 */
		public static KeyboardState getKeyboardState() => currentKeyboardState;
		public static KeyboardState getLastKeyboardState() => lastKeyboardState;


		public static Keys getMappedKey(Keys key) {
			return keyboardInputMap.GetValueOrDefault(key, key);
		}

		public static bool isKeyDown(Keys key, bool useInputMap = true, bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && keysToDisable.Contains(key)) { return false; }
			return useInputMap ?
				currentKeyboardState.IsKeyDown(getMappedKey(key)) :
				currentKeyboardState.IsKeyDown(key);
		}
		public static bool isKeyUp(Keys key, bool useInputMap = true, bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && keysToDisable.Contains(key)) { return false; }
			return useInputMap ?
				currentKeyboardState.IsKeyUp(getMappedKey(key)) :
				currentKeyboardState.IsKeyUp(key);
		}
		public static bool isLastStatesKeyDown(Keys key, bool useInputMap = true, bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && keysToDisable.Contains(key)) { return false; }
			return useInputMap ?
				lastKeyboardState.IsKeyDown(getMappedKey(key)) :
				lastKeyboardState.IsKeyDown(key);
		}
		public static bool isLastStatesKeyUp(Keys key, bool useInputMap = true, bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && keysToDisable.Contains(key)) { return false; }
			return useInputMap ?
				lastKeyboardState.IsKeyUp(getMappedKey(key)) :
				lastKeyboardState.IsKeyUp(key);
		}

		public static void setKeyInputConsumption(Keys key) => keysToDisable.Add(key);
		public static bool wasKeyDisabled(Keys key) => keysToDisable.Contains(key);


		public static bool wasKeyPressed(Keys key, bool useInputMap = true) => isKeyDown(key, useInputMap) && isLastStatesKeyUp(key, useInputMap);
		public static bool wasKeyReleased(Keys key, bool useInputMap = true) => isKeyUp(key, useInputMap) && isLastStatesKeyDown(key, useInputMap);



		/**
		 *  The following block is mouse inputs
		 */
		public static MouseState getMouseState() => currentMouseState;
		public static MouseState getLastMouseState() => lastMouseState;

		public static void disableScroll(bool value = true) {
			_disableScroll = value;
		}
		public static void disableM1(bool value = true) {
			_disableM1 = value;
		}
		public static void disableM2(bool value = true) {
			_disableM2 = value;
		}
		public static void disableM3(bool value = true) {
			_disableM3 = value;
		}
		public static float getScrollDiff(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableScroll) { return 0; }
			return currentMouseState.ScrollWheelValue - lastMouseState.ScrollWheelValue;
		}
		public static bool isMouse1Down(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM1) { return false; }
			return currentMouseState.LeftButton == ButtonState.Pressed;
		}
		public static bool isMouse2Down(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM2) { return false; }
			return currentMouseState.RightButton == ButtonState.Pressed;
		}
		public static bool isMouse3Down(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM3) { return false; }
			return currentMouseState.MiddleButton == ButtonState.Pressed;
		}
		public static bool wasMouse1Pressed(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM1) { return false; }
			return lastMouseState.LeftButton == ButtonState.Released && currentMouseState.LeftButton == ButtonState.Pressed;
		}
		public static bool wasMouse2Pressed(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM2) { return false; }
			return lastMouseState.RightButton == ButtonState.Released && currentMouseState.RightButton == ButtonState.Pressed;
		}
		public static bool wasMouse3Pressed(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM3) { return false; }
			return lastMouseState.MiddleButton == ButtonState.Released && currentMouseState.MiddleButton == ButtonState.Pressed;
		}
		public static bool wasMouse1Released(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM1) { return false; }
			return lastMouseState.LeftButton == ButtonState.Pressed && currentMouseState.LeftButton == ButtonState.Released;
		}
		public static bool wasMouse2Released(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM2) { return false; }
			return lastMouseState.RightButton == ButtonState.Pressed && currentMouseState.RightButton == ButtonState.Released;
		}
		public static bool wasMouse3Released(bool ignoreKeyDisable = false) {
			if (!ignoreKeyDisable && _disableM3) { return false; }
			return lastMouseState.MiddleButton == ButtonState.Pressed && currentMouseState.MiddleButton == ButtonState.Released;
		}
		public static Point getMousePositionDiff(bool ignoreKeyDisable = false) {
			return currentMouseState.Position - lastMouseState.Position;
		}
		public static Point getMousePosition(bool ignoreKeyDisable = false) {
			return currentMouseState.Position;
		}

	}
}
