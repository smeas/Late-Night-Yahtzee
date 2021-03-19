using Firebase.Database;
using JetBrains.Annotations;
using UnityEngine;

namespace Extensions {
	public static class DataSnapshotExtensions {
		[CanBeNull]
		public static T ToObject<T>(this DataSnapshot snap) {
			string json = snap.GetRawJsonValue();
			if (string.IsNullOrEmpty(json))
				return default;

			return JsonUtility.FromJson<T>(json);
		}

		[CanBeNull]
		public static T ToObjectWithId<T>(this DataSnapshot snap) where T : DataWithId {
			T data = snap.ToObject<T>();
			if (data != null)
				data.id = snap.Key;

			return data;
		}
	}
}