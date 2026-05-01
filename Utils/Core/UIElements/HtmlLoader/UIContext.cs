namespace MonoTools.Core.UIElements {
	using System;

	public class UIContext {
		public InteractionContext interactions { get; private set; } = new();
		public BindingContext      bindings    { get; private set; } = new();

		public UIContext() { }

		internal UIContext(BindingContext bindings, InteractionContext interactions) {
			this.bindings     = bindings;
			this.interactions = interactions;
		}

		// Convenience delegates into bindings.
		public void bindText(string name, Func<string> getter)        => bindings.bindText(name, getter);
		public void bindColor(string name, Func<Color> getter)        => bindings.bindColor(name, getter);
		public void bindVisible(string name, Func<bool> getter)       => bindings.bindVisible(name, getter);
		public void register(string name, Func<object> getter)        => bindings.register(name, getter);
		// Wires a text-change callback to an <input> element via onchange="{name}".
		public void bindInput(string name, Action<string> handler)    => interactions.bindInput(name, handler);
	}
}
