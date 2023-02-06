#nullable enable
using System;
using UnityEngine;

namespace UnityBase.DynamicUndo.Base
{
	[Serializable]
	public struct TransformUndoElement : IUndoElement
	{
		public Vector3    localScale;
		public Vector3    localPosition;
		public Quaternion localRotation;


		//public float Time { get; set; }

		public void GetDataFrom(GameObject o)
		{
			localScale    = o.transform.localScale;
			localPosition = o.transform.localPosition;
			localRotation = o.transform.localRotation;
		}
	}
}
