using UnityEngine;

public abstract class SingletonBehaviour<T> : MonoBehaviour where T : SingletonBehaviour<T> {
	public static T Instance { get; protected set; }

	protected virtual void Awake() {
		if (Instance == null)
			Instance = (T)this;
		else
			Destroy(gameObject);
	}

	protected virtual void OnDestroy() {
		if (Instance == this)
			Instance = null;
	}
}