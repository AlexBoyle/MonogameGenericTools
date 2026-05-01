namespace Utils {
	public class ExampleScene : SceneBase {
		private UILoadResult _ui;
		private Point _currentScreenSize = new();

		private string _username = "";
		private string _password = "";
		private string _submittedUser = "";
		private string _submittedPass = "";
		private bool _hasSubmitted = false;

		public ExampleScene() {
			this.name = "UITestScene";
			this.isInitalScene = true;
		}

		public override void setup() {
			_currentScreenSize = WindowUtility.getwindowScreenSize();
			buildUI();
			base.setup();
		}

		public override void reset() {
			_ui = null;
			_username = "";
			_password = "";
			_submittedUser = "";
			_submittedPass = "";
			_hasSubmitted = false;
			gameObjects.Clear();
			base.reset();
		}

		private void buildUI() {
			var ctx = new UIContext();

			ctx.interactions.bind("onBack", _ => {
				//SceneUtility.setActive("MainMenuScene");
				//SceneUtility.setInactive(this.name);
				Globals.gameRef.Exit();
			});

			ctx.interactions.bindInput("onUsernameChange", v => _username = v);
			ctx.interactions.bindInput("onPasswordChange", v => _password = v);

			ctx.interactions.bind("onLogin", _ => {
				_submittedUser = _username;
				_submittedPass = _password;
				_hasSubmitted = true;
			});

			ctx.bindText("submittedUser", () => _submittedUser);
			ctx.bindText("submittedPass", () => _submittedPass);
			ctx.bindVisible("hasSubmitted", () => _hasSubmitted);

			_ui = UIHtmlLoader.load("testTemplate.html", ctx);
			_ui.root.setDimensions(_currentScreenSize.X, _currentScreenSize.Y);
			gameObjects.Add(_ui.root);
		}

		public override void update(GameTime gameTime) {
			Point screenSize = WindowUtility.getwindowScreenSize();
			if (!_currentScreenSize.Equals(screenSize)) {
				_currentScreenSize = screenSize;
				_ui.root.setDimensions(screenSize.X, screenSize.Y);
			}
			_ui?.update(gameTime);
			_ui?.root.update(gameTime);
		}

		public override void draw(GameTime gameTime) {
			Globals.spriteBatch.Begin(
				sortMode: SpriteSortMode.BackToFront,
				blendState: BlendState.NonPremultiplied,
				samplerState: SamplerState.LinearClamp
			);
			_ui?.root.draw(gameTime);
			Globals.spriteBatch.End();
			_ui?.root.customDraw(gameTime);
		}
	}
}
