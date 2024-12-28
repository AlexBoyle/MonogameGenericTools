namespace Utils.Core.Objects {
	using Utils.Core.GlobalUtilities;

	public class TestSquare : GameObject {

		Texture2D texture;
		double tra;
		Color color;
		Color colorA = Color.Red;
		Color colorB = Color.Blue;

		public TestSquare(int x, int y, Texture2D texture, double tra) {
			position = new Vector2(x, y);
			this.texture = texture;
			this.tra = tra;
			this.color = lerp(colorA, colorB, tra);
		}


		private Color lerp(Color a, Color b, double l) {
			Color c = new Color();
			c.R = (byte)((double)a.R - (((double)a.R - (double)b.R) * l));
			c.G = (byte)((double)a.G - (((double)a.G - (double)b.G) * l));
			c.B = (byte)((double)a.B - (((double)a.B - (double)b.B) * l));
			c.A = (byte)((double)a.A - (((double)a.A - (double)b.A) * l));

			return c;
		}

		public override void draw(GameTime gameTime) {
			Globals.spriteBatch.Draw(texture, position, color);
		}
	}
}
