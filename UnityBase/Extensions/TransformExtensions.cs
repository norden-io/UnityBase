using System.Linq;
using UnityEngine;

namespace UnityBase.Extensions
{
	public static class TransformExtensions
	{
		public static Transform FindRecursive(this Transform underTransform, string withName)
		{
			var names = withName.Split('/', 2);
			var o = underTransform.GetComponentsInChildren<Transform>()
				.Where(t => t.name == names[0])
				.FirstOrDefault();
			return names.Length < 2 ? o : o.FindRecursive(names[1]);
		}

		public static string GetPath(this Transform current)
		{
			if (current.parent == null)
				return "/" + current.name;
			return current.parent.GetPath() + "/" + current.name;
		}


		// https://answers.unity.com/questions/1238142/version-of-transformtransformpoint-which-is-unaffe.html
		public static Vector3 TransformPointUnscaled(this Transform transform, Vector3 position)
		{
			return transform.position + transform.rotation * position;
		}

		public static Vector3 InverseTransformPointUnscaled(this Transform transform, Vector3 position)
		{
			position -= transform.position;
			return Quaternion.Inverse(transform.rotation) * position;
		}


		// https://forum.unity.com/threads/projection-of-point-on-plane.855958/
		public static Vector3 ProjectPoint(this Transform transform, Vector3 point, Vector3 direction)
		{
			return point + point.VectorFromPlane(transform.position, direction) * direction;
		}

		public static Vector3 ProjectPoint(this Transform transform, Vector3 point)
		{
			return ProjectPoint(transform, point, transform.forward);
		}
	}
}
