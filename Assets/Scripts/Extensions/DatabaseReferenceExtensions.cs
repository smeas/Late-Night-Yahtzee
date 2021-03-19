using System.Threading.Tasks;
using Firebase.Database;
using UnityEngine;

namespace Extensions {
	public static class DatabaseReferenceExtensions {
		public static Task SetValueFromObjectAsJson(this DatabaseReference reference, object data) {
			string json = JsonUtility.ToJson(data);
			return reference.SetRawJsonValueAsync(json);
		}
	}
}