using System;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour {
	protected static T _instance;
	private static GameObject go;

	public static T instance {
		get {
			if (_instance == null) {
				_instance = (T)FindObjectOfType(typeof(T));

				if (_instance == null) {
					go = Instantiate(new GameObject(String.Format("singleton - {1}", typeof(T).GetType().ToString())));
					_instance = go.AddComponent<T>();
					DontDestroyOnLoad(_instance);
				}
			}
			return _instance;
		}
	}

	protected virtual void Awake() {
		if (_instance != null && _instance != this) {
			DestroyImmediate(this);
			throw new Exception("Singeton is singleton");
		}
	}
}
