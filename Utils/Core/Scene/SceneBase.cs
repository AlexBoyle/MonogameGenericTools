namespace MonoTools.Core.Scene {
	public class SceneBase {
		private static int nextSceneId = 0;
		public int sceneId { get; private set; } = -1;
		public string name = "BaseScene";
		public Status rednerStatus = Status.ACTIVE;
		public Status updateStatus = Status.ACTIVE;
		protected ArrayList gameObjects = new();
		public bool isInitalScene { get; protected set; } = false;
		public bool isSetup { get; private set; }
		public SceneBase() {
			sceneId = nextSceneId;
			nextSceneId++;
			isSetup = false;
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

		public virtual void draw(GameTime gameTime) {

		}

		public virtual void onDestroy() { }

	}
}
