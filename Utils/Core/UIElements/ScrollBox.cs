namespace MonoTools.Core.UIElements {


	public class ScrollBox : UIElement {
		private int scrollOffset = 0;
		private int scrollSpeed = 12;
		private int itemsWide = 2;
		private int itemWidth = 0;
		private int itemHeight = 0;
		private int itemPadding = 4;
		private UIElement scrollBox = null;
		public bool shouldManageChildren = true;


		public bool didScrollThisFrame { get; private set; } = false;
		public bool isMouseOnElement { get; private set; } = false;
		private int scrollBarHeight = 0;
		private int maxScrollBarTravelDist = 0;
		private int scrollBarOffset = 0;

		public ScrollBox() {
			name = "ScrollBoxContainer-" + elementId;
			setJustify(Justify.CENTER);
			scrollBox = new();
			scrollBox.name = "ScrollBox-" + scrollBox.elementId;
			setOrientation(Orientation.ABSOLUTE);
			scrollBox.setOrientation(Orientation.RELATIVE);
			scrollBox.willRenderInViewport = true;
			scrollBox.setParent(this);
			children.Add(scrollBox);
		}

		public ScrollBox setColumns(int columns) {
			itemsWide = columns;
			updateRealDimensions();
			relayoutChildren();
			return this;
		}

		protected override void updateRealDimensions() {
			base.updateRealDimensions();
			float contentW = screenDimensionsInPixels.X - padding.left - padding.right;
			itemWidth = (int)(contentW / itemsWide);
			itemHeight = (int)(contentW / itemsWide);
		}

		public override void update(GameTime gt) {
			if (!visible) { base.update(gt); return; }
			if (getBoundsOnScreen().Contains(InputUtility.getMousePosition())) {
				isMouseOnElement = true;
				float scroll = InputUtility.getScrollDiff();
				InputUtility.disableScroll();
				if (scroll != 0) {
					didScrollThisFrame = true;
					int maxScroll = (int)scrollBox.screenDimensionsInPixels.Y;
					int extraScroll = maxScroll - (int)screenDimensionsInPixels.Y;
					if (screenDimensionsInPixels.Y < maxScroll) {
						int scrollSpeedReal = (int)(scroll / 10);
						scrollOffset += scrollSpeedReal;

						if (-scrollOffset > extraScroll) scrollOffset = -extraScroll;
						if (-scrollOffset < 0) scrollOffset = 0;

						scrollBox.setPosition(0, scrollOffset);
						scrollBarOffset = -(int)(maxScrollBarTravelDist * ((float)scrollOffset / extraScroll));
					}
					else {
						scrollOffset = 0;
					}
				}
				else {
					didScrollThisFrame = false;
				}
			}
			else {
				isMouseOnElement = false;
				didScrollThisFrame = false;
			}
			base.update(gt);
		}

		public override UIElement addElement(UIElement uIElement) {
			int index = scrollBox.children.Count;
			positionItem(uIElement, index);
			scrollBox.addElement(uIElement);
			updateScrollMetrics();
			return this;
		}

		public override UIElement removeElement(UIElement uIElement) {
			if (scrollBox.children.Remove(uIElement)) {
				uIElement.parent = null;
				relayoutChildren();
			}
			return this;
		}

		private void positionItem(UIElement item, int index) {
			int col = index % itemsWide;
			int row = index / itemsWide;
			item.setPosition(new Point(col * itemWidth + itemPadding, row * itemHeight + itemPadding));
			item.setDimensions(itemWidth - itemPadding * 2, itemHeight - itemPadding * 2);
		}

		private void relayoutChildren() {
			for (int i = 0; i < scrollBox.children.Count; i++) {
				positionItem(scrollBox.children[i], i);
			}
			updateScrollMetrics();
		}

		private void updateScrollMetrics() {
			int rows = (int)Math.Ceiling((float)scrollBox.children.Count / itemsWide);
			scrollBox.setDimensions(itemWidth * itemsWide, itemHeight * rows, Unit.PX);
			scrollBarHeight = scrollBox.screenDimensionsInPixels.Y > 0
				? (int)(screenDimensionsInPixels.Y * (screenDimensionsInPixels.Y / scrollBox.screenDimensionsInPixels.Y))
				: (int)screenDimensionsInPixels.Y;
			maxScrollBarTravelDist = (int)screenDimensionsInPixels.Y - scrollBarHeight;
		}

		public override void draw(GameTime gt) { }

		public override void customDraw(GameTime gt) {
			if (!visible) return;
			Viewport _originalViewport = Globals.graphicsDevice.Viewport;
			Viewport scrollBoxViewport = new Viewport {
				X = (int)renderedPosition.X,
				Y = (int)renderedPosition.Y,
				Width = (int)screenDimensionsInPixels.X,
				Height = (int)screenDimensionsInPixels.Y,
				MinDepth = 0,
				MaxDepth = 1
			};
			Globals.graphicsDevice.Viewport = scrollBoxViewport;
			Globals.spriteBatch.Begin(
				sortMode: SpriteSortMode.Deferred,
				blendState: BlendState.NonPremultiplied,
				samplerState: null
			);
			base.draw(gt);
			Globals.spriteBatch.Draw(whiteRectangle, new(screenDimensionsInPixels.X - 5, scrollBarOffset), null, Color.Black, 0f, Vector2.Zero, new Vector2(5, scrollBarHeight), SpriteEffects.None, zIndex);
			Globals.spriteBatch.End();
			Globals.graphicsDevice.Viewport = _originalViewport;
			base.customDraw(gt);
		}
	}
}
