namespace MonoTools.Core.UIElements {
	using System;
	using System.Collections.Generic;
	using System.Text;

	// Per-load CSS rule store. Parses rule blocks and resolves the merged
	// style map for a given element (tag + class list + inline style).
	// Priority: tag rules → class rules (declaration order) → inline style.
	internal class StyleSheet {
		private readonly Dictionary<string, Dictionary<string, string>> rules =
			new(StringComparer.OrdinalIgnoreCase);

		public void addRules(string css) {
			css = stripComments(css);
			int i = 0;
			while (i < css.Length) {
				int open = css.IndexOf('{', i);
				if (open < 0) break;
				int close = css.IndexOf('}', open + 1);
				if (close < 0) break;

				string selectorBlock = css.Substring(i, open - i).Trim();
				string body          = css.Substring(open + 1, close - open - 1);
				var parsed           = StyleParser.parse(body);

				foreach (string raw in selectorBlock.Split(',')) {
					string sel = raw.Trim();
					if (string.IsNullOrEmpty(sel)) continue;
					if (!rules.TryGetValue(sel, out var dict)) {
						dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
						rules[sel] = dict;
					}
					foreach (var kv in parsed) dict[kv.Key] = kv.Value;
				}

				i = close + 1;
			}
		}

		public Dictionary<string, string> resolve(string tag, string classAttr, string inlineStyle) {
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

			if (rules.TryGetValue(tag, out var tagRules))
				foreach (var kv in tagRules) result[kv.Key] = kv.Value;

			if (!string.IsNullOrWhiteSpace(classAttr)) {
				foreach (string cls in classAttr.Split(' ', StringSplitOptions.RemoveEmptyEntries)) {
					if (rules.TryGetValue("." + cls, out var classRules))
						foreach (var kv in classRules) result[kv.Key] = kv.Value;
				}
			}

			if (!string.IsNullOrWhiteSpace(inlineStyle)) {
				foreach (var kv in StyleParser.parse(inlineStyle))
					result[kv.Key] = kv.Value;
			}

			return result;
		}

		private static string stripComments(string css) {
			var sb = new StringBuilder(css.Length);
			int i = 0;
			while (i < css.Length) {
				if (i + 1 < css.Length && css[i] == '/' && css[i + 1] == '*') {
					int end = css.IndexOf("*/", i + 2, StringComparison.Ordinal);
					i = end >= 0 ? end + 2 : css.Length;
				} else {
					sb.Append(css[i++]);
				}
			}
			return sb.ToString();
		}
	}
}
