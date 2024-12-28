namespace Utils.Core.Objects.AssetPooling {
	using System.Collections.Generic;

	public class Poolable {
		private static Dictionary<System.Type, int> nextIdDict = new();
		private int id = 0;
		public bool isActive = false;
		//private Pool<> poolRef;
		public Poolable() {
			System.Type type = this.GetType();
			if (!nextIdDict.ContainsKey(type)) {
				nextIdDict.Add(type, 0);
			}
			id = nextIdDict[type]++;
		}

		public int getId() {
			return id;
		}

		public virtual void reset() {
			isActive = false;
		}

	}
}
