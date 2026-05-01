namespace MonoTools.Core.ECS {
	using Microsoft.Xna.Framework;

	/// <summary>
	/// A system processes entities with specific components each frame.
	/// Register systems with GlobalECS.RegisterSystem().
	/// </summary>
	public interface ISystem {
		void Update(GameTime gameTime);
		void Draw(GameTime gameTime) { }
	}
}
