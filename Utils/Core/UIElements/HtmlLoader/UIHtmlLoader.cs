namespace MonoTools.Core.UIElements
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;

    public static class UIHtmlLoader
    {
        // Extra directories to search when resolving component tags.
        private static readonly List<string> _extraComponentPaths = new();
        // Per-thread set of absolute paths currently being loaded — detects circular references.
        [System.ThreadStatic] private static HashSet<string> _loadingStack;

        private static readonly HashSet<string> _nativeTags = new(StringComparer.OrdinalIgnoreCase) {
            "div", "p", "span", "label", "button", "img", "hr", "br",
            "ul", "ol", "li", "style", "link", "section", "article",
            "header", "footer", "main", "nav", "aside",
            "h1", "h2", "h3", "h4", "h5", "h6",
            "input"
        };
        private static readonly HashSet<string> _reservedAttrs = new(StringComparer.OrdinalIgnoreCase) {
            "id", "class", "style", "onclick", "onhover", "onhoverexit", "visible", "onchange", "scroll"
        };

        public static void addComponentPath(string dir) => _extraComponentPaths.Add(dir);

        // Load a file from Content/UI/ and build a UIElement tree.
        // ctx:   live bindings and interaction handlers (optional).
        // props: static {{key}} substitutions applied before XML parsing.
        public static UILoadResult load(string relativePath, UIContext ctx = null, Dictionary<string, string> props = null)
        {
            string fullPath = resolvePath(relativePath);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"[UIHtmlLoader] file not found: {fullPath}");

            string raw = File.ReadAllText(fullPath);
            string subst = applyStaticSlots(raw, props);
            string dir = Path.GetDirectoryName(fullPath);
            var sheet = new StyleSheet();
            var idMap = new Dictionary<string, UIElement>(StringComparer.OrdinalIgnoreCase);

            XDocument doc;
            try
            {
                doc = XDocument.Parse(subst);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"[UIHtmlLoader] invalid XML in '{fullPath}': {ex.Message}", ex);
            }

            // Pre-load all <link>/<style> nodes so CSS rules are in the sheet
            // before any element is styled. This lets component files that
            // declare their stylesheet inside the root element work correctly.
            foreach (var node in doc.Root.DescendantsAndSelf())
            {
                string t = node.Name.LocalName.ToLowerInvariant();
                if (t == "style") sheet.addRules(node.Value);
                else if (t == "link") processLink(node, dir, sheet);
            }

            UIElement root;
            UIElement.suspendLayout = true;
            try
            {
                root = buildElement(doc.Root, dir, sheet, ctx, idMap);
            }
            finally
            {
                UIElement.suspendLayout = false;
            }

            // Hydrate live bindings before autoSize so text elements have a
            // real string to measure against.
            ctx?.bindings.update(null);
            autoSize(root);

            return new UILoadResult(root, ctx?.bindings ?? new BindingContext(), idMap);
        }

        // ── Auto-sizing ──────────────────────────────────────────────────────
        // Bottom-up pass: containers that received no explicit dimensions get
        // width=100% and/or height derived from their children so the parent
        // COLUMN accumulator sees a non-zero value.

        private static void autoSize(UIElement el)
        {
            foreach (UIElement child in el.children)
                autoSize(child);
            // Containers and word-wrap text elements both default to 100% width when
            // no explicit width was given. Word-wrap elements need the width resolved
            // before we can compute line breaks and derive the height.
            if (el.rawDimensions.X == 0 && (el.text == null || el.wordWrap))
                el.setWidth(100, Unit.PER);
            if (el.wordWrap && el.text != null)
            {
                el.updateWordWrap();
                return;
            }
            if (el.children.Count == 0) return;
            if (el.rawDimensions.Y == 0)
            {
                float h = el.computeContentHeight();
                if (h > 0) el.setHeight((int)Math.Ceiling(h));
            }
        }

        // ── Tree building ────────────────────────────────────────────────────

        private static UIElement buildElement(XElement node, string dir, StyleSheet sheet,
                                              UIContext ctx, Dictionary<string, UIElement> idMap)
        {
            string tag = node.Name.LocalName.ToLowerInvariant();

            // Meta-tags update the StyleSheet but produce no element.
            if (tag == "style")
            {
                sheet.addRules(node.Value);
                return null;
            }
            if (tag == "link")
            {
                processLink(node, dir, sheet);
                return null;
            }

            if (!isNativeTag(tag))
                return buildComponent(node, tag, dir, sheet, ctx, idMap);

            string classAttr = (string)node.Attribute("class") ?? "";
            string inlineStyle = (string)node.Attribute("style") ?? "";
            Dictionary<string, string> styles = sheet.resolve(tag, classAttr, inlineStyle);

            UIElement el = createElement(tag, node, ctx);
            applyStyle(el, styles, ctx);

            // ID registry
            string id = ((string)node.Attribute("id") ?? "").Trim();
            if (!string.IsNullOrEmpty(id)) idMap[id] = el;

            // Event wiring
            if (ctx != null)
            {
                wireEvent(node, "onclick", ctx, h => { el.interactive = true; el.onClick = h; });
                wireEvent(node, "onhover", ctx, h => { el.interactive = true; el.onHover = h; });
                wireEvent(node, "onhoverexit", ctx, h => { el.interactive = true; el.onHoverExit = h; });

                if (el is UITextBox tb) {
                    string onchange = ((string)node.Attribute("onchange") ?? "").Trim();
                    if (!string.IsNullOrEmpty(onchange) && StyleParser.isSingleLiveKey(onchange, out string changeKey))
                        tb.onChange = ctx.interactions.resolveInput(changeKey);
                }
            }

            // Visibility binding
            string visAttr = ((string)node.Attribute("visible") ?? "").Trim();
            if (StyleParser.isSingleLiveKey(visAttr, out string visKey) && ctx?.bindings != null)
                ctx.bindings.subscribeVisible(visKey, el);

            if (!isLeafTag(tag))
            {
                foreach (XElement child in node.Elements())
                {
                    UIElement childEl = buildElement(child, dir, sheet, ctx, idMap);
                    if (childEl != null) el.addElement(childEl);
                }
            }

            return el;
        }

        private static void wireEvent(XElement node, string attr, UIContext ctx,
                                      Action<Action<GameTime>> apply)
        {
            string val = ((string)node.Attribute(attr) ?? "").Trim();
            if (string.IsNullOrEmpty(val)) return;
            if (StyleParser.isSingleLiveKey(val, out string key))
                apply(ctx.interactions.resolve(key));
        }

        private static void processLink(XElement node, string dir, StyleSheet sheet)
        {
            string rel = ((string)node.Attribute("rel") ?? "").Trim();
            string href = ((string)node.Attribute("href") ?? "").Trim();
            if (!rel.Equals("stylesheet", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(href))
                return;
            string cssPath = Path.GetFullPath(Path.Combine(dir, href));
            if (File.Exists(cssPath))
                sheet.addRules(File.ReadAllText(cssPath));
            else
                LoggingUtil.info($"[UIHtmlLoader] stylesheet not found: {cssPath}");
        }

        // Leaf tags never have meaningful child elements.
        private static bool isLeafTag(string tag) =>
            tag == "img" || tag == "hr" || tag == "br" || tag == "input";

        // ── Element factory ──────────────────────────────────────────────────

        private static UIElement createElement(string tag, XElement node, UIContext ctx)
        {
            switch (tag)
            {
                case "button":
                    return new SimpleButton(directText(node));

                case "img":
                    {
                        var el = new UIElement();
                        string src = ((string)node.Attribute("src") ?? "").Trim();
                        if (!string.IsNullOrEmpty(src))
                        {
                            try { el.texture = ContentUtility.get<Texture2D>(src); }
                            catch { LoggingUtil.info($"[UIHtmlLoader] texture not found: {src}"); }
                        }
                        return el;
                    }

                case "hr":
                    {
                        var el = new UIElement();
                        el.setDimensions(100, Unit.PER, 1, Unit.PX);
                        el.color = new Color(100, 100, 100);
                        return el;
                    }

                case "input":
                    {
                        string itype = ((string)node.Attribute("type") ?? "text").Trim().ToLowerInvariant();
                        if (itype != "text") return new UIElement();
                        string ph = ((string)node.Attribute("placeholder") ?? "").Trim();
                        var tb = new UITextBox(ph);
                        string valAttr = ((string)node.Attribute("value") ?? "").Trim();
                        if (!string.IsNullOrEmpty(valAttr)) {
                            if (StyleParser.isSingleLiveKey(valAttr, out string valKey) && ctx?.bindings != null) {
                                var getter = ctx.bindings.resolveTextGetter(valKey);
                                if (getter != null) tb.value = getter();
                            } else {
                                tb.value = valAttr;
                            }
                        }
                        return tb;
                    }

                case "p":
                    {
                        var el = new UIElement();
                        el.wordWrap = true;
                        string t = directText(node);
                        if (!string.IsNullOrEmpty(t))
                        {
                            var liveKeys = StyleParser.extractLiveKeys(t);
                            if (liveKeys.Count > 0 && ctx?.bindings != null)
                                ctx.bindings.subscribeText(t, el);
                            else
                                el.setText(t);
                        }
                        return el;
                    }

                case "span":
                case "label":
                    {
                        var el = new UIElement();
                        string t = directText(node);
                        if (!string.IsNullOrEmpty(t))
                        {
                            var liveKeys = StyleParser.extractLiveKeys(t);
                            if (liveKeys.Count > 0 && ctx?.bindings != null)
                                ctx.bindings.subscribeText(t, el);
                            else
                                el.setText(t);
                        }
                        return el;
                    }

                default:
                    {
                        string scrollAttr = ((string)node.Attribute("scroll") ?? "").Trim();
                        if (scrollAttr.Equals("true", StringComparison.OrdinalIgnoreCase))
                            return new UIScrollPane();
                        return new UIElement();
                    }
            }
        }

        // Only the direct text nodes of an element, not descendant element text.
        private static string directText(XElement node) =>
            string.Concat(node.Nodes().OfType<XText>().Select(t => t.Value)).Trim();

        // ── Style application ────────────────────────────────────────────────

        private static void applyStyle(UIElement el, Dictionary<string, string> styles, UIContext ctx)
        {
            if (styles.Count == 0) return;

            float pxOr0(string key) =>
                styles.TryGetValue(key, out string val) ? StyleParser.parsePx(val) : 0f;

            if (styles.TryGetValue("font-size", out string v)) el.setFontSize(StyleParser.parseFontSize(v));
            if (el.text != null) el.sizeToText();

            if (styles.TryGetValue("width",  out string wStr)) { (int w, Unit wu) = StyleParser.parseDimension(wStr); el.setWidth(w, wu); }
            if (styles.TryGetValue("height", out string hStr)) { (int h, Unit hu) = StyleParser.parseDimension(hStr); el.setHeight(h, hu); }

            if (styles.TryGetValue("min-width",  out v)) el.setMinWidth(StyleParser.parsePx(v));
            if (styles.TryGetValue("max-width",  out v)) el.setMaxWidth(StyleParser.parsePx(v));
            if (styles.TryGetValue("min-height", out v)) el.setMinHeight(StyleParser.parsePx(v));
            if (styles.TryGetValue("max-height", out v)) el.setMaxHeight(StyleParser.parsePx(v));

            if (styles.TryGetValue("background-color", out v))
            {
                if (StyleParser.isSingleLiveKey(v, out string bgKey) && ctx?.bindings != null)
                    ctx.bindings.subscribeColor(bgKey, el, false);
                else
                    el.color = ColorParser.parse(v, Color.Transparent);
            }
            if (styles.TryGetValue("color", out v))
            {
                if (StyleParser.isSingleLiveKey(v, out string tcKey) && ctx?.bindings != null)
                    ctx.bindings.subscribeColor(tcKey, el, true);
                else
                    el.textColor = ColorParser.parse(v, Color.Black);
            }

            if (styles.TryGetValue("flex-direction", out v))
                el.setOrientation(v.Trim().Equals("row", StringComparison.OrdinalIgnoreCase)
                    ? Orientation.ROW : Orientation.COLUMN);

            if (styles.TryGetValue("align-items",     out v)) el.setJustify(StyleParser.parseJustify(v));
            if (styles.TryGetValue("justify-content", out v)) el.setJustify(StyleParser.parseJustify(v));

            if (styles.TryGetValue("gap", out v)) el.setGap(StyleParser.parsePx(v));

            {
                float pt = pxOr0("padding-top"), pr = pxOr0("padding-right");
                float pb = pxOr0("padding-bottom"), pl = pxOr0("padding-left");
                if (pt != 0 || pr != 0 || pb != 0 || pl != 0)
                    el.setPadding((int)pl, (int)pt, (int)pr, (int)pb);
            }
            {
                float mt = pxOr0("margin-top"), mr = pxOr0("margin-right");
                float mb = pxOr0("margin-bottom"), ml = pxOr0("margin-left");
                if (mt != 0 || mr != 0 || mb != 0 || ml != 0)
                    el.setMargin((int)ml, (int)mt, (int)mr, (int)mb);
            }

            if (styles.TryGetValue("border-width", out v)) el.borderWidth = StyleParser.parsePx(v);
            if (styles.TryGetValue("border-color", out v)) el.borderColor = ColorParser.parse(v, Color.Black);

            if (styles.TryGetValue("overflow",    out v) && v.Trim().Equals("hidden",  StringComparison.OrdinalIgnoreCase)) el.clipToBounds = true;
            if (styles.TryGetValue("display",     out v) && v.Trim().Equals("none",    StringComparison.OrdinalIgnoreCase)) el.visible = false;
            if (styles.TryGetValue("text-align",  out v) && v.Trim().Equals("center",  StringComparison.OrdinalIgnoreCase)) el.centerText = true;
            if (styles.TryGetValue("white-space", out v))
            {
                string ws = v.Trim().ToLowerInvariant();
                if (ws == "nowrap") el.wordWrap = false;
                else if (ws == "normal" || ws == "pre-wrap") el.wordWrap = true;
            }
        }

        // ── Static slot substitution ─────────────────────────────────────────

        private static string applyStaticSlots(string raw, Dictionary<string, string> props)
        {
            if (props == null || props.Count == 0) return raw;
            foreach (var kv in props)
                raw = raw.Replace("{{" + kv.Key + "}}", kv.Value, StringComparison.Ordinal);
            return raw;
        }

        // ── Path resolution ──────────────────────────────────────────────────

        private static string resolvePath(string relativePath) =>
            Path.GetFullPath(Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory,
                "Content", "UI",
                relativePath.Replace('/', Path.DirectorySeparatorChar)));

        // ── Component system ─────────────────────────────────────────────────

        private static bool isNativeTag(string tag) => _nativeTags.Contains(tag);
        private static bool isReservedAttr(string attr) => _reservedAttrs.Contains(attr);

        // Searches for {tag}.html in the component search order:
        //   1. {currentDir}/components/{tag}.html
        //   2. {currentDir}/{tag}.html
        //   3. Extra directories registered via addComponentPath()
        private static string resolveComponentPath(string tag, string currentDir)
        {
            string fileName = tag.ToLowerInvariant() + ".html";

            string candidate = Path.Combine(currentDir, "components", fileName);
            if (File.Exists(candidate)) return candidate;

            candidate = Path.Combine(currentDir, fileName);
            if (File.Exists(candidate)) return candidate;

            foreach (string extra in _extraComponentPaths)
            {
                candidate = Path.Combine(extra, fileName);
                if (File.Exists(candidate)) return candidate;
            }

            return null;
        }

        // Forwards one component attribute into the child BindingContext.
        // Three cases: static string, single live key (forwarded by reference),
        // composite template (assembled getter from parent resolvers).
        private static void forwardAttr(string name, string value,
                                        BindingContext parent, BindingContext child)
        {
            var liveKeys = StyleParser.extractLiveKeys(value);
            if (liveKeys.Count == 0)
            {
                var cv = value;
                child.bindText(name, () => cv);
                return;
            }
            if (liveKeys.Count == 1 && StyleParser.isSingleLiveKey(value, out string singleKey))
            {
                if (parent != null) parent.forwardTo(singleKey, name, child);
                else { var cv = value; child.bindText(name, () => cv); }
                return;
            }
            // Composite template — assemble a getter from parent resolvers.
            var pairs = new List<(string ph, Func<string> g)>();
            if (parent != null)
            {
                foreach (string k in liveKeys)
                {
                    var g = parent.resolveTextGetter(k);
                    if (g != null) pairs.Add(("{" + k + "}", g));
                }
            }
            var tmpl = value;
            child.bindText(name, () =>
            {
                string result = tmpl;
                foreach (var (ph, g) in pairs) result = result.Replace(ph, g());
                return result;
            });
        }

        // Loads a component file, wires a scoped child context with forwarded
        // props, builds the element tree, then merges child pollers into the
        // parent so parent.bindings.update() drives everything.
        private static UIElement buildComponent(XElement node, string tag, string currentDir,
                                                StyleSheet parentSheet, UIContext ctx,
                                                Dictionary<string, UIElement> idMap)
        {
            string path = resolveComponentPath(tag, currentDir);
            if (path == null)
            {
                LoggingUtil.info($"[UIHtmlLoader] component '{tag}' not found; skipped");
                return null;
            }

            string absPath = Path.GetFullPath(path);
            _loadingStack ??= new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!_loadingStack.Add(absPath))
            {
                LoggingUtil.info($"[UIHtmlLoader] circular component reference: {tag}");
                return null;
            }

            try
            {
                var childBindings = new BindingContext();
                var childCtx = new UIContext(childBindings, ctx?.interactions ?? new InteractionContext());

                foreach (var attr in node.Attributes())
                {
                    string attrName = attr.Name.LocalName;
                    if (isReservedAttr(attrName)) continue;
                    forwardAttr(attrName, attr.Value.Trim(), ctx?.bindings, childBindings);
                }

                string raw = File.ReadAllText(absPath);
                string compDir = Path.GetDirectoryName(absPath);

                XDocument doc;
                try { doc = XDocument.Parse(raw); }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"[UIHtmlLoader] invalid XML in component '{absPath}': {ex.Message}", ex);
                }

                // Inherit the parent stylesheet so component elements resolve the same CSS rules.
                // Any <link> tags inside the component append additional rules to the shared sheet.
                UIElement compRoot = buildElement(doc.Root, compDir, parentSheet, childCtx, idMap);

                if (compRoot != null)
                {
                    // Apply usage-site class/style onto the component root.
                    string usageClass = (string)node.Attribute("class") ?? "";
                    string usageStyle = (string)node.Attribute("style") ?? "";
                    if (!string.IsNullOrEmpty(usageClass) || !string.IsNullOrEmpty(usageStyle))
                        applyStyle(compRoot, parentSheet.resolve("", usageClass, usageStyle), ctx);

                    string id = ((string)node.Attribute("id") ?? "").Trim();
                    if (!string.IsNullOrEmpty(id)) idMap[id] = compRoot;
                }

                if (ctx?.bindings != null)
                    ctx.bindings.mergePollers(childBindings);

                return compRoot;
            }
            finally
            {
                _loadingStack.Remove(absPath);
            }
        }
    }

}
