namespace MonoTools.Core.UIElements {
	using System;
	using System.Collections.Generic;
	using MonoTools.Core.GlobalUtilities;

	public class InteractionContext {
		private readonly Dictionary<string, Action<GameTime>> handlers =
			new(StringComparer.OrdinalIgnoreCase);
		private readonly Dictionary<string, Action<string>> inputHandlers =
			new(StringComparer.OrdinalIgnoreCase);

		public void bind(string name, Action<GameTime> handler)      => handlers[name]      = handler;
		public void bindInput(string name, Action<string> handler)   => inputHandlers[name] = handler;

		// Returns the registered handler, or a no-op if none is registered.
		public Action<GameTime> resolve(string name) {
			if (handlers.TryGetValue(name, out var h)) return h;
			LoggingUtil.info($"[InteractionContext] no handler '{name}' registered");
			return _ => { };
		}

		public Action<string> resolveInput(string name) {
			if (inputHandlers.TryGetValue(name, out var h)) return h;
			LoggingUtil.info($"[InteractionContext] no input handler '{name}' registered");
			return _ => { };
		}

		public bool has(string name) => handlers.ContainsKey(name);
	}
}
