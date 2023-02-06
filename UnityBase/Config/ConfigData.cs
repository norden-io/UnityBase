using System;

namespace UnityBase.Config
{
	[Serializable]
	public abstract class ConfigData : Data
	{
		public delegate void OnChangeEvent(ConfigData config);

		public ConfigData()
		{
			_dataPath = dataPath;
		}

		public abstract override string dataPath { get; }
		public event OnChangeEvent      OnChange;

		public virtual void Changed()
		{
			OnChange?.Invoke(this);
		}

		public override void Save()
		{
			Changed();
			base.Save();
		}
	}
}
