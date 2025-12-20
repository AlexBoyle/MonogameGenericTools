namespace MonoTools.Core.YamlSettingsReader {
	using MonoTools.Core.GlobalUtilities;
	using System;
	using System.Collections.Generic;

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
				LoggingUtil.info($"Finished loading file={filename}");
			}
			catch (Exception ex) {
				LoggingUtil.info($"Failed loading file={filename}");
				LoggingUtil.info(ex.Message);
				LoggingUtil.info(ex.StackTrace.ToString());
			}
			return null;
		}

		public Dictionary<string, T> getSettings() {
			return settings;
		}

		public T get(string key) {
			return settings.GetValueOrDefault(key.ToLower(), null);
		}
		public List<T> getAllValues() {
			return settings.Values.ToList();
		}

		public List<string> getAllKeys() {
			return settings.Keys.ToList();
		}
	}
}
