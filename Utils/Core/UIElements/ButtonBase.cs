namespace MonoTools.Core.UIElements {
	using System;

	public class ButtonBase : UIElement {

		protected Func<GameTime, bool> callbackRefrence = null;
		protected bool isHovered = false;
		protected bool isPressed = false;
		protected bool isPressedLast = false;

		public override void update(GameTime gt) {
			Point mousePosition = InputUtility.getMousePosition();
			Rectangle r = new(screenPosition.ToPoint(), screenDimentionsInPixles.ToPoint());
			if (r.Contains(mousePosition)) {
				if (InputUtility.isMouse1Down()) {
					isBeingPressed();
				}
				else {
					isNotBeingPressed();
				}
				isBeingHovered();
			}
			else {
				isNotBeingHovered();
				resetPressed();
			}


			if (!isPressed && isPressedLast && callbackRefrence != null) {
				InputUtility.disableM1();
				// To Fix?
				callbackRefrence(gt);
			}
			base.update(gt);
		}


		public ButtonBase setCallback(Func<GameTime, bool> funcRef) {
			callbackRefrence = funcRef;
			return this;
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

	}
}
