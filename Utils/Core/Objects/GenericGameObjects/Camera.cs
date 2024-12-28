namespace Utils.Core.Objects.GenericGameObjects {
	using Utils.Core.Scene;

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
