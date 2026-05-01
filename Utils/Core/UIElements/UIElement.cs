namespace MonoTools.Core.UIElements
{
    using MonoTools.Core.Scene;
    using System;
    using System.Collections.Generic;
    using System.Text;

    public class UIElement : GameObject
    {
        public bool shouldPrintDebug = false;
        public string debugName = "debugName";

        public UIElement enableDebug(string name) { debugName = name; shouldPrintDebug = true; return this; }
        protected static Texture2D whiteRectangle = null;
        protected static SpriteFont defaultFont = null;

        private static readonly Dictionary<SpriteFont, HashSet<char>> fontCharSets = new();

        protected float zIndex = .1f;
        public bool willRenderInViewport = false;
        public string name { get; set; }
        public Vector2 screenPosition { get; protected set; } = Vector2.Zero;
        public Vector2 renderedPosition { get; protected set; } = Vector2.Zero;

        private Vector2 screenDimensions = Vector2.Zero;
        public Vector2 screenDimensionsInPixels { get; protected set; } = Vector2.Zero;
        // Raw stored value before unit resolution. Non-zero means an explicit dimension was set.
        public Vector2 rawDimensions => screenDimensions;

        private static int nextElementId = 0;
        public int elementId { get; private set; } = -1;
        // Set true during batch tree construction to suppress layout cascades.
        internal static bool suspendLayout = false;

        private Unit posUnitW = Unit.PX;
        private Unit posUnitH = Unit.PX;
        private Unit dimUnitW = Unit.PX;
        private Unit dimUnitH = Unit.PX;

        // Visual
        public Texture2D texture { get; set; } = null;
        public Orientation orientation { get; private set; } = Orientation.COLUMN;
        public Justify justify { get; private set; } = Justify.START;
        public Color color { get; set; } = Color.Transparent;
        public float borderWidth { get; set; } = 0f;
        public Color borderColor { get; set; } = Color.Black;
        public bool visible { get; set; } = true;
        public bool clipToBounds { get; set; } = false;
        public bool wordWrap { get; set; } = false;
        private List<string> _wrappedLines = null;
        private float _wrapComputedAtWidth = -1f;
        // True when the element has word-wrap text whose wrap hasn't been computed
        // at the current resolved width — checked each frame in UILoadResult.update().
        internal bool needsRewrap =>
            wordWrap && text != null && screenDimensionsInPixels.X > 0 &&
            screenDimensionsInPixels.X != _wrapComputedAtWidth;

        protected static readonly RasterizerState scissorState = new RasterizerState
        {
            CullMode = CullMode.None,
            ScissorTestEnable = true
        };

        // Text
        public string text { get; private set; } = null;
        public float fontSize { get; private set; } = 0.3f;
        public Color textColor { get; set; } = Color.Black;
        public Vector2 textOffset { get; set; } = Vector2.Zero;
        public bool centerText { get; set; } = false;
        public string fontName { get; set; } = null;  // null uses the shared default font
        private SpriteFont _instanceFont = null;
        private string _loadedFontName = null;

        // Interactivity
        public bool interactive { get; set; } = false;
        public Color? hoverColor { get; set; } = null;
        public Color? pressedColor { get; set; } = null;
        public Action<GameTime> onClick { get; set; } = null;
        public Action<GameTime> onHover { get; set; } = null;
        public Action<GameTime> onHoverExit { get; set; } = null;
        protected bool isHovered = false;
        protected bool isPressed = false;
        protected bool isPressedLast = false;

        // Space outside this element (pushes it away from siblings / parent edge).
        public Spacing margin { get; private set; } = Spacing.Zero;
        // Space inside this element (offsets where children begin).
        public Spacing padding { get; private set; } = Spacing.Zero;
        // Extra space inserted between each flow child (px). Does not apply before the first or after the last.
        public float gap { get; private set; } = 0f;

        // Size constraints (px) applied after unit resolution. Defaults allow any size.
        public float minWidth { get; private set; } = 0f;
        public float maxWidth { get; private set; } = float.MaxValue;
        public float minHeight { get; private set; } = 0f;
        public float maxHeight { get; private set; } = float.MaxValue;

        public UIElement parent { get; internal set; } = null;
        public List<UIElement> children { get; protected set; } = new();

        static UIElement()
        {
            whiteRectangle = DebugUtility.getSolidTexture(Color.White, 1, 1);
        }

        public UIElement()
        {
            this.elementId = nextElementId++;
            this.name = "UIElement-" + elementId;
            this.drawTiming = DrawType.WITHOUT_CAMERA;
            this.updateTiming = Timing.BEFORE;
        }

        // ── Draw / Update / CustomDraw ────────────────────────────────────

        public override void draw(GameTime gt)
        {
            if (!visible) return;

            // Background
            Color bgColor = color;
            if (interactive)
            {
                if (isPressed && pressedColor.HasValue) bgColor = pressedColor.Value;
                else if (isHovered && hoverColor.HasValue) bgColor = hoverColor.Value;
            }
            if (bgColor != Color.Transparent)
                Globals.spriteBatch.Draw(whiteRectangle, renderedPosition, null, bgColor, 0f, Vector2.Zero, screenDimensionsInPixels, SpriteEffects.None, zIndex);

            // Border
            if (borderWidth > 0 && borderColor != Color.Transparent)
            {
                int x = (int)renderedPosition.X;
                int y = (int)renderedPosition.Y;
                int w = (int)screenDimensionsInPixels.X;
                int h = (int)screenDimensionsInPixels.Y;
                int bw = (int)borderWidth;
                float bz = zIndex - 0.00005f;
                Globals.spriteBatch.Draw(whiteRectangle, new Rectangle(x, y, w, bw), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, bz);
                Globals.spriteBatch.Draw(whiteRectangle, new Rectangle(x, y + h - bw, w, bw), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, bz);
                Globals.spriteBatch.Draw(whiteRectangle, new Rectangle(x, y + bw, bw, h - 2 * bw), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, bz);
                Globals.spriteBatch.Draw(whiteRectangle, new Rectangle(x + w - bw, y + bw, bw, h - 2 * bw), null, borderColor, 0f, Vector2.Zero, SpriteEffects.None, bz);
            }

            // Texture
            if (texture != null)
                Globals.spriteBatch.Draw(
                    texture: texture,
                    destinationRectangle: new Rectangle(renderedPosition.ToPoint(), screenDimensionsInPixels.ToPoint()),
                    sourceRectangle: null,
                    color: Color.White,
                    rotation: 0f,
                    origin: Vector2.Zero,
                    effects: SpriteEffects.None,
                    layerDepth: zIndex - 0.0001f);

            // Text
            if (text != null)
            {
                SpriteFont font = resolveFont();
                if (font != null)
                {
                    if (_wrappedLines != null)
                    {
                        float lineH = font.LineSpacing * fontSize;
                        float contentW = screenDimensionsInPixels.X - padding.left - padding.right;
                        float y = renderedPosition.Y + padding.top;
                        foreach (string line in _wrappedLines)
                        {
                            string safe = sanitizeForFont(line, font);
                            float x = renderedPosition.X + padding.left;
                            if (centerText && safe.Length > 0)
                                x += (contentW - font.MeasureString(safe).X * fontSize) / 2f;
                            if (safe.Length > 0)
                            {
                                Vector2 pos = new Vector2(MathF.Round(x), MathF.Round(y)) + textOffset;
                                Globals.spriteBatch.DrawString(font, safe, pos, textColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, zIndex - 0.0002f);
                            }
                            y += lineH;
                        }
                    }
                    else
                    {
                        string safe = sanitizeForFont(text, font);
                        if (safe.Length > 0)
                        {
                            Vector2 drawPos = renderedPosition + textOffset;
                            if (centerText)
                            {
                                Vector2 textSize = font.MeasureString(safe) * fontSize;
                                drawPos = renderedPosition + (screenDimensionsInPixels - textSize) / 2f + textOffset;
                            }
                            drawPos = new Vector2(MathF.Round(drawPos.X), MathF.Round(drawPos.Y));
                            Globals.spriteBatch.DrawString(font, safe, drawPos, textColor, 0f, Vector2.Zero, fontSize, SpriteEffects.None, zIndex - 0.0002f);
                        }
                    }
                }
            }

            if (!clipToBounds)
            {
                foreach (UIElement child in children)
                {
                    child.draw(gt);
                }
            }
            // clipToBounds children are drawn in customDraw under a scissor rect
        }

        public virtual void customDraw(GameTime gt)
        {
            if (!visible) return;

            if (clipToBounds)
            {
                Rectangle prev = Globals.graphicsDevice.ScissorRectangle;
                Globals.graphicsDevice.ScissorRectangle = new Rectangle(
                    renderedPosition.ToPoint(), screenDimensionsInPixels.ToPoint());
                Globals.spriteBatch.Begin(
                    sortMode: SpriteSortMode.BackToFront,
                    blendState: BlendState.NonPremultiplied,
                    samplerState: SamplerState.PointClamp,
                    rasterizerState: scissorState);
                foreach (UIElement child in children)
                {
                    child.draw(gt);
                }
                Globals.spriteBatch.End();
                Globals.graphicsDevice.ScissorRectangle = prev;
            }

            foreach (UIElement child in children)
            {
                child.customDraw(gt);
            }
        }

        public override void update(GameTime gt)
        {
            if (!visible) return;

            Point mousePos = InputUtility.getMousePosition();
            Rectangle r = new(screenPosition.ToPoint(), screenDimensionsInPixels.ToPoint());

            if (interactive)
            {
                if (r.Contains(mousePos))
                {
                    if (InputUtility.isMouse1Down())
                    {
                        isPressedLast = isPressed;
                        isPressed = true;
                    }
                    else
                    {
                        isPressedLast = isPressed;
                        isPressed = false;
                    }
                    if (!isHovered)
                    {
                        isHovered = true;
                        onHover?.Invoke(gt);
                    }
                }
                else
                {
                    if (isHovered)
                    {
                        isHovered = false;
                        onHoverExit?.Invoke(gt);
                    }
                    isPressedLast = false;
                    isPressed = false;
                }

                if (!isPressed && isPressedLast)
                {
                    InputUtility.disableM1();
                    onClick?.Invoke(gt);
                }
            }

            foreach (UIElement child in children)
            {
                child.update(gt);
            }
        }

        // ── Tree management ───────────────────────────────────────────────

        public void setParent(UIElement el)
        {
            this.parent = el;
            updateRealDimensions();
            updatePositions();
        }

        public virtual UIElement addElement(UIElement uIElement)
        {
            uIElement.setParent(this);
            children.Add(uIElement);
            // Auto-height containers (rawDimensions.Y == 0) need to grow to fit the new child,
            // and the parent needs to reposition its siblings around the taller container.
            if (rawDimensions.Y == 0)
            {
                updateRealDimensions();
                parent?.updatePositions();
            }
            return this;
        }

        public virtual UIElement removeElement(UIElement uIElement)
        {
            if (children.Remove(uIElement))
            {
                uIElement.parent = null;
                if (rawDimensions.Y == 0) updateRealDimensions();
                if (parent != null) parent.updatePositions();
                else updatePositions();
            }
            return this;
        }

        // ── Layout calculation ────────────────────────────────────────────

        protected virtual void updateRealDimensions()
        {
            float _realWidth = 0;
            float _realHeight = 0;

            if (dimUnitW == Unit.PX)
            {
                _realWidth = screenDimensions.X;
            }
            else if (dimUnitW == Unit.PER && parent != null)
            {
                float contentW = parent.screenDimensionsInPixels.X - parent.padding.left - parent.padding.right;
                _realWidth = contentW * (screenDimensions.X / 100f);
            }
            if (parent != null && parent.orientation == Orientation.ROW && dimUnitW == Unit.PER)
            {
                _realWidth -= parent.getChildGapSpacingConsideration();
            }

            if (dimUnitH == Unit.PX)
            {
                // rawDimensions.Y == 0 means no explicit height was set — derive from children.
                _realHeight = (screenDimensions.Y == 0 && children.Count > 0)
                    ? computeContentHeight()
                    : screenDimensions.Y;
            }
            else if (dimUnitH == Unit.PER && parent != null)
            {
                float contentH = parent.screenDimensionsInPixels.Y - parent.padding.top - parent.padding.bottom;
                _realHeight = contentH * (screenDimensions.Y / 100f);
            }

            _realWidth = Math.Clamp(_realWidth, minWidth, maxWidth);
            _realHeight = Math.Clamp(_realHeight, minHeight, maxHeight);
            if (shouldPrintDebug)
            {
                LoggingUtil.info(debugName + "-" + elementId + " realWidth=" + _realWidth + " realHeight=" + _realHeight);
            }
            screenDimensionsInPixels = new(_realWidth, _realHeight);

            if (!suspendLayout)
            {
                foreach (UIElement child in children)
                    child.updateRealDimensions();
            }
        }

        protected virtual void updatePositions()
        {
            if (suspendLayout) return;
            float spX = 0;
            float spY = 0;
            float rpX = 0;
            float rpY = 0;

            if (parent != null)
            {
                zIndex = parent.zIndex - .001f;

                // Start inside parent's content area (offset by padding).
                spX = parent.screenPosition.X + parent.padding.left;
                spY = parent.screenPosition.Y + parent.padding.top;
                if (!willRenderInViewport)
                {
                    rpX = parent.renderedPosition.X + parent.padding.left;
                    rpY = parent.renderedPosition.Y + parent.padding.top;
                }

                // Accumulate space consumed by preceding siblings.
                foreach (UIElement child in parent.children)
                {
                    if (this == child) break;
                    if (parent.orientation == Orientation.ROW)
                    {
                        float sibW = child.margin.left + child.screenDimensionsInPixels.X + child.margin.right + child.calculateWidthOffset() + parent.gap;
                        spX += sibW;
                        rpX += sibW;
                    }
                    else if (parent.orientation == Orientation.COLUMN)
                    {
                        float sibH = child.margin.top + child.screenDimensionsInPixels.Y + child.margin.bottom + child.calculateHeightOffset() + parent.gap;
                        spY += sibH;
                        rpY += sibH;
                    }
                    // RELATIVE / ABSOLUTE: no automatic accumulation.
                }
            }

            int tempX = calculateWidthOffset();
            int tempY = calculateHeightOffset();

            // Cross-axis alignment within the parent's content area (accounting for padding).
            if (parent != null && parent.orientation == Orientation.COLUMN)
            {
                float contentW = parent.screenDimensionsInPixels.X - parent.padding.left - parent.padding.right;
                if (parent.justify == Justify.END)
                {
                    tempX = (int)(contentW - screenDimensionsInPixels.X - margin.left - margin.right);
                }
                else if (parent.justify == Justify.CENTER)
                {
                    tempX = (int)((contentW - screenDimensionsInPixels.X - margin.left - margin.right) / 2);
                }
            }
            else if (parent != null && parent.orientation == Orientation.ROW)
            {
                float contentH = parent.screenDimensionsInPixels.Y - parent.padding.top - parent.padding.bottom;
                if (parent.justify == Justify.END)
                {
                    tempY = (int)(contentH - screenDimensionsInPixels.Y - margin.top - margin.bottom);
                }
                else if (parent.justify == Justify.CENTER)
                {
                    tempY = (int)((contentH - screenDimensionsInPixels.Y - margin.top - margin.bottom) / 2);
                }
            }

            spX += margin.left + tempX;
            spY += margin.top + tempY;
            rpX += margin.left + tempX;
            rpY += margin.top + tempY;

            screenPosition = new Vector2((int)spX, (int)spY);
            // y is off for vars that are bound
            renderedPosition = new Vector2((int)rpX, rpY);
            if (shouldPrintDebug)
            {
                LoggingUtil.info(debugName + "-" + elementId + " POS: ");
                LoggingUtil.info(screenPosition.ToString());
                LoggingUtil.info(renderedPosition.ToString());
            }
            foreach (UIElement child in children)
            {
                child.updatePositions();
            }
        }

        // Returns this element's position.Y in pixels, accounting for unit and parent content area.
        private int calculateHeightOffset()
        {
            if (posUnitH == Unit.PX) return (int)position.Y;
            if (posUnitH == Unit.PER && parent != null)
            {
                float contentH = parent.screenDimensionsInPixels.Y - parent.padding.top - parent.padding.bottom;
                return (int)(contentH * (position.Y / 100f));
            }
            return 0;
        }

        // Returns this element's position.X in pixels, accounting for unit and parent content area.
        private int calculateWidthOffset()
        {
            if (posUnitW == Unit.PX) return (int)position.X;
            if (posUnitW == Unit.PER && parent != null)
            {
                float contentW = parent.screenDimensionsInPixels.X - parent.padding.left - parent.padding.right;
                return (int)(contentW * (position.X / 100f));
            }
            return 0;
        }

        // ── Setters (all chainable) ───────────────────────────────────────

        // Fast-path setPosition used internally (does not trigger layout recalc).
        public UIElement setPosition(int x, int y)
        {
            position = new(x, y);
            return this;
        }

        public void setPosition(Point p)
        {
            setPosition(p.X, p.Y);
        }

        public UIElement setPosition(int x, int y, Unit unit)
        {
            return setPosition(x, unit, y, unit);
        }

        public UIElement setPosition(int x, Unit unitW, int y, Unit unitH)
        {
            position = new Vector2(x, y);
            posUnitW = unitW;
            posUnitH = unitH;
            updateRealDimensions();
            updatePositions();
            return this;
        }

        public UIElement setDimensions(Vector2 vec, Unit unit = Unit.PX)
        {
            return setDimensions((int)vec.X, unit, (int)vec.Y, unit);
        }

        public UIElement setDimensions(int x, int y, Unit unit = Unit.PX)
        {
            return setDimensions(x, unit, y, unit);
        }

        public UIElement setDimensions(int x, Unit unitW, int y, Unit unitH)
        {
            screenDimensions = new(x, y);
            dimUnitW = unitW;
            dimUnitH = unitH;
            var prev = screenDimensionsInPixels;
            updateRealDimensions();
            updatePositions();
            if (!suspendLayout && screenDimensionsInPixels != prev)
                notifyParentOfResize();
            return this;
        }

        // When this element's resolved size changes, walk up auto-height ancestors
        // so they recompute their height and reposition their children.
        private void notifyParentOfResize()
        {
            UIElement p = parent;
            while (p != null && p.rawDimensions.Y == 0)
            {
                p.updateRealDimensions();
                p = p.parent;
            }
            p?.updatePositions();
        }

        public UIElement setWidth(int x, Unit unit = Unit.PX)
        {
            screenDimensions = new Vector2(x, screenDimensions.Y);
            dimUnitW = unit;
            updateRealDimensions();
            updatePositions();
            return this;
        }

        public UIElement setHeight(int y, Unit unit = Unit.PX)
        {
            screenDimensions = new Vector2(screenDimensions.X, y);
            dimUnitH = unit;
            updateRealDimensions();
            updatePositions();
            return this;
        }

        public UIElement setOrientation(Orientation orientation)
        {
            this.orientation = orientation;
            updatePositions();
            return this;
        }

        public UIElement setJustify(Justify j)
        {
            justify = j;
            updatePositions();
            return this;
        }

        public UIElement setMargin(int all)
        {
            margin = new Spacing(all);
            updatePositions();
            return this;
        }

        public UIElement setMargin(int left, int top, int right, int bottom)
        {
            margin = new Spacing(left, top, right, bottom);
            updatePositions();
            return this;
        }

        public UIElement setPadding(int all)
        {
            padding = new Spacing(all);
            updateRealDimensions();
            updatePositions();
            return this;
        }

        public UIElement setPadding(int left, int top, int right, int bottom)
        {
            padding = new Spacing(left, top, right, bottom);
            updateRealDimensions();
            updatePositions();
            return this;
        }

        public UIElement setGap(float px) { gap = px; updatePositions(); return this; }

        public UIElement setMinWidth(float px) { minWidth = px; updateRealDimensions(); updatePositions(); return this; }
        public UIElement setMaxWidth(float px) { maxWidth = px; updateRealDimensions(); updatePositions(); return this; }
        public UIElement setMinHeight(float px) { minHeight = px; updateRealDimensions(); updatePositions(); return this; }
        public UIElement setMaxHeight(float px) { maxHeight = px; updateRealDimensions(); updatePositions(); return this; }

        public UIElement setBorder(float width, Color color) { borderWidth = width; borderColor = color; return this; }
        public UIElement setBorderWidth(float px) { borderWidth = px; return this; }
        public UIElement setBorderColor(Color c) { borderColor = c; return this; }

        public UIElement setCallback(Func<GameTime, bool> funcRef)
        {
            onClick += gt => funcRef(gt);
            interactive = true;
            return this;
        }

        public Rectangle getBoundsOnScreen()
        {
            return new(screenPosition.ToPoint(), screenDimensionsInPixels.ToPoint());
        }

        // ── Text helpers ─────────────────────────────────────────────────────

        public UIElement setText(string value, bool resize = true)
        {
            text = value;
            if (wordWrap)
                updateWordWrap();
            else if (resize)
                sizeToText();
            return this;
        }

        public UIElement setFontSize(float scale, bool resize = false)
        {
            fontSize = scale;
            if (resize) sizeToText();
            return this;
        }

        // Measures current text and resizes the element to exactly fit it.
        // No-op when wordWrap is true — the element retains its set width and
        // updateWordWrap() controls the height.
        public UIElement sizeToText()
        {
            if (text == null || wordWrap) return this;
            SpriteFont font = resolveFont();
            if (font != null) setDimensions(font.MeasureString(text) * fontSize);
            return this;
        }

        // Recomputes wrapped lines from the current text and element width.
        // Sets height to fit the wrapped content when no explicit height was given.
        // Called by autoSize after width is resolved, and by setText on every text change.
        internal void updateWordWrap()
        {
            if (!wordWrap || text == null) return;
            SpriteFont font = resolveFont();
            if (font == null) return;
            float contentW = screenDimensionsInPixels.X - padding.left - padding.right;
            if (contentW <= 0) { _wrappedLines = null; return; }
            _wrappedLines = computeWrappedLines(text, font, fontSize, contentW);
            _wrapComputedAtWidth = screenDimensionsInPixels.X;
            if (rawDimensions.Y == 0)
            {
                float lineH = font.LineSpacing * fontSize;
                int newH = (int)Math.Ceiling(_wrappedLines.Count * lineH + padding.top + padding.bottom);
                if ((int)screenDimensionsInPixels.Y != newH)
                {
                    setHeight(newH);
                    notifyParentOfResize();
                }
            }
        }

        private static List<string> computeWrappedLines(string text, SpriteFont font, float scale, float maxWidth)
        {
            var lines = new List<string>();
            foreach (string paragraph in text.Split('\n'))
            {
                var current = new StringBuilder();
                foreach (string word in paragraph.Split(' '))
                {
                    if (word.Length == 0) continue;
                    string candidate = current.Length == 0 ? word : current + " " + word;
                    if (font.MeasureString(candidate).X * scale <= maxWidth)
                    {
                        if (current.Length > 0) current.Append(' ');
                        current.Append(word);
                    }
                    else
                    {
                        if (current.Length > 0) lines.Add(current.ToString());
                        current.Clear().Append(word);
                    }
                }
                lines.Add(current.Length > 0 ? current.ToString() : "");
            }
            return lines;
        }

        public Vector2 getTextSize()
        {
            if (text == null) return Vector2.Zero;
            SpriteFont font = resolveFont();
            return font != null ? font.MeasureString(text) * fontSize : Vector2.Zero;
        }

        public static UIElement MakeText(string content, float scale = 0.3f, string font = null)
        {
            var el = new UIElement();
            el.fontSize = scale;
            if (font != null) el.fontName = font;
            el.setText(content);
            return el;
        }

        // Strips characters not present in the font's character set.
        // Uses a cached HashSet per font so the Contains check is O(1).
        protected static string sanitizeForFont(string text, SpriteFont font)
        {
            if (!fontCharSets.TryGetValue(font, out HashSet<char> set))
            {
                set = new HashSet<char>(font.Characters);
                fontCharSets[font] = set;
            }
            for (int i = 0; i < text.Length; i++)
            {
                if (!set.Contains(text[i]))
                {
                    return new string(System.Array.FindAll(text.ToCharArray(), c => set.Contains(c)));
                }
            }
            return text;
        }

        // Returns the height this element's content occupies, including padding and child gaps.
        // Used for auto-height containers (rawDimensions.Y == 0) and for autoSize in the loader.
        public float computeContentHeight()
        {
            float h = padding.top + padding.bottom;
            if (orientation == Orientation.COLUMN)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    UIElement c = children[i];
                    h += c.margin.top + c.screenDimensionsInPixels.Y + c.margin.bottom;
                    if (i < children.Count - 1) h += gap;
                }
            }
            else
            {
                float maxH = 0;
                foreach (UIElement c in children)
                {
                    float ch = c.margin.top + c.screenDimensionsInPixels.Y + c.margin.bottom;
                    if (ch > maxH) maxH = ch;
                }
                h += maxH;
            }
            return h;
        }

        protected float getChildGapSpacingConsideration()
        {
            float numChildren = children.Count;
            if (numChildren == 0 || gap == 0) return 0;
            return ((numChildren - 1f) * gap) / numChildren;
        }

        // Returns the per-element font if fontName is set, otherwise the shared default.
        protected SpriteFont resolveFont()
        {
            if (fontName != _loadedFontName)
            {
                _instanceFont = fontName != null ? ContentUtility.get<SpriteFont>(fontName) : null;
                _loadedFontName = fontName;
            }
            if (_instanceFont != null) return _instanceFont;
            if (defaultFont == null) defaultFont = ContentUtility.get<SpriteFont>("fonts/Vipnagorgialla");
            return defaultFont;
        }

        // The natural line height of the default font at scale 1.0.
        // Used by the HTML loader to convert CSS px values to a scale multiplier.
        internal static float defaultFontLineSpacing()
        {
            if (defaultFont == null) defaultFont = ContentUtility.get<SpriteFont>("fonts/Vipnagorgialla");
            return defaultFont?.LineSpacing ?? 50f;
        }

    }
}
