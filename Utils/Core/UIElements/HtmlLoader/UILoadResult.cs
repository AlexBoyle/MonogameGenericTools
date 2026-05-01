namespace MonoTools.Core.UIElements {
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using MonoTools.Core.GlobalUtilities;

	public class UILoadResult {
		public UIElement root { get; }

		private readonly BindingContext                bindings;
		private readonly Dictionary<string, UIElement> idMap;
		private readonly List<ListPoller>              _listPollers = new();

		internal UILoadResult(UIElement root, BindingContext bindings, Dictionary<string, UIElement> idMap) {
			this.root     = root;
			this.bindings = bindings;
			this.idMap    = idMap;
		}

		// Returns the element registered with the given id="..." attribute, or null.
		public UIElement getElementById(string id) {
			idMap.TryGetValue(id, out var el);
			return el;
		}

		// Binds a container element to a live data list. On each update tick the
		// list is diffed by key — new items are appended as children built by the
		// factory; removed items are detached. Per-item UILoadResults stay alive
		// between ticks so their bindings refresh every frame.
		//
		//   containerId  — id="..." of the container element in the HTML
		//   getter       — returns the current collection; called every tick
		//   keySelector  — stable unique key per item; drives add/remove diffing
		//   factory      — called once per new item; build a UIContext, call
		//                  UIHtmlLoader.load("components/myrow.html", ctx),
		//                  and return the result
		public UILoadResult bindList<T, TKey>(
			string containerId,
			Func<IEnumerable<T>> getter,
			Func<T, TKey> keySelector,
			Func<T, UILoadResult> factory)
		{
			UIElement container = getElementById(containerId);
			if (container == null) {
				LoggingUtil.info($"[UILoadResult] bindList: no element with id '{containerId}'");
				return this;
			}
			_listPollers.Add(new ListPoller<T, TKey>(container, getter, keySelector, factory));
			return this;
		}

		// Call after root.update(gt) each frame. Blocks M1/M2 for game-world code
		// if the cursor is over any visible solid UI element.
		public void blockMouseOverUI() {
			Point m = InputUtility.getMousePosition();
			if (isMouseOverSolid(root, m)) {
				InputUtility.disableM1();
				InputUtility.disableM2();
			}
		}

		private static bool isMouseOverSolid(UIElement el, Point m) {
			if (!el.visible) return false;
			var r = new Rectangle(el.screenPosition.ToPoint(), el.screenDimensionsInPixels.ToPoint());
			if (r.Contains(m) && (el.color.A > 0 || el.texture != null)) return true;
			foreach (UIElement child in el.children)
				if (isMouseOverSolid(child, m)) return true;
			return false;
		}

		// Call each frame before root.update(gt) to push fresh data into elements.
		// Also ticks per-item bindings for all active list items.
		public void update(GameTime gt) {
			rewrapDescendants(root);
			bindings?.update(gt);
			foreach (var lp in _listPollers) lp.tick(gt);
		}

		// Re-wraps any word-wrap elements whose resolved width changed since the last
		// wrap pass (e.g., on the first frame after root.setDimensions is called).
		private static void rewrapDescendants(UIElement el) {
			if (el.needsRewrap) el.updateWordWrap();
			foreach (UIElement child in el.children) rewrapDescendants(child);
		}

		// ── List reconciliation ──────────────────────────────────────────────

		private abstract class ListPoller {
			internal abstract void tick(GameTime gt);
		}

		private sealed class ListPoller<T, TKey> : ListPoller {
			private readonly UIElement             _container;
			private readonly Func<IEnumerable<T>>  _getter;
			private readonly Func<T, TKey>         _keySelector;
			private readonly Func<T, UILoadResult> _factory;

			private readonly List<TKey>                     _orderedKeys = new();
			private readonly Dictionary<TKey, UILoadResult> _active      = new();

			internal ListPoller(UIElement container, Func<IEnumerable<T>> getter,
			                    Func<T, TKey> keySelector, Func<T, UILoadResult> factory) {
				_container   = container;
				_getter      = getter;
				_keySelector = keySelector;
				_factory     = factory;
			}

			internal override void tick(GameTime gt) {
				List<T> newItems = _getter().ToList();

				// Build lookup set for O(1) containment checks.
				var newKeySet = new HashSet<TKey>(newItems.Count);
				foreach (var item in newItems) newKeySet.Add(_keySelector(item));

				// Remove stale items (reverse iterate to keep indices valid).
				for (int i = _orderedKeys.Count - 1; i >= 0; i--) {
					TKey key = _orderedKeys[i];
					if (!newKeySet.Contains(key)) {
						_container.removeElement(_active[key].root);
						_active.Remove(key);
						_orderedKeys.RemoveAt(i);
					}
				}

				// Add items that are new in source order.
				foreach (var item in newItems) {
					TKey key = _keySelector(item);
					if (!_active.ContainsKey(key)) {
						UILoadResult result = _factory(item);
						_container.addElement(result.root);
						_active[key] = result;
						_orderedKeys.Add(key);
					}
				}

				// Refresh per-item bindings every tick.
				foreach (var result in _active.Values) result.update(gt);
			}
		}
	}
}
