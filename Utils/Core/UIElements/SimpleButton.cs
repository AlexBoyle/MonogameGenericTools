namespace Utils.Core.UIElements
{
    public class SimpleButton : ButtonBase
    {
        protected Vector2 textPosition = Vector2.Zero;
        protected float textScale = 2f;
        protected TextElement textElement;

        public SimpleButton()
        {
            color = Color.White;
            setJustify(Justify.CENTER);
            setText("");
        }
        public SimpleButton(string str)
        {
            color = Color.White;
            setJustify(Justify.CENTER);
            setText(str);

        }
        public override void update(GameTime gt)
        {
            if (isPressed)
            {
                color = Color.Gray;
            }
            else if (isHovered)
            {
                color = Color.Beige;
            }
            else
            {
                color = Color.Blue;
            }
            base.update(gt);
        }

        public void setText(string str)
        {
            if (textElement == null)
            {
                textElement = new(str);
                textElement.color = Color.White;
                this.addElement(textElement);
            }
            else
            {
                textElement.setText(str);
            }


        }


        public override void draw(GameTime gt)
        {
            Globals.spriteBatch.Draw(whiteRectangle, renderedPosition, null, color, 0f, Vector2.Zero, screenDimentionsInPixles, SpriteEffects.None, zIndex);
            base.draw(gt);
        }
    }
}
