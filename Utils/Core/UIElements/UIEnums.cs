namespace MonoTools.Core.UIElements
{
    public enum Unit { PX, PER }

    public enum Orientation { COLUMN, ROW, RELATIVE, ABSOLUTE }

    public enum Justify { NONE, START, END, CENTER }

    public struct Spacing {
        public int left, top, right, bottom;

        public static readonly Spacing Zero = new Spacing(0);

        public Spacing(int all) { left = top = right = bottom = all; }
        public Spacing(int horizontal, int vertical) { left = right = horizontal; top = bottom = vertical; }
        public Spacing(int left, int top, int right, int bottom) {
            this.left = left; this.top = top; this.right = right; this.bottom = bottom;
        }
    }
}
