using UnityEditor;
using UnityEngine;

namespace UnityBase.Inspector
{
	public class SceneViewOnPlay : MonoBehaviour
	{
#if UNITY_EDITOR
		private void Start()
		{
			EditorWindow.FocusWindowIfItsOpen(typeof(SceneView));
		}
#endif
	}
}
