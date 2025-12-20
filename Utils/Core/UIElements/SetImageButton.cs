namespace MonoTools.Core.UIElements
{
    public class SetImageButton : ButtonBase
    {
        public List<Rectangle?> buttonStates = new List<Rectangle?>();
        public int activeButtonState = 0;


        public SetImageButton(Texture2D texture, Rectangle initalImage, int numberOfStates)
        {
            List<Rectangle?> buttonStatesToSet = new List<Rectangle?>();
            for (int i = 0; i < numberOfStates; i++)
            {
                buttonStatesToSet.Add(new Rectangle(initalImage.X + (initalImage.Width * i), initalImage.Y, initalImage.Width, initalImage.Height));
            }
            buttonStatesToSet.Add(initalImage);
            setInitalState(texture, buttonStatesToSet);
        }



        protected void setInitalState(Texture2D texture, List<Rectangle?> buttonStates)
        {
            this.texture = texture;
            this.buttonStates = buttonStates;
        }

        public override void update(GameTime gt)
        {
            if (isPressed)
            {
                activeButtonState = 2;
            }
            else if (isHovered)
            {
                activeButtonState = 1;
            }
            else
            {
                activeButtonState = 0;
            }

            base.update(gt);
        }




        public override void draw(GameTime gt)
        {
            //Vector2 scale = new Vector2(width, height);
            //scale = new Vector2((float)width / buttonStates[activeButtonState].GetValueOrDefault().Width, (float)height / buttonStates[activeButtonState].GetValueOrDefault().Height);
            Globals.spriteBatch.Draw(
                texture: texture,
                sourceRectangle: buttonStates[activeButtonState],
                color: color,
                rotation: 0f,
                origin: Vector2.Zero,
                destinationRectangle: new Rectangle(renderedPosition.ToPoint(), screenDimentionsInPixles.ToPoint()),
                effects: SpriteEffects.None,
                layerDepth: zIndex
            );
            //base.draw(gt);
        }
    }
}
