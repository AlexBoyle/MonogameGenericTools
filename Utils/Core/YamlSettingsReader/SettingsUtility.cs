namespace MonoTools.Core.YamlSettingsReader {
	using MonoTools.Core.GlobalUtilities;
	using System;
	using System.Collections.Generic;
	using System.Linq;

	public static class SettingsUtility {

		public static Dictionary<Type, YamlSettingsReader> settings { get; private set; } = new Dictionary<Type, YamlSettingsReader>();
		public static Dictionary<Type, YamlSettingsReader> settingsReader { get; private set; } = new Dictionary<Type, YamlSettingsReader>();

		public static void intialize() {

			List<Type> types = ObjectUtilities.getAllTypesOfBaseClass<YamlSettingsReader>();
			foreach (var t in types) {
				try {
					var yamlSettingsReader = Activator.CreateInstance(t);
					if (yamlSettingsReader.GetType().BaseType.GetGenericArguments().Length > 0) {
						Type type = yamlSettingsReader.GetType().BaseType.GetGenericArguments().Single();
						settings.Add(type, yamlSettingsReader as YamlSettingsReader);
						settingsReader.Add(yamlSettingsReader.GetType(), yamlSettingsReader as YamlSettingsReader);
						LoggingUtil.info("Found and added SettingsFile=\"" + (yamlSettingsReader as YamlSettingsReader).fileName + "\"");
					}
				}
				catch { }

			}

		}

		public static YamlSettingsReader<T> getSettingsReader<T>() where T : SettingItem, new() {
			return settings[typeof(T)] as YamlSettingsReader<T>;
		}

		public static T get<T>(string key) where T : SettingItem, new() {
			return getSettingsReader<T>().get(key) as T;
		}

		public static List<T> getAllValues<T>() where T : SettingItem, new() {
			return getSettingsReader<T>().getAllValues();
		}

		public static List<string> getAllKeys<T>() where T : SettingItem, new() {
			return getSettingsReader<T>().getAllKeys();
		}


		public static T getSettingReader<T>() where T : YamlSettingsReader, new() {
			return (T)settingsReader[typeof(T)];
		}
	}
}
