namespace Utils.Core.GlobalUtilities;

using Microsoft.Xna.Framework.Graphics;
using Utils.Core;

public static class Globals {
	public static GraphicsDevice graphicsDevice { get; private set; } = null;
	public static SpriteBatch spriteBatch { get; private set; } = null;
	public static void initialize(Game game) {
		graphicsDevice = game.GraphicsDevice;
		spriteBatch = new SpriteBatch(game.GraphicsDevice);
	}

	public static Texture2D GetNewTexture2D(int w, int h) {
		if (graphicsDevice != null) {
			return new Texture2D(graphicsDevice, w, h);
		}
		return null;
	}




}
