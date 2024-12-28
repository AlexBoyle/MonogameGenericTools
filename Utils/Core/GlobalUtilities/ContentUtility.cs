namespace Utils.Core.GlobalUtilities {
	using Microsoft.Xna.Framework.Content;
	using Utils.Core;
	using System;
	using System.Collections.Generic;

	public static class ContentUtility {

		private static ContentManager content { get; set; } = null;
		private static Dictionary<Type, ContentPoolBase> contentUtilities { get; set; } = new Dictionary<Type, ContentPoolBase>();

		public static void initialize(Game game) {
			content = game.Content;
			content.RootDirectory = "Content";
		}

		private static ContentPool<T> GetContentUtility<T>() where T : class {
			ContentPool<T> contentUtility = null;
			if (contentUtilities.ContainsKey(typeof(T))) {
				contentUtility = contentUtilities.GetValueOrDefault(typeof(T), null) as ContentPool<T>;
			}
			else {
				contentUtility = new ContentPool<T>(content);
				contentUtilities.Add(typeof(T), contentUtility);
			}
			return contentUtility;
		}
		public static T getAndSave<T>(string name) where T : class {
			ContentPool<T> contentUtility = GetContentUtility<T>();
			return contentUtility.getAndSave(name) as T;
		}

		public static T get<T>(string name) where T : class {
			ContentPool<T> contentUtility = GetContentUtility<T>();
			return contentUtility.get(name) as T;
		}
	}
}
