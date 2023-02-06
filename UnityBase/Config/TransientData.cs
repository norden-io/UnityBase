#nullable enable
using System;
using System.IO;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace UnityBase.Config
{
	[Serializable]
	public abstract class TransientData
	{
		[SerializeField] internal string? _dataPath;

#if UNITY_EDITOR // avoid saving the variable to disk in builds, but have it still show up in the inspector
		[SerializeField]
		[Tooltip("Debugging attribute indicating whether this TransientData has been loaded from disk")]
#endif
		public bool loaded;

		public TransientData(string? path = null)
		{
			_dataPath = path;
		}

		private static string _databasePath => Application.persistentDataPath;

		public virtual string? dataPath
		{
			get => _dataPath;
			set => _dataPath = value;
		}

		public string? dataPathFull => dataPath == null ? null : database(dataPath);

		public static string database(string filename = "")
		{
			return Path.Combine(_databasePath, filename);
		}


		public virtual void Save()
		{
			if (string.IsNullOrEmpty(dataPath)) return;
			Debug.Log($"Saving {dataPath}");

			CreateDirectories();
			//JsonUtils.save(database(config_filename), this);
			File.WriteAllText(dataPathFull, JsonUtility.ToJson(this, true).Replace("    ", "\t"));
		}

		/*
		public TransientData? Load() {
			TransientData? d = null;
			Debug.Log($"Loading {dataPath}");
			try {
				d = JsonUtility.FromJson(File.ReadAllText(dataPathFull), GetType()) as TransientData;
			} catch (Exception e) {
				Debug.LogWarning($"Failed to load '{dataPathFull}': {e.Message}");
			}

			if (d == null) Debug.LogWarning($"Failed to cast '{dataPathFull}' to {GetType()}");
			return d;
		}*/
		public TransientData? Load()
		{
			return Load(dataPathFull, GetType());
		}


		public static TransientData Load(string? dataPath, Type type)
		{
			if (string.IsNullOrEmpty(dataPath))
				return (TransientData)Activator.CreateInstance(type);

			var dataPathFull = database(dataPath!);
			Debug.Log($"Loading {dataPath}");

			TransientData? d = null;
			try {
				var jsonText = File.ReadAllText(dataPathFull);
				d = JsonUtility.FromJson(jsonText, type) as TransientData;
				if (d == null) Debug.LogWarning($"Failed to cast '{dataPathFull}' to {type}");
			}
			catch (Exception e) {
				Debug.LogWarning($"Failed to load '{dataPathFull}': {e.Message}");
			}

			if (d is null) // loading failed: make new object
				d = (TransientData)Activator.CreateInstance(type);
			else
				d.loaded = true;

			d.Save();
			return d;
		}

		public static T Load<T>(string? dataPath) where T : TransientData, new()
		{
			return (T)Load(dataPath, typeof(T));
		}

		public JObject GetJObject()
		{
			return JObject.Parse(JsonUtility.ToJson(this));
		}
		/*
		public static T Load<T>(string? dataPath) where T : TransientData, new() {
			if (string.IsNullOrEmpty(dataPath)) return new T();
			var dataPathFull = database(dataPath!);
			Debug.Log($"Loading {dataPath}");

			T? d = null;
			try {
				d = JsonUtility.FromJson<T>(File.ReadAllText(dataPathFull));
			} catch { }

			if (d is null) { // loading failed: make new object
				d = new T();
			} else {
				d.loaded = true;
			}

			d.Save();
			return d;
		}*/

		public void CreateDirectories()
		{
			Directory.CreateDirectory(Path.GetDirectoryName(dataPathFull));
		}
	}
}
