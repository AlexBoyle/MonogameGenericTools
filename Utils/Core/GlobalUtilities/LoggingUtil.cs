namespace Utils.Core.GlobalUtilities {
	public static class LoggingUtil {

		public static void info(string s) {
			Debug.WriteLine(s);
		}

		public static string getTimeTaken(Stopwatch stopwatch) {
			return $" timeTaken={(stopwatch.ElapsedTicks / 10000f).ToString("000.0000")}ms";
		}

		public static void logWithTimeTaken(string log, Stopwatch timer) {
			Debug.WriteLine(log + getTimeTaken(timer));

		}
		public static string getParsedGameTime(GameTime gameTime) {
			string gameTimeString = gameTime.TotalGameTime.ToString();
			return gameTimeString.Substring(0, gameTimeString.IndexOf("."));
		}

		public static void logWithGameTime(string log, GameTime gameTime) {
			info(
				getParsedGameTime(gameTime) + " - " +
				log
			);
		}
	}
}
