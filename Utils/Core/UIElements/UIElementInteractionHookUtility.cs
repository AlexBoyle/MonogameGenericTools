namespace Utils.Core.UIElements {
	using System.Collections.Generic;
	using Utils.Core.GlobalUtilities;

	public static class UIElementInteractionHookUtility {
		public static HashSet<UIElement> uIElements { get; private set; } = new HashSet<UIElement>();
		public static void addElement(UIElement el) {
			if (!uIElements.Contains(el)) {
				uIElements.Add(el);
			}
		}
		public static void removeElement(UIElement el) {
			if (uIElements.Contains(el)) {
				uIElements.Remove(el);
			}
		}

		public static void update(GameTime gt) {
			Point mousePosition = InputUtility.getMousePosition();
			foreach (UIElement el in uIElements) {

				if (el.getBounds().Contains(mousePosition)) {
					if (InputUtility.isMouse1Down()) {
						el.isBeingPressed();
					}
					else {
						el.isNotBeingPressed();
					}
					el.isBeingHovered();
				}
				else {
					el.isNotBeingHovered();
					el.resetPressed();
				}
			}
		}
	}
}
