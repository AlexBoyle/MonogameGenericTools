namespace Utils.Core.Objects {
	using Utils.Core.Scene;

	public class GameObject : Poolable {
		public DrawType drawTiming = DrawType.WITH_CAMERA;
		public Timing updateTiming = Timing.DURRING;
		public Vector2 position = Vector2.Zero;
		public GameObject() { }

		public virtual void update(GameTime gameTime) { }

		public virtual void draw(GameTime gameTime) { }

	}
}
