using UnityEditor;
using UnityEngine;

namespace UnityBase.Inspector.Editor
{
#if UNITY_EDITOR
	[CustomEditor(typeof(MonoBehaviour), true)] [CanEditMultipleObjects]
	public class DefaultInspector : UnityEditor.Editor
	{
		public override void OnInspectorGUI()
		{
			this.DrawDefaultInspectorWithoutScriptField();
		}
	}

	public static class DefaultInspector_EditorExtension
	{
		public static bool DrawDefaultInspectorWithoutScriptField(this UnityEditor.Editor Inspector)
		{
			EditorGUI.BeginChangeCheck();

			Inspector.serializedObject.Update();

			var Iterator = Inspector.serializedObject.GetIterator();

			Iterator.NextVisible(true);

			while (Iterator.NextVisible(false)) EditorGUILayout.PropertyField(Iterator, true);

			Inspector.serializedObject.ApplyModifiedProperties();

			return EditorGUI.EndChangeCheck();
		}
	}
#endif
}
