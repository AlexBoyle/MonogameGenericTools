namespace Utils.Core.UIElements {
	using Microsoft.Xna.Framework;
	using Utils.Core.GlobalUtilities;

	public class Box : UIElement {
		static Texture2D whiteRectangle = null;

		public Box() {
			if (whiteRectangle == null) {
				try {
					whiteRectangle = Globals.GetNewTexture2D(1, 1);
					whiteRectangle.SetData(new[] { Color.White });
				}
				catch (System.Exception e) {
					Debug.WriteLine(e);
				}
			}
		}


		public override void update(GameTime gt) {
			base.update(gt);
		}


		public override void draw(GameTime gt) {
			Globals.spriteBatch.Draw(whiteRectangle, realPosition, null, color, 0f, Vector2.Zero, new Vector2(realWidth, realHeight), SpriteEffects.None, zIndex);
			base.draw(gt);
		}



	}
}
