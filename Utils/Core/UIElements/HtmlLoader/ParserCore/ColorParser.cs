namespace MonoTools.Core.UIElements {
	using System;
	using System.Collections.Generic;

	public static class ColorParser {
		// Parses a CSS color string into a MonoGame Color.
		// Returns false and Color.Transparent on failure.
		public static bool tryParse(string value, out Color color) {
			color = Color.Transparent;
			if (string.IsNullOrWhiteSpace(value)) return false;

			string s = value.Trim();

			if (s.StartsWith('#'))
				return tryParseHex(s, out color);

			if (s.StartsWith("rgba(", StringComparison.OrdinalIgnoreCase))
				return tryParseRgba(s, out color);

			if (s.StartsWith("rgb(", StringComparison.OrdinalIgnoreCase))
				return tryParseRgb(s, out color);

			return tryParseNamed(s, out color);
		}

		public static Color parse(string value, Color fallback = default) {
			return tryParse(value, out Color c) ? c : fallback;
		}

		// ── Hex ──────────────────────────────────────────────────────────────

		private static bool tryParseHex(string s, out Color color) {
			color = Color.Transparent;
			string hex = s.Substring(1);

			// Expand shorthand: #rgb → #rrggbb, #rgba → #rrggbbaa
			if (hex.Length == 3 || hex.Length == 4) {
				char[] expanded = new char[hex.Length * 2];
				for (int i = 0; i < hex.Length; i++) {
					expanded[i * 2]     = hex[i];
					expanded[i * 2 + 1] = hex[i];
				}
				hex = new string(expanded);
			}

			if (hex.Length == 6) {
				if (!tryHexByte(hex, 0, out byte r) ||
					!tryHexByte(hex, 2, out byte g) ||
					!tryHexByte(hex, 4, out byte b)) return false;
				color = new Color(r, g, b);
				return true;
			}

			if (hex.Length == 8) {
				if (!tryHexByte(hex, 0, out byte r) ||
					!tryHexByte(hex, 2, out byte g) ||
					!tryHexByte(hex, 4, out byte b) ||
					!tryHexByte(hex, 6, out byte a)) return false;
				color = new Color(r, g, b, a);
				return true;
			}

			return false;
		}

		private static bool tryHexByte(string s, int offset, out byte value) {
			value = 0;
			try {
				value = Convert.ToByte(s.Substring(offset, 2), 16);
				return true;
			} catch {
				return false;
			}
		}

		// ── rgb() / rgba() ───────────────────────────────────────────────────

		private static bool tryParseRgb(string s, out Color color) {
			color = Color.Transparent;
			string inner = extractInner(s, "rgb(");
			if (inner == null) return false;

			string[] parts = inner.Split(',');
			if (parts.Length != 3) return false;

			if (!tryChannel(parts[0], out byte r) ||
				!tryChannel(parts[1], out byte g) ||
				!tryChannel(parts[2], out byte b)) return false;

			color = new Color(r, g, b);
			return true;
		}

		private static bool tryParseRgba(string s, out Color color) {
			color = Color.Transparent;
			string inner = extractInner(s, "rgba(");
			if (inner == null) return false;

			string[] parts = inner.Split(',');
			if (parts.Length != 4) return false;

			if (!tryChannel(parts[0], out byte r) ||
				!tryChannel(parts[1], out byte g) ||
				!tryChannel(parts[2], out byte b)) return false;

			// Alpha channel: CSS uses 0.0–1.0 float; also accept 0–255 integer.
			string alphaTrim = parts[3].Trim();
			byte a;
			if (alphaTrim.Contains('.')) {
				if (!float.TryParse(alphaTrim, System.Globalization.NumberStyles.Float,
					System.Globalization.CultureInfo.InvariantCulture, out float af)) return false;
				a = (byte)Math.Clamp((int)(af * 255f), 0, 255);
			} else {
				if (!tryChannel(alphaTrim, out a)) return false;
			}

			color = new Color(r, g, b, a);
			return true;
		}

		private static string extractInner(string s, string prefix) {
			if (!s.EndsWith(')')) return null;
			if (!s.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return null;
			return s.Substring(prefix.Length, s.Length - prefix.Length - 1);
		}

		private static bool tryChannel(string s, out byte value) {
			value = 0;
			if (!int.TryParse(s.Trim(), out int i)) return false;
			if (i < 0 || i > 255) return false;
			value = (byte)i;
			return true;
		}

		// ── Named colors ─────────────────────────────────────────────────────

		private static bool tryParseNamed(string s, out Color color) {
			return namedColors.TryGetValue(s.ToLowerInvariant(), out color);
		}

		private static readonly Dictionary<string, Color> namedColors = new() {
			// CSS Level 1
			{ "black",       new Color(  0,   0,   0) },
			{ "silver",      new Color(192, 192, 192) },
			{ "gray",        new Color(128, 128, 128) },
			{ "grey",        new Color(128, 128, 128) },
			{ "white",       new Color(255, 255, 255) },
			{ "maroon",      new Color(128,   0,   0) },
			{ "red",         new Color(255,   0,   0) },
			{ "purple",      new Color(128,   0, 128) },
			{ "fuchsia",     new Color(255,   0, 255) },
			{ "green",       new Color(  0, 128,   0) },
			{ "lime",        new Color(  0, 255,   0) },
			{ "olive",       new Color(128, 128,   0) },
			{ "yellow",      new Color(255, 255,   0) },
			{ "navy",        new Color(  0,   0, 128) },
			{ "blue",        new Color(  0,   0, 255) },
			{ "teal",        new Color(  0, 128, 128) },
			{ "aqua",        new Color(  0, 255, 255) },
			// CSS Level 2+
			{ "orange",      new Color(255, 165,   0) },
			{ "coral",       new Color(255, 127,  80) },
			{ "salmon",      new Color(250, 128, 114) },
			{ "tomato",      new Color(255,  99,  71) },
			{ "crimson",     new Color(220,  20,  60) },
			{ "pink",        new Color(255, 192, 203) },
			{ "hotpink",     new Color(255, 105, 180) },
			{ "deeppink",    new Color(255,  20, 147) },
			{ "violet",      new Color(238, 130, 238) },
			{ "magenta",     new Color(255,   0, 255) },
			{ "orchid",      new Color(218, 112, 214) },
			{ "plum",        new Color(221, 160, 221) },
			{ "indigo",      new Color( 75,   0, 130) },
			{ "slateblue",   new Color(106,  90, 205) },
			{ "royalblue",   new Color( 65, 105, 225) },
			{ "dodgerblue",  new Color( 30, 144, 255) },
			{ "cornflowerblue", new Color(100, 149, 237) },
			{ "deepskyblue", new Color(  0, 191, 255) },
			{ "skyblue",     new Color(135, 206, 235) },
			{ "lightskyblue",new Color(135, 206, 250) },
			{ "steelblue",   new Color( 70, 130, 180) },
			{ "cadetblue",   new Color( 95, 158, 160) },
			{ "powderblue",  new Color(176, 224, 230) },
			{ "lightblue",   new Color(173, 216, 230) },
			{ "cyan",        new Color(  0, 255, 255) },
			{ "turquoise",   new Color( 64, 224, 208) },
			{ "mediumturquoise", new Color( 72, 209, 204) },
			{ "darkturquoise",   new Color(  0, 206, 209) },
			{ "mediumaquamarine", new Color(102, 205, 170) },
			{ "aquamarine",  new Color(127, 255, 212) },
			{ "palegreen",   new Color(152, 251, 152) },
			{ "lightgreen",  new Color(144, 238, 144) },
			{ "mediumspringgreen", new Color(  0, 250, 154) },
			{ "springgreen", new Color(  0, 255, 127) },
			{ "lawngreen",   new Color(124, 252,   0) },
			{ "chartreuse",  new Color(127, 255,   0) },
			{ "greenyellow", new Color(173, 255,  47) },
			{ "yellowgreen", new Color(154, 205,  50) },
			{ "darkolivegreen", new Color( 85, 107,  47) },
			{ "olivedrab",   new Color(107, 142,  35) },
			{ "darkgreen",   new Color(  0, 100,   0) },
			{ "forestgreen", new Color( 34, 139,  34) },
			{ "seagreen",    new Color( 46, 139,  87) },
			{ "mediumseagreen", new Color( 60, 179, 113) },
			{ "darkseagreen",   new Color(143, 188, 143) },
			{ "lightseagreen",  new Color( 32, 178, 170) },
			{ "gold",        new Color(255, 215,   0) },
			{ "goldenrod",   new Color(218, 165,  32) },
			{ "darkgoldenrod", new Color(184, 134,  11) },
			{ "palegoldenrod", new Color(238, 232, 170) },
			{ "khaki",       new Color(240, 230, 140) },
			{ "darkkhaki",   new Color(189, 183, 107) },
			{ "tan",         new Color(210, 180, 140) },
			{ "burlywood",   new Color(222, 184, 135) },
			{ "wheat",       new Color(245, 222, 179) },
			{ "sandybrown",  new Color(244, 164,  96) },
			{ "peru",        new Color(205, 133,  63) },
			{ "chocolate",   new Color(210, 105,  30) },
			{ "saddlebrown", new Color(139,  69,  19) },
			{ "sienna",      new Color(160,  82,  45) },
			{ "brown",       new Color(165,  42,  42) },
			{ "firebrick",   new Color(178,  34,  34) },
			{ "darkred",     new Color(139,   0,   0) },
			{ "indianred",   new Color(205,  92,  92) },
			{ "rosybrown",   new Color(188, 143, 143) },
			{ "lightcoral",  new Color(240, 128, 128) },
			{ "snow",        new Color(255, 250, 250) },
			{ "mistyrose",   new Color(255, 228, 225) },
			{ "seashell",    new Color(255, 245, 238) },
			{ "linen",       new Color(250, 240, 230) },
			{ "antiquewhite",new Color(250, 235, 215) },
			{ "blanchedalmond", new Color(255, 235, 205) },
			{ "bisque",      new Color(255, 228, 196) },
			{ "moccasin",    new Color(255, 228, 181) },
			{ "navajowhite", new Color(255, 222, 173) },
			{ "peachpuff",   new Color(255, 218, 185) },
			{ "papayawhip",  new Color(255, 239, 213) },
			{ "lavenderblush", new Color(255, 240, 245) },
			{ "lavender",    new Color(230, 230, 250) },
			{ "thistle",     new Color(216, 191, 216) },
			{ "honeydew",    new Color(240, 255, 240) },
			{ "mintcream",   new Color(245, 255, 250) },
			{ "azure",       new Color(240, 255, 255) },
			{ "aliceblue",   new Color(240, 248, 255) },
			{ "ghostwhite",  new Color(248, 248, 255) },
			{ "whitesmoke",  new Color(245, 245, 245) },
			{ "floralwhite", new Color(255, 250, 240) },
			{ "oldlace",     new Color(253, 245, 230) },
			{ "ivory",       new Color(255, 255, 240) },
			{ "beige",       new Color(245, 245, 220) },
			{ "lightyellow", new Color(255, 255, 224) },
			{ "lightgoldenrodyellow", new Color(250, 250, 210) },
			{ "cornsilk",    new Color(255, 248, 220) },
			{ "lemonchiffon",new Color(255, 250, 205) },
			{ "darkgray",    new Color(169, 169, 169) },
			{ "darkgrey",    new Color(169, 169, 169) },
			{ "dimgray",     new Color(105, 105, 105) },
			{ "dimgrey",     new Color(105, 105, 105) },
			{ "lightgray",   new Color(211, 211, 211) },
			{ "lightgrey",   new Color(211, 211, 211) },
			{ "gainsboro",   new Color(220, 220, 220) },
			{ "slategray",   new Color(112, 128, 144) },
			{ "slategrey",   new Color(112, 128, 144) },
			{ "lightslategray", new Color(119, 136, 153) },
			{ "lightslategrey", new Color(119, 136, 153) },
			{ "darkslategray",  new Color( 47,  79,  79) },
			{ "darkslategrey",  new Color( 47,  79,  79) },
			{ "midnightblue",   new Color( 25,  25, 112) },
			{ "darkblue",    new Color(  0,   0, 139) },
			{ "mediumblue",  new Color(  0,   0, 205) },
			{ "blueviolet",  new Color(138,  43, 226) },
			{ "darkviolet",  new Color(148,   0, 211) },
			{ "darkorchid",  new Color(153,  50, 204) },
			{ "mediumpurple",new Color(147, 112, 219) },
			{ "darkmagenta", new Color(139,   0, 139) },
			{ "rebeccapurple", new Color(102,  51, 153) },
			{ "transparent", Color.Transparent },
		};
	}
}
