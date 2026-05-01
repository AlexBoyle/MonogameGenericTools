namespace MonoTools.Core.ECS {

	public struct Entity {
		public static readonly Entity Invalid = new(-1);

		public readonly int id = -1;
		public bool IsValid => id >= 0;

		public Entity(int id) {
			this.id = id;
		}

		public override string ToString() => $"Entity({id})";
	}
}
