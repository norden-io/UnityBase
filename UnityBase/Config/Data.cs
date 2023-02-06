using System;

namespace UnityBase.Config
{
	[Serializable]
	public abstract class Data : TransientData
	{
		public override string dataPath
		{
			get
			{
				if (string.IsNullOrEmpty(_dataPath)) throw new Exception("_dataPath must not be null or empty!");
				return _dataPath;
			}
			set
			{
				if (string.IsNullOrEmpty(value)) throw new Exception("Cannot set _dataPath to null or empty string!");
				_dataPath = value;
			}
		}
	}
}
