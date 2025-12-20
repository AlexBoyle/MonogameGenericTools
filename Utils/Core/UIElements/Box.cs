namespace MonoTools.Core.UIElements {
	using Microsoft.Xna.Framework;
	using MonoTools.Core.GlobalUtilities;

	public class Box : UIElement {
		static Texture2D whiteRectangle = null;

		public Box() {
			if (whiteRectangle == null) {
				try {
					whiteRectangle = Globals.GetNewTexture2D(1, 1);
					whiteRectangle.SetData(new[] { Color.White });
				}
				catch (System.Exception e) {
					LoggingUtil.err(e.Message);
				}
			}
		}


		public override void update(GameTime gt) {
			base.update(gt);
		}


		public override void draw(GameTime gt) {
			Globals.spriteBatch.Draw(whiteRectangle, renderedPosition, null, color, 0f, Vector2.Zero, screenDimentionsInPixles, SpriteEffects.None, zIndex);
			base.draw(gt);
		}



	}
}
