namespace MonoTools.Core
{
    using MonoTools.Core.GlobalUtilities;

    public static class DebugUtility
    {

        static DebugUtility()
        {

        }
        public static Texture2D getSolidTexture(Color color, int width, int height)
        {
            Texture2D output = Globals.GetNewTexture2D(width, height);
            Color[] outCol = new Color[width * height];
            for (int i = 0; i < outCol.Length; i++)
            {
                outCol[i] = color;
            }
            output.SetData(outCol);
            return output;
        }

    }
}
