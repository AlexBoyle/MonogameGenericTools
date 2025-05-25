namespace Utils.Core.GlobalUtilities;

using Microsoft.Xna.Framework.Graphics;
using Utils.Core;

public static class Globals
{
    public static GraphicsDevice graphicsDevice { get; private set; } = null;
    public static SpriteBatch spriteBatch { get; private set; } = null;
    public static void initialize(Game game)
    {
        graphicsDevice = game.GraphicsDevice;
        spriteBatch = new SpriteBatch(game.GraphicsDevice);
    }

    public static Texture2D GetNewTexture2D(int w, int h, Color? color = null)
    {
        if (color == null)
        {
            color = Color.White;

        }
        if (graphicsDevice != null)
        {
            Texture2D tex = new Texture2D(graphicsDevice, w, h);
            Color[] data = new Color[w * h];
            for (int i = 0; i < data.Length; ++i)
            {
                data[i] = color.Value;
            }
            tex.SetData(data);
            return tex;
        }
        return null;
    }




}
