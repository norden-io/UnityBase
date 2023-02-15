using System;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

namespace UnityBase.Types
{
	// Based on https://gist.github.com/timofei7/fed468b0d23c58875bb9

	/// <summary>
	///     Prefab attribute. Use this on child classes
	///     to define if they have a prefab associated or not
	///     By default will attempt to load a prefab
	///     that has the same name as the class,
	///     otherwise [Prefab("path/to/prefab")]
	///     to define it specifically.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PrefabAttribute : Attribute
	{
		public PrefabAttribute() => Name = "";
		public PrefabAttribute(string name) => Name = name;

		public string Name { get; }
	}

	/// <summary>
	///     MONOBEHAVIOR PSEUDO SINGLETON ABSTRACT CLASS
	///     usage		: can be attached to a gameobject and if not
	///     : this will create one on first access
	///     example		: '''public sealed class MyClass : Singleton
	///     <MyClass>
	///         {'''
	///         references	: http://tinyurl.com/d498g8c
	///         : http://tinyurl.com/cc73a9h
	///         : http://unifycommunity.com/wiki/index.php?title=Singleton
	/// </summary>
	public abstract class Singleton<T> : DerivedSingleton<T, MonoBehaviour> where T : MonoBehaviour
	{
	}

	[DefaultExecutionOrder(-10000)]
	public abstract class DerivedSingleton<T, TBase> : MonoBehaviour
		where T : TBase
		where TBase : MonoBehaviour
	{
		private static T    _instance;
		public static  bool IsAwake => _instance != null;

		/// <summary>
		///     gets the instance of this Singleton
		///     use this for all instance calls:
		///     MyClass._.MyMethod();
		///     or make your public methods static
		///     and have them use Instance internally
		///     for a nice clean interface
		/// </summary>
		public static T _
		{
			get
			{
				if (_instance == null) {
					_instance = (T)FindObjectOfType(typeof(T));
					if (_instance == null) {
						var go = GetGameObject();

						_instance = go.GetComponent<T>();
						if (_instance == null) _instance = go.AddComponent<T>();
					}
					else {
						//Debug.Log(mytype.Name + " had to be searched for but was found"); 
						var count = FindObjectsOfType(typeof(T)).Length;
						if (count > 1) {
							Debug.LogError("Singleton: there are " + count + " of "                          + typeof(T).Name);
							throw new Exception("Too many ("       + count + ") prefab singletons of type: " + typeof(T).Name);
						}
					}
				}

				return _instance;
			}
		}

		public virtual void Awake()
		{
			ValidateSingletonInstance();
		}

		public virtual void Reset()
		{
			_instance = this as T;
		}

		/// <summary>
		///     for garbage collection
		/// </summary>
		public virtual void OnApplicationQuit()
		{
			_instance = null;
		}

		public virtual void OnValidate()
		{
			ValidateSingletonInstance();
		}

		private static GameObject GetGameObject()
		{
			//Debug.Log("initializing instance of: " + mytype.Name);
			var goName = typeof(T).ToString();
			var go     = GameObject.Find(goName);
			if (go == null) // try again searching for a cloned object
			{
				go = GameObject.Find(goName + "(Clone)");
				if (go != null) {
					//Debug.Log("found clone of object using it!"); 
				}
			}

			if (go == null) //if still not found try prefab or create
			{
				var hasPrefab = Attribute.IsDefined(typeof(T), typeof(PrefabAttribute));
				// checks if the [Prefab] attribute is set and pulls that if it can
				if (hasPrefab) {
					var attr       = (PrefabAttribute)Attribute.GetCustomAttribute(typeof(T), typeof(PrefabAttribute));
					var prefabname = attr.Name;
					//Debug.LogWarning(goName + " not found attempting to instantiate prefab... either: " + goName + " or: " + prefabname);
					try {
						if (prefabname != "")
							go = (GameObject)Instantiate(Resources.Load(prefabname, typeof(GameObject)));
						else
							go = (GameObject)Instantiate(Resources.Load(goName, typeof(GameObject)));
					}
					catch (Exception e) {
						Debug.LogError("could not instantiate prefab even though prefab attribute was set: " +
						               e.Message + "\n" + e.StackTrace);
					}
				}

				if (go == null) {
					//Debug.LogWarning(goName + " not found creating...");
					go      = new GameObject();
					go.name = goName;
				}
			}

			return go;
		}

		protected void ValidateSingletonInstance()
		{
#if UNITY_EDITOR
			// ignore prefabs and prefab instances
			var stage = PrefabStageUtility.GetCurrentPrefabStage();
			if (stage == null || stage.scene != gameObject.scene) return;
			if (EditorUtility.IsPersistent(this)) return;
#endif
			if (_instance != null && _instance != this)
				//Debug.LogError($"Not allowed to create another instance of {GetType()}, one already exists!", _instance);
				throw new Exception($"Not allowed to create another instance of {GetType()}, one already exists!");
			_instance = this as T;
		}

		// in your child class you can implement Awake()
		// 	and add any initialization code you want such as
		// 	DontDestroyOnLoad(this.gameObject);
		// 	if you want this to persist across loads
		//  or if you want to set a parent object with SetParent()

		/// <summary>
		///     parent this to another gameobject by string
		///     call from Awake if you so desire
		/// </summary>
		protected void SetParent(string parentGOName)
		{
			if (parentGOName != null) {
				var parentGO = GameObject.Find(parentGOName);
				if (parentGO == null) {
					parentGO                  = new GameObject();
					parentGO.name             = parentGOName;
					parentGO.transform.parent = null;
				}

				transform.parent = parentGO.transform;
			}
		}
	}
}
