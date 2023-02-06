using System;
using System.Collections.Generic;

namespace UnityBase.Extensions
{
	public static class LINQExtensions
	{
		public static void ForEach<T>(this IEnumerable<T> objects, Action<T> action)
		{
			foreach (var obj in objects) action(obj);
		}

		public static void ForEach<T, _>(this IEnumerable<T> objects, Func<T, _> action)
		{
			foreach (var obj in objects) action(obj);
		}
	}
}
