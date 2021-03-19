namespace Utilities {
	public static class Util {
		public static T[] CreateInitializedArray<T>(int size) where T : new() {
			T[] array = new T[size];

			for (int i = 0; i < array.Length; i++)
				array[i] = new T();

			return array;
		}
	}
}