namespace MonoTools.Core.GlobalUtilities {

	public enum LogLevel {
		DEBG = 0,
		INFO = 1,
		WARN = 2,
		ERRO = 3
	}

	public static class LoggingUtil {

		public static LogLevel logLevel = LogLevel.ERRO;

		public static string getFormatedGlobalTime() {
			return Globals.globalStopwatch.Elapsed.ToString("hh\\:mm\\:ss\\.ff");
		}

		public static void debg(string s) {
			if (logLevel >= LogLevel.DEBG)
				Debug.WriteLine(getFormatedGlobalTime() + " [DEBG] - " + s);
		}

		public static void info(string s) {
			if (logLevel >= LogLevel.INFO)
				Debug.WriteLine(getFormatedGlobalTime() + " [INFO] - " + s);
		}

		public static void warn(string s) {
			if (logLevel >= LogLevel.WARN)
				Debug.WriteLine(getFormatedGlobalTime() + " [WARN] - " + s);
		}

		public static void err(string s) {
			if (logLevel >= LogLevel.INFO)
				Debug.WriteLine(getFormatedGlobalTime() + " [ERRO] - " + s);
		}


	}
}
