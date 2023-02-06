using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace UnityBase.Extensions
{
	public static class Vector3Extensions
	{
		public static Vector3 Mean(this IEnumerable<Vector3> vectors)
		{
			var count = 0;
			var sum = vectors.Aggregate(Vector3.zero, (agg, v) =>
			{
				count++;
				return agg + v;
			});
			return sum / count;
		}

		/// <summary>
		///     Inverts a scale vector by dividing 1 by each component
		/// </summary>
		public static Vector3 Invert(this Vector3 vec)
		{
			return new Vector3(1 / vec.x, 1 / vec.y, 1 / vec.z);
		}

		public static float VectorFromPlaneToPoint(Vector3 planePosition, Vector3 planeNormal, Vector3 point)
		{
			return Vector3.Dot(planePosition - point, planeNormal);
		}

		public static float VectorFromPlane(this Vector3 point, Vector3 planePosition, Vector3 planeNormal)
		{
			return VectorFromPlaneToPoint(planeNormal, planeNormal, point);
		}
	}
}
