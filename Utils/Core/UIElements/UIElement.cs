namespace Utils.Core.UIElements
{
    using Utils.Core.Scene;
    using System.Collections.Generic;

    public class UIElement : GameObject
    {
        protected static Texture2D whiteRectangle = null;
        protected static SpriteFont defaultFont = null;
        protected float zIndex = .1f;
        public bool willRenderInViewport = false;
        public string name { get; set; }
        public Vector2 screenPosition { get; protected set; } = Vector2.Zero;
        public Vector2 renderedPosition { get; protected set; } = Vector2.Zero;


        private Vector2 screenDimentions = Vector2.Zero;
        public Vector2 screenDimentionsInPixles { get; protected set; } = Vector2.Zero;

        private static int nextElementId = 0;
        public int elementId { get; private set; } = -1;



        private Unit posUnitW = Unit.PX;
        private Unit posUnitH = Unit.PX;
        private Unit dimUnitW = Unit.PX;
        private Unit dimUnitH = Unit.PX;



        public Texture2D texture { get; set; } = null;
        //private Rectangle bounds = new();
        public Orientation orientation { get; protected set; } = Orientation.VERTICAL;
        public Justify justify { get; protected set; } = Justify.LEFT;
        public Color color { get; set; } = Color.Transparent;

        public UIElement parent { get; protected set; } = null;
        public List<UIElement> children { get; protected set; } = new();

        static UIElement()
        {
            // Defaults
            whiteRectangle = DebugUtility.getSolidTexture(Color.White, 1, 1);
            defaultFont = ContentUtility.getAndSave<SpriteFont>("fonts/file");
        }
        public UIElement()
        {
            this.elementId = nextElementId++;
            this.name = "UIElement-" + elementId;
            this.drawTiming = DrawType.WITHOUT_CAMERA;
            this.updateTiming = Timing.BEFORE;
        }
        public UIElement setPosition(int x, int y)
        {
            position = new(x, y);
            return this;
        }
        public void setPosition(Point p)
        {
            setPosition(p.X, p.Y);
        }
        public override void draw(GameTime gt)
        {
            foreach (UIElement child in children)
            {
                child.draw(gt);
            }
        }

        public virtual void customDraw(GameTime gt)
        {
            foreach (UIElement child in children)
            {
                child.customDraw(gt);
            }
        }
        public override void update(GameTime gt)
        {


            foreach (UIElement child in children)
            {
                child.update(gt);
            }
        }

        public void setParent(UIElement el)
        {
            this.parent = el;
            updateRealDimentions();
            updatePositions();
        }
        public virtual UIElement addElement(UIElement uIElement)
        {
            uIElement.setParent(this);
            children.Add(uIElement);
            return this;
        }
        protected virtual void updateRealDimentions()
        {
            float _realWidth = 0;
            float _realHeight = 0;
            if (dimUnitW == Unit.PX)
            {
                _realWidth = screenDimentions.X;
            }
            else if (dimUnitW == Unit.PER && parent != null)
            {
                _realWidth = (int)(parent.screenDimentionsInPixles.X * (screenDimentions.X / 100f));
            }
            if (dimUnitH == Unit.PX)
            {
                _realHeight = screenDimentions.Y;
            }
            else if (dimUnitH == Unit.PER && parent != null)
            {
                _realHeight = (int)(parent.screenDimentionsInPixles.Y * (screenDimentions.Y / 100f));
            }
            screenDimentionsInPixles = new(_realWidth, _realHeight);

            foreach (UIElement child in children)
            {
                child.updateRealDimentions();
            }

        }
        protected virtual void updatePositions()
        {
            // Screen Positon
            float spX = 0;
            float spY = 0;

            // rendered position
            float rpX = 0;
            float rpY = 0;

            if (parent != null)
            {
                zIndex = parent.zIndex - .001f;
                spX += parent.screenPosition.X;
                spY += parent.screenPosition.Y;
                if (!willRenderInViewport)
                {
                    rpX += parent.renderedPosition.X;
                    rpY += parent.renderedPosition.Y;
                }
                foreach (UIElement child in parent.children)
                {
                    if (this == child) { break; }
                    if (parent.orientation == Orientation.HORIZONTAL)
                    {
                        spX += child.screenDimentionsInPixles.X + child.position.X;
                        rpX += child.screenDimentionsInPixles.X + child.position.X;
                    }
                    else if (parent.orientation == Orientation.VERTICAL)
                    {
                        spY += child.screenDimentionsInPixles.Y + child.position.Y;
                        rpY += child.screenDimentionsInPixles.Y + child.position.Y;
                    }
                    else if (parent.orientation == Orientation.FREE_INSIDE) { }
                }
            }

            int tempX = calculateWidth();
            int tempY = calculateHeight();

            if (this.elementId == 59)
            {
                int a = 1;
            }


            if (parent != null && parent.orientation == Orientation.VERTICAL)
            {
                if (parent.justify == Justify.RIGHT)
                {
                    tempX = (int)((parent.screenDimentionsInPixles.X) - (screenDimentionsInPixles.X + tempX));
                }
                if (parent.justify == Justify.CENTER)
                {
                    tempX = (int)((parent.screenDimentionsInPixles.X) - (screenDimentionsInPixles.X + tempX)) / 2;
                }
            }
            spX += tempX;
            spY += tempY;

            rpX += tempX;
            rpY += tempY;

            screenPosition = new Vector2((int)spX, (int)spY);
            renderedPosition = new Vector2((int)rpX, rpY);
            foreach (UIElement child in children)
            {
                child.updatePositions();
            }
        }

        private int calculateHeight()
        {
            int output = 0;
            if (posUnitH == Unit.PX)
            {
                output = (int)position.Y;
            }
            else if (posUnitH == Unit.PER && parent != null)
            {
                output = (int)(parent.screenDimentionsInPixles.Y * (position.Y / 100f));
            }
            return output;
        }
        private int calculateWidth()
        {
            int output = 0;
            if (posUnitH == Unit.PX)
            {
                output = (int)position.X;
            }
            else if (posUnitH == Unit.PER && parent != null)
            {
                output = (int)(parent.screenDimentionsInPixles.X * (position.X / 100f));
            }
            return output;
        }

        public UIElement setPosition(int x, int y, Unit unit = Unit.PX)
        {
            return setPosition(x, unit, y, unit);
        }

        public UIElement setPosition(int x, Unit unitW, int y, Unit unitH)
        {
            position = new Vector2(x, y);
            posUnitH = unitH;
            posUnitW = unitW;
            updateRealDimentions();
            updatePositions();
            return this;
        }

        public UIElement setDimentions(Vector2 vec, Unit unit = Unit.PX)
        {
            return setDimentions((int)vec.X, unit, (int)vec.Y, unit);
        }

        public UIElement setDimentions(int x, int y, Unit unit = Unit.PX)
        {
            return setDimentions(x, unit, y, unit);
        }

        public UIElement setDimentions(int x, Unit unitW, int y, Unit unitH)
        {
            screenDimentions = new(x, y);
            dimUnitH = unitH;
            dimUnitW = unitW;
            updateRealDimentions();
            updatePositions();
            return this;
        }
        public UIElement setOrientation(Orientation orientation)
        {
            this.orientation = orientation;
            return this;
        }
        public Rectangle getBoundsOnScreen()
        {
            return new(screenPosition.ToPoint(), screenDimentionsInPixles.ToPoint());
        }

        public UIElement setJustify(Justify j)
        {
            justify = j;
            return this;
        }


        ~UIElement()
        {

        }
    }
}
