namespace MonoTools.Core.UIElements {
	using System;
	using System.Collections.Generic;

	public static class StyleParser {
		// ── Public API ───────────────────────────────────────────────────────

		// Parses a CSS style attribute string into a flat property map.
		// Shorthand properties (padding, margin, border) are expanded into
		// their longhand equivalents using standard CSS multi-value rules.
		// Property names are lowercased; values are trimmed but otherwise raw.
		public static Dictionary<string, string> parse(string styleAttr) {
			var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
			if (string.IsNullOrWhiteSpace(styleAttr)) return result;

			foreach (string declaration in styleAttr.Split(';')) {
				string trimmed = declaration.Trim();
				if (string.IsNullOrEmpty(trimmed)) continue;

				int colon = trimmed.IndexOf(':');
				if (colon <= 0) continue;

				string property = trimmed.Substring(0, colon).Trim().ToLowerInvariant();
				string value    = trimmed.Substring(colon + 1).Trim();
				if (string.IsNullOrEmpty(value)) continue;

				expandInto(property, value, result);
			}

			return result;
		}

		// Returns the names of all live binding keys ({key}, not {{key}}) found
		// in a property value string. Used by the loader to detect which style
		// values are live bindings rather than static CSS values.
		public static List<string> extractLiveKeys(string value) {
			var keys = new List<string>();
			if (string.IsNullOrEmpty(value)) return keys;

			int i = 0;
			while (i < value.Length) {
				if (value[i] == '{') {
					if (i + 1 < value.Length && value[i + 1] == '{') {
						// Static slot {{key}} — skip to closing }}
						int end = value.IndexOf("}}", i + 2, StringComparison.Ordinal);
						i = end >= 0 ? end + 2 : value.Length;
					} else {
						// Live key {key} — extract name
						int end = value.IndexOf('}', i + 1);
						if (end > i) {
							string key = value.Substring(i + 1, end - i - 1).Trim();
							if (!string.IsNullOrEmpty(key)) keys.Add(key);
							i = end + 1;
						} else {
							i++;
						}
					}
				} else {
					i++;
				}
			}

			return keys;
		}

		// Returns true when the entire value is a single live binding ({key})
		// with no surrounding text. Used by the loader to decide between a
		// binding and a static style value.
		public static bool isSingleLiveKey(string value, out string key) {
			key = null;
			if (string.IsNullOrWhiteSpace(value)) return false;
			string t = value.Trim();
			if (t.Length < 3) return false;
			if (t[0] != '{' || t[1] == '{') return false;
			if (t[t.Length - 1] != '}' || t[t.Length - 2] == '}') return false;
			key = t.Substring(1, t.Length - 2).Trim();
			return !string.IsNullOrEmpty(key);
		}

		// ── CSS value parsers ────────────────────────────────────────────────

		// Strips "px" suffix and returns the numeric value, or 0 on failure.
		internal static float parsePx(string s) {
			s = s.Trim();
			if (s.EndsWith("px", StringComparison.OrdinalIgnoreCase))
				s = s.Substring(0, s.Length - 2);
			return float.TryParse(s.Trim(),
				System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : 0f;
		}

		// Parses a CSS dimension value into a (pixels, unit) pair.
		internal static (int val, Unit unit) parseDimension(string s) {
			s = s.Trim();
			if (s.EndsWith('%') && float.TryParse(s.TrimEnd('%'),
				System.Globalization.NumberStyles.Float,
				System.Globalization.CultureInfo.InvariantCulture, out float pct))
				return ((int)pct, Unit.PER);
			return ((int)parsePx(s), Unit.PX);
		}

		// Converts a CSS px font-size to a UIElement scale multiplier.
		internal static float parseFontSize(string s) {
			float px = parsePx(s);
			if (px <= 0) return 0.3f;
			return px / UIElement.defaultFontLineSpacing();
		}

		// Maps CSS alignment keywords to the Justify enum.
		internal static Justify parseJustify(string s) => s.Trim().ToLowerInvariant() switch {
			"center"               => Justify.CENTER,
			"end"   or "flex-end"  => Justify.END,
			"start" or "flex-start"=> Justify.START,
			_                      => Justify.NONE,
		};

	// ── Shorthand expansion ──────────────────────────────────────────────

		private static void expandInto(string property, string value, Dictionary<string, string> result) {
			switch (property) {
				case "padding": expandBoxModel("padding", value, result); break;
				case "margin":  expandBoxModel("margin",  value, result); break;
				case "border":  expandBorder(value, result);              break;
				default:        result[property] = value;                 break;
			}
		}

		// border: [width] [style] [color] — style keywords are skipped (only solid is rendered).
		private static void expandBorder(string value, Dictionary<string, string> result) {
			foreach (string part in value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries)) {
				if (_borderStyles.Contains(part)) continue;
				if (part.EndsWith("px", StringComparison.OrdinalIgnoreCase) ||
				    float.TryParse(part, System.Globalization.NumberStyles.Float,
				                   System.Globalization.CultureInfo.InvariantCulture, out _))
					result["border-width"] = part;
				else
					result["border-color"] = part;
			}
		}

		private static readonly HashSet<string> _borderStyles = new(StringComparer.OrdinalIgnoreCase) {
			"none", "hidden", "dotted", "dashed", "solid", "double", "groove", "ridge", "inset", "outset"
		};

		// CSS box-model shorthand rules:
		//   1 value  → all four sides
		//   2 values → top+bottom, left+right
		//   3 values → top, left+right, bottom
		//   4 values → top, right, bottom, left  (clockwise)
		private static void expandBoxModel(string prefix, string value, Dictionary<string, string> result) {
			string[] parts = value.Split(new char[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
			string top, right, bottom, left;

			switch (parts.Length) {
				case 1: top = right = bottom = left = parts[0]; break;
				case 2: top = bottom = parts[0]; right = left = parts[1]; break;
				case 3: top = parts[0]; right = left = parts[1]; bottom = parts[2]; break;
				case 4: top = parts[0]; right = parts[1]; bottom = parts[2]; left = parts[3]; break;
				default: result[prefix] = value; return;
			}

			result[prefix + "-top"]    = top;
			result[prefix + "-right"]  = right;
			result[prefix + "-bottom"] = bottom;
			result[prefix + "-left"]   = left;
		}
	}
}
