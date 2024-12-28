namespace Utils.Core.Scene {
	using Microsoft.Xna.Framework;
	using Utils.Core.Utilities;
	using System;
	using System.Collections.Generic;

	public static class SceneUtility {


		private static List<string> scenesToAdd = new List<string>();
		private static List<string> scenesToRemove = new List<string>();
		public static Dictionary<string, SceneBase> scenes { get; private set; } = new Dictionary<string, SceneBase>();
		public static Dictionary<string, SceneBase> liveScenes { get; private set; } = new Dictionary<string, SceneBase>();
		public static SceneBase sceneBeingRendered { get; set; }

		public static void intialize() {
			List<Type> types = ObjectUtilities.getAllTypesOfBaseClass<SceneBase>();
			foreach (Type t in types) {

				SceneBase scene = (SceneBase)Activator.CreateInstance(t);
				addScene(scene);
				if (scene.isInitalScene) {
					setActive(scene.name);
				}
				Debug.WriteLine("Found and added Scene=\"" + scene.name + "\"");
			}

		}

		public static void addScene(SceneBase scene) {
			if (scene != null) {
				if (!scenes.ContainsKey(scene.name)) {
					scenes.Add(scene.name, scene);
				}
				else {
					Debug.WriteLine("Failed to add scene name=" + scene.name);
				}
			}
		}
		public static void setInactive(string name) {
			scenesToRemove.Add(name);
		}

		public static void setActive(string name) {
			scenesToAdd.Add(name);
		}

		private static bool addSceneLogic(string name, bool shouldMakeRendered = true) {
			if (!liveScenes.ContainsKey(name) && scenes.ContainsKey(name)) {
				SceneBase scene = scenes[name];
				liveScenes.Add(name, scene);
				if (!scene.isSetup) {
					scene.setup();
				}
				if (shouldMakeRendered) {
					sceneBeingRendered = scene;
				}
				return true;
			}
			return false;
		}
		private static bool removeSceneLogic(string name) {
			SceneBase scene = null;
			try { liveScenes.TryGetValue(name, out scene); } catch { }
			if (scene != null) {
				scene.reset();
				if (sceneBeingRendered == scene) { sceneBeingRendered = null; }
				liveScenes.Remove(name);
				return true;
			}
			return false;
		}

		public static void update(GameTime gt) {
			if (scenesToRemove.Count > 0) {
				while (scenesToRemove.Count > 0) {
					string sceneName = scenesToRemove[0];
					scenesToRemove.Remove(sceneName);
					removeSceneLogic(sceneName);
				}
			}
			if (scenesToAdd.Count > 0) {
				while (scenesToAdd.Count > 0) {
					string sceneName = scenesToAdd[0];
					scenesToAdd.Remove(sceneName);
					addSceneLogic(sceneName);
				}
			}
			if (sceneBeingRendered == null) {
				foreach (var scene in scenes) {
					if (scene.Value.isInitalScene) {
						sceneBeingRendered = scene.Value;
						break;
					}
				}
			}
		}

		public static void draw(GameTime gt) {
			if (sceneBeingRendered != null) {
				if (sceneBeingRendered.rednerStatus == Status.ACTIVE)
					sceneBeingRendered.draw(gt);
			}
		}
	}
}
