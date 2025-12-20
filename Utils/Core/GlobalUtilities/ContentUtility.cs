namespace MonoTools.Core.GlobalUtilities {
	using Microsoft.Xna.Framework.Content;
	using MonoTools.Core;

	public static class ContentUtility {

		private static ContentManager content { get; set; } = null;

		public static void initialize(Game game) {
			content = game.Content;
			content.RootDirectory = "Content";
		}

		public static T get<T>(string name) where T : class {
			try {
				return content.Load<T>(name) as T;
			}
			catch (ContentLoadException e) {
				LoggingUtil.err($"Error loading content: {name}");
				LoggingUtil.err(e.Message);
				return null;
			}
		}
	}
}
