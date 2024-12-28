namespace Utils.Core.YamlSettingsReader {
	using System.Collections.Generic;

	public class SettingItem {
		protected string key;

		public SettingItem() { }

		public string getKey() {
			return this.key.ToLower();
		}
		public virtual void setData(Dictionary<string, string> properties) {
			this.key = properties["key"];
		}
	}
}
