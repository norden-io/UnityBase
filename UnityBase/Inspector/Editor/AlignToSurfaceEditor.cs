#nullable enable
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace UnityBase.Inspector.Editor
{
	[CustomEditor(typeof(AlignToSurface), true)] [CanEditMultipleObjects]
	public class AlignToSurfaceEditor : UnityEditor.Editor
	{
		private bool _selectingWithMouse;


		private AlignToSurface[] _targets          => Array.ConvertAll(targets,  o => (AlignToSurface)o);
		private Transform[]      _targetTransforms => Array.ConvertAll(_targets, o => o.transform);

		private static string[] _axes => AlignToSurface.axes;
		private static Dictionary<string, Vector3> _directions => AlignToSurface.directions;
		private        bool singleTarget => targets.Length == 1;
		private        string _targetName => singleTarget ? $"'{target.name}'" : $"{targets.Length} targets";

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			//EditorGUILayout.LabelField("Align to surface normal", EditorStyles.boldLabel);

			/* Update configuration */
			var align = _axes[EditorGUILayout.Popup("Alignment axis", Array.IndexOf(_axes, _targets[0].align), _axes)];
			var moveObject = EditorGUILayout.Toggle("Move to hit position", _targets[0].moveObject);
			var moveObjectChanged = moveObject != _targets[0].moveObject;
			foreach (var ats in _targets) {
				Undo.RecordObject(ats, $"Align to surface on {_targetName}");
				if (moveObjectChanged) ats.moveObject = moveObject;
			}

			if (singleTarget) _targets[0].align = align;

			/* Raycast along axis */
			GUILayout.Label("Raycast to surface along: ");
			var i_selected = GUILayout.SelectionGrid(-1, _axes, 3);
			if (i_selected >= 0) {
				var selected = _axes[i_selected];
				var dir      = _directions[selected];

				foreach (var ats in _targets) {
					var worldDir = ats.transform.TransformDirection(dir);

					ats.RaycastAndTransform(new Ray(ats.transform.position, worldDir));
				}
			}

			/* Raycast from mouse click */
			if (_selectingWithMouse) GUI.enabled = false;
			if (GUILayout.Button("Select surface with mouse", GUILayout.MinHeight(40))) {
				_selectingWithMouse      =  true;
				SceneView.duringSceneGui += OnSceneMouseSelector;
			}

			GUI.enabled = true;
		}

		private void OnSceneMouseSelector(SceneView scene)
		{
			// https://answers.unity.com/questions/1260602/how-to-get-mouse-click-world-position-in-the-scene.html
			var e = Event.current;

			/* Prevent default event action (e.g. focus change to different GameObject) */
			// https://answers.unity.com/questions/564457/intercepting-left-click-in-scene-view-for-custom-e.html
			HandleUtility.AddDefaultControl(0);

			if (e.type == EventType.MouseDown) {
				//Debug.Log($"Scene event button {e.button} {e} in view {scene}", target);
				if (e.button == 0) /* Left click */
					// https://answers.unity.com/questions/1679412/how-to-raycast-from-mouse-position-in-scene-view.html
					foreach (var ats in _targets)
						ats.RaycastAndTransform(HandleUtility.GUIPointToWorldRay(e.mousePosition));

				/* Stop catching mouse events */
				_selectingWithMouse      =  false;
				SceneView.duringSceneGui -= OnSceneMouseSelector;
				Repaint();
			}
		}
	}
}
