namespace MonoTools.Core.Objects.GenericGameObjects {
	using MonoTools.Core.Scene;

	public class Camera : GameObject {
		public Camera() {
			drawTiming = DrawType.NEVER;
			updateTiming = Timing.NEVER;
		}
		public virtual Matrix getCameraMatrix() {
			return Matrix.Identity;
		}
	}

}
