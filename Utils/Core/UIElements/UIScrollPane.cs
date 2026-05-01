namespace MonoTools.Core.UIElements
{
	using MonoTools.Core.GlobalUtilities;
	using System;

	public class UIScrollPane : UIElement
	{
		private const int ScrollBarW = 6;

		private readonly UIElement _inner;
		private float _scrollOffset = 0f;
		private int _lastAppliedOffset = 0;
		private bool _scrollbarVisible = false;

		public UIScrollPane()
		{
			clipToBounds = true;
			setOrientation(Orientation.RELATIVE);

			_inner = new UIElement();
			_inner.setOrientation(Orientation.COLUMN);
			_inner.setWidth(100, Unit.PER);
			base.addElement(_inner);
		}

		public override UIElement addElement(UIElement child)
		{
			_inner.addElement(child);
			return this;
		}

		public override UIElement removeElement(UIElement child)
		{
			_inner.removeElement(child);
			return this;
		}

		public override void update(GameTime gt)
		{
			if (!visible) return;

			// Always recompute _inner height so items added while hidden (e.g. popup was
			// invisible when ListPoller ticked) get their positions calculated via setHeight
			// → updatePositions. Skipping the equality check is intentional: if height
			// didn't change, setHeight is cheap but still triggers updatePositions, which
			// is required when items were inserted without a parent layout pass.
			float actualContentH = _inner.computeContentHeight();
			if (actualContentH > 0)
				_inner.setHeight((int)Math.Ceiling(actualContentH));

			// Narrow _inner by the scrollbar width when content overflows, so children
			// don't lay out under the scrollbar track. Restore full width when it fits.
			bool needsScrollbar = _inner.screenDimensionsInPixels.Y > screenDimensionsInPixels.Y;
			if (needsScrollbar != _scrollbarVisible)
			{
				_scrollbarVisible = needsScrollbar;
				if (needsScrollbar)
					_inner.setWidth((int)screenDimensionsInPixels.X - ScrollBarW);
				else
					_inner.setWidth(100, Unit.PER);
				// Re-measure after width change (word-wrap content may reflow).
				actualContentH = _inner.computeContentHeight();
				if (actualContentH > 0)
					_inner.setHeight((int)Math.Ceiling(actualContentH));
			}

			Point mousePos = InputUtility.getMousePosition();
			var bounds = new Rectangle(screenPosition.ToPoint(), screenDimensionsInPixels.ToPoint());
			if (bounds.Contains(mousePos))
			{
				float delta = InputUtility.getScrollDiff();
				InputUtility.disableScroll();
				if (delta != 0)
				{
					_scrollOffset -= delta * 0.25f;
					float maxScroll = Math.Max(0, _inner.screenDimensionsInPixels.Y - screenDimensionsInPixels.Y);
					_scrollOffset = Math.Clamp(_scrollOffset, 0, maxScroll);
				}
			}

			int targetOffset = (int)_scrollOffset;
			if (targetOffset != _lastAppliedOffset)
			{
				_lastAppliedOffset = targetOffset;
				_inner.setPosition(0, Unit.PX, -targetOffset, Unit.PX);
			}

			base.update(gt);
		}

		public override void customDraw(GameTime gt)
		{
			if (!visible) return;

			// Pass 1: clipped content
			Rectangle prev = Globals.graphicsDevice.ScissorRectangle;
			Globals.graphicsDevice.ScissorRectangle = new Rectangle(
				renderedPosition.ToPoint(), screenDimensionsInPixels.ToPoint());
			Globals.spriteBatch.Begin(
				sortMode: SpriteSortMode.BackToFront,
				blendState: BlendState.NonPremultiplied,
				samplerState: SamplerState.LinearClamp,
				rasterizerState: scissorState);
			foreach (UIElement child in children)
				child.draw(gt);
			Globals.spriteBatch.End();
			Globals.graphicsDevice.ScissorRectangle = prev;

			// Pass 2: scrollbar — separate batch so it always renders on top of content
			float maxScroll = Math.Max(0, _inner.screenDimensionsInPixels.Y - screenDimensionsInPixels.Y);
			if (maxScroll > 0)
			{
				Globals.spriteBatch.Begin(
					sortMode: SpriteSortMode.BackToFront,
					blendState: BlendState.NonPremultiplied,
					samplerState: SamplerState.LinearClamp);
				drawScrollBar(maxScroll);
				Globals.spriteBatch.End();
			}

			foreach (UIElement child in children)
				child.customDraw(gt);
		}

		private void drawScrollBar(float maxScroll)
		{
			const int thumbPad  = 1;
			const int minThumbH = 24;

			float viewH    = screenDimensionsInPixels.Y;
			float contentH = _inner.screenDimensionsInPixels.Y;
			int   trackX   = (int)(renderedPosition.X + screenDimensionsInPixels.X) - ScrollBarW;
			int   trackY   = (int)renderedPosition.Y;
			int   trackH   = (int)viewH;

			// Track
			Globals.spriteBatch.Draw(whiteRectangle,
				new Rectangle(trackX, trackY, ScrollBarW, trackH),
				null, new Color(25, 25, 38), 0f, Vector2.Zero, SpriteEffects.None, 0.01f);

			// Thumb — sized proportionally to visible fraction, clamped to a minimum
			int   thumbH = (int)Math.Max(minThumbH, viewH / contentH * viewH);
			int   thumbY = trackY + (int)(_scrollOffset / maxScroll * (viewH - thumbH));
			Globals.spriteBatch.Draw(whiteRectangle,
				new Rectangle(trackX + thumbPad, thumbY + thumbPad, ScrollBarW - 2 * thumbPad, thumbH - 2 * thumbPad),
				null, new Color(100, 100, 145), 0f, Vector2.Zero, SpriteEffects.None, 0.009f);
		}
	}
}
