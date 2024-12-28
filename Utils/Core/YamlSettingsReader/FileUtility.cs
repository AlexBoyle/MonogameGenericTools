using System;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

namespace Utils.Core.YamlSettingsReader {
	public static class FileUtility {

		public static Dictionary<object, object> readYamlFile(string fileName) {
			string path = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
			if (string.Compare(path.Substring(path.Length - 1, 1), "\\") != 0) {
				path = path + "\\";
			}
			StreamReader streamReader = new StreamReader(path + "Content\\" + fileName);
			string yaml = streamReader.ReadToEnd();
			var deserializer = new DeserializerBuilder().Build();
			streamReader.Close();
			return (Dictionary<object, object>)deserializer.Deserialize<object>(yaml);
		}

		public static bool writeYamlFile(object o) {
			return false;
		}

		public static bool writeTextFile(string fileName, string[] lines) {
			string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			using (StreamWriter outputFile = new StreamWriter(Path.Combine(docPath, fileName + ".txt"))) {
				outputFile.NewLine = "\n";
				foreach (string line in lines)
					outputFile.WriteLine(line);
			}
			return true;
		}
		public static string[] readTextFile(string fileName) {
			string docPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
			using (var sr = new StreamReader(Path.Combine(docPath, fileName + ".txt"))) {
				// Read the stream as a string, and write the string to the console.
				string output = sr.ReadToEnd();
				return output.Split('\n');
			}
			return null;
		}
	}
}
