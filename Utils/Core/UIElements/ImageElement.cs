namespace Utils.Core.UIElements
{
    public class ImageElement : UIElement
    {
        public ImageElement(String imageName)
        {
            texture = ContentUtility.getAndSave<Texture2D>(imageName);
        }
        public ImageElement(Texture2D texture)
        {
            this.texture = texture;

        }

        public override void draw(GameTime gt)
        {
            Globals.spriteBatch.Draw(
                texture: texture,
                sourceRectangle: null,
                destinationRectangle: new(renderedPosition.ToPoint(), screenDimentionsInPixles.ToPoint()),
                color: Color.White,
                rotation: 0f,
                origin: Vector2.Zero,
                effects: SpriteEffects.None,
                layerDepth: zIndex);
            base.draw(gt);
        }
    }
}
