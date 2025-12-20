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
			setOrientation(Orientation.FREE_GLOBAL);
			scrollBox.setOrientation(Orientation.FREE_INSIDE);
			scrollBox.willRenderInViewport = true;
			scrollBox.setParent(this);
			children.Add(scrollBox);

		}

		protected override void updateRealDimentions() {
			base.updateRealDimentions();
			itemWidth = (int)(screenDimentionsInPixles.X / itemsWide);
			itemHeight = (int)(screenDimentionsInPixles.X / itemsWide);
		}
		public override void update(GameTime gt) {
			if (getBoundsOnScreen().Contains(InputUtility.getMousePosition())) {
				isMouseOnElement = true;
				float scroll = InputUtility.getScrollDiff();
				InputUtility.disableScroll();
				if (scroll != 0) {
					didScrollThisFrame = true;
					int maxScroll = (int)scrollBox.screenDimentionsInPixles.Y;
					int extraScroll = maxScroll - (int)screenDimentionsInPixles.Y;
					if (screenDimentionsInPixles.Y < maxScroll) {
						int scrollSpeedReal = (int)(scroll / 10);
						// Calculate new scroll offset
						scrollOffset += scrollSpeedReal;

						// Bound scroll offset
						if (-scrollOffset > extraScroll) {
							scrollOffset = -extraScroll;
						}
						if (-scrollOffset < 0) {
							scrollOffset = 0;
						}

						// update scrollBox position
						scrollBox.setPosition(0, scrollOffset);

						// update scrollBar position
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
			int indexInList = scrollBox.children.Count;
			uIElement.setPosition(new Point(((indexInList % itemsWide) * itemWidth) + (indexInList % 2 == 0 ? itemPadding : itemPadding * 2), (((indexInList / itemsWide) * itemWidth))));
			uIElement.setDimentions(itemWidth - (itemPadding + itemPadding), itemHeight - (itemPadding + itemPadding));
			scrollBox.addElement(uIElement);
			scrollBox.setDimentions(itemWidth * itemsWide, itemHeight * ((int)Math.Ceiling((float)scrollBox.children.Count / itemsWide)), Unit.PX);
			scrollBarHeight = (int)(screenDimentionsInPixles.Y * (screenDimentionsInPixles.Y / scrollBox.screenDimentionsInPixles.Y));
			maxScrollBarTravelDist = (int)screenDimentionsInPixles.Y - scrollBarHeight;
			return this;
		}


		public override void draw(GameTime gt) {
		}
		public override void customDraw(GameTime gt) {
			Viewport _originalViewport = Globals.graphicsDevice.Viewport;
			Viewport scrollBoxViewport = new Viewport {
				X = (int)renderedPosition.X,
				Y = (int)renderedPosition.Y,
				Width = (int)screenDimentionsInPixles.X,
				Height = (int)screenDimentionsInPixles.Y,
				MinDepth = 0,
				MaxDepth = 1
			};
			Globals.graphicsDevice.Viewport = scrollBoxViewport;
			Globals.spriteBatch.Begin(
				sortMode: SpriteSortMode.Deferred,
				blendState: BlendState.NonPremultiplied,
				samplerState: null
			);
			Globals.spriteBatch.Draw(whiteRectangle, new(0, 0), null, color, 0f, Vector2.Zero, screenDimentionsInPixles, SpriteEffects.None, zIndex);
			base.draw(gt);
			Globals.spriteBatch.Draw(whiteRectangle, new(screenDimentionsInPixles.X - 5, scrollBarOffset), null, Color.Black, 0f, Vector2.Zero, new Vector2(5, scrollBarHeight), SpriteEffects.None, zIndex);
			Globals.spriteBatch.End();
			Globals.graphicsDevice.Viewport = _originalViewport;
			base.customDraw(gt);
		}
	}
}
