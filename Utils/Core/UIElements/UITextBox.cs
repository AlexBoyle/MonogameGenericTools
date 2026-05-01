namespace MonoTools.Core.UIElements {
	using System;
	using MonoTools.Core.GlobalUtilities;

	public class UITextBox : UIElement {
		private static UITextBox _focused = null;
		public static bool hasFocus => _focused != null;

		// Clears focus without going through a specific instance — call when a scene unloads.
		public static void clearFocus() { _focused?.loseFocus(); _focused = null; }

		private string _value       = "";
		private string _placeholder = "";
		public Action<string> onChange { get; set; } = null;

		// Key-repeat state for Backspace/Delete
		private Keys   _heldKey     = Keys.None;
		private double _heldTime    = 0;
		private double _nextRepeat  = 0;
		private const double RepeatDelay    = 0.40;
		private const double RepeatInterval = 0.05;

		private static readonly Color DefaultBorder  = new Color(70,  70,  90);
		private static readonly Color FocusedBorder  = new Color(120, 140, 200);
		private static readonly Color PlaceholderCol = new Color(100, 100, 120);

		public UITextBox(string placeholder = "") {
			_placeholder = placeholder;
			interactive  = true;
			color        = new Color(35, 35, 50);
			borderWidth  = 1f;
			borderColor  = DefaultBorder;
			textColor    = Color.White;
			setPadding(6, 4, 6, 4);
			setHeight(36);
			onClick = _ => gainFocus();
		}

		public string value {
			get => _value;
			set => _value = value ?? "";
		}

		public void setPlaceholder(string text) => _placeholder = text;

		// ── Update ───────────────────────────────────────────────────────────

		public override void update(GameTime gt) {
			base.update(gt);
			if (_focused != this) return;

			// Click outside = blur
			if (InputUtility.wasMouse1Pressed() &&
			    !getBoundsOnScreen().Contains(InputUtility.getMousePosition())) {
				loseFocus();
				return;
			}

			InputUtility.disableM1();
			handleSpecialKeys(gt);
			consumeTextInput();
		}

		private void handleSpecialKeys(GameTime gt) {
			if (InputUtility.wasKeyPressed(Keys.Escape) || InputUtility.wasKeyPressed(Keys.Enter)) {
				InputUtility.setKeyInputConsumption(Keys.Escape);
				InputUtility.setKeyInputConsumption(Keys.Enter);
				loseFocus();
				return;
			}

			Keys repeatKey = Keys.None;
			if      (InputUtility.isKeyDown(Keys.Back))   repeatKey = Keys.Back;
			else if (InputUtility.isKeyDown(Keys.Delete)) repeatKey = Keys.Delete;

			if (repeatKey != Keys.None) {
				double dt = gt.ElapsedGameTime.TotalSeconds;
				if (repeatKey != _heldKey) {
					_heldKey   = repeatKey;
					_heldTime  = 0;
					_nextRepeat = RepeatDelay;
					applyDeleteKey(repeatKey);
				} else {
					_heldTime += dt;
					if (_heldTime >= _nextRepeat) {
						_nextRepeat += RepeatInterval;
						applyDeleteKey(repeatKey);
					}
				}
				InputUtility.setKeyInputConsumption(Keys.Back);
				InputUtility.setKeyInputConsumption(Keys.Delete);
			} else if (_heldKey == Keys.Back || _heldKey == Keys.Delete) {
				_heldKey = Keys.None;
			}
		}

		private void applyDeleteKey(Keys key) {
			if (key == Keys.Back && _value.Length > 0) {
				_value = _value.Substring(0, _value.Length - 1);
				onChange?.Invoke(_value);
			} else if (key == Keys.Delete && _value.Length > 0) {
				_value = "";
				onChange?.Invoke(_value);
			}
		}

		private void consumeTextInput() {
			bool changed = false;
			while (InputUtility.dequeueTextInput(out char c)) {
				if (!char.IsControl(c)) { _value += c; changed = true; }
			}
			if (changed) onChange?.Invoke(_value);
		}

		// ── Draw ─────────────────────────────────────────────────────────────

		public override void draw(GameTime gt) {
			if (!visible) return;
			base.draw(gt);  // background, border, children

			SpriteFont font = resolveFont();
			if (font == null) return;

			bool   showPlaceholder = _value.Length == 0 && _focused != this;
			string display = showPlaceholder ? _placeholder : _value;
			Color  col     = showPlaceholder ? PlaceholderCol : textColor;
			if (display.Length == 0) return;

			string safe = sanitizeForFont(display, font);
			if (safe.Length == 0) return;

			float contentH = screenDimensionsInPixels.Y - padding.top - padding.bottom;
			float textH    = font.LineSpacing * fontSize;
			float x = MathF.Round(renderedPosition.X + padding.left);
			float y = MathF.Round(renderedPosition.Y + padding.top + (contentH - textH) / 2f);
			Globals.spriteBatch.DrawString(font, safe, new Vector2(x, y), col,
				0f, Vector2.Zero, fontSize, SpriteEffects.None, zIndex - 0.0002f);
		}

		// ── Focus management ─────────────────────────────────────────────────

		private void gainFocus() {
			if (_focused == this) return;
			_focused?.loseFocus();
			_focused    = this;
			InputUtility.clearTextInput();
			InputUtility.disableM1();
			borderColor = FocusedBorder;
		}

		private void loseFocus() {
			if (_focused != this) return;
			_focused    = null;
			borderColor = DefaultBorder;
			_heldKey    = Keys.None;
		}
	}
}
