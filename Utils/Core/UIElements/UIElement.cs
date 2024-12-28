namespace Utils.Core.UIElements {
	using Utils.Core.Scene;
	using System.Collections.Generic;

	public class UIElement : GameObject {
		protected float zIndex = .1f;
		public string name { get; protected set; }
		public Vector2 position { get; protected set; } = Vector2.Zero;
		public Vector2 realPosition { get; protected set; } = Vector2.Zero;
		public int width { get; protected set; } = 0;
		public int height { get; protected set; } = 0;
		public int realWidth { get; protected set; } = 0;
		public int realHeight { get; protected set; } = 0;
		public Unit posUnit { get; protected set; } = Unit.PX;
		public Unit dimUnit { get; protected set; } = Unit.PX;
		public int maxWidth { get; protected set; } = 0;
		public int maxHeight { get; protected set; } = 0;
		public int minWidth { get; protected set; } = 0;
		public int minHeight { get; protected set; } = 0;

		protected bool isHovered = false;
		protected bool isPressed = false;
		protected bool isPressedLast = false;
		public Texture2D texture { get; protected set; } = null;
		private Rectangle bounds = new();
		public Orientation orientation { get; protected set; } = Orientation.VERTICAL;
		public Justify justify { get; protected set; } = Justify.LEFT;
		public Color color { get; set; } = Color.Transparent;

		public UIElement parent { get; protected set; } = null;
		public List<UIElement> children { get; protected set; } = new();
		public UIElement() {
			UIElementInteractionHookUtility.addElement(this);
			this.drawTiming = DrawType.WITHOUT_CAMERA;
			this.updateTiming = Timing.BEFORE;
		}
		public UIElement setPosition(int x, int y) {
			position = new(x, y);
			return this;
		}
		public void setPosition(Point p) {
			setPosition(p.X, p.Y);
		}
		public override void draw(GameTime gt) {
			foreach (UIElement child in children) {
				child.draw(gt);
			}
		}
		public override void update(GameTime gt) {
			foreach (UIElement child in children) {
				child.update(gt);
			}
		}

		public virtual void onClick() {

		}
		protected void setParent(UIElement el) {
			this.parent = el;
			updateRealDimentions();
			updateRealPosition();
		}
		public UIElement addElement(UIElement uIElement) {
			uIElement.setParent(this);
			children.Add(uIElement);
			return this;
		}
		protected virtual void updateRealDimentions() {
			if (dimUnit == Unit.PX) {
				realHeight = height;
				realWidth = width;
			}
			else if (dimUnit == Unit.PER && parent != null) {
				realWidth = (int)(parent.realWidth * ((float)width / 100f));
				realHeight = (int)(parent.realHeight * ((float)height / 100f));
			}
			updateBounds();
			foreach (UIElement child in children) {
				child.updateRealDimentions();
			}

		}
		protected virtual void updateBounds() {
			bounds.Width = realWidth;
			bounds.Height = realHeight;
			bounds.Location = new((int)realPosition.X, (int)realPosition.Y);
			return;
		}
		protected virtual void updateRealPosition() {
			float x = 0;
			float y = 0;
			if (parent != null) {
				zIndex = parent.zIndex - .001f;
				Vector2 pos = parent.getRealPosition();
				x += pos.X;
				y += pos.Y;

				foreach (UIElement child in parent.children) {
					if (this == child) { break; }
					if (parent.orientation == Orientation.HORIZONTAL) {
						x += child.realWidth + child.position.X;
					}
					else {
						y += child.realHeight + child.position.Y;
					}
				}

			}
			int tempX = 0;
			int tempY = 0;
			if (posUnit == Unit.PX) {
				tempX = (int)position.X;
				tempY = (int)position.Y;
			}
			else if (posUnit == Unit.PER && parent != null) {
				tempX = (int)(parent.realWidth * (position.X / 100f));
				tempY = (int)(parent.realHeight * (position.Y / 100f));
			}
			if (parent != null && parent.orientation == Orientation.VERTICAL) {
				if (parent.justify == Justify.RIGHT) {
					tempX = (int)((parent.realWidth) - (realWidth + tempX));
					tempX = tempX;
				}
			}
			x += tempX;
			y += tempY;
			realPosition = new Vector2((int)x, (int)y);
			updateBounds();
			foreach (UIElement child in children) {
				child.updateRealPosition();
			}
		}

		public Vector2 getRealPosition() {
			return realPosition;
		}
		public UIElement setPosition(int x, int y, Unit unit = Unit.PX) {
			position = new Vector2(x, y);
			posUnit = unit;
			updateRealDimentions();
			updateRealPosition();
			return this;
		}
		public UIElement setDimentions(int x, int y, Unit unit = Unit.PX, int maxX = int.MaxValue, int minX = 0, int maxY = int.MaxValue, int minY = 0) {
			width = x;
			height = y;
			dimUnit = unit;
			maxHeight = maxY;
			maxWidth = maxX;
			minHeight = minY;
			minWidth = minX;
			updateRealDimentions();
			updateRealPosition();
			return this;
		}
		public UIElement setOrientation(Orientation orientation) {
			this.orientation = orientation;
			return this;
		}
		public Rectangle getBounds() {
			return bounds;
		}
		public virtual void isBeingHovered() {
			isHovered = true;
		}
		public virtual void isNotBeingHovered() {
			isHovered = false;
		}
		public virtual void isBeingPressed() {
			isPressedLast = isPressed;
			isPressed = true;
		}
		public virtual void isNotBeingPressed() {
			isPressedLast = isPressed;
			isPressed = false;
		}
		public virtual void resetPressed() {
			isPressedLast = false;
			isPressed = false;
		}
		public UIElement setJustify(Justify j) {
			justify = j;
			return this;
		}
		~UIElement() {
			UIElementInteractionHookUtility.removeElement(this);
		}
	}
}
