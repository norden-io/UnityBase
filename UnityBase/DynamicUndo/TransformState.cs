using System;
using UnityEngine;

namespace DynamicUndo
{
	public class TransformState
	{
		public Vector3    localPosition;
		public Quaternion localRotation;
		public Vector3    localScale;

		public Transform transform;

		public TransformState()
		{
		}

		public TransformState(Transform from)
		{
			transform = from;
			SaveLocal();
		}

		public Vector3    position   => transform.parent.TransformPoint(localPosition);
		public Quaternion rotation   => transform.parent.rotation * localRotation;
		public Vector3    lossyScale => throw new NotImplementedException();

		public Vector3 GetOffsetPosition(Vector3 direction, float amount)
		{
			if (Mathf.Approximately(amount, 0)) return position;
			return position + rotation * direction * amount;
		}

		public TransformState SaveLocal(Transform from)
		{
			localPosition = from.localPosition;
			localRotation = from.localRotation;
			localScale    = from.localScale;
			return this;
		}

		public TransformState SaveLocal()
		{
			return SaveLocal(transform);
		}

		public void ApplyLocal(Transform to)
		{
			to.localPosition = localPosition;
			to.localRotation = localRotation;
			to.localScale    = localScale;
		}

		public void ApplyLocal()
		{
			ApplyLocal(transform);
		}

		public void ApplyLerpLocal(Transform to, float pct_keep_old)
		{
			to.localPosition = Vector3.LerpUnclamped(localPosition, to.localPosition, pct_keep_old);
			to.localScale    = Vector3.LerpUnclamped(localScale,    to.localScale,    pct_keep_old);
			to.localRotation = Quaternion.LerpUnclamped(localRotation, to.localRotation, pct_keep_old);
		}

		public void ApplyLerpLocal(float pct_keep_old)
		{
			ApplyLerpLocal(transform, pct_keep_old);
		}
	}
}
