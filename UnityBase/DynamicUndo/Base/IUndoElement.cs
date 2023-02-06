#nullable enable
using UnityEngine;

namespace UnityBase.DynamicUndo.Base
{
	public interface IUndoElement
	{
		//public float Time { get; set; }

		void GetDataFrom(GameObject o);
	}
}
