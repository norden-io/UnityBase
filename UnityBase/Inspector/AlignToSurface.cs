using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
#if NET_STANDARD
using System.Windows.Forms;
#endif

namespace UnityBase.Inspector
{
	public class AlignToSurface : MonoBehaviour
	{
		public static readonly Dictionary<string, Vector3> directions = new()
		{
			{ "+x", new Vector3(1,  0,  0) },
			{ "+y", new Vector3(0,  1,  0) },
			{ "+z", new Vector3(0,  0,  1) },
			{ "-x", new Vector3(-1, 0,  0) },
			{ "-y", new Vector3(0,  -1, 0) },
			{ "-z", new Vector3(0,  0,  -1) }
		};

		public static readonly string[] axes;

		[HideInInspector] public bool   moveObject = true;
		[HideInInspector] public string align      = "+y";
		public                   bool   changeParent;
		public                   float  moveOffset           = 0.0001f;
		public                   string printTransformFormat = "";

		static AlignToSurface()
		{
			axes = directions.Keys.ToArray();
		}

		public void RaycastAndTransform(Ray ray)
		{
			Undo.RecordObject(transform, $"Align to surface normal '{transform.name}'");

			var was_active = transform.gameObject.activeInHierarchy;
			transform.gameObject.SetActive(false);

			RaycastHit hit;
			if (Physics.Raycast(ray, out hit)) {
				Debug.Log($"{hit.collider.name} p:{hit.point} normal:{hit.normal}", this);

				var direction = directions[align];
				transform.rotation = Quaternion.FromToRotation(direction, hit.normal);

				if (moveObject) {
					transform.position = hit.point;
					transform.Translate(direction * moveOffset);
				}

				if (changeParent) transform.parent = hit.transform;

				PrintTransform();
			}
			else {
				Debug.Log($"No collider encountered for '{transform.name}'", this);
			}

			transform.gameObject.SetActive(was_active);
		}

		private void PrintTransform()
		{
			if (!string.IsNullOrWhiteSpace(printTransformFormat)) {
				var printStr = string.Format(printTransformFormat,
					transform.localPosition.ToString("F5"),
					transform.localRotation.eulerAngles.ToString("F5"),
					transform.lossyScale.ToString("F5"));
				Debug.Log(printStr, this);
#if NET_STANDARD
				Clipboard.SetText(printStr);
#endif
			}
		}
	}
}
