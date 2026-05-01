namespace MonoTools.Core.UIElements {
	public class SimpleButton : UIElement {
		public SimpleButton(string label = "") {
			interactive = true;
			textColor = new Color(30, 30, 30);
			centerText = true;
			color = new Color(224, 224, 224);
			hoverColor = new Color(196, 196, 196);
			pressedColor = new Color(160, 160, 160);
			setJustify(Justify.CENTER);
			setText(label);
		}
	}
}
