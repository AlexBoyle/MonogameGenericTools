namespace Utils.Core.UIElements {
	using Utils.Core.GlobalUtilities;
	using System;

	public class Button : Box {

		public Color baseColor = Color.Blue;
		public Color hoverColor = Color.Purple;
		public Color pressedColor = Color.DarkBlue;
		protected static SpriteFont defaultFont = null;
		protected string buttonText = "TestText";
		protected Vector2 textPosition = Vector2.Zero;
		protected float textScale = 2f;
		protected Func<GameTime, bool> callbackRefrence = null;


		static Button() {
			defaultFont = ContentUtility.get<SpriteFont>("fonts/default");
		}
		public Button() { }
		public Button(string str) {
			color = baseColor;
			setText(str);
		}
		public override void update(GameTime gt) {
			if (isPressed) {
				color = pressedColor;
			}
			else if (isHovered) {
				color = hoverColor;
			}
			else {
				color = baseColor;
			}
			if (!isPressed && isPressedLast && callbackRefrence != null) {// To Fix
				callbackRefrence(gt);
			}

			base.update(gt);
		}

		public void setText(string str) {
			buttonText = str;
			updateTextPosition();
		}
		protected override void updateBounds() {
			base.updateBounds();
			updateTextPosition();
		}
		public Button setCallback(Func<GameTime, bool> funcRef) {
			callbackRefrence = funcRef;
			return this;
		}

		protected void updateTextPosition() {
			Vector2 size = defaultFont.MeasureString(buttonText) * textScale;
			Vector2 cornerOfRect = new Vector2(getBounds().Left, getBounds().Top);
			Vector2 dimensionsOfRect = new Vector2(getBounds().Width, getBounds().Height);
			Vector2 offsetFromCornerOfRect = (dimensionsOfRect - size) / 2;
			textPosition = cornerOfRect + offsetFromCornerOfRect;
			textPosition.X = (int)textPosition.X;
			textPosition.Y = (int)textPosition.Y;

		}

		public override void draw(GameTime gt) {
			base.draw(gt);
			Globals.spriteBatch.DrawString(defaultFont, buttonText, textPosition, Color.White, 0f, Vector2.Zero, textScale, SpriteEffects.None, zIndex - .01f);


		}
	}
}
