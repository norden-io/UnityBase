using UnityBase.DynamicUndo.Base;
using UnityEditor;
using UnityEngine;

namespace UnityBase.DynamicUndo
{
	public class MouseUndo : MonoBehaviour
	{
#if UNITY_EDITOR
		private static bool attached;
		private        bool _attached;

		[SerializeReference] private IUndoManager _undoManager;

		public virtual IUndoManager undoManager => _undoManager;

		protected void OnEnable()
		{
			if (!attached) {
				attached                 =  true;
				_attached                =  true;
				SceneView.beforeSceneGui += OnSceneMouseEvent;
			}
			else {
				Debug.LogWarning($"A different {GetType()} is already active!", this);
			}
		}

		protected void OnDisable()
		{
			if (_attached) {
				SceneView.beforeSceneGui -= OnSceneMouseEvent;
				_attached                =  false;
				attached                 =  false;
			}
		}

		private void OnSceneMouseEvent(SceneView scene)
		{
			var e = Event.current;

			//Debug.Log($"{e.type} {e.keyCode} {e.button}");
			if (e.control && e.alt && e.type == EventType.MouseDown)
				switch (e.button) {
					// thumb buttons (>3) don't work :(
					case 1: // forward
						HandleUtility.AddDefaultControl(0);
						_undoManager.Redo();
						break;
					case 0: // back
						HandleUtility.AddDefaultControl(0);
						_undoManager.Undo();
						break;
				}
		}
#endif
	}
}
