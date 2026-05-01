namespace MonoTools.Core.UIElements {
	public class SetImageButton : UIElement {
		public List<Rectangle?> buttonStates = new List<Rectangle?>();
		public int activeButtonState = 0;

		public SetImageButton(Texture2D texture, Rectangle initialImage, int numberOfStates) {
			interactive = true;
			List<Rectangle?> states = new List<Rectangle?>();
			for (int i = 0; i < numberOfStates; i++) {
				states.Add(new Rectangle(initialImage.X + (initialImage.Width * i), initialImage.Y, initialImage.Width, initialImage.Height));
			}
			states.Add(initialImage);
			this.texture = texture;
			this.buttonStates = states;
		}

		public override void update(GameTime gt) {
			if (isPressed) activeButtonState = 2;
			else if (isHovered) activeButtonState = 1;
			else activeButtonState = 0;
			base.update(gt);
		}

		public override void draw(GameTime gt) {
			if (!visible) return;
			Globals.spriteBatch.Draw(
				texture: texture,
				sourceRectangle: buttonStates[activeButtonState],
				color: color,
				rotation: 0f,
				origin: Vector2.Zero,
				destinationRectangle: new Rectangle(renderedPosition.ToPoint(), screenDimensionsInPixels.ToPoint()),
				effects: SpriteEffects.None,
				layerDepth: zIndex
			);
		}
	}
}
