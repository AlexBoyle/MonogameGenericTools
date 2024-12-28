namespace Utils.Core.GlobalUtilities {
	using System.Collections.Generic;

	public static class Flags {

		private static Dictionary<string, bool> gameFlags { get; set; } = new Dictionary<string, bool>();

		public static bool get(string key) {
			if (gameFlags.ContainsKey(key)) {
				return gameFlags[key];
			}
			return false;
		}

		public static void set(string key, bool value) {
			if (gameFlags.ContainsKey(key)) {
				gameFlags[key] = value;
			}
			else {
				gameFlags.Add(key, value);
			}
		}

		public static void clearAll() {
			gameFlags.Clear();
		}

		public static void clear(string key) {
			gameFlags.Remove(key);
		}

		public static Dictionary<string, bool>.KeyCollection getAllKeys() {
			return gameFlags.Keys;
		}

	}
}
