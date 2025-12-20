namespace MonoTools.Core {
	public class Game : Microsoft.Xna.Framework.Game {
		private static int sampleSeconds = 2;
		private static int targetfps = 144;
		private int sampelPeriod = targetfps * sampleSeconds;
		private int frameCount = 0;
		public static float fps { get; private set; } = 0;
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

			TargetElapsedTime = TimeSpan.FromSeconds(1.0 / (float)targetfps);

			IsFixedTimeStep = true;



			Debug.WriteLine("Initializing");
			WindowUtility.initialize(this, graphicsDeviceManager);
			Globals.initialize(this);
			ContentUtility.initialize(this);
			WindowUtility.switchToFullScreen(true, false);
			SceneUtility.intialize();
			SettingsUtility.intialize();
			base.Initialize();
			LoggingUtil.info("Finished Initializing");
			LoggingUtil.info("Starting");
		}

		protected override void LoadContent() {
			LoggingUtil.info("Loading Content");
			return;
		}


		protected override void Update(GameTime gameTime) {
			var watch = new Stopwatch();
			watch.Start();
			////////////////////////////////
			InputUtility.update(gameTime);
			SceneUtility.update(gameTime);

			foreach (KeyValuePair<string, SceneBase> entry in SceneUtility.liveScenes) {
				if (entry.Value.updateStatus == Status.ACTIVE)
					entry.Value.update(gameTime);
			}
			base.Update(gameTime);

			if (updateTimeTaken.Count > sampelPeriod) { updateTimeTaken.Dequeue(); }
			if (updatesSinceLastLog >= sampelPeriod) {
				updatesSinceLastLog = 0;
				fps = frameCount / (float)sampleSeconds;
				frameCount = 0;
				// 60 update per second is ~16.6ms
				/*
				LoggingUtil.info(
					$"avrUpdate={(updateTimeTaken.Average() / 10000f).ToString("00.000")}ms " +
					$"avrDraw={(drawTimeTaken.Average() / 10000f).ToString("00.000")}ms " +
					$"Fps={fps.ToString("00.0")}"
				);
				*/
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
			if (watch.ElapsedTicks > 160000) {
				//LoggingUtil.info($"WARN: draw took {(watch.ElapsedTicks / 10000f).ToString("00.000")}ms");
			}
			if (drawTimeTaken.Count > sampelPeriod) { drawTimeTaken.Dequeue(); }
			frameCount++;
		}
	}
}
