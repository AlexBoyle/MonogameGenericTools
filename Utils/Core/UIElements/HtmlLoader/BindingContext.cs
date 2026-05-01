namespace MonoTools.Core.UIElements
{
    using MonoTools.Core.GlobalUtilities;
    using System;
    using System.Collections.Generic;

    public class BindingContext
    {
        // Named getters registered by the game programmer before load().
        private readonly Dictionary<string, Func<string>> textGetters   = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<Color>>  colorGetters  = new(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<string, Func<bool>>   boolGetters   = new(StringComparer.OrdinalIgnoreCase);
        // Objects registered for path navigation: ctx.register("player", () => player)
        private readonly Dictionary<string, Func<object>> objectGetters = new(StringComparer.OrdinalIgnoreCase);

        // Per-element update actions built by the loader during tree construction.
        private readonly List<Action<GameTime>> pollers = new();

        // ── Public registration API ──────────────────────────────────────────

        public void bindText(string name, Func<string> getter)   => textGetters[name]   = getter;
        public void bindColor(string name, Func<Color> getter)  => colorGetters[name]  = getter;
        public void bindVisible(string name, Func<bool> getter) => boolGetters[name]   = getter;
        // Register an object for dot/bracket path navigation: {player.hp}, {stats['health']}, {list[0].name}
        public void register(string name, Func<object> getter)  => objectGetters[name] = getter;

        // ── Internal: subscriptions wired by the loader ──────────────────────

        // Subscribe a text template to target.text. Template may contain zero or
        // more {key} tokens; the assembled string is pushed when it changes.
        internal void subscribeText(string template, UIElement target)
        {
            var keys = StyleParser.extractLiveKeys(template);
            if (keys.Count == 0) { target.setText(template); return; }

            var resolved = new List<(string placeholder, Func<string> getter)>();
            foreach (string k in keys)
            {
                if (textGetters.TryGetValue(k, out var g))
                {
                    resolved.Add(("{" + k + "}", g));
                }
                else
                {
                    var (rootName, segments) = PathNavigator.parse(k);
                    if (objectGetters.TryGetValue(rootName, out var objGetter))
                    {
                        // Capture loop-local copies so each closure is independent.
                        var segs    = segments;
                        var getter_ = objGetter;
                        Func<string> pathGetter = segs.Length == 0
                            ? () => getter_()?.ToString() ?? ""
                            : () => PathNavigator.toString(PathNavigator.evaluate(getter_(), segs));
                        resolved.Add(("{" + k + "}", pathGetter));
                    }
                    else
                    {
                        LoggingUtil.info($"[BindingContext] no text binding '{k}'");
                    }
                }
            }

            // No known keys — set literally so the element is at least visible.
            if (resolved.Count == 0) { target.setText(template); return; }

            string last = null;
            pollers.Add(_ =>
            {
                string result = template;
                foreach (var (ph, g) in resolved)
                    result = result.Replace(ph, g() ?? "");
                if (result != last) { last = result; target.setText(result); }
            });
        }

        // Subscribe a color getter to target.color (isTextColor=false) or
        // target.textColor (isTextColor=true). Uses the shared colorGetters dict.
        internal void subscribeColor(string key, UIElement target, bool isTextColor)
        {
            if (!colorGetters.TryGetValue(key, out var getter))
            {
                LoggingUtil.info($"[BindingContext] no color binding '{key}'");
                return;
            }
            Color last = default;
            bool first = true;
            pollers.Add(_ =>
            {
                Color v = getter();
                if (first || v != last)
                {
                    first = false; last = v;
                    if (isTextColor) target.textColor = v;
                    else target.color = v;
                }
            });
        }

        internal void subscribeVisible(string key, UIElement target)
        {
            if (!boolGetters.TryGetValue(key, out var getter))
            {
                LoggingUtil.info($"[BindingContext] no visibility binding '{key}'");
                return;
            }
            bool last = true;
            bool first = true;
            pollers.Add(_ =>
            {
                bool v = getter();
                if (first || v != last) { first = false; last = v; target.visible = v; }
            });
        }

        // ── Internal: component forwarding helpers ───────────────────────────

        // Returns a live Func<string> for 'key' by checking textGetters first,
        // then objectGetters with path navigation. Returns null if not found.
        internal Func<string> resolveTextGetter(string key)
        {
            if (textGetters.TryGetValue(key, out var g)) return g;
            var (rootName, segs) = PathNavigator.parse(key);
            if (objectGetters.TryGetValue(rootName, out var objG))
            {
                if (segs.Length == 0) return () => objG()?.ToString() ?? "";
                var capturedSegs = segs;
                return () => PathNavigator.toString(PathNavigator.evaluate(objG(), capturedSegs));
            }
            return null;
        }

        // Copies a binding from this context into 'child' under a new name.
        // Priority: text → bool/visible → color → object (with or without path).
        internal void forwardTo(string sourceKey, string targetKey, BindingContext child)
        {
            if (textGetters.TryGetValue(sourceKey, out var textG))  { child.bindText(targetKey, textG);     return; }
            if (boolGetters.TryGetValue(sourceKey, out var boolG))   { child.bindVisible(targetKey, boolG);  return; }
            if (colorGetters.TryGetValue(sourceKey, out var colorG)) { child.bindColor(targetKey, colorG);   return; }

            var (rootName, segs) = PathNavigator.parse(sourceKey);
            if (objectGetters.TryGetValue(rootName, out var objG))
            {
                if (segs.Length == 0) { child.register(targetKey, objG); return; }
                var capturedSegs = segs;
                child.bindText(targetKey, () => PathNavigator.toString(PathNavigator.evaluate(objG(), capturedSegs)));
                return;
            }
            LoggingUtil.info($"[BindingContext] no binding '{sourceKey}' to forward");
        }

        // Pulls all pollers from 'source' into this context so this.update() drives them.
        internal void mergePollers(BindingContext source) => pollers.AddRange(source.pollers);

        // ── Frame update ─────────────────────────────────────────────────────

        public void update(GameTime gt)
        {
            foreach (var p in pollers) p(gt);
        }
    }
}
