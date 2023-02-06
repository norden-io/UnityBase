using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.Profiling;

namespace UnityBase.Extensions
{
	public interface IHasGameobject
	{
		GameObject gameObject { get; }
	}

	public static class DebuggingExtensions
	{
		public static string _debug(this object o, string msg)
		{
			return $"[{o.GetType().Name}] {msg}";
		}

		public static string _longDebug(this object o, string msg, string class_suffix)
		{
			return $"[{o.GetType()}{class_suffix}]:\n{msg}";
		}

		public static string _longDebug(this Component o, string msg)
		{
			return o._longDebug(msg, $" in '{o.gameObject.name}'");
		}

		public static string _longDebug(this IHasGameobject o, string msg)
		{
			return o._longDebug(msg, $" in '{o.gameObject.name}'");
		}

		public static void Log(this Component o, string msg, LogType type = LogType.Log)
		{
			Debug.unityLogger.Log(type, null, o._longDebug(msg), o.gameObject);
		}

		public static void Log(this IHasGameobject o, string msg, LogType type = LogType.Log)
		{
			Debug.unityLogger.Log(type, null, o._longDebug(msg), o.gameObject);
		}

		// Debug: profiling
		public static void ProfileBeginSample(this object o, string part = "",
			[CallerMemberName]                      string callerMethod = "",
			[CallerFilePath]                        string callerFile   = "", [CallerLineNumber] int callerLine = 0)
		{
			Profiler.BeginSample($"{o.GetType().Name}.{callerMethod} {part}");
		}
	}
}
