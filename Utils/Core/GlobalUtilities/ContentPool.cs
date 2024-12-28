namespace Utils.Core.GlobalUtilities {
	using Microsoft.Xna.Framework.Content;
	using System.Collections.Generic;

	public class ContentPool<T> : ContentPoolBase where T : class {
		protected ContentManager content { get; set; }
		protected Dictionary<string, T> assetMap { get; private set; } = new Dictionary<string, T>();

		public ContentPool(ContentManager content) {
			this.content = content;
		}

		public T getAndSave(string name) {
			if (assetMap.ContainsKey(name)) {
				return assetMap[name];
			}
			else {
				T effect = content.Load<T>(name);
				assetMap[name] = effect;
				if (effect == null) {
					Debug.WriteLine("Failed to Load content name=" + name);
				}
				return effect;
			}
		}

		public T get(string name) {
			T effect = content.Load<T>(name);
			assetMap[name] = effect;
			if (effect == null) {
				Debug.WriteLine("Failed to Load content name=" + name);
			}
			return effect;
		}
	}
}
