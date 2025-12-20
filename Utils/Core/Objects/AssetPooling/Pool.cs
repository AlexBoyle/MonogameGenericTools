namespace MonoTools.Core.Objects.AssetPooling {
	public class Pool<T> where T : Poolable, new() {
		private Stack<T> inactive = new Stack<T>();
		private Dictionary<int, T> active = new Dictionary<int, T>();
		public Pool() { }
		public Pool(int initalNumberOfObjects) {
			addToPool(initalNumberOfObjects);
		}

		public void addToPool(int numObjects) {
			for (int i = 0; i < numObjects; i++) {
				inactive.Push(new T());
			}
		}
		public virtual T getForUse() {
			T output;
			if (inactive.Count == 0) {
				output = new T();
				active.Add(output.getId(), output);

			}
			else {
				output = inactive.Pop();
				active.Add(output.getId(), output);

			}
			output.isActive = true;
			return output;
		}
		public virtual void removeFromUse(T t) {
			if (active.ContainsKey(t.getId())) {
				t.isActive = false;
				active.Remove(t.getId());
				t.reset();
				inactive.Push(t);
			}
		}
	}
}
