namespace Utils.Core.GlobalUtilities
{
    using Microsoft.Xna.Framework.Content;
    using Utils.Core;

    public static class ContentUtility
    {

        private static ContentManager content { get; set; } = null;

        public static void initialize(Game game)
        {
            content = game.Content;
            content.RootDirectory = "Content";
        }

        public static T get<T>(string name) where T : class
        {
            return content.Load<T>(name) as T;
        }
    }
}
