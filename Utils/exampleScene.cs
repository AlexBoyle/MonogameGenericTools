namespace Utils {
	public class ExampleScene : SceneBase {
		UIElement root = new();
		public ExampleScene() {
			this.name = "ExampleScene";
			this.isInitalScene = true;
		}

		public override void setup() {
			root.setPosition(0, 0);
			Point windowSize = WindowUtility.getwindowScreenSize();
			root.setDimentions(windowSize.X, windowSize.Y);

			UIElement padding = (new UIElement())
				.setOrientation(Orientation.HORIZONTAL)
				.setPosition(5, 5, Unit.PER)
				.setDimentions(90, 90, Unit.PER);

			UIElement left = (new Box())
				.setPosition(0, 0, Unit.PER)
				.setDimentions(33, 100, Unit.PER);
			UIElement middle = (new Box())
				.setPosition(0, 0, Unit.PER)
				.setDimentions(34, 100, Unit.PER);
			UIElement right = (new Box())
				.setPosition(0, 0, Unit.PER)
				.setDimentions(33, 100, Unit.PER)
				.setJustify(Justify.RIGHT);

			left.color = Color.Brown;
			middle.color = Color.RosyBrown;
			right.color = Color.SaddleBrown;

			UIElement loadGameButton = ((Button)((new Button("Load Game"))
				.setPosition(5, 5, Unit.PX)
				.setDimentions(256, 64, Unit.PX)))
				.setCallback(loadGame);

			UIElement newGameButton = ((Button)((new Button("New Game"))
				.setPosition(5, 5, Unit.PX)
				.setDimentions(256, 64, Unit.PX)))
				.setCallback(newGame);

			UIElement settingsButton = ((Button)((new Button("Settings"))
				.setPosition(5, 5, Unit.PX)
				.setDimentions(256, 64, Unit.PX)))
				.setCallback(settings);

			padding.addElement(left).addElement(middle).addElement(right);
			right.addElement(loadGameButton).addElement(newGameButton).addElement(settingsButton);
			root.addElement(padding);

			gameObjects.Add(root);

			base.setup();
		}
		public bool newGame(GameTime gt) {
			LoggingUtil.logWithGameTime("Button Click - NewGame", gt);

			//SceneUtility.setActive("GameScene");
			//SceneUtility.setInactive(this.name);

			return true;
		}
		public bool loadGame(GameTime gt) {
			LoggingUtil.logWithGameTime("Button Click - loadGame", gt);

			//SceneUtility.setActive("PerlinTestScene");
			//SceneUtility.setInactive(this.name);

			return true;
		}
		public bool settings(GameTime gt) {
			LoggingUtil.logWithGameTime("Button Click - settings", gt);
			return true;
		}

		public override void draw(GameTime gameTime) {
			Globals.spriteBatch.Begin(
				sortMode: SpriteSortMode.BackToFront,
				blendState: BlendState.NonPremultiplied,
				samplerState: SamplerState.PointClamp
			);
			foreach (GameObject gameObject in gameObjects) {
				gameObject.draw(gameTime);
			}
			Globals.spriteBatch.End();
		}

		public override void update(GameTime gameTime) {
			Point a = WindowUtility.getwindowScreenSize();
			root.setDimentions(a.X, a.Y);//ToDo: make better

			base.update(gameTime);
		}
	}
}
