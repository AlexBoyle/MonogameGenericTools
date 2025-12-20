namespace MonoTools.Core.UIElements {
	public class TextElement : UIElement {
		private String text = "";
		private SpriteFont font = null;
		public Vector2 offset = Vector2.Zero;
		private Vector2 aproxTextRenderSize = Vector2.Zero;
		private float textScale = .3f;
		public TextElement(String text, String fontName = "fonts/Vipnagorgialla") {
			this.color = Color.Black;
			this.text = text;
			font = ContentUtility.get<SpriteFont>(fontName);
			aproxTextRenderSize = font.MeasureString(text);
			// This can be expensive, so we may need a diffrent approach for constently updating text
			// or we just never update this, or only force update it
			//ie we ignore the default dimentions set below if we dont care about it
			setDimentions(getAprSizeInPixles());
		}

		public TextElement setTextScaling(float f, bool updateDim = true) {
			textScale = f;
			if (updateDim) {
				setDimentions(getAprSizeInPixles());
			}
			return this;
		}


		public TextElement setText(String text, bool updateDim = false) {
			this.text = text;
			if (updateDim) {
				aproxTextRenderSize = font.MeasureString(text);
				setDimentions(getAprSizeInPixles());
			}
			return this;
		}

		public Vector2 getAprSizeInPixles() {
			return aproxTextRenderSize * textScale;
		}

		public override void draw(GameTime gt) {
			Globals.spriteBatch.DrawString(
				spriteFont: font,
				text: text,
				position: renderedPosition + offset,
				color: color,
				rotation: 0f,
				origin: Vector2.Zero,
				scale: textScale,
				effects: SpriteEffects.None,
				layerDepth: zIndex);
			base.draw(gt);
		}
	}
}
