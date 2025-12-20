namespace MonoTools.Core.GlobalUtilities {


	public static class ObjectUtilities {

		public static List<Type> getAllTypesOfBaseClass<T>() {
			var baseType = typeof(T);
			List<Type> output = AppDomain.CurrentDomain.GetAssemblies()
				.SelectMany(a => a.GetTypes())
				.Where(
					t => baseType.IsAssignableFrom(t)
						&& baseType != t
						&& !t.IsAbstract

				).ToList();

			return output;
		}
	}
}
