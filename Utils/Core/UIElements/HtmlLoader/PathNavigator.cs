namespace MonoTools.Core.UIElements {
	using System;
	using System.Collections;
	using System.Collections.Generic;
	using System.Reflection;

	// Walks a dot/bracket path against a registered object via reflection.
	// Paths are parsed once at bind time; property lookups are cached per object type.
	internal static class PathNavigator {

		// Splits a full key like "player.hp" or "inventory[0].name" into
		// (rootName, segments[]). A plain key with no dots or brackets
		// returns (key, empty array) — the plain getter path still applies.
		internal static (string root, PathSegment[] segments) parse(string fullKey) {
			int split = -1;
			for (int i = 0; i < fullKey.Length; i++) {
				if (fullKey[i] == '.' || fullKey[i] == '[') { split = i; break; }
			}
			if (split < 0) return (fullKey, Array.Empty<PathSegment>());

			string root = fullKey.Substring(0, split);
			var segs = new List<PathSegment>();
			int pos = split;
			while (pos < fullKey.Length) {
				char c = fullKey[pos];
				if (c == '.') {
					pos++;
					int start = pos;
					while (pos < fullKey.Length && fullKey[pos] != '.' && fullKey[pos] != '[') pos++;
					string name = fullKey.Substring(start, pos - start);
					if (!string.IsNullOrEmpty(name)) segs.Add(new PropertySegment(name));
				} else if (c == '[') {
					pos++;
					int start = pos;
					while (pos < fullKey.Length && fullKey[pos] != ']') pos++;
					string content = fullKey.Substring(start, pos - start).Trim();
					pos++; // skip ]
					if (content.Length >= 2 && (content[0] == '\'' || content[0] == '"')) {
						segs.Add(new KeySegment(content.Substring(1, content.Length - 2)));
					} else if (int.TryParse(content, out int index)) {
						segs.Add(new IndexSegment(index));
					}
				} else {
					pos++;
				}
			}
			return (root, segs.ToArray());
		}

		// Walks segments against root. Returns null if any step fails.
		internal static object evaluate(object root, PathSegment[] segments) {
			object current = root;
			foreach (PathSegment seg in segments) {
				if (current == null) return null;
				current = seg.apply(current);
			}
			return current;
		}

		internal static string toString(object value) => value?.ToString() ?? "";
	}

	internal abstract class PathSegment {
		public abstract object apply(object obj);
	}

	// Accesses a public property or field by name. Reflection result is cached per object type.
	internal sealed class PropertySegment : PathSegment {
		private readonly string _name;
		private Type         _lastType;
		private PropertyInfo _pi;
		private FieldInfo    _fi;

		internal PropertySegment(string name) => _name = name;

		public override object apply(object obj) {
			Type t = obj.GetType();
			if (t != _lastType) {
				_lastType = t;
				const BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;
				_pi = t.GetProperty(_name, flags);
				_fi = _pi == null ? t.GetField(_name, flags) : null;
			}
			if (_pi != null) return _pi.GetValue(_pi.GetMethod.IsStatic ? null : obj);
			if (_fi != null) return _fi.GetValue(_fi.IsStatic    ? null : obj);
			return null;
		}
	}

	// Accesses a list or array by integer index.
	internal sealed class IndexSegment : PathSegment {
		private readonly int _index;
		internal IndexSegment(int index) => _index = index;

		public override object apply(object obj) {
			if (obj is IList list) return _index < list.Count  ? list[_index]          : null;
			if (obj is Array arr)  return _index < arr.Length  ? arr.GetValue(_index)  : null;
			return null;
		}
	}

	// Accesses a dictionary or generic indexer by string key.
	internal sealed class KeySegment : PathSegment {
		private readonly string _key;
		internal KeySegment(string key) => _key = key;

		public override object apply(object obj) {
			if (obj is IDictionary dict) return dict.Contains(_key) ? dict[_key] : null;
			// Fallback: generic indexer via reflection (e.g. custom typed dictionaries)
			PropertyInfo indexer = obj.GetType().GetProperty("Item");
			if (indexer != null) {
				try { return indexer.GetValue(obj, new object[] { _key }); }
				catch { }
			}
			return null;
		}
	}
}
