#nullable enable
using UnityBase.DynamicUndo.Base;
using UnityEngine;

namespace DynamicUndo
{
	public class UndoTransformManager : UndoManager<TransformUndoElement>
	{
		protected override void Update()
		{
			if (!UpdateProgress()) return;
			transform.localScale    = Vector3.Lerp(source.localScale,    target.localScale,    progress);
			transform.localPosition = Vector3.Lerp(source.localPosition, target.localPosition, progress);
			transform.localRotation = Quaternion.Lerp(source.localRotation, target.localRotation, progress);
		}
	}
}
