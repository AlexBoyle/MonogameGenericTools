namespace Utils.Core.Scene {
	public class SceneBase {
		public string name = "BaseScene";
		public Status rednerStatus = Status.ACTIVE;
		public Status updateStatus = Status.ACTIVE;
		protected ArrayList gameObjects = new();
		public bool isInitalScene { get; protected set; } = false;
		public bool isSetup { get; private set; }
		public SceneBase() {
			isSetup = false;
		}

		protected virtual void postConstructSetup() {
			SceneUtility.addScene(this);
		}

		public virtual void setup() {
			isSetup = true;
		}
		public virtual void reset() {
			isSetup = false;
		}

		public virtual void update(GameTime gameTime) {
			foreach (GameObject gameObject in gameObjects) {
				if (gameObject.updateTiming == Timing.BEFORE) {
					gameObject.update(gameTime);
				}
			}
			foreach (GameObject gameObject in gameObjects) {
				if (gameObject.updateTiming == Timing.DURRING) {
					gameObject.update(gameTime);
				}
			}
			foreach (GameObject gameObject in gameObjects) {
				if (gameObject.updateTiming == Timing.AFTER) {
					gameObject.update(gameTime);
				}
			}
		}

		public virtual void draw(GameTime gameTime) { }

		public virtual void onDestroy() { }

	}
}
