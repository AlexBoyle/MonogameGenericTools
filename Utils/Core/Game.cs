using System.Collections.Generic;

namespace Utils.Core {
	public class Game : Microsoft.Xna.Framework.Game {
		private static int sampleSeconds = 10;
		private int sampelPeriod = 60 * sampleSeconds;
		private int frameCount = 0;
		private float fps = 0;
		private Queue<long> drawTimeTaken = new();
		private int drawsSinceLastLog = 0;
		private Queue<long> updateTimeTaken = new();
		private int updatesSinceLastLog = 0;
		private GraphicsDeviceManager graphicsDeviceManager;
		public Game() {
			IsMouseVisible = true;
			graphicsDeviceManager = new GraphicsDeviceManager(this);
			graphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
		}

		protected override void Initialize() {
			var watch = new Stopwatch();
			watch.Start();
			Debug.WriteLine("Initializing");
			WindowUtility.initialize(this, graphicsDeviceManager);
			Globals.initialize(this);
			ContentUtility.initialize(this);
			WindowUtility.switchToFullScreen(true, false);
			SceneUtility.intialize();
			SettingsUtility.intialize();
			base.Initialize();
			LoggingUtil.logWithTimeTaken("Finished Initializing", watch);
			Debug.WriteLine($"00:00:00 - Starting");
		}

		protected override void LoadContent() {
			Debug.WriteLine("Loading Content");
			return;
		}


		protected override void Update(GameTime gameTime) {
			var watch = new Stopwatch();
			watch.Start();
			////////////////////////////////
			InputUtility.update(gameTime);
			SceneUtility.update(gameTime);

			if (InputUtility.isKeyDown(Keys.Escape)) { Exit(); }
			UIElementInteractionHookUtility.update(gameTime);
			foreach (KeyValuePair<string, SceneBase> entry in SceneUtility.liveScenes) {
				if (entry.Value.updateStatus == Status.ACTIVE)
					entry.Value.update(gameTime);
			}
			base.Update(gameTime);

			if (updateTimeTaken.Count > sampelPeriod) { updateTimeTaken.Dequeue(); }
			if (updatesSinceLastLog >= sampelPeriod) {
				updatesSinceLastLog = 0;
				long totaltimeUpdate = 0;
				long totaltimeDraw = 0;
				foreach (long time in updateTimeTaken) {
					totaltimeUpdate += time;
				}
				foreach (long time in drawTimeTaken) {
					totaltimeDraw += time;
				}
				fps = frameCount / (float)sampleSeconds;
				frameCount = 0;
				// 60 update per second is 16.6ms
				LoggingUtil.logWithGameTime(
					$"avrUpdate={((totaltimeUpdate / updateTimeTaken.Count) / 10000f).ToString("00.000")}ms " +
					$"avrDraw={((totaltimeDraw / updateTimeTaken.Count) / 10000f).ToString("00.000")}ms " +
					$"Fps={fps.ToString("00.0")}"
					, gameTime
				);
			}
			updatesSinceLastLog++;

			watch.Stop();
			updateTimeTaken.Enqueue(watch.ElapsedTicks);
		}

		protected override void Draw(GameTime gameTime) {
			var watch = new Stopwatch();
			watch.Start();
			GraphicsDevice.Clear(Color.Black);
			SceneUtility.draw(gameTime);
			base.Draw(gameTime);
			watch.Stop();
			drawTimeTaken.Enqueue(watch.ElapsedTicks);
			if (drawTimeTaken.Count > sampelPeriod) { drawTimeTaken.Dequeue(); }
			frameCount++;
		}
	}
}
