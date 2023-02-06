using System.Collections.Generic;
using UnityEngine;

namespace UnityBase.Extensions
{
	public static class GameObjectExtensions
	{
		public static GameObject FindRecursive(this GameObject underGameObject, string withName)
		{
			return underGameObject.transform.FindRecursive(withName)?.gameObject;
		}

		public static void DestroyAndClear<T>(this IList<T> components) where T : Object
		{
			components.ForEach(Object.Destroy);
			components.Clear();
		}
	}
}
