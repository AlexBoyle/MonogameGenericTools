namespace Utils.Core.YamlSettingsReader {
	using System;
	using System.Collections.Generic;
	using Utils.Core.GlobalUtilities;

	public class YamlSettingsReader {
		public string fileName { get; protected set; }
	}
	public class YamlSettingsReader<T> : YamlSettingsReader where T : SettingItem, new() {
		protected Dictionary<string, T> settings = new();

		public YamlSettingsReader() { }

		public YamlSettingsReader(string filename, string key) {
			this.fileName = filename;
			proccessSettingsFile(filename, key);
		}

		private Dictionary<string, T> proccessSettingsFile(string filename, string key) {
			var watch = new Stopwatch();
			watch.Start();
			try {
				settings = new Dictionary<string, T>();
				List<object> elementSettings = (List<object>)(FileUtility.readYamlFile(filename).GetValueOrDefault(key));
				foreach (Dictionary<object, object> element in elementSettings) {
					Dictionary<string, string> data = new();
					foreach (KeyValuePair<object, object> property in element) {
						data.Add((string)property.Key, (string)property.Value);
					}
					T yamlSetting = new T();
					yamlSetting.setData(data);
					settings.Add(yamlSetting.getKey(), yamlSetting);
				}
				LoggingUtil.logWithTimeTaken($"Finished loading file={filename}", watch);
			}
			catch (Exception ex) {
				LoggingUtil.logWithTimeTaken($"Failed loading file={filename}", watch);
				Debug.WriteLine(ex.Message);
				Debug.WriteLine(ex.StackTrace.ToString());
			}
			return null;
		}

		public Dictionary<string, T> getSettings() {
			return settings;
		}

		public T get(string key) {
			return settings[key.ToLower()];
		}

	}
}
